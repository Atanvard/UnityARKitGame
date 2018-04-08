using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SKStudios.Portals {
    public class PortalTrigger : MonoBehaviour {
        //Trigger segregated from Portal to prevent scaling interaction
        public Portal portal;

        void Awake() {
            this.enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
        }

        public void OnTriggerEnter(Collider col)
        {
            if (!col || !portal)
                return;
            portal.E_OnTriggerEnter(col);
        }

        public void OnTriggerStay(Collider col) {
            if (!col || !portal)
                return;
            portal.E_OnTriggerStay(col);
        }

        public void OnTriggerExit(Collider col) {
            if (!col || !portal)
                return;
            portal.E_OnTriggerExit(col);
        }
    }
}
