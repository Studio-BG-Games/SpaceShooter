using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(MultiANDNode), true)]
	class MultiANDNodeEditor : Editor {
		public override void OnInspectorGUI() {
			MultiANDNode node = target as MultiANDNode;
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