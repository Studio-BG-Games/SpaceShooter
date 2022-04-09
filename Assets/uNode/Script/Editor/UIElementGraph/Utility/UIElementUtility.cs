using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Reflection;

namespace MaxyGames.uNode.Editors {
	public static class UIElementUtility {
		#region Dark Skin
		private static readonly StyleSheet s_ToolbarDarkStyleSheet;
		private static readonly StyleSheet s_ToolbarLightStyleSheet;
		private static readonly StyleSheet s_DefaultCommonDarkStyleSheet;
		private static readonly StyleSheet s_DefaultCommonLightStyleSheet;

		static UIElementUtility() {
#if UNITY_2019_3_OR_NEWER
			MethodInfo getStyleForCurrentFont = null;
			Type unityUIUtils = "UnityEditor.UIElements.UIElementsEditorUtility".ToType(false);
			if(unityUIUtils != null) {
				getStyleForCurrentFont = unityUIUtils.GetMethod("GetStyleSheetPathForCurrentFont", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			}
			if(getStyleForCurrentFont != null) {
				s_DefaultCommonDarkStyleSheet = EditorGUIUtility.Load(getStyleForCurrentFont.Invoke(null, new object[] { "StyleSheets/Generated/DefaultCommonDark.uss.asset" }) as string) as StyleSheet;
				s_DefaultCommonLightStyleSheet = EditorGUIUtility.Load(getStyleForCurrentFont.Invoke(null, new object[] { "StyleSheets/Generated/DefaultCommonLight.uss.asset" }) as string) as StyleSheet;
				s_ToolbarDarkStyleSheet = EditorGUIUtility.Load(getStyleForCurrentFont.Invoke(null, new object[] { "StyleSheets/Generated/ToolbarDark.uss.asset" }) as string) as StyleSheet;
				s_ToolbarLightStyleSheet = EditorGUIUtility.Load(getStyleForCurrentFont.Invoke(null, new object[] { "StyleSheets/Generated/ToolbarLight.uss.asset" }) as string) as StyleSheet;
			}
#endif
			s_DefaultCommonDarkStyleSheet = s_DefaultCommonDarkStyleSheet ?? EditorGUIUtility.Load("StyleSheets/Generated/DefaultCommonDark.uss.asset") as StyleSheet;
			if(s_DefaultCommonDarkStyleSheet == null) {
				s_DefaultCommonDarkStyleSheet = EditorGUIUtility.Load("StyleSheets/DefaultCommonDark.uss") as StyleSheet;
			}
			s_DefaultCommonLightStyleSheet = s_DefaultCommonLightStyleSheet ?? EditorGUIUtility.Load("StyleSheets/Generated/DefaultCommonLight.uss.asset") as StyleSheet;
			if(s_DefaultCommonLightStyleSheet == null) {
				s_DefaultCommonLightStyleSheet = EditorGUIUtility.Load("StyleSheets/DefaultCommonLight.uss") as StyleSheet;
			}

			s_ToolbarDarkStyleSheet = s_ToolbarDarkStyleSheet ?? EditorGUIUtility.Load("StyleSheets/Generated/ToolbarDark.uss.asset") as StyleSheet;
			if(s_ToolbarDarkStyleSheet == null) {
				s_ToolbarDarkStyleSheet = (EditorGUIUtility.Load("StyleSheets/ToolbarDark.uss") as StyleSheet);
			}
			s_ToolbarLightStyleSheet = s_ToolbarLightStyleSheet ?? EditorGUIUtility.Load("StyleSheets/Generated/ToolbarLight.uss.asset") as StyleSheet;
			if(s_ToolbarLightStyleSheet == null) {
				s_ToolbarLightStyleSheet = (EditorGUIUtility.Load("StyleSheets/ToolbarLight.uss") as StyleSheet);
			}
		}

		public static void ForceDarkStyleSheet(VisualElement ele) {
			if(!EditorGUIUtility.isProSkin) {
				var e = ele;
				while(e != null) {
					if(e.styleSheets.Contains(s_DefaultCommonLightStyleSheet)) {
						e.styleSheets.Remove(s_DefaultCommonLightStyleSheet);
						e.styleSheets.Add(s_DefaultCommonDarkStyleSheet);
						break;
					}
					e = e.parent;
				}
			}
		}

		public static void ForceDarkToolbarStyleSheet(VisualElement ele) {
			if(!EditorGUIUtility.isProSkin) {
				if(ele.styleSheets.Contains(s_ToolbarLightStyleSheet)) {
					ele.styleSheets.Remove(s_ToolbarLightStyleSheet);
					ele.styleSheets.Add(s_ToolbarDarkStyleSheet);
				}
				foreach(var e in ele.Children()) {
					ForceDarkToolbarStyleSheet(e);
				}
			}
		}
		#endregion

		#region Graph Utils
		public static EdgeView GetOverlapsEdge(UGraphView graph, UNodeView node) {
			EdgeView edgeView = null;
			graph.edges.ForEach(e => {
				var edge = e as EdgeView;
				if(edge == null || !edge.isValid || edge.isProxy || !edge.isFlow)
					return;
				if(edge.Input.owner == node || edge.Output.owner == node)
					return;
				if(edge.Overlaps(node.ChangeCoordinatesTo(edge, new Rect(Vector2.zero, node.layout.size)))) {
					edgeView = edge;
				}
			});
			return edgeView;
		}

		public static void ShowReferenceMenu(ContextualMenuPopulateEvent evt, MemberData memberData, string startMenuName = "References/") {
			switch(memberData.targetType) {
				case MemberData.TargetType.Type:
				case MemberData.TargetType.uNodeType:
				case MemberData.TargetType.Constructor:
				case MemberData.TargetType.Event:
				case MemberData.TargetType.Field:
				case MemberData.TargetType.Method:
				case MemberData.TargetType.Property:
					ShowReferenceMenu(evt, memberData.startType);
					break;
				case MemberData.TargetType.uNodeVariable:
					evt.menu.AppendAction($"{startMenuName}Find All References: {memberData.startName} (Variable)", (e) => {
						GraphUtility.ShowVariableUsages(memberData.startTarget as uNodeRoot, memberData.startName);
					}, DropdownMenuAction.AlwaysEnabled);
					break;
				case MemberData.TargetType.uNodeProperty:
					evt.menu.AppendAction($"{startMenuName}Find All References: {memberData.startName} (Property)", (e) => {
						GraphUtility.ShowPropertyUsages(memberData.startTarget as uNodeRoot, memberData.startName);
					}, DropdownMenuAction.AlwaysEnabled);
					break;
				case MemberData.TargetType.uNodeFunction:
					evt.menu.AppendAction($"{startMenuName}Find All References: {memberData.startName} (Function)", (e) => {
						GraphUtility.ShowFunctionUsages(memberData.startTarget as uNodeRoot, memberData.startName);
					}, DropdownMenuAction.AlwaysEnabled);
					break;
				case MemberData.TargetType.uNodeLocalVariable:
					evt.menu.AppendAction($"{startMenuName}Find All References: {memberData.startName} (Local Variable)", (e) => {
						GraphUtility.ShowLocalVariableUsages(memberData.startTarget as RootObject, memberData.startName);
					}, DropdownMenuAction.AlwaysEnabled);
					break;
				case MemberData.TargetType.uNodeParameter:
					evt.menu.AppendAction($"{startMenuName}Find All References: {memberData.startName} (Parameter)", (e) => {
						GraphUtility.ShowParameterUsages(memberData.startTarget as RootObject, memberData.startName);
					}, DropdownMenuAction.AlwaysEnabled);
					break;
				case MemberData.TargetType.uNodeGenericParameter:
					evt.menu.AppendAction($"{startMenuName}Find All References: {memberData.startName} (Generic Parameter)", (e) => {
						GraphUtility.ShowParameterUsages(memberData.startTarget as RootObject, memberData.startName);
					}, DropdownMenuAction.AlwaysEnabled);
					break;
			}
			var members = memberData.GetMembers(false);
			if(members != null && members.Length > 0) {
				for(int i = 0; i < members.Length; i++) {
					var member = members[i];
					if(member == null)
						continue;
					evt.menu.AppendSeparator(startMenuName);
					ShowReferenceMenu(evt, member, startMenuName);
				}
			}
		}

		public static void ShowReferenceMenu(ContextualMenuPopulateEvent evt, MemberInfo info, string startMenuName = "References/") {
			if(info == null)
				return;
			evt.menu.AppendAction($"{startMenuName}Find: {info.Name} ({info.MemberType}) in browser", (e) => {
				var win = NodeBrowserWindow.ShowWindow();
				win.browser.RevealItem(info);
				win.Focus();
			}, DropdownMenuAction.AlwaysEnabled);
			evt.menu.AppendAction($"{startMenuName}Find All References: {info.Name} ({info.MemberType})", (e) => {
				GraphUtility.ShowMemberUsages(info);
			}, DropdownMenuAction.AlwaysEnabled);
			if(info is IRuntimeMember) {
				evt.menu.AppendAction($"{startMenuName}Go To Definition: {info.Name} ({info.MemberType})", (e) => {
					RuntimeType runtimeType = info as RuntimeType;
					if(info is RuntimeField) {
						runtimeType = (info as RuntimeField).owner;
					} else if(info is RuntimeProperty) {
						runtimeType = (info as RuntimeProperty).owner;
					} else if(info is RuntimeMethod) {
						runtimeType = (info as RuntimeMethod).owner;
					}
					if(runtimeType is RuntimeGraphType) {
						var graphType = runtimeType as RuntimeGraphType;
						if(graphType.target != null) {
							uNodeEditor.Open(graphType.target);
						}
					} else if(runtimeType is RuntimeGraphInterface) {
						var graphType = runtimeType as RuntimeGraphInterface;
						if(graphType.target != null) {
							EditorGUIUtility.PingObject(graphType.target);
							Selection.instanceIDs = new int[] { graphType.target.GetInstanceID() };
						}
					} else {
						uNodeEditorUtility.DisplayErrorMessage("Un-implemented current runtime type.");
					}
				}, DropdownMenuAction.AlwaysEnabled);
			}
		}
		#endregion

		#region Node Utils
		public static PortView GetDefaultFlowPort(UNodeView node) {
			foreach(var port in node.inputPorts) {
				if(port.GetPortID() == UGraphView.SelfPortID) {
					return port;
				}
			}
			return null;
		}

		public static PortView GetFinishFlowPort(UNodeView node) {
			foreach(var port in node.outputPorts) {
				if(port.isFlow && port.GetPortID() == UGraphView.NextFlowID) {
					return port;
				}
			}
			return null;
		}

		public static PortView GetLastFinishFlowPort(UNodeView node) {
			var port = GetFinishFlowPort(node);
			if(port == null)
				return null;
			if(port.connected) {
				foreach(var edge in port.GetValidEdges()) {
					if(edge.Input.owner is var n && n != null) {
						return GetLastFinishFlowPort(n);
					}
				}
				return null;
			}
			return port;
		}
		#endregion

		public static void ShowMenu(this VisualElement visualElement, DropdownMenu menu) {
			if (visualElement != null && menu != null && menu.MenuItems().Any()) {
				Vector2 p = new Vector2(visualElement.layout.xMin, visualElement.layout.yMax);
				p = visualElement.parent.LocalToWorld(p);
				menu.DoDisplayEditorMenu(p);
			}
		}

		private static GenericMenu PrepareMenu(DropdownMenu menu, EventBase triggerEvent) {
			menu.PrepareForDisplay(triggerEvent);
			GenericMenu genericMenu = new GenericMenu();
			foreach (DropdownMenuItem item in menu.MenuItems()) {
				DropdownMenuAction action = item as DropdownMenuAction;
				if (action != null) {
					if ((action.status & DropdownMenuAction.Status.Hidden) != DropdownMenuAction.Status.Hidden && action.status != 0) {
						bool on = (action.status & DropdownMenuAction.Status.Checked) == DropdownMenuAction.Status.Checked;
						if ((action.status & DropdownMenuAction.Status.Disabled) == DropdownMenuAction.Status.Disabled) {
							genericMenu.AddDisabledItem(new GUIContent(action.name), on);
						} else {
							genericMenu.AddItem(new GUIContent(action.name), on, delegate {
								action.Execute();
							});
						}
					}
				} else {
					DropdownMenuSeparator dropdownMenuSeparator = item as DropdownMenuSeparator;
					if (dropdownMenuSeparator != null) {
						genericMenu.AddSeparator(dropdownMenuSeparator.subMenuPath);
					}
				}
			}
			return genericMenu;
		}

		public static void DoDisplayEditorMenu(this DropdownMenu menu, Vector2 position) {
			PrepareMenu(menu, null).DropDown(new Rect(position, Vector2.zero));
		}

		public static void DoDisplayEditorMenu(this DropdownMenu menu, EventBase triggerEvent) {
			GenericMenu genericMenu = PrepareMenu(menu, triggerEvent);
			Vector2 position = Vector2.zero;
			if (triggerEvent is IMouseEvent) {
				position = ((IMouseEvent)triggerEvent).mousePosition;
			} else if (triggerEvent.target is VisualElement) {
				position = ((VisualElement)triggerEvent.target).layout.center;
			}
			genericMenu.DropDown(new Rect(position, Vector2.zero));
		}

		#region Private
		static Dictionary<Type, Type> nodeViewPerType;
		#endregion

		#region Extensions
		/// <summary>
		/// Set the border color
		/// </summary>
		/// <param name="style"></param>
		/// <param name="color"></param>
		public static void SetBorderColor(this IStyle style, Color color) {
			#if UNITY_2019_3_OR_NEWER
			style.borderBottomColor = color;
			style.borderLeftColor = color;
			style.borderRightColor = color;
			style.borderTopColor = color;
			#else
			style.borderColor = color;
			#endif
		}

		/// <summary>
		/// Add StyleSheet to Visual Element
		/// </summary>
		/// <param name="visualElement"></param>
		/// <param name="resourcePath"></param>
		public static void AddStyleSheet(this VisualElement visualElement, string resourcePath) {
			AddStyleSheet(visualElement, Resources.Load<StyleSheet>(resourcePath));
		}

		/// <summary>
		/// Add StyleSheet to Visual Element
		/// </summary>
		/// <param name="visualElement"></param>
		/// <param name="styleSheet"></param>
		public static void AddStyleSheet(this VisualElement visualElement, StyleSheet styleSheet) {
			if(styleSheet == null) return;
			visualElement.styleSheets.Add(styleSheet);
		}
		
		/// <summary>
		/// Add StyleSheet to Visual Element
		/// </summary>
		/// <param name="visualElement"></param>
		/// <param name="styleSheets"></param>
		public static void AddStyleSheet(this VisualElement visualElement, IEnumerable<StyleSheet> styleSheets) {
			if(styleSheets == null) return;
			foreach(var style in styleSheets) {
				if(style == null) continue;
				visualElement.styleSheets.Add(style);
			}
		}

		/// <summary>
		/// Get layout size
		/// </summary>
		/// <param name="visualElement"></param>
		/// <returns></returns>
		public static Rect GetRect(this VisualElement visualElement) {
			return new Rect(0.0f, 0.0f, visualElement.layout.width, visualElement.layout.height);
		}

		/// <summary>
		/// Is the element is faded?
		/// </summary>
		/// <param name="visualElement"></param>
		/// <returns></returns>
		public static bool IsFaded(this VisualElement visualElement) {
			return visualElement.resolvedStyle.opacity == 0 || !visualElement.visible;
		}

		public static void SetOpacity(this VisualElement visualElement, bool show) {
			if(visualElement != null) {
				visualElement.style.opacity = show ? 1 : 0;
			}
		}

		public static void SetOpacity(this VisualElement visualElement, float opacity) {
			if(visualElement != null) {
				visualElement.style.opacity = opacity;
			}
		}

		public static void SetDisplay(this VisualElement visualElement, DisplayStyle display) {
			if(visualElement != null) {
				visualElement.style.display = display;
			}
		}

		/// <summary>
		/// This will set the opacity to 0 / zero and add / remove 'hidden' class list based on isVisible to the element
		/// </summary>
		/// <param name="element"></param>
		/// <param name="isVisible"></param>
		public static void SetElementVisibility(this VisualElement element, bool isVisible) {
			element.SetOpacity(isVisible);
			if(isVisible) {
				element.RemoveFromClassList("hidden");
			} else {
				element.AddToClassList("hidden");
			}
		}

		public static void SetDisplay(this VisualElement visualElement, bool flex) {
			if(visualElement != null) {
				visualElement.style.display = flex ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		public static void HideElement(this VisualElement visualElement) {
			if(visualElement != null) {
				visualElement.style.position = Position.Absolute;
				visualElement.style.width = 0;
				visualElement.style.height = 0;
				visualElement.visible = false;
			}
		}

		public static void ShowElement(this VisualElement visualElement) {
			if(visualElement != null) {
				visualElement.style.position = new StyleEnum<Position>(StyleKeyword.Null);
				visualElement.style.width = new StyleLength(StyleKeyword.Null);
				visualElement.style.height = new StyleLength(StyleKeyword.Null);
				visualElement.visible = true;
			}
		}

		public static void ShowElement(this VisualElement visualElement, float width, float height) {
			if(visualElement != null) {
				visualElement.style.position = Position.Relative;
				visualElement.style.width = width;
				visualElement.style.height = height;
				visualElement.visible = true;
			}
		}

		public static void ShowElement(this VisualElement visualElement, Vector2 size) {
			if(visualElement != null) {
				visualElement.style.position = Position.Relative;
				visualElement.style.width = size.x;
				visualElement.style.height = size.y;
				visualElement.visible = true;
			}
		}

		/// <summary>
		/// Is the visual element is visible, true if opacity not zero and visible is true
		/// </summary>
		/// <param name="visualElement"></param>
		/// <returns></returns>
		public static bool IsVisible(this VisualElement visualElement) {
			return visualElement.resolvedStyle.opacity != 0 && visualElement.visible;
		}

		public static void SetLayout(this VisualElement visualElement, Rect rect) {
			if(visualElement.layout == rect)
				return;
			visualElement.style.position = Position.Absolute;
			visualElement.style.marginLeft = 0f;
			visualElement.style.marginRight = 0f;
			visualElement.style.marginBottom = 0f;
			visualElement.style.marginTop = 0f;
			visualElement.style.top = rect.y;
			visualElement.style.left = rect.x;
			visualElement.style.width = rect.width;
			visualElement.style.height = rect.height;
			visualElement.style.right = float.NaN;
			visualElement.style.bottom = float.NaN;
		}

		public static void SetSize(this VisualElement visualElement, Vector2 size) {
			visualElement.style.width = size.x;
			visualElement.style.height = size.y;
		}

		public static void SetPosition(this VisualElement visualElement, Vector2 position) {
			visualElement.style.top = position.y;
			visualElement.style.left = position.x;
		}

		public static void SetToNoClipping(this VisualElement visualElement) {
			visualElement.style.overflow = Overflow.Visible;
		}

		public static void SetToClipContents(this VisualElement visualElement) {
			visualElement.style.overflow = Overflow.Hidden;
		}

		public static void RegisterRepaintAction(this VisualElement visualElement, Action action) {
			if(action == null)
				return;
			visualElement.Add(new RepaintEventElement(action));
		}

		public static void ExecuteAndScheduleAction(this VisualElement visualElement, Action action, long intervalMS) {
			action();
			ScheduleAction(visualElement, action, intervalMS);
		}

		public static void ScheduleOnce(this VisualElement visualElement, Action action, long intervalMS) {
			var schedule = visualElement.schedule.Execute(action);
			schedule.ExecuteLater(intervalMS);
		}

		public static void ScheduleAction(this VisualElement visualElement, Action action, long intervalMS) {
			var schedule = visualElement.schedule.Execute(action);
			schedule.Every(intervalMS);
		}

		public static void ScheduleAction(this VisualElement visualElement, Action<TimerState> action, long intervalMS) {
			var schedule = visualElement.schedule.Execute(action);
			schedule.Every(intervalMS);
		}

		public static void ScheduleActionUntil(this VisualElement visualElement, Action action, Func<bool> stopCondition) {
			var schedule = visualElement.schedule.Execute(action);
			schedule.Until(stopCondition);
		}

		public static void ScheduleActionUntil(this VisualElement visualElement, Action<TimerState> action, Func<bool> stopCondition) {
			var schedule = visualElement.schedule.Execute(action);
			schedule.Until(stopCondition);
		}

		public static void RegisterCallback(this VisualElement visualElement, GroupCallback callback)  {
			if(callback != null) {
				callback.RegisterCallbacks(visualElement);
			}
		}

		public static void RemoveCallback(this VisualElement visualElement, GroupCallback callback) {
			if(callback != null) {
				callback.UnregisterCallbacks(visualElement);
			}
		}
		#endregion

		#region Find Connections
		public static List<UNodeView> FindConnectedNodes(UNodeView source, HashSet<UNodeView> oldNodes = null) {
			if(oldNodes == null) {
				oldNodes = new HashSet<UNodeView>();
			}
			List<UNodeView> nodes = new List<UNodeView>();
			if(oldNodes.Contains(source)) {
				return nodes;
			}

			oldNodes.Add(source);
			var outputs = source.outputPorts.
				Where((p) => p.connected && p.isValue && !p.IsProxy()).
				SelectMany(p => p.GetConnectedNodes()).ToList();
			outputs.RemoveAll((n) => oldNodes.Contains(n));
			var inputs = source.inputPorts.
				Where((p) => p.connected && p.isValue && !p.IsProxy()).
				SelectMany(p => p.GetConnectedNodes()).ToList();
			inputs.RemoveAll((n) => oldNodes.Contains(n));
			var flows = source.outputPorts.
				Where((p) => p.connected && p.isFlow && !p.IsProxy()).
				SelectMany(p => p.GetConnectedNodes()).ToList();
			flows.RemoveAll((n) => oldNodes.Contains(n));
			if(source is INodeBlock) {
				INodeBlock block = source as INodeBlock;
				var inputBlock = block.blockViews.SelectMany(b => b.portViews).Where((p) =>
						p.connected &&
						p.isValue &&
						!p.IsProxy()).
					SelectMany(p => p.GetConnectedNodes()).Distinct();
				inputs.AddRange(inputBlock);
			}

			nodes.AddRange(outputs);
			nodes.AddRange(inputs);
			nodes.AddRange(flows);
			foreach(var n in flows) {
				var nestedNodes = FindConnectedNodes(n, oldNodes);
				nestedNodes.RemoveAll(c => nodes.Contains(c));
				nodes.AddRange(nestedNodes);
			}
			foreach(var n in outputs) {
				var nestedNodes = FindConnectedOutputValueNodes(n, oldNodes);
				nestedNodes.RemoveAll(c => nodes.Contains(c));
				nodes.AddRange(nestedNodes);
			}
			foreach(var n in inputs) {
				var nestedNodes = FindConnectedInputValueNodes(n, oldNodes);
				nestedNodes.RemoveAll(c => nodes.Contains(c));
				nodes.AddRange(nestedNodes);
			}
			return nodes;
		}

		public static List<UNodeView> FindConnectedFlowNodes(UNodeView source, HashSet<UNodeView> oldNodes = null) {
			List<UNodeView> nodes = new List<UNodeView>();
			if(oldNodes == null) {
				oldNodes = new HashSet<UNodeView>();
			}
			if(oldNodes.Contains(source)) {
				return nodes;
			}
			oldNodes.Add(source);
			var inputs = source.outputPorts.
				Where((p) => p.connected && p.isFlow && !p.IsProxy()).
				SelectMany(p => p.GetConnectedNodes()).ToList();
			inputs.RemoveAll((n) => oldNodes.Contains(n));

			nodes.AddRange(inputs);
			foreach(var n in inputs) {
				var nestedNodes = FindConnectedNodes(n, oldNodes);
				nestedNodes.RemoveAll(c => nodes.Contains(c));
				nodes.AddRange(nestedNodes);
			}
			return nodes;
		}

		public static List<UNodeView> FindConnectedInputValueNodes(UNodeView source, HashSet<UNodeView> oldNodes = null) {
			List<UNodeView> nodes = new List<UNodeView>();
			if(oldNodes == null) {
				oldNodes = new HashSet<UNodeView>();
			}
			if(oldNodes.Contains(source)) {
				return nodes;
			}
			oldNodes.Add(source);
			var inputs = source.inputPorts.
				Where((p) => p.connected && p.isValue && !p.IsProxy()).
				SelectMany(p => p.GetConnectedNodes()).ToList();
			if(source is INodeBlock) {
				INodeBlock block = source as INodeBlock;
				var inputBlock = block.blockViews.SelectMany(b => b.portViews).Where((p) =>
						p.connected &&
						p.isValue &&
						!p.IsProxy()).
					SelectMany(p => p.GetConnectedNodes()).Distinct();
				inputs.AddRange(inputBlock);
			}
			inputs.RemoveAll((n) => oldNodes.Contains(n));

			nodes.AddRange(inputs);
			foreach(var n in inputs) {
				var nestedNodes = FindConnectedInputValueNodes(n, oldNodes);
				nestedNodes.RemoveAll(c => nodes.Contains(c));
				nodes.AddRange(nestedNodes);
			}
			return nodes;
		}

		public static List<UNodeView> FindConnectedOutputValueNodes(UNodeView source, HashSet<UNodeView> oldNodes = null) {
			List<UNodeView> nodes = new List<UNodeView>();
			if(oldNodes == null) {
				oldNodes = new HashSet<UNodeView>();
			}
			if(oldNodes.Contains(source)) {
				return nodes;
			}
			oldNodes.Add(source);
			var outputs = source.outputPorts.
				Where((p) => p.connected && p.isValue && !p.IsProxy()).
				SelectMany(p => p.GetConnectedNodes()).ToList();
			outputs.RemoveAll((n) => oldNodes.Contains(n));

			nodes.AddRange(outputs);
			foreach(var n in outputs) {
				var nestedNodes = FindConnectedOutputValueNodes(n, oldNodes);
				nestedNodes.RemoveAll(c => nodes.Contains(c));
				nodes.AddRange(nestedNodes);
			}
			return nodes;
		}
		#endregion

		#region Nodes
		public static class Nodes {
			public static void GetConnectedFlowNodes(UNodeView node, HashSet<UNodeView> nodeViews) {
				if(nodeViews.Contains(node)) {
					return;
				}
				nodeViews.Add(node);
				var nodes = node.outputPorts.
					Where((p) => p.connected && p.isFlow && !p.IsProxy()).
					SelectMany(p => p.GetConnectedNodes());
				foreach(var n in nodes) {
					GetConnectedFlowNodes(n, nodeViews);
				}
			}
		}
		#endregion

		#region Fit Nodes
		static class PlaceFitUtils {
			class PlaceFitTree {
				public UNodeView node;

				public PlaceFitTree(UNodeView node) {
					this.node = node;
				}

				public List<PlaceFitTree> flows = new List<PlaceFitTree>();
				public List<PlaceFitTree> inputs = new List<PlaceFitTree>();
				public List<PlaceFitTree> outputs = new List<PlaceFitTree>();
			}

			private static Vector2 flowSpacing = new Vector2(20, 45);
			private static Vector2 valueSpacing = new Vector2(25, 25);

			public static void PlaceFitNodes(UNodeView node) {
				if(node == null)
					return;
				AutoHideGraphElement.ResetVisibility(node.owner);
				var flowNodes = new HashSet<UNodeView>();
				Nodes.GetConnectedFlowNodes(node, flowNodes);
				var tree = BuildTree(node, flowNodes);
				{
					var nodeTrees = GetNodes(tree);
					foreach(var n in nodeTrees) {
						Undo.RegisterCompleteObjectUndo(n.targetNode, "Place Fit Nodes");
					}
				}
				var nodes = new HashSet<UNodeView>();
				PlaceFitNodes(tree, ref nodes);
				//Finalize: Update node positions
				foreach(var n in nodes) {
					n.Teleport(n.targetNode.editorRect);
				}
				AutoHideGraphElement.MarkUpdateVisibility(node.owner);
			}

			private static void PlaceFitNodes(PlaceFitTree tree, ref HashSet<UNodeView> updatedNodes) {
				if(updatedNodes == null) {
					updatedNodes = new HashSet<UNodeView>();
				}
				if(tree.inputs.Count > 0) {
					var parentPos = GetLayoutNode(tree.node);
					List<UNodeView> listNodes = new List<UNodeView>();
					float offset = 0;
					foreach(var childTree in tree.inputs) {
						PlaceFitNodes(childTree, ref updatedNodes);
						var nodes = GetNodes(childTree);
						var totalRect = GetNodeRect(nodes);

						TeleportNodes(nodes, new Vector2(parentPos.x - totalRect.width - valueSpacing.x, parentPos.y + offset), false);
						offset += totalRect.height + valueSpacing.y;
						listNodes.AddRange(nodes);
					}
					if(tree.inputs.Count == 1) {
						var rect = GetNodeRect(listNodes);
						var sourcePosition = GetNodeRect(tree.inputs[0].node);
						MoveNodes(listNodes, new Vector2(0, -(sourcePosition.y - rect.y) + (parentPos.height - sourcePosition.height) / 2));
					} else {
						MoveNodes(listNodes, new Vector2(0, ((parentPos.height - GetNodeRect(listNodes).height) / 2)));
					}
					foreach(var n in listNodes) {
						updatedNodes.Add(n);
					}
				}
				if(tree.outputs.Count > 0) {
					var parentPos = GetLayoutNode(tree.node);
					List<UNodeView> listNodes = new List<UNodeView>();
					float offset = 0;
					foreach(var childTree in tree.outputs) {
						PlaceFitNodes(childTree, ref updatedNodes);
						var nodes = GetNodes(childTree);
						var totalRect = GetNodeRect(nodes);

						TeleportNodes(nodes, new Vector2(parentPos.x + totalRect.width + valueSpacing.x, parentPos.y + offset), false);
						offset += totalRect.height + valueSpacing.y;
						listNodes.AddRange(nodes);
					}
					if(tree.outputs.Count == 1) {
						var rect = GetNodeRect(listNodes);
						var sourcePosition = GetNodeRect(tree.outputs[0].node);
						MoveNodes(listNodes, new Vector2(0, -(sourcePosition.y - rect.y) + (parentPos.height - sourcePosition.height) / 2));
					} else {
						MoveNodes(listNodes, new Vector2(0, ((parentPos.height - GetNodeRect(listNodes).height) / 2)));
					}
					foreach(var n in listNodes) {
						updatedNodes.Add(n);
					}
				}
				if(tree.flows.Count > 0) {
					var parentPos = GetNodeRect(tree.node);
					float parentY = parentPos.y + parentPos.height;
					{
						List<UNodeView> nodeViews = new List<UNodeView>();
						if(tree.inputs.Count > 0) {
							foreach(var childTree in tree.inputs) {
								nodeViews.AddRange(GetNodes(childTree));
							}
						}
						if(tree.outputs.Count > 0) {
							foreach(var childTree in tree.outputs) {
								nodeViews.AddRange(GetNodes(childTree));
							}
						}
						if(nodeViews.Count > 0) {
							foreach(var n in nodeViews) {
								var rect = GetNodeRect(n);
								if(rect.y + rect.height > parentY) {
									parentY = rect.y + rect.height;
								}
							}
						}
					}
					if(tree.flows.Count > 0) {
						List<UNodeView> listNodes = new List<UNodeView>();
						float offset = 0;
						foreach(var childTree in tree.flows) {
							PlaceFitNodes(childTree, ref updatedNodes);
							var nodes = GetNodes(childTree);
							var totalRect = GetLayoutNodes(nodes);
							var dist = Mathf.Abs(GetNodeRect(nodes).width - totalRect.width);

							TeleportNodes(nodes, new Vector2(parentPos.x + offset + dist, parentY + flowSpacing.y), false);
							offset += totalRect.width + flowSpacing.x + dist;
							listNodes.AddRange(nodes);
						}
						if(tree.flows.Count == 1) {
							var rect = GetNodeRect(listNodes);
							var dist = Mathf.Abs(rect.width - GetLayoutNodes(listNodes).width);
							var sourcePosition = GetNodeRect(tree.flows[0].node);
							//var parentPosition = GetNodeRect(tree.node);
							//TeleportNodes(listNodes, new Vector2(parentPosition.x - (sourcePosition.x - rect.x) - ((sourcePosition.width - parentPos.width) / 2), rect.y), false);
							MoveNodes(listNodes, new Vector2(-(sourcePosition.x - rect.x) + (parentPos.width - sourcePosition.width) / 2 - dist, 0));
						} else {
							var rect = GetNodeRect(listNodes);
							var dist = Mathf.Abs(rect.width - GetLayoutNodes(listNodes).width);
							MoveNodes(listNodes, new Vector2(((parentPos.width - rect.width) / 2) - dist, 0));
						}
						foreach(var n in listNodes) {
							updatedNodes.Add(n);
						}
					}
				}
			}

			private static List<UNodeView> GetNodes(PlaceFitTree tree, HashSet<UNodeView> oldNodes = null) {
				if(oldNodes == null) {
					oldNodes = new HashSet<UNodeView>();
				}
				List<UNodeView> nodes = new List<UNodeView>();
				if(oldNodes.Contains(tree.node)) {
					return nodes;
				}
				nodes.Add(tree.node);
				if(tree.inputs.Count > 0) {
					foreach(var n in tree.inputs) {
						nodes.AddRange(GetNodes(n, oldNodes));
					}
				}
				if(tree.outputs.Count > 0) {
					foreach(var n in tree.outputs) {
						nodes.AddRange(GetNodes(n, oldNodes));
					}
				}
				if(tree.flows.Count > 0) {
					foreach(var n in tree.flows) {
						nodes.AddRange(GetNodes(n, oldNodes));
					}
				}
				return nodes;
			}

			private static PlaceFitTree BuildTree(UNodeView node, HashSet<UNodeView> flowNodes, HashSet<UNodeView> buildTrees = null, bool includeFlows = true, bool includeInputs = true, bool includeOutputs = true) {
				if(buildTrees == null) {
					buildTrees = new HashSet<UNodeView>();
				}
				var result = new PlaceFitTree(node);
				if(includeFlows) {//Flows
					var nodes = node.outputPorts.
						Where((p) => p.connected && p.isFlow && !p.IsProxy()).
						SelectMany(p => p.GetConnectedNodes()).ToList();
					foreach(var n in nodes) {
						if(n == null || !buildTrees.Add(n))
							continue;
						var childTree = BuildTree(n, flowNodes, buildTrees, includeFlows: true, includeInputs: true, includeOutputs: true);
						result.flows.Add(childTree);
					}
				}
				if(includeInputs) {//Inputs
					var nodes = node.inputPorts.
						Where((p) => p.connected && p.isValue && !p.IsProxy()).
						SelectMany(p => p.GetConnectedNodes()).
						Where(n => !flowNodes.Contains(n)).ToList();
					foreach(var n in nodes) {
						if(n == null || !buildTrees.Add(n))
							continue;
						var childTree = BuildTree(n, flowNodes, buildTrees, includeFlows:includeFlows, includeInputs:includeInputs, false);
						result.inputs.Add(childTree);
					}
				}
				if(includeOutputs) {//Outputs
					var nodes = node.outputPorts.
						Where((p) => p.connected && p.isValue && !p.IsProxy()).
						SelectMany(p => p.GetConnectedNodes()).
						Where(n => !flowNodes.Contains(n)).ToList();
					foreach(var n in nodes) {
						if(n == null || !buildTrees.Add(n))
							continue;
						var childTree = BuildTree(n, flowNodes, buildTrees, includeFlows: includeFlows, includeInputs: false, includeOutputs: includeOutputs);
						result.outputs.Add(childTree);
					}
				}
				return result;
			}
		}

		public static void PlaceFitNodes(UNodeView node) {
			PlaceFitUtils.PlaceFitNodes(node);
		}
		#endregion

		#region Move And Teleport Nodes
		public static void TeleportNodes(IList<UNodeView> nodes, Vector2 position, Vector2 center) {
			foreach(var node in nodes) {
				if(position.x != 0)
					node.targetNode.editorRect.x = (node.targetNode.editorRect.x - center.x) + position.x;
				if(position.y != 0)
					node.targetNode.editorRect.y = (node.targetNode.editorRect.y - center.y) + position.y;
			}
		}

		public static void TeleportNodes(IList<UNodeView> nodes, Vector2 position, bool fromCenter = true) {
			if(fromCenter) {
				Vector2 center = Vector2.zero;
				foreach(var node in nodes) {
					center.x += node.targetNode.editorRect.x;
					center.y += node.targetNode.editorRect.y;
				}
				center /= nodes.Count;
				foreach(var node in nodes) {
					if(position.x != 0)
						node.targetNode.editorRect.x = (node.targetNode.editorRect.x - center.x) + position.x;
					if(position.y != 0)
						node.targetNode.editorRect.y = (node.targetNode.editorRect.y - center.y) + position.y;
				}
			} else {
				Vector2 pos = Vector2.zero;
				foreach(var node in nodes) {
					var p = node.targetNode.editorRect;
					if(pos.x > p.x || pos.x == 0) {
						pos.x = p.x;
					}
					if(pos.y > p.y || pos.y == 0) {
						pos.y = p.y;
					}
				}
				foreach(var node in nodes) {
					if(position.x != 0)
						node.targetNode.editorRect.x = (node.targetNode.editorRect.x - pos.x) + position.x;
					if(position.x != 0)
						node.targetNode.editorRect.y = (node.targetNode.editorRect.y - pos.y) + position.y;
				}
			}
		}

		public static void MoveNodes(IList<UNodeView> nodes, Vector2 position) {
			foreach(var node in nodes) {
				node.targetNode.editorRect.x += position.x;
				node.targetNode.editorRect.y += position.y;
			}
		}
		#endregion

		#region Get Node Rect
		private static Rect GetNodeRect(UNodeView node) {
			return new Rect(node.targetNode.editorRect.x, node.targetNode.editorRect.y, node.layout.width, node.layout.height);
		}

		public static Rect GetLayoutNode(UNodeView node) {
			var container = node.portInputContainer;
			var rect = GetNodeRect(node);
			if(container != null) {
				var inputPorts = container.Children().Where(i => i is PortInputView).Select(i => i as PortInputView);
				float width = 0;
				foreach(var port in inputPorts) {
					if(port.IsVisible()) {
						width = Mathf.Max(port.GetPortWidth() - 19, width);
					}
				}
				rect.width += width;
				rect.x -= width;
				return rect;
			}
			return rect;
		}

		public static Rect GetLayoutNodes(IList<UNodeView> nodes) {
			if(nodes == null || nodes.Count == 0)
				return Rect.zero;
			if(nodes.Count == 1) {
				return GetLayoutNode(nodes[0]);
			}
			Rect rect = Rect.zero;
			foreach(var node in nodes) {
				if(rect == Rect.zero) {
					rect = GetLayoutNode(node);
					continue;
				}
				rect = RectUtils.Encompass(rect, GetLayoutNode(node));
			}
			return rect;
		}

		public static Rect GetNodeRect(IList<UNodeView> nodes) {
			if(nodes == null || nodes.Count == 0)
				return Rect.zero;
			if(nodes.Count == 1) {
				return GetNodeRect(nodes[0]);
			}
			Rect rect = Rect.zero;
			foreach(var node in nodes) {
				if(rect == Rect.zero) {
					rect = GetNodeRect(node);
					continue;
				}
				rect = RectUtils.Encompass(rect, GetNodeRect(node));
			}
			return rect;
		}

		public static Rect GetNodeRect(IList<NodeComponent> nodes) {
			if(nodes == null || nodes.Count == 0)
				return Rect.zero;
			if(nodes.Count == 1) {
				return nodes[0].editorRect;
			}
			Rect rect = Rect.zero;
			foreach(var node in nodes) {
				if(rect == Rect.zero) {
					rect = node.editorRect;
					continue;
				}
				rect = RectUtils.Encompass(rect, node.editorRect);
			}
			return rect;
		}
		#endregion

		/// <summary>
		/// Get the root visual element
		/// </summary>
		/// <param name="visualElement"></param>
		/// <returns></returns>
		public static VisualElement GetRootElement(this VisualElement visualElement) {
			if(visualElement.parent != null) {
				return GetRootElement(visualElement.parent);
			}
			return visualElement;
		}

		/// <summary>
		/// Get the screen mouse position from the element and its local mouse position.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="mousePosition"></param>
		/// <param name="window"></param>
		/// <returns></returns>
		public static Vector2 GetScreenMousePosition(this VisualElement element, Vector2 mousePosition, EditorWindow window) {
			Vector2 position = window.GetMousePositionForMenu(element.ChangeCoordinatesTo(
				window.rootVisualElement,
				mousePosition));
			return position;
		}

		public static T CreateLayoutElement<T>(T element) where T : VisualElement {
			element.EnableInClassList("Layout", true);
			return element;
		}

		/// <summary>
		/// Check the port if is proxy.
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public static bool IsProxy(PortView port) {
			if(port != null) {
				MemberData member = port.portData?.GetPortValue();
				return IsProxy(member);
			}
			return false;
		}

		/// <summary>
		/// Check the member if is proxy.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static bool IsProxy(MemberData member) {
			if(member != null) {
				return member.controlPoints != null && member.controlPoints.Length > 0 && member.controlPoints[0].point == Vector2.zero;
			}
			return false;
		}

		/// <summary>
		/// Get the actual node view type.
		/// </summary>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		public static Type GetNodeViewTypeFromType(Type nodeType) {
			if(nodeViewPerType == null) {
				nodeViewPerType = new Dictionary<Type, Type>();
				foreach(var asm in EditorReflectionUtility.GetAssemblies()) {
					foreach(var type in EditorReflectionUtility.GetAssemblyTypes(asm)) {
						if(type.IsClass && !type.IsAbstract) {
							if(type.IsSubclassOf(typeof(UNodeView))) {
								if(type.GetCustomAttributes(typeof(NodeCustomEditor), true) is NodeCustomEditor[] attrs && attrs.Length > 0) {
									for(int i = 0; i < attrs.Length; i++) {
										Type nType = attrs[i].nodeType;
										nodeViewPerType[nType] = type;
									}
								}
							}
						}
					}
				}
			}
			Type oriType = nodeType;
			Type view = null;
			while(nodeType != typeof(UNodeView) && nodeType != typeof(object) && nodeType != typeof(BaseNodeView)) {
				if(nodeViewPerType.TryGetValue(nodeType, out view)) {
					if(oriType != nodeType) {
						nodeViewPerType[oriType] = view;
					}
					return view;
				} else {
					nodeType = nodeType.BaseType;
				}
			}
			return view;
		}

		private static List<ControlFieldAttribute> _controlsField;
		/// <summary>
		/// Find command pin menu.
		/// </summary>
		/// <returns></returns>
		public static List<ControlFieldAttribute> FindControlsField() {
			if(_controlsField == null) {
				_controlsField = new List<ControlFieldAttribute>();

				foreach(System.Reflection.Assembly assembly in EditorReflectionUtility.GetAssemblies()) {
					foreach(System.Type type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
						var atts = type.GetCustomAttributes(typeof(ControlFieldAttribute), true);
						if(atts.Length > 0) {
							foreach(var a in atts) {
								var control = a as ControlFieldAttribute;
								control.classType = type;
								_controlsField.Add(control);
							}
						}
					}
				}
				_controlsField.Sort((x, y) => Comparer<int>.Default.Compare(x.order, y.order));
			}
			return _controlsField;
		}

		public static ValueControl CreateControl(Type controlType, ControlConfig config, bool autoLayout = false) {
			var controlAtts = FindControlsField();
			for(int i = 0; i < controlAtts.Count; i++) {
				if(controlAtts[i].type == controlType || controlType.IsSubclassOf(controlAtts[i].type)) {
					return Activator.CreateInstance(controlAtts[i].classType, new object[] { config, autoLayout }) as ValueControl;
				}
			}
			return new DefaultControl(config, autoLayout);
		}

		public static Type FindControlType(Type controlType) {
			var controlAtts = FindControlsField();
			for(int i = 0; i < controlAtts.Count; i++) {
				if(controlAtts[i].type == controlType || controlType.IsSubclassOf(controlAtts[i].type)) {
					return controlAtts[i].classType;
				}
			}
			return typeof(DefaultControl);
		}
		
		/// <summary>
		/// Get the current editor theme
		/// </summary>
		public static VerticalEditorTheme Theme => uNodePreference.editorTheme as VerticalEditorTheme;
	}

	public class RepaintEventElement : ImmediateModeElement {
		private Action action;

		public RepaintEventElement(Action action) {
			style.position = Position.Absolute;
			style.width = 0;
			style.height = 0;
			this.action = action;
		}

		protected override void ImmediateRepaint() {
			action?.Invoke();
		}
	}
}