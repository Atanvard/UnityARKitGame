using SKStudios.Common.Editor;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SKStudios.Portals.Editor {
	public class SetupPanel : GUIPanel {
		internal static class Content {
			public static readonly GUIContent welcomeText       = new GUIContent ( "You're good to go! You can switch project types any time below, or configure any other setting via the left menu." );
			public static readonly GUIContent documentationText = new GUIContent( "View the documentation", EditorGUIUtility.FindTexture( "_Help" ) );
			public static readonly GUIContent viewDemoText      = new GUIContent( "Check out a demo", EditorGUIUtility.FindTexture( "SceneAsset Icon" ) );
			public static readonly GUIContent extraText         = new GUIContent( "What's next?" );
			public static readonly GUIContent noExtrasText      = new GUIContent( "• Enjoy PortalKit Pro!" );
		}

		internal static class Styles {
			static bool _initialized = false;

			public static GUIStyle extraStyle;

			public static void Init () {
				if( _initialized ) return;
				_initialized = true;

				extraStyle = new GUIStyle();
				extraStyle.padding = new RectOffset( 20, 10, 15, 0 );
			}
		}


		public override string title {
			get {
				return "Setup";
			}
		}

		ProjectModeSelector selector;
		SettingsWindow window;

		public SetupPanel ( SettingsWindow window ) {
			this.window = window;
			selector = new ProjectModeSelector( window );
		}

		public override void OnDisable () {
			window.selectedMode = SetupUtility.projectMode;
		}

		public override void OnGUI ( Rect position ) {
			Styles.Init();

			GUILayout.Label( Content.welcomeText, GlobalStyles.welcomeTextStyle );

			var selectorRect = GUILayoutUtility.GetRect( position.width, 260 );
			selector.Draw( selectorRect );

			GUILayout.BeginVertical( Styles.extraStyle );

			GUILayout.Label( Content.extraText, EditorStyles.boldLabel );

			EditorGUIUtility.SetIconSize( new Vector2( 16, 16 ) );

			var noExtrasToShow = true;
			var documentPath = GetDocumentationPath();
			if( !string.IsNullOrEmpty( documentPath ) ) {
				GlobalStyles.LayoutExternalLink( Content.documentationText, GetDocumentationPath() );
				noExtrasToShow = false;
			}

			var demoPath = GetDemoPath();
			if( !string.IsNullOrEmpty( demoPath ) ) {
				if( GlobalStyles.LayoutButtonLink( Content.viewDemoText ) ) {
					EditorSceneManager.OpenScene( demoPath, OpenSceneMode.Single );
				}
				noExtrasToShow = false;
			}

			if( noExtrasToShow ) {
				GUILayout.Label( Content.noExtrasText );
			}

			GUILayout.EndVertical();
		}

		string GetDemoPath () {
			var path = GetAssetRoot();
			if( SetupUtility.projectMode == ProjectMode.VR ) {
				 path = path + "DemoScenes/VR/VRDemoScene.unity";
			} else {
				path = path + "DemoScenes/Non VR/Demo Scene.unity";
			}
			path = path.Replace( Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, "" );
			if( File.Exists( path ) ) return path;
			return string.Empty;
		}

		string GetDocumentationPath () {
			var path = GetAssetRoot() + "readme.pdf";
			if( File.Exists( path ) ) return path;
			return string.Empty;
		}

		// @Hack, there's got to be a better way to do this
		string GetAssetRoot () {
			string assetName = "PortalKit Pro";
			string path = new System.Diagnostics.StackTrace( true ).GetFrame( 0 ).GetFileName();
			if( path == null ) return string.Empty;

			path = path.Substring( 0, path.LastIndexOf( Path.DirectorySeparatorChar ) );
			string root = path.Substring( 0, path.LastIndexOf( assetName ) + ( assetName.Length + 1 ) );
			return root;
		}
	}
}