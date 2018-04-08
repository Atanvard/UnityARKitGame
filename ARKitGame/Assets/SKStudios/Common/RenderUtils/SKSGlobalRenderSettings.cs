using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using SKStudios.Portals;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif
#if SKS_VR
using UnityEngine.VR;
#endif

namespace SKStudios.Rendering
{

    /// <summary>
    /// Stores settings for Mirror that are used globally
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    [CreateAssetMenu(menuName = "ScriptableObjects/SKGlobalRenderSettings")]
    public class SKSGlobalRenderSettings : ScriptableObject
    {
        [SerializeField] [HideInInspector] private bool _shouldOverrideMaskCached;
        [SerializeField] [HideInInspector] private Texture2D _maskCached;
        [SerializeField] [HideInInspector] private bool _physicsPassthroughCached;
        [SerializeField] [HideInInspector] private int _importedCached;
        [SerializeField] [HideInInspector] private int _recursionNumberCached;
        [SerializeField] [HideInInspector] private bool _invertedCached;
        [SerializeField] [HideInInspector] private bool _uvFlipCached;
        [SerializeField] [HideInInspector] private bool _clippingCached;
        [SerializeField] [HideInInspector] private bool _physStyleBCached;
        [SerializeField] [HideInInspector] private bool _minimizedCached;
        [SerializeField] [HideInInspector] private bool _closedCached;
        [SerializeField] [HideInInspector] private bool _MirrorVisualizationCached;
        [SerializeField] [HideInInspector] private bool _MirrorGizmosCached;
        [SerializeField] [HideInInspector] private bool _MirrorPreviewCached;
        [SerializeField] [HideInInspector] private bool _nonscaledMirrorsCached;
        [SerializeField] [HideInInspector] private bool _adaptiveQualityCached;
        [SerializeField] [HideInInspector] private bool _aggressiveRecursionOptimizationCached;
        [SerializeField] [HideInInspector] private bool _customSkyboxCached;
        [SerializeField] [HideInInspector] private float _ipdCached = 0.00068f;

#if SKS_VR
        [SerializeField] [HideInInspector] public bool SinglePassStereoCached;
#endif
        private static SKSGlobalRenderSettings _instance = null;
        /// <summary>
        /// Get the singleton instance of this object
        /// </summary>
        public static SKSGlobalRenderSettings Instance {
            get
            {
                
                bool loaded = false;
                if (!_instance)
                {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                    loaded = true;
                    _instance = Resources.Load<SKSGlobalRenderSettings>("SK Global Render Settings");

                   if (_instance)
                        _instance.Initialize();
                }

                if (!_instance) {
                    loaded = false;
                }
              
                if (loaded)
                {
#if UNITY_EDITOR
                    EditorApplication.playmodeStateChanged -= RecompileCleanup;
                    EditorApplication.playmodeStateChanged += RecompileCleanup;
#endif
                }
                return _instance;
            }
        }

        /// <summary>
        /// Is Physic style B enabled?
        /// </summary>
        public static bool PhysStyleB {
            get { return Instance._physStyleBCached; }
            set {
                Instance._physStyleBCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }


        /// <summary>
        /// Should the mask for all Mirrors be overridden?
        /// </summary>
        public static bool ShouldOverrideMask {
            get { return Instance._shouldOverrideMaskCached; }
            set {
                Instance._shouldOverrideMaskCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Texture2D with which the masks on all Mirrors are overwritten
        /// </summary>
        public static Texture2D Mask {
            get { return Instance._maskCached; }
            set {
                Instance._maskCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is the UV inverted? Changing this value changes the global "_InvertOverride" value.
        /// </summary>
        public static bool Inverted {
            get { return Instance._invertedCached; }
            set {
                Shader.SetGlobalFloat("_InvertOverride", value ? 1 : 0);
                Instance._invertedCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is the UV flipped? Changing this value changes the global "_YFlipOverride" value.
        /// </summary>
        public static bool UvFlip {
            get { return Instance._uvFlipCached; }
            set {
                Shader.SetGlobalFloat("_YFlipOverride", value ? 1 : 0);
                Instance._uvFlipCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is Object clipping enabled to make objects disappear as they enter Mirrors?
        /// </summary>
        public static bool Clipping {
            get { return Instance._clippingCached; }
            set {
                Shader.SetGlobalFloat("_ClipOverride", value ? 0 : 1);
                Instance._clippingCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Are passthrough physics simulated?
        /// </summary>
        public static bool PhysicsPassthrough {
            get { return Instance._physicsPassthroughCached; }
            set {
                Instance._physicsPassthroughCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }

        }

        /// <summary>
        /// Has the asset been imported?
        /// </summary>
        public static int Imported {
            get { return Instance._importedCached; }
            set {
                Instance._importedCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// how many times should the Mirrors recurse while rendering?
        /// </summary>
        public static int RecursionNumber {
            get { return Instance._recursionNumberCached; }
            set {
                Instance._recursionNumberCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is the MirrorMenu minimized?
        /// </summary>
        public static bool Minimized {
            get { return Instance._minimizedCached; }
            set {
                Instance._minimizedCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is the MirrorMenu minimized?
        /// </summary>
        public static bool NonScaledRenderers {
            get { return Instance._nonscaledMirrorsCached; }
            set {
                Instance._nonscaledMirrorsCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is the MirrorMenu closed?
        /// </summary>
        public static bool AggressiveRecursionOptimization {
            get { return Instance._aggressiveRecursionOptimizationCached; }
            set {
                Instance._aggressiveRecursionOptimizationCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Is the MirrorMenu closed?
        /// </summary>
        public static bool MenuClosed {
            get { return Instance._closedCached; }
            set {
                Instance._closedCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Enable visualization of all Mirrors
        /// </summary>
        public static bool Visualization {
            get { return Instance._MirrorVisualizationCached; }
            set {
                Instance._MirrorVisualizationCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }


        /// <summary>
        /// Enable visualization of all Mirrors
        /// </summary>
        public static bool CustomSkybox {
            get { return Instance._customSkyboxCached; }
            set {
                Instance._customSkyboxCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        /// <summary>
        /// Disable showing Mirror gizmos even while selected
        /// </summary>
        public static bool Gizmos {
            get { return Instance._MirrorGizmosCached; }
            set {
                Instance._MirrorGizmosCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }


        /// <summary>
        /// Disable showing Mirror gizmos even while selected
        /// </summary>
        public static bool Preview {
            get { return Instance._MirrorPreviewCached; }
            set {
                Instance._MirrorPreviewCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }


        /// <summary>
        /// Is the MirrorMenu minimized?
        /// </summary>
        public static bool AdaptiveQualityCached {
            get { return Instance._adaptiveQualityCached; }
            set {
                Instance._adaptiveQualityCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }


        public static bool LightPassthrough;

#if SKS_VR
/// <summary>
/// Is SinglePassStereo rendering enabled?
/// </summary> 
        public static bool SinglePassStereo{
            get { return Instance.SinglePassStereoCached; }
            set { Instance.SinglePassStereoCached = value; 
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
}
        }

/// <summary>
/// IPD of eyes (temporary while SteamVR fixes itself)
/// </summary>
        public static float IPD
        {
            get { return Instance._ipdCached; }
            set { Instance._ipdCached = value; 
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
}
        }
#endif
        /// <summary>
        /// Initialize
        /// </summary>
        public void OnEnable()
        {
            Initialize();
        }
        // Use this for initialization
        private void Initialize()
        {

#if SKS_VR
            SinglePassStereo = SinglePassStereoCached;
            //SinglePassStereo = SinglePassStereoCached;
            //Debug.Log("Single Pass Stereo Mode: " + SinglePassStereo);
            Shader.SetGlobalFloat("_VR", 1);
#else
            Shader.SetGlobalFloat("_VR", 0);


#endif
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.Append("SKSRenderSettings:{");
            builder.Append(_shouldOverrideMaskCached ? 0 : 1);
            builder.Append(_maskCached ? 0 : 1);
            builder.Append(_physicsPassthroughCached ? 0 : 1);
            builder.Append(_invertedCached ? 0 : 1);
            builder.Append(_uvFlipCached ? 0 : 1);
            builder.Append(_clippingCached ? 0 : 1);
            builder.Append(_physStyleBCached ? 0 : 1);
            builder.Append(_minimizedCached ? 0 : 1);
            builder.Append(_closedCached ? 0 : 1);
            builder.Append(_MirrorVisualizationCached ? 0 : 1);
            builder.Append(_MirrorGizmosCached ? 0 : 1);
            builder.Append(_MirrorPreviewCached ? 0 : 1);
            builder.Append(_nonscaledMirrorsCached ? 0 : 1);
            builder.Append(_adaptiveQualityCached ? 0 : 1);
            builder.Append(_aggressiveRecursionOptimizationCached ? 0 : 1);
            builder.Append(_customSkyboxCached ? 0 : 1).Append('|');
            builder.Append(_importedCached).Append('|');
            builder.Append(_recursionNumberCached);
            builder.Append("}");
            return builder.ToString();
        }

#if UNITY_EDITOR
        [DidReloadScripts]
#endif
        private static void RecompileCleanup()
        {
#if UNITY_EDITOR
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains("SKSEditorTemp")) return;
                GameObject[] tempObjects = GameObject.FindGameObjectsWithTag("SKSEditorTemp");
                for (int i = 0; i < tempObjects.Length; i++)
                {
                    DestroyImmediate(tempObjects[i], true);
                }
            
#endif
        }
    }
}
