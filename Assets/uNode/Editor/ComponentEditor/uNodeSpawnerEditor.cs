using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(uNodeSpawner), true)]
	class uNodeSpawnerEditor : Editor {
#if UNITY_2019_3_OR_NEWER
		Color proColor = new Color32(62, 62, 62, 255);
		Color plebColor = new Color32(194, 194, 194, 255);
#else
		Color proColor = new Color32(56, 56, 56, 255);
		Color plebColor = new Color32(194, 194, 194, 255);
#endif

		public override void OnInspectorGUI() {
			//OnHeaderGUI();
			uNodeSpawner root = target as uNodeSpawner;
			serializedObject.UpdateIfRequiredOrScript();
			var position = uNodeGUIUtility.GetRect();
			var bPos = position;
			bPos.x += position.width - 20;
			bPos.width = 20;
			if(GUI.Button(bPos, "", EditorStyles.label)) {
				var items = ItemSelector.MakeCustomItemsForInstancedType(new System.Type[] { typeof(IGraphWithUnityEvent), typeof(IClassComponent) }, (val) => {
					serializedObject.FindProperty(nameof(root.target)).objectReferenceValue = val as uNodeRoot;
					serializedObject.ApplyModifiedProperties();
				}, uNodeEditorUtility.IsSceneObject(root));
				ItemSelector.ShowWindow(null, null, null, null, items).ChangePosition(bPos.ToScreenRect()).displayDefaultItem = false;
				Event.current.Use();
			}
			EditorGUI.PropertyField(position, serializedObject.FindProperty(nameof(root.target)), new GUIContent("Graph", "The target graph reference"));
			// EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(root.mainObject)));
			serializedObject.ApplyModifiedProperties();
			if(root.target != null) {
				if(!Application.isPlaying || root.runtimeBehaviour == null) {
					EditorGUI.BeginChangeCheck();
					VariableEditorUtility.DrawLinkedVariables(root, root.target, null);
					if(EditorGUI.EndChangeCheck()) {
						uNodeEditorUtility.MarkDirty(root);
					}
				} else if(root.runtimeBehaviour != null) {
					Editor editor = CustomInspector.GetEditor(root.runtimeBehaviour);
					if(editor != null) {
						EditorGUI.DropShadowLabel(uNodeGUIUtility.GetRect(), "Runtime Component");
						editor.OnInspectorGUI();
						if(Event.current.type == EventType.Repaint) {
							editor.Repaint();
						}
					} else {
						uNodeGUIUtility.ShowFields(root.runtimeBehaviour, root.runtimeBehaviour);
					}
				}
				if(root.target is IGraphWithUnityEvent || root.target is IClassComponent) {
					if(!Application.isPlaying) {
						if(uNodePreference.preferenceData.generatorData.compilationMethod == CompilationMethod.Unity) {
							var type = root.target.GeneratedTypeName.ToType(false);
							if(type != null) {
								EditorGUILayout.HelpBox("Run using Native C#", MessageType.Info);
							} else {
								EditorGUILayout.HelpBox("Run using Reflection", MessageType.Info);
							}
						} else {
							if(!GenerationUtility.IsGraphCompiled(root.target.gameObject)) {
								EditorGUILayout.HelpBox("Run using Reflection.", MessageType.Info);
							} else if(GenerationUtility.IsGraphUpToDate(root.target.gameObject)) {
								EditorGUILayout.HelpBox("Run using Native C#", MessageType.Info);
							} else {
								var boxRect = EditorGUILayout.BeginVertical();
								EditorGUILayout.HelpBox("Run using Native C# but script is outdated.\n[Click To Recompile]", MessageType.Warning);
								EditorGUILayout.EndVertical();
								if(Event.current.clickCount == 1 && Event.current.button == 0 && boxRect.Contains(Event.current.mousePosition)) {
									GraphUtility.SaveAllGraph(false);
									GenerationUtility.GenerateCSharpScript();
									Event.current.Use();
								}
							}
						}
					}
				} else {
					EditorGUILayout.HelpBox("The target graph is not supported.", MessageType.Warning);
				}
				if(!Application.isPlaying || root.runtimeBehaviour == null) {
					EditorGUILayout.BeginHorizontal();
					if(GUILayout.Button(new GUIContent("Edit Graph", ""), EditorStyles.toolbarButton)) {
						uNodeEditor.Open(root.target);
					}
					if(Application.isPlaying && root.runtimeInstance != null) {
						if(GUILayout.Button(new GUIContent("Live Debug", ""), EditorStyles.toolbarButton)) {
							uNodeEditor.Open(root.runtimeInstance);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			} else {
				EditorGUILayout.HelpBox("Please assign the target graph", MessageType.Error);
			}
		}

		protected override void OnHeaderGUI() {
			var obj = target as uNodeSpawner;
			if(obj.target != null) {
				var rect = EditorGUILayout.GetControlRect(false, 0f);
				rect.height = EditorGUIUtility.singleLineHeight;
#if UNITY_2019_3_OR_NEWER
				rect.y -= rect.height + 5;
				rect.x = 60;
#else
				rect.y -= rect.height;
				rect.x = 48;
#endif
				rect.xMax -= rect.x * 2f;

				EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? proColor : plebColor);

				string header = obj.target.GraphName;
				if(string.IsNullOrEmpty(header))
					header = target.ToString();

				EditorGUI.LabelField(rect, header, EditorStyles.boldLabel);
			} else {
				base.OnHeaderGUI();
			}
		}
	}
}