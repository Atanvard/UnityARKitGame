using System.Collections;
using System.Collections.Generic;
using SKStudios.Portals;
using UnityEditor;
using UnityEngine;
/**
namespace SKStudios.Portals.Editor
{
    [CustomEditor(typeof(PlayerTeleportable))]
    public class PlayerTeleportableEditor : UnityEditor.Editor {

        public void OnEnable()
        {
            PlayerTeleportable playerTeleportable = (PlayerTeleportable) target;
            Undo.RecordObject(GlobalPortalSettings.Instance, "GlobalPortalSettings");
            GlobalPortalSettings.PlayerTeleportable = playerTeleportable.GetComponent<Teleportable>();
        }

        public void OnDestroy()
        {
            Undo.RecordObject(GlobalPortalSettings.Instance, "GlobalPortalSettings");
            GlobalPortalSettings.PlayerTeleportable = null;
        }
    }
}
*/