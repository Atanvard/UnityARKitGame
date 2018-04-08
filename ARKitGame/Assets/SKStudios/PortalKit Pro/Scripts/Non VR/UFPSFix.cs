/*
using SKStudios.Portals;
using UnityEngine;
[RequireComponent(typeof(vp_CharacterController))]
[RequireComponent(typeof(CharacterController))]
public class UFPSFix :TeleportableScript {
    private new vp_FPCamera camera;
    private vp_CharacterController controller;
    private CharacterController pController;

    private void Start() {
        this.teleportScriptIndependantly = false;
        camera = transform.GetComponentInChildren<vp_FPCamera>();
        controller = GetComponent<vp_CharacterController>();
        pController = GetComponent<CharacterController>();
    }

    public override void OnTeleport() {
        base.OnTeleport();
        Quaternion newRotation = currentPortal.TargetPortal.PortalCamera.CameraForPortal.transform.rotation;
        camera.SetRotation(new Vector2(newRotation.eulerAngles.x, newRotation.eulerAngles.y), true);
        transform.rotation = currentPortal.Origin.rotation * Quaternion.Inverse(currentPortal.TargetPortal.Origin.rotation) * transform.rotation;
    }
}
*/