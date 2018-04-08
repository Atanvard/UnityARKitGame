using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using SKStudios.Rendering;
using SKStudios.Common.Editor;

namespace SKStudios.Portals.Editor {

	public abstract class SettingsPanel : GUIPanel {
		public override void OnGUI ( Rect position ) {
			position = ApplySettingsPadding( position );

			GUILayout.BeginArea( position );

			GUILayout.Label( title, GlobalStyles.settingsHeaderText );

			EditorGUIUtility.labelWidth = 250;

			position.x = 0;
			position.y = 0;

			EditorGUILayout.Space();

			DoSettingsGUI( position );

			GUILayout.EndArea();
		}

		protected abstract void DoSettingsGUI ( Rect position );
	}

	public class InteractionSettingsPanel : SettingsPanel {

		[MenuItem( SettingsWindow.baseMenuPath + "Interactions", priority = 210 )]
		static void Show () {
			SettingsWindow.Show( true, 2 );
		}

		internal static class Content {
			public static readonly GUIContent physicsHeading = new GUIContent( "Physics" );

			public static readonly GUIContent physicsPassthroughLabel    = new GUIContent( "Enable Physics Passthrough", "Enable collision with objects on the other side of portals." );
			public static readonly GUIContent physicsPassthroughWarning  = new GUIContent(
				"This setting enables interaction with objects on the other side of portals. " +
				"Objects can pass through portals without it, and it is not needed for most games. " +
				"In extreme cases, it can cause a slight performance hit."
			);

			public static readonly GUIContent physicsModelBLabel         = new GUIContent( "Use Physics Model B", "Physics Model B maintains relative momentum between portals. This may or may not be desirable when the portals move." );

			public static readonly GUIContent portalScalingLabel         = new GUIContent( "Enable Portal Scaling", "This should be disabled if portals are never used to change an object's size." );
		}

		public override string title {
			get {
				return "Interactions";
			}
		}

		AnimBool fadePhysicsPassthroughWarning;

		public InteractionSettingsPanel ( SettingsWindow window ) {
			fadePhysicsPassthroughWarning = new AnimBool( SKSGlobalRenderSettings.PhysicsPassthrough, window.Repaint );
		}

		protected override void DoSettingsGUI ( Rect position ) {
			Undo.RecordObject( SKSGlobalRenderSettings.Instance, "Update PortalKit Pro Settings" );

			EditorGUILayout.LabelField( Content.physicsHeading, EditorStyles.boldLabel );
			
			// physics passthrough
			SKSGlobalRenderSettings.PhysicsPassthrough = EditorGUILayout.Toggle( Content.physicsPassthroughLabel, SKSGlobalRenderSettings.PhysicsPassthrough );
			fadePhysicsPassthroughWarning.target = SKSGlobalRenderSettings.PhysicsPassthrough;

			if( EditorGUILayout.BeginFadeGroup( fadePhysicsPassthroughWarning.faded ) ) {
				EditorGUILayout.HelpBox( Content.physicsPassthroughWarning.text, MessageType.Info, true );
			}
			EditorGUILayout.EndFadeGroup();

			// phys style b
			SKSGlobalRenderSettings.PhysStyleB = EditorGUILayout.Toggle( Content.physicsModelBLabel, SKSGlobalRenderSettings.PhysStyleB );

			// scaled renderers
			SKSGlobalRenderSettings.NonScaledRenderers = !EditorGUILayout.Toggle( Content.portalScalingLabel, !SKSGlobalRenderSettings.NonScaledRenderers );


		}
	}

	public class EditorSettingsPanel : SettingsPanel {

		[MenuItem( SettingsWindow.baseMenuPath + "Editor", priority = 220 )]
		static void Show () {
			SettingsWindow.Show( true, 3 );
		}

		internal static class Content {

			public static readonly GUIContent drawGizmosLabel           = new GUIContent( "Draw Portal Gizmos", "Draw portal gizmos when selected in the editor, and when all portals are visualized." );
			public static readonly GUIContent visualizeConnectionsLabel = new GUIContent( "Visualize Portal Connections", "Visualize all portal connections in the scene." );
			public static readonly GUIContent portalPreviewLabel        = new GUIContent( "Draw Portal Previews (Experimental)", "Draw portal previews when selected in the editor. Experimental, and only works with shallow viewing angles." );
		}

		public override string title {
			get {
				return "Editor";
			}
		}

		protected override void DoSettingsGUI ( Rect position ) {
			Undo.RecordObject( SKSGlobalRenderSettings.Instance, "Update PortalKit Pro Settings" );

			SKSGlobalRenderSettings.Gizmos = EditorGUILayout.Toggle( Content.drawGizmosLabel, SKSGlobalRenderSettings.Gizmos );
			SKSGlobalRenderSettings.Visualization = EditorGUILayout.Toggle( Content.visualizeConnectionsLabel, SKSGlobalRenderSettings.Visualization );
			SKSGlobalRenderSettings.Preview = EditorGUILayout.Toggle( Content.portalPreviewLabel, SKSGlobalRenderSettings.Preview );
		}
	}

	public class ImageSettingsPanel : SettingsPanel {
		[MenuItem( SettingsWindow.baseMenuPath + "General", priority = 200 )]
		static void Show () {
			SettingsWindow.Show( true, 1 );
		}

		internal static class Content {
			public static readonly GUIContent recursionLabel           = new GUIContent( "Recursion Number", "The number of times that portals will draw through each other." );
			public static readonly GUIContent recursionQualityWarning  = new GUIContent( "Recursion can get very expensive very quickly. Consider making this scale with the quality settings of your game." );

			public static readonly GUIContent aggressiveOptimiziationLabel = new GUIContent( 
				"Use Aggressive Optimization", 
				"Aggressive optimization will halt recursive rendering immediately if the " +
				"source portal cannot raycast to the portal it is trying to render.\n" +
				"Without occlusion culling (due to lack of Unity support), this is a lifesaver for " +
				"large scenes."
			);
			public static readonly GUIContent aggressiveOptimizinationWarning = new GUIContent(
				"Enabling aggressive optimization can save some serious performance, but it is possible for visual bugs to arise due to portals being partially " +
				"inside walls.\n" +
				"If you are seeing black portals while recursing, try turning this option off and see if it helps. If it does, then please " +
				"make sure that your portals are not inside walls."
			);


			public static readonly GUIContent adaptiveQualityLabel  = new GUIContent( "Use Adaptive Quality (Recommended)", "Adaptive quality rapidly degrades the quality of recursively rendered portals. This is usually desirable." );
			public static readonly GUIContent clippingLabel         = new GUIContent( "Use Perfect Object Clipping", "Enable objects clipping as they enter portals. This is usually desirable." );
			public static readonly GUIContent overrideSkyboxLabel   = new GUIContent( "Override Skybox", "Enable custom skybox rendering. This is needed for skyboxes to not look strange through SKSEffectCameras." );
			public static readonly GUIContent overrideMasksLabel    = new GUIContent( "Override All PortalSpawner Masks" );
			public static readonly GUIContent flipUVsLabel          = new GUIContent( "Flip Portal UVs", "On some hardware, UVs are laid out top-to-bottom rather than bottom-to-top. Check this if your portals are rendering upside down." );


			public static readonly GUIContent headerText = new GUIContent( "Welcome to PortalKit <color=#ff9c00><b>PRO</b></color>!" );
		}

		public override string title {
			get {
				return "General";
			}
		}

		AnimBool fadeAggressiveRecursionWarning;
		AnimBool fadeRecursionNumberWarning;

		public ImageSettingsPanel ( SettingsWindow window ) {
			fadeAggressiveRecursionWarning = new AnimBool( SKSGlobalRenderSettings.AggressiveRecursionOptimization, window.Repaint );
			fadeRecursionNumberWarning = new AnimBool( SKSGlobalRenderSettings.RecursionNumber > 1, window.Repaint );
		}

		protected override void DoSettingsGUI ( Rect position ) {
			Undo.RecordObject( SKSGlobalRenderSettings.Instance, "Update PortalKit Pro Settings" );

			Rendering();
			EditorGUILayout.Space();
			Recursion();
		}

		void Rendering () {

			EditorGUILayout.LabelField( "Rendering", EditorStyles.boldLabel );

			// clipping
			SKSGlobalRenderSettings.Clipping = EditorGUILayout.Toggle( Content.clippingLabel, SKSGlobalRenderSettings.Clipping );

			// custom skybox
			SKSGlobalRenderSettings.CustomSkybox = EditorGUILayout.Toggle( Content.overrideSkyboxLabel, SKSGlobalRenderSettings.CustomSkybox );

			// override masks
			SKSGlobalRenderSettings.ShouldOverrideMask = EditorGUILayout.Toggle( Content.overrideMasksLabel, SKSGlobalRenderSettings.ShouldOverrideMask );
			GUI.enabled = SKSGlobalRenderSettings.ShouldOverrideMask;
			EditorGUI.indentLevel = 1;
			SKSGlobalRenderSettings.Mask = (Texture2D)EditorGUILayout.ObjectField( SKSGlobalRenderSettings.Mask, typeof( Texture2D ), false );
			EditorGUI.indentLevel = 0;
			GUI.enabled = true;

			SKSGlobalRenderSettings.UvFlip = EditorGUILayout.Toggle( Content.flipUVsLabel, SKSGlobalRenderSettings.UvFlip );
		}

		void Recursion () {
			EditorGUILayout.LabelField( "Recursion", EditorStyles.boldLabel );

			// recursion number
			SKSGlobalRenderSettings.RecursionNumber = EditorGUILayout.IntSlider( Content.recursionLabel, SKSGlobalRenderSettings.RecursionNumber, 0, 10 );
			fadeRecursionNumberWarning.target = SKSGlobalRenderSettings.RecursionNumber > 1;

			if( EditorGUILayout.BeginFadeGroup( fadeRecursionNumberWarning.faded ) ) {
				var msgType = SKSGlobalRenderSettings.RecursionNumber > 5 ? MessageType.Error : MessageType.Warning;
				EditorGUILayout.HelpBox( Content.recursionQualityWarning.text, msgType, true );
			}
			EditorGUILayout.EndFadeGroup();

			// adaptive quality
			SKSGlobalRenderSettings.AdaptiveQualityCached = EditorGUILayout.Toggle( Content.adaptiveQualityLabel, SKSGlobalRenderSettings.AdaptiveQualityCached );

			// aggressive optimization
			SKSGlobalRenderSettings.AggressiveRecursionOptimization = EditorGUILayout.Toggle( Content.aggressiveOptimiziationLabel, SKSGlobalRenderSettings.AggressiveRecursionOptimization );
			fadeAggressiveRecursionWarning.target = SKSGlobalRenderSettings.AggressiveRecursionOptimization;

			if( EditorGUILayout.BeginFadeGroup( fadeAggressiveRecursionWarning.faded ) ) {
				EditorGUILayout.HelpBox( Content.aggressiveOptimizinationWarning.text, MessageType.Info, true );
			}

			EditorGUILayout.EndFadeGroup();
		}
	}
}
