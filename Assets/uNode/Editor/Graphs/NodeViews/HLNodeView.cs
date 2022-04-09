using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(HLNode))]
	public class HLNodeView : BaseNodeView {
		protected override void InitializeView() {
			HLNode node = targetNode as HLNode;
			node.Refresh();
			var instance = node.instance;
			if(instance == null) return;
			if(instance is IStateNode || instance is IStateCoroutineNode) {
				AddOutputFlowPort(
					new PortData() {
						portID = nameof(node.onSuccess),
						onValueChanged = (o) => {
							RegisterUndo();
							node.onSuccess = o as MemberData;
						},
						getPortName = () => "Success",
						getPortType = () => typeof(FlowInput),
						getPortValue = () => node.onSuccess,
						getPortTooltip = () => "Flow to execute on success",
					}
				);
				AddOutputFlowPort(
					new PortData() {
						portID = nameof(node.onFailure),
						onValueChanged = (o) => {
							RegisterUndo();
							node.onFailure = o as MemberData;
						},
						getPortName = () => "Failure",
						getPortType = () => typeof(FlowInput),
						getPortValue = () => node.onFailure,
						getPortTooltip = () => "Flow to execute on failure",
					}
				);
			}
			if (node.IsFlowNode()) {//Flow input
				nodeFlowPort = AddInputFlowPort(
					new PortData() {
						portID = UGraphView.SelfPortID,
						getPortName = () => "",
						getConnection = () => {
							return new MemberData(node, MemberData.TargetType.FlowNode);
						},
					}
				);
				AddOutputFlowPort(
					new PortData() {
						portID = "onFinished",
						onValueChanged = (o) => {
							RegisterUndo();
							node.onFinished = o as MemberData;
						},
						getPortName = () => instance is IStateNode || instance is IStateCoroutineNode ? "Finished" : "",
						getPortType = () => typeof(FlowInput),
						getPortValue = () => node.onFinished,
					}
				);
			}
			if (node.CanGetValue() || node.CanSetValue()) {//Value output
				nodeValuePort = AddOutputValuePort(
					new PortData() {
						portID = UGraphView.SelfPortID,
						getPortName = () => "Out",
						getPortType = () => node.ReturnType(),
						getConnection = () => {
							return new MemberData(node, MemberData.TargetType.ValueNode);
						},
					}
				);
			}
			Type type = instance.GetType();
			var fields = EditorReflectionUtility.GetFields(type);
			foreach(var field in fields) {
				if(field.IsDefined(typeof(NonSerializedAttribute)) || field.IsDefined(typeof(HideAttribute))) continue;
				var option = field.GetCustomAttribute(typeof(NodePortAttribute), true) as NodePortAttribute;
				if(option != null && option.hideInNode) continue;
				var val = node.initializers.FirstOrDefault(d => d.name == field.Name);
				if(val == null) {
					val = new FieldValueData() {
						name = field.Name,
						value = MemberData.CreateFromValue(field.GetValue(instance), field.FieldType),
					};
					node.initializers.Add(val);
				}
				AddInputValuePort(
					new PortData() {
						portID = field.Name,
						onValueChanged = (o) => {
							RegisterUndo();
							val.value = o as MemberData;
						},
						getPortName = () => option != null ? option.name : field.Name,
						getPortType = () => field.FieldType,
						getPortValue = () => val.value,
						getPortTooltip = () => {
							var tooltip = field.GetCustomAttribute(typeof(TooltipAttribute), true) as TooltipAttribute;
							return tooltip != null ? tooltip.tooltip : string.Empty;
						},
					});
			}
		}
	}
}