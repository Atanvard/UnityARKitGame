using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Eppy;
namespace SKStudios.Portals.Demos
{
    public class LaserEmitter : MonoBehaviour {

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            Ray ray = new Ray(transform.position, transform.forward);
            Tuple<Ray, RaycastHit> hit = PortalUtils.TeleportableRaycast(ray, 100, ~0, QueryTriggerInteraction.Ignore);
            Debug.Log(hit);
        }
    }
}
