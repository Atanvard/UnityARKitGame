using SKStudios.Common.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SKStudios.Portals
{
    /// <summary>
    /// Class allowing for scripts on gameobjects to be teleported separately from the object proper.
    /// </summary>
    public abstract class TeleportableScript : MonoBehaviour
    {
        [HideInInspector]
        public Portal currentPortal;
        [HideInInspector]
        public Teleportable teleportable;
        [HideInInspector]
        public bool throughPortal = false;
        public bool teleportScriptIndependantly = true;

        private Transform originalParent;
        private Transform otherTransformParent;
        private Transform otherTransform;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;

        public virtual void Initialize(Teleportable teleportable)
        {
            this.teleportable = teleportable;
            originalParent = transform.parent;
            try
            {
                otherTransform = SKSGeneralUtils.FindAnalogousTransform(transform, teleportable.root, teleportable.Doppleganger.transform, true);
                otherTransformParent = otherTransform.parent;
            }
            catch (System.NullReferenceException e)
            {
                Debug.LogError("Teleportablescript on " + name + "had a problem:" + e.Message);
            }

            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
            originalScale = transform.localScale;
        }

        // Update is called once per frame
        public virtual void CustomUpdate()
        {
            
            if (throughPortal)
            {
                Teleport();
            }
            
            //Check if the gameobject this script is attached to is through a Portal
            if (!throughPortal && currentPortal && PortalUtils.IsBehind(transform.position, currentPortal.Origin.position, currentPortal.Origin.forward * 1.01f)) {
                    //If it is, move the gameobject to the Doppleganger
                    StartCoroutine(DelayedTeleport());
            }
            else if (throughPortal && currentPortal && PortalUtils.IsBehind(transform.position, currentPortal.TargetPortal.Origin.position, currentPortal.TargetPortal.Origin.forward * 1.01f))
            {
                StartCoroutine(DelayedUnTeleport());
            }
        }

        IEnumerator DelayedTeleport() {
            
               
            //Is this script going to teleport before its primary object?
            if (teleportScriptIndependantly)
            {
                
                otherTransform.SetParent(transform.parent);
                throughPortal = true;
                ActivateInheritance(gameObject);
                Teleport();
            }
            //yield return new WaitForEndOfFrame();
            OnPassthrough();
            yield return null;
        }

        IEnumerator DelayedUnTeleport()
        {


            if (teleportScriptIndependantly)
            {

                otherTransform.SetParent(otherTransformParent);
                transform.SetParent(originalParent);
                throughPortal = false;
                ResetTransform();
            }
            OnPassthrough();

            yield return new WaitForEndOfFrame();
        }

        public virtual void Teleport()
        {
            transform.SetParent(otherTransformParent);
            ResetTransform();
        }
        //Hook for detecting script Portal passthrough
        protected virtual void OnPassthrough() { }

        //Hook for detecting parent object teleport
        public virtual void OnTeleport() { }

        public virtual void LeavePortal()
        {
            if (teleportScriptIndependantly)
            {
                transform.SetParent(originalParent);
                throughPortal = false;

                ResetTransform();
                currentPortal = null;
            }
            OnPassthrough();
        }

        private void ActivateInheritance(GameObject child)
        {
            child.SetActive(true);
            if (child.transform.parent != null)
                ActivateInheritance(child.transform.parent.gameObject);
        }

        private void ResetTransform()
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            transform.localScale = originalScale;
        }

        public void OnTriggerEnter(Collider other) {
            if (throughPortal)
                return;
            PortalTrigger t;
            if (t = other.GetComponent<PortalTrigger>())
            {
                if(teleportable)
                    t.OnTriggerEnter(teleportable.GetComponent<Collider>());
            }
        }


        public void OnTriggerStay(Collider other)
        {
            if (throughPortal)
                return;
            PortalTrigger t;
            if (t = other.GetComponent<PortalTrigger>()) {
                if (!teleportable)
                    return;
                Collider c = teleportable.GetComponent<Collider>();
                t.OnTriggerStay(c);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (throughPortal)
                return;
            PortalTrigger t;
            if (t = other.GetComponent<PortalTrigger>())
            {
                if(t && teleportable)
                    t.OnTriggerExit(teleportable.GetComponent<Collider>());
            }
        }

        /*
        public void OnTriggerEnter(Collider other)
        {
            PortalTrigger trigger;
            if (trigger = other.GetComponent<PortalTrigger>())
            {
                trigger.SendMessage("OnTriggerEnter", teleportable.GetComponent<Collider>());
            }
            
        }*/

    }
}
