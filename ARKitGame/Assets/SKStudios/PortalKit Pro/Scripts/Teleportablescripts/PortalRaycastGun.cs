
//Legacy- only works with SteamVR
/*
using UnityEngine;
using Eppy;
namespace SKStudios.Portals.Demos
{
    public class PortalRaycastGun : TeleportableScript {
#if VR_PORTALS
        public SteamVR_TrackedObject trackedObj;
        public SteamVR_Controller.Device device;
#endif
        // Update is called once per frame
         public override void CustomUpdate() {
            base.CustomUpdate();
            try {
#if VR_PORTALS
                device = SteamVR_Controller.Input((int)trackedObj.index);
#endif
            } catch (System.IndexOutOfRangeException) {
                return;
            }
#if VR_PORTALS
            if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger)) {
#endif
            Ray ray = new Ray(transform.position, transform.forward);
                Tuple<Ray, RaycastHit> hit = PortalUtils.TeleportableRaycast(ray, 100, ~0, QueryTriggerInteraction.Ignore);
                Debug.Log(hit);
            }
#if VR_PORTALS
        }
#endif
    }
}
*/