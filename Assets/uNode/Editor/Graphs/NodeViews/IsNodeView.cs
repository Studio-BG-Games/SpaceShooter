using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.ISNode))]
	public class IsNodeView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			// titleIcon.image = null;
			var node = targetNode as Nodes.ISNode;
			if (uNodeUtility.preferredDisplay != DisplayKind.Full) {
				var control = inputControls.FirstOrDefault();
				if(control != null) {
					var label = control.Query<Label>().First();
					if(label != null) {
						label.RemoveFromHierarchy();
					}
				}
				ConstructCompactTitle("target", control: control);
			}
		}
	}
}