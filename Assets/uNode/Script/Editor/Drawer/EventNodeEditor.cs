using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(BaseEventNode), true)]
	public class EventNodeEditor : Editor {
		public override void OnInspectorGUI() {
			BaseEventNode comp = target as BaseEventNode;
			DrawDefaultEditor();
			DrawFooter(comp);
		}

		protected void DrawDefaultEditor() {
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			if(EditorGUI.EndChangeCheck()) {
				uNodeGUIUtility.GUIChanged(target);
			}
		}

		protected void DrawFooter(BaseEventNode node) {
			EditorGUI.BeginChangeCheck();
			var flows = node.GetFlows();
			var num = EditorGUILayout.IntSlider(new GUIContent("Flows"), flows.Count, 1, 10);
			if(num != flows.Count && num > 0) {
				while(flows.Count != num) {
					if(num > flows.Count) {
						flows.Add(MemberData.none);
					} else {
						flows.RemoveAt(flows.Count - 1);
					}
				}
			}
			if(node is EventNode) {
				var enode = node as EventNode;
				if(enode.eventType != EventNode.EventType.Awake && enode.eventType != EventNode.EventType.Custom && enode.eventType != EventNode.EventType.Start) {
					var num1 = EditorGUILayout.IntSlider(new GUIContent("Targets"), enode.targetObjects.Length, 0, 10);
					if(num1 != enode.targetObjects.Length && num1 >= 0) {
						while(enode.targetObjects.Length != num1) {
							if(num1 > enode.targetObjects.Length) {
								uNodeUtility.AddArray(ref enode.targetObjects, MemberData.none);
							} else {
								uNodeUtility.RemoveArrayAt(ref enode.targetObjects, enode.targetObjects.Length - 1);
							}
						}
					}
				}
			}
			if(EditorGUI.EndChangeCheck()) {
				uNodeGUIUtility.GUIChanged(node);
			}
			if(node.GetType().IsDefinedAttribute(typeof(DescriptionAttribute))) {
				DescriptionAttribute descriptionEvent = (DescriptionAttribute)node.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
				if(descriptionEvent.description != null && descriptionEvent != null) {
					GUI.backgroundColor = Color.yellow;
					EditorGUILayout.HelpBox("Description: " + descriptionEvent.description, MessageType.None);
					GUI.backgroundColor = Color.white;
				}
			}
		}
	}
}