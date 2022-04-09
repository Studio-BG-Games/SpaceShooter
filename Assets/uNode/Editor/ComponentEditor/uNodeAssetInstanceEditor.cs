using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(uNodeAssetInstance), true)]
	class uNodeAssetInstanceEditor : Editor {
		public override void OnInspectorGUI() {
			uNodeAssetInstance script = target as uNodeAssetInstance;
			serializedObject.UpdateIfRequiredOrScript();
			var position = uNodeGUIUtility.GetRect();
			var bPos = position;
			bPos.x += position.width - 20;
			bPos.width = 20;
			if(GUI.Button(bPos, "", EditorStyles.label)) {
				var items = ItemSelector.MakeCustomItemsForInstancedType(new System.Type[] { typeof(uNodeClassAsset) }, (val) => {
					script.target = val as uNodeClassAsset;
					uNodeEditorUtility.MarkDirty(script);
				}, uNodeEditorUtility.IsSceneObject(script));
				ItemSelector.ShowWindow(null, null, null, null, items).ChangePosition(bPos.ToScreenRect()).displayDefaultItem = false;
				Event.current.Use();
			}
			EditorGUI.PropertyField(position, serializedObject.FindProperty(nameof(script.target)), new GUIContent("Graph", "The target graph reference"));
			serializedObject.ApplyModifiedProperties();
			if(script.target != null) {
				if(!Application.isPlaying || script.runtimeAsset == null) {
					EditorGUI.BeginChangeCheck();
					VariableEditorUtility.DrawLinkedVariables(script, script.target, "");
					if(EditorGUI.EndChangeCheck()) {
						uNodeEditorUtility.MarkDirty(script);
					}
				} else if(script.runtimeAsset != null) {
					Editor editor = CustomInspector.GetEditor(script.runtimeAsset);
					if(editor != null) {
						EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Runtime Asset");
						editor.OnInspectorGUI();
					} else {
						uNodeGUIUtility.ShowFields(script.runtimeAsset, script.runtimeAsset);
					}
				}
				if(script.target is IClassAsset) {
					if(!Application.isPlaying) {
						var type = script.target.GeneratedTypeName.ToType(false);
						if(type != null) {
							EditorGUILayout.HelpBox("Run using Native C#", MessageType.Info);
						} else {
							EditorGUILayout.HelpBox("Run using Reflection", MessageType.Info);
						}
					}
				} else {
					EditorGUILayout.HelpBox("The target graph is not supported.", MessageType.Warning);
				}
				if(!Application.isPlaying || script.runtimeAsset == null) {
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button(new GUIContent("Edit Target", ""), EditorStyles.toolbarButton)) {
						uNodeEditor.Open(script.target);
					}
					if(Application.isPlaying && script.runtimeInstance != null) {
						if(GUILayout.Button(new GUIContent("Debug Target", ""), EditorStyles.toolbarButton)) {
							uNodeEditor.Open(script.runtimeInstance);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			} else {
				EditorGUILayout.HelpBox("Please assign the target graph", MessageType.Error);
			}
		}
	}
}