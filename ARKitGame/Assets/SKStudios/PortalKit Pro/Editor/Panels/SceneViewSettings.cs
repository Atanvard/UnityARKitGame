using System;
using System.IO;
using System.Net;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using UnityEditor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
	/// <summary>
	/// Used to setup SKSGlobalRenderSettings. Appears as a semitransparent fogged docked window.
	/// </summary>
	[InitializeOnLoad]
    [ExecuteInEditMode]
    public class SceneViewSettings : EditorWindow {
        private static Rect windowRect;
        private static GUISkin skin;
        private static GUIStyle menuOptionsStyle;
        private static Vector2 size = new Vector2(360, 200);
        private static float bumperSize = 3;
        private static Vector2 scrollPos;
        private static Vector2 defaultDockLocation = Vector2.zero;

        private static bool ImageSettings = false;
        private static bool InteractionSettings = false;
        private static bool EditorSettings = false;

        private static GUIStyle coloredFoldout;
        private static Material material;
        private static double startTime;
        private static float movetime = 4f;
        private static Vector2 defaultSize = new Vector2(360, 200);
        private static Vector2 minimizedSize = new Vector2(200, 200);
        private  static float time = 0;
		private static Camera sceneCamera;
		private static bool compatabilitymode;
        private static bool importantUpdate;
        private static String importantUpdateString = null;
        private static bool hasLoadedWebpage;
        private static SceneViewSettings portalMenu;

        private static Texture texturePlaceholder = new Texture2D(1, 1);
        static SceneViewSettings() {
			EditorApplication.update += Enable;
        }


        /// <summary>
        /// Enable the menu
        /// </summary>
        [MenuItem( "Tools/SK Studios/PortalKit Pro/Dock Portal Settings", priority = 100 )]
        public static void MenuEnable()
        {
            //Undo.RecordObject(SKSGlobalRenderSettings.Instance, "Global Portal Settings");
            SKSGlobalRenderSettings.MenuClosed = false;
			EditorApplication.update += Enable;
        }

        /// <summary>
        /// Setup the menu
        /// </summary>
        public static void Enable()
        {
            Disable();
#if SKS_VR
            SKSGlobalRenderSettings.SinglePassStereo = PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
#endif
            compatabilitymode = (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX);
			EditorApplication.update -= Enable;
            SKSGlobalRenderSettings.Minimized = false;
           // EditorApplication.update += UpdateRect;
            skin = Resources.Load<GUISkin>("UI/PortalEditorGUISkin");
            SceneView.onSceneGUIDelegate += OnScene;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public static void Disable()
        {
            //SceneView.onSceneGUIDelegate -= OnScene;
        }


        private static void NewVersion(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Result.Equals("0")) {
                importantUpdateString = e.Result;
                importantUpdate = true;
            }     
        }

        private static void UpdateRect()
        { if (Event.current.type == EventType.Layout)
            {
                if (!compatabilitymode)
                    defaultDockLocation = new Vector2((Screen.width) - size.x - 10, (Screen.height) - size.y - 19);
                else
                    defaultDockLocation = new Vector2(0, 20);

                windowRect = new Rect(defaultDockLocation.x, defaultDockLocation.y, size.x, size.y);
                time = (float) (EditorApplication.timeSinceStartup - startTime) * movetime;
                if (time > 1)
                    time = 1;



                if (SKSGlobalRenderSettings.Minimized)
                {
                    windowRect.position = Mathfx.Sinerp(defaultDockLocation + new Vector2(0, size.y - 20),
                        defaultDockLocation, 1 - time);
                    size.x = Mathfx.Sinerp(defaultSize.x, minimizedSize.x, time);
                    if (time < 1)
                        SceneView.RepaintAll();
                }
                else
                {
                    windowRect.position = Mathfx.Sinerp(defaultDockLocation + new Vector2(0, size.y - 20),
                        defaultDockLocation, time);
                    size.x = Mathfx.Sinerp(defaultSize.x, minimizedSize.x, 1 - time);
                    SceneView.RepaintAll();
                }
            }

        }
       

        private static void OnScene(SceneView sceneview) {
            if (!texturePlaceholder)
            {
                texturePlaceholder = new Texture2D(1, 1);
            }
            if (!hasLoadedWebpage)
            {
                WebClient client = new WebClient();
                client.DownloadStringCompleted += NewVersion;
                client.DownloadStringAsync(new Uri("http://pastebin.com/raw/SeGjnLg5"));
                hasLoadedWebpage = true;
            }


#if SKS_VR
            SKSGlobalRenderSettings.SinglePassStereo = PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
#endif

            /*
            //Visualize portal connections, and easily find portals that are not connected to anything
            if (SKSGlobalRenderSettings.PortalVisualization)
            {
                foreach (PortalController p in SKSGlobalRenderSettings.PortalControllers)
                {
                    if (!p) continue;
                    p.OnDrawGizmos();
                    p.OnDrawGizmosSelected();
                }
            }*/

          

                if (material == null)
                material = Resources.Load<Material>("UI/blur");

			if (Camera.current.name.Equals ("SceneCamera")) {
				sceneCamera = Camera.current;
				UIBlurController.AddBlurToCamera(sceneCamera);
			}
               

            if (skin.button != null)
                menuOptionsStyle = new GUIStyle(skin.button);
            else
                menuOptionsStyle = new GUIStyle();

            if (EditorStyles.foldout != null)
                coloredFoldout = new GUIStyle(EditorStyles.foldout);
            else
                coloredFoldout = new GUIStyle();
            coloredFoldout.normal.textColor = Color.white;
            coloredFoldout.hover.textColor = Color.white;
            coloredFoldout.active.textColor = Color.white;
            coloredFoldout.focused.textColor = Color.white;
            coloredFoldout.active.textColor = Color.white;
            coloredFoldout.onActive.textColor = Color.white;
            coloredFoldout.onFocused.textColor = Color.white;
            coloredFoldout.onHover.textColor = Color.white;
            coloredFoldout.onNormal.textColor = Color.white;


            menuOptionsStyle.fontSize = 10;
            menuOptionsStyle.fontStyle = FontStyle.Bold;
            menuOptionsStyle.wordWrap = false;
            menuOptionsStyle.clipping = TextClipping.Overflow;
            Handles.BeginGUI();



            GUI.skin = skin;
			//windowRect = new Rect (Screen.width - size.x - 100, Screen.height - size.y - 190, 200, 200);
            if (SetupUtility.projectInitialized)
            {
                if (SKSGlobalRenderSettings.MenuClosed)
                {
                    Handles.EndGUI();
                    return;
                }
                Graphics.DrawTexture(new Rect(windowRect.x, windowRect.y - 16, windowRect.width, windowRect.height),
                    texturePlaceholder, material);
                GUI.Window(0, windowRect, DoMyWindow, "Global Portal Settings");
                GUI.skin = null;
                Handles.EndGUI();
            }
            else
            {
                Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturePlaceholder, material);
                if(GUILayout.Button("Click here to re-open the import dialog"))
                    SettingsWindow.Show();
            }
            
        }

        /// <summary>
        /// Displays the window
        /// </summary>
        /// <param name="windowID">ID of the window</param>
        public static void DoMyWindow(int windowID)
        {
			UpdateRect ();
            Undo.RecordObject(SKSGlobalRenderSettings.Instance, "SKSGlobalRenderSettings");
            //Header controls

				if (GUI.Button (new Rect (3, 3, 15, 15), "X", menuOptionsStyle)) {
					Debug.LogWarning ("Global Portal Settings menu closed. To restore it, go to Tools/SKStudios/PortalKitPro/Dock Portal Settings");
					SKSGlobalRenderSettings.MenuClosed = true;
					Disable ();
				}
			if (!compatabilitymode) {
				if (GUI.Button (new Rect (20, 3, 15, 15), SKSGlobalRenderSettings.Minimized ? "□" : "_", menuOptionsStyle)) {
					SKSGlobalRenderSettings.Minimized = !SKSGlobalRenderSettings.Minimized;
					startTime = EditorApplication.timeSinceStartup;
				}
			} 

            //if (minimized) return;
            if (time >= 1)
                try
                {
                    scrollPos = GUILayout.BeginScrollView(scrollPos, false, false,
                        GUILayout.Width(windowRect.width - 10), GUILayout.Height(windowRect.height - 20));
                }
                catch (System.InvalidCastException) { }

            //Body controls     

            if(importantUpdate)
                EditorGUILayout.HelpBox(
                    importantUpdateString != null ? importantUpdateString : @"A new update marked as 'important' has been released. Please, unless you have serious reason not to, backup your project, remove the asset, and reinstall the new version. Make sure to follow the readme guide for updating if you are using VR.",
                    MessageType.Error);
            if (!GlobalPortalSettings.PlayerTeleportable)
            {
                EditorGUILayout.HelpBox(
                    "No PlayerTeleportable set. Seamless passthrough will not function. Add a PlayerTeleportable script to your teleportable player object.",
                    MessageType.Warning);
            } else {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Player Teleportable", new GUIStyle(){normal = new GUIStyleState(){textColor = Color.white}});
                EditorGUILayout.ObjectField(GlobalPortalSettings.PlayerTeleportable.gameObject, typeof(object), true);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(bumperSize);
            if (ImageSettings = EditorGUILayout.Foldout(ImageSettings, "Image Settings", coloredFoldout))
            {
                EditorGUI.indentLevel = 2;

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
#if SKS_VR
                GUILayout.Label("Single Pass Stereo Rendering: " + SKSGlobalRenderSettings.SinglePassStereo);
#endif
                GUILayout.EndHorizontal();

                
                GUI.enabled = !Application.isPlaying;

                //GUILayout.BeginHorizontal();
                // GUILayout.Space(EditorGUI.indentLevel * 5);
#if SKS_VR
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                GUILayout.Label("Recursion in VR is very expensive. 3 is the typically acceptable max (prefer 0 if possible)");
                GUILayout.EndHorizontal();
#endif

                SKSGlobalRenderSettings.RecursionNumber = EditorGUILayout.IntSlider(new GUIContent("Recursion Number", "The number of times that Portals will draw through each other."), SKSGlobalRenderSettings.RecursionNumber, 0, 10);

               
                if (SKSGlobalRenderSettings.RecursionNumber > 1)
                    EditorGUILayout.HelpBox(
                        "Please be aware that recursion can get very expensive very quickly. Consider making this scale with the Quality setting of your game.",
                        MessageType.Warning);



                GUI.enabled = true;


                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.AggressiveRecursionOptimization = GUILayout.Toggle(SKSGlobalRenderSettings.AggressiveRecursionOptimization,
                    new GUIContent("Enable Aggressive Optimization for Recursion", "Aggressive optimization will halt recursive rendering immediately if the " +
                                                                                   "source portal cannot raycast to the portal it is trying to render. Without" +
                                                                                   "Occlusion Culling (due to lack of Unity Support), this is a lifesaver for " +
                                                                                   "large scenes."));
                GUILayout.EndHorizontal();

                if (SKSGlobalRenderSettings.AggressiveRecursionOptimization)
                    EditorGUILayout.HelpBox(
                        "Enabling this option can save some serious performance, but it is possible for visual bugs to arise due to portals being partially" +
                        "inside walls. If you are seeing black portals while recursing, try turning this option off and see if it helps. If it does, then please" +
                        "make sure that your portals are not inside walls.",
                        MessageType.Warning);

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.AdaptiveQualityCached = GUILayout.Toggle(SKSGlobalRenderSettings.AdaptiveQualityCached,
                    new GUIContent("Enable Adaptive Quality Optimization for Recursion", "Adaptive quality rapidly degrades the quality of recursively rendered portals. This is usually desirable."));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Clipping = GUILayout.Toggle(SKSGlobalRenderSettings.Clipping,
                    new GUIContent("Enable perfect object clipping", "Enable objects clipping as they enter portals. This is usually desirable."));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.ShouldOverrideMask = GUILayout.Toggle(SKSGlobalRenderSettings.ShouldOverrideMask,
                    "Override Masks on all PortalSpawners");
                GUILayout.EndHorizontal();

                if (SKSGlobalRenderSettings.ShouldOverrideMask)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    SKSGlobalRenderSettings.Mask =
                        (Texture2D) EditorGUILayout.ObjectField(SKSGlobalRenderSettings.Mask, typeof(Texture2D), false);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.CustomSkybox = GUILayout.Toggle(SKSGlobalRenderSettings.CustomSkybox,
                    new GUIContent("Enable Skybox Override", "Enable custom skybox rendering. This is needed for skyboxes to not look strange through" +
                                                             "SKSEffectCameras."));
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel = 0;
            }



            GUILayout.Space(bumperSize);
            if (InteractionSettings = EditorGUILayout.Foldout(InteractionSettings, "Interaction Settings", coloredFoldout))
            {
                EditorGUI.indentLevel = 2;

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.PhysicsPassthrough = GUILayout.Toggle(SKSGlobalRenderSettings.PhysicsPassthrough,
                    new GUIContent("Enable Physics Passthrough", "Enable collision with objects on the other side of portals"));
                GUILayout.EndHorizontal();


                if (SKSGlobalRenderSettings.PhysicsPassthrough)
                    EditorGUILayout.HelpBox(
                        "This setting enables interaction with objects on the other side of portals. " +
                        "Objects can pass through portals without it, and it is not needed for most games. " +
                        "In extreme cases, it can cause a slight performance hit.",
                        MessageType.Info);

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.PhysStyleB = GUILayout.Toggle(SKSGlobalRenderSettings.PhysStyleB,
                    new GUIContent("Enable Physics Model B (More Accurate)", "Physics Model B maintains relative momentum between portals. This may or may not be desirable when the portals move."));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.NonScaledRenderers = GUILayout.Toggle(SKSGlobalRenderSettings.NonScaledRenderers,
                    new GUIContent("Disable Portal scaling", "Disable portal scaling. This should be enabled if portals are never used to change object's size."));
                GUILayout.EndHorizontal();

                EditorGUI.indentLevel = 0;
            }

            GUILayout.Space(bumperSize);
            if (EditorSettings =
                EditorGUILayout.Foldout(EditorSettings, "Editor Settings", coloredFoldout))
            {
                EditorGUI.indentLevel = 2;

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Visualization = GUILayout.Toggle(SKSGlobalRenderSettings.Visualization,
                    new GUIContent("Visualize Portal Connections", "Visualize all portal connections in the scene"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Gizmos = GUILayout.Toggle(SKSGlobalRenderSettings.Gizmos, new GUIContent("Draw Portal Gizmos", "Draw Portal Gizmos when selected in the Editor, and when all portals are Visualized."));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 10);
                SKSGlobalRenderSettings.Preview = GUILayout.Toggle(SKSGlobalRenderSettings.Preview, new GUIContent("Draw Portal Previews (experimental)", "Draw Portal Previews when selected in the Editor. Experimental, and only works with shallow viewing angles."));
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Something doesn't look right!/I'm getting errors!");
			
			SKSGlobalRenderSettings.UvFlip = GUILayout.Toggle(SKSGlobalRenderSettings.UvFlip,
				"My portals are upside down!");

            GUILayout.Label("Troubleshooting:");

            string assetName = "PortalKit Pro";
            string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            if (path == null)
                return;
            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            string root = path.Substring(0, path.LastIndexOf(assetName) + (assetName.Length + 1));
            string PDFPath = root + "README.pdf";

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Click here to open the manual")) {
                Application.OpenURL(PDFPath);
            }
            if (GUILayout.Button("Setup")) {
               SettingsWindow.Show();
            }
            GUILayout.EndHorizontal();
            if (time >= 1)
                GUILayout.EndScrollView();
        }
    }
}
