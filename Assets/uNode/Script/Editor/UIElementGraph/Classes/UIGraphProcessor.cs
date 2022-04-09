using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public abstract class UIGraphProcessor {
		public virtual int order => 0;

		public virtual bool Delete(List<ISelectable> selected) {
			return false;
		}

		public virtual bool Connect(UGraphView graph, PortView input, PortView output) {
			return false;
		}

		public virtual bool RepaintNode(UGraphView graph, NodeComponent node, bool fullReload) {
			return false;
		}

		public virtual EdgeView InitializeEdge(UGraphView graph, PortView port, MemberData portValue, UNodeView node) {
			return null;
		}
	}

	class DefaultUIGraphProcessor : UIGraphProcessor {
		public override int order => int.MaxValue;

		public override bool RepaintNode(UGraphView graph, NodeComponent node, bool fullReload) {
			if(node is Nodes.NodeValueConverter) {
				var n = node as Nodes.NodeValueConverter;
				var tNode = n.target.GetTargetNode();
				if(tNode != null) {
					return false;
				} else {
					NodeEditorUtility.RemoveNode(graph.editorData, n);
				}
			}
			return false;
		}

		public override EdgeView InitializeEdge(UGraphView graph, PortView port, MemberData portValue, UNodeView node) {
			var tNode = portValue.GetTargetNode() as Nodes.NodeValueConverter;
			if(tNode != null) {
				if(port.direction == Direction.Input) {
					tNode.type = port.GetPortType();
					var nView = graph.GetNodeView(tNode);
					if(nView != null) {
						tNode.editorRect.position = new Vector2(node.targetNode.editorRect.position.x - 50, node.targetNode.editorRect.position.y);
						nView.HideElement();
						nView.SetDisplay(false);
					}
					return new ConversionEdgeView(tNode, port, PortUtility.GetOutputPort(tNode.target, node.owner));
				}
			}
			return null;
		}

		public override bool Connect(UGraphView graph, PortView input, PortView output) {
			if(output.isValue) {
				var outputNode = output.GetNode();
				if(outputNode is MultipurposeNode mNode) {
					var connecteds = UIElementUtility.FindConnectedFlowNodes(output.owner);
					if(connecteds.Contains(input.owner)) {
						if(EditorUtility.DisplayDialog("", "Value can be cached for get better performance\nDo you want to cache the value?", "Yes", "No")) {
							NodeEditorUtility.AddNewNode(graph.editorData, output.owner.GetPosition().position, (Nodes.CacheNode node) => {
								input.ChangeValue(MemberData.ValueOutput(node));
								node.target = MemberData.ValueOutput(mNode);
								node.onFinished = new MemberData(mNode.onFinished);
								mNode.onFinished = MemberData.none;
								mNode.editorRect.x = node.editorRect.x - node.editorRect.width - 100;
								foreach(var port in output.owner.inputPorts) {
									if(port.connected && port.isFlow) {
										var edges = port.GetValidEdges();
										foreach(var e in edges) {
											e.Output.ChangeValue(MemberData.FlowInput(node));
										}
									}
								}
							});
							return true;
						}
					}
				}
			}
			return false;
		}

		public override bool Delete(List<ISelectable> selected) {
			HashSet<UNodeView> nodes = new HashSet<UNodeView>();
			for(int i = 0; i < selected.Count; i++) {
				if(selected[i] is UNodeView) {
					nodes.Add(selected[i] as UNodeView);
				}
			}
			if(nodes.Count > 0) {
				Action action = null;
				foreach(var node in nodes) {
					var inputPort = UIElementUtility.GetDefaultFlowPort(node);
					var outputPort = UIElementUtility.GetFinishFlowPort(node);
					if(inputPort != null && outputPort != null) {
						//Flow auto re-connection.
						var targetInput = inputPort.GetConnectedPorts();
						var targetOutput = outputPort.GetConnectedPorts().FirstOrDefault();
						if(targetInput.Count > 0 && targetOutput != null) {
							node.RegisterUndo();
							foreach(var p in targetInput) {
								p.owner.RegisterUndo();
								action += () => {
									p.ChangeValue(targetOutput.GetConnection());
								};
							}
						}
					}
					if(node.targetNode is Nodes.NodeReroute) {
						//Reroute auto re-connection
						inputPort = node.inputPorts.FirstOrDefault();
						outputPort = node.outputPorts.FirstOrDefault();
						if(inputPort != null && outputPort != null) {
							if(inputPort.isValue) {
								var inputEdge = inputPort.GetValidEdges().FirstOrDefault();
								var targetOutput = outputPort.GetConnectedPorts();
								if(targetOutput.Count > 0 && inputEdge != null) {
									node.RegisterUndo();
									foreach(var p in targetOutput) {
										p.owner.RegisterUndo();
										action += () => {
											if(inputEdge is ConversionEdgeView) {
												p.ChangeValue(MemberData.ValueOutput((inputEdge as ConversionEdgeView).node));
											} else {
												p.ChangeValue(inputEdge.GetReceiverPort().GetConnection());
											}
										};
									}
								}
							}
						}
					}
				}
				action?.Invoke();
			}
			return false;
		}
	}
}