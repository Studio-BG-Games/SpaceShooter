using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using NodeView = UnityEditor.Experimental.GraphView.Node;

namespace MaxyGames.uNode.Editors {
	public abstract class UNodeView : NodeView {
		public List<PortView> inputPorts = new List<PortView>();
		public List<PortView> outputPorts = new List<PortView>();

		public List<ControlView> inputControls = new List<ControlView>();
		public List<ControlView> outputControls = new List<ControlView>();

		public NodeComponent targetNode { protected set; get; }
		public UGraphView owner { protected set; get; }
		public GraphLayout graphLayout => owner.graphLayout;
		public virtual bool autoReload => false;

		public VisualElement portInputContainer { protected set; get; }
		public VisualElement flowInputContainer { protected set; get; }
		public VisualElement flowOutputContainer { protected set; get; }
		public VisualElement controlsContainer { protected set; get; }
		//For Hiding
		public VisualElement parentElement { get; set; }
		public Rect hidingRect { get; set; }
		public bool isHidden => parent == null;

		public override bool expanded {
			get {
				return base.expanded;
			}
			set {
				targetNode.nodeExpanded = value;
				RefreshControl(value);
				base.expanded = value;
			}
		}

		public UIElementGraph graph {
			get {
				return owner.graph;
			}
		}

		#region VisualElement
		private IconBadge errorBadge;
		protected VisualElement debugContainer;
		protected VisualElement border;
		protected Image titleIcon;
		#endregion

		#region Initialization
		protected void Initialize(UGraphView owner) {
			this.owner = owner;
			this.AddToClassList("node-view");
			if(!ShowExpandButton()) {//Hides colapse button
				m_CollapseButton.style.position = Position.Absolute;
				m_CollapseButton.style.width = 0;
				m_CollapseButton.style.height = 0;
				m_CollapseButton.visible = false;
			}
			base.expanded = true;

			border = this.Q("node-border");
			{//Flow inputs
				flowInputContainer = new VisualElement();
				flowInputContainer.name = "flow-inputs";
				flowInputContainer.AddToClassList("flow-container");
				flowInputContainer.AddToClassList("input");
				flowInputContainer.pickingMode = PickingMode.Ignore;
				border.Insert(0, flowInputContainer);
			}
			{//Flow outputs
				flowOutputContainer = new VisualElement();
				flowOutputContainer.name = "flow-outputs";
				flowOutputContainer.AddToClassList("flow-container");
				flowOutputContainer.AddToClassList("output");
				flowOutputContainer.pickingMode = PickingMode.Ignore;
				Add(flowOutputContainer);
			}
			controlsContainer = new VisualElement { name = "controls" };
			mainContainer.Add(controlsContainer);

			titleIcon = new Image() { name = "title-icon" };
			titleContainer.Add(titleIcon);
			titleIcon.SendToBack();

			OnSetup();

			RegisterCallback<MouseDownEvent>(evt => {
				var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
				if(evt.button == 0 && evt.shiftKey && !evt.altKey) {
					ActionPopupWindow.ShowWindow(Vector2.zero, () => {
						CustomInspector.ShowInspector(new GraphEditorData(graph.editorData) { selected = targetNode });
					}, 300, 300).ChangePosition(mPos);
				}
			});
			RegisterCallback<MouseOverEvent>((e) => {
				for(int i = 0; i < inputPorts.Count; i++) {
					var edges = inputPorts[i].GetEdges();
					foreach(var edge in edges) {
						if(edge == null)
							continue;
						if(edge.isProxy) {
							edge.edgeControl.visible = true;
						}
					}
				}
				for(int i = 0; i < outputPorts.Count; i++) {
					var edges = outputPorts[i].GetEdges();
					foreach(var edge in edges) {
						if(edge == null)
							continue;
						if(edge.isProxy) {
							edge.edgeControl.visible = true;
						}
					}
				}
			});
			RegisterCallback<MouseLeaveEvent>((e) => {
				for(int i = 0; i < inputPorts.Count; i++) {
					var edges = inputPorts[i].GetEdges();
					foreach(var edge in edges) {
						if(edge == null)
							continue;
						if(edge.isProxy) {
							edge.edgeControl.visible = false;
						}
					}
				}
				for(int i = 0; i < outputPorts.Count; i++) {
					var edges = outputPorts[i].GetEdges();
					foreach(var edge in edges) {
						if(edge == null)
							continue;
						if(edge.isProxy) {
							edge.edgeControl.visible = false;
						}
					}
				}
			});
			long trickedFrame = 0;
			RegisterCallback<KeyDownEvent>(e => {
				trickedFrame = uNodeThreadUtility.frame;
			}, TrickleDown.TrickleDown);
			RegisterCallback<GeometryChangedEvent>(evt => {
				if(owner.isLoading || this.isHidden || !this.IsVisible() || trickedFrame == 0 || trickedFrame + 10 < uNodeThreadUtility.frame)
					return;//For fix node auto move.
				if(evt.oldRect != Rect.zero && evt.oldRect.width != evt.newRect.width) {
					Teleport(new Rect(evt.newRect.x + (evt.oldRect.width - evt.newRect.width), evt.newRect.y, evt.newRect.width, evt.newRect.height));
				}
			});
		}

		/// <summary>
		/// Initialize once on created.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="node"></param>
		public virtual void Initialize(UGraphView owner, NodeComponent node) {
			targetNode = node;
			Initialize(owner);
			ReloadView();
		}

		/// <summary>
		/// Called once on node created.
		/// </summary>
		protected virtual void OnSetup() {

		}

		public virtual void InitializeEdge() {

		}

		protected void RefreshControl(bool isVisible) {
			portInputContainer.SetOpacity(isVisible);
			foreach(var c in inputControls) {
				// c.visible = isVisible;
				c.SetOpacity(isVisible);
				if(isVisible) {
					c.RemoveFromClassList("hidden");
				} else {
					c.AddToClassList("hidden");
				}
			}
			foreach(var c in outputControls) {
				// c.visible = isVisible;
				c.SetOpacity(isVisible);
				if(isVisible) {
					c.RemoveFromClassList("hidden");
				} else {
					c.AddToClassList("hidden");
				}
			}
		}
		#endregion

		#region Add Ports
		public PortView AddInputValuePort(string fieldName, Func<Type> type, string portName = null) {
			FieldInfo field = targetNode.GetType().GetField(fieldName);
			return AddPort(false, Direction.Input, new PortData() {
				portID = fieldName,
				getPortName = () => portName ?? ObjectNames.NicifyVariableName(field.Name),
				getPortValue = () => field.GetValueOptimized(targetNode) as MemberData,
				getPortType = type,
				onValueChanged = (o) => {
					RegisterUndo();
					field.SetValueOptimized(targetNode, o);
				},
			});
		}

		public PortView AddInputValuePort(string fieldName, Func<Type> type, FilterAttribute filter, string portName = null) {
			FieldInfo field = targetNode.GetType().GetField(fieldName);
			return AddPort(false, Direction.Input, new PortData() {
				portID = fieldName,
				getPortName = () => portName ?? ObjectNames.NicifyVariableName(field.Name),
				getPortValue = () => field.GetValueOptimized(targetNode) as MemberData,
				getPortType = type,
				filter = filter,
				onValueChanged = (o) => {
					RegisterUndo();
					field.SetValueOptimized(targetNode, o);
				},
			});
		}

		public PortView AddInputValuePort(string fieldName, FilterAttribute filter, string portName = null) {
			FieldInfo field = targetNode.GetType().GetField(fieldName);
			return AddPort(false, Direction.Input, new PortData() {
				portID = fieldName,
				getPortName = () => portName ?? ObjectNames.NicifyVariableName(field.Name),
				getPortValue = () => field.GetValueOptimized(targetNode) as MemberData,
				onValueChanged = (o) => {
					RegisterUndo();
					field.SetValueOptimized(targetNode, o);
				},
				filter = filter,
			});
		}

		public PortView AddInputValuePort(PortData portData) {
			return AddPort(false, Direction.Input, portData);
		}

		public PortView AddInputValuePort(string fieldName, string portName = null) {
			FieldInfo field = targetNode.GetType().GetField(fieldName);
			var attributes = field.GetCustomAttributes(true);
			ReflectionUtils.TryCorrectingAttribute(targetNode, ref attributes);
			FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(attributes);
			return AddInputValuePort(
				new PortData() {
					portID = field.Name,
					filter = filter,
					onValueChanged = (o) => {
						RegisterUndo();
						field.SetValueOptimized(targetNode, o);
					},
					getPortName = () => portName ?? ObjectNames.NicifyVariableName(field.Name),
					getPortValue = () => field.GetValueOptimized(targetNode) as MemberData,
					getPortTooltip = () => {
						var tooltip = ReflectionUtils.GetAttribute<TooltipAttribute>(attributes);
						return tooltip != null ? tooltip.tooltip : string.Empty;
					},
				});
		}

		public PortView AddOutputValuePort(string fieldName, Func<Type> type, string portName = null) {
			FieldInfo field = targetNode.GetType().GetField(fieldName);
			return AddPort(false, Direction.Output, new PortData() {
				portID = fieldName,
				getPortName = () => portName ?? ObjectNames.NicifyVariableName(field.Name),
				getPortValue = () => field.GetValueOptimized(targetNode) as MemberData,
				getPortType = type,
				onValueChanged = (val) => {
					field.SetValueOptimized(targetNode, val);
				},
			});
		}

		public PortView AddOutputValuePort(IExtendedOutput node, int index, Func<string> portName = null, Func<string> portTooltop = null) {
			return AddOutputValuePort(
				new PortData() {
					portID = "out." + node.GetOutputName(index),
					getPortName = portName ?? (() => node.GetOutputName(index)),
					getPortType = () => node.GetOutputType(node.GetOutputName(index)),
					getConnection = () => {
						return MemberData.ValueOutputExtended(node, node.GetOutputName(index));
					},
					getPortTooltip = portTooltop,
				}
			);
		}

		public PortView AddOutputValuePort(PortData portData) {
			return AddPort(false, Direction.Output, portData);
		}

		public PortView AddInputFlowPort(PortData portData) {
			portData.getPortType = () => typeof(MemberData);
			return AddPort(true, Direction.Input, portData);
		}

		public PortView AddInputFlowPort(IExtendedInput node, int index, Func<string> portName = null, Func<string> portTooltop = null) {
			return AddInputFlowPort(
				new PortData() {
					portID = "in." + node.GetInputName(index),
					getPortName = portName ?? (() => node.GetInputName(index)),
					getConnection = () => {
						return MemberData.FlowInputExtended(node, node.GetInputName(index));
					},
					getPortTooltip = portTooltop,
				}
			);
		}

		public PortView AddOutputFlowPort(PortData portData) {
			portData.getPortType = () => typeof(MemberData);
			return AddPort(true, Direction.Output, portData);
		}

		public PortView AddOutputFlowPort(Func<string> portName, Func<MemberData> portValue, Action<MemberData> onValueChanged) {
			return AddPort(true, Direction.Output, new PortData() {
				getPortName = portName,
				getPortType = () => typeof(MemberData),
				getPortValue = portValue,
				onValueChanged = onValueChanged,
			});
		}

		public PortView AddOutputFlowPort(string fieldName, string portName = null) {
			FieldInfo field = targetNode.GetType().GetField(fieldName);
			return AddPort(true, Direction.Output,
				new PortData() {
					portID = fieldName,
					getPortName = () => portName ?? ObjectNames.NicifyVariableName(field.Name),
					getPortValue = () => field.GetValueOptimized(targetNode) as MemberData,
					getPortType = () => typeof(MemberData),
					onValueChanged = (val) => {
						field.SetValueOptimized(targetNode, val);
					},
				});
		}

		public PortView AddPort(bool isFlow, Direction direction, PortData portData) {
			if(string.IsNullOrEmpty(portData.portID)) {//Make sure port has unique id.
				portData.portID = "$" + UnityEngine.Random.Range(0, short.MaxValue).ToString();
			}
			portData.owner = this;
			portData.isFlow = isFlow;
			Orientation orientation = isFlow && owner.graphLayout == GraphLayout.Vertical ? Orientation.Vertical : Orientation.Horizontal;
			PortView p = new PortView(orientation, direction, portData);
			if(p.direction == Direction.Input) {
				inputPorts.Add(p);
				if(isFlow) {
					if(owner.graphLayout == GraphLayout.Vertical) {
						flowInputContainer.Add(p);
					} else {
						inputContainer.Add(p);
					}
				} else {
					if(UIElementUtility.Theme.preferredDisplayValue == DisplayValueKind.Inside) {
						p.EnableInClassList("port-control", true);
						p.Add(new ControlView(portData.InstantiateControl(true), true));
					} else {
						var portInputView = new PortInputView(portData);
						portInputContainer.Add(portInputView);
					}
					if(portData.GetPortType().IsByRef) {
						p.EnableInClassList("port-byref", true);
					}
					inputContainer.Add(p);
				}
			} else {
				outputPorts.Add(p);
				if(isFlow) {
					if(owner.graphLayout == GraphLayout.Vertical) {
						flowOutputContainer.Add(p);
					} else {
						outputContainer.Add(p);
					}
				} else {
					outputContainer.Add(p);
				}
			}
			p.Initialize(portData);
			return p;
		}
		#endregion

		#region Add Controls
		public void AddControl(Direction direction, ControlView control) {
			switch(direction) {
				case Direction.Input: {
					inputControls.Add(control);
					inputContainer.Add(control);
					break;
				}
				case Direction.Output: {
					outputControls.Add(control);
					outputContainer.Add(control);
					break;
				}
			}
		}

		public void AddControl(Direction direction, ValueControl visualElement) {
			var control = new ControlView(visualElement);
			AddControl(direction, control);
		}

		public void AddControl(Direction direction, VisualElement visualElement) {
			var control = new ControlView();
			control.Add(visualElement);
			switch(direction) {
				case Direction.Input: {
					inputControls.Add(control);
					inputContainer.Add(control);
					break;
				}
				case Direction.Output: {
					outputControls.Add(control);
					outputContainer.Add(control);
					break;
				}
			}
		}
		#endregion

		#region Remove Control & Ports
		protected void RemoveControls() {
			for(int i = 0; i < inputControls.Count; i++) {
				if(inputControls[i].parent != null) {
					inputControls[i].RemoveFromHierarchy();
				}
			}
			for(int i = 0; i < outputControls.Count; i++) {
				if(outputControls[i].parent != null) {
					outputControls[i].RemoveFromHierarchy();
				}
			}
			inputControls.Clear();
			outputControls.Clear();
		}

		protected void RemovePorts() {
			for(int i = 0; i < inputPorts.Count; i++) {
				RemovePort(inputPorts[i]);
				i--;
			}
			for(int i = 0; i < outputPorts.Count; i++) {
				RemovePort(outputPorts[i]);
				i--;
			}
		}

		public void RemovePort(PortView p) {
			if(p.direction == Direction.Input) {
				inputPorts.Remove(p);
				//if(p.orientation == Orientation.Vertical) {
				//	flowInputContainer.Remove(p);
				//} else {
				//	inputContainer.Remove(p);
				//}
			} else {
				outputPorts.Remove(p);
				//if(p.orientation == Orientation.Vertical) {
				//	flowOutputContainer.Remove(p);
				//} else {
				//	outputContainer.Remove(p);
				//}
			}
			if(p.parent != null) {
				p.parent.Remove(p);
			}
		}
		#endregion

		#region Repaint
		/// <summary>
		/// Repaint the node view in next frame of editor loop.
		/// </summary>
		public void MarkRepaint() {
			MarkDirtyRepaint();
			owner.MarkRepaint(this);
		}
		
		public void RefreshPortTypes() {
			for (int i = 0; i < inputPorts.Count;i++) {
				inputPorts[i]?.ReloadView();
			}
			for (int i = 0; i < outputPorts.Count;i++) {
				outputPorts[i]?.ReloadView();
			}
			MarkDirtyRepaint();
		}
		#endregion

		#region Functions
		public virtual void ReloadView() {
			base.expanded = true;
			RemovePorts();
			RemoveControls();
			if(portInputContainer != null) {//Remove port container to make sure data is up to date.
				Remove(portInputContainer);
			}

			portInputContainer = new VisualElement {
				name = "portInputContainer",
				pickingMode = PickingMode.Ignore,
			};
			Add(portInputContainer);
			portInputContainer.SendToBack();
		}

		public virtual void RegisterUndo(string name = "") {
			uNodeEditorUtility.RegisterUndo(targetNode, name);
		}

		/// <summary>
		/// Called on port has been connected
		/// </summary>
		/// <param name="port"></param>
		public virtual void OnPortConnected(PortView port) {
			if(HasControl()) {
				m_CollapseButton.SetEnabled(false);
				m_CollapseButton.SetEnabled(true);
			}
		}

		/// <summary>
		/// Called on port has been disconnected
		/// </summary>
		/// <param name="port"></param>
		public virtual void OnPortDisconnected(PortView port) {

		}

		/// <summary>
		/// Called on any value changed.
		/// </summary>
		public virtual void OnValueChanged() {

		}

		public virtual string GetTitle() {
			return title;
		}

		public virtual bool ShowExpandButton() {
			return false;
		}

		public bool HasControl() {
			return inputControls.Count > 0 || outputControls.Count > 0;
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

		}

		public PortView GetInputPortByID(string id) {
			for (int i = 0; i < inputPorts.Count;i++) {
				if(inputPorts[i]?.GetPortID() == id) {
					return inputPorts[i];
				}
			}
			return null;
		}

		public void UpdateError(string message) {
			if(!string.IsNullOrEmpty(message)) {//Has error
				if(errorBadge == null) {
					errorBadge = IconBadge.CreateError(message);
					Add(errorBadge);
					errorBadge.AttachTo(titleContainer, SpriteAlignment.LeftCenter);
				} else {
					errorBadge.badgeText = message;
				}
			} else {
				if(errorBadge != null && errorBadge.parent != null) {
					errorBadge.Detach();
					errorBadge.RemoveFromHierarchy();
					errorBadge = null;
				}
			}
		}

		public virtual void Teleport(Rect position) {
			DoTeleport(position);
			targetNode.editorRect = position;
		}

		public override Rect GetPosition() {
			if(isHidden) {
				return hidingRect;
			}
			return base.GetPosition();
		}

		public override void SetPosition(Rect newPos) {
			DoTeleport(newPos);
		}

		private void DoTeleport(Rect position) {
			//style.position = Position.Absolute;
			//style.left = new StyleLength(StyleKeyword.Auto);
			//style.right = position.x;
			//style.top = position.y;
			if(isHidden) {
				hidingRect = new Rect(position.position, hidingRect.size);
			}
			base.SetPosition(position);
		}

		//public override Rect GetPosition() {
		//	if(resolvedStyle.position == Position.Absolute) {
		//		return new Rect(resolvedStyle.right, resolvedStyle.top, layout.width, layout.height);
		//	}
		//	return base.GetPosition();
		//}
		#endregion
	}
}