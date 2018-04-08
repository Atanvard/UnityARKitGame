using SKStudios.Common.Editor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
	public class FirstRunPanel : GUIPanel {
		internal static class Content {
			public static readonly Texture2D splashImg = GlobalStyles.LoadImageResource( "pkpro_splash" );
			public static readonly GUIContent headerText = new GUIContent( "Welcome to PortalKit <color=#ff9c00><b>PRO</b></color>!" );
			public static readonly GUIContent welcomeText = new GUIContent( "Thank you for supporting <b>PortalKit <color=#ff9c00>PRO</color></b>! We'll have you up and running in no time! Simply choose what type of project you're using this asset with below:" );
		}

		internal static class Styles {
			static bool _initialized = false;

			public static GUIStyle fallbackHeaderStyle;

			public static void Init () {
				if( _initialized ) return;
				_initialized = true;

				fallbackHeaderStyle = new GUIStyle();
				fallbackHeaderStyle.alignment = TextAnchor.MiddleCenter;
				fallbackHeaderStyle.fontSize = 30;
				fallbackHeaderStyle.richText = true;

				var bgTex = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
				bgTex.SetPixel( 0, 0, new Color( 14 / 255f, 3 / 255f, 1 / 255f ) );
				bgTex.Apply();

				fallbackHeaderStyle.normal.background = bgTex;
				fallbackHeaderStyle.normal.textColor = Color.white;
			}
		}

		public override string title {
			get {
				return "Welcome";
			}
		}

		ProjectModeSelector selector;
		SettingsWindow window;

		public FirstRunPanel ( SettingsWindow window ) {
			this.window = window;
			selector = new ProjectModeSelector( window );
		}

		public override void OnGUI ( Rect position ) {
			Styles.Init();

			Rect headerRect;

			var halfWidth = position.width * 0.5f;
			float runningHeight = 0;

			float headerHeight = 200;
			var splashImg = Content.splashImg;

			if( splashImg != null ) {
				// image splash
				headerRect = new Rect( position.width * 0.5f - splashImg.width * 0.5f, 0, splashImg.width, headerHeight );
				GUI.Box( new Rect( 0, 0, position.width, headerHeight ), GUIContent.none, Styles.fallbackHeaderStyle );
				GUI.DrawTexture( headerRect, splashImg, ScaleMode.ScaleToFit );
			} else {
				// text splash
				headerRect = new Rect( 0, 0, position.width, headerHeight );
				GUI.Label( headerRect, Content.headerText, Styles.fallbackHeaderStyle );
			}

			runningHeight += headerRect.height;

			var textWidth = Mathf.Min( position.width * 0.6f, 500 );
			var welcomeTextRect = new Rect( halfWidth - textWidth * 0.5f, headerRect.height, textWidth, GlobalStyles.welcomeTextStyle.CalcHeight( Content.welcomeText, textWidth ) );
			GUI.Label( welcomeTextRect, Content.welcomeText, GlobalStyles.welcomeTextStyle );

			runningHeight += welcomeTextRect.height;

			var selectionBounds = new Rect( 0, runningHeight, position.width, 260 );

			selector.Draw( selectionBounds );
		}
	}
}