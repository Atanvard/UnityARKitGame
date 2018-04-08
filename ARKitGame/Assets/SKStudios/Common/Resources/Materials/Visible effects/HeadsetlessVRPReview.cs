using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Common.Demos {
    public class HeadsetlessVRPReview : MonoBehaviour
    {
        public Material mat;
        private Camera cam;

        private void Start()
        {
            cam = GetComponent<Camera>();
        }

        bool eye = false;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Camera test = Camera.current;
            eye = !eye;
            if(eye)
                mat.SetTexture("_LeftEyeTexture", src);
            else
                mat.SetTexture("_RightEyeTexture", src);
            Graphics.Blit(src, dest, mat);
        }
    }

}
