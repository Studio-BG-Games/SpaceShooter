using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.NodeLambda))]
	public class NodeLambdaView : BaseNodeView {
		protected override void InitializeView() {
			InitializeDefaultPort();
			Nodes.NodeLambda node = targetNode as Nodes.NodeLambda;
			var delegateType = node.delegateType.Get<Type>();
			if(delegateType == null) return;
			var methodInfo = delegateType.GetMethod("Invoke");
			var parameters = methodInfo?.GetParameters();
			if(methodInfo == null || parameters == null) return;
			while(node.parameterValues.Count != parameters.Length) {
				if(node.parameterValues.Count > parameters.Length) {
					node.parameterValues.RemoveAt(node.parameterValues.Count - 1);
				} else {
					node.parameterValues.Add(null);
				}
			}
			for(int i = 0; i < parameters.Length; i++) {
				int x = i;
				Type type = parameters[i].ParameterType;
				//var port = 
				AddOutputValuePort(
					new PortData() {
						portID = nameof(node.parameterValues) + "#" + x,
						getPortName = () => $"P {x}",
						getPortType = () => type,
						getConnection = () => {
							return MemberData.ValueOutput(node, nameof(node.parameterValues), x, type);
						},
					}
				);
			}
			if(methodInfo.ReturnType == typeof(void)) {
				AddOutputFlowPort(nameof(node.body));
			} else {
				AddInputValuePort(nameof(node.input), () => methodInfo.ReturnType);
			}
		}
	}
}