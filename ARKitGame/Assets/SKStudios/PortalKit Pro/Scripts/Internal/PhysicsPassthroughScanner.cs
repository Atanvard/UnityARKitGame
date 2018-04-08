using Eppy;
using SKStudios.Portals;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace SKStudios.Portals {
    /// <summary>
    /// Used to prevent non teleportable objects from interacting with physics passthrough duplicates
    /// </summary>
    public class PhysicsPassthroughScanner : MonoBehaviour {
        private PhysicsPassthrough _passthrough;
        public BoxCollider Collider;
        private const float Bufferfactor = 0.4f;
        private Vector3 _currentSize;


        /// <summary>
        /// Initialize this passthrough scanner
        /// </summary>
        /// <param name="passthrough">the "parent" scanner</param>
        public void Initialize(PhysicsPassthrough passthrough) {
            _passthrough = passthrough;
            Collider = gameObject.GetComponent<BoxCollider>();
            Collider.enabled = true;
            
        }

        /// <summary>
        /// Mantains constant size on the boundaries of the scanner. Grows the bounds to include new duplicates.
        /// </summary>
        public void Update() {

            if (!_passthrough || !Collider || !Collider.enabled)
                return;

            transform.position = _passthrough.Portal.Origin.position;
            Bounds b = new Bounds();
            b.center = transform.position;
            
            if (b.size.magnitude > 0)
                b.size -= b.size / 10f;

            foreach (Collider c in _passthrough.Portal.BufferWall) {
                if(c)
                    b.Encapsulate(c.bounds);
            }
               
            
            foreach (Collider c in _passthrough.Portal.PassthroughColliders.Values) {
                if(c)
                    b.Encapsulate(c.bounds);
            }
                

            Collider.center = transform.InverseTransformPoint(b.center);
            Vector3 size = transform.InverseTransformVector(b.size);
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            _currentSize = size;
            Collider.size = _currentSize + (_currentSize * Bufferfactor);
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Adds a new collider to be ignored when it enters the detection bounds
        /// </summary>
        /// <param name="col">collider to be ignored</param>
        public void OnTriggerEnter(Collider col) {
    
            if (!col || col.gameObject.isStatic || col.CompareTag("PhysicsPassthroughDuplicate"))
                return;

            Teleportable teleportable;
            
            if (col.attachedRigidbody != null && (teleportable = col.attachedRigidbody.GetComponent<Teleportable>()))
            {
                if (_passthrough.Portal.NearTeleportables.Contains(teleportable))
                    return;
            }
            
            foreach (Collider c in _passthrough.Portal.PassthroughColliders.Values) {
                Physics.IgnoreCollision(c, col, true);
            }

            if(_passthrough.Portal.TargetPortal)
            foreach (Collider c in _passthrough.Portal.TargetPortal.BufferWall) {
                if(c)
                    Physics.IgnoreCollision(c, col, true);
            }

            _passthrough.IgnoredColliders.Add(col);
        }

        /// <summary>
        /// Detect colliders leaving the volume
        /// </summary>
        /// <param name="col"></param>
        public void OnTriggerExit(Collider col) {
            _passthrough.IgnoredColliders.Remove(col);  
        }

        /// <summary>
        /// Draw the gizmos for the passthrough scanner
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            if (!Collider) return;

            GUIStyle style = new GUIStyle(new GUIStyle() { alignment = TextAnchor.MiddleCenter });
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            Handles.Label(Collider.bounds.center, "Physics Passthrough Scanner volume", style);
#endif

            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawCube(Collider.bounds.center, Collider.bounds.extents * 2f);
        }
    }
}