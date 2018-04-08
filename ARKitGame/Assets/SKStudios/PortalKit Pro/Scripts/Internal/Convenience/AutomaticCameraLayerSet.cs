using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Portals {
    [RequireComponent(typeof(Camera))]

    public class AutomaticCameraLayerSet : MonoBehaviour
    {
        public List<string> ExcludedLayers;
        private new Camera camera;

        // Use this for initialization
        void Start() {
            camera = gameObject.GetComponent<Camera>();
            int mask = camera.cullingMask;
            foreach (string l in ExcludedLayers)
            {
                mask &= ~(1 << LayerMask.NameToLayer(l));
            }
            camera.cullingMask = mask;
        }
    }
}