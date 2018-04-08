using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Common.Editor {
	public abstract class GUIPanel {
		public abstract string title { get; }
		public abstract void OnGUI ( Rect position );
		public virtual void OnEnable () { }
		public virtual void OnDisable () { }

		protected Rect ApplySettingsPadding ( Rect position ) {
			position.width -= 30;
			position.x += 10;
			position.y += 10;
			position.height -= 30;

			return position;
		}
	}
}