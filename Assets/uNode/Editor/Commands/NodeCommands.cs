using System.Collections.Generic;
using UnityEngine;
using MaxyGames.uNode.Nodes;
using MaxyGames.Events;
using UnityEditor;

namespace MaxyGames.uNode.Editors.Commands {
	public class ConvertNotNodeToConditionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Condition";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			NotNode node = source as NotNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ConditionNode n) => {
				n.Condition.AddBlockRange(BlockUtility.GetConditionBlockFromNode(source));
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is NotNode) {
				return true;
			}
			return false;
		}
	}
	
	public class ConvertToMultiArithmeticCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To multi Arithmetic";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			ArithmeticNode node = source as ArithmeticNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiArithmeticNode n) => {
				n.targets[0] = node.targetA;
				n.targets[1] = node.targetB;
				n.operatorType = node.operatorType;
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetValueNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is ArithmeticNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertComparisonToConditionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Condition";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			ComparisonNode node = source as ComparisonNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ConditionNode n) => {
				n.Condition.AddBlockRange(BlockUtility.GetConditionBlockFromNode(source));
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is ComparisonNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertToMultiANDCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To multi AND";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			ANDNode node = source as ANDNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiANDNode n) => {
				n.targets[0] = node.targetA;
				n.targets[1] = node.targetB;
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetValueNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is ANDNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertAndNodeToConditionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Condition";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			MultiANDNode node = source as MultiANDNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ConditionNode n) => {
				n.Condition.AddBlockRange(BlockUtility.GetConditionBlockFromNode(source));
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is MultiANDNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertToMultiORCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To multi OR";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			ORNode node = source as ORNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultiORNode n) => {
				n.targets[0] = node.targetA;
				n.targets[1] = node.targetB;
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetValueNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is ORNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertOrNodeToConditionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Condition";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			MultiORNode node = source as MultiORNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ConditionNode n) => {
				n.Condition.AddBlockRange(BlockUtility.GetConditionBlockFromNode(source));
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is MultiORNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertMacroCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Linked Macro";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			MacroNode node = source as MacroNode;
			string path = EditorUtility.SaveFilePanelInProject("Export to macro asset",
				"New Macro.prefab",
				"prefab",
				"Please enter a file name to save the macro to");
			if(path.Length != 0) {
				Undo.RegisterFullObjectHierarchyUndo(node, "");
				Undo.RegisterFullObjectHierarchyUndo(node.owner, "");
				var tmpMacro = Object.Instantiate(node);
				GameObject go = new GameObject("New Macro");
				var macro = go.AddComponent<uNodeMacro>();
				macro.Variables.AddRange(tmpMacro.Variables);
				if(macro.RootObject == null) {
					macro.RootObject = new GameObject("Root");
					macro.RootObject.transform.SetParent(macro.transform);
				}
				var behaviors = tmpMacro.GetComponentsInChildren<MonoBehaviour>(true);
				AnalizerUtility.RetargetNodeOwner(tmpMacro.owner, macro, behaviors, (obj) => {
					MemberData member = obj as MemberData;
					if(member != null && member.targetType == MemberData.TargetType.uNodeVariable && member.GetInstance() as Object == tmpMacro) {
						member.RefactorUnityObject(new Object[] { tmpMacro }, new Object[] { macro });
					}
				});
				for(int i = 0; i < tmpMacro.transform.childCount;i++) {
					tmpMacro.transform.GetChild(i).SetParent(macro.RootObject.transform);
					i--;
				}
				macro.Refresh();
#if UNITY_2018_3_OR_NEWER
				GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
#else
				GameObject prefab = PrefabUtility.CreatePrefab(path, go);
#endif
				AssetDatabase.SaveAssets();
				Object.DestroyImmediate(go);
				Object.DestroyImmediate(tmpMacro.gameObject);
				var macroAsset = prefab.GetComponent<uNodeMacro>();
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (LinkedMacroNode n) => {
					n.macroAsset = macroAsset;
					n.editorRect = node.editorRect;
					NodeEditorUtility.AddNewObject(graph.editorData.graph, "pins", n.transform, (pin) => {
						n.pinObject = pin;
						n.Refresh();
						RefactorUtility.RetargetNode(node, n);
						if(n.inputFlows.Count == node.inputFlows.Count) {
							for(int i = 0; i < n.inputFlows.Count; i++) {
								RefactorUtility.RetargetNode(node.inputFlows[i], n.inputFlows[i]);
							}
						}
						if(n.inputValues.Count == node.inputValues.Count) {
							for(int i = 0; i < n.inputValues.Count; i++) {
								n.inputValues[i].target = new MemberData(node.inputValues[i].target);
							}
						}
						if(n.outputFlows.Count == node.outputFlows.Count) {
							for(int i = 0; i < n.outputFlows.Count; i++) {
								n.outputFlows[i].target = new MemberData(node.outputFlows[i].target);
							}
						}
						if(n.outputValues.Count == node.outputValues.Count) {
							for(int i = 0; i < n.outputValues.Count; i++) {
								RefactorUtility.RetargetNode(node.outputValues[i], n.outputValues[i]);
							}
						}
					});
				});
				NodeEditorUtility.RemoveNode(graph.editorData, node);
			}
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is MacroNode) {
				return true;
			}
			return false;
		}
	}

	public class ConvertSetValueToActionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Action";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			NodeSetValue node = source as NodeSetValue;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (NodeAction n) => {
				n.Action.AddBlockRange(BlockUtility.GetActionBlockFromNode(source));
				n.onFinished = node.onFinished;
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is NodeSetValue) {
				return true;
			}
			return false;
		}
	}

	public class ConvertMultipurposeToActionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Action";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			MultipurposeNode node = source as MultipurposeNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (NodeAction n) => {
				n.Action.AddBlockRange(BlockUtility.GetActionBlockFromNode(source));
				n.onFinished = node.onFinished;
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is MultipurposeNode) {
				MultipurposeNode node = source as MultipurposeNode;
				if(node.IsFlowNode()) {
					if(node.CanGetValue() || node.CanSetValue()) {
						var nodes = NodeEditorUtility.FindConnectedNodeToValueNode(node);
						if(nodes.Count > 0) {
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}
	}

	public class ConvertAsToGetComponentCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To GetComponent";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			var node = source as ASNode;
			var type = node.ReturnType();
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (GetComponentNode n) => {
				n.type = MemberData.CreateFromType(type);
				n.target = node.target;
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is ASNode) {
				var node = source as ASNode;
				var type = node.ReturnType();
				if(type != null && type.IsCastableTo(typeof(Component))) {
					return true;
				}
			}
			return false;
		}
	}

	public class ConvertMultipurposeToConditionCommands : NodeMenuCommand {
		public override string name {
			get {
				return "Convert/To Condition";
			}
		}

		public override void OnClick(Node source, Vector2 mousePosition) {
			MultipurposeNode node = source as MultipurposeNode;
			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ConditionNode n) => {
				n.Condition.AddBlockRange(BlockUtility.GetConditionBlockFromNode(source));
				n.editorRect = node.editorRect;
				RefactorUtility.RetargetNode(node, n);
			});
			NodeEditorUtility.RemoveNode(graph.editorData, node);
			graph.Refresh();
		}

		public override bool IsValidNode(Node source) {
			if(source is MultipurposeNode) {
				MultipurposeNode node = source as MultipurposeNode;
				if(node.CanGetValue() && node.ReturnType() == typeof(bool)) {
					if(node.IsFlowNode()) {
						var nodes = NodeEditorUtility.FindConnectedNodeToFlowNode(node);
						if(nodes.Count > 0) {
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}
	}

	//public class GetInstanceNodeCommands : NodeMenuCommand {
	//	public override string name {
	//		get {
	//			return "Get instance";
	//		}
	//	}

	//	public override void OnClick(Node source, Vector2 mousePosition) {
	//		var rType = source.ReturnType();
	//		FilterAttribute filter = new FilterAttribute {
	//			VoidType = true,
	//			MaxMethodParam = int.MaxValue,
	//			Public = true,
	//			Instance = true,
	//			Static = false,
	//			UnityReference = false,
	//			InvalidTargetType = MemberData.TargetType.Null | MemberData.TargetType.Values,
	//			// DisplayDefaultStaticType = false
	//		};
	//		List<ItemSelector.CustomItem> customItems = null;
	//		if(rType.IsCastableTo(typeof(IGraphSystem)) && source is MultipurposeNode) {
	//			MultipurposeNode multipurposeNode = source as MultipurposeNode;
	//			if(multipurposeNode.target != null && multipurposeNode.target.target != null && (multipurposeNode.target.target.targetType == MemberData.TargetType.SelfTarget || multipurposeNode.target.target.targetType == MemberData.TargetType.Values)) {
	//				var sTarget = multipurposeNode.target.target.startTarget;
	//				if(sTarget is IGraphSystem) {
	//					customItems = ItemSelector.MakeCustomItems(sTarget as Object);
	//					customItems.AddRange(ItemSelector.MakeCustomItems(typeof(uNodeRoot), sTarget, filter, "Inherit Member"));
	//				}
	//			}
	//		}
	//		bool flag = false;
	//		if(customItems == null) {
	//			if(rType is RuntimeType) {
	//				customItems = ItemSelector.MakeCustomItems((rType as RuntimeType).GetRuntimeMembers(), filter);
	//				if(rType.BaseType != null)
	//					customItems.AddRange(ItemSelector.MakeCustomItems(rType.BaseType, filter, "Inherit Member"));
	//			} else {
	//				customItems = ItemSelector.MakeCustomItems(rType, filter, " " + rType.PrettyName());
	//			}
	//			var usedNamespaces = source.owner.GetNamespaces().ToHashSet();
	//			if(usedNamespaces != null) {
	//				customItems.AddRange(ItemSelector.MakeExtensionItems(rType, usedNamespaces, filter, "Extensions"));
	//				flag = true;
	//			}

	//			var customInputItems = NodeEditorUtility.FindCustomInputPortItems();
	//			if(customInputItems != null && customInputItems.Count > 0) {
	//				var mData = new MemberData(source, MemberData.TargetType.ValueNode);
	//				foreach(var c in customInputItems) {
	//					c.graph = graph;
	//					c.mousePositionOnCanvas = mousePositionOnCanvas;
	//					if(c.IsValidPort(rType, PortAccessibility.OnlyGet)) {
	//						var items = c.GetItems(source, mData, rType);
	//						if(items != null) {
	//							customItems.AddRange(items);
	//						}
	//					}
	//				}
	//			}
	//		}
	//		if(customItems != null) {
	//			filter.Static = false;
	//			customItems.Sort((x, y) => {
	//				if(x.category != y.category) {
	//					return string.Compare(x.category, y.category, System.StringComparison.OrdinalIgnoreCase);
	//				}
	//				return string.Compare(x.name, y.name, System.StringComparison.OrdinalIgnoreCase);
	//			});
	//			ItemSelector w = ItemSelector.ShowWindow(source, MemberData.none, filter, delegate (MemberData value) {
	//				flag = flag && value.targetType == MemberData.TargetType.Method && !rType.IsCastableTo(value.startType);
	//				if(!flag && !value.isStatic) {
	//					value.instance = new MemberData(source, MemberData.TargetType.ValueNode);
	//				}
	//				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (MultipurposeNode n) {
	//					if(n.target == null) {
	//						n.target = new MultipurposeMember();
	//					}
	//					n.target.target = value;
	//					MemberDataUtility.UpdateMultipurposeMember(n.target);
	//					if(flag) {
	//						var pTypes = value.ParameterTypes;
	//						if(pTypes != null) {
	//							int paramIndex = 0;
	//							MemberData param = null;
	//							for (int i = 0; i < pTypes.Length;i++){
	//								var types = pTypes[i];
	//								if(types != null) {
	//									for (int y = 0; y < types.Length;y++) {
	//										if(rType.IsCastableTo(types[y])) {
	//											param = new MemberData(source, MemberData.TargetType.ValueNode);
	//											break;
	//										}
	//										paramIndex++;
	//									}
	//									if(param != null) break;
	//								}
	//							}
	//							if(n.target.parameters.Length > paramIndex && param != null) {
	//								n.target.parameters[paramIndex] = param;
	//							}
	//						}
	//					}
	//				});
	//				graph.Refresh();
	//			}, customItems).ChangePosition(GUIUtility.GUIToScreenPoint(mousePosition));
	//			w.displayRecentItem = false;
	//			w.displayNoneOption = false;
	//		}
	//	}

	//	public override bool IsValidNode(Node source) {
	//		if(source.CanGetValue()) {
	//			return true;
	//		}
	//		return false;
	//	}
	//}

	//public class SetInstanceFieldNodeCommands : NodeMenuCommand {
	//	public override string name {
	//		get {
	//			return "Set instance field";
	//		}
	//	}

	//	public override void OnClick(Node source, Vector2 mousePosition) {
	//		FilterAttribute filter = new FilterAttribute();
	//		filter.HideTypes.Add(typeof(void));
	//		filter.MaxMethodParam = int.MaxValue;
	//		filter.SetMember = true;
	//		filter.Public = true;
	//		filter.Instance = true;
	//		filter.Static = false;
	//		filter.DisplayDefaultStaticType = false;
	//		var customItems = ItemSelector.MakeCustomItems(source.ReturnType(), filter, source.ReturnType().PrettyName());
	//		if(customItems != null) {
	//			ItemSelector w = ItemSelector.ShowWindow(source, MemberData.none, filter, delegate (MemberData value) {
	//				value.instance = new MemberData(source, MemberData.TargetType.ValueNode);
	//				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (MultipurposeNode n) => {
	//					if(n.target == null) {
	//						n.target = new MultipurposeMember();
	//					}
	//					n.target.target = value;
	//					MemberDataUtility.UpdateMultipurposeMember(n.target);
	//					NodeEditorUtility.AddNewNode(graph.editorData, null, null,
	//						new Vector2(mousePositionOnCanvas.x + n.editorRect.width + 150, mousePositionOnCanvas.y),
	//						(NodeSetValue SV) => {
	//							SV.target = new MemberData(n, MemberData.TargetType.ValueNode);
	//						});
	//				});
	//				graph.Refresh();
	//			}, customItems).ChangePosition(GUIUtility.GUIToScreenPoint(mousePosition));
	//			w.displayDefaultItem = false;
	//		}
	//	}

	//	public override bool IsValidNode(Node source) {
	//		if(source.CanGetValue()) {
	//			return true;
	//		}
	//		return false;
	//	}
	//}

	//public class SetInstanceNodeCommands : NodeMenuCommand {
	//	public override string name {
	//		get {
	//			return "Set instance";
	//		}
	//	}

	//	public override void OnClick(Node source, Vector2 mousePosition) {
	//		var type = source.ReturnType();
	//		if(type.IsSubclassOf(typeof(System.MulticastDelegate))) {
	//			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (EventHook n) {
	//				n.target = new MemberData(source, MemberData.TargetType.ValueNode);
	//			});
	//		} else {
	//			NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (NodeSetValue n) {
	//				n.target = new MemberData(source, MemberData.TargetType.ValueNode);
	//				if (type.IsSubclassOf(typeof(System.MulticastDelegate))) {
	//					NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, delegate (NodeLambda node) {
	//						n.value = new MemberData(node, MemberData.TargetType.ValueNode);
	//						n.setType = SetType.Add;
	//						node.delegateType = MemberData.CreateFromType(type);
	//					});
	//				} else {
	//					n.value = MemberData.CreateValueFromType(type);
	//				}
	//			});
	//		}
	//		graph.Refresh();
	//	}

	//	public override bool IsValidNode(Node source) {
	//		if(source.CanSetValue()) {
	//			return true;
	//		}
	//		return false;
	//	}
	//}

	//public class CacheInstanceNodeCommands : NodeMenuCommand {
	//	public override string name {
	//		get {
	//			return "Cache instance";
	//		}
	//	}

	//	public override void OnClick(Node source, Vector2 mousePosition) {
	//		NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (CacheNode n) => {
	//			n.target = MemberData.ValueOutput(source);
	//		});
	//		graph.Refresh();
	//	}

	//	public override bool IsValidNode(Node source) {
	//		if(source.CanGetValue()) {
	//			return true;
	//		}
	//		return false;
	//	}
	//}
}