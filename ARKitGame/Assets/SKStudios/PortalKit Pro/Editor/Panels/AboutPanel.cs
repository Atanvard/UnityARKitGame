using SKStudios.Common.Editor;
using UnityEditor;
using UnityEngine;

namespace SKStudios.Portals.Editor {
	public class AboutPanel : GUIPanel {
		[MenuItem( SettingsWindow.baseMenuPath + "About", priority = 310 )]
		static void Show () {
			SettingsWindow.Show( true, 5 );
		}

		internal static class Content {
			public static readonly GUIContent aboutVersionText = new GUIContent( string.Format( "PortalKit PRO v{0}.{1}.{2}", GlobalPortalSettings.MAJOR_VERSION, GlobalPortalSettings.MINOR_VERSION, GlobalPortalSettings.PATCH_VERSION ) );
		    public static readonly GUIContent disclaimerText = new GUIContent("Note: These are <i>not</i> support emails.\nIf you need help, please see the 'Feedback' tab.");
            public static readonly GUIContent createdByText = new GUIContent( "Made with <color=#ad164b>❤</color> by:" );
			public static readonly GUIContent createdByText2 = new GUIContent( "Editor by:" );
			public static readonly GUIContent c1 = new GUIContent( "SKStudios" );
			public static readonly GUIContent c2 = new GUIContent( "Nate Tessman" );

			public static readonly GUIContent c1WebsiteText = new GUIContent("skstudios.io");
			public static readonly GUIContent c1EmailText = new GUIContent("superkawaiiltd@gmail.com");
			public static readonly GUIContent c1TwitterText = new GUIContent("@studios_sk");

			public static readonly string c1WebsiteLink = "http://skstudios.io";
			public static readonly string c1EmailLink = "mailto:superkawaiiltd@gmail.com";
			public static readonly string c1TwitterLink = "https://twitter.com/studios_sk";

			public static readonly GUIContent c2WebsiteText = new GUIContent( "madgvox.com" );
			public static readonly GUIContent c2TwitterText = new GUIContent( "@madgvox" );

			public static readonly string c2WebsiteLink = "https://madgvox.com/";
			public static readonly string c2TwitterLink = "https://twitter.com/madgvox";

			public static readonly Texture2D logo = GlobalStyles.LoadImageResource( "pkpro_about_logo" );
		}


		internal static class Styles {
			static bool _initialized = false;

			public static GUIStyle createdByTextStyle;
			public static GUIStyle createdByTextStyle2;
			public static GUIStyle aboutVersionStyle;
		    public static GUIStyle disclaimerStyle;

            public static void Init () {
				if( _initialized ) return;
				_initialized = true;

				aboutVersionStyle = new GUIStyle( EditorStyles.largeLabel );
				aboutVersionStyle.alignment = TextAnchor.MiddleCenter;

				createdByTextStyle = new GUIStyle( EditorStyles.label );
				createdByTextStyle.richText = true;
				createdByTextStyle.alignment = TextAnchor.LowerLeft;
				createdByTextStyle.normal.textColor = new Color( 0.38f, 0.38f, 0.38f, 1 );

				createdByTextStyle2 = new GUIStyle( EditorStyles.label );
				createdByTextStyle2.richText = true;

                disclaimerStyle = new GUIStyle(EditorStyles.label);
                disclaimerStyle.richText = true;
                disclaimerStyle.alignment = TextAnchor.LowerLeft;
                disclaimerStyle.normal.textColor = new Color(0.38f, 0.38f, 0.38f, 1);
            }
		}


		public override string title {
			get {
				return "About";
			}
		}

		public override void OnGUI ( Rect position ) {
			Styles.Init();

			position = ApplySettingsPadding( position );

			GUILayout.BeginArea( position );

			GUILayout.FlexibleSpace();

			var img = Content.logo;
			var imgRect = GUILayoutUtility.GetRect( img.width, img.height );
			GUI.DrawTexture( imgRect, img, ScaleMode.ScaleToFit );

			GUILayout.Space( 30 );

			GUILayout.Label( Content.aboutVersionText, Styles.aboutVersionStyle );

			GUILayout.Space( 20 );
            

            GUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();

		    

            GUILayout.BeginVertical();
			GUILayout.Label( Content.createdByText, Styles.createdByTextStyle, GUILayout.Width( 160 ) );
			GUILayout.Space( 10 );
			GUILayout.Label( Content.c1, EditorStyles.boldLabel );
			GlobalStyles.LayoutExternalLink( Content.c1WebsiteText, Content.c1WebsiteLink );
			GlobalStyles.LayoutExternalLink( Content.c1EmailText,   Content.c1EmailLink );
			GlobalStyles.LayoutExternalLink( Content.c1TwitterText, Content.c1TwitterLink );
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical();
			GUILayout.Label( Content.createdByText2, Styles.createdByTextStyle, GUILayout.Width( 160 ) );
			GUILayout.Space( 10 );
			GUILayout.Label( Content.c2, EditorStyles.boldLabel );
			GlobalStyles.LayoutExternalLink( Content.c2TwitterText, Content.c2TwitterLink );
			GlobalStyles.LayoutExternalLink( Content.c2WebsiteText, Content.c2WebsiteLink );
			GUILayout.EndVertical();


			GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

		    GUILayout.Space(10);

            GUILayout.BeginHorizontal();
		    GUILayout.Space(25);
            GUILayout.Label(Content.disclaimerText, Styles.disclaimerStyle);
		    GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

			GUILayout.EndArea();
		   
        }
	}
}
