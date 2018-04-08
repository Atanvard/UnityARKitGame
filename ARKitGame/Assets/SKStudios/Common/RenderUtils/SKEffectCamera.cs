using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Rendering;
using SKStudios.Common.Utils.VR;
using SKStudios.Portals;
using UnityEngine.Rendering;
using SKStudios.ProtectedLibs.Rendering;
using UnityEngine.VR;
using Random = UnityEngine.Random;


namespace SKStudios.Rendering
{
    /// <summary>
    /// Class handling SKS Custom rendering
    /// </summary>
    public class SKEffectCamera : MonoBehaviour
    {

        //Render lib
        private SKSRenderLib _cameraLib;
        private SKSRenderLib CameraLib {
            get {
                if (!_cameraLib)
                    _cameraLib = gameObject.AddComponent<SKSRenderLib>();
                return _cameraLib;
            }
        }
        public Transform RenderingCameraParent { get; set; }

        public Transform OriginTransform { get; private set; }
        public Transform DestinationTransform { get; private set; }

        public Camera PrimaryCam { get; set; }
        public Mesh BlitMesh { get; set; }
        public Camera[] RecursionCams;
        private SKEffectCamera OtherCamera { get; set; }

        

        private RenderData _renderData;
        private RenderData RenderData {
            get {
                if(_renderData == null)
                    _renderData = new RenderData(_renderProperties, null, null,
                        Vector3.zero, Matrix4x4.zero, null, null,
                        Camera.main.pixelRect.size, null, null,
                        null, 0, SKSGlobalRenderSettings.RecursionNumber, true, true);
                return _renderData;
            }
        }
        private RenderProperties _renderProperties = 0;

        private int _recursionNumber = 0;
        public static int CurrentDepth;

        //Keeps track of cameras actively rendering during this frame for image processing
        public static List<Camera> RenderingCameras = new List<Camera>();
        public static Rect LastRect;
        public bool Initialized;

        /// <summary>
        /// Initialize this SKEffectCamera
        /// </summary>
        /// <param name="other">The Sister SKSEffectCamera</param>
        /// <param name="material">The Material to be rendered to</param>
        /// <param name="sourceRenderer">Source renderer</param>
        /// <param name="targetRenderer">Target Renderer</param>
        /// <param name="mesh">Mesh to be rendered to</param>
        public void Initialize(SKEffectCamera other, Material material, MeshRenderer sourceRenderer, MeshRenderer targetRenderer, Mesh mesh, Transform originTransform, Transform destinationTransform) {
            
            //OnDisable();

            OtherCamera = other;
            PrimaryCam = GetComponent<Camera>();
            PrimaryCam.enabled = false;
            CameraLib.Initialize(originTransform, destinationTransform);
            OriginTransform = originTransform;
            DestinationTransform = destinationTransform;
            RenderData.Material = material;
            RenderData.SourceRenderer = sourceRenderer;
            RenderData.TargetRenderer = targetRenderer;
            RenderData.Mesh = mesh;
           

            InstantiateRecursion(SKSGlobalRenderSettings.RecursionNumber);
            Initialized = true;
        }
        
        /// <summary>
        /// Updates the material to be rendered to 
        /// </summary>
        /// <param name="m"></param>
        public void UpdateMaterial(Material m) {
            RenderData.Material = m;
        }

        /// <summary>
        /// Updates the target renderer
        /// </summary>
        /// <param name="targetRenderer"></param>
        public void UpdateTargetRenderer(MeshRenderer targetRenderer) {
             RenderData.TargetRenderer = targetRenderer;
        }

        /// <summary>
        /// Updates the mesh to be rendered to
        /// </summary>
        /// <param name="mesh"></param>
        public void UpdateMesh(Mesh mesh) {
            RenderData.Mesh = mesh;
        }

        /// <summary>
        /// Instantiation is done on awake
        /// </summary>
        void Awake()
        {
            this.enabled = false;
        }

        private void LateUpdate()
        {
            _recursionNumber = 0;
        }

        /// <summary>
        /// Instantiate recursion cameras
        /// </summary>
        /// <param name="count"></param>
        private void InstantiateRecursion(int count)
        {
            count++;
            RecursionCams = new Camera[count + 1];
            string name = this.name + Random.value;
            CameraMarker mainMarker = gameObject.AddComponent<CameraMarker>();
            mainMarker.Initialize(CameraLib, RenderData.SourceRenderer);
            for (int i = 1; i < count; i++)
            {
                Camera cam = InstantiateCamera(i);
                cam.name = name + "Recursor " + i;
            }
            RecursionCams[0] = PrimaryCam;
        }

        /// <summary>
        /// Sets up and returns a recursion camera at index i
        /// </summary>
        /// <param name="index">index</param>
        /// <returns></returns>
        private Camera InstantiateCamera(int index)
        {
            GameObject cameraRecursor = new GameObject();
            cameraRecursor.transform.SetParent(RenderingCameraParent);
            cameraRecursor.transform.localPosition = Vector3.zero;
            cameraRecursor.transform.localRotation = Quaternion.identity;
            CameraMarker marker = cameraRecursor.AddComponent<CameraMarker>();
            marker.Initialize(CameraLib, RenderData.SourceRenderer);
            Camera camera = cameraRecursor.AddComponent<Camera>();

            camera.cullingMask = PrimaryCam.cullingMask;

            camera.renderingPath = PrimaryCam.renderingPath;
            camera.stereoTargetEye = PrimaryCam.stereoTargetEye;
            camera.useOcclusionCulling = PrimaryCam.useOcclusionCulling;
            camera.depthTextureMode = PrimaryCam.depthTextureMode;
            camera.enabled = false;

            camera.ResetProjectionMatrix();
            camera.ResetWorldToCameraMatrix();
            camera.ResetCullingMatrix();
            //camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, invertBuffer);

            RecursionCams[index] = camera;
            return camera;
        }

       

        public void ForceResetRender()
        {
            CurrentDepth = 0;
            _recursionNumber = 0;
        }

        /// <summary>
        /// Renders the view of a given Renderer as if it were through another renderer
        /// </summary>
        /// <param name="camera">The origin camera</param>
        /// <param name="material">The Material to modify</param>
        /// <param name="sourceRenderer">The Source renderer</param>
        /// <param name="targetRenderer">The Target renderer</param>
        /// <param name="mesh">The Mesh of the source Renderer</param> 
        /// <param name="obliquePlane">Will the projection matrix be clipped at the near plane?</param>
        /// <param name="is3d">Is the renderer not being treated as two-dimenstional?</param>
        /// <param name="isMirror">Is the renderer rendering through itself?</param>
        public void RenderIntoMaterial(Camera camera, Material material, MeshRenderer sourceRenderer, MeshRenderer targetRenderer, Mesh mesh, bool obliquePlane = true, bool is3d = false, bool isMirror = false, bool isSSR = false) {
            _renderData = RenderData;
            if (!Initialized) return;
#if !SKS_Portals
            //if (camera.transform.parent == transform.parent)
            //    return;
#endif

            bool firstRender = false;
            Camera renderingCamera = RecursionCams[CurrentDepth];
            Camera currentCamera = camera;

            //Render Placeholder if max depth hit
            if (CurrentDepth > SKSGlobalRenderSettings.RecursionNumber)
            {
                return;
            }

            RenderTexture renderTarget = camera.targetTexture;
            CameraMarker marker = CameraMarker.GetMarker(camera);

            if (marker) {
               // marker.CurrentRenderer = sourceRenderer;
                if (marker.Owner == OtherCamera)
                    return;
            }

            Graphics.SetRenderTarget(renderTarget);

            //Sets up the Render Properties for this render
            RenderProperties renderProps = new RenderProperties();

            //Is this the first time that the IsMirror is being rendered this frame?
            if (camera == Camera.main)
                firstRender = true;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            renderProps |= firstRender ? RenderProperties.FirstRender : 0;
            renderProps |= RenderProperties.Optimize;
            renderProps |= (CurrentDepth < 1) ? (obliquePlane ? RenderProperties.ObliquePlane : 0) : RenderProperties.ObliquePlane;
            renderProps |= isMirror ? RenderProperties.Mirror : 0;
            renderProps |= isSSR ? RenderProperties.IsSSR : 0;
            renderProps |= SKSGlobalRenderSettings.CustomSkybox ? RenderProperties.RipCustomSkybox : 0;
            renderProps |= SKSGlobalRenderSettings.AggressiveRecursionOptimization
                ? RenderProperties.AggressiveOptimization
                : 0;
            if (firstRender)
            {
                renderProps |= SKSGlobalRenderSettings.Inverted ? RenderProperties.InvertedCached : 0;
                renderProps |= SKSGlobalRenderSettings.UvFlip ? RenderProperties.UvFlipCached : 0;
            }

#if SKS_VR
            renderProps |= RenderProperties.VR;
            renderProps |= SKSGlobalRenderSettings.SinglePassStereo ? RenderProperties.SinglePass : 0;
#endif

            _recursionNumber++;


            //Renders the IsMirror itself to the rendertexture
            transform.SetParent(RenderingCameraParent);

            CurrentDepth++;

            renderingCamera.renderingPath = camera.renderingPath;
            renderingCamera.cullingMask = camera.cullingMask;
            renderingCamera.stereoTargetEye = StereoTargetEyeMask.None;
            renderingCamera.enabled = false;
            RenderingCameras.Add(camera);


            //Set up the RenderData for the current frame
            RenderData.Camera = camera;
            RenderData.RenderingCamera = renderingCamera;
            RenderData.CurrentDepth = CurrentDepth;

#if SKS_VR
            //Stereo Rendering
            if (camera.stereoTargetEye == StereoTargetEyeMask.Both)
            {
                RenderingCameraParent.rotation = DestinationTransform.rotation *
                    (Quaternion.Inverse(OriginTransform.rotation) * 
                    (camera.transform.rotation));

                RenderData tempDataLeft = RenderData.Clone();
                RenderData tempDataRight = RenderData.Clone();

                //Left eye


                tempDataLeft.Position = -SKVREyeTracking.EyeOffset(camera);
                tempDataLeft.ProjectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                tempDataLeft.TextureName = "_LeftEyeTexture";
                tempDataLeft.RenderProperties = renderProps;
                tempDataLeft.Eye = false;
                _cameraLib.RenderCamera(tempDataLeft);

                //Right eye
                
                tempDataRight.Position = SKVREyeTracking.EyeOffset(camera);
                tempDataRight.ProjectionMatrix = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                tempDataRight.TextureName = "_RightEyeTexture";
                tempDataRight.RenderProperties = renderProps;
                tempDataRight.Eye = true;
                if (!SKSGlobalRenderSettings.SinglePassStereo)
                {
                    _cameraLib.RenderCamera(tempDataRight);
                }
                else
                {
                    renderingCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                    _cameraLib.RenderCamera(tempDataRight);
                }
            }

            else
            {
                //Non-VR rendering with VR enabled

                RenderData tempData = RenderData.Clone();
                renderProps &= ~RenderProperties.SinglePass;
                renderProps &= ~RenderProperties.VR;

                tempData.RenderProperties = renderProps;
                renderingCamera.transform.rotation = DestinationTransform.rotation *
                    (Quaternion.Inverse(OriginTransform.rotation) * 
                    (camera.transform.rotation));

                tempData.ProjectionMatrix = camera.projectionMatrix;
                tempData.Position = renderingCamera.transform.position;

                if (renderingCamera.stereoTargetEye == StereoTargetEyeMask.Left) {
                    tempData.TextureName = "_LeftEyeTexture";
                    tempData.Eye = false;
                    _cameraLib.RenderCamera(tempData);
                }
                else
                {
                    tempData.TextureName = "_RightEyeTexture";
                    tempData.Eye = true;
                    _cameraLib.RenderCamera(tempData); 
                }
            }




#else
            //Non-stereo rendering
            //RenderData.Position = camera.transform.position;
            RenderData tempData = RenderData.Clone();
      
            tempData.ProjectionMatrix = camera.projectionMatrix;
            tempData.TextureName = "_RightEyeTexture";
            tempData.RenderProperties = renderProps;
            
            tempData.Eye = true;
           
           
            renderingCamera.transform.rotation = DestinationTransform.rotation * (Quaternion.Inverse(OriginTransform.rotation) * (camera.transform.rotation));
            CameraLib.RenderCamera(tempData);
#endif
            SKEffectCamera.CurrentDepth--;

            RenderingCameras.Remove(camera);
            if (RenderingCameras.Count == 0)
            {
                try {
                    _cameraLib.TerminateRender();
                    //SKSRenderLib.ClearUnwinder();
                }
                catch (NullReferenceException e) {
                    Debug.LogWarning("Attempted to render without proper setup");
                }
                
            }
        }



        private void OnDisable()
        {
            //if (_cameraLib)
            //    Destroy(_cameraLib);
            CameraMarker marker;
            if (marker = gameObject.GetComponent<CameraMarker>())
            {
                Destroy(marker);
            }

            foreach(Camera c in RecursionCams)
                if(c && c.gameObject)
                    if(c != this.PrimaryCam)
                        Destroy(c.gameObject);
        }
    }

}

