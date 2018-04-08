using System;
using System.Collections;
using UnityEngine;
using Eppy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VR;
using SKStudios.Common.Extensions;
using SKStudios.Rendering;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace SKStudios.Portals
{
    /// <summary>
    /// Class handling the majority of Portal logic. All public fields are automatically handled by a PortalController if you wish to control portals via the Editor.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        //Keeps the Portal effect 100% seamless even at higher head speeds
        private const float FudgeFactor = 0.001f;


        private bool _enterable;

        /// <summary>
        /// Is the portal enterable?
        /// </summary>
        public bool Enterable {
            get { return _enterable; }
            set {
                _enterable = value;
                if (!value)
                {
                    while (NearTeleportables.Count > 0)
                    {
                        RemoveTeleportable(NearTeleportables.First());
                    }
                    NearTeleportables.Clear();
                    transform.parent.Find("PortalTrigger").GetComponent<Collider>().enabled = false;
                }
                else
                {
                    transform.parent.Find("PortalTrigger").GetComponent<Collider>().enabled = true;
                }
            }
        }

        private SKEffectCamera _portalCamera;
        //The PortalCamera that this portal uses.
        public SKEffectCamera PortalCamera {
            get {
                if (!_portalCamera)
                {
                    _portalCamera = transform.parent.parent.GetComponentInChildren<SKEffectCamera>(true);
                    _portalCamera.Initialize(TargetPortal.PortalCamera, PortalMaterial, MeshRenderer, TargetPortal.MeshRenderer, MeshFilter.sharedMesh, Origin, ArrivalTarget);
                }
                return _portalCamera;
            }
            set { _portalCamera = value; }
        }

        private Texture2D _mask;
        /// <summary>
        /// Mask to control the alpha of the portal
        /// </summary>
        public Texture2D Mask {
            get { return _mask; }
            set {
                _mask = value;
                try
                {
                    MeshRenderer.sharedMaterial.SetTexture("_AlphaTexture", _mask);
                }
                catch (NullReferenceException e)
                {
                    Debug.Log(e.Message);
                }

            }
        }

        private Collider _portalCollider;

        private Collider PortalCollider {
            get {
                if (!_portalCollider)
                    _portalCollider = GetComponent<Collider>();
                return _portalCollider;
            }
        }


        private Transform _origin;
        /// <summary>
        /// Origin of the Portal, facing inward
        /// </summary>
        public Transform Origin {
            get {
                if (!_origin)
                    _origin = PortalRoot.Find("PortalSource");
                return _origin;
            }
        }

        private Transform _arrivalTarget;

        /// <summary>
        /// The transform attached to the target Portal, for ref purposes. facing outward.
        /// </summary>
        public Transform ArrivalTarget {
            get {
                if (!_arrivalTarget)
                    _arrivalTarget = TargetPortal.PortalRoot.Find("PortalTarget");
                return _arrivalTarget;
            }
        }

        public Transform PortalRoot {
            get { return transform.parent.parent; }
        }

        /// <summary>
        /// The transform of an object used to fix recursion issues
        /// </summary>
        public Transform SeamlessRecursionFix { get; set; }
        /// <summary>
        ///  The collider of the Player's head
        /// </summary>
        public Collider HeadCollider { get; set; }
        /// <summary>
        /// The target Portal
        /// </summary>
        public Portal TargetPortal;
        /// <summary>
        /// Should cameras use Oblique culling? (Default True)
        /// </summary>
        public bool NonObliqueOverride { get; set; }
        /// <summary>
        /// Is the Portal 3d, such as a crystal ball?
        /// </summary>
        public bool Is3D { get; set; }
        /// <summary>
        /// Should Physics Passthrough be used?
        /// </summary>
        public PhysicsPassthrough PhysicsPassthrough { get; set; }

        private GameObject _placeholder;
        /// <summary>
        /// Placeholder for when recursion bottoms out
        /// </summary>
        public GameObject Placeholder {
            get {

                Transform t = PortalRoot.transform.Find("PortalRenderer/PortalPlaceholder");
                if (t)
                    _placeholder = t.gameObject;
                return _placeholder;
            }
        }

        private MaterialPropertyBlock _seamlessRecursionBlock;
        private Renderer _seamlessRecursionRenderer;
        private Renderer SeamlessRecursionRenderer {
            get {
                if (!_seamlessRecursionRenderer)
                    _seamlessRecursionRenderer = SeamlessRecursionFix.GetComponent<Renderer>();
                return _seamlessRecursionRenderer;
            }
        }
        /// <summary>
        /// Is the portal a mirror? (Not Implemented)
        /// </summary>
        public bool Mirror {
            get { return false; }
        }

        private MeshRenderer _meshRenderer;
        /// <summary>
        /// The Mesh Renderer used by the Portal. Used for determining portal size on screen.
        /// </summary>
        public MeshRenderer MeshRenderer {
            get {
                if (!_meshRenderer)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                }
                return _meshRenderer;
            }
            set { _meshRenderer = value; }
        }

        private MeshFilter _meshFilter;
        /// <summary>
        /// The Mesh Filter used by the Portal. Also used for determining portal size on screen.
        /// </summary>
        public MeshFilter MeshFilter {
            get {
                if (!_meshFilter)
                    _meshFilter = gameObject.GetComponent<MeshFilter>();
                return _meshFilter;
            }
            set { _meshFilter = value; }
        }

        private Material _portalMaterial;
        /// <summary>
        /// The Material for the Portal to use
        /// </summary>
        public Material PortalMaterial {
            get {
                if (!_portalMaterial)
                    _portalMaterial = new Material(Resources.Load<Material>("Materials/Visible effects/PortalMat"));
                return _portalMaterial;
            }
            set {
                _portalMaterial = value;
                MeshRenderer.material = _portalMaterial;
                if (SeamlessRecursionFix)
                    SeamlessRecursionFix.GetComponent<Renderer>().sharedMaterial = _portalMaterial;

                PortalCamera.UpdateMaterial(_portalMaterial);
            }
        }
        private Camera _headCamera;

        //Physical collision fixes
        public GameObject BufferWallObj;
        public List<Collider> BufferWall;

        private bool _headInPortalTrigger = false;

        private bool _updatedDopplegangersThisFrame = false;
        //The portals are not "actually" seamless, as no such thing is possible. However, they APPEAR to be, as once the camera gets within a 
        //certain distance the back wall begins stretching. This variable activates that effect.
        //The value is equal to the level at which the cheese has been activated.
        private int _cheeseActivated = 0;
        public int RenderCount = 0;
        //Portal effects
        public List<Teleportable> NearTeleportables;

        //Objects near enough to trigger passthrough (controlled by their respective scripts)
        public Dictionary<Collider, Collider> PassthroughColliders;
        //Camera tracking information
        private Vector3[] _nearClipVertsLocal;
        private Vector3[] _nearClipVertsGlobal;
        //Ignored colliders behind Portal
        public Collider[] RearColliders;

        private void Awake()
        {
            this.enabled = false;
        }

        private void Update()
        {
            _updatedDopplegangersThisFrame = false;
            MeshRenderer.sharedMaterial.SetFloat("_ZTest", (int)CompareFunction.Less);
        }

        /// <summary>
        /// Setup for the Portal
        /// </summary>
        private void OnEnable()
        {

            NearTeleportables = new List<Teleportable>();
            PassthroughColliders = new Dictionary<Collider, Collider>();
            SetupCamera();

            //Add the buffer colliders to the collection
            BufferWall = new List<Collider>();
            foreach (Transform tran in BufferWallObj.transform)
            {
                Collider c = tran.GetComponent<Collider>();
                BufferWall.Add(c);
                c.enabled = false;
            }

            PhysicsPassthrough.Initialize(this);

            //Set up things to keep the seamless recursion fix updated
            SeamlessRecursionRenderer.sharedMaterial = PortalMaterial;
            _seamlessRecursionBlock = new MaterialPropertyBlock();
            StartCoroutine(KeepSeamlessFixUpdated());
        }

        private void SetupCamera()
        {
            _headCamera = Camera.main;
            HeadCollider = _headCamera.GetComponent<Collider>();
            _nearClipVertsLocal = EyeNearPlaneDimensions(_headCamera);
            _nearClipVertsGlobal = new Vector3[_nearClipVertsLocal.Length];
        }

        /// <summary>
        /// All Portal updates are done after everything else has updated
        /// </summary>
        private void LateUpdate()
        {
            //Prevents collision from reporting false before we check the next frame
            UpdateBackPosition();

            //Setup camera again if the main camera has changed
            if (_headCamera != Camera.main)
            {
                SetupCamera();
            }

            //Update the effects, disabling masks/ztesting for seamless transitions
            //todo: delete, has been moved to post-render step
            //UpdateEffects();



            //Resets the rendered state before the next frame
            SKEffectCamera.CurrentDepth = 0;
            RenderCount = 0;

            //UpdateDopplegangers();
            //TargetPortal.UpdateDopplegangers();
        }

        /// <summary>
        /// Updates the "dopplegangers", the visual counterparts, to all teleportable objects near portals.
        /// </summary>
        public void UpdateDopplegangers(bool force = false) {
  
            if (!ArrivalTarget || (!force && _updatedDopplegangersThisFrame))
                return;
            _updatedDopplegangersThisFrame = true;
            //Updates dopplegangers
            foreach (var tp in NearTeleportables)
            {

                //PortalUtils.TeleportObject(teleportable.root.gameObject, Origin, ArrivalTarget, teleportable.root, null, true, 
                //!SKSGlobalRenderSettings.NonScaledRenderers)
                tp.UpdateDoppleganger();
                /*
                tp.Doppleganger.transform.SetParent(tp.root.parent);
                tp.Doppleganger.transform.localRotation = tp.root.localRotation;
                tp.Doppleganger.transform.localPosition = tp.root.localPosition;*/
                //tp.Doppleganger.transform.SetParent(null, true);
                if (tp.Doppleganger) {
                    PortalUtils.TeleportObject(tp.Doppleganger, Origin, ArrivalTarget, tp.Doppleganger.transform, null, false);
                }
                else {
                    RemoveTeleportable(tp);
                    break;
                }
                //PortalUtils.TeleportObject(tp.Doppleganger, Origin, ArrivalTarget, tp.Doppleganger.transform);
                //StartCoroutine(tp.UpdateDoppleganger());

            }


        }

        /// <summary>
        /// Updates the back position of the Portal wall for visual seamlessness
        /// </summary>
        private void UpdateBackPosition()
        {
            _cheeseActivated = -1;
            //Gets the near clipping plane verts in global space
            if (_nearClipVertsGlobal == null) return;
            for (int i = 0; i < _nearClipVertsGlobal.Length; i++)
            {
                // nearClipVertsGlobal[i] = _headCamera.transform.TransformPoint(nearClipVertsLocal[i]);
                _nearClipVertsGlobal[i] = _headCamera.transform.position + (_headCamera.transform.rotation * _nearClipVertsLocal[i]);
            }


            if (!Is3D)
                transform.localScale = new Vector3(1, 1, FudgeFactor);

            if (SeamlessRecursionFix)
            {
                if (SeamlessRecursionFix.gameObject.activeSelf)
                {
                    SeamlessRecursionFix.gameObject.SetActive(false);
                }
            }

            if (!_headInPortalTrigger) return;
            //Moves the drawn "plane" back if the camera gets too close
            float deepestVert = 0;
            Vector3 deepestVertVector = Vector3.zero;
            Plane portalPlane = new Plane(-Origin.forward, Origin.position);
            foreach (Vector3 currentVert in _nearClipVertsGlobal)
            {

                float currentDepth = portalPlane.GetDistanceToPoint(currentVert);
                if (currentDepth < 0)
                {
                    Debug.DrawLine(_headCamera.transform.position, currentVert, Color.red);
                    continue;
                }
                Debug.DrawLine(_headCamera.transform.position, currentVert, Color.green);
                _cheeseActivated = SKEffectCamera.CurrentDepth;
                if (currentDepth > deepestVert)
                {
                    deepestVert = currentDepth;
                    deepestVertVector = currentVert;
                }

            }

            //Scale the portal for seamless passthrough
            if (_cheeseActivated != -1 && !Is3D)
            {

                SeamlessRecursionFix.gameObject.SetActive(true);
                //Reset scale so that InverseTransformPoint doesn't return a scaled value
                transform.localScale = Vector3.one;
                //Get the local-space distance
                float dotDist = -PortalUtils.PlanePointDistance(Vector3.forward, Vector3.zero,
                    transform.InverseTransformPoint(deepestVertVector));
                //Clamp to Fudge
                float dist = Mathf.Max(FudgeFactor, dotDist);
                //Scale, so that the back wall updates properly
                transform.localScale = new Vector3(1, 1, dist);
            }
        }

        /// <summary>
        /// Calls the rendering of a Portal frame
        /// </summary>
        private void OnWillRenderObject()
        {
            if (!TargetPortal) return;
            //Is the mesh renderer in the camera frustrum?
            if (MeshRenderer.isVisible)
            {

                UpdateDopplegangers();
                TargetPortal.UpdateDopplegangers();

#if UNITY_EDITOR
                if (Camera.current.name == "SceneCamera" || Camera.current.name == "Preview Camera")
                    return;
#endif
                TryRenderPortal(Camera.current, _nearClipVertsGlobal);
                if (Camera.current == Camera.main)
                {
                    SKSGlobalRenderSettings.Inverted = SKSGlobalRenderSettings.Inverted;
                    SKSGlobalRenderSettings.UvFlip = SKSGlobalRenderSettings.UvFlip;
                }
                MeshRenderer.GetPropertyBlock(_seamlessRecursionBlock);
                SeamlessRecursionRenderer.SetPropertyBlock(_seamlessRecursionBlock);
                //}
            }
            UpdateEffects();
        }

        WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private IEnumerator KeepSeamlessFixUpdated()
        {
            while (true)
            {
                yield return _waitForEndOfFrame;

            }
        }


        /// <summary>
        /// Sets the z rendering order to make through-wall rendering seamless, as well as disabling masks while traversing portals
        /// </summary>
        private void UpdateEffects()
        {
            //MeshRenderer.GetPropertyBlock(_seamlessRecursionBlock);
            if (_cheeseActivated == SKEffectCamera.CurrentDepth)
            {
                //_seamlessRecursionBlock.SetFloat("_ZTest", (int)CompareFunction.Always);
                //MeshRenderer.SetPropertyBlock(_seamlessRecursionBlock);
                _meshRenderer.sharedMaterial.SetFloat("_ZTest", (int)CompareFunction.Always);
                //_meshRenderer.material.SetFloat("_Mask", 0);
            }
            else
            {
                //_meshRenderer.material.SetFloat("_ZTest", (int)CompareFunction.Less);
                //_meshRenderer.material.SetFloat("_Mask", 1);
            }
        }

        /// <summary>
        /// Render a Portal frame, assuming that the camera is in front of the Portal and all conditions are met.
        /// </summary>
        /// <param name="camera">The camera rendering the Portal</param>
        /// <param name="nearClipVerts">The vertices of the camera's near clip plane</param>
        private void TryRenderPortal(Camera camera, Vector3[] nearClipVerts)
        {
            if (!TargetPortal)
                return;
            //bool isVisible = false;
            bool isVisible = false;
            //Check if the camera itself is behind the Portal, even if the frustum isn't.

            if (!PortalUtils.IsBehind(camera.gameObject.transform.position, Origin.position, Origin.forward))
            {
                isVisible = true;
            }
            else
            {
                //Checks to see if any part of the camera is in front of the Portal
                foreach (Vector3 v in nearClipVerts)
                {
                    if (!PortalUtils.IsBehind(v, Origin.position, Origin.forward))
                    {
                        isVisible = true;
                        break;
                    }
                }
            }

            if ((isVisible || _cheeseActivated != -1))
            {
                PortalCamera.RenderIntoMaterial(camera, PortalMaterial, MeshRenderer, TargetPortal.MeshRenderer, MeshFilter.mesh, !NonObliqueOverride ? _cheeseActivated == -1 : false, Is3D);
            }
        }

        /// <summary>
        /// External class keeps track of the physics duplicates for clean, consistent physical passthrough
        /// </summary>
        public void FixedUpdate()
        {
            //if (GlobalPortalSettings.PhysicsPassthrough)
            _headInPortalTrigger = false;
            PhysicsPassthrough.UpdatePhysics();
        }

        /// <summary>
        /// All collsision methods are externed to another script
        /// </summary>
        public void E_OnTriggerEnter(Collider col)
        {
            Physics.IgnoreCollision(col, PortalCollider);
            Rigidbody teleportableBody = col.attachedRigidbody;
            if (!teleportableBody)
                return;
            Teleportable teleportScript = teleportableBody.GetComponent<Teleportable>();

            AddTeleportable(teleportScript);
        }


        /// <summary>
        /// Checks if objects are in Portal, and teleports them if they are. Also handles player entry.
        /// </summary>
        /// <param name="col"></param>
        public void E_OnTriggerStay(Collider col)
        {
            if (!TargetPortal) return;

            Rigidbody teleportableBody = col.attachedRigidbody;
            if (!teleportableBody)
                return;
            //todo: cache these
            Teleportable teleportScript = teleportableBody.GetComponent<Teleportable>();
            AddTeleportable(teleportScript);
            if (!Enterable || !teleportScript || !teleportScript.initialized) return;

            if (teleportScript == GlobalPortalSettings.PlayerTeleportable)
            {
                _headInPortalTrigger = true;
                //teleportScript = PlayerTeleportable;
            }


            //Updates clip planes for disappearing effect
            if (!NonObliqueOverride)
            {
                teleportScript.SetClipPlane(Origin.position, Origin.forward /* (1 + transform.localScale.z)*/ + (Origin.forward * 0.01f), teleportScript.Renderers.Keys);
                teleportScript.SetClipPlane(ArrivalTarget.position, -ArrivalTarget.forward, teleportScript.Renderers.Values);
            }

            WakeBufferWall();

            //Makes objects collide with invisible buffer bounds
            foreach (Collider c in BufferWall)
            {
                teleportScript.AddCollision(this, c);
            }

            //Makes objects collide with invisible buffer bounds
            foreach (Collider c in TargetPortal.BufferWall)
            {
                teleportScript.IgnoreCollision(this, c, true);
            }

            if (SKSGlobalRenderSettings.PhysicsPassthrough)
            {
                //Makes objects collide with objects on the other side of the Portal
                foreach (Collider c in PassthroughColliders.Values)
                {
                    teleportScript.AddCollision(this, c);
                }
            }


            //Passes Portal info to teleport script
            teleportScript.SetPortalInfo(this);

            //Enables Doppleganger
            teleportScript.EnableDoppleganger();

            //Checks if object should be teleported
            TryTeleportTeleporable(teleportScript, col);

        }

        /// <summary>
        /// Teleports the given teleportable, making passthrough and image effects seamless.
        /// </summary>
        /// <param name="teleportable">Teleportable to teleport</param>
        /// <param name="col">Associated Collider</param>
        private void TryTeleportTeleporable(Teleportable teleportable, Collider col)
        {

            if (!PortalUtils.IsBehind(teleportable.TeleportableBounds.center, Origin.position, Origin.forward) || teleportable.VisOnly) return;
            //if (!PortalUtils.IsBehind(col.transform.position, Origin.position, Origin.forward) || teleportable.VisOnly) return;
            if (teleportable.TeleportedLastFrame) return;

            teleportable.TeleportedLastFrame = true;
            RemoveTeleportable(teleportable);
            //Makes objects not with invisible buffer bounds in the case of portals being too close
            foreach (Collider c in BufferWall)
            {
                teleportable.IgnoreCollision(this, c, true);
            }

            PortalUtils.TeleportObject(teleportable.root.gameObject, Origin, ArrivalTarget, teleportable.root, null, true, 
                !SKSGlobalRenderSettings.NonScaledRenderers);
            TargetPortal.FixedUpdate();
            teleportable.Teleport();
            //TargetPortal.UpdateDopplegangers();
            TargetPortal.PhysicsPassthrough.UpdatePhysics();
            //teleportable.SetClipPlane(Vector3.zero, Vector3.zero, teleportable.Renderers.Keys);

            TargetPortal.PhysicsPassthrough.ForceRescanOnColliders(teleportable.Colliders.Values);

            PhysicsPassthrough.ForceRescanOnColliders(teleportable.Colliders.Values);
            TargetPortal.E_OnTriggerStay(col);

            //TargetPortal.UpdateDopplegangers(true);

            if (teleportable == GlobalPortalSettings.PlayerTeleportable)
            {
                TargetPortal.UpdateDopplegangers(true);
                TargetPortal.IncomingCamera();
                _cheeseActivated = -1;
                _headInPortalTrigger = false;
                //Resets the vis depth of the Portal volume
                if (!Is3D)
                    transform.localScale = new Vector3(1f, 1f, FudgeFactor);


            }

            //teleportable.EnableDoppleganger();
            //teleportable.SetClipPlane();
            //Applies relative velocity
            if (!SKSGlobalRenderSettings.PhysStyleB) return;

            Rigidbody body;
            if (body = col.attachedRigidbody)
            {
                bool colliderEnabled = PortalCollider.enabled;
                bool otherColliderEnabled = TargetPortal.PortalCollider.enabled;

                PortalCollider.enabled = true;
                TargetPortal.PortalCollider.enabled = true;

                Rigidbody portalBody = PortalCollider.attachedRigidbody;
                Rigidbody targetBody = TargetPortal.PortalCollider.attachedRigidbody;

                PortalCollider.enabled = colliderEnabled;
                TargetPortal.PortalCollider.enabled = otherColliderEnabled;

                if ((portalBody) != null
                    && (targetBody) != null)
                {
                    Vector3 relativeVelocity = Quaternion.Inverse(portalBody.rotation) * portalBody.velocity;
                    relativeVelocity += Quaternion.Inverse(targetBody.rotation) * targetBody.velocity;
                    relativeVelocity = targetBody.rotation * relativeVelocity;
                    body.AddForce(relativeVelocity, ForceMode.Impulse);
                }
            }
            TargetPortal.UpdateDopplegangers(true);
            UpdateDopplegangers(true);
        }
        /// <summary>
        /// Removes primed objects from the queue if they move away from the Portal
        /// </summary>
        public void E_OnTriggerExit(Collider col)
        {
            Rigidbody teleportableBody = col.attachedRigidbody;
            if (!teleportableBody)
                return;
            Teleportable teleportScript = teleportableBody.GetComponent<Teleportable>();
            if (col == HeadCollider)
            {
                _headInPortalTrigger = false;
            }

            RemoveTeleportable(teleportScript);
        }

        /// <summary>
        /// Attempt to add a teleportableScript to the nearteleportables group
        /// </summary>
        /// <param name="teleportScript">the script to add</param>
        private void AddTeleportable(Teleportable teleportScript)
        {
            if (!TargetPortal) return;
            if (!teleportScript || NearTeleportables.Contains(teleportScript)) return;
            if (teleportScript.AddedLastFrame) return;

            SetBufferWallActive(true);

            teleportScript.AddedLastFrame = true;
            teleportScript.EnableDoppleganger();

            NearTeleportables.Add(teleportScript);

            //Ignores collision with rear objects
            Vector3[] checkedVerts = PortalUtils.ReferenceVerts(MeshFilter.mesh);

            //Ignore collision with the Portal itself
            teleportScript.IgnoreCollision(this, PortalCollider);
            teleportScript.IgnoreCollision(this, TargetPortal.PortalCollider);

            //Ignores rear-facing colliders
            Ray ray = new Ray();
            RaycastHit[] hit;

            //Enables the buffer wall if it is disabled
            foreach (Transform tran in BufferWallObj.transform)
            {
                Collider c = tran.GetComponent<Collider>();
                c.enabled = true;
            }

            foreach (Vector3 v in checkedVerts)
            {
                ray.origin = transform.TransformPoint(v) + transform.forward * 0.01f;
                ray.direction = -transform.forward;
                hit = Physics.RaycastAll(ray, 1 * transform.parent.localScale.z, ~0, QueryTriggerInteraction.Collide);
                Debug.DrawRay(ray.origin, -transform.forward * transform.parent.localScale.z, Color.cyan, 10);
                if (hit.Length <= 0) continue;
                foreach (RaycastHit h in hit)
                {
                    //Never ignore collisions with teleportables
                    //Never ignore collisions with teleportables

                    if (h.collider.gameObject.tag.Equals("PhysicsPassthroughDuplicate") ||
                        h.collider.gameObject.GetComponent<Teleportable>() != null)
                        continue;

                    if (h.collider.transform.parent && transform.parent && h.collider.transform.parent.parent && transform.parent.parent)
                    {
                        if (h.collider.transform.parent.parent != transform.parent.parent)
                            teleportScript.IgnoreCollision(this, h.collider);

                    }
                    else
                    {
                        teleportScript.IgnoreCollision(this, h.collider);
                    }
                }
            }

            //todo: Remove this when multiple simultaneous portal intersection gets added
            //TargetPortal.RemoveTeleportable(teleportScript);
            UpdateDopplegangers();

        }

        /// <summary>
        /// Removes a teleportable from the portal area
        /// </summary>
        /// <param name="teleportScript">teleportable to remove</param>
        private void RemoveTeleportable(Teleportable teleportScript)
        {
            if (!teleportScript || !NearTeleportables.Contains(teleportScript)) return;
            if (teleportScript.RemovedLastFrame) return;
            teleportScript.RemovedLastFrame = true;
            if (!NonObliqueOverride)
            {
                teleportScript.SetClipPlane(Vector3.zero, Vector3.zero, teleportScript.Renderers.Keys);
                teleportScript.SetClipPlane(Vector3.zero, Vector3.zero, teleportScript.Renderers.Values);
            }

            UpdateDopplegangers();
            NearTeleportables.Remove(teleportScript);

            if (NearTeleportables.Count < 1)
                SetBufferWallActive(false);

            teleportScript.LeavePortal();
            //teleportScript.ResetDoppleganger();
            teleportScript.ResumeAllCollision(this);
        }

        /// <summary>
        /// Returns the quad verts of the near clip plane
        /// </summary>
        /// <param name="headCam">The camera to return</param>
        /// <returns></returns>
        private Vector3[] EyeNearPlaneDimensions(Camera headCam)
        {
            float nearClip = headCam.nearClipPlane;//get length
            float fov = headCam.fieldOfView * 0.5f;//get angle
            fov = fov * Mathf.Deg2Rad;//convert tor radians
            float h = (Mathf.Tan(fov) * nearClip);//calc height
            float w = (h / headCam.pixelHeight) * headCam.pixelWidth;//deduct width
            //Vr eye fudging
#if SKS_VR
            Vector3 eyeDiff = Quaternion.Inverse(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head)) * (UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.RightEye)
                                                                                                                               - UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.LeftEye));
            w += Mathf.Abs(eyeDiff.z) / 2;
            h += Mathf.Abs(eyeDiff.z) / 2;
#endif

            Vector3[] returnedVerts = new Vector3[4];
            //Upper left
            returnedVerts[0] = new Vector3(-w * 2f, h, headCam.nearClipPlane * 2f);
            //Upper right
            returnedVerts[1] = new Vector3(w * 2f, h, headCam.nearClipPlane * 2f);
            //Lower left
            returnedVerts[2] = new Vector3(-w * 2f, -h, headCam.nearClipPlane * 2f);
            //Lower right
            returnedVerts[3] = new Vector3(w * 2f, -h, headCam.nearClipPlane * 2f);
            //Camera position
            //returnedVerts[4] = Vector3.zero;
            return returnedVerts;
        }

        /// <summary>
        /// Called when another Portal is sending a camera to this Portal
        /// </summary>
        public void IncomingCamera()
        {
            //Debug.Break();
            _cheeseActivated = -1;
            _headInPortalTrigger = true;
            UpdateBackPosition();
            UpdateDopplegangers();
            UpdateEffects();
            //TargetPortal.PortalCamera.ForceResetRender();
            PortalCamera.ForceResetRender();
            WakeBufferWall();
            //TargetPortal.TryRenderPortal(_headCamera, _nearClipVertsGlobal);
            //TryRenderPortal(_headCamera, _nearClipVertsGlobal);

        }

        /// <summary>
        /// Post-Deletion cleanup
        /// </summary>
        private void OnDisable()
        {
            foreach (Collider c in PassthroughColliders.Keys)
            {
                if (PassthroughColliders[c])
                    Destroy(PassthroughColliders[c]);
            }
        }

        /// <summary>
        /// Wake the buffer wall. Walls spawn disabled to prevent fast-moving objects from colliding with them before any objects have ever entered the teleportable.
        /// </summary>
        private void WakeBufferWall()
        {
            if (BufferWall.Count > 0)
            {
                if (BufferWall[0].attachedRigidbody)
                    BufferWall[0].attachedRigidbody.WakeUp();
            }
        }

        private void SetBufferWallActive(bool active)
        {
            if (BufferWall.Count > 0)
            {
                foreach (Collider c in BufferWall)
                {
                    c.enabled = active;
                }
            }
        }
    }
}