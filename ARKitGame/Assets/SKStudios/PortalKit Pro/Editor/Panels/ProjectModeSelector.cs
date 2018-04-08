#define PKPRO_SHOW_DEBUG
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Reflection;
using System;

namespace SKStudios.Portals.Editor {
	public class ProjectModeSelector {

		internal static class Content {
			public static readonly GUIContent applyVrModeText = new GUIContent( "Use VR" );
			public static readonly GUIContent usingVrModeText = new GUIContent( "Using VR" );
			public static readonly GUIContent applyDefaultModeText = new GUIContent( "Use Default" );
			public static readonly GUIContent usingDefaultModeText = new GUIContent( "Using Default" );
			public static readonly GUIContent vrModeTooltipText = new GUIContent( "Choose <b>VR Mode</b> if you wish to use portals in a VR game." );
			public static readonly GUIContent defaultModeTooltipText = new GUIContent( "Choose <b>Default Mode</b> if you wish to use portals in a desktop, tablet, or mobile project." );
			public static readonly GUIContent importVRTKText = new GUIContent( "I already have VRTK installed.", "VR portals require VRTK as a dependency.\nIf you are providing your own version of VRTK, uncheck this to prevent conflicts." );

			public static readonly GUIContent ignoreButtonText = new GUIContent( "No thanks, I'll set up PKPro later." );
			public static readonly string ignorePopupWarningText = string.Format( "Warning! PortalKit will *not* function properly until it has been configured!\n\nYou can always return to this setup menu by going to {0}.", SettingsWindow.menuPath.Replace( "/", " > " ) );

			static readonly Texture2D vrIcon = GlobalStyles.LoadImageResource( "pkpro_vr" );
			static readonly Texture2D defaultIcon = GlobalStyles.LoadImageResource( "pkpro_default" );

			public static readonly GUIContent vrModeText = new GUIContent( "VR", vrIcon );
			public static readonly GUIContent normalModeText = new GUIContent( "Default", defaultIcon );
			public static readonly GUIContent vrModeActiveText = new GUIContent( "VR (Active)", vrIcon );
			public static readonly GUIContent normalModeActiveText = new GUIContent( "Default (Active)", defaultIcon );
		}

		internal static class Styles {
			static bool _initialized = false;

			public static readonly Texture2D borderImg = GlobalStyles.LoadImageResource( "pkpro_border" );

			public static GUIStyle modeTooltipStyle;

			public static GUIStyle selectedTextStyle;
			public static GUIStyle deselectedTextStyle;
			public static GUIStyle largeButtonStyle;
		    public static GUIStyle bgStyle;
		    
            public static readonly Color selectedBorderColor = new Color( 1, 0.61f, 0, 1 );
			public static readonly Color deselectedBorderColor = new Color( 0.4f, 0.4f, 0.4f, 1 );

			public static void Init () {
				if( _initialized ) return;
				_initialized = true;

				deselectedTextStyle = new GUIStyle( EditorStyles.label );
				deselectedTextStyle.fontSize = 13;
				deselectedTextStyle.richText = false;
				deselectedTextStyle.alignment = TextAnchor.MiddleCenter;

				selectedTextStyle = new GUIStyle( deselectedTextStyle );
				selectedTextStyle.fontStyle = FontStyle.Bold;

				largeButtonStyle = new GUIStyle();
				largeButtonStyle.normal.background = borderImg;
				largeButtonStyle.border = new RectOffset( 4, 4, 4, 4 );

				modeTooltipStyle = new GUIStyle();
				modeTooltipStyle.wordWrap = true;
				modeTooltipStyle.fontSize = 11;
				modeTooltipStyle.richText = true;
				modeTooltipStyle.alignment = TextAnchor.UpperLeft;


				var proBg     = GlobalStyles.LoadImageResource( "pkpro_selector_bg_pro" );
				var defaultBg = GlobalStyles.LoadImageResource( "pkpro_selector_bg" );

				bgStyle = new GUIStyle();
				bgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
                bgStyle.border = new RectOffset( 2, 2, 2, 2 );
            }
		}

		SettingsWindow window;

		AnimBool showModeTooltip;

		bool needVRTK = false;

		public ProjectModeSelector ( SettingsWindow window ) {
			this.window = window;
			showModeTooltip = new AnimBool( SetupUtility.ignoringInitialSetup || SetupUtility.projectMode != ProjectMode.None, window.Repaint );

			needVRTK = SetupUtility.VRTKIsMaybeInstalled;
		}

		public void DrawLayout () {

		}

		public void Draw ( Rect rect ) {
			Styles.Init();

			var ignoreButtonRect = new Rect( rect.x + rect.width * 0.5f - rect.width * 0.25f, rect.y + rect.height - 50, rect.width * 0.5f, 40 );

			var selectedMode = window.selectedMode;

			if( SetupUtility.projectMode == ProjectMode.None ) {
				if( selectedMode == ProjectMode.None ) {
				    GlobalPortalSettings.TimeInstalled = System.DateTime.UtcNow.Ticks;

                    if ( GUI.Button( ignoreButtonRect, Content.ignoreButtonText ) ) {
						if( EditorUtility.DisplayDialog( "PKPro Setup", Content.ignorePopupWarningText, "Ok", "Cancel" ) ) {
							SetupUtility.ignoringInitialSetup = true;
							window.Close();
						}
					}
				} else {
					GUI.Box( ignoreButtonRect, Content.ignoreButtonText, new GUIStyle( "button" ) );
				}
			}

			int tooltipHeight = 60;
			var localRect = new Rect( 0, 0, (int)rect.width, (int)rect.height - tooltipHeight );

			var currentTooltipHeight = showModeTooltip.Fade( 0, tooltipHeight );
			rect.height -= tooltipHeight - currentTooltipHeight;
			GUI.BeginGroup( rect );

			var boxRect = localRect;
			boxRect.height += currentTooltipHeight;

            GUI.Box(boxRect, GUIContent.none, Styles.bgStyle);

			var tooltipRect = new Rect();
			tooltipRect.width = Mathf.Clamp( boxRect.width * 0.6f, 380, 500 );
			tooltipRect.height = tooltipHeight;
			tooltipRect.x = (int)( boxRect.width * 0.5f - tooltipRect.width * 0.5f );
			tooltipRect.y = boxRect.height - currentTooltipHeight;
			GUI.BeginClip( tooltipRect, Vector2.zero, Vector2.zero, false );

			var tooltipText = selectedMode == ProjectMode.VR ? Content.vrModeTooltipText : Content.defaultModeTooltipText;
			GUI.Label( new Rect( 10, 0, tooltipRect.width - 130, tooltipRect.height ), tooltipText, Styles.modeTooltipStyle );


			if( selectedMode == ProjectMode.VR ) {
				var effectiveTextHeight = Styles.modeTooltipStyle.CalcHeight( tooltipText, tooltipRect.width - 130 );

				var toggleRect = new Rect( 10, effectiveTextHeight + 2, tooltipRect.width - 130, 20 );
				GUI.enabled = SetupUtility.projectMode != selectedMode;
				needVRTK = GUI.Toggle( toggleRect, needVRTK, Content.importVRTKText );
				GUI.enabled = true;
			}


			var alreadyUsingMode = selectedMode == SetupUtility.projectMode;

			GUIContent buttonText = GUIContent.none;
			if( alreadyUsingMode ) {
				if( selectedMode == ProjectMode.Default ) buttonText = Content.usingDefaultModeText;
				else buttonText = Content.usingVrModeText;
			} else {
				if( selectedMode == ProjectMode.Default ) buttonText = Content.applyDefaultModeText;
				else buttonText = Content.applyVrModeText;
			}
			GUI.enabled = !alreadyUsingMode;
			if( GUI.Button( new Rect( tooltipRect.width - 110, 0, 100, 30 ), buttonText ) ) {
				if( selectedMode == ProjectMode.VR ) {
					SetupUtility.ApplyVR(!needVRTK );
				} else {
					SetupUtility.ApplyDefault();
				}
			}

			GUI.enabled = true;
			GUI.EndClip();

			localRect.width = Mathf.Clamp( localRect.width * 0.6f, 380, 500 );
			localRect.x = rect.width * 0.5f - localRect.width * 0.5f;

			var buttonHeight = 150;
			var buttonRect = new Rect( localRect.x + 5, localRect.height * 0.5f - buttonHeight * 0.5f, localRect.width * 0.5f - 10, buttonHeight );



			DoDefaultButton( buttonRect, selectedMode == ProjectMode.Default );
			buttonRect.x += localRect.width * 0.5f;
			DoVRButton( buttonRect, selectedMode == ProjectMode.VR );

			GUI.EndGroup();
		}

		void DoVRButton ( Rect buttonRect, bool selected ) {
			var text = Content.vrModeText;
			if( SetupUtility.projectMode == ProjectMode.VR ) {
				text = Content.vrModeActiveText;
			}

			if( ModeButton( buttonRect, text, selected ) ) {
				window.selectedMode = ProjectMode.VR;
				showModeTooltip.target = true;
			}
		}

		void DoDefaultButton ( Rect buttonRect, bool selected ) {
			var text = Content.normalModeText;
			if( SetupUtility.projectMode == ProjectMode.Default ) {
				text = Content.normalModeActiveText;
			}

			if( ModeButton( buttonRect, text, selected ) ) {
				window.selectedMode = ProjectMode.Default;
				showModeTooltip.target = true;
			}
		}

		bool ModeButton ( Rect buttonRect, GUIContent content, bool selected ) {
			var textHeight = 40;
			var textRect = new Rect( buttonRect.x, buttonRect.y + buttonRect.height - textHeight, buttonRect.width, textHeight );
			var imgRect = new Rect( buttonRect.x + ( buttonRect.width * 0.5f - 64 ), buttonRect.y + ( buttonRect.height * 0.4f - 64 ), 128, 128 );

			var c = GUI.color;
			GUIStyle textStyle;
			if( selected ) {
				GUI.color = Styles.selectedBorderColor;
				textStyle = Styles.selectedTextStyle;
			} else {
				GUI.color = Styles.deselectedBorderColor;
				textStyle = Styles.deselectedTextStyle;
			}
			var hit = GUI.Button( buttonRect, GUIContent.none, Styles.largeButtonStyle );
			GUI.color = c;

			GUI.Label( textRect, content.text, textStyle );
			GUI.DrawTexture( imgRect, content.image );

			return hit;
		}
	}
}