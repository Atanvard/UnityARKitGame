using UnityEngine;
using UnityEngine.VR;

namespace SKStudios.Portals
{
    
    public class PKProVRTracking : MonoBehaviour
    {
        public static Vector3 EyeOffset = Vector3.zero;

        public void Update()
        {
            EyeOffset = (Quaternion.Inverse(UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head))
                              * Vector3.Scale(UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightEye) -
                                 UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftEye), transform.lossyScale)) / 2f;
        }

    }


}
