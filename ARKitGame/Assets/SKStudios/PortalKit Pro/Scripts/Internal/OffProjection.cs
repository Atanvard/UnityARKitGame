using UnityEngine;
using System.Collections;


namespace SKStudios.Portals
{
    [ExecuteInEditMode]
    public class OffProjection : MonoBehaviour
    {
        public float left = -0.2F;
        public float right = 0.2F;
        public float top = 0.2F;
        public float bottom = -0.2F;
        
        private Camera _camera;
        private Camera Camera
        {
            get
            {
                if (!_camera)
                    _camera = GetComponent<Camera>();
                return _camera;
            }
        }
        void LateUpdate()
        {
            Camera cam = Camera;
            float width = (right - left);
            float height = (top - bottom);
            cam.aspect = width / height;
            float fov = (Mathf.Atan(top / cam.nearClipPlane) - Mathf.Atan(bottom / cam.nearClipPlane)) * 57.2957795f;
            cam.fieldOfView = 90;

            Matrix4x4 m = PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
            cam.projectionMatrix = m;
        }

        static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
        {
            float x = 2.0F * near / (right - left);
            float y = 2.0F * near / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(far + near) / (far - near);
            float d = -(2.0F * far * near) / (far - near);
            float e = -1.0F;
            Matrix4x4 m = new Matrix4x4();
            m[0, 0] = x;
            m[0, 1] = 0;
            m[0, 2] = a;
            m[0, 3] = 0;
            m[1, 0] = 0;
            m[1, 1] = y;
            m[1, 2] = b;
            m[1, 3] = 0;
            m[2, 0] = 0;
            m[2, 1] = 0;
            m[2, 2] = c;
            m[2, 3] = d;
            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = e;
            m[3, 3] = 0;
            return m;
        }
    }
}