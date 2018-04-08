using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using SKStudios.Common.Utils.SafeRemoveComponent;
using SKStudios.Portals.Editor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

namespace SKStudios.Common.Editor {

	public class DebugInfoPanel : GUIPanel {
		[MenuItem( SettingsWindow.baseMenuPath + "Debug", priority = 300 )]
		static void Show () {
			SettingsWindow.Show( true, 4 );
		}

		internal static class Content {
			public static readonly GUIContent dependencyGraphHeading = new GUIContent( "RequireComponent Dependency Graph" );
			public static readonly GUIContent debugDumpHeading = new GUIContent( "Debug Log Dump" );

			public static readonly GUIContent scanButtonText = new GUIContent( "Refresh Graph" );
			public static readonly GUIContent noDataText = new GUIContent( "No data to show. This usually means something has gone wrong. Please press the 'refresh graph' button above." );
			public static readonly GUIContent regeneratingGraphText = new GUIContent( "Regenerating Dependency Graph..." );
		}

		internal static class Styles {
			static bool _initialized = false;


			public static GUIStyle sectionHeading;
			public static GUIStyle sectionBody;

			public static GUIStyle scanButtonStyle;

			public static GUIStyle debugBody;

			public static GUIStyle tableBgStyle;
			public static GUIStyle tableRowStyle;
			public static GUIStyle tableRowHeaderStyle;

			public static GUIStyle debugTextArea;


			public static readonly Color lineColor = EditorGUIUtility.isProSkin ? new Color( 0.157f, 0.157f, 0.157f ) : new Color( 0.5f, 0.5f, 0.5f );

			public static void Init () {
				if( _initialized ) return;
				_initialized = true;

				sectionHeading = new GUIStyle( "IN TitleText" );

				scanButtonStyle = new GUIStyle( EditorStyles.miniButton );
				scanButtonStyle.margin = new RectOffset( 0, 0, 0, 0 ); // reset margin to avoid shifting during fades

				tableBgStyle = new GUIStyle();
				tableBgStyle.normal.background = EditorGUIUtility.whiteTexture;
				tableBgStyle.padding = new RectOffset( 0, 0, 10, 0 );

				tableRowStyle = new GUIStyle( EditorStyles.label );
				tableRowStyle.border = new RectOffset( 2, 2, 2, 2 );
				tableRowStyle.padding = new RectOffset( 30, 10, 4, 4 );
				tableRowStyle.contentOffset = new Vector2( 0, -2 );
				tableRowStyle.alignment = TextAnchor.MiddleLeft;
				tableRowStyle.margin = new RectOffset( 0, 0, 0, 0 ); // reset margin to avoid shifting during fades

				tableRowHeaderStyle = new GUIStyle( "foldout" );
				tableRowHeaderStyle.contentOffset = new Vector2( 0, -2 );
				tableRowHeaderStyle.margin = new RectOffset( 0, 0, 0, 0 ); // reset margin to avoid shifting during fades

				debugBody = new GUIStyle();
				debugBody.padding = new RectOffset( 10, 20, 10, 20 );

				sectionBody = new GUIStyle( EditorStyles.helpBox );
				sectionBody.padding.left = 14;

				debugTextArea = new GUIStyle( EditorStyles.textArea );
				debugTextArea.margin = new RectOffset( 0, 0, 0, 0 ); // reset margin to avoid shifting during fades
			}
		}


		public override string title {
			get {
				return "Debug";
			}
		}
		
		struct GraphRow {
			public Type type;
			public List<Type> subTypes;

			public AnimBool fader;
		}

		int selectedSection = -1;
		AnimBool fadeDependencyGraph;
		AnimBool fadeDebugDump;

		Vector2 scrollPos = new Vector2();
		List<GraphRow> displayGraph = new List<GraphRow>();
		SettingsWindow window;

		public DebugInfoPanel ( SettingsWindow window ) {
			this.window = window;

			fadeDependencyGraph = new AnimBool( false, window.Repaint );
			fadeDebugDump = new AnimBool( false, window.Repaint );

			selectedSection = 0;
			UpdateFaders();
		}

		public override void OnEnable () {
			ConstructDisplayGraph();
		}

		public override void OnGUI ( Rect position ) {
			Styles.Init();

			scrollPos = GUILayout.BeginScrollView( scrollPos );
			GUILayout.BeginVertical( Styles.debugBody );

			GUILayout.Label( title, GlobalStyles.settingsHeaderText );

			EditorGUILayout.Space();

			DoDebugInfo();
			DoDependencyGraph( position );

			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}

		private void UpdateFaders () {
			fadeDebugDump.target = selectedSection == 0;
			fadeDependencyGraph.target = selectedSection == 1;
		}

		void DoDebugInfo () {
			EditorGUILayout.BeginVertical( Styles.sectionBody );

			EditorGUI.BeginChangeCheck();
			var toggled = GUILayout.Toggle( selectedSection == 0, Content.debugDumpHeading, Styles.sectionHeading );

			if( EditorGUI.EndChangeCheck() ) {
				selectedSection = toggled ? 0 : -1;
				UpdateFaders();
			}

			if( EditorGUILayout.BeginFadeGroup( fadeDebugDump.faded ) ) {

				EditorGUILayout.Space();

				const string controlName = "pkpro_debug_dump";
				GUI.SetNextControlName( controlName );

				GUILayout.TextArea( "debug log goes here", Styles.debugTextArea, GUILayout.Height( 70 ) );
				if( GUI.GetNameOfFocusedControl() == controlName ) {
					var textEditor = (TextEditor)GUIUtility.GetStateObject( typeof( TextEditor ), GUIUtility.keyboardControl );
					if( Event.current.type == EventType.ValidateCommand ) {
						if( Event.current.commandName == "Copy" ) {
							textEditor.Copy();
						}
					} else if( Event.current.type != EventType.Layout || Event.current.type != EventType.Layout ) {
						textEditor.SelectAll();
					}
				}
			}

			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.EndVertical();
		}

		void DoDependencyGraph ( Rect position ) {
			EditorGUILayout.BeginVertical( Styles.sectionBody );

			EditorGUI.BeginChangeCheck();
			var toggled = GUILayout.Toggle( selectedSection == 1, Content.dependencyGraphHeading, Styles.sectionHeading );

			if( EditorGUI.EndChangeCheck() ) {
				selectedSection = toggled ? 1 : -1;
				UpdateFaders();
			}

			if( EditorGUILayout.BeginFadeGroup( fadeDependencyGraph.faded ) ) {

				EditorGUILayout.Space();

				if( GUILayout.Button( Content.scanButtonText, Styles.scanButtonStyle, GUILayout.ExpandWidth( false ) ) ) {
					EditorUtility.DisplayProgressBar( Content.regeneratingGraphText.text, "", 1f );
					Dependencies.RescanDictionary();
					EditorUtility.ClearProgressBar();
					ConstructDisplayGraph();
				}

				EditorGUILayout.Space();

				DrawGraph( position );
			}

			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.EndVertical();
		}

		// resets expanded rows also
		void ConstructDisplayGraph () {
			displayGraph.Clear();

			foreach( var type in Dependencies.DependencyGraph.Keys ) {
				GraphRow row = new GraphRow();
				row.type = type;
				row.fader = new AnimBool( false, window.Repaint );

				// @Hack, Dependencies can return duplicates
				List<Type> subTypes = null;
				subTypes = new List<Type>();
				foreach( var item in Dependencies.DependencyGraph[ type ] ) {
					if( subTypes.Contains( item ) ) continue;
					subTypes.Add( item );
				}

				row.subTypes = subTypes;
				displayGraph.Add( row );
			}
		}

		void DrawGraph ( Rect position ) {
			if( Dependencies.DependencyGraph.Count == 0 ) {
				EditorGUILayout.HelpBox( Content.noDataText.text, MessageType.Info );
			} else {
				float entryHeight = EditorGUIUtility.singleLineHeight + 5;

				EditorGUIUtility.SetIconSize( new Vector2( 16, 16 ) );

				foreach( var row in displayGraph ) {
					var rowHeight = entryHeight;
					var fader = row.fader;
					var type = row.type;
					var subTypes = row.subTypes;

					var entryIcon = GetIconForType( type );
					var headerContent = new GUIContent( type.Name + " (" + subTypes.Count + ")", entryIcon );

					fader.target = GUILayout.Toggle( fader.target, headerContent, Styles.tableRowHeaderStyle );

					bool showSubTypes = fader.value;

					var canFade = fadeDependencyGraph.faded == 1;
					if( canFade ) {
						showSubTypes = EditorGUILayout.BeginFadeGroup( fader.faded );
					}
					if( showSubTypes ) {
						Handles.BeginGUI();
						var color = Handles.color;
						Rect firstRect = new Rect();
						Handles.color = Styles.lineColor;

						var indent = 10;

						for( int i = 0; i < subTypes.Count; i++ ) {
							Type subType = subTypes[ i ];

							var rowIcon = GetIconForType( subType );
							GUILayout.Label( new GUIContent( subType.Name, rowIcon ), Styles.tableRowStyle );

							var iconRect = GUILayoutUtility.GetLastRect();

							if( i == 0 ) firstRect = iconRect;

							Handles.DrawLine( new Vector3( (int)iconRect.x + indent - 3, (int)iconRect.y + 11 ), new Vector3( (int)iconRect.x + indent - 3 + 15, (int)iconRect.y + 11 ) );
						}

						var lastRect = GUILayoutUtility.GetLastRect();
						Handles.DrawLine( new Vector3( firstRect.x + indent - 3, firstRect.y ), new Vector3( lastRect.x + indent - 3, lastRect.y + 11 ) );

						Handles.color = color;
						Handles.EndGUI();

						GUILayout.Space( 3 );
					}
					if( canFade ) {
						EditorGUILayout.EndFadeGroup();
					}
				}
			}
		}

		private static Texture2D GetIconForType ( Type type ) {
			Texture2D icon = AssetPreview.GetMiniTypeThumbnail( type );
			if( icon == null ) {
				var guids = AssetDatabase.FindAssets( "t:MonoScript " + type.Name );

				if( guids.Length > 0 ) {
					if( guids.Length > 1 ) {
						for( int i = 0; i < guids.Length; i++ ) {
							var guid = guids[ i ];
							var path = AssetDatabase.GUIDToAssetPath( guid );
							if( Path.GetFileNameWithoutExtension( path ) == type.Name ) {
								icon = (Texture2D)AssetDatabase.GetCachedIcon( path );
								break;
							}
						}
					} else {
						icon = (Texture2D)AssetDatabase.GetCachedIcon( AssetDatabase.GUIDToAssetPath( guids[ 0 ] ) );
					}
				}

				if( icon == null ) {
					icon = EditorGUIUtility.FindTexture( "DefaultAsset Icon" );
				}
			}
			return icon;
		}
	}
}

