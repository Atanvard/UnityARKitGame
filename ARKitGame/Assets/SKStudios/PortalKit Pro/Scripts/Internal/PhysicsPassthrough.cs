using Eppy;
using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Extensions;
using SKStudios.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SKStudios.Portals
{
    public class PhysicsPassthrough : MonoBehaviour
    {
        public bool Initialized = false;
        private Bounds _bounds;
        private PhysicsPassthroughScanner _scanner;

        /// <summary>
        /// The target portal
        /// </summary>
        [HideInInspector] public Portal Portal;
        /// <summary>
        /// Collection of all ignored colliders 
        /// </summary>
        [HideInInspector] public HashSet<Collider> IgnoredColliders;
        /// <summary>
        /// All colliders managed by this class
        /// </summary>
        [HideInInspector] public HashSet<int> Colliders;
        /// <summary>
        /// All colliders that can not be managed by this class
        /// </summary>
        [HideInInspector] public HashSet<int> BumColliders;
        private BoxCollider _collider;

        /// <summary>
        /// Initialize the scanner with the given "parent" Portal.
        /// </summary>
        /// <param name="portal">The Portal that owns this scanner</param>
        public void Initialize(Portal portal)
        {
            if (Initialized) return;

            IgnoredColliders = new HashSet<Collider>();
            Colliders = new HashSet<int>();
            BumColliders = new HashSet<int>();
            _collider = gameObject.GetComponent<BoxCollider>();

            this.Portal = portal;
            Initialized = true;
            _collider.enabled = true;
            _collider.size = Vector3.zero;

            foreach (Transform t in transform)
                if (_scanner = t.GetComponent<PhysicsPassthroughScanner>())
                    break;

            if (_scanner)
            {
                _scanner.Initialize(this);
            }
            else
            {
                Debug.LogWarning("No PhysicsPassthroughScanner child found on PhysicsPassthrough! Physics passthrough will not function as expected.");
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update()
        {
            if (!Initialized || !Portal.ArrivalTarget) return;
            //Used for positioning the collision volume
            transform.position = Portal.ArrivalTarget.position + Vector3.Scale(_bounds.extents, -Portal.ArrivalTarget.forward * 1.1f); //(Portal.ArrivalTarget.forward * (Quaternion.Inverse(Portal.TargetPortal.Origin.rotation) * bounds.extents).z);
            Vector3 scale = Portal.Origin.InverseTransformVector(Vector3.one * Portal.TargetPortal.Origin.lossyScale.magnitude) * 0.9f;
            scale.x = Mathf.Abs(scale.x);
            scale.y = Mathf.Abs(scale.y);
            scale.z = Mathf.Abs(scale.z);
            transform.localScale = scale;
            transform.rotation = Portal.ArrivalTarget.rotation;
            _collider.size = Vector3.one * 0.9f;
            _bounds = _collider.bounds;
        }

        /// <summary>
        /// Add a collider to the physics passthrough tracker
        /// </summary>
        /// <param name="col">the collider to add</param>
        public void OnTriggerEnter(Collider col)
        {
            if (!Initialized) return;
            //Skip triggers
            if (col.isTrigger)
                return;
            //Ignores unresolvable colliders
            if (BumColliders.Contains(col.GetInstanceID()))
                return;
            //Only run if ready and enabled
            if (!SKSGlobalRenderSettings.PhysicsPassthrough || !Initialized) return;

            //Early rejection for invalid colliders
            if (!col
                || col.gameObject.layer.Equals(LayerMask.NameToLayer("Portal"))
                || col.isTrigger
                || col.gameObject.tag.Equals("PhysicsPassthroughDuplicate")
                || Colliders.Contains(col.GetInstanceID()))
                return;

            if (IgnoredColliders.Count == 0)
            {
                StartCoroutine(DelayedDetect(col));
                return;
            }

            //Instantiate new collider copy
            GameObject newCollider = new GameObject { isStatic = false };
            Collider newColliderComponent = new Collider();


            //Unfortunately no better way to do this
            if (col is BoxCollider)
            {
                newColliderComponent = newCollider.AddComponent<BoxCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.GetCopyOf((BoxCollider)col);
                ((BoxCollider)newColliderComponent).size = ((BoxCollider)col).size;
                ((BoxCollider)newColliderComponent).center = ((BoxCollider)col).center;
            }
            else if (col is MeshCollider)
            {
                /*if (!((MeshCollider)col).convex)
                {
                    newColliderComponent = newCollider.AddComponent<BoxCollider>();
                    ((BoxCollider) newColliderComponent).size = col.bounds.size / col.transform.lossyScale.magnitude;
                    newColliderComponent.enabled = false;
                    goto AFTERDETECTION;
                }*/

                newColliderComponent = newCollider.AddComponent<MeshCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.GetCopyOf((MeshCollider)col);

            }
            else if (col is CapsuleCollider)
            {
                newColliderComponent = newCollider.AddComponent<CapsuleCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.GetCopyOf((CapsuleCollider)col);

            }
            else if (col is SphereCollider)
            {
                newColliderComponent = newCollider.AddComponent<SphereCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.GetCopyOf((SphereCollider)col);

            }
            else if (col is CharacterController)
            {
                newColliderComponent = newCollider.AddComponent<CapsuleCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.GetCopyOf((CapsuleCollider)col);
            }
            else if (col is TerrainCollider)
            {
                newColliderComponent = newCollider.AddComponent<TerrainCollider>();
                newColliderComponent.enabled = false;
                //newColliderComponent = newColliderComponent.GetCopyOf((TerrainCollider)col);
                ((TerrainCollider)newColliderComponent).terrainData = ((TerrainCollider)col).terrainData;
            }
            else
            {
                BumColliders.Add(col.GetInstanceID());
                return;
            }

            AFTERDETECTION:
            bool test = false;

            foreach (Collider c in IgnoredColliders)
            {
                if (c && newColliderComponent)
                    Physics.IgnoreCollision(c, newColliderComponent, true);
            }

            UpdatePhysicsDupe(col, newColliderComponent);

            newCollider.layer = 2;
            newCollider.tag = "PhysicsPassthroughDuplicate";
            newCollider.name = "Duplicate Collider of " + col.name;
            newCollider.SetActive(false);
            _scanner.Update();

            StartCoroutine(DelayedEnable(newCollider, newColliderComponent));

            FinishInstantiation(col, newCollider);
        }

        private void FinishInstantiation(Collider col, GameObject newCollider)
        {
            Portal.PassthroughColliders.Add(col, newCollider.GetComponent<Collider>());
            Colliders.Add(col.GetInstanceID());
        }

        private IEnumerator DelayedDetect(Collider col)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            if (col)
                OnTriggerEnter(col);
        }

        private IEnumerator DelayedEnable(GameObject obj, Collider col)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            if (obj)
                obj.SetActive(true);
            if (col)
                col.enabled = true;
        }

        /// <summary>
        /// Draw the bounding box for the passthrough manager
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            if (!Initialized) return;

            GUIStyle style = new GUIStyle(new GUIStyle() { alignment = TextAnchor.MiddleCenter });
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            Handles.Label(_bounds.center + transform.up, "Physics Passthrough Detection Zone", style);
#endif

            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(_bounds.center, _bounds.extents * 2f);
        }

        /// <summary>
        /// Remove collider's dupes that leave the detection zone
        /// </summary>
        /// <param name="col"></param>
        public void OnTriggerExit(Collider col)
        {
            //Was the collider that left the volume one of the dupes? If so, destroy.
            if (!Initialized) return;
            Collider toDestroy;

            if (!Portal.PassthroughColliders.TryGetValue(col, out toDestroy))
                return;

            toDestroy.enabled = false;
            Destroy(toDestroy.gameObject);
            Portal.PassthroughColliders.Remove(col);
            Colliders.Remove(col.GetInstanceID());

        }

        /// <summary>
        /// Used while teleporting objects to prevent physics step kickback
        /// </summary>
        /// <param name="col">the collider to rescan</param>
        public void ForceRescanOnColliders(IEnumerable<Collider> cols)
        {
            foreach (Collider col in cols)
            {
                if (col && _scanner.GetComponent<Collider>().bounds.Intersects(col.bounds))
                    _scanner.OnTriggerEnter(col);

            }
        }

        /// <summary>
        /// Realtime physics interactions on other side of Portal. Only triggered when teleportable is near.
        /// </summary>
        public void UpdatePhysics()
        {
            if (!SKSGlobalRenderSettings.PhysicsPassthrough) return;

            foreach (Collider original in Portal.PassthroughColliders.Keys)
            {
                //Get the analogous collider
                Collider dupe;
                if (!Portal.PassthroughColliders.TryGetValue(original, out dupe))
                    continue;
                UpdatePhysicsDupe(original, dupe);

            }
        }

        /// <summary>
        /// Update the location of a physical duplicate of another collider
        /// </summary>
        /// <param name="original">The original collider</param>
        /// <param name="dupe">The Collider to move</param>
        private void UpdatePhysicsDupe(Collider original, Collider dupe)
        {
            if (!original || !dupe)
                return;
            bool enabled = dupe.enabled;
            dupe.enabled = false;
            dupe.transform.SetParent(original.transform);
            dupe.transform.localScale = Vector3.one;
            dupe.transform.localRotation = Quaternion.identity;
            dupe.transform.localPosition = Vector3.zero;

            dupe.transform.SetParent(Portal.ArrivalTarget, true);
            dupe.transform.SetParent(Portal.Origin, false);
            dupe.enabled = enabled;
        }
    }
}
