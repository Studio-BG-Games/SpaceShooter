using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(MultiORNode), true)]
	class MultiORNodeEditor : Editor {
		public override void OnInspectorGUI() {
			Nodes.MultiORNode node = target as MultiORNode;
			DrawDefaultInspector();
			VariableEditorUtility.DrawMembers(new GUIContent("Targets"), node.targets, node, new FilterAttribute(typeof(bool)),
				(obj) => {
					node.targets = obj;
				},
				() => {
					uNodeEditorUtility.RegisterUndo(node);
					node.targets.Add(new MemberData(true));
				});
		}
	}
}