using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public static class PortUtility {
		public static PortView GetInputPort(MemberData outputValue, UGraphView graphView) {
			var inputNode = outputValue.GetTargetNode();
			if(inputNode != null) {
				if(outputValue.IsTargetingNode) {
					switch(outputValue.targetType) {
						case MemberData.TargetType.FlowNode:
							PortView port;
							if(graphView.portFlowNodeAliases.TryGetValue(inputNode, out port) && port != null) {
								return port;
							}
							return GetSelfPort(inputNode, true, graphView);
						case MemberData.TargetType.ValueNode:
							return GetSelfPort(inputNode, false, graphView);
					}
				}
				UNodeView nodeView;
				if(graphView.nodeViewsPerNode.TryGetValue(inputNode, out nodeView)) {
					string portID;
					switch(outputValue.targetType) {
						case MemberData.TargetType.FlowInputExtended:
							portID = "in." + outputValue.startName;
							foreach(var p in nodeView.inputPorts) {
								if(p.portData.portID == portID) {
									return p;
								}
							}
							break;
						default:
							portID = outputValue.startName;
							foreach(var p in nodeView.inputPorts) {
								if(p.portData.portID == portID) {
									return p;
								}
							}
							break;
					}
				}
			}
			return null;
		}

		public static PortView GetOutputPort(MemberData inputValue, UGraphView graphView) {
			var outputNode = inputValue.GetTargetNode();
			if(outputNode != null) {
				if(inputValue.IsTargetingNode) {
					switch(inputValue.targetType) {
						case MemberData.TargetType.FlowNode:
							return GetSelfPort(outputNode, true, graphView);
						case MemberData.TargetType.ValueNode:
							PortView port;
							if(graphView.portValueNodeAliases.TryGetValue(outputNode, out port) && port != null) {
								return port;
							}
							return GetSelfPort(outputNode, false, graphView);
					}
				}
				UNodeView nodeView;
				if(graphView.nodeViewsPerNode.TryGetValue(outputNode, out nodeView)) {
					string portID;
					switch(inputValue.targetType) {
						case MemberData.TargetType.NodeOutputValue:
							portID = "out." + inputValue.startName;
							foreach(var p in nodeView.outputPorts) {
								if(p.portData.portID == portID) {
									return p;
								}
							}
							break;
						default:
							portID = inputValue.startName;
							foreach(var p in nodeView.outputPorts) {
								if(p.portData.portID == portID) {
									return p;
								}
							}
							break;
					}
				}
			}
			return null;
		}

		public static PortView GetSelfPort(NodeComponent node, bool isFlow, UGraphView graphView) {
			UNodeView nodeView;
			if(graphView.nodeViewsPerNode.TryGetValue(node, out nodeView)) {
				if(isFlow) {
					foreach(var p in nodeView.inputPorts) {
						if(p.isFlow && p.portData.portID == UGraphView.SelfPortID) {
							return p;
						}
					}
				} else {
					foreach(var p in nodeView.outputPorts) {
						if(p.isValue && p.portData.portID == UGraphView.SelfPortID) {
							return p;
						}
					}
				}
			}
			return null;
		}
	}
}