using System;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.EventHook))]
	public class EventHookView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			Nodes.EventHook node = targetNode as Nodes.EventHook;
			if(node.target.isAssigned) {
				Type targetType = node.target.type;
				Type[] parameterTypes = null;
				ParameterInfo[] parameterInfos = null;
				if(targetType == null) return;
				if(targetType.IsCastableTo(typeof(Delegate))) {
					parameterInfos = targetType.GetMethod("Invoke").GetParameters();
					parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
				} else if(targetType.IsCastableTo(typeof(UnityEventBase))) {
					var method = targetType.GetMethod("AddListener");
					parameterTypes = method.GetParameters()[0].ParameterType.GetGenericArguments();
				}
				if(parameterTypes == null)
					return;
				while(node.parameters.Count != parameterTypes.Length) {
					if(node.parameters.Count > parameterTypes.Length) {
						node.parameters.RemoveAt(node.parameters.Count - 1);
					} else {
						node.parameters.Add(null);
					}
				}
				for(int i = 0; i < parameterTypes.Length; i++) {
					int x = i;
					Type type = parameterTypes[i];
					AddOutputValuePort(
						new PortData() {
							portID = "parameters#" + x,
							getPortName = () => parameterInfos != null ? parameterInfos[x].Name : "Parameter " + x,
							getPortType = () => type,
							getConnection = () => {
								return MemberData.ValueOutput(node, "parameters", x, type);
							},
						}
					);
				}
			}
		}

		public override void OnValueChanged() {
			//Ensure to repaint every value changed.
			MarkRepaint();
		}
	}
}