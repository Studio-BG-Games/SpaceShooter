using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public class BaseNodeView : UNodeView {
		/// <summary>
		/// The default flow port
		/// </summary>
		protected PortView nodeFlowPort;
		/// <summary>
		/// The default value port
		/// </summary>
		protected PortView nodeValuePort;

		protected VisualElement debugView;
		protected VisualElement debugStateView;
		
		private Image compactIcon = null;

		#region  Initialization
		/// <summary>
		/// Initialize once on created.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="node"></param>
		public override void Initialize(UGraphView owner, NodeComponent node) {
			this.AddStyleSheet("uNodeStyles/NativeNodeStyle");
			this.AddStyleSheet(UIElementUtility.Theme.nodeStyle);
			base.Initialize(owner, node);
			this.ExecuteAndScheduleAction(DoUpdate, 500);
		}

		/// <summary>
		/// Call this to initialize extended port if node implement IExtendedOutput interface
		/// </summary>
		protected void InitializeOutputExtenderPorts() {
			var node = targetNode as IExtendedOutput;
			if(node == null)
				return;
			int count = node.OutputCount;
			if(count > 0) {
				for(int i=0;i<count;i++) {
					int index = i;
					AddOutputValuePort(node, index);
				}
			}
		}

		/// <summary>
		/// Call this to initialize extended port if node implement IExtendedInput interface
		/// </summary>
		protected void InitializeInputExtenderPorts() {
			var node = targetNode as IExtendedInput;
			if(node == null)
				return;
			int count = node.InputCount;
			if(count > 0) {
				for(int i = 0; i < count; i++) {
					int index = i;
					AddInputFlowPort(node, index);
				}
			}
		}

		/// <summary>
		/// Initialize the default port, by default this will be called by InitializeView
		/// </summary>
		protected void InitializeDefaultPort() {
			nodeFlowPort = null;
			nodeValuePort = null;
			var node = targetNode as Node;
			if (node.IsFlowNode()) {//Flow input
				nodeFlowPort = AddInputFlowPort(
					new PortData() {
						portID = UGraphView.SelfPortID,
						getPortName = () => "",
						getConnection = () => {
							return new MemberData(node, MemberData.TargetType.FlowNode);
						},
						getPortTooltip = () => {
							return "The flow to execute the node";
						},
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
						getPortTooltip = () => {
							return "The result value";
						},
					}
				);
				if(owner.graphLayout == GraphLayout.Horizontal) {
					uNodeThreadUtility.Queue(() => nodeValuePort.BringToFront());
				}
			}
		}

		/// <summary>
		/// Initialize the port and control by using reflection, default this will be called by InitializeView
		/// </summary>
		protected void InitializeFields() {
			var node = targetNode;
			var fields = NodeEditorUtility.GetFieldNodes(node);
			if(fields.Length > 0) {
				try {
					var graphLayout = owner.graphLayout;
					bool isFlow = node is Node && (node as Node).IsFlowNode();
					//uNodePreference.enableEditOnNode = uNodePreference.GetPreference().preferredEditor == uNodePreference.PreferredEditor.Node;
					List<KeyValuePair<float, Action>> flowOutputPorts = new List<KeyValuePair<float, Action>>();
					for(int i = 0; i < fields.Length; i++) {
						var field = fields[i];
						if(field.attribute is FieldConnectionAttribute) {
							var FCA = field.attribute as FieldConnectionAttribute;
							if(FCA.hideOnFlowNode && isFlow) {
								continue;
							}
							if(FCA.hideOnNotFlowNode && !isFlow) {
								continue;
							}
							if(field.field.FieldType == typeof(MemberData)) {
								if(FCA.label == null) {
									FCA.label = new GUIContent(field.field.Name);
								}
								MemberData member = field.field.GetValueOptimized(node) as MemberData;
								if(member == null) {
									member = MemberData.none;
									field.field.SetValueOptimized(node, member);
								}
								if(FCA is FlowOutAttribute) {//Flow output
									float pos = 0;
									var tn = member.GetTargetNode();
									if(tn != null) {
										pos = tn.editorRect.x + tn.editorRect.width;
										if(tn is Nodes.MacroPortNode macroPort && macroPort.kind == PortKind.FlowInput) {
											var parentNode = macroPort.parentComponent as NodeComponent;
											if(parentNode != null) {
												pos = parentNode.editorRect.x + parentNode.editorRect.width;
											}
										}
									}
									flowOutputPorts.Add(new KeyValuePair<float, Action>(
										pos,
										() => {
											AddOutputFlowPort(
												new PortData() {
													portID = field.field.Name,
													onValueChanged = (o) => {
														RegisterUndo();
														field.field.SetValueOptimized(node, o);
													},
													getPortName = () => FCA.label.text,
													getPortType = () => typeof(FlowInput),
													getPortValue = () => field.field.GetValueOptimized(node) as MemberData,
													getPortTooltip = () => {
														var tooltip = ReflectionUtils.GetAttribute<TooltipAttribute>(field.attributes);
														return tooltip != null ? tooltip.tooltip : string.Empty;
													},
												}
											);
										}));
									if(graphLayout == GraphLayout.Horizontal) {
										flowOutputPorts[flowOutputPorts.Count - 1].Value();
									}
								} else {//Value input
									ReflectionUtils.TryCorrectingAttribute(node, ref field.attributes);
									FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(field.attributes);
									AddInputValuePort(
										new PortData() {
											portID = field.field.Name,
											filter = filter,
											onValueChanged = (o) => {
												RegisterUndo();
												field.field.SetValueOptimized(node, o);
											},
											getPortName = () => FCA.label.text,
											getPortValue = () => field.field.GetValueOptimized(node) as MemberData,
											getPortTooltip = () => {
												var tooltip = ReflectionUtils.GetAttribute<TooltipAttribute>(field.attributes);
												return tooltip != null ? tooltip.tooltip : string.Empty;
											},
										});
								}
							} else if(field.field.FieldType == typeof(List<MemberData>)) {
								List<MemberData> members = field.field.GetValueOptimized(node) as List<MemberData>;
								if(members == null) {
									members = new List<MemberData>();
									field.field.SetValueOptimized(node, members);
								}
								if(FCA is FlowOutAttribute) {
									flowOutputPorts.Add(new KeyValuePair<float, Action>(
										0,
										() => {
											for(int x = 0; x < members.Count; x++) {
												int index = x;
												AddOutputFlowPort(
													new PortData() {
														portID = field.field.Name,
														onValueChanged = (o) => {
															RegisterUndo();
															members[index] = o as MemberData;
															field.field.SetValueOptimized(node, members);
														},
														getPortType = () => typeof(FlowInput),
														getPortValue = () => members[index],
													}
												);
											}
										}));
									ControlView control = new ControlView();
									control.style.alignSelf = Align.Center;
									control.Add(new Button(() => {
										if(members.Count > 0) {
											RegisterUndo();
											members.RemoveAt(members.Count - 1);
											field.field.SetValueOptimized(node, members);
											MarkRepaint();
										}
									}) { text = "-" });
									control.Add(new Button(() => {
										RegisterUndo();
										members.Add(MemberData.none);
										field.field.SetValueOptimized(node, members);
										MarkRepaint();
									}) { text = "+" });
									AddControl(Direction.Input, control);
									if(graphLayout == GraphLayout.Horizontal) {
										flowOutputPorts[flowOutputPorts.Count - 1].Value();
									}
								} else {
									ReflectionUtils.TryCorrectingAttribute(node, ref field.attributes);
									FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(field.attributes);
									if(filter == null) {
										filter = new FilterAttribute();
									}
									for(int x = 0; x < members.Count; x++) {
										int index = x;
										AddInputValuePort(
											new PortData() {
												portID = field.field.Name + "#" + index,
												filter = filter,
												onValueChanged = (o) => {
													RegisterUndo();
													members[index] = o as MemberData;
												},
												getPortName = () => "",
												getPortValue = () => members[index],
											}
										);
									}
								}
							} else {
								var obj = field.field.GetValueOptimized(node);
								if(obj is MultipurposeMember multipurposeMember) {
									AddMultipurposeMemberPort(multipurposeMember, (o) => {
										field.field.SetValueOptimized(node, o);
									}, node);
								} else {
									throw new System.Exception("Can't draw connection for field: " + field.field.Name + "\nFieldType not supported.");
								}
							}
						} else if(field.attribute is ValueOutAttribute) {//Draw label on right.
							var FVA = field.attribute as ValueOutAttribute;
							if(FVA.type == null) {
								FVA.type = field.field.FieldType;
							}
							ReflectionUtils.TryCorrectingAttribute(node, ref field.attributes);
							FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(field.attributes);
							AddOutputValuePort(
								new PortData() {
									portID = field.field.Name,
									onValueChanged = (o) => {
										RegisterUndo();
										field.field.SetValueOptimized(node, o);
									},
									getPortName = () => FVA.label.text,
									getPortType = () => filter?.GetActualType() ?? FVA.type,
									getConnection = () => {
										return new MemberData(new object[] { node, field.field }, MemberData.TargetType.NodeField);
									},
									getPortTooltip = () => {
										var tooltip = ReflectionUtils.GetAttribute<TooltipAttribute>(field.attributes);
										return tooltip != null ? tooltip.tooltip : string.Empty;
									},
								}
							);
						} else if(field.attribute is FieldDrawerAttribute) {//Draw label on left.
							var FD = field.attribute as FieldDrawerAttribute;
							if(FD.label == null) {
								FD.label = new GUIContent(field.field.Name);
							}
							var obj = field.field.GetValueOptimized(node);
							/*if(uNodePreference.enableEditOnNode)*/
							{
								ReflectionUtils.TryCorrectingAttribute(node, ref field.attributes);
								FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(field.attributes);
								var element = new ControlView();
								element.Add(new Label(FD.label.text));
								{
									var controlAtts = UIElementUtility.FindControlsField();
									ValueControl control;
									ControlConfig config = new ControlConfig() {
										owner = this,
										value = obj,
										type = field.field.FieldType,
										filter = filter,
										onValueChanged = (val) => {
											RegisterUndo();
											field.field.SetValueOptimized(node, val);
											MarkDirtyRepaint();
										},
									};
									control = UIElementUtility.CreateControl(field.field.FieldType, config);
									element.Add(control);
								}
								AddControl(Direction.Input, element);
							} /*else {*/
							  //	var element = new ControlView();
							  //	element.Add(new Label(FD.label.text));
							  //	AddControl(Direction.Input, element);
							  //}
						} else if(field.field.FieldType == typeof(FlowInput)) {//Draw label on left.
							string name = field.field.Name;
							FlowInput flowInput = field.field.GetValueOptimized(node) as FlowInput;
							if(flowInput != null && !string.IsNullOrEmpty(flowInput.name)) {
								name = flowInput.name;
							}
							AddInputFlowPort(
								new PortData() {
									portID = field.field.Name,
									getPortName = () => name,
									getConnection = () => MemberData.FlowInput(targetNode as Node, field.field.Name),
									getPortTooltip = () => {
										if(field.field.IsDefined(typeof(TooltipAttribute), true)) {
											var tooltip = field.field.GetCustomAttribute<TooltipAttribute>();
											return tooltip != null ? tooltip.tooltip : string.Empty;
										}
										return string.Empty;
									},
								}
							);
						}
					}
					if(graphLayout == GraphLayout.Vertical && flowOutputPorts.Count > 0) {
						flowOutputPorts.Sort((x, y) => {
							if(x.Key == 0 || y.Key == 0) {
								return 0;
							}
							if(x.Key < y.Key) {
								return -1;
							} else if(x.Key == y.Key) {
								return 0;
							} else {
								return 1;
							}
							//return string.CompareOrdinal(x.Key.ToString(), y.Key.ToString());
							//if(val == 0) {
							//	return string.CompareOrdinal(x.sKey.y.ToString(), y.Key.y.ToString());
							//}
							//return val;
						});
						for(int i = 0; i < flowOutputPorts.Count; i++) {
							flowOutputPorts[i].Value();
						}
					}
				}
				catch(System.Exception ex) {
					uNodeDebug.LogException(ex, node);
					Debug.Log("error on initialize field in node.", node);
					Debug.LogException(ex);
				}
				//uNodePreference.enableEditOnNode = true;
			}
		}

		/// <summary>
		/// Called inside ReloadView
		/// </summary>
		protected virtual void InitializeView() {
			if(!(targetNode is Node)) {
				return;
			}
			InitializeDefaultPort();
			InitializeFields();
			InitializeOutputExtenderPorts();
			//InitializeInputExtenderPorts();
		}
		#endregion

		#region Functions
		public override void ReloadView() {
			try {
				base.ReloadView();
				title = targetNode.GetNodeName();
				if(titleIcon != null)
					titleIcon.image = uNodeEditorUtility.GetTypeIcon(targetNode.GetNodeIcon());
				InitializeView();
			} catch (Exception ex) {
				uNodeDebug.LogException(ex, targetNode);
				Debug.LogException(ex, targetNode);
			}
			Teleport(targetNode.editorRect);

			#region Debug
			if (debugView == null && targetNode is Node &&
				(Application.isPlaying && nodeFlowPort != null || GraphDebug.HasBreakpoint(uNodeUtility.GetObjectID(targetNode)))) {
				debugView = new VisualElement() {
					name = "debug-container"
				};
				titleButtonContainer.Add(debugView);
			}
			if(Application.isPlaying && targetNode is Node && nodeFlowPort != null) {
				this.RegisterRepaintAction(() => {
					var debugData = owner.graph.GetDebugInfo();
					if(debugData != null && debugData.nodeDebug.ContainsKey(uNodeUtility.GetObjectID(targetNode))) {
						var nodeDebug = debugData.nodeDebug[uNodeUtility.GetObjectID(targetNode)];
						if(nodeDebug != null) {
							var layout = new Rect(5, -8, 8, 8);
							switch(nodeDebug.nodeState) {
								case StateType.Success:
									GUI.DrawTexture(layout, uNodeEditorUtility.MakeTexture(1, 1, 
										Color.Lerp(
											UIElementUtility.Theme.nodeRunningColor, 
											UIElementUtility.Theme.nodeSuccessColor, 
											(Time.unscaledTime - nodeDebug.calledTime) * 2)
										)
									);
									break;
								case StateType.Failure:
									GUI.DrawTexture(layout, uNodeEditorUtility.MakeTexture(1, 1,
										Color.Lerp(
											UIElementUtility.Theme.nodeRunningColor,
											UIElementUtility.Theme.nodeFailureColor,
											(Time.unscaledTime - nodeDebug.calledTime) * 2)
										)
									);
									break;
								case StateType.Running:
									GUI.DrawTexture(layout, uNodeEditorUtility.MakeTexture(1, 1, UIElementUtility.Theme.nodeRunningColor));
									break;
							}
						}
					}
				});
			}
			if (debugView != null) {
				this.ExecuteAndScheduleAction(() => {
					if (!this.IsVisible())
						return;
					bool hasBreakpoint = GraphDebug.HasBreakpoint(uNodeUtility.GetObjectID(targetNode));
					if (hasBreakpoint) {
						debugView.style.backgroundColor = UIElementUtility.Theme.breakpointColor;
						debugView.style.width = 16;
						debugView.style.height = 16;
						debugView.style.position = Position.Relative;
						debugView.visible = true;
					} else {
						debugView.style.position = Position.Absolute;
						debugView.style.width = 0;
						debugView.style.height = 0;
						debugView.visible = false;
					}
				}, 50);
			}
			#endregion

			#region Node Styles
			if (border != null) {
				border.SetToNoClipping();
				int flowInputCount = inputPorts.Count((p) => p.isFlow);
				int flowOutputCount = outputPorts.Count((p) => p.isFlow);
				if (flowInputCount + flowOutputCount > 0) {
					bool flag = outputPorts.Count((p) => p.isFlow && !string.IsNullOrEmpty(p.GetName())) == 0;
					border.EnableInClassList("flowNodeBorder", true);
					if (flowOutputCount == 0 || flag) {
						border.EnableInClassList("onlyInput", true);
						border.EnableInClassList("onlyOutput", false);
					} else if (flowInputCount == 0) {
						border.EnableInClassList("onlyInput", false);
						border.EnableInClassList("onlyOutput", true);
					} else {
						border.EnableInClassList("onlyInput", false);
						border.EnableInClassList("onlyOutput", false);
					}
					portInputContainer.EnableInClassList("flow", flowInputCount > 0);
				} else {
					border.EnableInClassList("flowNodeBorder", false);
				}
				if (targetNode is Node) {
					Node node = targetNode as Node;
					if (node.parentComponent is GroupNode) {
						GroupNode gn = node.parentComponent as GroupNode;
						if (gn.nodeToExecute == targetNode) {
							this.AddToClassList("start-node");
						}
					} else if (node.parentComponent is RootObject) {
						RootObject ro = node.parentComponent as RootObject;
						if (ro.startNode == targetNode) {
							this.AddToClassList("start-node");
						}
					}
				}
			}
			#endregion

			if (titleIcon != null) {
				titleIcon.EnableInClassList("ui-hidden", titleIcon.image == null);
			}

			RefreshPorts();
			if (ShowExpandButton()) {
				expanded = targetNode.nodeExpanded;
			}
		}

		/// <summary>
		/// Initialize default compact node style
		/// </summary>
		protected void ConstructCompactStyle(bool displayIcon = true) {
			compactIcon?.RemoveFromHierarchy();
			EnableInClassList("compact-value", true);
			var element = this.Q("top");
			if(element != null) {
				if (displayIcon) {
					compactIcon = new Image() {
						image = uNodeEditorUtility.GetTypeIcon(targetNode.GetNodeIcon())
					};
					compactIcon.AddToClassList("compact-icon");
					element.Insert(2, compactIcon);
				}
				foreach(var p in outputPorts) {
					p.portName = "";
				}
			}
		}

		/// <summary>
		/// Construct compact title node, invoke it after initializing view
		/// </summary>
		/// <param name="inputValuePort"></param>
		/// <param name="outputValuePort"></param>
		protected void ConstructCompactTitle(string inputValuePort = "Instance", string outputValuePort = UGraphView.SelfPortID, ControlView control = null) {
			var firstPort = inputPorts.FirstOrDefault(p => p != null && p.GetPortID() == inputValuePort);
			if(firstPort != null) {
				firstPort.RemoveFromHierarchy();
				firstPort.AddToClassList("compact-input");
				titleContainer.Insert(0, firstPort);
				EnableInClassList("compact-node", true);
			}
			if(control != null) {
				control.RemoveFromHierarchy();
				control.AddToClassList("compact-control");
				titleContainer.Add(control);
				EnableInClassList("compact-node", true);
			}
			firstPort = outputPorts.FirstOrDefault(p => p != null && p.GetPortID() == outputValuePort);
			if(firstPort != null) {
				firstPort.RemoveFromHierarchy();
				firstPort.AddToClassList("compact-output");
				titleContainer.Add(firstPort);
				EnableInClassList("compact-node", true);
			}
		}

		protected void AddMultipurposeMemberPort(MultipurposeMember member, Action<MultipurposeMember> onChanged, NodeComponent node) {
			if (member != null && member.target != null) {
				MemberDataUtility.UpdateMultipurposeMember(member);
				if (member.target.targetType == MemberData.TargetType.Values) {
					if (member.target.type != null) {
						AddControl(Direction.Input, new MemberControl(new ControlConfig() {
							value = member.target,
							owner = this,
							type = member.target.type,
							onValueChanged = (o) => {
								RegisterUndo();
								member.target = o as MemberData;
								onChanged?.Invoke(member);
							},
						}));
					}
				} else if (
					member.target.isTargeted && !member.target.isStatic &&
					!member.target.IsTargetingUNode &&
					member.target.targetType != MemberData.TargetType.Type &&
					(member.target.targetType != MemberData.TargetType.SelfTarget || member.target.GetInstance() != targetNode.owner as object) &&
					member.target.targetType != MemberData.TargetType.Null) {
					MemberDataUtility.UpdateMemberInstance(member.target, member.target.startType);
					AddInputValuePort(
						new PortData() {
							portID = "Instance",
							onValueChanged = (o) => {
								RegisterUndo();
								member.target.instance = o;
								onChanged?.Invoke(member);
							},
							filter = new FilterAttribute(member.target.startType),
							getPortName = () => "Instance",
							getPortType = () => member.target.startType,
							getPortValue = () => {
								var obj = member.target?.instance;
								if(obj is MemberData) {
									return obj as MemberData;
								}
								return obj == null ? null : MemberData.CreateFromValue(obj);
							},
							getPortTooltip = () => "The instance value",
						});
				}
				if (member.parameters != null && member.parameters.Length > 0 && member.target.SerializedItems?.Length > 0) {
					MemberInfo[] members;
					{//For documentation
						members = member.target.GetMembers(false);
						if (members != null && members.Length > 0 && members.Length + 1 != member.target.SerializedItems.Length) {
							members = null;
						}
					}
					uNodeFunction objRef = null;
					switch (member.target.targetType) {
						case MemberData.TargetType.uNodeFunction: {
								uNodeRoot root = member.target.GetInstance() as uNodeRoot;
								if (root != null) {
									var gTypes = MemberData.Utilities.SafeGetGenericTypes(member.target)[0];
									objRef = root.GetFunction(member.target.startName, gTypes != null ? gTypes.Length : 0, MemberData.Utilities.SafeGetParameterTypes(member.target)[0]);
								}
								break;
							}
					}
					int totalParam = 0;
					bool flag = false;
					for (int z = 0; z < member.target.SerializedItems.Length; z++) {
						if (z != 0) {
							if (members != null && (member.target.isDeepTarget || !member.target.IsTargetingUNode)) {
								MemberInfo mData = members[z - 1];
								if (mData is MethodInfo || mData is ConstructorInfo) {
									var method = mData as MethodInfo;
									var parameters = method != null ? method.GetParameters() : (mData as ConstructorInfo).GetParameters();
									if (parameters.Length > 0) {
										totalParam++;
										if (totalParam > 1) {
											flag = true;
											break;
										}
									}
								}
							}
						}
					}
					totalParam = 0;
					int methodDrawCount = 1;
					for (int z = 0; z < member.target.SerializedItems.Length; z++) {
						if (members == null) {
							Type[] paramsType = MemberData.Utilities.SafeGetParameterTypes(member.target)[z];
							if (paramsType != null && paramsType.Length > 0) {
								if (flag) {
									AddControl(Direction.Input, new Label("Method " + (methodDrawCount)));
									methodDrawCount++;
								}
								for (int x = 0; x < paramsType.Length; x++) {
									System.Type PType = paramsType[x];
									MemberData mParam = member.parameters[totalParam];
									if (mParam == null) {
										if (ReflectionUtils.CanCreateInstance(PType)) {
											mParam = MemberData.CreateValueFromType(PType);
										} else {
											mParam = MemberData.none;
										}
										member.parameters[totalParam] = mParam;
									}
									if (PType != null) {
										int index = totalParam;
										string label;
										if (objRef != null) {
											label = objRef.parameters[x].name;
										} else {
											label = "P" + x;
										}
										AddInputValuePort(
											new PortData() {
												portID = "P" + (z + x),
												onValueChanged = (o) => {
													RegisterUndo();
													mParam = o as MemberData;
													member.parameters[index] = mParam;
													onChanged?.Invoke(member);
												},
												filter = new FilterAttribute(PType),
												getPortName = () => label,
												getPortType = () => PType,
												getPortValue = () => mParam,
											});
									}
									totalParam++;
								}
							}
						} else if (z != 0) {
							if (members != null && (member.target.isDeepTarget || !member.target.IsTargetingUNode)) {
								MemberInfo mData = members[z - 1];
								if (mData is MethodInfo || mData is ConstructorInfo) {
									var method = mData as MethodInfo;
									var parameters = method != null ? method.GetParameters() : (mData as ConstructorInfo).GetParameters();
									if (parameters.Length > 0) {
										if (flag) {
											AddControl(Direction.Input, new Label(method.Name));
										}
										for (int x = 0; x < parameters.Length; x++) {
											System.Type PType = parameters[x].ParameterType;
											MemberData mParam = member.parameters[totalParam];
											if (mParam == null) {
												if (ReflectionUtils.CanCreateInstance(PType)) {
													mParam = MemberData.CreateValueFromType(PType);
												} else {
													mParam = MemberData.none;
												}
												member.parameters[totalParam] = mParam;
											}
											if (PType != null) {
												int index = totalParam;
												AddInputValuePort(
													new PortData() {
														portID = "P" + (z + x),
														onValueChanged = (o) => {
															RegisterUndo();
															mParam = o as MemberData;
															member.parameters[index] = mParam;
															onChanged?.Invoke(member);
														},
														filter = new FilterAttribute(PType),
														getPortName = () => parameters[x].Name,
														getPortType = () => PType,
														getPortValue = () => mParam,
													});
											}
											totalParam++;
										}
										continue;
									}
								}
							}
						}
					}
				}
				if (member.target.targetType == MemberData.TargetType.Values || member.target.targetType == MemberData.TargetType.Constructor) {
					if (member.initializer != null && member.initializer.Value as ConstructorValueData != null) {
						ConstructorValueData ctor = member.initializer.Value as ConstructorValueData;
						if (ctor.initializer != null && ctor.initializer.Length > 0) {
							AddControl(Direction.Input, new Label("Initializer"));
							for (int x = 0; x < ctor.initializer.Length; x++) {
								var param = ctor.initializer[x];
								int index = x;
								Type type = param.type;
								AddInputValuePort(
									new PortData() {
										portID = "ctor" + (index),
										onValueChanged = (o) => {
											RegisterUndo();
											param.value = o;
											ctor.initializer[index] = param;
											//Serialize the changed values.
											member.initializer.Value = ctor;
											onChanged?.Invoke(member);
										},
										filter = new FilterAttribute(type),
										getPortName = () => param.name,
										getPortType = () => type,
										getPortValue = () => {
											var obj = param.value;
											if(obj is MemberData) {
												return obj as MemberData;
											}
											return obj == null ? null : MemberData.CreateFromValue(obj);
										},
									});
							}
						}
					}
				}
			}
		}
		#endregion

		#region Callbacks & Overrides
		public override void SetPosition(Rect newPos) {
			// if (newPos != targetNode.editorRect && preference.snapNode) {
			// 	float range = preference.snapRange;
			// 	newPos.x = NodeEditorUtility.SnapTo(newPos.x, range);
			// 	newPos.y = NodeEditorUtility.SnapTo(newPos.y, range);
			// 	if (preference.snapToPin && owner.selection.Count == 1) {
			// 		var connectedPort = inputPorts.Where((p) => p.connected).ToList();
			// 		bool hFlag = false;
			// 		bool vFalg = false;
			// 		var snapRange = preference.snapToPinRange / uNodePreference.nodeGraph.zoomScale;
			// 		for (int i = 0; i < connectedPort.Count; i++) {
			// 			var edges = connectedPort[i].GetEdges();
			// 			if (connectedPort[i].orientation == Orientation.Vertical) {
			// 				if (vFalg)
			// 					continue;
			// 				foreach (var e in edges) {
			// 					if (e != null) {
			// 						float distanceToPort = e.input.GetGlobalCenter().x - e.output.GetGlobalCenter().x;
			// 						if (Mathf.Abs(distanceToPort) <= snapRange && Mathf.Abs(newPos.x - layout.x) <= snapRange) {
			// 							newPos.x = layout.x - distanceToPort;
			// 							vFalg = true;
			// 							break;
			// 						}
			// 					}
			// 				}
			// 			} else {
			// 				//if(hFlag || vFalg)
			// 				//	continue;
			// 				//foreach(var e in edges) {
			// 				//	if(e != null) {
			// 				//		float distanceToPort = e.edgeControl.to.y - e.edgeControl.from.y;
			// 				//		if(Mathf.Abs(distanceToPort) <= preference.snapToPinRange &&
			// 				//			Mathf.Abs(newPos.y - layout.y) <= preference.snapToPinRange) {
			// 				//			newPos.y = layout.y - distanceToPort;
			// 				//			hFlag = true;
			// 				//			break;
			// 				//		}
			// 				//	}
			// 				//}
			// 			}
			// 		}
			// 		connectedPort = outputPorts.Where((p) => p.connected).ToList();
			// 		for (int i = 0; i < connectedPort.Count; i++) {
			// 			var edges = connectedPort[i].GetEdges();
			// 			if (connectedPort[i].orientation == Orientation.Vertical) {
			// 				//if(vFalg)
			// 				//	continue;
			// 				//foreach(var e in edges) {
			// 				//	if(e != null) {
			// 				//		float distanceToPort = e.edgeControl.to.x - e.edgeControl.from.x;
			// 				//		if(Mathf.Abs(distanceToPort) <= preference.snapToPinRange &&
			// 				//			Mathf.Abs(newPos.x - layout.x) <= preference.snapToPinRange) {
			// 				//			newPos.x = layout.x + distanceToPort;
			// 				//			break;
			// 				//		}
			// 				//	}
			// 				//}
			// 			} else {
			// 				if (hFlag || vFalg)
			// 					continue;
			// 				foreach (var e in edges) {
			// 					if (e != null) {
			// 						float distanceToPort = e.input.GetGlobalCenter().y - e.output.GetGlobalCenter().y;
			// 						if (Mathf.Abs(distanceToPort) <= snapRange && Mathf.Abs(newPos.y - layout.y) <= snapRange) {
			// 							newPos.y = layout.y + distanceToPort;
			// 							break;
			// 						}
			// 					}
			// 				}
			// 			}
			// 		}
			// 	}
			// }
			float xPos = newPos.x - targetNode.editorRect.x;
			float yPos = newPos.y - targetNode.editorRect.y;

			base.SetPosition(newPos);
			targetNode.editorRect = newPos;
			if(owner.selection.Count == 1 && owner.selection.Contains(this) && owner.currentEvent != null) {
				if((xPos != 0 || yPos != 0)) {
					var preference = uNodePreference.GetPreference();
					if(preference.carryNodes) {
						if(owner.currentEvent.modifiers.HasFlags(EventModifiers.Control | EventModifiers.Command)) {
							return;
						}
					} else {
						if(!owner.currentEvent.modifiers.HasFlags(EventModifiers.Control | EventModifiers.Command)) {
							return;
						}
					}
					List<UNodeView> nodes = new List<UNodeView>();
					nodes.AddRange(UIElementUtility.FindConnectedFlowNodes(this));
					nodes.AddRange(UIElementUtility.FindConnectedInputValueNodes(this));
					foreach(var n in nodes.Distinct()) {
						if(n != null) {
							Rect rect = n.targetNode.editorRect;
							rect.x += xPos;
							rect.y += yPos;
							n.Teleport(rect);
						}
					}
				}
			}
		}

		/// <summary>
		/// Do update every 0.5 second.
		/// </summary>
		protected virtual void DoUpdate() {
			if(!this.IsVisible()) return;
			#region Error
			if (uNodeEditor.editorErrors != null && uNodeEditor.editorErrors.ContainsKey(targetNode)) {
				var errors = uNodeEditor.editorErrors[targetNode];
				if (errors != null && errors.Count > 0) {
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					for (int i = 0; i < errors.Count; i++) {
						if (i != 0) {
							sb.AppendLine();
							sb.AppendLine();
						}
						sb.Append("-" + uNodeEditorUtility.RemoveHTMLTag(errors[i].message));
					}
					UpdateError(sb.ToString());
				} else {
					UpdateError("");
				}
			} else {
				UpdateError("");
			}
			#endregion
		}
		#endregion
	}

	public class BlockControl : VisualElement {
		public Label label;
		public ControlView control;

		public BlockControl(string label, ControlView control) {
			style.flexDirection = FlexDirection.Row;

			this.label = new Label(ObjectNames.NicifyVariableName(label));
			this.label.style.width = 100;
			Add(this.label);
			if (control != null) {
				this.control = control;
				Add(control);
			}
		}

		public BlockControl(Label label, ControlView control) {
			style.flexDirection = FlexDirection.Row;

			this.label = label;
			this.label.style.width = 100;
			Add(this.label);
			if (control != null) {
				this.control = control;
				Add(control);
			}
		}
	}

	public class ControlView : VisualElement {
		public ValueControl control;

		// public new bool visible {
		// 	get {
		// 		return !this.IsFaded();
		// 	}
		// 	set {
		// 		this.SetOpacity(value);
		// 	}
		// }

		public ControlView() { }

		public ControlView(VisualElement visualElement, bool autoLayout = false) {
			Add(visualElement);
			ToggleLayout(autoLayout);
		}

		public ControlView(ValueControl valueControl, bool autoLayout = false) {
			Add(valueControl);
			control = valueControl;
			ToggleLayout(autoLayout);
		}

		public void ToggleLayout(bool value) {
			EnableInClassList("Layout", value);
			if (control != null)
				control.EnableInClassList("Layout", value);
		}
	}
}