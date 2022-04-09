using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(GroupNode))]
	public class GroupNodeView : BaseNodeView {
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
			evt.menu.AppendAction("Open Group", (e) => {
				owner.graph.editorData.selectedGroup = targetNode as Node;
				owner.graph.Refresh();
				owner.graph.UpdatePosition();
			}, DropdownMenuAction.AlwaysEnabled);
			base.BuildContextualMenu(evt);
		}

		protected override void InitializeView() {
			base.InitializeView();
			titleContainer.RegisterCallback<MouseDownEvent>(e => {
				if(e.button == 0 && e.clickCount == 2) {
					owner.graph.editorData.selectedGroup = targetNode as Node;
					owner.graph.Refresh();
					owner.graph.UpdatePosition();
				}
			});
		}
	}
}