using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace MaxyGames.uNode.Editors {
	public class BlockView : NodeView {
		public EventActionData data;
		public INodeBlock owner;
		public UNodeView ownerNode => owner.nodeView;

		protected RichLabel richTitle;
		public List<PortView> portViews = new List<PortView>();
		public List<BlockControl> controls = new List<BlockControl>();

		public BlockView() {
			AddToClassList("block");
			this.AddStyleSheet("uNodeStyles/NativeBlockStyle");
			this.AddStyleSheet(UIElementUtility.Theme.blockStyle);
			pickingMode = PickingMode.Position;

			capabilities &= ~Capabilities.Ascendable;
			capabilities |= Capabilities.Selectable;

			style.position = Position.Relative;
		}

		public void Initialize(EventActionData data, INodeBlock owner) {
			this.data = data;
			this.owner = owner;
			base.expanded = true;

			if(data.eventType == EventActionData.EventType.Or) {
				title = "OR";
				EnableInClassList("or-block", true);
				m_CollapseButton.RemoveFromHierarchy();
			} else {
				if(UIElementGraph.richText) {
					this.Q("title-label").RemoveFromHierarchy();
					richTitle = new RichLabel {
						name = "title-label"
					};
					titleContainer.Insert(0, richTitle);
				}
				if(richTitle != null) {
					title = "";
					richTitle.text = data.displayName;
				}
			}
			Repaint();
			try {
				InitializeView();
			}
			catch(System.Exception ex) {
				uNodeDebug.LogException(ex, owner.nodeView.targetNode);
				Debug.LogException(ex, owner.nodeView.targetNode);
			}

			RefreshPorts();
			expanded = data.expanded;
		}

		public void RegisterUndo(string name = "") {
			owner.nodeView.RegisterUndo(name);
		}

		void Repaint() {
			if(data.eventType == EventActionData.EventType.Event) {
				if(richTitle != null) {
					richTitle.text = data.displayName;
					richTitle.MarkDirtyLayout();
				} else {
					title = uNodeEditorUtility.RemoveHTMLTag(data.displayName);
				}
			}
		}

		public override bool expanded {
			get {
				return base.expanded;
			}
			set {
				data.expanded = value;
				if(base.expanded != value) {
					RefreshControl(value);
				}
				base.expanded = value;
			}
		}

		protected void RefreshControl(bool isVisible) {
			//inputContainer.style.overflow = isVisible ? Overflow.Visible : Overflow.Hidden;
			foreach(var c in controls) {
				c.visible = isVisible;
				c.SetElementVisibility(isVisible);
			}
		}

		void InitializeHLBlock(Block block) {
			if (block is Events.HLAction action) {
				Type type = action.type.startType;
				if (type != null) {
					var fields = EditorReflectionUtility.GetFields(type);
					foreach (var field in fields) {
						if (field.IsDefined(typeof(NonSerializedAttribute)) || field.IsDefined(typeof(HideAttribute))) continue;
						var option = field.GetCustomAttribute(typeof(NodePortAttribute), true) as NodePortAttribute;
						if (option != null && option.hideInNode) continue;
						var val = action.initializers.FirstOrDefault(d => d.name == field.Name);
						if (val == null) {
							val = new FieldValueData() {
								name = field.Name,
								value = MemberData.CreateFromValue(field.GetValueOptimized(ReflectionUtils.CreateInstance(type)), field.FieldType),
							};
							action.initializers.Add(val);
						}
						var port = AddPort(new PortData() {
							portID = field.Name,
							getPortName = () => option != null ? option.name : field.Name,
							getPortType = () => field.FieldType,
							getPortValue = () => val.value,
							onValueChanged = (obj) => {
								RegisterUndo();
								val.value = obj as MemberData;
								Repaint();
							},
							getPortTooltip = () => {
								var tooltip = field.GetCustomAttribute(typeof(TooltipAttribute), true) as TooltipAttribute;
								return tooltip != null ? tooltip.tooltip : string.Empty;
							},
						});
						port.Add(new ControlView(port.portData.InstantiateControl(true), true));
					}
				}
			} else if (block is Events.HLCondition condition) {
				Type type = condition.type.startType;
				if (type != null) {
					var fields = EditorReflectionUtility.GetFields(type);
					foreach (var field in fields) {
						if (field.IsDefined(typeof(NonSerializedAttribute)) || field.IsDefined(typeof(HideAttribute))) continue;
						var option = field.GetCustomAttribute(typeof(NodePortAttribute), true) as NodePortAttribute;
						if (option != null && option.hideInNode) continue;
						var val = condition.initializers.FirstOrDefault(d => d.name == field.Name);
						if (val == null) {
							val = new FieldValueData() {
								name = field.Name,
								value = MemberData.CreateFromValue(field.GetValueOptimized(ReflectionUtils.CreateInstance(type)), field.FieldType),
							};
							condition.initializers.Add(val);
						}
						var port = AddPort(new PortData() {
							portID = field.Name,
							getPortName = () => option != null ? option.name : field.Name,
							getPortType = () => field.FieldType,
							getPortValue = () => val.value,
							onValueChanged = (obj) => {
								RegisterUndo();
								val.value = obj as MemberData;
								Repaint();
							},
							getPortTooltip = () => {
								var tooltip = field.GetCustomAttribute(typeof(TooltipAttribute), true) as TooltipAttribute;
								return tooltip != null ? tooltip.tooltip : string.Empty;
							},
						});
						port.Add(new ControlView(port.portData.InstantiateControl(true), true));
					}
				}
			}
		}

		void InitializeView() {
			if(data.block == null)
				return;
			if(data.block is Events.HLAction || data.block is Events.HLCondition) {
				InitializeHLBlock(data.block);
				return;
			}
			var fields = EditorReflectionUtility.GetFields(data.block.GetType());
			if(fields != null && fields.Length > 0) {
				for(int idx = 0; idx < fields.Length; idx++) {
					FieldInfo field = fields[idx];
					if(uNodeGUIUtility.IsHide(field, data.block))
						continue;
					Type type = field.FieldType;
					if(type == typeof(MemberData)) {
						MemberData member = field.GetValueOptimized(data.block) as MemberData;
						object[] fieldAttribute = field.GetCustomAttributes(true);
						if(!ReflectionUtils.TryCorrectingAttribute(data.block, ref fieldAttribute)) {
							continue;
						}
						var filter = ReflectionUtils.GetAttribute<FilterAttribute>(fieldAttribute);
						if(member == null) {
							if(filter != null && !filter.SetMember && !filter.OnlyGetType && ReflectionUtils.CanCreateInstance(filter.GetActualType())) {
								member = MemberData.CreateValueFromType(filter.GetActualType());
							} else if(filter != null && !filter.SetMember && !filter.OnlyGetType) {
								member = MemberData.none;
							}
							field.SetValueOptimized(data.block, member);
							Repaint();
						}
						bool hasDependencies = EditorReflectionUtility.HasFieldDependencies(new string[] { field.Name }, fields);
						var port = AddPort(new PortData() {
							getPortName = () => ObjectNames.NicifyVariableName(field.Name),
							getPortType = () => filter?.GetActualType() ?? typeof(object),
							getPortValue = () => field.GetValueOptimized(data.block) as MemberData,
							filter = filter,
							onValueChanged = (obj) => {
								RegisterUndo();
								member = obj as MemberData;
								field.SetValueOptimized(data.block, member);
								Repaint();
								if(hasDependencies) {
									owner.nodeView.MarkRepaint();
								}
							},
						});
						port.Add(new ControlView(port.portData.InstantiateControl(true), true));
					} else if(type == typeof(MultipurposeMember)) {
						InitMultipurposeMember(field);
					} else {
						object fieldVal = field.GetValueOptimized(data.block);
						object[] fieldAttribute = field.GetCustomAttributes(true);
						if(!ReflectionUtils.TryCorrectingAttribute(data.block, ref fieldAttribute)) {
							continue;
						}
						bool hasDependencies = EditorReflectionUtility.HasFieldDependencies(new string[] { field.Name }, fields);
						var filter = ReflectionUtils.GetAttribute<FilterAttribute>(fieldAttribute);
						ControlConfig config = new ControlConfig() {
							owner = owner.nodeView,
							value = fieldVal,
							type = type,
							filter = filter,
							onValueChanged = (obj) => {
								RegisterUndo();
								field.SetValueOptimized(data.block, obj);
								Repaint();
								if(hasDependencies) {
									owner.nodeView.MarkRepaint();
								}
							},
						};
						var valueControl = UIElementUtility.CreateControl(type, config, true);
						AddControl(field.Name, new ControlView(valueControl, true));
						if(fieldVal is IList<MemberData>) {
							IList<MemberData> members = fieldVal as IList<MemberData>;
							if(members != null) {
								for(int i = 0; i < members.Count; i++) {
									int index = i;
									var member = members[i];
									if(member == null) {
										if(filter != null && !filter.SetMember && !filter.OnlyGetType &&
											ReflectionUtils.CanCreateInstance(filter.GetActualType())) {
											member = MemberData.CreateValueFromType(filter.GetActualType());
										} else {
											member = MemberData.none;
										}
										members[index] = member;
										field.SetValueOptimized(data.block, members);
										Repaint();
									}
									var port = AddPort(new PortData() {
										getPortName = () => "Element " + index.ToString(),
										getPortType = () => filter?.GetActualType() ?? typeof(object),
										getPortValue = () => (field.GetValueOptimized(data.block) as IList<MemberData>)[index],
										filter = filter,
										onValueChanged = (obj) => {
											RegisterUndo();
											member = obj as MemberData;
											members[index] = member;
											field.SetValueOptimized(data.block, members);
											Repaint();
										},
									});
									port.Add(new ControlView(port.portData.InstantiateControl(true), true));
								}
							}
						} else if(fieldVal is IList) {
							IList list = fieldVal as IList;
							if(list != null) {
								for(int i = 0; i < list.Count; i++) {
									int index = i;
									var element = list[i];
									ControlConfig cfg = new ControlConfig() {
										owner = owner.nodeView,
										value = fieldVal,
										type = type,
										filter = filter,
										onValueChanged = (obj) => {
											RegisterUndo();
											field.SetValueOptimized(data.block, obj);
											Repaint();
										},
									};
									var elementControl = UIElementUtility.CreateControl(type, cfg, true);
									AddControl("Element " + index, new ControlView(elementControl, true));
								}
							}
						}
					}
				}
			}
		}

		void InitMultipurposeMember(FieldInfo field) {
			MultipurposeMember member = field.GetValueOptimized(data.block) as MultipurposeMember;
			object[] fieldAttribute = field.GetCustomAttributes(true);
			if(!ReflectionUtils.TryCorrectingAttribute(data.block, ref fieldAttribute)) {
				return;
			}
			if(member == null) {
				member = new MultipurposeMember();
				field.SetValueOptimized(data.block, member);
				Repaint();
			}
			if(member.target == null) {
				member.target = MemberData.none;
			}
			if(member.target != null) {
				var filter = ReflectionUtils.GetAttribute<FilterAttribute>(fieldAttribute) ?? new FilterAttribute() {
					MaxMethodParam = int.MaxValue,
				};
				var port = AddPort(new PortData() {
					getPortName = () => field.Name,
					getPortType = () => typeof(object),
					getPortValue = () => member.target,
					filter = filter,
					onValueChanged = (obj) => {
						var val = obj as MemberData;
						RegisterUndo();
						if(member.target.targetType != val.targetType || val.targetType != MemberData.TargetType.Values) {
							member.target = val;
							owner.nodeView.MarkRepaint();
							MemberDataUtility.UpdateMultipurposeMember(member);
						} else {
							member.target = val;
						}
						field.SetValueOptimized(data.block, member);
						Repaint();
					},
				});
				var control = port.portData.InstantiateControl(true);
				control.HideInstance(true);
				port.Add(new ControlView(control, true));
			}
			if(member.target.isTargeted) {
				if(member.target.targetType != MemberData.TargetType.Values) {
					if(!member.target.isStatic &&
						!member.target.IsTargetingUNode &&
						member.target.targetType != MemberData.TargetType.Type &&
						member.target.targetType != MemberData.TargetType.Null) {
						MemberDataUtility.UpdateMemberInstance(member.target, member.target.startType);
						var port = AddPort(new PortData() {
							portID = "Instance",
							getPortName = () => "Instance",
							getPortType = () => member.target.startType,
							getPortValue = () => {
								var obj = member.target?.instance;
								if(obj is MemberData) {
									return obj as MemberData;
								}
								return obj == null ? null : MemberData.CreateFromValue(obj);
							},
							filter = new FilterAttribute(member.target.startType),
							onValueChanged = (o) => {
								RegisterUndo();
								member.target.instance = o;
								field.SetValueOptimized(data.block, member);
							},
						});
						port.Add(new ControlView(port.portData.InstantiateControl(true), true));
						port.SetPortDepth(1);
					}
				}
				if(member.target.SerializedItems?.Length > 0) {
					MemberInfo[] members;
					{//For documentation
						members = member.target.GetMembers(false);
						if(members != null && members.Length > 0 && members.Length + 1 != member.target.SerializedItems.Length) {
							members = null;
						}
					}
					uNodeFunction objRef = null;
					switch(member.target.targetType) {
						case MemberData.TargetType.uNodeFunction: {
							uNodeRoot root = member.target.GetInstance() as uNodeRoot;
							if(root != null) {
								var gTypes = MemberData.Utilities.SafeGetGenericTypes(member.target)[0];
								objRef = root.GetFunction(member.target.startName, gTypes != null ? gTypes.Length : 0, MemberData.Utilities.SafeGetParameterTypes(member.target)[0]);
							}
							break;
						}
					}
					int totalParam = 0;
					bool flag = false;
					for(int i = 0; i < member.target.SerializedItems.Length; i++) {
						if(i != 0) {
							if(members != null && (member.target.isDeepTarget || !member.target.IsTargetingUNode)) {
								MemberInfo mData = members[i - 1];
								if(mData is MethodInfo || mData is ConstructorInfo) {
									var method = mData as MethodInfo;
									var parameters = method != null ? method.GetParameters() : (mData as ConstructorInfo).GetParameters();
									if(parameters.Length > 0) {
										totalParam++;
										if(totalParam > 1) {
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
					for(int i = 0; i < member.target.SerializedItems.Length; i++) {
						if(i != 0) {
							if(members != null && (member.target.isDeepTarget || !member.target.IsTargetingUNode)) {
								MemberInfo memberInfo = members[i - 1];
								if(memberInfo is MethodInfo || memberInfo is ConstructorInfo) {
									var method = memberInfo as MethodInfo;
									var documentation = XmlDoc.XMLFromMember(memberInfo);
									if(flag) {
										AddControl(memberInfo.Name, null);
									}
									var parameters = method != null ? method.GetParameters() : (memberInfo as ConstructorInfo).GetParameters();
									if(parameters.Length > 0) {
										while(parameters.Length + totalParam > member.parameters.Length) {
											ArrayUtility.Add(ref member.parameters, MemberData.empty);
										}
										for(int x = 0; x < parameters.Length; x++) {
											var parameter = parameters[x];
											if(parameter.ParameterType != null) {
												int index = totalParam;
												var param = member.parameters[index];
												var port = AddPort(new PortData() {
													getPortName = () => ObjectNames.NicifyVariableName(parameter.Name),
													getPortType = () => parameter.ParameterType,
													getPortValue = () => param,
													filter = new FilterAttribute(parameter.ParameterType),
													onValueChanged = (obj) => {
														RegisterUndo();
														param = obj as MemberData;
														member.parameters[index] = param;
														field.SetValueOptimized(data.block, member);
														Repaint();
													},
												});
												port.Add(new ControlView(port.portData.InstantiateControl(true), true));
												port.SetPortDepth(1);
											}
											totalParam++;
										}
										continue;
									}
								}
							}
						}
						System.Type[] paramsType = MemberData.Utilities.SafeGetParameterTypes(member.target)[i];
						if(paramsType != null && paramsType.Length > 0) {
							if(flag) {
								AddControl("Method " + (methodDrawCount), null);
								methodDrawCount++;
							}
							while(paramsType.Length + totalParam > member.parameters.Length) {
								ArrayUtility.Add(ref member.parameters, MemberData.none);
							}
							for(int x = 0; x < paramsType.Length; x++) {
								System.Type PType = paramsType[x];
								if(member.parameters[totalParam] == null) {
									member.parameters[totalParam] = MemberData.none;
								}
								if(PType != null) {
									int index = totalParam;
									var param = member.parameters[index];
									string pLabel;
									if(objRef != null) {
										pLabel = objRef.parameters[x].name;
									} else {
										pLabel = "P" + (x + 1);
									}
									var port = AddPort(new PortData() {
										getPortName = () => ObjectNames.NicifyVariableName(pLabel),
										getPortType = () => PType,
										getPortValue = () => param,
										filter = new FilterAttribute(PType),
										onValueChanged = (obj) => {
											RegisterUndo();
											param = obj as MemberData;
											member.parameters[index] = param;
											field.SetValueOptimized(data.block, member);
											Repaint();
										},
									});
									port.Add(new ControlView(port.portData.InstantiateControl(true), true));
									port.SetPortDepth(1);
								}
								totalParam++;
							}
						}
					}
					while(member.parameters.Length > totalParam) {
						ArrayUtility.RemoveAt(ref member.parameters, member.parameters.Length - 1);
					}
				}
			}
		}

		public void InitializeEdge() {
			base.expanded = true;
			for(int i = 0; i < portViews.Count; i++) {
				var view = portViews[i];
				var member = view.GetValue();
				if(member != null) {
					if(member.isAssigned && member.IsTargetingPortOrNode) {
						EdgeView edge = new EdgeView(view, PortUtility.GetOutputPort(member, owner.nodeView.owner));
						owner.nodeView.owner.Connect(edge, false);
					}
				} 
				//else if(val is MultipurposeMember) {

				//}
			}
			expanded = data.expanded;
			RefreshPorts();
		}

		public BlockControl AddControl(BlockControl block) {
			inputContainer.Add(block);
			controls.Add(block);
			return block;
		}

		public BlockControl AddControl(string label, ControlView control) {
			BlockControl block = new BlockControl(label, control);
			inputContainer.Add(block);
			controls.Add(block);
			return block;
		}

		public PortView AddPort(PortData portData) {
			if(string.IsNullOrEmpty(portData.portID)) {//Make sure port has unique id.
				portData.portID = UnityEngine.Random.Range(0, short.MaxValue).ToString();
			}
			portData.isFlow = false;
			portData.owner = owner.nodeView;
			PortView p = new PortView(Orientation.Horizontal, Direction.Input, portData);
			inputContainer.Add(p);
			portViews.Add(p);
			p.EnableInClassList("control", true);
			p.Initialize(portData);
			return p;
		}

		public override void SetPosition(Rect newPos) {
			style.position = Position.Relative;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

		}
	}
}