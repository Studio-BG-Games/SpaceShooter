using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public class GraphCreatorWindow : EditorWindow {
		private GraphCreator graphCreator;
		private Vector2 scrollPos;

		#region Window
		private static GraphCreatorWindow window;

		[MenuItem("Tools/uNode/Create New Graph", false, 1)]
		public static GraphCreatorWindow ShowWindow() {
			window = GetWindow<GraphCreatorWindow>(true);
			window.minSize = new Vector2(300, 250);
			window.titleContent = new GUIContent("Create New Graph");
			window.Show();
			return window;
		}

		public static GraphCreatorWindow ShowWindow(Type graphType) {
			window = ShowWindow();
			if(graphType == typeof(uNodeClass)) {
				window.graphCreator = FindGraphCreators().First(g => g is ClassGraphCreator);
			} else if(graphType == typeof(uNodeStruct)) {
				window.graphCreator = FindGraphCreators().First(g => g is StructGraphCreator);
			} else if(graphType == typeof(uNodeClassComponent)) {
				window.graphCreator = FindGraphCreators().First(g => g is ClassComponentCreator);
			} else if(graphType == typeof(uNodeClassAsset)) {
				window.graphCreator = FindGraphCreators().First(g => g is ClassAssetCreator);
			} else if(graphType == typeof(uNodeComponentSingleton)) {
				window.graphCreator = FindGraphCreators().First(g => g is SingletonGraphCreator);
			} else if(graphType == typeof(uNodeInterface)) {
				window.graphCreator = FindGraphCreators().First(g => g is ClassGraphCreator);
			} else if(graphType == typeof(uNodeClass)) {
				window.graphCreator = FindGraphCreators().First(g => g is ClassGraphCreator);
			}
			return window;
		}
		#endregion

		void OnGUI() {
			if (graphCreator == null) {
				graphCreator = FindGraphCreators().FirstOrDefault(item => item is ClassComponentCreator);
				if(graphCreator == null) {
					graphCreator = FindGraphCreators().FirstOrDefault();
				}
				if (graphCreator == null) {
					EditorGUILayout.HelpBox("No Graph Creator found", MessageType.Error);
					return;
				}
			}
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			var rect = EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Graph"));
			if (GUI.Button(rect, new GUIContent(graphCreator.menuName), EditorStyles.popup)) {
				var creators = FindGraphCreators();
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < creators.Count; i++) {
					var creator = creators[i];
					menu.AddItem(new GUIContent(creator.menuName), graphCreator == creator, () => {
						graphCreator = creator;
					});
				}
				menu.ShowAsContext();
				Event.current.Use();
			}
			graphCreator.OnGUI();
			EditorGUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save")) {
				var obj = graphCreator.CreateAsset();
				if(obj is Component component) {
					obj = component.gameObject;
				}
				string startPath = "Assets";
				var guids = Selection.assetGUIDs;
				foreach(var guid in guids) {
					startPath = AssetDatabase.GUIDToAssetPath(guid);
					if(!AssetDatabase.IsValidFolder(startPath)) {
						var pts = startPath.Split('/').ToArray();
						startPath = string.Join("/", pts, 0, pts.Length - 1);
					}
					break;
				}
				if(obj is GameObject gameObject) {
					string path = EditorUtility.SaveFilePanelInProject("Create new graph asset",
						gameObject.name + ".prefab",
						"prefab",
						"Please enter a file name to save the graph to",
						startPath);
					if (path.Length != 0) {
						PrefabUtility.SaveAsPrefabAsset(gameObject, path);
						Close();
					}
					DestroyImmediate(gameObject);
				} else if(obj is ScriptableObject asset) {
					string path = EditorUtility.SaveFilePanelInProject("Create new graph asset",
							asset.name + ".asset",
							"asset",
							"Please enter a file name to save the graph to",
							startPath);
					if (path.Length != 0) {
						AssetDatabase.CreateAsset(asset, path);
						AssetDatabase.SaveAssets();
						Close();
					}
					DestroyImmediate(asset);
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		private static List<GraphCreator> _graphCreators;
		public static List<GraphCreator> FindGraphCreators() {
			if (_graphCreators == null) {
				_graphCreators = EditorReflectionUtility.GetListOfType<GraphCreator>();
				_graphCreators.Sort((x, y) => {
					return CompareUtility.Compare(x.menuName, x.order, y.menuName, y.order);
				});
			}
			return _graphCreators;
		}
	}

	public abstract class GraphCreator {
		public abstract string menuName { get; }
		public virtual int order => 0;

		public abstract void OnGUI();
		public abstract Object CreateAsset();

		#region Fields
		protected string graphNamespaces;
		protected List<string> graphUsingNamespaces = new List<string>() {
			"UnityEngine",
			"System.Collections.Generic",
		};
		protected Texture2D graphIcon;
		protected List<UnityEventType> graphUnityEvents = new List<UnityEventType>() {
			UnityEventType.Start,
			UnityEventType.Update,
		};
		protected List<MemberInfo> graphOverrideMembers = new List<MemberInfo>();
		protected MemberData graphInheritFrom = MemberData.CreateFromType(typeof(object));
		protected FilterAttribute graphInheritFilter = new FilterAttribute() {
			OnlyGetType = true,
			DisplaySealedType = false,
			DisplayValueType = false,
			DisplayInterfaceType = false,
			UnityReference = false,
			ArrayManipulator = false,
			DisplayRuntimeType = false
		};
		protected GraphLayout graphLayout = GraphLayout.Vertical;
		#endregion

		#region Enums
		public enum UnityEventType {
			Awake,
			Start,
			Update,
			FixedUpdate,
			LateUpdate,
			OnAnimatorIK,
			OnAnimatorMove,
			OnApplicationFocus,
			OnApplicationPause,
			OnApplicationQuit,
			OnBecameInvisible,
			OnBecameVisible,
			OnCollisionEnter,
			OnCollisionEnter2D,
			OnCollisionExit,
			OnCollisionExit2D,
			OnCollisionStay,
			OnCollisionStay2D,
			OnDestroy,
			OnDisable,
			OnEnable,
			OnGUI,
			OnMouseDown,
			OnMouseDrag,
			OnMouseEnter,
			OnMouseExit,
			OnMouseOver,
			OnMouseUp,
			OnMouseUpAsButton,
			OnPostRender,
			OnPreCull,
			OnPreRender,
			OnRenderObject,
			OnTransformChildrenChanged,
			OnTransformParentChanged,
			OnTriggerEnter,
			OnTriggerEnter2D,
			OnTriggerExit,
			OnTriggerExit2D,
			OnTriggerStay,
			OnTriggerStay2D,
			OnWillRenderObject,
		}
		#endregion

		#region Functions
		protected Transform GetRootTransfrom(uNodeRoot graph) {
			if(graph.RootObject == null) {
				graph.RootObject = new GameObject("Root");
				graph.RootObject.transform.SetParent(graph.transform);
			}
			return graph.RootObject.transform;
		}

		protected void CreateOverrideMembers(uNodeRoot graph) {
			foreach(var member in graphOverrideMembers) {
				if(member is MethodInfo method) {
					var func = CreateComponent<uNodeFunction>(method.Name, GetRootTransfrom(graph), (val) => {
						val.Name = method.Name;
						val.returnType = MemberData.CreateFromType(method.ReturnType);
						val.parameters = method.GetParameters().Select(p => new ParameterData(p.Name, p.ParameterType) { value = p.DefaultValue }).ToArray();
						val.genericParameters = method.GetGenericArguments().Select(p => new GenericParameterData(p.Name)).ToArray();
					});
					CreateComponent<Nodes.NodeAction>("Entry", func, node => {
						func.startNode = node;
					});
				} else if(member is PropertyInfo property) {
					var prop = CreateComponent<uNodeProperty>(property.Name, GetRootTransfrom(graph), (val) => {
						val.Name = property.Name;
						val.type = MemberData.CreateFromType(property.PropertyType);
					});
					var getMethod = property.GetGetMethod(true);
					var setMethod = property.GetSetMethod(true);
					if(getMethod != null) {
						var func = CreateComponent<uNodeFunction>("Getter", GetRootTransfrom(graph), (val) => {
							val.Name = getMethod.Name;
							val.returnType = MemberData.CreateFromType(getMethod.ReturnType);
							val.parameters = getMethod.GetParameters().Select(p => new ParameterData(p.Name, p.ParameterType) { value = p.DefaultValue }).ToArray();
							val.genericParameters = getMethod.GetGenericArguments().Select(p => new GenericParameterData(p.Name)).ToArray();
						});
						CreateComponent<Nodes.NodeAction>("Entry", func, node => {
							func.startNode = node;
							CreateComponent<NodeReturn>("Return", func, returnNode => {
								node.onFinished = MemberData.FlowInput(returnNode);
								returnNode.returnValue = MemberData.CreateFromValue(null, property.PropertyType);
							});
						});
						prop.getRoot = func;
					}
					if(setMethod != null) {
						var func = CreateComponent<uNodeFunction>("Setter", GetRootTransfrom(graph), (val) => {
							val.Name = setMethod.Name;
							val.returnType = MemberData.CreateFromType(setMethod.ReturnType);
							val.parameters = setMethod.GetParameters().Select(p => new ParameterData(p.Name, p.ParameterType) { value = p.DefaultValue }).ToArray();
							val.genericParameters = setMethod.GetGenericArguments().Select(p => new GenericParameterData(p.Name)).ToArray();
						});
						CreateComponent<Nodes.NodeAction>("Entry", func, node => {
							func.startNode = node;
						});
						prop.setRoot = func;
					}
				}
			}
		}

		protected void CreateUnityEvents(uNodeRoot graph) {
			foreach(var evt in graphUnityEvents) {
				var func = CreateComponent<uNodeFunction>(evt.ToString(), GetRootTransfrom(graph), (val) => {
					val.Name = evt.ToString();
				});
				CreateComponent<Nodes.NodeAction>("Entry", func, node => {
					func.startNode = node;
				});
				switch(evt) {
					case UnityEventType.OnAnimatorIK:
						func.parameters = new ParameterData[] {
							new ParameterData("parameter", typeof(int))
						};
						break;
					case UnityEventType.OnApplicationFocus:
					case UnityEventType.OnApplicationPause:
						func.parameters = new ParameterData[] {
							new ParameterData("parameter", typeof(bool))
						};
						break;
					case UnityEventType.OnCollisionEnter:
					case UnityEventType.OnCollisionExit:
					case UnityEventType.OnCollisionStay:
						func.parameters = new ParameterData[] {
							new ParameterData("collision", typeof(Collision))
						};
						break;
					case UnityEventType.OnCollisionEnter2D:
					case UnityEventType.OnCollisionExit2D:
					case UnityEventType.OnCollisionStay2D:
						func.parameters = new ParameterData[] {
							new ParameterData("collision", typeof(Collision2D))
						};
						break;
					case UnityEventType.OnTriggerEnter:
					case UnityEventType.OnTriggerExit:
					case UnityEventType.OnTriggerStay:
						func.parameters = new ParameterData[] {
							new ParameterData("collider", typeof(Collider))
						};
						break;
					case UnityEventType.OnTriggerEnter2D:
					case UnityEventType.OnTriggerExit2D:
					case UnityEventType.OnTriggerStay2D:
						func.parameters = new ParameterData[] {
							new ParameterData("collider", typeof(Collider2D))
						};
						break;
				}
			}
		}

		protected T CreateComponent<T>(string name, Component parent, Action<T> action) where T : Component {
			GameObject gameObject = new GameObject(name);
			gameObject.transform.SetParent(parent.transform);
			var comp = gameObject.AddComponent<T>();
			action?.Invoke(comp);
			return comp;
		}
		#endregion

		#region GUI Functions
		protected void DrawNamespaces(string label = "Namespace") {
			graphNamespaces = EditorGUILayout.TextField(label, graphNamespaces);
		}

		protected void DrawUsingNamespaces(string label = "Using Namespaces") {
			VariableEditorUtility.DrawNamespace(label, graphUsingNamespaces, null, (val) => {
				graphUsingNamespaces = val.ToList();
			});
		}

		protected void DrawGraphLayout(string label = "Graph Layout") {
			graphLayout = (GraphLayout)EditorGUILayout.EnumPopup(label, graphLayout);
		}

		protected void DrawGraphIcon(string label = "Icon") {
			graphIcon = EditorGUI.ObjectField(uNodeGUIUtility.GetRect(), label, graphIcon, typeof(Texture2D), false) as Texture2D;
		}

		protected void DrawUnityEvent(string label = "Unity Events") {
			VariableEditorUtility.DrawCustomList(
				graphUnityEvents,
				label,
				drawElement: (position, index, element) => {
					EditorGUI.LabelField(position, element.ToString());
				},
				add: (pos) => {
					GenericMenu menu = new GenericMenu();
					var values = Enum.GetValues(typeof(UnityEventType)) as UnityEventType[];
					for (int i = 0; i < values.Length; i++) {
						var value = values[i];
						if (graphUnityEvents.Contains(value)) continue;
						menu.AddItem(new GUIContent(value.ToString()), false, () => {
							graphUnityEvents.Add(value);
						});
					}
					menu.ShowAsContext();
				},
				remove: (index) => {
					graphUnityEvents.RemoveAt(index);
				}
			);
		}

		protected void DrawOverrideMembers(string label = "Override Members") {
			Type type = graphInheritFrom.Get<Type>();
			if(type == null)
				return;
			VariableEditorUtility.DrawCustomList(
				graphOverrideMembers,
				label,
				drawElement: (position, index, element) => {
					EditorGUI.LabelField(position, NodeBrowser.GetRichMemberName(element));
				},
				add: (pos) => {
					var members = EditorReflectionUtility.GetOverrideMembers(type);
					GenericMenu menu = new GenericMenu();
					for(int i = 0; i < members.Count; i++) {
						var member = members[i];
						if(member is PropertyInfo) {
							menu.AddItem(new GUIContent("Properties/" + NodeBrowser.GetRichMemberName(member)), graphOverrideMembers.Contains(member), () => {
								graphOverrideMembers.Add(member);
							});
						} else {
							menu.AddItem(new GUIContent("Methods/" + NodeBrowser.GetRichMemberName(member)), graphOverrideMembers.Contains(member), () => {
								graphOverrideMembers.Add(member);
							});
						}
					}
					menu.ShowAsContext();
				},
				remove: (index) => {
					graphOverrideMembers.RemoveAt(index);
				}
			);
		}

		protected void DrawInheritFrom(string label = "Inherit From") {
			uNodeGUIUtility.EditValueLayouted(new GUIContent(label), graphInheritFrom, (val => {
				graphInheritFrom = val;
			}),
			new uNodeUtility.EditValueSettings() {
				attributes = new object[] {
					graphInheritFilter
				}
			});
		}
		#endregion
	}

	class StructGraphCreator : GraphCreator {
		public override string menuName => "C# Script/Struct";

		public override Object CreateAsset() {
			GameObject gameObject = new GameObject("new_struct");
			var graphData = gameObject.AddComponent<uNodeData>();
			graphData.Namespace = graphNamespaces;
			graphData.generatorSettings.usingNamespace = graphUsingNamespaces.ToArray();
			var graph = gameObject.AddComponent<uNodeStruct>();
			graph.graphData.graphLayout = graphLayout;
			return graph;
		}

		public override void OnGUI() {
			DrawNamespaces();
			DrawUsingNamespaces();
			DrawGraphLayout();
		}
	}

	abstract class ClassGraphCreator : GraphCreator {
		protected virtual uNodeClass CreateGraph() {
			GameObject gameObject = new GameObject("new_class");
			var graphData = gameObject.AddComponent<uNodeData>();
			graphData.Namespace = graphNamespaces;
			graphData.generatorSettings.usingNamespace = graphUsingNamespaces.ToArray();
			var graph = gameObject.AddComponent<uNodeClass>();
			graph.inheritFrom = new MemberData(graphInheritFrom);
			graph.graphData.graphLayout = graphLayout;
			return graph;
		}

		public override Object CreateAsset() {
			return CreateGraph();
		}

		public override void OnGUI() {
			DrawNamespaces();
			DrawUsingNamespaces();
			DrawGraphLayout();
		}
	}

	class EnumGraphCreator : GraphCreator {
		public override string menuName => "C# Script/Enum";

		public override Object CreateAsset() {
			GameObject gameObject = new GameObject(enumName);
			var graphData = gameObject.AddComponent<uNodeData>();
			graphData.Namespace = graphNamespaces;
			graphData.enums = new EnumData[] {
				new EnumData() {
					name = enumName,
					enumeratorList = enumeratorList.ToArray()
				}
			};
			return graphData;
		}

		string enumName = "";
		List<EnumData.Element> enumeratorList = new List<EnumData.Element>();

		public override void OnGUI() {
			DrawNamespaces();
			enumName = EditorGUILayout.TextField("Name", enumName);
			VariableEditorUtility.DrawCustomList(
				enumeratorList,
				"Enumerator List",
				drawElement: (position, index, element) => {
					element.name = EditorGUI.TextField(position, element.name);
				},
				add: (pos) => {
					enumeratorList.Add(new EnumData.Element());
				},
				remove: (index) => {
					enumeratorList.RemoveAt(index);
				}
			);
		}
	}

	class ClassScriptGraphCreator : ClassGraphCreator {
		public override string menuName => "C# Script/Class";

		protected override uNodeClass CreateGraph() {
			var graph = base.CreateGraph();
			CreateOverrideMembers(graph);
			return graph;
		}

		public override void OnGUI() {
			DrawInheritFrom();
			base.OnGUI();
			DrawOverrideMembers();
		}
	}

	class MonobehaviourScriptCreator : ClassGraphCreator {
		public override string menuName => "C# Script/MonoBehaviour";

		public MonobehaviourScriptCreator() {
			graphInheritFrom = MemberData.CreateFromType(typeof(MonoBehaviour));
			graphInheritFilter.Types.Add(typeof(MonoBehaviour));
		}

		protected override uNodeClass CreateGraph() {
			var graph = base.CreateGraph();
			CreateUnityEvents(graph);
			CreateOverrideMembers(graph);
			return graph;
		}

		public override void OnGUI() {
			DrawInheritFrom();
			base.OnGUI();
			DrawUnityEvent();
			DrawOverrideMembers();
		}
	}

	class ClassComponentCreator : GraphCreator {
		public override string menuName => "Class Component";

		protected virtual uNodeClassComponent CreateGraph() {
			GameObject gameObject = new GameObject("new_graph");
			return gameObject.AddComponent<uNodeClassComponent>();
		}

		public override Object CreateAsset() {
			var graph = CreateGraph();
			var fields = graph.GetType().GetFields(MemberData.flags);
			graph.GetType().GetField("icon", MemberData.flags).SetValue(graph, graphIcon);
			graph.GetType().GetField("namespace", MemberData.flags).SetValue(graph, graphNamespaces);
			graph.UsingNamespaces = graphUsingNamespaces;
			graph.graphData.graphLayout = graphLayout;
			CreateUnityEvents(graph);
			return graph;
		}

		public override void OnGUI() {
			DrawGraphIcon();
			DrawNamespaces();
			DrawUsingNamespaces();
			DrawUnityEvent();
			DrawGraphLayout();
		}
	}

	class ClassAssetCreator : GraphCreator {
		public override string menuName => "Class Asset";

		protected virtual uNodeClassAsset CreateClassAsset() {
			GameObject gameObject = new GameObject("new_graph");
			return gameObject.AddComponent<uNodeClassAsset>();
		}

		public override Object CreateAsset() {
			var graph = CreateClassAsset();
			graph.GetType().GetField("namespace", MemberData.flags).SetValue(graph, graphNamespaces);
			graph.GetType().GetField("icon", MemberData.flags).SetValue(graph, graphIcon);
			graph.UsingNamespaces = graphUsingNamespaces;
			graph.graphData.graphLayout = graphLayout;
			return graph;
		}

		public override void OnGUI() {
			DrawGraphIcon();
			DrawNamespaces();
			DrawUsingNamespaces();
			DrawGraphLayout();
		}
	}

	class SingletonGraphCreator : ClassComponentCreator {
		public override string menuName => "Component Singleton";

		protected override uNodeClassComponent CreateGraph() {
			GameObject gameObject = new GameObject("new_graph");
			return gameObject.AddComponent<uNodeComponentSingleton>();
		}
	}

	class CustomEditorCreator : GraphCreator {
		[Filter(typeof(Object), ArrayManipulator =false, OnlyGetType =true)]
		public MemberData editorType = MemberData.CreateFromType(typeof(Object));
		public override string menuName => "C# Script/Editor/Custom Editor";

		public CustomEditorCreator() {
			graphInheritFrom = MemberData.CreateFromType(typeof(Editor));
			graphInheritFilter.Types.Add(typeof(Editor));
			graphUsingNamespaces = new List<string>() {
				"UnityEngine",
				"UnityEditor",
				"System.Collections.Generic",
			};
			graphOverrideMembers = new List<MemberInfo>() {
				typeof(Editor).GetMethod(nameof(Editor.OnInspectorGUI))
			};
		}

		protected virtual uNodeClass CreateClassAsset() {
			GameObject gameObject = new GameObject("new_graph");
			return gameObject.AddComponent<uNodeClass>();
		}

		public override Object CreateAsset() {
			var graph = CreateClassAsset();
			var data = graph.gameObject.AddComponent<uNodeData>();
			data.Namespace = graphNamespaces;
			data.generatorSettings.usingNamespace = graphUsingNamespaces.ToArray();
			CreateOverrideMembers(graph);
			graph.Attributes = new AttributeData[] {
				new AttributeData() {
					type = MemberData.CreateFromType(typeof(CustomEditor)),
					value = new ValueData() {
						typeData = MemberData.CreateFromType(typeof(CustomEditor)),
						Value = new ConstructorValueData() {
							typeData = MemberData.CreateFromType(typeof(CustomEditor)),
							parameters = new[] {
								new ParameterValueData() {
									name = "inspectedType",
									typeData = MemberData.CreateFromType(typeof(Type)),
									value = editorType.Get<Type>()
								}
							}
						},
					},
				}
			};
			return graph;
		}

		public override void OnGUI() {
			uNodeGUIUtility.EditValueLayouted(nameof(editorType), this);
			DrawInheritFrom();
			DrawNamespaces();
			DrawUsingNamespaces();
			DrawOverrideMembers();
			DrawGraphLayout();
		}
	}

	//class EditorWindowCreator : GraphCreator {
	//	string menuItem = "Tools/My Window";
	//	public override string menuName => "C# Script/Editor/Editor Window";

	//	public EditorWindowCreator() {
	//		graphInheritFrom = MemberData.CreateFromType(typeof(EditorWindow));
	//		graphInheritFilter.Types.Add(typeof(EditorWindow));
	//		graphUsingNamespaces = new List<string>() {
	//			"UnityEngine",
	//			"UnityEditor",
	//			"System.Collections.Generic",
	//		};
	//	}

	//	protected virtual uNodeClass CreateClassAsset() {
	//		GameObject gameObject = new GameObject("new_graph");
	//		return gameObject.AddComponent<uNodeClass>();
	//	}

	//	public override Object CreateAsset() {
	//		var graph = CreateClassAsset();
	//		var data = graph.gameObject.AddComponent<uNodeData>();
	//		data.Namespace = graphNamespaces;
	//		data.generatorSettings.usingNamespace = graphUsingNamespaces.ToArray();
	//		CreateComponent<uNodeFunction>("ShowWindow", GetRootTransfrom(graph), val => {
	//			val.Name = "ShowWindow";
	//			val.attributes = new AttributeData[] {
	//				new AttributeData() {
	//					type = MemberData.CreateFromType(typeof(MenuItem)),
	//					value = new ValueData() {
	//						typeData = MemberData.CreateFromType(typeof(MenuItem)),
	//						value = new ConstructorValueData(new MenuItem(menuItem)),
	//					},
	//				}
	//			};
	//		});
	//		return graph;
	//	}

	//	public override void OnGUI() {
	//		menuItem = EditorGUILayout.TextField(new GUIContent("Menu"), menuItem);
	//		DrawInheritFrom();
	//		DrawNamespaces();
	//		DrawUsingNamespaces();
	//	}
	//}
}