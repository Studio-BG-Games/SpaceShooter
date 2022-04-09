using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	internal class GraphPanel : VisualElement, IDragManager, IDisposable {
		ScrollView scroll;
		UIElementGraph graph;

		private GraphEditorData editorData {
			get {
				return graph.editorData;
			}
		}

		VisualElement classContainer, variableContainer, propertyContainer, functionContainer, eventContainer, constructorContainer, namespaceElement, interfaceContainer, enumContainer, localContainer, nestedContainer;
		ClickableElement namespaceButton, classAddButton, functionAddButton;
		Dictionary<object, ClickableElement> contentMap = new Dictionary<object, ClickableElement>();

		[SerializeField]
		bool showClasses = true, showVariables = true, showProperties = true, showFunctions = true, showEvents = true, showConstructors = true, showInterfaces = true, showEnums = true, showLocal = true, showNested = true;

		public List<VisualElement> draggableElements {
			get {
				return contentMap.Where(item => item.Value is IDragableElement).Select(item => item.Value as VisualElement).ToList();
			}
		}

		public GraphPanel(UIElementGraph graph) {
			this.graph = graph;
			this.StretchToParentSize();
			this.AddStyleSheet("uNodeStyles/NativePanelStyle");
			this.AddStyleSheet(UIElementUtility.Theme.graphPanelStyle);
			scroll = new ScrollView(ScrollViewMode.Vertical) {
				name = "scroll-view",
			};
			//scroll.Add(explorer);
			scroll.StretchToParentSize();
			Add(scroll);
			this.RegisterCallback(new DragableElementEvent());
			InitializeView();
		}

		void InitializeView() {
			VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("uxml/GraphPanel");
			visualTreeAsset.CloneTree(scroll.contentContainer);
			{//Namespace
				namespaceElement = new VisualElement() { name = "namespace" };
				namespaceElement.style.flexDirection = FlexDirection.Row;
				namespaceElement.Add(new Label("Namespace") { name = "label" });
				namespaceButton = new ClickableElement("") { name = "value" };
				namespaceButton.onClick = () => {
					var mPos = (namespaceButton.clickedEvent.currentTarget as VisualElement).GetScreenMousePosition((namespaceButton.clickedEvent as IMouseEvent).localMousePosition, graph.window);
					if (editorData.graphData == null && !(editorData.graph is IIndependentGraph)) {
						editorData.owner.AddComponent<uNodeData>();
					}
					OnNamespaceClicked(mPos, editorData.graphData);
				};
				namespaceElement.Add(namespaceButton);
				scroll.contentContainer.Insert(0, namespaceElement);
			}
			{//Classes
				VisualElement header = scroll.Q("class");
				classContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.GraphIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					plusElement.menu = new DropdownMenu();
					plusElement.menu.AppendAction("Class", act => {
						Undo.AddComponent<uNodeClass>(editorData.owner);
						ReloadView();
					});
					plusElement.menu.AppendAction("Struct", act => {
						Undo.AddComponent<uNodeStruct>(editorData.owner);
						ReloadView();
					});
					if (editorData.graph is IClassSystem) {
						plusElement.menu.AppendSeparator("");
						plusElement.menu.AppendAction("Interface", act => {
							var data = editorData.graphData;
							if (!data) {
								data = editorData.owner.AddComponent<uNodeData>();
							}
							uNodeEditorUtility.RegisterUndo(data, "Add Interface");
							ArrayUtility.Add(ref data.interfaces, new InterfaceData() { name = "newInterface" });
							ReloadView();
						});
						plusElement.menu.AppendAction("Enum", act => {
							var data = editorData.graphData;
							if (!data) {
								data = editorData.owner.AddComponent<uNodeData>();
							}
							uNodeEditorUtility.RegisterUndo(data, "Add Enum");
							ArrayUtility.Add(ref data.enums, new EnumData() { name = "newEnum" });
							ReloadView();
						});
					}
					var templates = uNodeEditorUtility.FindAssetsByType<uNodeTemplate>();
					if (templates != null && templates.Count > 0) {
						plusElement.menu.AppendSeparator("");
						foreach (var t in templates) {
							string path = t.name;
							if (!string.IsNullOrEmpty(t.path)) {
								path = t.path;
							}
							var tmp = t;
							plusElement.menu.AppendAction(path, act => {
								Serializer.Serializer.Deserialize(tmp.serializedData, editorData.owner);
								//var comp = editorData.owner.GetComponents<uNodeRoot>();
							});
						}
					}
				};
				icon.parent.Add(plusElement);
				classAddButton = plusElement;
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showClasses = evt.newValue;
					ReloadView();
				});
			}
			{//Variable
				VisualElement header = scroll.Q("variable");
				variableContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					var mPos = (plusElement.clickedEvent.currentTarget as VisualElement).GetScreenMousePosition((plusElement.clickedEvent as IMouseEvent).localMousePosition, graph.window);
					ShowTypeMenu(mPos, type => {
						uNodeEditorUtility.AddVariable(type, editorData.graph.Variables, editorData.graph);
						ReloadView();
					});
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showVariables = evt.newValue;
					ReloadView();
				});
			}
			{//Property
				VisualElement header = scroll.Q("property");
				propertyContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.PropertyIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					var mPos = (plusElement.clickedEvent.currentTarget as VisualElement).GetScreenMousePosition((plusElement.clickedEvent as IMouseEvent).localMousePosition, graph.window);
					ShowTypeMenu(mPos, type => {
						NodeEditorUtility.AddNewProperty(editorData.graph, "newProperty", delegate (uNodeProperty prop) {
							prop.type = new MemberData(type, MemberData.TargetType.Type);
						});
						graph.Refresh();
					});
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showProperties = evt.newValue;
					ReloadView();
				});
			}
			{//Events
				VisualElement header = scroll.Q("event");
				eventContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.EventIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showEvents = evt.newValue;
					ReloadView();
				});
			}
			{//Function
				VisualElement header = scroll.Q("function");
				functionContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					plusElement.menu = new DropdownMenu();
					plusElement.menu.AppendAction("Add new", act => {
						NodeEditorUtility.AddNewFunction(editorData.graph, "NewFunction", typeof(void), f => {
							if(uNodePreference.preferenceData.newVariableAccessor == uNodePreference.DefaultAccessor.Private) {
								f.modifiers.SetPrivate();
							}
						});
						graph.Refresh();
					});
					plusElement.menu.AppendAction("Add new coroutine", act => {
						NodeEditorUtility.AddNewFunction(editorData.graph, "NewFunction", typeof(System.Collections.IEnumerator), f => {
							if(uNodePreference.preferenceData.newVariableAccessor == uNodePreference.DefaultAccessor.Private) {
								f.modifiers.SetPrivate();
							}
						});
						graph.Refresh();
					});
					Type inheritType = editorData.graph.GetInheritType();
					if (editorData.graph is uNodeStruct) {
						inheritType = typeof(ValueType);
					}

					#region UnityEvent
					if (typeof(MonoBehaviour).IsAssignableFrom(inheritType)) {
						plusElement.menu.AppendSeparator("");
						{//Start Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("Start", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Behavior/Start()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "Start", typeof(void), f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//Awake Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("Awake", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Behavior/Awake()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "Awake", typeof(void), f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnDestroy Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnDestroy", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Behavior/OnDestroy()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnDestroy", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnDisable Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnDisable", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Behavior/OnDisable()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnDisable", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnEnable Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnEnable", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Behavior/OnEnable()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnEnable", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//Update Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("Update", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Gameloop/Update()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "Update", typeof(void), f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//FixedUpdate Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("FixedUpdate", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Gameloop/FixedUpdate()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "FixedUpdate", typeof(void), f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//LateUpdate Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("LateUpdate", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Gameloop/LateUpdate()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "LateUpdate", typeof(void), f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnGUI Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnGUI", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Gameloop/OnGUI()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnGUI", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnAnimatorIK Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnAnimatorIK", 0, typeof(int))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Animation/OnAnimatorIK(int)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnAnimatorIK", typeof(void), new string[] { "layerIndex" }, new Type[] { typeof(int) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnAnimatorMove Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnAnimatorMove", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Animation/OnAnimatorMove()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnAnimatorMove", typeof(void), f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnApplicationFocus Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnApplicationFocus", 0, typeof(bool))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Game Event/OnApplicationFocus(bool)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnApplicationFocus", typeof(void), new string[] { "focusStatus" }, new Type[] { typeof(bool) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnApplicationPause Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnApplicationPause", 0, typeof(bool))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Game Event/OnApplicationPause(bool)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnApplicationPause", typeof(void), new string[] { "pauseStatus" }, new Type[] { typeof(bool) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnApplicationQuit Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnApplicationQuit", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Game Event/OnApplicationQuit()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnApplicationQuit", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnCollisionEnter Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnCollisionEnter", 0, typeof(Collision))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnCollisionEnter(Collision)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnCollisionEnter", typeof(void), new string[] { "collisionInfo" }, new Type[] { typeof(Collision) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnCollisionEnter2D Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnCollisionEnter2D", 0, typeof(Collision2D))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnCollisionEnter2D(Collision2D)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnCollisionEnter2D", typeof(void), new string[] { "collisionInfo" }, new Type[] { typeof(Collision2D) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnCollisionExit Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnCollisionExit", 0, typeof(Collision))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnCollisionExit(Collision)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnCollisionExit", typeof(void), new string[] { "collisionInfo" }, new Type[] { typeof(Collision) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnCollisionExit2D Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnCollisionExit2D", 0, typeof(Collision2D))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnCollisionExit2D(Collision2D)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnCollisionExit2D", typeof(void), new string[] { "collisionInfo" }, new Type[] { typeof(Collision2D) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnCollisionStay Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnCollisionStay", 0, typeof(Collision))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnCollisionStay(Collision)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnCollisionStay", typeof(void), new string[] { "collisionInfo" }, new Type[] { typeof(Collision) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnCollisionStay2D Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnCollisionStay2D", 0, typeof(Collision2D))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnCollisionStay2D(Collision2D)", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnCollisionStay2D", typeof(void), new string[] { "collisionInfo" }, new Type[] { typeof(Collision2D) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnParticleCollision Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnParticleCollision", 0, typeof(GameObject))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnParticleCollision(GameObject)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnParticleCollision", typeof(void), new string[] { "other" }, new Type[] { typeof(GameObject) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTriggerEnter Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTriggerEnter", 0, typeof(Collider))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnTriggerEnter(Collider)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTriggerEnter", typeof(void), new string[] { "colliderInfo" }, new Type[] { typeof(Collider) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTriggerEnter2D Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTriggerEnter2D", 0, typeof(Collider2D))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnTriggerEnter2D(Collider2D)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTriggerEnter2D", typeof(void), new string[] { "colliderInfo" }, new Type[] { typeof(Collider2D) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTriggerExit Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTriggerExit", 0, typeof(Collider))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnTriggerExit(Collider)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTriggerExit", typeof(void), new string[] { "colliderInfo" }, new Type[] { typeof(Collider) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTriggerExit2D Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTriggerExit2D", 0, typeof(Collider2D))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnTriggerExit2D(Collider2D)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTriggerExit2D", typeof(void), new string[] { "colliderInfo" }, new Type[] { typeof(Collider2D) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTriggerStay Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTriggerStay", 0, typeof(Collider))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnTriggerStay(Collider)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTriggerStay", typeof(void), new string[] { "colliderInfo" }, new Type[] { typeof(Collider) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTriggerStay2D Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTriggerStay2D", 0, typeof(Collider2D))) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Physics/OnTriggerStay2D(Collider2D)", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTriggerStay2D", typeof(void), new string[] { "colliderInfo" }, new Type[] { typeof(Collider2D) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTransformChildrenChanged Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTransformChildrenChanged", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Transfrom/OnTransformChildrenChanged()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTransformChildrenChanged", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnTransformParentChanged Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnTransformParentChanged", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Transfrom/OnTransformParentChanged()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnTransformParentChanged", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseDown Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseDown", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseDown()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseDown", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseDrag Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseDrag", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseDrag()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseDrag", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseEnter Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseEnter", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseEnter()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseEnter", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseExit Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseExit", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseExit()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseExit", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseOver Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseOver", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseOver()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseOver", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseUp Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseUp", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseUp()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseUp", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnMouseUpAsButton Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnMouseUpAsButton", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Mouse/OnMouseUpAsButton()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnMouseUpAsButton", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnBecameInvisible Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnBecameInvisible", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnBecameInvisible()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnBecameInvisible", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnBecameVisible Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnBecameVisible", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnBecameVisible()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnBecameVisible", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnPostRender Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnPostRender", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnPostRender()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnPostRender", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnPreCull Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnPreCull", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnPreCull()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnPreCull", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnPreRender Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnPreRender", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnPreRender()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnPreRender", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnRenderObject Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnRenderObject", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnRenderObject()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnRenderObject", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnRenderImage Event
							bool hasFunction = false;
							if(editorData.graph.GetFunction("OnRenderImage", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnRenderImage()", act => {
								if(!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnRenderImage", typeof(void), new[] { "src", "dest" }, new[] { typeof(RenderTexture), typeof(RenderTexture) }, action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						{//OnWillRenderObject Event
							bool hasFunction = false;
							if (editorData.graph.GetFunction("OnWillRenderObject", 0)) {
								hasFunction = true;
							}
							plusElement.menu.AppendAction("UnityEvent/Renderer/OnWillRenderObject()", act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, "OnWillRenderObject", typeof(void), action: f => f.modifiers.SetPrivate());
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
						if(editorData.graph is uNodeClass) {
							{//OnDrawGizmos Event
								bool hasFunction = false;
								if(editorData.graph.GetFunction("OnDrawGizmos", 0)) {
									hasFunction = true;
								}
								plusElement.menu.AppendAction("UnityEvent/Editor/OnDrawGizmos()", act => {
									if(!hasFunction) {
										NodeEditorUtility.AddNewFunction(editorData.graph, "OnDrawGizmos", typeof(void), action: f => f.modifiers.SetPrivate());
										graph.Refresh();
									}
								}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
							}
							{//OnDrawGizmosSelected Event
								bool hasFunction = false;
								if(editorData.graph.GetFunction("OnDrawGizmosSelected", 0)) {
									hasFunction = true;
								}
								plusElement.menu.AppendAction("UnityEvent/Editor/OnDrawGizmosSelected()", act => {
									if(!hasFunction) {
										NodeEditorUtility.AddNewFunction(editorData.graph, "OnDrawGizmosSelected", typeof(void), action: f => f.modifiers.SetPrivate());
										graph.Refresh();
									}
								}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
							}
							{//OnValidate Event
								bool hasFunction = false;
								if(editorData.graph.GetFunction("OnValidate", 0)) {
									hasFunction = true;
								}
								plusElement.menu.AppendAction("UnityEvent/Editor/OnValidate()", act => {
									if(!hasFunction) {
										NodeEditorUtility.AddNewFunction(editorData.graph, "OnValidate", typeof(void), action: f => f.modifiers.SetPrivate());
										graph.Refresh();
									}
								}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
							}
							{//Reset Event
								bool hasFunction = false;
								if(editorData.graph.GetFunction("Reset", 0)) {
									hasFunction = true;
								}
								plusElement.menu.AppendAction("UnityEvent/Editor/Reset()", act => {
									if(!hasFunction) {
										NodeEditorUtility.AddNewFunction(editorData.graph, "Reset", typeof(void), action: f => f.modifiers.SetPrivate());
										graph.Refresh();
									}
								}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
							}
						}
					}
					#endregion

					#region Override
					if (inheritType != null) {
						MethodInfo[] methods = inheritType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate (MethodInfo info) {
							if (!info.IsAbstract && !info.IsVirtual)
								return false;
							if (info.IsStatic)
								return false;
							if (info.IsSpecialName)
								return false;
							if (info.IsPrivate)
								return false;
							if (info.IsConstructor)
								return false;
							if (info.Name.StartsWith("get_", StringComparison.Ordinal))
								return false;
							if (info.Name.StartsWith("set_", StringComparison.Ordinal))
								return false;
							if (info.ContainsGenericParameters)
								return false;
							if (!info.IsPublic && !info.IsFamily)
								return false;
							if (info.IsFamilyAndAssembly)
								return false;
							if (info.IsDefinedAttribute(typeof(ObsoleteAttribute)))
								return false;
							if (info.GetCustomAttributes(true).Length > 0) {
								if (info.IsDefinedAttribute(typeof(System.Runtime.ConstrainedExecution.ReliabilityContractAttribute)))
									return false;
							}
							return true;
						}).ToArray();
						foreach (var method in methods) {
							bool hasFunction = false;
							if (editorData.graph.GetFunction(method.Name, method.GetGenericArguments().Length,
								method.GetParameters()
								.Select(item => item.ParameterType).ToArray())) {
								hasFunction = true;
							}
							var m = method;
							plusElement.menu.AppendAction("Override Function/" + EditorReflectionUtility.GetOverloadingMethodNames(method), act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, m.Name, m.ReturnType,
										m.GetParameters().Select(item => item.Name).ToArray(),
										m.GetParameters().Select(item => item.ParameterType).ToArray(),
										m.GetGenericArguments().Select(item => item.Name).ToArray(),
										delegate (uNodeFunction function) {
											function.modifiers.Override = true;
											function.modifiers.Private = m.IsPrivate;
											function.modifiers.Public = m.IsPublic;
											function.modifiers.Internal = m.IsAssembly;
											function.modifiers.Protected = m.IsFamily;
											if (m.IsFamilyOrAssembly) {
												function.modifiers.Internal = true;
												function.modifiers.Protected = true;
											}
										});
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
					}
					#endregion

					#region Hide Function
					if (inheritType != null) {
						MethodInfo[] methods = inheritType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate (MethodInfo info) {
							if (info.IsStatic)
								return false;
							if (info.IsPrivate)
								return false;
							if (info.IsAbstract)
								return false;
							if (info.IsConstructor)
								return false;
							if (info.IsSpecialName)
								return false;
							if (info.Name.StartsWith("get_", StringComparison.Ordinal))
								return false;
							if (info.Name.StartsWith("set_", StringComparison.Ordinal))
								return false;
							if (info.ContainsGenericParameters)
								return false;
							if (!info.IsPublic && !info.IsFamily)
								return false;
							if (info.IsDefinedAttribute(typeof(ObsoleteAttribute)))
								return false;
							if (info.GetCustomAttributes(true).Length > 0) {
								if (info.IsDefinedAttribute(typeof(System.Runtime.ConstrainedExecution.ReliabilityContractAttribute)))
									return false;
							}
							return true;
						}).ToArray();
						foreach (var method in methods) {
							bool hasFunction = false;
							if (editorData.graph.GetFunction(method.Name, method.GetGenericArguments().Length,
								method.GetParameters()
								.Select(item => item.ParameterType).ToArray())) {
								hasFunction = true;
							}
							var m = method;
							plusElement.menu.AppendAction("Hide Function/" + EditorReflectionUtility.GetOverloadingMethodNames(method), act => {
								if (!hasFunction) {
									NodeEditorUtility.AddNewFunction(editorData.graph, m.Name, m.ReturnType,
										m.GetParameters().Select(item => item.Name).ToArray(),
										m.GetParameters().Select(item => item.ParameterType).ToArray(),
										m.GetGenericArguments().Select(item => item.Name).ToArray(),
										delegate (uNodeFunction function) {
											function.modifiers.New = true;
											function.modifiers.Private = m.IsPrivate;
											function.modifiers.Public = m.IsPublic;
											function.modifiers.Internal = m.IsAssembly;
											function.modifiers.Protected = m.IsFamily;
											if (m.IsFamilyOrAssembly) {
												function.modifiers.Internal = true;
												function.modifiers.Protected = true;
											}
										});
									graph.Refresh();
								}
							}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
						}
					}
					#endregion

					#region Implement Interfaces
					var interfaceSystem = editorData.graph as IInterfaceSystem;
					if (interfaceSystem != null && interfaceSystem.Interfaces.Count > 0) {
						foreach (var inter in interfaceSystem.Interfaces) {
							if (inter == null || !inter.isAssigned)
								continue;
							Type t = inter.Get<Type>();
							if (t != null) {
								MethodInfo[] methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate (MethodInfo info) {
									if (info.Name.StartsWith("get_", StringComparison.Ordinal))
										return false;
									if (info.Name.StartsWith("set_", StringComparison.Ordinal))
										return false;
									return true;
								}).ToArray();
								foreach (var method in methods) {
									bool hasFunction = false;
									if (editorData.graph.GetFunction(method.Name, method.GetGenericArguments().Length,
										method.GetParameters()
										.Select(item => item.ParameterType).ToArray())) {
										hasFunction = true;
									}

									var m = method;
									plusElement.menu.AppendAction("Interface " + t.Name + "/" + EditorReflectionUtility.GetOverloadingMethodNames(method), act => {
										if (!hasFunction) {
											NodeEditorUtility.AddNewFunction(editorData.graph, m.Name, m.ReturnType,
												m.GetParameters().Select(item => item.Name).ToArray(),
												m.GetParameters().Select(item => item.ParameterType).ToArray(),
												m.GetGenericArguments().Select(item => item.Name).ToArray());
											graph.Refresh();
										}
									}, a => hasFunction ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
								}
							}
						}
					}
					#endregion
				};
				functionAddButton = plusElement;
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showFunctions = evt.newValue;
					ReloadView();
				});
			}
			{//Constructor
				VisualElement header = scroll.Q("constructor");
				constructorContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					if (editorData.graph != null)
						CreateNewConstructor(editorData.graph);
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showConstructors = evt.newValue;
					ReloadView();
				});
			}
			{//Local Variable
				VisualElement header = scroll.Q("localvariable");
				localContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.LocalVariableIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					if (editorData.selectedRoot != null) {
						var mPos = (plusElement.clickedEvent.currentTarget as VisualElement).GetScreenMousePosition((plusElement.clickedEvent as IMouseEvent).localMousePosition, graph.window);
						ShowTypeMenu(mPos, type => {
							List<VariableData> UNR = editorData.selectedRoot.LocalVariables;
							uNodeEditorUtility.AddVariable(type, UNR, editorData.selectedRoot);
							ReloadView();
						});
					}
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showLocal = evt.newValue;
					ReloadView();
				});
			}
			{//Nested Types
				VisualElement header = scroll.Q("nestedtypes");
				nestedContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.GraphIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					plusElement.menu = new DropdownMenu();
					CreateNestedTypesMenu(plusElement.menu, editorData.graph as INestedClassSystem);
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showNested = evt.newValue;
					ReloadView();
				});
			}
			{//Interfaces
				VisualElement header = scroll.Q("interface");
				interfaceContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.InterfaceIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					var data = editorData.graphData;
					if (!data) {
						data = editorData.owner.AddComponent<uNodeData>();
					}
					uNodeEditorUtility.RegisterUndo(data, "Add Interface");
					ArrayUtility.Add(ref data.interfaces, new InterfaceData() { name = "newInterface" });
					ReloadView();
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showInterfaces = evt.newValue;
					ReloadView();
				});
			}
			{//Enums
				VisualElement header = scroll.Q("enum");
				enumContainer = header.Q("contents");
				var icon = header.Q("title-icon") as Image;
				icon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.EnumIcon));
				icon.parent.Add(new VisualElement() { name = "spacer" });
				var plusElement = new ClickableElement("+") {
					name = "title-button-add",
				};
				plusElement.onClick = () => {
					var data = editorData.graphData;
					if (!data) {
						data = editorData.owner.AddComponent<uNodeData>();
					}
					uNodeEditorUtility.RegisterUndo(data, "Add Enum");
					ArrayUtility.Add(ref data.enums, new EnumData() { name = "newEnum" });
					ReloadView();
				};
				icon.parent.Add(plusElement);
				var toggle = header.Q("expanded") as Foldout;
				toggle.RegisterValueChangedCallback(evt => {
					showEnums = evt.newValue;
					ReloadView();
				});
			}
			uNodeEditor.onChanged += ReloadView;
			uNodeEditor.onSelectionChanged += OnSelectionChanged;
			ReloadView();
		}

		public void Dispose() {
			uNodeEditor.onChanged -= ReloadView;
			uNodeEditor.onSelectionChanged -= OnSelectionChanged;
		}

		void OnSelectionChanged(GraphEditorData graphData) {
			UpdateView();
		}

		public void UpdateView() {
			foreach(var pair in contentMap) {
				pair.Value.EnableInClassList("selected", pair.Key == editorData.selected);
				if(pair.Key is uNodeRoot) {
					var obj = pair.Key as uNodeRoot;
					pair.Value.EnableInClassList("active", obj == editorData.graph);
				} else if(pair.Key is uNodeProperty) {
					var obj = pair.Key as uNodeProperty;
					if(!obj.AutoProperty) {
						pair.Value.EnableInClassList("active", obj.setRoot != null && obj.setRoot == editorData.selectedRoot || obj.getRoot != null && obj.getRoot == editorData.selectedRoot);
					}
				} else if(pair.Key is RootObject) {
					var obj = pair.Key as RootObject;
					pair.Value.EnableInClassList("active", obj == editorData.selectedRoot);
				}
			}
		}

		private bool _markedRepaint;
		public void MarkRepaint() {
			if(!_markedRepaint) {
				_markedRepaint = true;
				uNodeThreadUtility.ExecuteAfterCondition(
					() => EditorWindow.focusedWindow == uNodeEditor.window, 
					() => {
						_markedRepaint = false;
						ReloadView();
					});
			}
		}

		void ReloadView() {
			contentMap.Clear();
			{//Namespace
				if(editorData.graphData != null || !(editorData.graph is IIndependentGraph)) {
					if(editorData.graphData != null) {
						namespaceButton.label.text = editorData.graphData.Namespace;
					}
					namespaceElement.ShowElement();
				} else {
					namespaceElement.HideElement();
				}
			}
			{//Classes
				for(int i = 0; i < classContainer.childCount; i++) {
					classContainer[i].RemoveFromHierarchy();
					i--;
				}
				var classes = editorData.graphs;
				if(classes == null) {
					classContainer.parent.HideElement();
				} else {
					classContainer.parent.ShowElement();
					if (editorData.graph is IIndependentGraph) {
						classAddButton.HideElement();
					} else {
						classAddButton.ShowElement();
					}
					if(showClasses) {
						for(int i=0;i< classes.Length;i++) {
							var root = classes[i];
							if(root != null) {
								string displayName = root.DisplayName;
								if(i == 0 && displayName.StartsWith("_", StringComparison.Ordinal)) {
									displayName = root.gameObject.name;
								}
								Texture icon = uNodeEditorUtility.GetTypeIcon(root);
								var content = new PanelElement(displayName, () => {
									if (editorData.graph != root) {
										uNodeEditor.Open(root);
									}
									graph.window.ChangeEditorSelection(editorData.graph);
								}) {
									name = "content",
									onStartDrag = () => {

									},
								};
								var current = root;
								content.RegisterCallback<MouseDownEvent>(evt => {
									var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
									if (evt.button == 0 && (evt.clickCount == 2 || evt.shiftKey)) {
										ActionPopupWindow.ShowWindow(Vector2.zero, () => {
											CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = root });
										}, 300, 300).ChangePosition(mPos);
									}
								});
								content.AddManipulator(new ContextualMenuManipulator(evt => {
									var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
									evt.menu.AppendAction("Inspect...", act => {
										ActionPopupWindow.ShowWindow(Vector2.zero, () => {
											CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = root });
										}, 300, 300).ChangePosition(mPos);
									});
									if(GraphUtility.IsTempGraphObject(current.gameObject)) {
										var prefab = uNodeEditorUtility.GetComponentSource(current, null);
										if(prefab != null) {
											var rType = ReflectionUtils.GetRuntimeType(prefab);
											evt.menu.AppendAction("Find All References", act => {
												GraphUtility.ShowMemberUsages(rType);
											});
										}
									}
									evt.menu.AppendSeparator("");
									// evt.menu.AppendAction("Export to template", act => {
									// 	Transform[] children = null;
									// 	if (current.RootObject != null) {
									// 		children = current.RootObject.GetComponentsInChildren<Transform>(true);
									// 	}
									// 	//List<Component> components = new List<Component>();
									// 	//components.Add(owner);
									// 	//var comps = owner.GetComponents<Component>();
									// 	//foreach(var c in comps) {
									// 	//	if(c is MonoBehaviour)
									// 	//		continue;
									// 	//	components.Add(c);
									// 	//}
									// 	var data = Serializer.Serializer.Serialize(current.gameObject, new Component[] { current, current.transform }, children);
									// 	string path = EditorUtility.SaveFilePanelInProject("Export to template",
									// 		current.gameObject.name + ".asset",
									// 		"asset",
									// 		"Please enter a file name to save the template to");
									// 	if (path.Length != 0) {
									// 		uNodeTemplate asset = ScriptableObject.CreateInstance(typeof(uNodeTemplate)) as uNodeTemplate;
									// 		asset.serializedData = data;
									// 		AssetDatabase.CreateAsset(asset, path);
									// 		AssetDatabase.SaveAssets();
									// 	}
									// });
									evt.menu.AppendAction("Duplicate", act => {
										string path = EditorUtility.SaveFilePanelInProject("Duplicate Graph", current.DisplayName, "prefab", "");
										if(!string.IsNullOrEmpty(path)) {
											var cls = UnityEngine.Object.Instantiate(current);
											var gameObject = new GameObject("Converted Graph");
											var result = gameObject.AddComponent(current.GetType()) as uNodeRoot;
											EditorUtility.CopySerialized(cls, result);
											result.RootObject = cls.RootObject;
											if(result.RootObject != null) {
												result.RootObject.transform.SetParent(gameObject.transform);
												AnalizerUtility.RetargetNodeOwner(cls, result, result.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
											}
											UnityEngine.Object.DestroyImmediate(cls.gameObject);
											result.Refresh();
											PrefabUtility.SaveAsPrefabAsset(result.gameObject, path);
											UnityEngine.Object.DestroyImmediate(result.gameObject);
											AssetDatabase.SaveAssets();
										}
									});
									if(editorData.graphData != null && !(current is IIndependentGraph)) {
										evt.menu.AppendAction("Remove", act => {
											if(current.RootObject != null) {
												Undo.DestroyObjectImmediate(current.RootObject);
											}
											Undo.DestroyObjectImmediate(current);
											ReloadView();
										});
									}
									var converters = GraphUtility.FindGraphConverters();
									for (int x = 0; x < converters.Count; x++) {
										var converter = converters[x];
										if (!converter.IsValid(current)) continue;
										evt.menu.AppendAction("Convert/" + converter.GetMenuName(current), act => {
											string path = EditorUtility.SaveFilePanelInProject("Convert Graph", current.gameObject.name, "prefab", "");
											if (!string.IsNullOrEmpty(path)) {
												var convertedGraph = converter.Convert(current);
												PrefabUtility.SaveAsPrefabAsset(convertedGraph.gameObject, path);
												UnityEngine.Object.DestroyImmediate(convertedGraph.gameObject);
												AssetDatabase.SaveAssets();
											}
										});
									}
									if (classes.Length > 1) {
										evt.menu.AppendAction("Remove", act => {
											NodeEditorUtility.RemoveObject(current.gameObject);
											for (int x = 0; x < classes.Length; x++) {
												if (classes[x] != null) {
													uNodeEditor.Open(classes[x]);
													graph.SelectionChanged();
												}
											}
											graph.Refresh();
										});
									}
									var system = GraphUtility.GetGraphSystem(root);
									if (system.supportConstructor) {
										evt.menu.AppendSeparator("");
										evt.menu.AppendAction("New Constructor", act => {
											CreateNewConstructor(current);
										});
									}
									if (root is INestedClassSystem) {
										evt.menu.AppendSeparator("");
										evt.menu.AppendAction("New Nested Types", null, DropdownMenuAction.AlwaysDisabled);
										evt.menu.AppendSeparator("");
										CreateNestedTypesMenu(evt.menu, root as INestedClassSystem);
									}
								}));
								content.ShowIcon(icon);
								classContainer.Add(content);
								contentMap[root] = content;
							}
						}
					}
				}
			}
			var graphSystem = GraphUtility.GetGraphSystem(editorData.graph);
			//Variables
			if(editorData.graph != null && (graphSystem.supportVariable || editorData.graph.Variables.Count > 0)) {
				variableContainer.parent.ShowElement();
				for(int i = 0; i < variableContainer.childCount; i++) {
					variableContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showVariables) {
					foreach(var variable in editorData.graph.Variables) {
						Texture icon = uNodeEditorUtility.GetTypeIcon(variable.type);
						var content = new PanelElement(variable.Name, () => {
							graph.window.ChangeEditorSelection(variable);
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", variable);
								DragAndDrop.SetGenericData("uNode-Target", editorData.graph);
							},
						};
						if(!variable.modifier.isPublic) {
							content.AddToClassList("private-modifier");
						}
						var current = variable;
						//Show Inpect on double click
						content.RegisterCallback<MouseDownEvent>(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							if (evt.button == 0 && (evt.clickCount == 2 || evt.altKey)) {
								uNodeGUIUtility.ShowRenameVariableWindow(Rect.zero, variable, editorData.graph.Variables, editorData.graph, null, uNodeEditor.AutoSaveCurrentGraph).ChangePosition(mPos);
							} else if(evt.button == 0 && evt.shiftKey) {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = variable });
								}, 300, 300).ChangePosition(mPos);
							}
						});
						content.AddManipulator(new ContextualMenuManipulator(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Rename", act => {
								uNodeGUIUtility.ShowRenameVariableWindow(Rect.zero, variable, editorData.graph.Variables, editorData.graph, null, uNodeEditor.AutoSaveCurrentGraph).ChangePosition(mPos);
							});
							evt.menu.AppendAction("Inspect...", act => {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = variable });
								}, 300, 300).ChangePosition(mPos);
							});
							evt.menu.AppendAction("Find All References", act => {
								GraphUtility.ShowVariableUsages(editorData.graph, current.Name);
							});
							evt.menu.AppendAction("Move To Local Variable", act => {
								RefactorUtility.MoveVariableToLocalVariable(current.Name, editorData.graph);
								uNodeGUIUtility.GUIChanged(editorData.graph);
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Move Up", act => {
								if (editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Up Variable: " + variable.Name);
								int index = 0;
								bool valid = false;
								foreach (VariableData var in editorData.graph.Variables) {
									if (var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if (valid) {
									if (index != 0) {
										editorData.graph.Variables.RemoveAt(index);
										editorData.graph.Variables.Insert(index - 1, current);
									}
								}
								ReloadView();
							});
							evt.menu.AppendAction("Move Down", act => {
								if (editorData.graph) {
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Down Variable: " + variable.Name);
								}
								int index = 0;
								bool valid = false;
								foreach (VariableData var in editorData.graph.Variables) {
									if (var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if (valid) {
									if (index + 1 != editorData.graph.Variables.Count) {
										editorData.graph.Variables.RemoveAt(index);
										editorData.graph.Variables.Insert(index + 1, current);
									}
								}
								ReloadView();
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Duplicate", act => {
								if (editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Duplicate Variable: " + variable.Name);
								VariableData v = current;
								string vName = uNodeEditorUtility.GetUniqueNameForGraph(v.Name, editorData.graph);
								int index = 0;
								bool valid = false;
								foreach (VariableData var in editorData.graph.Variables) {
									if (var == v) {
										valid = true;
										break;
									}
									index++;
								}
								if (valid) {
									if (index + 1 != editorData.graph.Variables.Count) {
										editorData.graph.Variables.Insert(index + 1, new VariableData(v) { Name = vName });
									} else {
										editorData.graph.Variables.Add(new VariableData(v) { Name = vName });
									}
								}
								ReloadView();
							});
							evt.menu.AppendAction("Remove", act => {
								if (editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Variable: " + variable.Name);
								editorData.graph.Variables.Remove(current);
								ReloadView();
							});
						}));
						content.ShowIcon(icon);
						{//Remove button
							content.Add(new VisualElement() { name = "spacer" });
							var btn = new ClickableElement("-") {
								name = "content-button-remove",
							};
							btn.onClick = () => {
								if (editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Variable: " + variable.Name);
								editorData.graph.Variables.Remove(current);
								ReloadView();
							};
							content.Add(btn);
						}
						variableContainer.Add(content);
						contentMap[variable] = content;
					}
				}
			} else {
				variableContainer.parent.HideElement();
			}
			//Properties
			if(editorData.graph != null && (graphSystem.supportProperty || editorData.graph.Properties.Count > 0)) {
				propertyContainer.parent.ShowElement();
				for(int i = 0; i < propertyContainer.childCount; i++) {
					propertyContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showProperties) {
					foreach(var property in editorData.graph.Properties) {
						Texture icon = uNodeEditorUtility.GetTypeIcon(property.type);
						var body = new VisualElement() { name = "property-content" };
						var content = new PanelElement(property.Name, () => {
							graph.window.ChangeEditorSelection(property);
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", property);
								DragAndDrop.SetGenericData("uNode-Target", editorData.graph);
							},
						};
						if(!property.modifier.isPublic) {
							content.AddToClassList("private-modifier");
						}
						var current = property;
						
						content.RegisterCallback<MouseDownEvent>(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							if (evt.button == 0 && (evt.clickCount == 2 || evt.altKey)) {
								ActionPopupWindow.ShowWindow(Vector2.zero, new object[] { current.Name, current },
									delegate (ref object obj) {
										object[] o = obj as object[];
										o[0] = EditorGUILayout.TextField("Property Name", o[0] as string);
									}, null, delegate (ref object obj) {
										if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
											object[] o = obj as object[];
											RefactorUtility.RefactorProperty(o[1] as uNodeProperty, o[0] as string);
											ActionPopupWindow.CloseLast();
										}
									}).ChangePosition(mPos).headerName = "Rename Property";
							} else if(evt.button == 0 && evt.shiftKey) {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							}
						});
						content.AddManipulator(new ContextualMenuManipulator(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Rename", act => {
								ActionPopupWindow.ShowWindow(Vector2.zero, new object[] { current.Name, current },
									delegate (ref object obj) {
										object[] o = obj as object[];
										o[0] = EditorGUILayout.TextField("Property Name", o[0] as string);
									}, null, delegate (ref object obj) {
										if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
											object[] o = obj as object[];
											RefactorUtility.RefactorProperty(o[1] as uNodeProperty, o[0] as string);
											ActionPopupWindow.CloseLast();
										}
									}).ChangePosition(mPos).headerName = "Rename Property";
							});
							evt.menu.AppendAction("Inspect...", act => {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							});
							evt.menu.AppendAction("Find All References", act => {
								GraphUtility.ShowPropertyUsages(editorData.graph, current.Name);
							});
							evt.menu.AppendSeparator("");
							if(current.getRoot == null && current.setRoot == null) {
								evt.menu.AppendAction("Add Getter and Setter", act => {
									NodeEditorUtility.AddNewObject(editorData.graph, "Setter", current.transform,
										delegate (GameObject go) {
											var func = go.AddComponent<uNodeFunction>();
											func.parameters = new ParameterData[1] { new ParameterData("value", current.ReturnType()) };
											uNodeProperty p = go.transform.parent.GetComponent<uNodeProperty>();
											p.setRoot = func;
											NodeEditorUtility.AddNewObject<Nodes.NodeAction>(editorData.graph, "Entry", func.transform, (node) => {
												func.startNode = node;
											});
										});
									NodeEditorUtility.AddNewObject(editorData.graph, "Getter", current.transform,
										delegate (GameObject go) {
											var func = go.AddComponent<uNodeFunction>();
											func.returnType = new MemberData(current.ReturnType(), MemberData.TargetType.Type);
											uNodeProperty p = go.transform.parent.GetComponent<uNodeProperty>();
											p.getRoot = func;
											NodeEditorUtility.AddNewObject<Nodes.NodeAction>(editorData.graph, "Entry", func.transform, (node) => {
												func.startNode = node;
												NodeEditorUtility.AddNewObject<NodeReturn>(editorData.graph, "return", func.transform, (rRode) => {
													node.onFinished = MemberData.FlowInput(rRode);
													if(ReflectionUtils.CanCreateInstance(current.ReturnType())) {
														rRode.returnValue = MemberData.CreateFromValue(ReflectionUtils.CreateInstance(current.ReturnType()));
													}
													rRode.editorRect.y += 100;
												});
											});
										});
									graph.Refresh();
								});
							}
							if(current.setRoot == null) {
								evt.menu.AppendAction("Add Setter", act => {
									NodeEditorUtility.AddNewObject(editorData.graph, "Setter", current.transform,
										delegate (GameObject go) {
											var func = go.AddComponent<uNodeFunction>();
											func.parameters = new ParameterData[1] { new ParameterData("value", current.ReturnType()) };
											func.returnType = MemberData.CreateFromType(typeof(void));
											uNodeProperty p = go.transform.parent.GetComponent<uNodeProperty>();
											p.setRoot = func;
											NodeEditorUtility.AddNewObject<Nodes.NodeAction>(editorData.graph, "Entry", func.transform, (node) => {
												func.startNode = node;
											});
										});
									graph.Refresh();
								});
							}
							if(current.getRoot == null) {
								evt.menu.AppendAction("Add Getter", act => {
									NodeEditorUtility.AddNewObject(editorData.graph, "Getter", current.transform,
										delegate (GameObject go) {
											var func = go.AddComponent<uNodeFunction>();
											func.returnType = new MemberData(current.ReturnType(), MemberData.TargetType.Type);
											uNodeProperty p = go.transform.parent.GetComponent<uNodeProperty>();
											p.getRoot = func;
											NodeEditorUtility.AddNewObject<Nodes.NodeAction>(editorData.graph, "Entry", func.transform, (node) => {
												func.startNode = node;
												NodeEditorUtility.AddNewObject<NodeReturn>(editorData.graph, "return", func.transform, (rRode) => {
													node.onFinished = MemberData.FlowInput(rRode);
													if(ReflectionUtils.CanCreateInstance(current.ReturnType())) {
														rRode.returnValue = MemberData.CreateFromValue(ReflectionUtils.CreateInstance(current.ReturnType()));
													}
													rRode.editorRect.y += 100;
												});
											});
										});
									graph.Refresh();
								});
							}
							if(current.getRoot && current.setRoot) {
								evt.menu.AppendAction("Remove Getter and Setter", act => {
									NodeEditorUtility.RemoveObject(editorData, current.setRoot.gameObject, current.getRoot.gameObject);
									graph.Refresh();
								});
							}
							if(current.setRoot) {
								evt.menu.AppendAction("Remove Setter", act => {
									NodeEditorUtility.RemoveObject(editorData, current.setRoot.gameObject);
									graph.Refresh();
								});
							}
							if(current.getRoot) {
								evt.menu.AppendAction("Remove Getter", act => {
									NodeEditorUtility.RemoveObject(editorData, current.getRoot.gameObject);
									graph.Refresh();
								});
							}
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Move Up", act => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Up Property: " + current.Name);
								int index = 0;
								bool valid = false;
								foreach(uNodeProperty var in editorData.graph.Properties) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index != 0) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Properties[index - 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendAction("Move Down", act => {
								if(editorData.graph) {
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Down Property: " + current.Name);
								}
								int index = 0;
								bool valid = false;
								foreach(uNodeProperty var in editorData.graph.Properties) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index + 1 != editorData.graph.Properties.Count) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Properties[index + 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Duplicate", act => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterFullHierarchyUndo(editorData.graph, "Duplicate Property: " + current.Name);
								var v = current;
								var obj = GameObject.Instantiate(v, v.transform.parent);
								obj.Name = uNodeEditorUtility.GetUniqueNameForGraph(v.Name, editorData.graph);
								editorData.graph.Refresh();
								ReloadView();
							});
							evt.menu.AppendAction("Remove", act => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Property: " + current.Name);
								//uNodeUtils.RemoveList(ref properties, obj as uNodeProperty);
								NodeEditorUtility.RemoveComponent(editorData, current);
								graph.Refresh();
							});
						}));
						content.ShowIcon(icon);
						{//Remove button
							content.Add(new VisualElement() { name = "spacer" });
							var btn = new ClickableElement("-") {
								name = "content-button-remove",
							};
							btn.onClick = () => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Property: " + current.Name);
								NodeEditorUtility.RemoveComponent(editorData, current);
								graph.Refresh();
							};
							content.Add(btn);
						}
						contentMap[property] = content;
						body.Add(content);
						if(!property.AutoProperty) {
							if(property.getRoot != null) {
								var root = new ClickableElement("Getter", () => {
									editorData.selectedRoot = current.getRoot;
									graph.Refresh();
									graph.UpdatePosition();
								}) { name = "content" };
								root.AddToClassList("property-accessor");
								body.Add(root);
								contentMap[property.getRoot] = root;
							}
							if(property.setRoot != null) {
								var root = new ClickableElement("Setter", () => {
									editorData.selectedRoot = current.setRoot;
									graph.Refresh();
									graph.UpdatePosition();
								}) { name = "content" };
								root.AddToClassList("property-accessor");
								body.Add(root);
								contentMap[property.setRoot] = root;
							}
						}
						propertyContainer.Add(body);
					}
				}
			} else {
				propertyContainer.parent.HideElement();
			}
			//Functions
			if(editorData.graph != null && (graphSystem.supportFunction || editorData.graph.Functions.Count > 0)) {
				functionContainer.parent.ShowElement();
				for(int i = 0; i < functionContainer.childCount; i++) {
					functionContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showFunctions) {
					if(editorData.graph is IMacroGraph) {
						functionAddButton.HideElement();
					} else {
						functionAddButton.ShowElement();
					}
					if(editorData.graph is IMacroGraph || editorData.graph is IStateGraph state && state.canCreateGraph) {
						Texture icon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.StateIcon));
						var content = new PanelElement(editorData.graph is IStateGraph ? "[STATE GRAPH]" : "[MACRO]", () => {
							editorData.selected = null;
							editorData.selectedRoot = null;
							graph.SelectionChanged();
							graph.Refresh();
							graph.UpdatePosition();
						}) { name = "content" };
						content.ShowIcon(icon);
						if(null == editorData.selectedRoot) {
							content.AddToClassList("active");
						}
						functionContainer.Add(content);
						contentMap[editorData.graph.GetInstanceID() + "[MAINGRAPH]"] = content;
					}
					foreach(var function in editorData.graph.Functions) {
						Texture icon = uNodeEditorUtility.GetTypeIcon(function.returnType);
						var content = new PanelElement(function.Name, () => {
							graph.window.ChangeEditorSelection(function);
							if (editorData.selectedRoot != function || editorData.selectedGroup != null) {
								editorData.selectedRoot = function;
								graph.SelectionChanged();
								graph.UpdatePosition();
								graph.Refresh();
							}
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", function);
								DragAndDrop.SetGenericData("uNode-Target", editorData.graph);
							},
						};
						if(!function.modifiers.isPublic) {
							content.AddToClassList("private-modifier");
						}
						var current = function;
						content.RegisterCallback<MouseDownEvent>(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							if (evt.button == 0 && (evt.clickCount == 2 || evt.altKey)) {
								//Rename Function
								ActionPopupWindow.ShowWindow(Vector2.zero,
									new object[] { current.Name, current },
									delegate (ref object obj) {
										object[] o = obj as object[];
										o[0] = EditorGUILayout.TextField("Function Name", o[0] as string);
									}, null, delegate (ref object obj) {
										if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
											object[] o = obj as object[];
											RefactorUtility.RefactorFunction(o[1] as uNodeFunction, o[0] as string);
											ActionPopupWindow.CloseLast();
										}
									}).ChangePosition(mPos).headerName = "Rename Function";
							} else if(evt.button == 0 && evt.shiftKey) {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							}
						});
						content.AddManipulator(new ContextualMenuManipulator((evt) => {
							var mPos = UIElementUtility.GetScreenMousePosition(evt.currentTarget as VisualElement, evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Rename", (act) => {
								ActionPopupWindow.ShowWindow(Vector2.zero,
									new object[] { current.Name, current },
									delegate (ref object obj) {
										object[] o = obj as object[];
										o[0] = EditorGUILayout.TextField("Function Name", o[0] as string);
									}, null, delegate (ref object obj) {
										if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
											object[] o = obj as object[];
											RefactorUtility.RefactorFunction(o[1] as uNodeFunction, o[0] as string);
											ActionPopupWindow.CloseLast();
										}
									}).ChangePosition(mPos).headerName = "Rename Function";
							});
							evt.menu.AppendAction("Inspect...", (act) => {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							});
							evt.menu.AppendAction("Find All References", act => {
								GraphUtility.ShowFunctionUsages(editorData.graph, current.Name);
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Move Up", (act) => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Up Function: " + current.Name);
								int index = 0;
								bool valid = false;
								foreach(uNodeFunction var in editorData.graph.Functions) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index != 0) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Functions[index - 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendAction("Move Down", (act) => {
								if(editorData.graph) {
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Down Function: " + current.Name);
								}
								int index = 0;
								bool valid = false;
								foreach(uNodeFunction var in editorData.graph.Functions) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index + 1 != editorData.graph.Functions.Count) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Functions[index + 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Duplicate", act => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterFullHierarchyUndo(editorData.graph, "Duplicate Function: " + current.Name);
								var v = current;
								var obj = GameObject.Instantiate(v, v.transform.parent);
								obj.Name = uNodeEditorUtility.GetUniqueNameForGraph(v.Name, editorData.graph);
								editorData.graph.Refresh();
								ReloadView();
							});
							evt.menu.AppendAction("Remove", (act) => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Function: " + current.Name);
								NodeEditorUtility.RemoveNodeRoot(editorData, current);
								graph.Refresh();
							});
						}));
						content.ShowIcon(icon);
						{//Remove button
							content.Add(new VisualElement() { name = "spacer" });
							var btn = new ClickableElement("-") {
								name = "content-button-remove",
							};
							btn.onClick = () => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Function: " + current.Name);
								NodeEditorUtility.RemoveNodeRoot(editorData, current);
								graph.Refresh();
							};
							content.Add(btn);
						}
						functionContainer.Add(content);
						contentMap[function] = content;
					}
				}
			} else {
				functionContainer.parent.HideElement();
			}
			//Events
			if(editorData.graph is IStateGraph stateGraph && stateGraph.canCreateGraph) {
				eventContainer.parent.ShowElement();
				for(int i = 0; i < eventContainer.childCount; i++) {
					eventContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showEvents && stateGraph.eventNodes?.Count > 0 && editorData.currentCanvas == editorData.graph) {
					foreach(var node in stateGraph.eventNodes) {
						if(node == null || node.gameObject == null) continue;
						Texture icon = uNodeEditorUtility.GetTypeIcon(node.GetNodeIcon());
						string title = node.GetNodeName();
						var content = new PanelElement(title, () => {
							uNodeEditor.HighlightNode(node);
						}) {
							name = "content",
						};
						var current = node;
						content.RegisterCallback<MouseDownEvent>(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							if (evt.button == 0 && (evt.clickCount == 2 || evt.shiftKey)) {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							}
						});
						content.AddManipulator(new ContextualMenuManipulator((evt) => {
							var mPos = UIElementUtility.GetScreenMousePosition(evt.currentTarget as VisualElement, evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Inspect...", (act) => {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Move Up", (act) => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Up Function: " + current.GetNodeName());
								int index = 0;
								bool valid = false;
								foreach(var var in stateGraph.eventNodes) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index != 0) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Functions[index - 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendAction("Move Down", (act) => {
								if(editorData.graph) {
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Down Function: " + current.GetNodeName());
								}
								int index = 0;
								bool valid = false;
								foreach(var var in stateGraph.eventNodes) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index + 1 != editorData.graph.Functions.Count) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Functions[index + 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Remove", (act) => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Node: " + current.GetNodeName());
								NodeEditorUtility.RemoveNode(editorData, current);
								graph.Refresh();
							});
						}));
						content.ShowIcon(icon);
						{//Remove button
							content.Add(new VisualElement() { name = "spacer" });
							var btn = new ClickableElement("-") {
								name = "content-button-remove",
							};
							btn.onClick = () => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Node: " + current.GetNodeName());
								NodeEditorUtility.RemoveNode(editorData, current);
								graph.Refresh();
							};
							content.Add(btn);
						}
						eventContainer.Add(content);
						contentMap[node] = content;
					}
				} else {
					eventContainer.parent.HideElement();
				}
			} else {
				eventContainer.parent.HideElement();
			}
			//Constructor
			if(editorData.graph != null && (graphSystem.supportConstructor && editorData.graph.Constuctors.Count > 0)) {
				constructorContainer.parent.ShowElement();
				for(int i = 0; i < constructorContainer.childCount; i++) {
					constructorContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showConstructors) {
					foreach(var ctor in editorData.graph.Constuctors) {
						var content = new PanelElement("ctor(" + string.Join(", ", ctor.parameters.Select(p => p.type.DisplayName(false, false)).ToArray()) + ")", () => {
							graph.window.ChangeEditorSelection(ctor);
							if (editorData.selectedRoot != ctor || editorData.selectedGroup != null) {
								editorData.selectedRoot = ctor;
								graph.SelectionChanged();
								graph.UpdatePosition();
								graph.Refresh();
							}
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", ctor);
								DragAndDrop.SetGenericData("uNode-Target", editorData.graph);
							},
						};
						var current = ctor;
						content.RegisterCallback<MouseDownEvent>(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							if (evt.button == 0 && (evt.clickCount == 2 || evt.shiftKey)) {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							}
						});
						content.AddManipulator(new ContextualMenuManipulator((evt) => {
							var mPos = UIElementUtility.GetScreenMousePosition(evt.currentTarget as VisualElement, evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Inspect...", (act) => {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Move Up", (act) => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Up Constructor: " + current.Name);
								int index = 0;
								bool valid = false;
								foreach(uNodeConstuctor var in editorData.graph.Constuctors) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index != 0) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Constuctors[index - 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendAction("Move Down", (act) => {
								if(editorData.graph) {
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Move Down Constructor: " + current.Name);
								}
								int index = 0;
								bool valid = false;
								foreach(uNodeConstuctor var in editorData.graph.Constuctors) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index + 1 != editorData.graph.Constuctors.Count) {
										NodeEditorUtility.SetSiblingIndex(current.transform, editorData.graph.Constuctors[index + 1].transform.GetSiblingIndex());
									}
								}
								graph.Refresh();
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Duplicate", act => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterFullHierarchyUndo(editorData.graph, "Duplicate Constructor: " + current.Name);
								var v = current;
								var obj = GameObject.Instantiate(v, v.transform.parent);
								obj.Name = uNodeEditorUtility.GetUniqueNameForGraph(v.Name, editorData.graph);
								editorData.graph.Refresh();
								ReloadView();
							});
							evt.menu.AppendAction("Remove", (act) => {
								if(editorData.graph)
									uNodeEditorUtility.RegisterUndo(editorData.graph, "Remove Constructor: " + current.Name);
								NodeEditorUtility.RemoveNodeRoot(editorData, current);
								graph.Refresh();
							});
						}));
						constructorContainer.Add(content);
						contentMap[ctor] = content;
					}
				}
			} else {
				constructorContainer.parent.HideElement();
			}
			//Local Variable
			if(editorData.graph != null && editorData.selectedRoot != null && editorData.selectedRoot.LocalVariables != null) {
				localContainer.parent.ShowElement();
				for(int i = 0; i < localContainer.childCount; i++) {
					localContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showLocal) {
					foreach(var variable in editorData.selectedRoot.LocalVariables) {
						Texture icon = uNodeEditorUtility.GetTypeIcon(variable.type);
						var content = new PanelElement(variable.Name, () => {
							graph.window.ChangeEditorSelection(variable);
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", variable);
								DragAndDrop.SetGenericData("uNode-Target", editorData.selectedRoot);
							},
						};
						var current = variable;
						content.RegisterCallback<MouseDownEvent>(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							if(evt.button == 0 && (evt.clickCount == 2 || evt.shiftKey)) {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = current });
								}, 300, 300).ChangePosition(mPos);
							}
						});
						content.AddManipulator(new ContextualMenuManipulator(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Rename", act => {
								uNodeGUIUtility.ShowRenameVariableWindow(Rect.zero,
									variable,
									editorData.selectedRoot.LocalVariables,
									editorData.selectedRoot,
									null,
									uNodeEditor.AutoSaveCurrentGraph).ChangePosition(mPos);
							});
							evt.menu.AppendAction("Inspect...", act => {
								ActionPopupWindow.ShowWindow(Vector2.zero, () => {
									CustomInspector.ShowInspector(new GraphEditorData(editorData) { selected = variable });
								}, 300, 300).ChangePosition(mPos);
							});
							evt.menu.AppendAction("Find All References", act => {
								GraphUtility.ShowLocalVariableUsages(editorData.selectedRoot, current.Name);
							});
							evt.menu.AppendAction("Move To Class Variable" , act => {
								RefactorUtility.MoveLocalVariableToVariable(current.Name, editorData.selectedRoot, editorData.graph);
								uNodeGUIUtility.GUIChanged(editorData.graph);
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Move Up", act => {
								uNodeEditorUtility.RegisterUndo(editorData.selectedRoot, "Move Up Variable: " + variable.Name);
								int index = 0;
								bool valid = false;
								foreach(VariableData var in editorData.selectedRoot.LocalVariables) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index != 0) {
										editorData.selectedRoot.LocalVariables.RemoveAt(index);
										editorData.selectedRoot.LocalVariables.Insert(index - 1, current);
									}
								}
								ReloadView();
							});
							evt.menu.AppendAction("Move Down", act => {
								uNodeEditorUtility.RegisterUndo(editorData.selectedRoot, "Move Down Variable: " + variable.Name);
								int index = 0;
								bool valid = false;
								foreach(VariableData var in editorData.selectedRoot.LocalVariables) {
									if(var == current) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index + 1 != editorData.selectedRoot.LocalVariables.Count) {
										editorData.selectedRoot.LocalVariables.RemoveAt(index);
										editorData.selectedRoot.LocalVariables.Insert(index + 1, current);
									}
								}
								ReloadView();
							});
							evt.menu.AppendSeparator("");
							evt.menu.AppendAction("Duplicate", act => {
								uNodeEditorUtility.RegisterUndo(editorData.selectedRoot, "Duplicate Variable: " + variable.Name);
								VariableData v = current;
								string vName = v.Name;
								int index = 0;
								bool hasSameName = false;
								do {
									if(hasSameName) {
										vName += index;
									}
									hasSameName = false;
									foreach(VariableData var in editorData.selectedRoot.LocalVariables) {
										if(var.Name.Equals(vName)) {
											hasSameName = true;
											break;
										}
									}
									foreach(var var in editorData.selectedRoot.LocalVariables) {
										if(var.Name.Equals(vName)) {
											hasSameName = true;
											break;
										}
									}
								} while(hasSameName);
								index = 0;
								bool valid = false;
								foreach(VariableData var in editorData.selectedRoot.LocalVariables) {
									if(var == v) {
										valid = true;
										break;
									}
									index++;
								}
								if(valid) {
									if(index + 1 != editorData.selectedRoot.LocalVariables.Count) {
										editorData.selectedRoot.LocalVariables.Insert(index + 1, new VariableData(v) { Name = vName });
									} else {
										editorData.selectedRoot.LocalVariables.Add(new VariableData(v) { Name = vName });
									}
								}
								ReloadView();
							});
							evt.menu.AppendAction("Remove", act => {
								uNodeEditorUtility.RegisterUndo(editorData.selectedRoot, "Remove Variable: " + variable.Name);
								editorData.selectedRoot.LocalVariables.Remove(current);
								ReloadView();
							});
						}));
						content.ShowIcon(icon);
						{//Remove button
							content.Add(new VisualElement() { name = "spacer" });
							var btn = new ClickableElement("-") {
								name = "content-button-remove",
							};
							btn.onClick = () => {
								uNodeEditorUtility.RegisterUndo(editorData.selectedRoot, "Remove Variable: " + variable.Name);
								editorData.selectedRoot.LocalVariables.Remove(current);
								ReloadView();
							};
							content.Add(btn);
						}
						localContainer.Add(content);
						contentMap[variable] = content;
					}
				}
			} else {
				localContainer.parent.HideElement();
			}
			//Nested Types
			if(editorData.graph && editorData.graph is INestedClassSystem) {
				nestedContainer.parent.ShowElement();
				for(int i = 0; i < nestedContainer.childCount; i++) {
					nestedContainer[i].RemoveFromHierarchy();
					i--;
				}
				var nestedSystem = editorData.graph as INestedClassSystem;
				var nestedTypes = nestedSystem.NestedClass;
				if(showInterfaces && nestedTypes) {
					var types = nestedTypes?.GetComponents<uNodeRoot>();
					if(types.Length > 0) {
						foreach(var root in types) {
							var content = new PanelElement(root.DisplayName, () => {
								uNodeEditor.Open(root);
							}) {
								name = "content",
								onStartDrag = () => {
									DragAndDrop.SetGenericData("uNode", root);
									DragAndDrop.SetGenericData("uNode-Target", root);
								},
							};
							content.ShowIcon(uNodeEditorUtility.GetTypeIcon(root));
							nestedContainer.Add(content);
							contentMap[root] = content;
						}
					}
					if(nestedTypes.enums?.Length > 0) {
						foreach(var data in nestedTypes.enums) {
							var content = new PanelElement(data.name, () => {
								uNodeEditor.Open(nestedTypes);
							}) {
								name = "content",
							};
							content.ShowIcon(uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.EnumIcon)));
							nestedContainer.Add(content);
							contentMap[data] = content;
						}
					}
					if(nestedTypes.interfaces?.Length > 0) {
						foreach(var data in nestedTypes.interfaces) {
							var content = new PanelElement(data.name, () => {
								uNodeEditor.Open(nestedTypes);
							}) {
								name = "content",
							};
							content.ShowIcon(uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.InterfaceIcon)));
							nestedContainer.Add(content);
							contentMap[data] = content;
						}
					}
					if(nestedTypes.delegates?.Length > 0) {
						foreach(var data in nestedTypes.delegates) {
							var content = new PanelElement(data.name, () => {
								uNodeEditor.Open(nestedTypes);
							}) {
								name = "content",
							};
							content.ShowIcon(uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.DelegateIcon)));
							nestedContainer.Add(content);
							contentMap[data] = content;
						}
					}
				} else {
					nestedContainer.parent.HideElement();
				}
			} else {
				nestedContainer.parent.HideElement();
			}
			//Interfaces
			if(editorData.graphData != null && editorData.graphData.interfaces.Length > 0) {
				interfaceContainer.parent.ShowElement();
				for(int i = 0; i < interfaceContainer.childCount; i++) {
					interfaceContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showInterfaces) {
					foreach(var iface in editorData.graphData.interfaces) {
						var content = new PanelElement(iface.name, () => {
							graph.window.ChangeEditorSelection(iface);
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", iface);
								DragAndDrop.SetGenericData("uNode-Target", editorData.graphData);
							},
						};
						var current = iface;
						content.AddManipulator(new ContextualMenuManipulator(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Rename", act => {
								ShowRenameAction(mPos, current.name, (string str) => {
									uNodeEditorUtility.RegisterUndo(editorData.graphData, "Rename Interface");
									current.name = str;
								});
							});
							evt.menu.AppendAction("Remove", act => {
								uNodeEditorUtility.RegisterUndo(editorData.graphData, "Remove Interface");
								ArrayUtility.Remove(ref editorData.graphData.interfaces, current);
								ReloadView();
							});
						}));
						interfaceContainer.Add(content);
						contentMap[iface] = content;
					}
				}
			} else {
				interfaceContainer.parent.HideElement();
			}
			//Enums
			if(editorData.graphData != null && editorData.graphData.enums.Length > 0) {
				enumContainer.parent.ShowElement();
				for(int i = 0; i < enumContainer.childCount; i++) {
					enumContainer[i].RemoveFromHierarchy();
					i--;
				}
				if(showEnums) {
					foreach(var e in editorData.graphData.enums) {
						var content = new PanelElement(e.name, () => {
							graph.window.ChangeEditorSelection(e);
						}) {
							name = "content",
							onStartDrag = () => {
								DragAndDrop.SetGenericData("uNode", e);
								DragAndDrop.SetGenericData("uNode-Target", editorData.graphData);
							},
						};
						var current = e;
						content.AddManipulator(new ContextualMenuManipulator(evt => {
							var mPos = (evt.currentTarget as VisualElement).GetScreenMousePosition(evt.localMousePosition, graph.window);
							evt.menu.AppendAction("Rename", act => {
								ShowRenameAction(mPos, current.name, (string str) => {
									uNodeEditorUtility.RegisterUndo(editorData.graphData, "Rename Enum");
									current.name = str;
								});
							});
							evt.menu.AppendAction("Remove", act => {
								uNodeEditorUtility.RegisterUndo(editorData.graphData, "Remove Enum");
								ArrayUtility.Remove(ref editorData.graphData.enums, current);
								ReloadView();
							});
						}));
						enumContainer.Add(content);
						contentMap[e] = content;
					}
				}
			} else {
				enumContainer.parent.HideElement();
			}
			UpdateView();
		}

		#region Other
		private void CreateNestedTypesMenu(DropdownMenu menu, INestedClassSystem nestedSystem) {
			var nestedTypes = nestedSystem.NestedClass;
			menu.AppendAction("Class", act => {
				if(nestedTypes) {
					nestedTypes.gameObject.AddComponent<uNodeClass>();
				} else {
					string path = EditorUtility.SaveFilePanelInProject("Create NestedType", "NestedType", "prefab", "");
					if(!string.IsNullOrEmpty(path)) {
						GameObject go = new GameObject();
						go.name = path.Split('/').Last().Split('.')[0];
						go.AddComponent<uNodeData>();
						go.AddComponent<uNodeClass>();
						GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
						uNodeData data = prefab.GetComponent<uNodeData>();
						nestedSystem.NestedClass = data;
						UnityEngine.Object.DestroyImmediate(go);
						AssetDatabase.SaveAssets();
					}
				}
			});
			menu.AppendAction("Struct", act => {
				if(nestedTypes) {
					nestedTypes.gameObject.AddComponent<uNodeStruct>();
				} else {
					string path = EditorUtility.SaveFilePanelInProject("Create NestedType", "NestedType", "prefab", "");
					if(!string.IsNullOrEmpty(path)) {
						GameObject go = new GameObject();
						go.name = path.Split('/').Last().Split('.')[0];
						go.AddComponent<uNodeData>();
						go.AddComponent<uNodeStruct>();
						GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
						uNodeData data = prefab.GetComponent<uNodeData>();
						nestedSystem.NestedClass = data;
						UnityEngine.Object.DestroyImmediate(go);
						AssetDatabase.SaveAssets();
					}
				}
			});
			menu.AppendAction("Interfaces", act => {
				if(nestedTypes) {
					var interfaces = new List<InterfaceData>(nestedTypes.interfaces);
					interfaces.Add(new InterfaceData());
					nestedTypes.interfaces = interfaces.ToArray();
				} else {
					string path = EditorUtility.SaveFilePanelInProject("Create NestedType", "NestedType", "prefab", "");
					if(!string.IsNullOrEmpty(path)) {
						GameObject go = new GameObject();
						go.name = path.Split('/').Last().Split('.')[0];
						var tmpData = go.AddComponent<uNodeData>();
						tmpData.interfaces = new [] { new InterfaceData() { name = "New_Iface" } };
						GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
						uNodeData data = prefab.GetComponent<uNodeData>();
						nestedSystem.NestedClass = data;
						UnityEngine.Object.DestroyImmediate(go);
						AssetDatabase.SaveAssets();
					}
				}
			});
			menu.AppendAction("Enums", act => {
				if(nestedTypes) {
					var interfaces = new List<InterfaceData>(nestedTypes.interfaces);
					interfaces.Add(new InterfaceData());
					nestedTypes.interfaces = interfaces.ToArray();
				} else {
					string path = EditorUtility.SaveFilePanelInProject("Create NestedType", "NestedType", "prefab", "");
					if(!string.IsNullOrEmpty(path)) {
						GameObject go = new GameObject();
						go.name = path.Split('/').Last().Split('.')[0];
						var tmpData = go.AddComponent<uNodeData>();
						tmpData.enums = new[] { new EnumData() { name = "New_Enum" } };
						GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
						uNodeData data = prefab.GetComponent<uNodeData>();
						nestedSystem.NestedClass = data;
						UnityEngine.Object.DestroyImmediate(go);
						AssetDatabase.SaveAssets();
					}
				}
			});
		}

		private void ShowTypeMenu(Vector2 position, Action<Type> onClick) {
			var customItmes = ItemSelector.MakeCustomTypeItems(new Type[] {
				typeof(string),
				typeof(float),
				typeof(bool),
				typeof(int),
				typeof(Vector2),
				typeof(Vector3),
				typeof(Transform),
				typeof(GameObject),
				typeof(uNodeRuntime),
				typeof(uNodeSpawner),
				typeof(RuntimeComponent),
				typeof(BaseRuntimeAsset),
				typeof(List<>),
			}, "General");
			var window = ItemSelector.ShowWindow( 
				editorData.graph as UnityEngine.Object ?? editorData.graphData, 
				new FilterAttribute() { OnlyGetType = true, UnityReference = false }, (m) => {
					onClick(m.Get<Type>());
				}, 
				true, 
				customItmes).ChangePosition(position);
			window.displayNoneOption = false;
			window.displayGeneralType = false;
		}

		private void OnNamespaceClicked(Vector2 position, uNodeData data) {
			ActionPopupWindow.ShowWindow(Vector2.zero, data.Namespace,
				delegate (ref object obj) {
					obj = EditorGUILayout.TextField("Namespace", obj as string);
				}, null, delegate (ref object obj) {
					if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
						Undo.RegisterCompleteObjectUndo(data, "Rename namespace");
						data.Namespace = obj as string;
						ActionPopupWindow.CloseLast();
						ReloadView();
					}
				}).ChangePosition(position).headerName = "Rename Namespace";
		}

		private void CreateNewConstructor(uNodeRoot root) {
			string fName = "NewConstructor";
			if(root.Constuctors != null && root.Constuctors.Count > 0) {
				int index = 0;
				while(true) {
					index++;
					bool found = false;
					foreach(var f in root.Constuctors) {
						if(f != null && f.Name.Equals(fName)) {
							found = true;
							break;
						}
					}
					if(found) {
						fName = "NewConstructor" + index;
					} else {
						break;
					}
				}
			}
			int num = 0;
			if(root.Constuctors != null && root.Constuctors.Count > 0) {
				while(true) {
					bool same = false;
					foreach(var f in root.Constuctors) {
						if(f != null && f.parameters.Length == num) {
							num++;
							same = true;
							break;
						}
					}
					if(!same)
						break;
				}
			}
			ParameterData[] parameters = new ParameterData[num];
			for(int i = 0; i < parameters.Length; i++) {
				parameters[i] = new ParameterData("p" + (i + 1), typeof(object));
			}
			NodeEditorUtility.AddNewConstructor(root, fName, delegate (uNodeConstuctor ctor) {
				ctor.parameters = parameters;
			});
		}

		private static void ShowRenameAction(Vector2 position, string startName, Action<string> onRename) {
			ActionPopupWindow.ShowWindow(Vector2.zero, new object[] { startName },
				delegate (ref object obj) {
					object[] o = obj as object[];
					o[0] = EditorGUILayout.TextField("Name", o[0] as string);
				}, null, delegate (ref object obj) {
					if(GUILayout.Button("Apply") || Event.current.keyCode == KeyCode.Return) {
						object[] o = obj as object[];
						//o[0] = CSharpGenerator.RemoveIncorrectName(o[0] as string);
						onRename(o[0] as string);
						ActionPopupWindow.CloseLast();
					}
				}).ChangePosition(position).headerName = "Rename";
		}
		#endregion
	}

	internal class PanelElement : ClickableElement, IDragableElement {
		public PanelElement(string text) : base(text) {
			Init();
		}

		public PanelElement(string text, Action onClick) : base(text, onClick) {
			Init();
		}

		void Init() {
			this.RemoveManipulator(clickable);
			this.AddManipulator(new LeftMouseClickable(evt => {
				if(onClick != null && !evt.shiftKey) {
					onClick();
				}
			}) { stopPropagationOnClick = false });
		}

		public Action onStartDrag;

		public void StartDrag() {
			if(onStartDrag != null) {
				onStartDrag();
			}
		}
	}
}