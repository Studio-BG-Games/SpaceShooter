using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public class PortView : Port, IEdgeConnectorListener {
		public new Type portType;
		/// <summary>
		/// The owner of the port
		/// </summary>
		public UNodeView owner => portData?.owner;

		public new string portName {
			get {
				return base.portName;
			}
			set {
				base.portName = value;
				m_ConnectorText.EnableInClassList("ui-hidden", string.IsNullOrEmpty(value) && controlView == null);
			}
		}

		public event Action<PortView, Edge> OnConnected;
		public event Action<PortView, Edge> OnDisconnected;
		public PortData portData;

		protected ControlView controlView;
		protected Image portIcon;
		protected bool displayProxyTitle = true;

		//private static CustomStyleProperty<int> s_ProxyOffsetX = new CustomStyleProperty<int>("--proxy-offset-x");
		//private static CustomStyleProperty<int> s_ProxyOffsetY = new CustomStyleProperty<int>("--proxy-offset-y");

		List<EdgeView> edges = new List<EdgeView>();

		/// <summary>
		/// True if the port is flow port
		/// </summary>
		public bool isFlow => portData.isFlow;
		/// <summary>
		/// True if the port is value port
		/// </summary>
		public bool isValue => !portData.isFlow;

		#region Initialization
		public PortView(Orientation portOrientation, Direction direction, PortData portData)
			: base(portOrientation, direction, Capacity.Multi, typeof(object)) {
			this.portData = portData;
			portData.port = this;
			this.AddStyleSheet("uNodeStyles/NativePortStyle");
			this.AddStyleSheet(UIElementUtility.Theme.portStyle);
			if(portData.isFlow) {
				AddToClassList("flow-port");
				if(owner.owner.graphLayout == GraphLayout.Vertical) {
					AddToClassList("flow-vertical");
				} else {
					AddToClassList("flow-horizontal");
				}
			} else {
				AddToClassList("value-port");
			}

			m_EdgeConnector = new EdgeConnector<EdgeView>(this);
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			this.AddManipulator(m_EdgeConnector);
			DoUpdate();
			this.ExecuteAndScheduleAction(DoUpdate, 1000);
		}

		public virtual void Initialize(PortData portData) {
			if(portData.owner == null) {
				throw new Exception("The port owner must be assigned.");
			}
			this.portData = portData;
			ReloadView(true);
		}

		public virtual void ReloadView(bool refreshName = false) {
			if(portData != null) {
				portType = portData.GetPortType();
				if(refreshName) {
					portName = ObjectNames.NicifyVariableName(portData.GetPortName());
					if(isFlow) {
						tooltip = "Flow";
					} else {
						tooltip = portType.PrettyName(true);
						var filter = portData.GetFilter();
						if(filter.Types != null && filter.Types.Count > 1) {
							tooltip += "\n\nType Filter:\n" + filter.Tooltip;
						}
					}
				}
			}
			if(isValue) {
				//portColor = new Color(0.09f, 0.7f, 0.4f);
				portColor = uNodePreference.GetColorForType(portType);
				if (portIcon == null) {
					portIcon = new Image();
					Insert(1, portIcon);
				}
				portIcon.image = uNodeEditorUtility.GetTypeIcon(portType);
				// portIcon.style.width = 16;
				// portIcon.style.height = 16;
				portIcon.pickingMode = PickingMode.Ignore;
			} else if(owner.owner.graphLayout == GraphLayout.Horizontal) {
				if(portIcon == null) {
					portIcon = new Image();
					Insert(1, portIcon);
				}
				portIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon));
				portIcon.pickingMode = PickingMode.Ignore;
			}
			UpdatePortClass();
		}

		private void UpdatePortClass() {
			if(connected)
				AddToClassList("connected");
			else
				RemoveFromClassList("connected");

			switch(direction) {
				case Direction.Input: {
					EnableInClassList("input", true);
					EnableInClassList("output", false);
				}
				break;
				case Direction.Output:
					EnableInClassList("input", false);
					EnableInClassList("output", true);
					break;
			}
		}
		#endregion

		public virtual void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

		}

		public void DisplayProxyTitle(bool value) {
			displayProxyTitle = value;
			proxyContainer?.RemoveFromHierarchy();
			proxyContainer = null;
			DoUpdate();
		}

		void DoUpdate() {
			if(portData != null && owner.IsVisible()) {
				bool isProxy = false;
				if(isFlow) {
					if(direction == Direction.Input) {
						var edges = GetEdges();
						if(edges != null && edges.Count > 0) {
							if(edges.Any(e => e != null && e.isProxy)) {
								isProxy = true;
							}
						}
					} else {
						if(connected) {
							var edges = GetEdges();
							if(edges != null && edges.Count > 0) {
								if(edges.Any(e => e != null && e.isProxy)) {
									isProxy = true;
								}
							}
						}
					}
				} else {
					if(direction == Direction.Input) {
						if(connected) {
							var edges = GetEdges();
							if(edges != null && edges.Count > 0) {
								if(edges.Any(e => e != null && e.isProxy)) {
									isProxy = true;
								}
							}
						}
					} else {
						var edges = GetEdges();
						if(edges != null && edges.Count > 0) {
							if(edges.Any(e => e != null && e.isProxy)) {
								isProxy = true;
							}
						}
					}
				}
				ToggleProxy(isProxy);
				if(isProxy) {
					var color = portColor;
					proxyContainer.style.backgroundColor = color;
					proxyCap.style.backgroundColor = color;
					proxyLine.style.backgroundColor = color;
					if(proxyTitleBox != null) {
						var edges = GetEdges();
						if(edges != null && edges.Count > 0) {
							var edge = edges.FirstOrDefault(e => e != null && e.isValid && e.isProxy);
							if(edge != null) {
								PortView port = edge.input != this ? edge.input as PortView : edge.output as PortView;
								if(port != null && proxyTitleLabel != null) {
									proxyTitleLabel.text = uNodeEditorUtility.RemoveHTMLTag(port.GetProxyName());
									if(isValue) {
										proxyTitleBox.style.SetBorderColor(port.portColor);
										proxyTitleIcon.image = port.portIcon?.image;
									}
								}
							}
						}
						MarkRepaintProxyTitle();
					}
				}
			}
		}

		private bool flagRepaintProxy;
		void MarkRepaintProxyTitle() {
			if(proxyTitleBox != null && !flagRepaintProxy) {
				flagRepaintProxy = true;
				if (orientation == Orientation.Horizontal && direction == Direction.Input) {
					proxyTitleBox.ScheduleOnce(() => {
						flagRepaintProxy = false;
						proxyTitleBox.style.left = -proxyTitleBox.layout.width;
					}, 0);
				}
			}
		}

		public override bool ContainsPoint(Vector2 localPoint) {
			if(isFlow && owner.owner.graphLayout == GraphLayout.Vertical) {
				return new Rect(0.0f, 0.0f, layout.width, layout.height).Contains(localPoint);
			} else {
				Rect layout = m_ConnectorBox.layout;
				Rect rect;
				if(direction == Direction.Input) {
					rect = new Rect(0f - layout.xMin, 0f - layout.yMin, layout.width + layout.xMin, this.layout.height);
				} else {
					rect = new Rect(-5, 0f - layout.yMin, this.layout.width - layout.xMin, this.layout.height);
				}
				rect.width += 5;
				return rect.Contains(this.ChangeCoordinatesTo(m_ConnectorBox, localPoint));
			}
		}

		protected override void OnCustomStyleResolved(ICustomStyle styles) {
			if(isValue) {
				portColor = uNodePreference.GetColorForType(portType);
			}
			base.OnCustomStyleResolved(styles);
		}

		#region Proxy
		private VisualElement proxyContainer;
		private VisualElement proxyCap;
		private VisualElement proxyLine;
		private VisualElement proxyTitleBox;
		private Label proxyTitleLabel;
		private Image proxyTitleIcon;
		private IMGUIContainer proxyDebug;
		protected void ToggleProxy(bool enable) {
			if(enable) {
				if(proxyContainer == null) {
					VisualElement connector = this.Q("connector");
					proxyContainer = new VisualElement { name = "connector" };
					proxyContainer.pickingMode = PickingMode.Ignore;
					proxyContainer.EnableInClassList("proxy", true);
					{
						proxyCap = new VisualElement() { name = "cap" };
						proxyCap.Add(proxyLine = new VisualElement() { name = "proxy-line" });
						proxyContainer.Add(proxyCap);
					}
					if(displayProxyTitle && (isValue && direction == Direction.Input || isFlow && direction == Direction.Output)) {
						proxyTitleBox = new VisualElement() {
							name = "proxy-title",
						};
						proxyTitleBox.pickingMode = PickingMode.Ignore;
						proxyTitleLabel = new Label();
						proxyTitleLabel.pickingMode = PickingMode.Ignore;
						proxyTitleBox.Add(proxyTitleLabel);
						proxyContainer.Add(proxyTitleBox);
						if(isValue) {
							proxyTitleBox.AddToClassList("proxy-horizontal");
							proxyTitleIcon = new Image();
							proxyTitleIcon.pickingMode = PickingMode.Ignore;
							proxyTitleBox.Add(proxyTitleIcon);
						} else {
							proxyTitleBox.AddToClassList("proxy-vertical");
						}
						MarkRepaintProxyTitle();
					}
					if(Application.isPlaying && isValue && direction == Direction.Input) {
						if(proxyDebug != null) {
							proxyDebug.RemoveFromHierarchy();
						}
						proxyDebug = new IMGUIContainer(DebugGUI);
						proxyDebug.style.position = Position.Absolute;
						proxyDebug.style.overflow = Overflow.Visible;
						proxyDebug.pickingMode = PickingMode.Ignore;
						proxyContainer.Add(proxyDebug);
					}
					connector.Add(proxyContainer);
					MarkDirtyRepaint();
				}
			} else if(proxyContainer != null) {
				proxyContainer.RemoveFromHierarchy();
				proxyContainer = null;
			}
		}
		#endregion

		#region Debug
		void DebugGUI() {
			if(Application.isPlaying && GraphDebug.useDebug && proxyContainer != null) {
				GraphDebug.DebugData debugData = owner.owner.graph.GetDebugInfo();
				if(debugData != null) {
					if(isValue && direction == Direction.Input) {
						MemberData member = portData.GetPortValue();
						if(member != null) {
							GUIContent debugContent = null;
							switch(member.targetType) {
								case MemberData.TargetType.ValueNode: {
									int ID = uNodeUtility.GetObjectID(member.startTarget as MonoBehaviour);
									if(debugData != null && debugData.valueNodeDebug.ContainsKey(ID)) {
										if(debugData.valueNodeDebug[ID].ContainsKey(int.Parse(member.startName))) {
											var vData = debugData.valueNodeDebug[ID][int.Parse(member.startName)];
											if(vData.value != null) {
												debugContent = new GUIContent
													(uNodeUtility.GetDebugName(vData.value),
													uNodeEditorUtility.GetTypeIcon(vData.value.GetType()));
											} else {
												debugContent = new GUIContent("null");
											}
										}
									}
									break;
								}
								case MemberData.TargetType.NodeField: {

									break;
								}
								case MemberData.TargetType.NodeFieldElement: {

									break;
								}
							}
							if(debugContent != null) {
								Vector2 pos;
								if(proxyTitleLabel != null) {
									pos = proxyTitleLabel.ChangeCoordinatesTo(proxyDebug, Vector2.zero);
								} else {
									pos = this.ChangeCoordinatesTo(proxyDebug, new Vector2(-proxyContainer.layout.width, 0));
								}
								Vector2 size = EditorStyles.helpBox.CalcSize(new GUIContent(debugContent.text));
								size.x += 25;
								GUI.Box(
									new Rect(pos.x - size.x, pos.y, size.x - 5, 20),
									debugContent,
									EditorStyles.helpBox);
							}
						}
					}
				}
			}
		}
		#endregion

		#region Connect & Disconnect
		public override void Connect(Edge edge) {
			base.Connect(edge);
			edges.Add(edge as EdgeView);
			OnConnected?.Invoke(this, edge);
			owner.OnPortConnected(this);
			UpdatePortClass();
		}

		public override void Disconnect(Edge edge) {
			base.Disconnect(edge);
			edges.Remove(edge as EdgeView);
			OnDisconnected?.Invoke(this, edge);
			owner.OnPortDisconnected(this);
			UpdatePortClass();
		}
		#endregion

		#region Drop Port
		public void OnDropOutsidePort(Edge edge, Vector2 position) {
			var input = edge.input as PortView;
			var output = edge.output as PortView;
			var screenRect = owner.owner.graph.window.GetMousePositionForMenu(position);
			Vector2 pos = owner.owner.graph.window.rootVisualElement.ChangeCoordinatesTo(
				owner.owner.graph.window.rootVisualElement.parent,
				screenRect - owner.owner.graph.window.position.position);
			position = owner.owner.contentViewContainer.WorldToLocal(pos);
			PortView sidePort = null;
			if(input != null && output != null) {
				var draggedPort = input.edgeConnector?.edgeDragHelper?.draggedPort ?? output.edgeConnector?.edgeDragHelper?.draggedPort;
				if(draggedPort == input) {
					sidePort = output;
					output = null;
				} else if(draggedPort == output) {
					sidePort = input;
					input = null;
				}
			}
			if(input != null) {//Process if source is input port.
				PortView portView = input as PortView;
				foreach(var node in owner.owner.nodeViews) {
					if(node != null && node != portView.owner) {
						if(node.layout.Contains(position)) {
							if((edge.input as PortView).isFlow) {//Flow
								foreach(var port in node.outputPorts) {
									if(port.isFlow) {//Find the first flow port and connect it.
										uNodeThreadUtility.Queue(() => {
											edge.input = portView;
											edge.output = port;
											owner.owner.Connect(edge as EdgeView, true);
											owner.owner.MarkRepaint();
										});
										return;
									}
								}
							} else {//Input Value
								FilterAttribute filter = portView.GetFilter();
								bool flag = true;
								if(filter.SetMember) {
									var tNode = portView.GetNode() as Node;
									if(tNode == null || !tNode.CanSetValue()) {
										flag = false;
									}
								}
								if(flag) {
									foreach(var port in node.outputPorts) {
										if(port.isValue && portView.IsValidTarget(port)) {
											uNodeThreadUtility.Queue(() => {
												edge.input = portView;
												edge.output = port;
												OnDrop(owner.owner, edge);
											});
											return;
										}
									}
								}
							}
							break;
						}
					}
				}
				if(input.isValue) {//Input Value
					FilterAttribute FA = portData.GetFilter();
					var types = FA.Types;
					FA = new FilterAttribute (FA) {
						MaxMethodParam = int.MaxValue,
						ValidateType = (type) => {
							for(int i=0;i< types.Count;i++) {
								if(CanAutoConvertType(type, types[i])) {
									return true;
								}
							}
							return false;
						},
						// DisplayDefaultStaticType = false
					};
					List<ItemSelector.CustomItem> customItems = new List<ItemSelector.CustomItem>();
					var editorData = owner.owner.editorData;
					var portType = GetPortType();
					if(editorData.graph is IVariableSystem) {
						var IVS = editorData.graph as IVariableSystem;
						var type = portType;
						if(type.IsByRef) {
							type = type.GetElementType();
						}
						customItems.Add(ItemSelector.CustomItem.Create("Promote to variable", () => {
							NodeEditorUtility.AddNewVariable(IVS, "newVariable", type, v => {
								if(portType.IsByRef)
									v.modifier.SetPrivate();
								NodeEditorUtility.AddNewNode<MultipurposeNode>(editorData, position, (node) => {
									node.target.target = MemberData.CreateFromValue(v, editorData.graph);
									portView.ChangeValue(MemberData.ValueOutput(node));
								});
							});
							uNodeEditor.GUIChanged();
						}, "@", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon))));
					}
					if(editorData.selectedRoot is ILocalVariableSystem) {
						var IVS = editorData.selectedRoot as ILocalVariableSystem;
						var type = portType;
						if(type.IsByRef) {
							type = type.GetElementType();
						}
						customItems.Add(ItemSelector.CustomItem.Create("Promote to Local Variable", () => {
							NodeEditorUtility.AddNewVariable(IVS, "localVariable", type, v => {
								v.modifier.SetPrivate();
								NodeEditorUtility.AddNewNode<MultipurposeNode>(editorData, position, (node) => {
									node.target.target = MemberData.CreateFromValue(v, editorData.graph);
									portView.ChangeValue(MemberData.ValueOutput(node));
								});
							});
							uNodeEditor.GUIChanged();
						}, "@", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon))));
					}
					{//Create custom items for port commands
						if(!portType.IsSubclassOf(typeof(Delegate)) && !portType.IsPrimitive) {
							var filter = new FilterAttribute() { MaxMethodParam = int.MaxValue };
							var ctors = portType.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
							for(int i = ctors.Length - 1; i >= 0; i--) {
								if(ReflectionUtils.IsValidParameters(ctors[i])) {
									var item = EditorReflectionUtility.GetReflectionItems(ctors[i], filter);
									if(item == null)
										continue;
									customItems.Add(ItemSelector.CustomItem.Create(item.displayName, item, category: "@"));
								}
							}
						}
						var portCommands = NodeEditorUtility.FindPortCommands();
						if(portCommands != null && portCommands.Count > 0) {
							PortCommandData commandData = new PortCommandData() {
								portType = portType,
								portName = GetName(),
								getConnection = portData.GetConnection,
								member = portData.GetPortValue(),
								portKind = PortKind.ValueInput,
							};
							foreach(var command in portCommands) {
								if(command.onlyContextMenu)
									continue;
								command.graph = owner.owner.graph;
								command.mousePositionOnCanvas = position;
								command.filter = portData.GetFilter();
								if(command.IsValidPort(owner.targetNode, commandData)) {
									customItems.Add(ItemSelector.CustomItem.Create(command.name, () => {
										command.OnClick(owner.targetNode, commandData, position);
										owner.MarkRepaint();
									}, "@", icon: command.GetIcon()));
								}
							}
						}
					}
					owner.owner.graph.ShowNodeMenu(position, FA, (n) => {
						if(n.CanGetValue()) {
							if(n is MultipurposeNode mNode) {
								var type = mNode.ReturnType();
								Type rightType = type;
								for(int i = 0; i < types.Count; i++) {
									if(CanAutoConvertType(type, types[i])) {
										rightType = types[i];
										break;
									}
								}
								if(!type.IsCastableTo(rightType)) {
									AutoConvertPort(type, rightType, n, input.owner.targetNode, MemberData.ValueOutput(n), (val) => {
										portView.ChangeValue(val);
									}, new FilterAttribute(rightType));
									return;
								}
							}
							portView.ChangeValue(new MemberData(n, MemberData.TargetType.ValueNode));
							if(sidePort != null) {
								//Reset the original connection
								sidePort.ResetPortValue();
							}
						}
					}, false, additionalItems: customItems);
				} else {//Input Flow
					owner.owner.graph.ShowNodeMenu(position, new FilterAttribute(), (n) => {
						if(n is MultipurposeNode mNode && !mNode.IsFlowNode() && mNode.CanSetValue()) {
							var rType = mNode.ReturnType();
							if(rType.IsCastableTo(typeof(Delegate)) || rType.IsCastableTo(typeof(UnityEngine.Events.UnityEventBase))) {
								NodeEditorUtility.AddNewNode(owner.owner.editorData, null, null, position, (Nodes.EventHook nod) => {
									nod.target = MemberData.ValueOutput(n);
									n = nod;
								});
							} else {
								NodeEditorUtility.AddNewNode(owner.owner.editorData, null, null, position, (NodeSetValue nod) => {
									nod.target = MemberData.ValueOutput(n);
									if(n.ReturnType() != typeof(void)) {
										nod.value = MemberData.CreateValueFromType(n.ReturnType());
									}
									n = nod;
								});
							}
						}
						var fields = NodeEditorUtility.GetFieldNodes(n);
						foreach(var field in fields) {
							if(field.field.FieldType == typeof(MemberData)) {
								if(field.attribute is FieldConnectionAttribute) {
									var FCA = field.attribute as FieldConnectionAttribute;
									if(FCA is FlowOutAttribute) {
										if(portView.portData.portID == UGraphView.SelfPortID) {
											field.field.SetValueOptimized(n, portView.portData.GetConnection());
										} else {
											field.field.SetValueOptimized(n,
												new MemberData(
													new object[] {
														portView.owner.targetNode,
														portView.portData.portID
													},
													MemberData.TargetType.FlowInput));
										}
										break;
									}
								}
							}
						}
						if(sidePort != null) {
							//Reset the original connection
							sidePort.ResetPortValue();
						}
					});
				}
			} else if(output != null) {//Process if source is output port.
				PortView portView = output as PortView;
				foreach(var node in owner.owner.nodeViews) {
					if(node != null && node != portView.owner) {
						if(node.layout.Contains(position)) {
							if(output.isFlow) {//Flow
								foreach(var port in node.inputPorts) {
									if(port.isFlow) {
										uNodeThreadUtility.Queue(() => {
											edge.output = portView;
											edge.input = port;
											owner.owner.Connect(edge as EdgeView, true);
											owner.owner.MarkRepaint();
										});
										return;
									}
								}
							} else {//Output Value
								FilterAttribute filter = portView.GetFilter();
								bool flag = true;
								if(filter.SetMember) {
									var tNode = portView.GetNode() as Node;
									if(tNode == null || !tNode.CanSetValue()) {
										flag = false;
									}
								}
								if(flag) {
									foreach(var port in node.inputPorts) {
										if(port.isValue && portView.IsValidTarget(port)) {
											uNodeThreadUtility.Queue(() => {
												edge.output = portView;
												edge.input = port;
												OnDrop(owner.owner, edge);
											});
											return;
										}
									}
								}
							}
							break;
						}
					}
				}
				if(output.isFlow) {//Output Flow
					owner.owner.graph.ShowNodeMenu(position, new FilterAttribute(), (n) => {
						if(n.IsFlowNode()) {
							portView.portData.ChangeValue(new MemberData(n, MemberData.TargetType.FlowNode));
						} else {
							if(n is MultipurposeNode mNode && mNode.CanSetValue()) {
								var rType = mNode.ReturnType();
								if(rType.IsCastableTo(typeof(Delegate)) || rType.IsCastableTo(typeof(UnityEngine.Events.UnityEventBase))) {
									NodeEditorUtility.AddNewNode(owner.owner.editorData, null, null, position, (Nodes.EventHook nod) => {
										nod.target = MemberData.ValueOutput(n);
										portView.portData.ChangeValue(MemberData.FlowInput(nod, nameof(nod.register)));
									});
								} else {
									NodeEditorUtility.AddNewNode(owner.owner.editorData, null, null, position, (NodeSetValue nod) => {
										nod.target = MemberData.ValueOutput(n);
										if(n.ReturnType() != typeof(void)) {
											nod.value = MemberData.CreateValueFromType(n.ReturnType());
										}
										portView.portData.ChangeValue(new MemberData(nod, MemberData.TargetType.FlowNode));
									});
								}
							}
						}
						if(sidePort != null) {
							//Reset the original connection
							sidePort.ResetPortValue();
						}

					});
				} else {//Output Value
					Type type = portView.GetPortType();
					bool canSetValue = false;
					bool canGetValue = true;
					if(portView.GetPortID() == UGraphView.SelfPortID && portView.GetNode() is Node) {
						canSetValue = (portView.GetNode() as Node).CanSetValue();
						canGetValue = (portView.GetNode() as Node).CanGetValue();
					}
					bool onlySet = canSetValue && !canGetValue;
					FilterAttribute FA = new FilterAttribute {
						VoidType = true,
						MaxMethodParam = int.MaxValue,
						Public = true,
						Instance = true,
						Static = false,
						UnityReference = false,
						InvalidTargetType = MemberData.TargetType.Null | MemberData.TargetType.Values,
						// DisplayDefaultStaticType = false
					};
					List<ItemSelector.CustomItem> customItems = null;
					if(!onlySet && GetNode() is MultipurposeNode && type.IsCastableTo(typeof(uNodeRoot))) {
						MultipurposeNode multipurposeNode = GetNode() as MultipurposeNode;
						if(multipurposeNode.target != null && multipurposeNode.target.target != null && (multipurposeNode.target.target.targetType == MemberData.TargetType.SelfTarget || multipurposeNode.target.target.targetType == MemberData.TargetType.Values)) {
							var sTarget = multipurposeNode.target.target.startTarget;
							if(sTarget is uNodeRoot) {
								customItems = ItemSelector.MakeCustomItems(sTarget as uNodeRoot);
								customItems.AddRange(ItemSelector.MakeCustomItems(typeof(uNodeRoot), sTarget, FA, "Inherit Member"));
							}
						}
					}
					if(customItems == null) {
						if(type is RuntimeType) {
							customItems = ItemSelector.MakeCustomItems((type as RuntimeType).GetRuntimeMembers(), FA);
							if (type.BaseType != null) 
								customItems.AddRange(ItemSelector.MakeCustomItems(type.BaseType, FA, "Inherit Member"));
						} else {
							customItems = onlySet ? new List<ItemSelector.CustomItem>() : ItemSelector.MakeCustomItems(type, FA, "Data", "Data ( Inherited )");
						}
						var data = portView?.owner?.targetNode?.owner?.GetComponent<uNodeData>();
						if(data != null) {
							var usingNamespaces = new HashSet<string>(data.GetNamespaces());
							customItems.AddRange(ItemSelector.MakeExtensionItems(type, usingNamespaces, FA, "Extensions"));
						}

						var customInputItems = NodeEditorUtility.FindCustomInputPortItems();
						if(customInputItems != null && customInputItems.Count > 0) {
							var mData = portView.portData.GetConnection();
							var portNode = GetNode() as Node;
							if(portNode != null) {
								var portType = GetPortType();
								foreach(var c in customInputItems) {
									c.graph = owner.owner.graph;
									c.mousePositionOnCanvas = position;
									if(c.IsValidPort(portType, 
										canSetValue && canGetValue ? 
											PortAccessibility.GetSet : 
											canGetValue ? PortAccessibility.OnlyGet : PortAccessibility.OnlySet)) {
										var items = c.GetItems(portNode, mData, portType);
										if(items != null) {
											customItems.AddRange(items);
										}
									}
								}
							}
						}
						var portCommands = NodeEditorUtility.FindPortCommands();
						if(portCommands != null && portCommands.Count > 0) {
							PortCommandData commandData = new PortCommandData() {
								portType = portType,
								portName = GetName(),
								getConnection = portData.GetConnection,
								portKind = PortKind.ValueOutput,
							};
							foreach(var command in portCommands) {
								if(command.onlyContextMenu)
									continue;
								command.graph = owner.owner.graph;
								command.mousePositionOnCanvas = position;
								command.filter = portData.GetFilter();
								if(command.IsValidPort(owner.targetNode, commandData)) {
									customItems.Add(ItemSelector.CustomItem.Create(command.name, () => {
										command.OnClick(owner.targetNode, commandData, position);
										owner.MarkRepaint();
									}, "@"));
								}
							}
						}
					}
					if(customItems != null) {
						FA.Static = true;
						customItems.Sort((x, y) => {
							if(x.category != y.category) {
								return string.Compare(x.category, y.category, StringComparison.OrdinalIgnoreCase);
							}
							return string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase);
						});
						ItemSelector w = ItemSelector.ShowWindow(portView.owner.targetNode, MemberData.none, FA, (MemberData mData) => {
							bool flag = mData.targetType == MemberData.TargetType.Method && !type.IsCastableTo(mData.startType);
							NodeGraph.CreateNodeProcessor(mData, owner.owner.graph.editorData, position, (n) => {
								if(!mData.isStatic) {//For auto connect to instance port
									if(type.IsCastableTo(mData.startType)) {
										mData.instance = portView.portData.GetConnection();
										flag = false;
									} else {
										AutoConvertPort(type, mData.startType, output.owner.targetNode, n, output.portData.GetConnection(), (val) => {
											mData.instance = val;
											flag = false;
										}, new FilterAttribute(mData.startType));
									}
								}
								if(n is MultipurposeNode multipurposeNode) {//For auto connect to parameter ports
									if(flag) {
										var pTypes = mData.ParameterTypes;
										if(pTypes != null) {
											int paramIndex = 0;
											MemberData param = null;
											for(int i = 0; i < pTypes.Length; i++) {
												var types = pTypes[i];
												if(types != null) {
													for(int y = 0; y < types.Length; y++) {
														if(type.IsCastableTo(types[y])) {
															param = portView.portData.GetConnection();
															break;
														}
														paramIndex++;
													}
													if(param != null)
														break;
												}
											}
											if(param == null) {
												paramIndex = 0;
												for(int i = 0; i < pTypes.Length; i++) {
													var types = pTypes[i];
													if(types != null) {
														for(int y = 0; y < types.Length; y++) {
															if(types[y].IsCastableTo(type)) {
																AutoConvertPort(type, types[y], output.owner.targetNode, n, output.portData.GetConnection(), (val) => {
																	param = val;
																}, new FilterAttribute(types[y]));
																break;
															}
															paramIndex++;
														}
														if(param != null)
															break;
													}
												}
											}
											if(param != null && multipurposeNode.target.parameters.Length > paramIndex) {
												multipurposeNode.target.parameters[paramIndex] = param;
											}
										}
									}
								}
								portView.owner.owner.MarkRepaint();
							});
						}, customItems).ChangePosition(owner.owner.graph.GetMenuPosition());
						w.displayRecentItem = false;
						w.displayNoneOption = false;
					}
				}
			}
		}

		public bool CanConnect(PortView port) {
			if(port.direction == direction || port.owner == owner) {
				return false;
			}
			if(port.isValue) {
				PortView input;
				PortView output;
				if(port.direction == Direction.Input) {
					input = port;
					output = this;
				} else {
					input = this;
					output = port;
				}
				return CanAutoConvertType(output.GetPortType(), input.GetPortType());
			}
			return true;
		}

		private static bool CanAutoConvertType(Type leftType, Type rightType) {
			if(leftType == typeof(string)) {
				if(rightType == typeof(float) ||
					rightType == typeof(int) ||
					rightType == typeof(double) ||
					rightType == typeof(decimal) ||
					rightType == typeof(short) ||
					rightType == typeof(ushort) ||
					rightType == typeof(uint) ||
					rightType == typeof(long) ||
					rightType == typeof(byte) ||
					rightType == typeof(sbyte)) {
					return true;
				}
			}
			if(leftType.IsCastableTo(rightType)) {
				return true;
			}
			var autoConverts = NodeEditorUtility.FindAutoConvertPorts();
			foreach(var c in autoConverts) {
				c.leftType = leftType;
				c.rightType = rightType;
				if(c.IsValid()) {
					return true;
				}
			}
			return false;
		}

		private bool AutoConvertPort(Type leftType, Type rightType, NodeComponent outputNode, NodeComponent inputNode, MemberData outputConnection, Action<MemberData> action, FilterAttribute filter = null, bool forceConvert = false) {
			if(rightType != typeof(object)) {
				var autoConverts = NodeEditorUtility.FindAutoConvertPorts();
				foreach(var c in autoConverts) {
					c.filter = filter;
					c.leftType = leftType;
					c.rightType = rightType;
					c.getConnection = () => outputConnection;
					c.graph = owner.owner.editorData.graph;
					c.parent = owner.owner.editorData.currentRoot;
					c.position = inputNode.editorRect.position;
					c.force = forceConvert;
					if(c.IsValid()) {
						if(c.CreateNode(n => action?.Invoke(new MemberData(n, MemberData.TargetType.ValueNode)))) {
							return true;
						}
						return false;
					}
				}
			}
			return false;
		}

		private void AutoConvertPort(UGraphView graphView, Edge edge, Type rightType) {
			var leftPort = edge.output as PortView;
			var rightPort = edge.input as PortView;

			Type leftType = leftPort.portData.GetFilter().GetActualType();
			AutoConvertPort(leftType, rightType, leftPort.owner.targetNode, rightPort.owner.targetNode, leftPort.portData.GetConnection(), (val) => {
				rightPort.portData.ChangeValue(val);
			}, rightPort.portData.GetFilter(), forceConvert: true);
			graphView.MarkRepaint();
		}

		public void OnDrop(GraphView graphView, Edge edge) {
			var edgeView = edge as EdgeView;
			var graph = graphView as UGraphView;
			if(graph == null || edgeView == null || edgeView.input == null || edgeView.output == null)
				return;
			if(edgeView.Input.isValue) {
				if(edgeView.input == this) {
					if(!IsValidTarget(edge.output)) {
						int option = EditorUtility.DisplayDialogComplex("Do you want to continue?",
							"The source port and destination port type is not match.",
							"Convert if possible", "Continue", "Cancel");
						if(option == 0) {
							var filteredTypes = (edge.input as PortView).portData.GetFilter().GetFilteredTypes();
							if(filteredTypes.Count > 1) {
								var menu = new GenericMenu();
								for(int i=0;i<filteredTypes.Count;i++) {
									var t = filteredTypes[i];
									menu.AddItem(new GUIContent(t.PrettyName(true)), false, () => {
										AutoConvertPort(graph, edge, t);
									});
								}
								menu.ShowAsContext();
							} else {
								AutoConvertPort(graph, edge, filteredTypes[0]);
							}
							return;
						} else if(option != 1) {
							return;
						}
					}
				} else {
					if(!IsValidTarget(edge.input)) {
						int option = EditorUtility.DisplayDialogComplex("Do you want to continue?",
							"The source port and destination port type is not match.",
							"Convert if possible", "Continue", "Cancel");
						if(option == 0) {
							var filteredTypes = (edge.input as PortView).portData.GetFilter().GetFilteredTypes();
							if(filteredTypes.Count > 1) {
								var menu = new GenericMenu();
								for(int i = 0; i < filteredTypes.Count; i++) {
									var t = filteredTypes[i];
									menu.AddItem(new GUIContent(t.PrettyName(true)), false, () => {
										AutoConvertPort(graph, edge, t);
									});
								}
								menu.ShowAsContext();
							} else {
								AutoConvertPort(graph, edge, filteredTypes[0]);
							}
							return;
						} else if(option != 1) {
							return;
						}
					}
				}
			}
			graph.Connect(edgeView, true);
		}
		#endregion

		#region Functions
		public void SetControl(VisualElement visualElement, bool autoLayout = false) {
			ControlView control = new ControlView();
			control.Add(visualElement);
			SetControl(control, autoLayout);
		}

		public void SetControl(ControlView control, bool autoLayout = false) {
			if(controlView != null) {
				controlView.RemoveFromHierarchy();
				controlView = null;
			}
			if(control != null) {
				control.EnableInClassList("output_port", true);
				m_ConnectorText.Add(control);
				controlView = control;
			}
			m_ConnectorText.EnableInClassList("Layout", autoLayout);
			portName = portName;
		}

		internal List<EdgeView> GetEdges() {
			return edges;
		}

		public IEnumerable<EdgeView> GetValidEdges() {
			foreach(var edge in edges) {
				if(!edge.isValid)
					continue;
				yield return edge;
			}
		}

		public HashSet<UNodeView> GetEdgeOwners() {
			HashSet<UNodeView> nodes = new HashSet<UNodeView>();
			foreach (var e in edges) {
				if(!e.isValid)
					continue;
				var sender = e.GetSenderPort()?.owner;
				if(sender != null) {
					nodes.Add(sender);
				}
				var receiver = e.GetReceiverPort()?.owner;
				if(receiver != null) {
					nodes.Add(receiver);
				}
			}
			return nodes;
		}

		public HashSet<UNodeView> GetConnectedNodes() {
			HashSet<UNodeView> nodes = new HashSet<UNodeView>();
			if(edges.Count > 0) {
				foreach(var e in edges) {
					if(!e.isValid)
						continue;
					if(direction == Direction.Input) {
						var targetPort = e.output as PortView;
						var targetView = targetPort.owner;
						if(targetView != null) {
							nodes.Add(targetView);
						}
					} else {
						var targetPort = e.input as PortView;
						var targetView = targetPort.owner;
						if(targetView != null) {
							nodes.Add(targetView);
						}
					}
				}
			}
			return nodes;
		}

		public HashSet<PortView> GetConnectedPorts() {
			HashSet<PortView> ports = new HashSet<PortView>();
			if(edges.Count > 0) {
				foreach(var e in edges) {
					if(!e.isValid)
						continue;
					if(direction == Direction.Input) {
						ports.Add(e.Output);
					} else {
						ports.Add(e.Input);
					}
				}
			}
			return ports;
		}

		public Type GetPortType() {
			return portType;
		}

		public string GetPortID() {
			return portData.portID;
		}

		public FilterAttribute GetFilter() {
			return portData.GetFilter();
		}

		public MemberData GetValue() {
			return portData.GetPortValue();
		}

		public void ChangeValue(MemberData value) {
			portData.ChangeValue(value);
		}

		/// <summary>
		/// Get the connection.
		/// This should not null if the port is value output or port is flow input
		/// </summary>
		/// <returns></returns>
		public MemberData GetConnection() {
			return portData.GetConnection();
		}

		public string GetName() {
			return portName;
		}

		public string GetTooltip() {
			var str = portData.GetPortTooltip();
			if(string.IsNullOrEmpty(str)) {
				if(isFlow) {
					if(direction == Direction.Input) {
						if(GetPortID() == UGraphView.SelfPortID) {
							return "Input flow to execute this node";
						}
					} else {
						if(GetPortID() == UGraphView.NextFlowID) {
							return "Flow to execute on finish";
						}
					}
				} else {
					if(direction == Direction.Input) {
						
					} else {
						if(GetPortID() == UGraphView.SelfPortID) {
							return "The result value";
						}
					}
				}
			}
			return str;
		}

		public string GetPrettyName() {
			var str = GetName();
			if(string.IsNullOrEmpty(str)) {
				if(isFlow) {
					if(direction == Direction.Input) {
						if(GetPortID() == UGraphView.SelfPortID) {
							return "Input";
						}
					} else {

					}
				} else {
					if(direction == Direction.Input) {
						
					} else {
						if(GetPortID() == UGraphView.SelfPortID) {
							return "Result";
						}
					}
				}
				if(portData.portID.StartsWith("$", StringComparison.Ordinal)) {
					return "Port";
				}
				return ObjectNames.NicifyVariableName(portData.portID);
			}
			return str;
		}

		private string GetProxyName() {
			var str = GetName();
			if(string.IsNullOrEmpty(str)) {
				if(isFlow) {
					if(direction == Direction.Input) {
						if(GetPortID() == UGraphView.SelfPortID) {
							return owner.targetNode.GetNodeName();
						}
					} else {

					}
				} else {
					if(direction == Direction.Input) {

					} else {
						if(GetPortID() == UGraphView.SelfPortID) {
							return owner.targetNode.GetNodeName();
						}
					}
				}
				if(portData.portID.StartsWith("$", StringComparison.Ordinal)) {
					return "Port";
				}
				return ObjectNames.NicifyVariableName(portData.portID);
			}
			return str;
		}

		public void SetName(string str) {
			portName = ObjectNames.NicifyVariableName(str);
			portData.getPortName = () => str;
		}

		public NodeComponent GetNode() {
			return owner.targetNode;
		}

		public bool IsValidTarget(Port port) {
			var portView = port as PortView;
			if(portView != null) {
				if(portData.owner == portView.portData.owner || orientation != portView.orientation)
					return false;
				if(isValue) {
					var inputPort = portView.direction == Direction.Input ? portView : this;
					var outputPort = portView.direction == Direction.Output ? portView : this;

					var filter = inputPort.portData.GetFilter();
					var outputType = outputPort.portData.GetPortType();
					if(filter.IsValidType(outputType) || outputType.IsCastableTo(inputPort.portType)) {
						return true;
					} else {
						var inputType = inputPort.portData.GetPortType();
						if(inputType == outputType ||
							inputType == typeof(MemberData) ||
							//inputType.IsCastableTo(outputType) ||
							outputType.IsCastableTo(inputType)) {
							return true;
						} else if(inputType is RuntimeType && (
							inputType.IsCastableTo(typeof(Component)) || 
							inputType.IsInterface)) {
							if(outputType == typeof(GameObject) || outputType.IsCastableTo(typeof(Component))) {
								return true;
							}
						}
					}
					return false;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// True if the port is proxy
		/// </summary>
		/// <returns></returns>
		public bool IsProxy() {
			if(edges.Count == 0)
				return false;
			return edges.All(e => e.isProxy);
		}

		/// <summary>
		/// Reset the port value
		/// </summary>
		public void ResetPortValue() {
			if(isFlow) {
				portData.ChangeValue(MemberData.none);
			} else {
				if(direction == Direction.Input) {
					var val = portData.GetPortValue();
					if(val != null) {
						Type valType = val.type;
						if(valType == null) {
							valType = GetPortType();
						}
						portData.ChangeValue(MemberData.CreateValueFromType(valType));
					} else {
						portData.ChangeValue(MemberData.CreateValueFromType(GetPortType()));
					}
				} else {
					portData.ChangeValue(MemberData.empty);
				}
			}
		}

		const int depthOffset = 8;
		private List<VisualElement> m_depths;
		/// <summary>
		/// Set the port depth
		/// </summary>
		/// <param name="depth"></param>
		public void SetPortDepth(int depth) {
			if(m_depths != null) {
				foreach(var v in m_depths) {
					v?.RemoveFromHierarchy();
				}
			}
			m_depths = new List<VisualElement>();
			for(int i = 0; i < depth; ++i) {
				VisualElement line = new VisualElement();
				line.name = "line";
				line.style.marginLeft = depthOffset + (i == 0 ? -2 : 0);
				line.style.marginRight = ((i == depth - 1) ? 2 : 0);
				m_depths.Add(line);
			}
			for(int i=m_depths.Count-1;i >=0;i--) {
				Insert(1, m_depths[i]);
			}
		}
		#endregion
	}
}