#define PKPRO_SHOW_DEBUG

using System.IO;
using SKStudios.Common.Utils.SafeRemoveComponent;
using SKStudios.Rendering.Common.Utils;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Reflection;
using System.Text;
using SKStudios.Common.Utils;
using SKStudios.Rendering;

namespace SKStudios.Portals.Editor {
	public enum ProjectImportStatus { Unknown, Uninitialized, Initialized }
	public enum ProjectMode { None, Default, VR }

	public static class SetupUtility {

		public const string vrSymbol      = "SKS_VR";
		public const string projectSymbol = "SKS_Portals";


		private const string _ignoreSetup              = "pkpro_dont_show_setup";
		private const string _importingVRTK            = "skspro_importing_vrtk";
		private const string _performingSetup          = "skspro_performing_import";
		private const string _performingFirstTimeSetup = "skspro_performing_first_time_setup";

		static ProjectMode _projectMode;
		static ProjectImportStatus _isProjectInitialized;

		public static bool ignoringInitialSetup {
			get { return EditorPrefs.GetBool( _ignoreSetup ); }
			set { EditorPrefs.SetBool( _ignoreSetup, value ); }
		}

		public static bool performingSetup {
			get { return EditorPrefs.GetBool( _performingSetup ); }
			set { EditorPrefs.SetBool( _performingSetup, value ); }
		}

		public static bool performingFirstRunSetup {
			get { return EditorPrefs.GetBool( _performingFirstTimeSetup ); }
			set { EditorPrefs.SetBool( _performingFirstTimeSetup, value ); }
		}

		// This is an int instead of a bool to account for the two-step reload process due to VRTK adding its own symbol defines and triggering an extra reload after import.
		public static int importingVRTK {
			get { return EditorPrefs.GetInt( _importingVRTK ); }
			set { EditorPrefs.SetInt( _importingVRTK, value ); }
		}

		// This doesn't account for a situation in which the VRTK scripting symbols are defined but the VRTK package isn't present.
		public static bool VRTKIsMaybeInstalled {
			get { return IsScriptingSymbolDefined( "VRTK_VERSION_" ); }
		}

		public static bool projectInitialized {
			get { return importStatus == ProjectImportStatus.Initialized; }
		}

		// We can safely cache this because any changes to the scripting symbols results in a reload of the project.
		public static ProjectImportStatus importStatus {
			get {
				if( _isProjectInitialized != ProjectImportStatus.Unknown ) return _isProjectInitialized;

				if( IsScriptingSymbolDefined( projectSymbol ) ) {
					_isProjectInitialized = ProjectImportStatus.Initialized;
				} else {
					_isProjectInitialized = ProjectImportStatus.Uninitialized;
				}

				return _isProjectInitialized;
			}
		}

		// We can safely cache this because any changes to the scripting symbols results in a reload of the project.
		public static ProjectMode projectMode {
			get {
				if( _projectMode != ProjectMode.None ) return _projectMode;

				if( importStatus == ProjectImportStatus.Initialized ) {
					if( IsScriptingSymbolDefined( vrSymbol ) ) {
						_projectMode = ProjectMode.VR;
					} else {
						_projectMode = ProjectMode.Default;
					}
				} else {
					_projectMode = ProjectMode.None;
				}

				return _projectMode;
			}
		}


		static SerializedObject _tagManager;
		public static SerializedObject unityTagManager {
			get {
				if( _tagManager != null ) return _tagManager;

				var asset = AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/TagManager.asset" );
				if( asset != null && asset.Length > 0 ) {
					return _tagManager = new SerializedObject( asset[ 0 ] );
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Makes sure the project has been properly configured, and handles properly showing the setup screen on first install.
		/// </summary>
		[DidReloadScripts]
		static void OnProjectReload () {
			// don't bother the user if they're playing
			if( EditorApplication.isPlayingOrWillChangePlaymode ) {
				return;
			}

			//Logger.Log( "Reload Fired" );

			if( projectInitialized ) {
			    if (!GlobalPortalSettings.Nagged &&
			        System.DateTime.UtcNow.Ticks - GlobalPortalSettings.TimeInstalled > 5.888e+12D) {
			        SettingsWindow.Show(true, 6);
			        GlobalPortalSettings.Nagged = true;
			    }
                if ( projectMode == ProjectMode.VR ) {
					//Logger.Log( "active project mode is VR" );
					// check to see if we're currently trying to load vrtk
					var importingStep = importingVRTK;
					if( importingStep > 0 ) {
						if( importingStep == 2 ) {
							// has vrtk been imported successfully?
							if( VRTKIsMaybeInstalled ) {
								//Logger.Log( "vrtk import complete" );
								
								// first reload of vrtk, wait out vrtk adding its own compile symbols
								importingVRTK = 1;
							} else {
								// vrtk probably didn't install/run correctly... maybe detect improper import later
								importingVRTK = 0;
								CheckImportFlags();
							}
						} else {
							//Logger.Log( "vrtk reimport" );
							// second reload of vrtk, check the import flags
							importingVRTK = 0;
							CheckImportFlags();
						}
					} else {
						// we're not importing vrtk
						CheckImportFlags();
					}
				} else {
					//Logger.Log( "active project mode is non-VR" );
					CheckImportFlags();
				}
			} else {
				// project is uninitialized, show the setup window
				if( !ignoringInitialSetup ) {
					EditorUtility.ClearProgressBar();
					SettingsWindow.Show();
				}
			}
		}

		static void CheckImportFlags () {
			if( performingSetup ) {
				performingSetup = false;
				//Logger.Log( "performing import, now is when we'd want to detect dependencies." );
				Dependencies.RescanDictionary();


				if( performingFirstRunSetup ) {
					performingFirstRunSetup = false;

					//Logger.Log( "first time setup is complete, show window" );
					SettingsWindow.Show( true, 0 );
				} else {
					//Logger.Log( "import complete, show window" );
					SettingsWindow.Show( true, 0 );
				}

				EditorUtility.ClearProgressBar();
			} else {
				//  all done, no need to pop up the window automatically!

				//Logger.Log( "no activity, hide window" );
				SettingsWindow.Hide();
			}
		}




		public static void ApplyVR ( bool includeVRTK ) {
			EditorUtility.DisplayProgressBar( "Applying VR Presets...", "", 1f );

			if( includeVRTK ) {
				Logger.Log( "Importing VRTK" );
				EditorPrefs.SetInt( _importingVRTK, 2 );

				var vrtkPath = Directory.GetFiles( "Assets", "vrtk.unitypackage", SearchOption.AllDirectories );
				if( vrtkPath.Length > 0 ) {
					AssetDatabase.ImportPackage( vrtkPath[ 0 ], false );
					//AssetDatabase.Refresh();
				} else {
					// failed to find vrtk package
				}

			   
            } else {
				// if the user decides to set VR mode without importing VRTK the compile flags can cause errors/
				// detect those errors and let the user know the problem.
				if( !VRTKIsMaybeInstalled ) {
					ConsoleCallbackHandler.AddCallback( HandleVRTKImportError, LogType.Error, "CS0246" );
				}
			}

		    var vrtkSupportPath = Directory.GetFiles("Assets", "vrtk_support.unitypackage", SearchOption.AllDirectories);
		    if (vrtkSupportPath.Length > 0)
		    {
		        AssetDatabase.ImportPackage(vrtkSupportPath[0], false);
		        AssetDatabase.Refresh();
		    }
		    else
		    {
		        // failed to find vrtk_support package
		    }

            EditorPrefs.SetBool( _performingSetup, true );
			if( !projectInitialized ) {
				PerformFirstTimeSetup();
			}

			SKSEditorUtil.AddDefine( vrSymbol );
		}

		private static void HandleVRTKImportError () {
			ConsoleCallbackHandler.RemoveCallback( LogType.Error, "CS0246" );
			EditorUtility.ClearProgressBar();

			EditorUtility.DisplayDialog( "No VRTK Installation Found", "No suitable VRTK installation found. VR Portal scripts will not function and may throw errors if VRTK is not present.\n\nIf you have no existing VRTK installation, you should check the 'Also import VRTK' box before applying VR mode.", "Okay" );

			SKSEditorUtil.RemoveDefine( vrSymbol );
			performingSetup = false;

			if( performingFirstRunSetup ) {
				SKSEditorUtil.RemoveDefine( projectSymbol );
				performingFirstRunSetup = false;
			}
		}

		public static void ApplyDefault () {
			EditorUtility.DisplayProgressBar( "Applying Default Presets...", "", 1f );

			EditorPrefs.SetBool( _performingSetup, true );
			if( !projectInitialized ) {
				PerformFirstTimeSetup();
			}

			SKSEditorUtil.RemoveDefine( vrSymbol );
		}

		static void PerformFirstTimeSetup () {
			Logger.Log( "performing first-time setup" );

			SKSEditorUtil.AddDefine( projectSymbol );
			EditorPrefs.SetBool( _performingFirstTimeSetup, true );
			EditorPrefs.DeleteKey( _ignoreSetup );

			CreateLayer( "Player" );
			CreateLayer( "Portal" );
			CreateLayer( "PortalPlaceholder" );
			CreateLayer( "PortalOnly" );
			CreateLayer( "RenderExclude" );
			CreateTag( "PhysicsPassthroughDuplicate" );
			CreateTag( "SKSEditorTemp" );

			ReassignPrefabLayers();
		}

		/// Returns true if the layer was successfully created or already exists
		public static bool CreateLayer ( string layerName ) {
			SerializedProperty layers = unityTagManager.FindProperty( "layers" );

			for( int i = 8; i < layers.arraySize; i++ ) {
				SerializedProperty layer = layers.GetArrayElementAtIndex( i );
				if( layer.stringValue == layerName ) {
					// target layer already exists
					return true;
				}
			}

			for( int j = 8; j < layers.arraySize; j++ ) {
				SerializedProperty layer = layers.GetArrayElementAtIndex( j );
				if( string.IsNullOrEmpty( layer.stringValue ) ) {
					layer.stringValue = layerName;
					unityTagManager.ApplyModifiedProperties();
					return true;
				}
			}

			return false;
		}

		/// Returns true if the tag was successfully created or already exists.
		public static bool CreateTag ( string tagName ) {
			SerializedProperty tags = unityTagManager.FindProperty( "tags" );

			for( int i = 0; i < tags.arraySize; ++i ) {
				var tag = tags.GetArrayElementAtIndex( i );
				if( tag.stringValue == tagName ) {
					// target tag already exists
					return true;
				}
			}

			tags.arraySize += 1;
			tags.GetArrayElementAtIndex( tags.arraySize - 1 ).stringValue = tagName;
			unityTagManager.ApplyModifiedProperties();

			return true;
		}

		/// Reshuffles layers created during startup process.
		public static void ReassignPrefabLayers () {
			GameObject[] objects = Resources.LoadAll<GameObject>( "" );
			foreach( GameObject go in objects ) {
				if( TagDatabase.tags.ContainsKey( go.name ) ) {
					if( TagDatabase.tags[ go.name ].Count == 0 ) continue;

					foreach( Transform transform in go.transform.GetComponentsInChildren<Transform>() ) {
						if( TagDatabase.tags[ go.name ].ContainsKey( transform.gameObject.name ) ) {
							transform.gameObject.layer = LayerMask.NameToLayer( TagDatabase.tags[ go.name ][ transform.gameObject.name ] );
						}
					}
				}

				EditorUtility.SetDirty( go );
			}
			AssetDatabase.Refresh();
			// unload unused assets?
		}

		// Allows for fuzzy matching of scripting symbols, eg. a query of "SOME_" will return true for "SOME_SYMBOL"
		public static bool IsScriptingSymbolDefined ( string symbolFragment ) {
			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			var targetGroup = GetGroupFromBuildTarget( buildTarget );
			var scriptingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( targetGroup );

			return scriptingSymbols.IndexOf( symbolFragment ) > -1;
		}

		static BuildTargetGroup GetGroupFromBuildTarget ( BuildTarget buildTarget ) {
			switch( buildTarget ) {
				case BuildTarget.StandaloneOSX:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneOSXIntel64:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneOSXIntel:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneWindows64:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneWindows:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneLinux64:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneLinux:
					return BuildTargetGroup.Standalone;

				case BuildTarget.StandaloneLinuxUniversal:
					return BuildTargetGroup.Standalone;

				case BuildTarget.Android:
					return BuildTargetGroup.Android;

				case BuildTarget.iOS:
					return BuildTargetGroup.iOS;

				case BuildTarget.WebGL:
					return BuildTargetGroup.WebGL;

				case BuildTarget.WSAPlayer:
					return BuildTargetGroup.WSA;

				case BuildTarget.Tizen:
					return BuildTargetGroup.Tizen;

				case BuildTarget.PSP2:
					return BuildTargetGroup.PSP2;

				case BuildTarget.PS4:
					return BuildTargetGroup.PS4;

				case BuildTarget.PSM:
					return BuildTargetGroup.PSM;

				case BuildTarget.XboxOne:
					return BuildTargetGroup.XboxOne;

				case BuildTarget.SamsungTV:
					return BuildTargetGroup.SamsungTV;

				case BuildTarget.N3DS:
					return BuildTargetGroup.N3DS;

				case BuildTarget.WiiU:
					return BuildTargetGroup.WiiU;

				case BuildTarget.tvOS:
					return BuildTargetGroup.tvOS;

				case BuildTarget.NoTarget:
					return BuildTargetGroup.Unknown;

				default:
					return BuildTargetGroup.Unknown;

			}
		}


		// This clears all your scripting symbols, not just ones created by the setup process! Only use for debugging purposes!
		public static void DEBUG_ClearSetupData () {
			EditorPrefs.DeleteKey( _ignoreSetup );
			EditorPrefs.DeleteKey( _performingSetup );
			EditorPrefs.DeleteKey( _performingFirstTimeSetup );

			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			var targetGroup = GetGroupFromBuildTarget( buildTarget );
			PlayerSettings.SetScriptingDefineSymbolsForGroup( targetGroup, "" );
		}
	}

	public static class Logger {
		public static void Log ( object message ) {
#if PKPRO_SHOW_DEBUG
			Debug.Log( "<color=#2599f5>[PKPRO]</color> " + message.ToString() );
#endif
		}

    }

   
}