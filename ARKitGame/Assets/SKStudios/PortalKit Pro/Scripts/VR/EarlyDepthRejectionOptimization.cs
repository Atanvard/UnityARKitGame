using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace SKStudios.Portals {

    /// <summary>
    /// Class that controls early depth fragment rejection for VR rendering. This can save up to 33% rendertime on OpenVR devices.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class EarlyDepthRejectionOptimization : MonoBehaviour {

        private Camera _camera;
        private Camera Camera {
            get {
                if (!_camera)
                    _camera = GetComponent<Camera>();
                return _camera;
            }
        }

        private Material _depthFillMat;
        private Material DepthFillMat {
            get {
                if(!_depthFillMat)
                    _depthFillMat = new Material(Shader.Find("Custom/HiddenArea"));
                return _depthFillMat;
            }
        }

        /// <summary>
        /// The Camera Event to call the cmdbuffer on
        /// </summary>
        private CameraEvent CamEvent {
            get {
                return Camera.actualRenderingPath== RenderingPath.Forward
                    ? CameraEvent.BeforeForwardOpaque
                    : CameraEvent.BeforeGBuffer;
            }
        }

        private CommandBuffer _depthRejBuffer;
        /// <summary>
        /// CommandBuffer that rejects 
        /// </summary>
        private CommandBuffer DepthRejBuffer {
            get {
                if (_depthRejBuffer == null) {
                    _depthRejBuffer = new CommandBuffer();
                    _depthRejBuffer.DrawMesh(PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Quad), Matrix4x4.identity * Matrix4x4.TRS(Vector3.forward * 0.3f, Quaternion.identity, Vector3.one), DepthFillMat);
                    //_depthRejBuffer.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CurrentActive, DepthFillMat);
                }
                return _depthRejBuffer;
            }
        }

        // Use this for initialization
        void OnEnable()
        {
            Camera.opaqueSortMode = OpaqueSortMode.FrontToBack;
            //Camera.AddCommandBuffer(CamEvent, DepthRejBuffer);
        }

        void OnDisable()
        {
            Camera.RemoveCommandBuffer(CamEvent, DepthRejBuffer);
        }
    }
}