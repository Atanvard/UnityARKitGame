using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils;
using SKStudios.ProtectedLibs.Rendering;
using SKStudios.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace SKStudios.Portals
{


    /// <summary>
    /// Class designed to spawn and control Portal instances.
    /// </summary>
    [ExecuteInEditMode]
    public class PortalController : MonoBehaviour
    {
        private GameObject _previewRoot;
        public GameObject PreviewRoot
        {
            get
            {
                if (!gameObject.activeInHierarchy)
                {
                    DestroyImmediate(_previewRoot, true);
                    return null;
                }
                    

                if (!_previewRoot)
                {
                    _previewRoot = new GameObject();
                    _previewRoot.name = "Preview Root";
                    _previewRoot.hideFlags = HideFlags.HideAndDontSave;
                    
                    _previewRoot.transform.localPosition = transform.position;
                    _previewRoot.transform.localRotation = Quaternion.AngleAxis(180, transform.up) * transform.rotation;
                    _previewRoot.transform.localScale = transform.lossyScale;
                    _previewRoot.transform.SetParent(transform, true);

                    _previewRoot.AddComponent<MeshRenderer>();
                    MeshFilter filter = _previewRoot.AddComponent<MeshFilter>();
                    filter.mesh = Resources.Load<Mesh>("Meshes/PortalPreview");
                }
                _previewRoot.tag = "SKSEditorTemp";
                return _previewRoot;
            }
        }

        private Camera _previewCamera;
        public Camera PreviewCamera {
            get {

                if (!gameObject.activeInHierarchy)
                {
                    DestroyImmediate(_previewCamera, true);
                    return null;
                }

                if (!_previewCamera)
                {
                    GameObject previewCamera = new GameObject();
                    previewCamera.name = "Preview Camera";
                    previewCamera.hideFlags = HideFlags.HideAndDontSave;

                    previewCamera.transform.SetParent(transform);
                    previewCamera.transform.localPosition = Vector3.zero;
                    previewCamera.transform.localScale = Vector3.one;
                   
                    Camera cam = previewCamera.AddComponent<Camera>();
                    
                    cam.cullingMask |= (LayerMask.NameToLayer("Portal"));
                    cam.enabled = false;


                   
                    _previewCamera = previewCamera.GetComponent<Camera>();
                    _previewCamera.useOcclusionCulling = false;

                    SKSRenderLib lib = previewCamera.AddComponent<SKSRenderLib>();

                    Material blitMat = new Material(Shader.Find("Custom/BlitWithInversion"));
                    lib.Initialize(PreviewRoot.transform, TargetController.transform); 
                }
                _previewCamera.tag = "SKSEditorTemp";
                return _previewCamera;
            }
        }

        [SerializeField] private PortalController _targetController;
        public PortalController TargetController {
            get { return _targetController; }
            set {
                _targetController = value;
                if (_targetController && _targetController.TargetController == null)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(_targetController, "Automatic Portal Linking");
#endif
                    _targetController.TargetController = this;
                }

                if (PortalScript)
                    _portalScript.TargetPortal = TargetController.GetComponentInChildren<Portal>(true);

                bool breakpoint = false;
            }
        }


       
        //The scale of this Portal, for resizing
        [HideInInspector] public float PortalScale = 1;
        //The actual unit size of the Portal opening
        [HideInInspector] public Vector3 PortalOpeningSize = Vector3.one;


        /// <summary>
        /// The Portal prefab to use
        /// </summary>
        public GameObject Portal;

        [SerializeField] private Texture2D _mask;
        /// <summary>
        /// The Mask for the spawned portal to use. Masks control alpha for stencil and fade effects
        /// </summary>
        public Texture2D Mask {
            get
            {
                if (SKSGlobalRenderSettings.ShouldOverrideMask)
                    return SKSGlobalRenderSettings.Mask;
                
                return _mask;      
            }
            set
            {
                if(PortalMaterial)
                    PortalMaterial.SetTexture("_AlphaTexture", value);
                if (SKSGlobalRenderSettings.ShouldOverrideMask && SKSGlobalRenderSettings.Mask &&
                    SKSGlobalRenderSettings.Mask == value)
                    return;

                if (PortalScript)
                    PortalScript.Mask = value;

                _mask = value;
            }
        }

        [SerializeField] private Material _portalMaterial;

        /// <summary>
        /// The Material for the Portal to use
        /// </summary>
        public Material PortalMaterial {
            get {
                if (!_portalMaterial)
                {
                    Material source;
                    source = Resources.Load<Material>("Materials/Visible effects/PortalMat");
                    _portalMaterial = source;
                }

                return _portalMaterial;
            }
            set {
                //if (PortalScript 
                   // && 
                    //(!Application.isPlaying || (PortalScript.PortalCamera.Initialized && PortalScript.TargetPortal.PortalCamera.Initialized)))
                    PortalScript.PortalMaterial = PortalMaterial;

                _portalMaterial = value;
               


                if (Application.isPlaying)
                {
                    if (_previewRoot)
                        PreviewRoot.GetComponent<MeshRenderer>().sharedMaterial = PortalMaterial;
                }
                else
                {
                    if (PreviewRoot)
                        PreviewRoot.GetComponent<MeshRenderer>().sharedMaterial = PortalMaterial;
                }

            }
        }


        [HideInInspector] public bool NonObliqueOverride = false;

        [SerializeField] private bool _enterable = true;
        /// <summary>
        /// Is the Portal enterable?
        /// </summary>
        public bool Enterable {
            get { return _enterable; }
            set {
                _enterable = value;
                if (PortalScript)
                    PortalScript.Enterable = _enterable;
            }
        }

        /// <summary>
        /// What is the color of the Portal in the editor?
        /// </summary>
        public Color color = Color.white;
        
        [SerializeField] private bool _is3D;
        /// <summary>
        /// Is the Portal 3d, such as a crystal ball or similar?
        /// </summary>
        public bool Is3D {
            get {
                return _is3D;
            }
            set {
                _is3D = value;

                if (PortalScript)
                    PortalScript.Is3D = _is3D;
            }
        }

        [SerializeField] private float _detectionScale;
        /// <summary>
        /// The scale of the detection zone for this portal
        /// </summary>
        public float DetectionScale
        {
            get { return _detectionScale; }
            set {
                _detectionScale = value;
                if (PortalTrigger) {
                  PortalTrigger.transform.localScale = new Vector3(1, 1, _detectionScale);
                }       
            }
        }


        [SerializeField] private Portal _portalScript;
        public Portal PortalScript {
            get {
                if (!_portalScript)
                    _portalScript = GetComponentInChildren<Portal>(true);
                return _portalScript;
            }
        }

        [SerializeField] private SKEffectCamera _portalCameraScript;
        private SKEffectCamera PortalCameraScript {
            get {
                if (!_portalCameraScript)
                {
                    _portalCameraScript = GetComponentInChildren<SKEffectCamera>(true);
                }
                return _portalCameraScript;
            }
        }


        

        private PortalTrigger _portalTrigger;
        private PortalTrigger PortalTrigger {
            get {
                if (!_portalTrigger)
                    _portalTrigger = GetComponentInChildren<PortalTrigger>(true);
                return _portalTrigger;
            }
        }

        private GameObject _portalRenderer;
        private GameObject PortalRenderer {
            get {
                if (!_portalRenderer)
                    _portalRenderer = PortalScript.transform.parent.gameObject;
                return _portalRenderer;
            }
        }



        private bool _setup = false;
        [NonSerialized] public Material VisLineMat;
        [NonSerialized] public Material OriginalMaterial;


        void Start() {

            CleanupTemp();
            if(TargetController)
                TargetController.CleanupTemp();
            GetComponent<Renderer>().enabled = true;
            if (!Application.isPlaying) return;
            //Todo: Once fully functioning and verified, remove this
            /*
            GlobalPortalSettings settings = GlobalPortalSettings.Instance;
            if (!settings)
            {
                settings = Resources.Load<GlobalPortalSettings>("Global Portal Settings");
                settings.OnEnable();
            }*/
            SKSGlobalRenderSettings.Instance.OnEnable();
            //Load the Portal prefab

            StartCoroutine(Setup());
        }

        IEnumerator Setup()
        {
            //Move all children to portal for accurate scaling
            List<Transform> childTransforms = new List<Transform>();
            foreach(Transform t in transform)
                childTransforms.Add(t);

            Portal = Instantiate(Portal, transform);
           
          
            Portal.transform.localPosition = Vector3.zero;
            Portal.transform.localRotation = Quaternion.Euler(0, 180, 0);
            Portal.transform.localScale = Vector3.one;
            Portal.name = "Portal";

            Destroy(gameObject.GetComponent<MeshRenderer>());

            while (!PortalScript || !TargetController.PortalScript)
            {
                yield return new WaitForEndOfFrame();
            }

            TargetController = TargetController;
            
            Mask = Mask;
            Enterable = Enterable;
            Is3D = Is3D;
           

            foreach(Transform t in childTransforms)
                t.SetParent(PortalScript.Origin);

            yield return new WaitForEndOfFrame();
            Portal.SetActive(true);

            PortalCameraScript.RenderingCameraParent = PortalCameraScript.transform.parent;

            PortalScript.Is3D = Is3D;
            PortalScript.SeamlessRecursionFix = transform.Find("Portal/PortalRenderer/SeamlessRecursionFix");

           
            PortalScript.Mask = Mask;

            PortalScript.NonObliqueOverride = NonObliqueOverride;
            PortalScript.PhysicsPassthrough = GetComponentInChildren<PhysicsPassthrough>();
            DetectionScale = DetectionScale;

            PortalTrigger.portal = PortalScript;




            //Enable scripts
            PortalScript.enabled = true;
            PortalCameraScript.enabled = true;
            PortalTrigger.enabled = true;
            _setup = true;

            //Transfer transform values to modifiable var
            PortalOpeningSize = transform.localScale;
            transform.localScale = Vector3.one;

            PortalMaterial = new Material(PortalMaterial);
            PortalScript.PortalMaterial = PortalMaterial;
            //Portal.transform.parent.gameObject.ApplyHideFlagsRecursive(HideFlags.HideAndDontSave);
        }



        void Update()
        {
            if (!Application.isPlaying) return;

            if (_setup)
            {
                PortalScale = PortalRenderer.transform.lossyScale.x;
                PortalRenderer.transform.localScale = PortalOpeningSize;
                _portalScript.Origin.localScale = Vector3.one * PortalScale;
                _portalScript.TargetPortal.ArrivalTarget.localScale = Vector3.one * PortalScale;
                transform.localScale = Vector3.one;
                //Portal.transform.localScale = PortalOpeningSize;
                Debug.DrawLine(transform.position, TargetController.transform.position, color);
            }
        }

        public void OnDrawGizmos()
        {

#if UNITY_EDITOR
            if (!SKSGlobalRenderSettings.Gizmos)
                return;


            //Change Portal colors
            Renderer renderer;

            if (renderer = gameObject.GetComponent<Renderer>())
            {
                Material[] materials = renderer.sharedMaterials;
                if (!OriginalMaterial)
                    OriginalMaterial = new Material(Resources.Load<Material>("Materials/Visible effects/PortalControllerMat"));
                Material material = OriginalMaterial;//new Material(OriginalMaterial);
                material.SetColor("_Color", color);
                renderer.sharedMaterial = material;
            }


            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;


            if (!TargetController)
            {
                Handles.Label(transform.position, "No Target Set", style);
                return;
            }

            if (!PortalMaterial)
            {
                Handles.Label(transform.position, "No Portal Material Set", style);
                return;
            }

            if ((!Mask && !SKSGlobalRenderSettings.ShouldOverrideMask) || (SKSGlobalRenderSettings.ShouldOverrideMask && !SKSGlobalRenderSettings.Mask))
            {
                Handles.Label(transform.position, "No Mask Set", style);
                return;
            }
            
            if (Application.isPlaying) {
                Gizmos.color = color;
                if (PortalScript) {
                    Gizmos.matrix = PortalScript.transform.localToWorldMatrix;
                    Gizmos.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 0.1f));
                    Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
                }
            }
               
#endif
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected() {

            if (!SKSGlobalRenderSettings.Gizmos)
                return;

            if (!this || !gameObject || !transform || !TargetController)
                return;

            float defaultSize = 1f;
            //Dir vector to second portal
            Vector3 diffVector = (TargetController.transform.position - transform.position).normalized;

            //orientation indicator
            Handles.color = new Color(255, 255, 255, 0.7f);
            Handles.ArrowHandleCap(0, transform.position, transform.rotation * Quaternion.Euler(0, 180, 0), defaultSize * ((((Mathf.Sin(Time.time * 2f) + 1) * 0.5f) / 10f) + 0.5f), EventType.Repaint);

            if (TargetController)
            {

                int iterations = Mathf.RoundToInt(Vector3.Distance(transform.position, TargetController.transform.position) / 1f);
                for (int i = 0; i < iterations; i++)
                {
                    float timeScalar = (((float)i / iterations) + (Time.time / 5f)) % 1f;


                    //Set the color to be between the two portal's colors
                    Color arrowColor = Color.Lerp(color, TargetController.color, timeScalar);
                    arrowColor.a = 0.5f;
                    Handles.color = arrowColor;

                    float arrowSize = defaultSize;

                    //Place arrows and move them accordingly
                    Vector3 arrowPosition = Vector3.Lerp(transform.position - (diffVector * arrowSize),
                        TargetController.transform.position, timeScalar);



                    //Scale arrows down as they approach destination
                    float distanceToTarget = Vector3.Distance(arrowPosition, TargetController.transform.position);
                    float distanceToOrigin;
                    if (distanceToTarget <= 1)
                    {
                        arrowSize *= distanceToTarget;
                    }
                    //Scale arrows up as they leave origin
                    else if ((distanceToOrigin = Vector3.Distance(arrowPosition + (diffVector * arrowSize), transform.position)) <= 1)
                    {
                        arrowSize *= distanceToOrigin;
                    }

                    //Scale arrows up as they spawn from origin

                    Handles.ArrowCap(0, arrowPosition,
                        Quaternion.LookRotation(diffVector, Vector3.up),
                        arrowSize);
                }

               //Draw children's gizmos
                foreach (Transform t in transform) {
                    if (t != transform)
                        RecursiveTryDrawGizmos(t);
                }

                //Draw detection zone preview

                //Gizmos.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube));
                if (!Application.isPlaying) {
                    Gizmos.color = new Color(1, 1, 1, 0.2f);
                    Vector3 detectionCenter = transform.position -
                                              (transform.forward * (DetectionScale / 2f) * transform.lossyScale.z) / 2f;
                    Gizmos.DrawWireMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Cube), detectionCenter,
                        transform.rotation,
                        Vector3.Scale(transform.lossyScale, new Vector3(1, 1, DetectionScale / 2f)));
                    GUIStyle style = new GUIStyle(new GUIStyle() {alignment = TextAnchor.MiddleCenter});
                    style.normal.textColor = Color.white;
                    Handles.Label(detectionCenter, "Portal Detection zone", style);
                }


            }

           
          


        }

        private RenderData renderData = null;

        private void OnWillRenderObject() {
#if UNITY_EDITOR
            if (!(Selection.activeGameObject == gameObject))
            {
                return;
            }

            if(!TargetController || !PortalMaterial ||
                !Mask || !SKSGlobalRenderSettings.Preview ||
                !this|| Application.isPlaying)
            {
                //CleanupTemp();
                return;
            }

            MeshRenderer previewRenderer = PreviewRoot.GetComponent<MeshRenderer>();
            previewRenderer.sharedMaterial = PortalMaterial;
            //previewRenderer.enabled = true;

            SKSRenderLib lib = PreviewCamera.GetComponent<SKSRenderLib>();
            PreviewCamera.transform.localPosition = Vector3.zero;

            Camera sceneCam = SceneView.GetAllSceneCameras()[0];

            Camera cam = PreviewCamera;


            GL.Clear(true, true, Color.black);
            Graphics.SetRenderTarget(null);

            RenderProperties renderProps = new RenderProperties();

            //renderState |= RenderState.FirstRender;
            renderProps |= RenderProperties.Optimize;
            renderProps |= SKSGlobalRenderSettings.Inverted ? RenderProperties.InvertedCached : 0;
            renderProps |= !SKSGlobalRenderSettings.UvFlip ? RenderProperties.UvFlipCached : 0;
            renderProps |= RenderProperties.ObliquePlane;
            renderProps |= RenderProperties.FirstRender;
            renderProps |= RenderProperties.RipCustomSkybox;

            MeshRenderer rend = GetComponent<MeshRenderer>();
            MeshRenderer rend2 = TargetController.GetComponent<MeshRenderer>();
            Mesh mesh = PreviewRoot.GetComponent<MeshFilter>().sharedMesh;
            //TargetController.PreviewRoot.GetComponent<MeshRenderer>().enabled = false;
            //TargetController.GetComponent<MeshRenderer>().enabled = false;
            cam.transform.localPosition = Vector3.zero;
            TargetController.PreviewRoot.transform.localPosition = Vector3.zero;

            cam.transform.rotation = TargetController.PreviewRoot.transform.rotation *
                                     (Quaternion.Inverse(transform.rotation) *
                                      (sceneCam.transform.rotation));

            TargetController.PreviewRoot.transform.localScale = Vector3.one;

            if (renderData == null)
                renderData = new RenderData(renderProps, cam, sceneCam,
                    sceneCam.transform.position, sceneCam.projectionMatrix, "_RightEyeTexture",
                    PortalMaterial, new Vector2(Screen.currentResolution.width,
                        Screen.currentResolution.height), previewRenderer, rend2, mesh, 1, 0, false, false);
            else
            {
                renderData.Position = sceneCam.transform.position;
                renderData.ProjectionMatrix = sceneCam.projectionMatrix;
                renderData.ScreenSize = new Vector2(Screen.currentResolution.width,
                    Screen.currentResolution.height);
                renderData.RenderingCamera = PreviewCamera;
                renderData.SourceRenderer = previewRenderer;
            }

            lib.RenderCamera(renderData);

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            previewRenderer.GetPropertyBlock(block);
            RenderTexture output = (RenderTexture)block.GetTexture("_RightEyeTexture");
            //RenderTexture cachedOutput = RenderTexture.GetTemporary(output.width, output.height, output.depth, output.format);
            //cachedOutput.Create();
            //texturesToDispose.Add(cachedOutput);
            //Graphics.CopyTexture(output, cachedOutput);
            if (output)
                previewRenderer.sharedMaterial.SetTexture("_RightEyeTexture", output);
            if (output)
                previewRenderer.sharedMaterial.SetTexture("_LeftEyeTexture", output);
            if(output)
                block.SetTexture("_LeftEyeTexture", output);
            //PortalController.PortalMaterial.SetVector("_LeftDrawPos", PortalController.PortalMaterial.GetVector("_RightDrawPos"));

            Graphics.SetRenderTarget(null);
            lib.TerminateRender();
#endif
        }

        private void RecursiveTryDrawGizmos(Transform target)
        {
            foreach (Transform t in target) {
                MonoBehaviour[] behaviour = t.GetComponents<MonoBehaviour>();
                if(behaviour.Length > 0)
                    behaviour[0].SendMessage("OnDrawGizmosSelected", SendMessageOptions.DontRequireReceiver);
                RecursiveTryDrawGizmos(t);
            }
        }
#endif
        public void CleanupTemp()
        {
            if(this)
                CleanupTempRecursive(transform);
        }

        private void CleanupTempRecursive(Transform targetTransform)
        {
            
            foreach (Transform t in targetTransform)
            {
                CleanupTempRecursive(t);

                if (t.gameObject.CompareTag("SKSEditorTemp"))
                    DestroyImmediate(t.gameObject, true);
            }
        }

    }
}
