using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(NodeComponent), true)]
	class NodeComponentEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			NodeComponent comp = target as NodeComponent;
			if(comp == null)
				return;
			EditorGUI.BeginChangeCheck();
			if(comp is MacroNode) {
				MacroNode node = comp as MacroNode;
				VariableEditorUtility.DrawVariable(node.variables, node,
					(v) => {
						node.variables = v;
					}, null);

				VariableEditorUtility.DrawCustomList(
					node.inputFlows,
					"Input Flows",
					(position, index, element) => {//Draw Element
						EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
					}, null, null);
				VariableEditorUtility.DrawCustomList(
					node.outputFlows,
					"Output Flows",
					(position, index, element) => {//Draw Element
						EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
					}, null, null);
				VariableEditorUtility.DrawCustomList(
					node.inputValues,
					"Input Values",
					(position, index, element) => {//Draw Element
						position.width -= EditorGUIUtility.labelWidth;
						EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(element.ReturnType())));
						position.x += EditorGUIUtility.labelWidth;
						uNodeGUIUtility.EditValue(position, GUIContent.none, "type", element, element);
					}, null, null);
				VariableEditorUtility.DrawCustomList(
					node.outputValues,
					"Output Values",
					(position, index, element) => {//Draw Element
						position.width -= EditorGUIUtility.labelWidth;
						EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(element.ReturnType())));
						position.x += EditorGUIUtility.labelWidth;
						uNodeGUIUtility.EditValue(position, GUIContent.none, "type", element, element);
					}, null, null);
			} else if(comp is LinkedMacroNode) {
				LinkedMacroNode node = comp as LinkedMacroNode;
				if(node.macroAsset != null) {
					VariableEditorUtility.DrawLinkedVariables(node, node.macroAsset, publicOnly: false);
					EditorGUI.BeginDisabledGroup(true);
					VariableEditorUtility.DrawCustomList(
						node.inputFlows,
						"Input Flows",
						(position, index, element) => {//Draw Element
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
						}, null, null);
					VariableEditorUtility.DrawCustomList(
						node.outputFlows,
						"Output Flows",
						(position, index, element) => {//Draw Element
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
						}, null, null);
					VariableEditorUtility.DrawCustomList(
						node.inputValues,
						"Input Values",
						(position, index, element) => {//Draw Element
							position.width -= EditorGUIUtility.labelWidth;
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(element.ReturnType())));
							position.x += EditorGUIUtility.labelWidth;
							uNodeGUIUtility.EditValue(position, GUIContent.none, "type", element, element);
						}, null, null);
					VariableEditorUtility.DrawCustomList(
						node.outputValues,
						"Output Values",
						(position, index, element) => {//Draw Element
							position.width -= EditorGUIUtility.labelWidth;
							EditorGUI.LabelField(position, new GUIContent(element.GetName(), uNodeEditorUtility.GetTypeIcon(element.ReturnType())));
							position.x += EditorGUIUtility.labelWidth;
							uNodeGUIUtility.EditValue(position, GUIContent.none, "type", element, element);
						}, null, null);
					EditorGUI.EndDisabledGroup();
				}
			}
			if(comp.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false).Length > 0) {
				DescriptionAttribute descriptionEvent = (DescriptionAttribute)comp.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false)[0];
				if(descriptionEvent.description != null && descriptionEvent != null) {
					GUI.backgroundColor = Color.yellow;
					EditorGUILayout.HelpBox("Description: " + descriptionEvent.description, MessageType.None);
					GUI.backgroundColor = Color.white;
				}
			}
			if(EditorGUI.EndChangeCheck()) {
				uNodeEditorUtility.MarkDirty(comp);
				uNodeGUIUtility.GUIChanged(comp);
			}
		}
	}
}