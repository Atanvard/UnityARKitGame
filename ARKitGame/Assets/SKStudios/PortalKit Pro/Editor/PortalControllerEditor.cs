using System;
using System.ComponentModel.Design.Serialization;
using SKStudios.Common.Editor;
using SKStudios.Common.Utils;
using UnityEditor;
using UnityEngine;
using SKStudios.Rendering;

namespace SKStudios.Portals.Editor
{
    [CustomEditor(typeof(PortalController))]
    [InitializeOnLoad]
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class PortalControllerEditor : MeshFilterPreview
    {
        private PreviewRenderUtility _previewRenderUtility;
        private LineRenderer renderer;
        private GameObject VisLineObj;

        private static Camera _sceneCameraDupe;
        UnityEditor.Editor _matEditor;

        public override MeshFilter TargetMeshFilter {
            get {
                //if(SKSGlobalRenderSettings.Preview)
               //     return PortalController.PreviewRenderer.GetComponent<MeshFilter>();
               // else
                    return EditorPreviewFilter;
                
            }
            set {}
        }
        public override MeshRenderer TargetMeshRenderer {
            get {
                //if (SKSGlobalRenderSettings.Preview)
                //    return PortalController.PreviewRenderer;
                //else
                    return EditorPreviewRenderer;
            }
            set { }
        }

        private MeshRenderer _editorPreviewRenderer;
        private MeshRenderer EditorPreviewRenderer {
            get {
                if (!_editorPreviewRenderer) {
                    GameObject editorPreviewObj = EditorUtility.CreateGameObjectWithHideFlags("Preview Object", HideFlags.None);
                    editorPreviewObj.tag = "SKSEditorTemp";
                    //editorPreviewObj.hideFlags = HideFlags.HideAndDontSave;
                    editorPreviewObj.transform.SetParent(PortalController.transform);
                    editorPreviewObj.transform.localPosition = Vector3.zero;
                    
                    _editorPreviewRenderer = editorPreviewObj.AddComponent<MeshRenderer>();
                   
                    _editorPreviewRenderer.sharedMaterials = new Material[4];
                    Material m = new Material(Shader.Find("Standard"));
                    Material[] MaterialArray = new Material[4];
                    MaterialArray[0] = Resources.Load<Material>("UI/Materials/Floor");
                    MaterialArray[1] = Resources.Load<Material>("UI/Materials/Background");
                    MaterialArray[2] = PortalController.PortalMaterial;
                    MaterialArray[3] = Resources.Load<Material>("UI/Materials/Background");
                    _editorPreviewRenderer.sharedMaterials = MaterialArray;
                }
                return _editorPreviewRenderer;
            }
        }

        private MeshFilter _editorPreviewFilter;

        private MeshFilter EditorPreviewFilter{
            get {
                if (!_editorPreviewFilter) {
                    _editorPreviewFilter = EditorPreviewRenderer.gameObject.AddComponent<MeshFilter>();
                    _editorPreviewFilter.mesh = Resources.Load<Mesh>("UI/PortalPreview");
                }
                return _editorPreviewFilter;
            }
        }

        private String sourceName;

        public Material PortalMaterial {
            set {
                if (!PortalController)
                    return;

                //if (Application.isPlaying)
                //   return;

                PortalController.PortalMaterial = value;
                DestroyImmediate(_matEditor, true);
            }
        }



        private Texture2D _fakeBackgroundTex;

        private Texture2D FakeBackgroundTex {
            get {
                if (!_fakeBackgroundTex)
                {
                    _fakeBackgroundTex = Resources.Load<Texture2D>("UI/PortalFakeBackground");
                }
                return _fakeBackgroundTex;
            }
        }

        private void OnEnable()
        {

            if (Application.isPlaying) return;

            if (!PortalController.gameObject.activeInHierarchy)
                return;

            if (!PortalController.isActiveAndEnabled)
                return;
            if (!PortalController.TargetController)
                return;

          

            if (SKSGlobalRenderSettings.Preview)
            {

                //PortalController.GetComponent<Renderer>().sharedMaterial.color = Color.clear;
                Camera pokecam = PortalController.PreviewCamera;
                GameObject pokeObj = PortalController.PreviewRoot;
                Camera pokecam2 = PortalController.TargetController.PreviewCamera;
                GameObject pokeObj2 = PortalController.TargetController.PreviewRoot;
                pokecam2.enabled = false;
                pokeObj2.SetActive(false);
            }



            //EditorApplication.update -= UpdatePreview;
            //EditorApplication.update += UpdatePreview;

#if SKS_VR
            //GlobalPortalSettings.SinglePassStereo = settings.SinglePassStereoCached;
#endif

        }


        private void OnDisable()
        {
            //EditorApplication.update -= UpdatePreview;

            CleanupTemp();
            if (PortalController && PortalController.TargetController)
                PortalController.TargetController.CleanupTemp();
            //DestroyImmediate(_matEditor, true);

            if (Application.isPlaying)
                return;

            if (PortalController)
                PortalController.GetComponent<Renderer>().enabled = true;

            if (PortalController)
                PortalController.GetComponent<Renderer>().sharedMaterial.color = PortalController.color;
        }

        //Preview texture for portals
        private RenderTexture _previewTex;

        private RenderTexture PreviewTex {
            get {
                if (_previewTex)
                    RenderTexture.ReleaseTemporary(_previewTex);

                _previewTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24,
                    RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Default);

                return _previewTex;
            }
        }

        private PortalController _portalController;

        private PortalController PortalController {
            get {
                if (!_portalController)
                    _portalController = (PortalController)target;
                return _portalController;
            }
        }

        public override bool HasPreviewGUI() {
            return base.HasPreviewGUI_s();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            TargetMeshRenderer.gameObject.SetActive(true);
            TargetMeshRenderer.gameObject.transform.localPosition = Vector3.zero;
            
            try
            {

                Texture fakePortalTex = FakeBackgroundTex;
                PortalController.PortalMaterial.SetTexture("_LeftEyeTexture", fakePortalTex);
                PortalController.PortalMaterial.SetVector("_LeftDrawPos", new Vector4(0, 0, 1, 1));
               
            }
            catch
            {
                //Unity silliness again
            }
            base.OnPreviewGUI_s(r, background);
            TargetMeshRenderer.gameObject.SetActive(false);
        }

        public override void OnInspectorGUI() {
            TargetMeshRenderer.gameObject.SetActive(false);
            try
            {

                foreach (PortalController p in targets)
                    Undo.RecordObject(p, "Portal Controller Editor Changes");

                EditorGUI.BeginChangeCheck();
                PortalController.TargetController = (PortalController)EditorGUILayout.ObjectField(
                    new GUIContent("Target Controller", "The targetTransform of this Portal."),
                    PortalController.TargetController, typeof(PortalController), true, null);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.TargetController = PortalController.TargetController;

                //if (!PortalController.PortalScript.PortalCamera ||
                //    !PortalController.TargetController.PortalScript.PortalCamera) return;

                EditorGUI.BeginChangeCheck();
                PortalController.Portal =
                    (GameObject)EditorGUILayout.ObjectField(
                        new GUIContent("Portal Prefab", "The Prefab to use for when the Portal is spawned"),
                        PortalController.Portal, typeof(GameObject), false, null);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Portal = PortalController.Portal;


                if (SKSGlobalRenderSettings.ShouldOverrideMask)
                    EditorGUILayout.HelpBox("Your Global Portal Settings are currently overriding the mask",
                        MessageType.Warning);

                EditorGUI.BeginChangeCheck();
                PortalController.Mask = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Portal Mask", "The transparency mask to use on the Portal"), PortalController.Mask, typeof(Texture2D), false, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Mask = PortalController.Mask;

                EditorGUI.BeginChangeCheck();
                Material material =
                    (Material)EditorGUILayout.ObjectField(
                        new GUIContent("Portal Material", "The material to use for the Portal"),
                        PortalController.PortalMaterial, typeof(Material), false, null);
                if (EditorGUI.EndChangeCheck())
                {
                    PortalMaterial = material;
                    foreach (PortalController p in targets)
                        p.PortalMaterial = PortalController.PortalMaterial;
                }


                EditorGUI.BeginChangeCheck();
                PortalController.Enterable =
                    EditorGUILayout.Toggle(
                        new GUIContent("Enterable", "Is the Portal Enterable by Teleportable Objects?"),
                        PortalController.Enterable);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Enterable = PortalController.Enterable;

                EditorGUI.BeginChangeCheck();
                PortalController.Is3D =
                    EditorGUILayout.Toggle(
                        new GUIContent("Portal is 3D Object", "Is the Portal a 3d object, such as a Crystal ball?"),
                        PortalController.Is3D);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.Is3D = PortalController.Is3D;

                EditorGUI.BeginChangeCheck();
                PortalController.DetectionScale = EditorGUILayout.Slider(
                    new GUIContent("Detection zone Scale", "The scale of the portal detection zone."),
                    PortalController.DetectionScale, 1f, 10f);
                if (EditorGUI.EndChangeCheck())
                    foreach (PortalController p in targets)
                        p.DetectionScale = PortalController.DetectionScale;
                //Show the Portal Material Inspector
                if (Application.isPlaying)
                    return;

                EditorGUILayout.LabelField("Portal Material Preview:");

                if(PortalController.PortalMaterial)
                    if (_matEditor == null)
                        _matEditor = UnityEditor.Editor.CreateEditor(PortalController.PortalMaterial);

                _matEditor.DrawHeader();
                _matEditor.OnInspectorGUI();
            }
            catch
            {
                //Just for cleanliness
            }
            finally
            {
                if (!SKSGlobalRenderSettings.Preview)
                {
                    //CleanupTemp();
                }
            }

            //Cache state of random
            UnityEngine.Random.State seed = UnityEngine.Random.state;
            //Make color deterministic based on ID
            UnityEngine.Random.InitState(PortalController.GetInstanceID());
            PortalController.color = UnityEngine.Random.ColorHSV(0, 1, 0.48f, 0.48f, 0.81f, 0.81f);
            //Reset the random
            UnityEngine.Random.state = seed;


        }

        private void CleanupTemp()
        {

            if (PortalController)
            {
                MeshRenderer renderer = PortalController.GetComponent<MeshRenderer>();
                if (renderer)
                    renderer.enabled = true;
            }

            PortalController.CleanupTemp();
        }
    }
}