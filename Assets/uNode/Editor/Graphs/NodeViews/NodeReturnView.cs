using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(NodeReturn))]
	public class NodeReturnView : BaseNodeView {
		protected override void InitializeView() {
			base.InitializeView();
			// titleIcon.image = null;
			NodeReturn node = targetNode as NodeReturn;
			if(!node.returnAnyType) {
				var parent = node.parentComponent;
				if(parent != null && parent is uNodeProperty) {
					uNodeProperty m = parent as uNodeProperty;
					if(m.CanGetValue() && m.ReturnType() != typeof(void)) {
						AddInputValuePort(
							new PortData() {
								portID = "return",
								onValueChanged = (o) => {
									RegisterUndo();
									node.returnValue = o as MemberData;
								},
								getPortName = () => "",
								getPortType = () => m.ReturnType(),
								getPortValue = () => node.returnValue,
							}
						);
					}
				} else if(node.rootObject != null && node.rootObject is uNodeFunction) {
					uNodeFunction m = node.rootObject as uNodeFunction;
					if(m.ReturnType() != typeof(void)) {
						AddInputValuePort(
							new PortData() {
								portID = "return",
								onValueChanged = (o) => {
									RegisterUndo();
									node.returnValue = o as MemberData;
								},
								getPortName = () => "",
								getPortType = () => m.ReturnType(),
								getPortValue = () => node.returnValue,
							}
						);
					}
				}
			} else {
				AddInputValuePort(
					new PortData() {
						portID = "return",
						onValueChanged = (o) => {
							RegisterUndo();
							node.returnValue = o as MemberData;
						},
						getPortName = () => "",
						getPortType = () => typeof(object),
						getPortValue = () => node.returnValue,
					}
				);
			}
			if (uNodeUtility.preferredDisplay != DisplayKind.Full) {
				ConstructCompactTitle("return");
			}
		}
	}
}