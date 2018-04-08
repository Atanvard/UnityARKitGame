#define PKPRO_SHOW_DEBUG
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
So basically, the requirements for the window are this:
1. It should probably have some sort of Documentation button (although it's not required)
2. Prompt the user to click either VR or NonVR. The VR button automagically imports VRTK and related scripts
3. Prompt the user to hit the "RequireComponent" button there
4. If the user has an existing established project, the "Reassign Layers" button is optional, and it re-assigns the layers used in Prefabs, since during the Import process the asset creates layers in empty slots, but the values are serialized off of ID and not name so they need to be shuffled around
*/

namespace SKStudios.Portals.Editor {
	internal static class GlobalStyles {
		static bool _initialized;

		public static Color linkColor = new Color( 0.24f, 0.49f, 0.90f, 1 );

		public static Texture2D settingsHeaderImg = LoadImageResource( "pkpro_header" );

		public static GUIStyle settingsHeaderText;
		public static GUIStyle welcomeTextStyle;

		public static GUIStyle linkStyle;

	    private static Stack<Color> guiColorStack;
        public static void Init () {
			if( _initialized ) return;
			_initialized = true;

			settingsHeaderText = new GUIStyle( EditorStyles.largeLabel );
			settingsHeaderText.alignment = TextAnchor.UpperLeft;
			settingsHeaderText.fontSize = 18;
			settingsHeaderText.margin.top = 10;
			settingsHeaderText.margin.left += 1;

			if( !EditorGUIUtility.isProSkin ) {
				settingsHeaderText.normal.textColor = new Color( 0.4f, 0.4f, 0.4f, 1f );
			} else {
				settingsHeaderText.normal.textColor = new Color( 0.7f, 0.7f, 0.7f, 1f );
			}


			welcomeTextStyle = new GUIStyle( EditorStyles.label );
			welcomeTextStyle.wordWrap = true;
			welcomeTextStyle.padding = new RectOffset( 30, 30, 30, 30 );
			welcomeTextStyle.fontSize = 12;
			welcomeTextStyle.richText = true;


			linkStyle = new GUIStyle( EditorStyles.label );
			linkStyle.normal.textColor = linkColor;
            linkStyle.margin = new RectOffset(0, 0, 0, 0);
            guiColorStack = new Stack<Color>();
		}
		 
		public static bool LayoutButtonLink ( GUIContent content, params GUILayoutOption[] options ) {
			var controlId = GUIUtility.GetControlID( FocusType.Passive ) + 1; // @Hack, predicting the next control id... may not always work.
			var clicked = GUILayout.Button( content, linkStyle, options );
			var rect = GUILayoutUtility.GetLastRect();

			var widthOffset = 0f;
			var widthContent = content;
			if( content.image != null ) {
				widthContent = new GUIContent( content.text );
				widthOffset = EditorGUIUtility.GetIconSize().x;
			}
			float min, max;
			linkStyle.CalcMinMaxWidth( widthContent, out min, out max );

			var start = new Vector2( rect.x + 2 + widthOffset, rect.y + rect.height - 2 );
			var end   = new Vector2( rect.x - 2 + min + widthOffset, start.y );

			var color = linkStyle.normal.textColor;
			if( GUIUtility.hotControl == controlId ) color = linkStyle.active.textColor;

			Handles.BeginGUI();
			Handles.color = color;
			Handles.DrawLine( start, end );
			Handles.EndGUI();

			return clicked;
		}

		public static void LayoutExternalLink ( GUIContent content, string url, params GUILayoutOption[] options ) {
			if( LayoutButtonLink( content, options ) ) {
				Application.OpenURL( url );
			}
		}

        public static Texture2D LoadImageResource ( string filename ) {
			var imgGuid = AssetDatabase.FindAssets( filename + " t:Texture" );

			if( imgGuid.Length > 0 ) {
				var texture = AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( imgGuid[ 0 ] ) );
				return texture;
			} else {
				return null;
			}
		}

	   
	    public static void StartColorArea(Color color) {
	        guiColorStack.Push(GUI.color);
	        GUI.color = color;
        }

	    public static void EndColorArea() {
	        GUI.color = guiColorStack.Pop();
	    }
	}
}