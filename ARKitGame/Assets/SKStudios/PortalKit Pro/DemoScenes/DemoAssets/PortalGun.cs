using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class PortalGun : MonoBehaviour {
        public PortalPayload PortalPayload1;
        public PortalPayload PortalPayload2;


        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                PortalPayload1.gameObject.SetActive(true);
                PortalPayload1.Travelling = true;
                PortalPayload1.transform.rotation = transform.rotation;
                PortalPayload1.transform.position = transform.position + transform.forward;
            }
            if (Input.GetMouseButtonDown(1)) {
                PortalPayload2.gameObject.SetActive(true);
                PortalPayload2.Travelling = true;
                PortalPayload2.transform.rotation = transform.rotation;
                PortalPayload2.transform.position = transform.position + transform.forward;
            }
        }
    }
}
