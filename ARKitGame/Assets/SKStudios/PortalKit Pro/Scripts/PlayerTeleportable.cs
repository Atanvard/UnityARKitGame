using System.Collections;
using System.Collections.Generic;
using SKStudios.Rendering;
using UnityEngine;
namespace SKStudios.Portals {
    /// <summary>
    /// Script used to mark which Teleportable is the Player's, for the purpose of seamless camera passthrough.
    /// </summary>
    [RequireComponent(typeof(Teleportable))]
    [ExecuteInEditMode]
    public class PlayerTeleportable : MonoBehaviour {

        void Awake()
        {
            Update();
        }

        void Update()
        {
            GlobalPortalSettings.PlayerTeleportable = this.GetComponent<Teleportable>();
        }

        private void OnDestroy()
        {
            GlobalPortalSettings.PlayerTeleportable = null;
        }
    }
}
