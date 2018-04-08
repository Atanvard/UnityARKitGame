using System.Collections;
using System.Collections.Generic;
using SKStudios.Portals;
using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    /// This class fixes issues with newer versions of VRTK and SteamVR where teleportation was broken.
    /// </summary>
    public class SDKPursuer : TeleportableScript
    {
        public GameObject SDK;


        public override void Initialize(Teleportable t) {
            
        }
        public override void CustomUpdate()
        {
            //We don't want to use this at all
        }

        public override void LeavePortal() {
            
        }

        public override void Teleport() {
            
        }

        public override void OnTeleport() {
           
        }

        void Start()
        {
            teleportScriptIndependantly = false;
        }
        void LateUpdate()
        {
            Vector3 position = SDK.transform.position;
            Quaternion rotation = SDK.transform.rotation;
            Vector3 diffVector = position - transform.position;
            Quaternion diffQuat = Quaternion.Inverse(rotation) * transform.rotation;
            transform.position = position;
            transform.rotation = rotation;
            SDK.transform.position = SDK.transform.position - diffVector;
            SDK.transform.rotation = rotation;
        }
    }

}
