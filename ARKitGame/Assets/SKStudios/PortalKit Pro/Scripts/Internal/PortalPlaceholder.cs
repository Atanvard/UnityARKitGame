using System.Collections;
using System.Collections.Generic;
using SKStudios.Portals;
using SKStudios.ProtectedLibs.Rendering;
using UnityEngine;

public class PortalPlaceholder : MonoBehaviour
{

    private Material _material;
    private bool _placeholderRendered;
    private Camera _camera;
    private SKSRenderLib _cameraLib;
    private Portal _portal;
    private RenderTexture placeholderTex;
    public void Instantiate(Portal portal)
    {

        _portal = portal;
        _cameraLib = _portal.PortalCamera.GetComponent<SKSRenderLib>();

        GameObject cameraParent = new GameObject();

        cameraParent.transform.parent = portal.PortalCamera.RenderingCameraParent;
        cameraParent.transform.localPosition = Vector3.zero;
        cameraParent.transform.localRotation = Quaternion.identity;

        _camera = cameraParent.AddComponent<Camera>();
        _camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Portal"));
        _camera.cullingMask &= ~(1 << LayerMask.NameToLayer("PortalPlaceholder"));
        _camera.name = "Portal Placeholder Camera";
        _camera.enabled = false;

        placeholderTex = new RenderTexture(100, 100, 24, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Default);
    }
    private void Awake()
    {
        _material = gameObject.GetComponent<Renderer>().sharedMaterial;
    }

    private void Update()
    {
        _placeholderRendered = false;
    }

    private void OnWillRenderObject()
    {
        /*
#if UNITY_EDITOR
        if (Camera.current.name.Equals("SceneCamera")) return;
#endif
        if (_placeholderRendered) return;

        RenderTexture.active = placeholderTex;
        GL.Clear(true, true, Color.blue);
        
        //_camera.transform.localPosition = Vector3.zero;
        float t = 0;
        _camera.transform.rotation  = _portal.Destination.rotation * (Quaternion.Inverse(_portal.transform.rotation) * (Camera.current.transform.rotation));
        _cameraLib.RenderCamera(new RenderState(), _camera, Camera.current, Camera.current.transform.position, _portal.Origin.localScale, Camera.current.nonJitteredProjectionMatrix, "_RefTexture", _material, placeholderTex, _portal.MeshRenderer, _portal.TargetPortal.MeshRenderer, _portal.MeshFilter.mesh, 0, 5, true);
        _material.SetTexture("_RefTexture", placeholderTex);
        _placeholderRendered = true;
        return;*/
    }
}
