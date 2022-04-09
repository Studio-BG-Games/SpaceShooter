using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(uNodeRoot), true)]
	class uNodeRootEditor : Editor {
		public override void OnInspectorGUI() {
			uNodeRoot root = target as uNodeRoot;
			EditorGUI.BeginDisabledGroup(uNodeEditorUtility.IsPrefab(root));
			EditorGUI.BeginChangeCheck();
			CustomInspector.DrawGraphInspector(root);
			if(EditorGUI.EndChangeCheck()) {
				uNodeEditor.GUIChanged();
			}
			EditorGUI.EndDisabledGroup();
			if(uNodeEditorUtility.IsPrefab(root)) {
				if(root is uNodeRuntime) {
					EditorGUILayout.HelpBox("Open Prefab to Edit Graph", MessageType.Info);
				} else {
					if(GUILayout.Button(new GUIContent("Open uNode Editor", "Open uNode Editor to edit this uNode"), EditorStyles.toolbarButton)) {
						uNodeEditor.Open(target as uNodeRoot);
					}
					EditorGUILayout.HelpBox("Open uNode Editor to Edit values", MessageType.Info);
				}
			} else {
				if(GUILayout.Button(new GUIContent("Open uNode Editor", "Open uNode Editor to edit this uNode"), EditorStyles.toolbarButton)) {
					uNodeEditor.Open(target as uNodeRoot);
				}
			}
			if(!Application.isPlaying && (root is uNodeRuntime || root is ISingletonGraph)) {
				var type = root.GeneratedTypeName.ToType(false);
				if(type != null) {
					EditorGUILayout.HelpBox("Run using Native C#", MessageType.Info);
				} else {
					EditorGUILayout.HelpBox("Run using Reflection", MessageType.Info);
				}
			} else if(Application.isPlaying && root is uNodeComponentSingleton singleton && (singleton.runtimeBehaviour != null || singleton.runtimeInstance != null)) {
				EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Runtime Component");
				if(singleton.runtimeBehaviour == null) {
					uNodeGUIUtility.DrawVariablesInspector(singleton.runtimeInstance.Variables, singleton.runtimeInstance, null);
				} else if(singleton.runtimeBehaviour != null) {
					Editor editor = CustomInspector.GetEditor(singleton.runtimeBehaviour);
					if(editor != null) {
						editor.OnInspectorGUI();
					} else {
						uNodeGUIUtility.ShowFields(singleton.runtimeBehaviour, singleton.runtimeBehaviour);
					}
				}
			}
			if(!Application.isPlaying && root is IIndependentGraph) {
				var system = GraphUtility.GetGraphSystem(root);
				if(system != null && system.allowAutoCompile && uNodeEditorUtility.IsPrefab(root)) {
					var actualGraph = root;
					if(GraphUtility.HasTempGraphObject(root.gameObject)) {
						actualGraph = GraphUtility.GetTempGraphObject(root);
					}
					uNodeGUIUtility.ShowField(new GUIContent("Compile to C#", "If true, the graph will be compiled to C# to run using native c# performance on build or in editor using ( Generate C# Scripts ) menu."), nameof(root.graphData.compileToScript), actualGraph.graphData, actualGraph);
				}
			}
		}
	}
}