using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SKStudios.Common.Utils;
using SKStudios.Common.Extensions;
using SKStudios.Common.Utils.SafeRemoveComponent;
using SKStudios.ProtectedLibs.Portals.Teleportable;
using SKStudios.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if VR_PORTALS
using VRTK;
#endif
namespace SKStudios.Portals
{
    public class Teleportable : MonoBehaviour
    {
        //The root of the game object to be copied
        public Transform root;

        //Material to replace all other materials on this object with (optional)
        public Material replacementMaterial;

        //Is the teleportable visual-only?
        public bool VisOnly;
        private bool _visOnly;

        //Delay (optional)
        public float delay;

        //Should scripts be stripped from the teleportable's doppleganger? 
        bool StripScripts = true;
        //Should scripts be stripped from the teleportable's doppleganger? 
        bool StripColliders = true;
        //Should scripts be stripped from the teleportable's doppleganger? 
        bool StripJoints = true;
        //Should scripts be stripped from the teleportable's doppleganger? 
        bool StripRigidbodies = true;
        private BoxCollider _boundsCollider;
        private BoxCollider BoundsCollider {
            get {
                if (!_boundsCollider)
                {
                    GameObject colliderObj = new GameObject();
                    colliderObj.transform.SetParent(transform, false);
                    _boundsCollider = colliderObj.AddComponent<BoxCollider>();
                    _boundsCollider.isTrigger = true;
                }
                return _boundsCollider;
            }
        }

        //In the form of Original, Duplicate
        public Dictionary<Transform, Transform> Transforms { get; private set; }

        public Dictionary<Renderer, Renderer> Renderers { get; private set; }
        public HashSet<SkinnedMeshRenderer> SkinnedRenderers { get; private set; }
        public HashSet<TeleportableScript> TeleportableScripts { get; private set; }

        public Dictionary<int, Collider> Colliders { get; private set; }

        private bool _teleportedLastFrame = false;
        [HideInInspector]
        public bool TeleportedLastFrame {
            get { return _teleportedLastFrame; }
            set {
                if (value)
                    kludgeExtraFrame = true;
                _teleportedLastFrame = value;
            }
        }

        private bool _addedLastFrame = false;
        [HideInInspector]
        public bool AddedLastFrame {
            get { return _addedLastFrame; }
            set {
                if (value)
                    kludgeExtraFrame = true;
                _addedLastFrame = value;
            }
        }

        private bool _removedLastFrame = false;
        [HideInInspector]
        public bool RemovedLastFrame {
            get { return _removedLastFrame; }
            set {
                if (value)
                    kludgeExtraFrame = true;
                _removedLastFrame = value;
            }
        }

        [HideInInspector] public GameObject Doppleganger;
        [HideInInspector] public bool MovementOverride;
        [HideInInspector] public Bounds TeleportableBounds;

        public bool IsActive = true;

        //Collider collections for the purpose of physics through portals
        private Dictionary<Portal, HashSet<Collider>> _ignoredColliders;
        private Dictionary<Portal, HashSet<Collider>> _addedColliders;

        private TeleportableLib _teleLib;
        private TeleportableLib TeleLib {
            get {
                if (!_teleLib)
                    _teleLib = gameObject.AddComponent<TeleportableLib>();
                return _teleLib;
            }
        }
        Animator test;
        [HideInInspector] public bool initialized = false;

#if VR_PORTALS
        [HideInInspector] VRTK_InteractableObject interactable;
#endif

        void Start()
        {
            /*if (!gameObject.activeInHierarchy)
                return;

            _teleLib = gameObject.AddComponent<TeleportableLib>();
            _ignoredColliders = new Dictionary<Portal, HashSet<Collider>>();
            _addedColliders = new Dictionary<Portal, HashSet<Collider>>();

            Transforms = new Dictionary<Transform, Transform>();
            Renderers = new Dictionary<Renderer, Renderer>();
            SkinnedRenderers = new HashSet<SkinnedMeshRenderer>();
            TeleportableScripts = new HashSet<TeleportableScript>();

            Colliders = new HashSet<Collider>();
            if (!root)
                root = transform;
            StartCoroutine(SpawnDoppleganger());


#if VR_PORTALS
            interactable = gameObject.GetComponent<VRTK_InteractableObject>();
#endif
            _visOnly = VisOnly;

            Doppleganger.ApplyHideFlagsRecursive(HideFlags.HideAndDontSave);
            UpdateBounds();*/
        }

        private void OnEnable()
        {

#if UNITY_EDITOR
            ConsoleCallbackHandler.AddCallback(() => {
                Dependencies.RescanDictionary();
                StartCoroutine(ConsoleTheConsole());
            }, LogType.Error, "Can't remove");
            if (PrefabUtility.GetPrefabParent(gameObject) == null &&
                PrefabUtility.GetPrefabObject(gameObject) != null)
                return;
#endif
            // if (!gameObject.activeInHierarchy)
            //   return;

            //TeleLib = gameObject.AddComponent<TeleportableLib>();
            //Reset the state of ignored and added colliders
            if (_ignoredColliders != null)
                foreach (Portal p in _ignoredColliders.Keys)
                    ResumeAllCollision(p);

            if (Doppleganger)
                Destroy(Doppleganger);

            _ignoredColliders = new Dictionary<Portal, HashSet<Collider>>();
            _addedColliders = new Dictionary<Portal, HashSet<Collider>>();

            Transforms = new Dictionary<Transform, Transform>();
            Renderers = new Dictionary<Renderer, Renderer>();
            SkinnedRenderers = new HashSet<SkinnedMeshRenderer>();
            TeleportableScripts = new HashSet<TeleportableScript>();

            Colliders = new Dictionary<int, Collider>();
            if (!root)
                root = transform;
            StartCoroutine(SpawnDoppleganger());


#if VR_PORTALS
            interactable = gameObject.GetComponent<VRTK_InteractableObject>();
#endif
            _visOnly = VisOnly;

            //Doppleganger.ApplyHideFlagsRecursive(HideFlags.HideAndDontSave);
            UpdateBounds();
            TeleportableBounds.center = transform.position;
            
        }

        //I am a master of wit
        private IEnumerator ConsoleTheConsole() {
            yield return new WaitForEndOfFrame();
            Debug.ClearDeveloperConsole();
            Debug.Log("<color=#2599f5>[PKPRO]</color> Dependency graph not computed! Caught exception and wrote Dependency graph.");
        }

        private IEnumerator SpawnDoppleganger()
        {
            //Wait for any initializing behaviors
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            //Spawns the Doppleganger game object
            GameObject doppleDisabler = new GameObject("Doppleganger Temp");
            doppleDisabler.SetActive(false);
            Doppleganger = (GameObject)(Instantiate(root.gameObject, doppleDisabler.transform));

            //Disables behaviors to prevent double instantiation
            IEnumerable<Component> dopplBehaviours = Doppleganger.GetComponentsInChildren<Behaviour>();
            foreach (Component c in dopplBehaviours)
            {
                if (c)
                    if (!(c is Light) && c is Behaviour)
                        ((Behaviour)c).enabled = false;
            }


            InstantiateDoppleganger(Doppleganger.transform);

            //Exchanges material for cullable
            if (replacementMaterial)
            {
                //yield return new WaitForEndOfFrame();
                foreach (Renderer renderer in Renderers.Keys)
                {
                    if (renderer.GetType() == typeof(ParticleSystemRenderer))
                        continue;
                    Material[] newMats = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        Material oNewMat = new Material(replacementMaterial);
                        Material m = renderer.sharedMaterials[i];
                        if (!oNewMat || !m)
                            continue;
                        oNewMat.CopyPropertiesFromMaterial(m);
                        newMats[i] = oNewMat;
                    }
                    renderer.sharedMaterials = newMats;
                }

                //yield return new WaitForEndOfFrame();

                foreach (Renderer renderer in Renderers.Values)
                {
                    Material[] newMats = new Material[renderer.sharedMaterials.Length];
                    if (renderer.GetType() == typeof(ParticleSystemRenderer))
                        continue;
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        Material dNewMat = new Material(replacementMaterial);
                        Material m = renderer.sharedMaterials[i];
                        if (!dNewMat || !m)
                            continue;
                        dNewMat.CopyPropertiesFromMaterial(m);
                        newMats[i] = dNewMat;
                    }
                    renderer.sharedMaterials = newMats;
                }
            }

            //Initializes teleportable scripts
            foreach (TeleportableScript ts in TeleportableScripts)
            {
                if (!ts) continue;
                ts.Initialize(this);
            }

            Doppleganger.transform.SetParent(transform);
            Destroy(doppleDisabler);

            ResetDoppleganger();
            initialized = true;
            Doppleganger.name = Doppleganger.name + " (Doppleganger)";
            UpdateBounds();
            yield return null;
        }

        private void InstantiateDoppleganger(Transform currentLevel)
        {
            Transform other = SKSGeneralUtils.FindAnalogousTransform(currentLevel, Doppleganger.transform, root, true);
            if (currentLevel.tag.Equals("MainCamera"))
                currentLevel.tag = "Untagged";
            currentLevel.gameObject.name = currentLevel.gameObject.name;
            foreach (Component component in currentLevel.GetComponents<Component>())
            {
                if (component is Teleportable)
                {
                    component.SafeDestroyComponent();
                }
                //Copies Transforms for later updating
                else if (component is Transform)
                {
                    if (other)
                    {
                        if (!Transforms.ContainsKey(other))
                        {
                            Transforms.Add(other, (Transform)component);
                        }
                    }
                    else
                    {
                        Destroy(currentLevel.gameObject);
                        break;
                    }
                }
                else if (component is Renderer)
                {
                    if (component is SkinnedMeshRenderer)
                    {
                        SkinnedRenderers.Add(component as SkinnedMeshRenderer);
                    }
                    if (component is Renderer)
                    {
                        if (!Renderers.ContainsKey((Renderer)component))
                            Renderers[other.GetComponent<Renderer>()] = (Renderer)component;
                        //Adds colliders to list for collision ignoring upon Portal entry
                    }
                }
                else if (component is Collider) {
                    if (!component.GetComponent<TeleportablePhysExclude>())
                    { 
                        Collider c = other.GetComponent<Collider>();
                        if (!Colliders.ContainsKey(c.GetInstanceID())) {
                            
                            Colliders.Add(c.GetInstanceID(), c);
                        }
                        else {
                            //Fix for VRTK double-genning body colliders for no reason
                            currentLevel.gameObject.transform.SetParent(null);
                            c.enabled = false;
                            Colliders[c.GetInstanceID()].enabled = false;
                            c.isTrigger = true;
                            Colliders[c.GetInstanceID()].isTrigger = true;
                           DestroyImmediate(Colliders[c.GetInstanceID()].gameObject);
                            DestroyImmediate(currentLevel.gameObject);
                            return;
                        }
                        
                    }
                    if (StripColliders && component)
                        component.SafeDestroyComponent();
                }
                else if (component is Rigidbody)
                {
                    if (StripRigidbodies)
                        component.SafeDestroyComponent();
                }
                else if (component is Joint)
                {
                    if (StripJoints)
                        component.SafeDestroyComponent();
                }
                else if (component is MonoBehaviour)
                {
                    //Handling of teleportable scripts
                    if (component is TeleportableScript)
                    {
                        TeleportableScripts.Add(other.GetComponent<TeleportableScript>());
                    }

                    if (!StripScripts)
                    {
                        if (component != null)
                            ((MonoBehaviour)component).enabled = true;
                    }
                    else
                    {
                        component.SafeDestroyComponent();
                    }
                    //Nonspecific setup copying
                }
                else if (component is MeshFilter || component is ParticleSystem || component is Light)
                {
                    //nothin to do
                }
                else
                {
                    component.SafeDestroyComponent();
                }
            }

            if (other)
                currentLevel.gameObject.SetActive(other.gameObject.activeSelf);

            foreach (Transform t in currentLevel)
                InstantiateDoppleganger(t);
        }

        /// <summary>
        /// Update the bounds of the teleportable to include all colliders
        /// </summary>
        private void UpdateBounds()
        {
            if (Colliders.Count <= 0) return;
            TeleportableBounds.size = Vector3.zero;
            Collider col = Colliders.Values.ElementAt(0);
            if (!col) return;

            bool centered = false;
            if (TeleportableScripts.Count == 0)
            {

                foreach (Collider c in Colliders.Values)
                {
                    if (!centered)
                        TeleportableBounds.center = c.transform.position;
                    TeleportableBounds.Encapsulate(c.bounds);
                }
                //if (c.GetComponent<TeleportableScript>() != null)
                if (!centered)
                    TeleportableBounds.center = transform.position;
            }
            //else
            //{
            UpdateBoundsRecursive(root);
            //}
            BoundsCollider.transform.position = Vector3.zero;
            BoundsCollider.transform.rotation = Quaternion.identity;
            BoundsCollider.transform.localScale = Vector3.one;
            BoundsCollider.transform.localScale = BoundsCollider.transform.InverseTransformVector(Vector3.one);

            BoundsCollider.center = TeleportableBounds.center;
            BoundsCollider.size = TeleportableBounds.size;
        }

        /// <summary>
        /// Update recursively, ignoring branches with teleportablescripts on them
        /// </summary>
        /// <param name="currentLevel"></param>
        private void UpdateBoundsRecursive(Transform currentLevel)
        {
            //if (currentLevel.gameObject.SKGetComponentOnce<TeleportableScript>())
            //   return;

            Collider c = currentLevel.gameObject.SKGetComponentOnce<Collider>();
            if (c && c != BoundsCollider)
            {
                TeleportableBounds.Encapsulate(c.bounds.center);
            }

            foreach (Transform t in currentLevel)
                UpdateBoundsRecursive(t);
        }

        public void Teleport()
        {
            //ResetDoppleganger();
            foreach (TeleportableScript t in TeleportableScripts)
            {
                if (!t) continue;
                t.OnTeleport();
            }

            UpdateBounds();
            //Close out collisions to prevent ignoring of important collisions post-teleport
        }

        /// <summary>
        /// Resets the doppleganger state
        /// </summary>
        public void ResetDoppleganger()
        {
            Doppleganger.transform.SetParent(null);
            Doppleganger.transform.localPosition = Vector3.zero;
            Doppleganger.transform.localRotation = Quaternion.identity;
            //Doppleganger.SetActive(false);
            DisableDoppleganger();
        }

        void FixedUpdate()
        {
            if (!gameObject.activeInHierarchy)
                return;

        }

        // Update is called once per frame

        void LateUpdate()
        {
            if (!gameObject.activeInHierarchy || !enabled)
                return;
            //If the object is grabbed, make vis only
#if VR_PORTALS
            if (interactable)
                VisOnly = interactable.IsGrabbed() || _visOnly;
#endif
            foreach (TeleportableScript t in TeleportableScripts)
            {
                if (t && t.isActiveAndEnabled)
                    t.CustomUpdate();
            }

            UpdateBounds();

        }

        //Temp fix for issue with VR setups. It is admittedly fast, so it might not actually be temp. Time will tell.
        private bool kludgeExtraFrame = false;

        /// <summary>
        /// Reset state
        /// </summary>
        void Update()
        {
            if (!gameObject.activeInHierarchy)
                return;

            //if (!kludgeExtraFrame) {
            TeleportedLastFrame = false;
            AddedLastFrame = false;
            RemovedLastFrame = false;
            //}
            //else {
            //    kludgeExtraFrame = false;
            //    return;
            //}

        }

        /// <summary>
        /// Updates the doppleganger's visuals
        /// </summary>
        /// <returns></returns>
        public void UpdateDoppleganger()
        {
            if (!initialized) return;
            //Doppleganger.transform.SetParent(root.parent);
            //Doppleganger.transform.SetParent(null);
            Doppleganger.transform.SetParent(root.parent);
            Doppleganger.transform.localRotation = root.localRotation;
            Doppleganger.transform.localPosition = root.localPosition;
            if (!MovementOverride)
            {
                foreach (Transform t in Transforms.Keys)
                {

                    if (t == null || Transforms[t] == null || t == root)
                        continue;
                    //if(t.GetComponent<TeleportableScript>())
                    Transforms[t].localPosition = t.localPosition;
                    Transforms[t].localRotation = t.localRotation;
                }


                //Prevents renderers from being enabled mid-frame

                foreach (Transform t in Transforms.Keys)
                {
                    if (t == null || Transforms[t] == null)
                        continue;
                    Transforms[t].gameObject.SetActive(t.gameObject.activeSelf);
                }

                //yield return new WaitForEndOfFrame();
                /*
                Doppleganger.transform.localScale = root.lossyScale;
                Doppleganger.transform.position = root.position;
                Doppleganger.transform.rotation = root.rotation;*/
                Doppleganger.transform.localScale = root.localScale;
                Doppleganger.transform.localPosition = root.localPosition;
                Doppleganger.transform.localRotation = root.localRotation;
                /*
                Doppleganger.transform.localPosition = root.localPosition;
                Doppleganger.transform.localRotation = root.localRotation;*/
            }
            //yield return null;
            //StartCoroutine(UpdateDopplegangerCoroutine());
        }

        private IEnumerator UpdateDopplegangerCoroutine()
        {
            return null;
        }

        /// <summary>
        /// Enable doppleganger rendering
        /// </summary>
        public void EnableDoppleganger()
        {
            if (Doppleganger)
            {
                foreach (Renderer r in Renderers.Keys)
                    if (r && Renderers[r])
                        Renderers[r].enabled = r.enabled;

                Doppleganger.SetActive(true);
            }

        }

        /// <summary>
        /// Disables doppleganger rendering
        /// </summary>
        public void DisableDoppleganger()
        {
            if (Doppleganger)
            {
                foreach (Renderer r in Renderers.Keys)
                    if (r && Renderers[r])
                        Renderers[r].enabled = false;

                SetClipPlane(Vector3.zero, Vector3.zero, Renderers.Values);
                Doppleganger.SetActive(false);
            }

        }

        /// <summary>
        /// Sets the clip plane for the teleportable object's material
        /// </summary>
        /// <param name="position">The position of the plane</param>
        /// <param name="vector">The normal of the plane</param>
        /// <param name="renderers">The set of renderers to change</param>
        public void SetClipPlane(Vector3 position, Vector3 vector, IEnumerable<Renderer> renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                if (!renderer) continue;
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (!renderer.sharedMaterials[i]) continue;
                    renderer.sharedMaterials[i].SetVector("_ClipPosition", position);
                    renderer.sharedMaterials[i].SetVector("_ClipVector", vector);
                    if (!renderer.sharedMaterials[i].HasProperty("_ClipPosition") && SKSGlobalRenderSettings.Clipping)
                        Debug.LogWarning("Valid pixel-perfect material not set on teleportable. Object will be able to be seen through the back of portals unless this is replaced.");
                }
            }
        }

        /// <summary>
        /// Adds a child to the teleportable and updates components
        /// </summary>
        /// <param name="gameObject">GameObject to add</param>
        public void AddChild(GameObject gameObject)
        {
            gameObject.SetActive(!gameObject.activeSelf);
            GameObject newObject = Instantiate(gameObject);
            gameObject.SetActive(!gameObject.activeSelf);
            newObject.transform.parent =
                SKSGeneralUtils.FindAnalogousTransform(gameObject.transform.parent, root, Doppleganger.transform, true);
            UpdateBounds();
            InstantiateDoppleganger(newObject.transform);
        }

        /// <summary>
        /// Remove child from teleportable
        /// </summary>
        /// <param name="gameObject"></param>
        public void RemoveChild(GameObject gameObject)
        {
            Destroy(SKSGeneralUtils.FindAnalogousTransform(gameObject.transform, root, Doppleganger.transform)
                .gameObject);
        }

        /// <summary>
        /// Make Teleportable not collide with gameobject. Overridden by AddCollision by default.
        /// </summary>
        /// <param name="ignoredCollider">Collider to ignore</param>
        /// <param name="shouldOverrideAdd">Should this override added collisions?</param>
        public void IgnoreCollision(Portal portal, Collider ignoredCollider, bool shouldOverrideAdd = false)
        {
            if (!portal)
                return;

            if (!_ignoredColliders.ContainsKey(portal))
                _ignoredColliders[portal] = new HashSet<Collider>();

            if (!_addedColliders.ContainsKey(portal))
                _addedColliders[portal] = new HashSet<Collider>();

            _ignoredColliders[portal].Add(ignoredCollider);
            if (_addedColliders[portal].Contains(ignoredCollider) && !shouldOverrideAdd) return;

            foreach (Collider col in Colliders.Values)
            {
                if (col == null || ignoredCollider == null)
                    continue;
                Physics.IgnoreCollision(col, ignoredCollider, true);
                //_addedColliders.Remove(ignoredCollider);
            }
        }


        /// <summary>
        /// Make Teleportable collide with gameobject. Overrides IgnoreCollision by default.
        /// </summary>
        /// <param name="addCollider">Collider to add</param>
        public void AddCollision(Portal portal, Collider addCollider)
        {
            if (!_ignoredColliders.ContainsKey(portal))
                _ignoredColliders[portal] = new HashSet<Collider>();

            if (!_addedColliders.ContainsKey(portal))
                _addedColliders[portal] = new HashSet<Collider>();

            _addedColliders[portal].Add(addCollider);

            foreach (Collider col in Colliders.Values)
            {
                if (col == null || addCollider == null)
                    continue;
                Physics.IgnoreCollision(col, addCollider, false);
            }
        }

        /// <summary>
        /// Undo all changes made with AddCollision and RemoveCollision
        /// </summary>
        public void ResumeAllCollision(Portal portal)
        {
            if (!portal) return;

            if (_ignoredColliders == null) return;
            if (!_ignoredColliders.ContainsKey(portal))
                _ignoredColliders[portal] = new HashSet<Collider>();

            if (_addedColliders == null) return;
            if (!_addedColliders.ContainsKey(portal))
                _addedColliders[portal] = new HashSet<Collider>();

            HashSet<Portal> portals = new HashSet<Portal>(_ignoredColliders.Keys.ToList());
            Portal[] addedColliderPortals = _addedColliders.Keys.ToArray();
            foreach (Portal p in addedColliderPortals)
                portals.Add(p);

            foreach (Collider col in Colliders.Values)
            {

                foreach (Collider aCol in _addedColliders[portal])
                {
                    bool terminate = false;
                    foreach (Portal p in portals)
                    {
                        if (p == portal) continue;
                        if (_ignoredColliders[p].Contains(aCol))
                        {
                            terminate = true;
                            break;
                        }
                    }
                    if (terminate)
                        continue;
                    if (aCol && col)
                        Physics.IgnoreCollision(col, aCol, true);
                }

                foreach (Collider iCol in _ignoredColliders[portal])
                {
                    bool terminate = false;
                    foreach (Portal p in portals)
                    {
                        if (p == portal) continue;
                        if (_ignoredColliders[p].Contains(iCol))
                        {
                            terminate = true;
                            break;
                        }
                    }
                    if (terminate)
                        continue;
                    if (iCol && col)
                        Physics.IgnoreCollision(col, iCol, false);
                }
            }
            _ignoredColliders.Remove(portal);
            _addedColliders.Remove(portal);
        }

        /// <summary>
        /// Set the portal info of the currently active portal
        /// </summary>
        /// <param name="portal"></param>
        public void SetPortalInfo(Portal portal)
        {
            if (TeleportableScripts != null)
                foreach (TeleportableScript ts in TeleportableScripts)
                {
                    if (!ts) continue;
                    ts.currentPortal = portal;
                }
        }

        /// <summary>
        /// Leave the portal volume
        /// </summary>
        public void LeavePortal()
        {
            if (TeleportableScripts != null)
                foreach (TeleportableScript ts in TeleportableScripts)
                {
                    if (!ts) continue;
                    ts.LeavePortal();
                }
            DisableDoppleganger();
        }


        private void OnDestroy()
        {
            if (TeleportableScripts != null)
                foreach (TeleportableScript ts in TeleportableScripts)
                {
                    if (!ts) continue;
                    Destroy(ts);
                }
            Destroy(Doppleganger);
            Destroy(TeleLib);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 1, 0.4f);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
#if UNITY_EDITOR
            Handles.Label(transform.position, "Bounds of Teleportable", style);
#endif

            Gizmos.DrawCube(TeleportableBounds.center, TeleportableBounds.extents * 2f);
        }
    }
}