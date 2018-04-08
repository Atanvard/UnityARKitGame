using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SKStudios.Portals;
using SKStudios.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SKStudios.Portals
{

    [CreateAssetMenu(menuName = "ScriptableObjects/GlobalPortalSettings")]
    public class GlobalPortalSettings : ScriptableObject {
        public const int MAJOR_VERSION = 6;
        public const int MINOR_VERSION = 0;
        public const int PATCH_VERSION = 0;
        private static GlobalPortalSettings _instance = null;

        /// <summary>
        /// Returns the singleton instance of the Global Portal Settings
        /// </summary>
        public static GlobalPortalSettings Instance {
            get {

                if (!_instance)
                {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif

                    _instance = Resources.Load<GlobalPortalSettings>("Global Portal Settings");
                }

                return _instance;
            }
        }

        [SerializeField] [HideInInspector] private Teleportable _playerTeleportableCached = null;

        /// <summary>
        /// The Player Teleportable
        /// </summary>
        public static Teleportable PlayerTeleportable {
            get { return GlobalPortalSettings.Instance._playerTeleportableCached; }
            set {
                Instance._playerTeleportableCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        [SerializeField] [HideInInspector] private Double _timeInstalled = 0;
        public static Double TimeInstalled {
            get { return GlobalPortalSettings.Instance._timeInstalled; }
            set {
                Instance._timeInstalled = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        [SerializeField] [HideInInspector] private bool _nagged = false;
        public static bool Nagged {
            get { return GlobalPortalSettings.Instance._nagged; }
            set {
                Instance._nagged= value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        public override string ToString() {
            try {
                StringBuilder builder = new StringBuilder();
                TimeSpan duration = new TimeSpan((long) (System.DateTime.UtcNow.Ticks - TimeInstalled));
                double minutes = duration.TotalMinutes;
                builder.Append("SKSRenderSettings:{");
                builder.Append(minutes).Append('|');
                builder.Append(_nagged);
                builder.Append("}");
                return builder.ToString();
            }
            catch (Exception e) {
                return String.Format("SKSRenderSettings:{Error {0}}", e.Message);
            }
        }
    }
}

