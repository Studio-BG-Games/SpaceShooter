using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.uNode.Editors.TreeViews {
	#region Class Data
	public class ExplorerEditorData {
		public bool showSummary = true;
		public bool showTypeIcon = true;
		public bool showVariable = true;
		public bool showProperty = true;
		public bool showFunction = true;
		public bool showEnum = true;
		public bool showInterface = true;
		public bool showGraph = true;
		public bool showMacro = true;

		public bool expandGraphs;
		public bool expandMacros;

		public SearchKind searchKind;

		public List<ExplorerObjectData> objects = new List<ExplorerObjectData>();
		public List<ExplorerGraphData> graphs = new List<ExplorerGraphData>();
	}

	public abstract class ExplorerData : ITreeData {
		public string guid;
		public bool expanded;
	}

	public class ExplorerObjectData : ExplorerData {
		public List<ExplorerComponentData> components = new List<ExplorerComponentData>();
	}

	public abstract class ExplorerComponentData : ExplorerData {

	}

	public class ExplorerGraphData : ExplorerComponentData {
		public List<ExplorerVariableData> variables = new List<ExplorerVariableData>();
		public List<ExplorerPropertyData> properties = new List<ExplorerPropertyData>();
		public List<ExplorerFunctionData> functions = new List<ExplorerFunctionData>();
	}

	public class ExplorerVariableData : ExplorerData {
	}

	public class ExplorerPropertyData : ExplorerData {
	}

	public class ExplorerFunctionData : ExplorerData {
	}
	#endregion

	#region Views
	public class SimpleTreeView : TreeViewItem {
		public SimpleTreeView() {

		}

		public SimpleTreeView(Texture icon, Action onFirstExpanded) {
			if(onFirstExpanded == null) {
				expandedElement.visible = false;
			} else {
				this.onFirstExpanded += onFirstExpanded;
			}
			titleIcon.image = icon;
		}
	}

	public class ObjectTreeView : TreeViewItem, ITreeDataSystem {
		public GameObject gameObject;
		public ExplorerObjectData data;

		public ObjectTreeView(GameObject gameObject, ExplorerObjectData data = null) {
			if(data == null) {
				data = new ExplorerObjectData();
			}
			data.guid = uNodeUtility.GetObjectID(gameObject).ToString();
			expanded = data.expanded;
			this.data = data;

			this.gameObject = gameObject;
			title = gameObject.name;

			onExpandClicked += () => {
				if(!isInSearch) {
					data.expanded = expanded;
					Save();
				}
			};

			onFirstExpanded += () => {
				var components = gameObject.GetComponents<uNodeComponentSystem>();
				foreach(var c in components) {
					if(c is uNodeRoot) {
						string uid = uNodeUtility.GetObjectID(c).ToString();
						var childData = data.components.FirstOrDefault(d => d.guid == uid) as ExplorerGraphData;
						if(childData == null) {
							childData = new ExplorerGraphData();
							data.components.Add(childData);
						}
						AddChild(new GraphTreeView(c as uNodeRoot, childData));
					} else if(c is uNodeData) {
						var root = c as uNodeData;
						if(ExplorerManager.explorerData.showEnum) {
							foreach(var e in root.enums) {
								AddChild(new EnumView(e, root));
							}
						}
						if(ExplorerManager.explorerData.showInterface) {
							foreach(var i in root.interfaces) {
								AddChild(new InterfaceView(i, root));
							}
						}
					}
				}
			};
		}

		public ITreeData GetData() {
			return data;
		}

		public void SetData(ITreeData value) {
			data = value as ExplorerObjectData;
		}
	}

	public class GraphTreeView : TreeViewItem, ITreeDataSystem, IDragableTree {
		public uNodeRoot root;
		public ExplorerGraphData data = new ExplorerGraphData();

		public GraphTreeView(uNodeRoot root, ExplorerGraphData data = null) {
			if(data == null) {
				data = new ExplorerGraphData();
			}
			data.guid = uNodeUtility.GetObjectID(root).ToString();
			expanded = data.expanded;
			this.data = data;
			this.root = root;
			title = root.DisplayName;

			titleContainer.RegisterCallback<MouseDownEvent>(evt => {
				if(evt.clickCount >= 2 && evt.button == 0) {
					uNodeEditor.Open(root);
				}
			});
			onExpandClicked += () => {
				if(!isInSearch) {
					data.expanded = expanded;
					Save();
				}
			};

			if(!string.IsNullOrWhiteSpace(root.summary) && ExplorerManager.explorerData.showSummary) {
				var summary = new SummaryView(root.summary);
				headerContainer.Add(summary);
			}
			if(root is ICustomIcon) {
				titleIcon.image = uNodeEditorUtility.GetTypeIcon(root);
			} else if(root is IMacroGraph) {
				titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.GraphIcon));
			} else if(root is IClassSystem) {
				titleIcon.image = uNodeEditorUtility.GetTypeIcon((root as IClassSystem).IsStruct ? typeof(TypeIcons.StructureIcon) : typeof(TypeIcons.ClassIcon));
			} else {
				titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.RuntimeTypeIcon));
			}

			onFirstExpanded += () => {
				if(ExplorerManager.explorerData.showVariable) {
					foreach(var v in root.Variables) {
						string uid = v.Name;
						var childData = data.variables.FirstOrDefault(d => d.guid == uid);
						if(childData == null) {
							childData = new ExplorerVariableData();
							data.variables.Add(childData);
						}
						AddChild(new VariableView(v, root, childData));
					}
				}
				if(ExplorerManager.explorerData.showProperty) {
					foreach(var p in root.Properties) {
						string uid = uNodeUtility.GetObjectID(p).ToString();
						var childData = data.properties.FirstOrDefault(d => d.guid == uid);
						if(childData == null) {
							childData = new ExplorerPropertyData();
							data.properties.Add(childData);
						}
						AddChild(new PropertyView(p, childData));
					}
				}
				if(ExplorerManager.explorerData.showFunction) {
					foreach(var f in root.Functions) {
						string uid = uNodeUtility.GetObjectID(f).ToString();
						var childData = data.functions.FirstOrDefault(d => d.guid == uid);
						if(childData == null) {
							childData = new ExplorerFunctionData();
							data.functions.Add(childData);
						}
						AddChild(new FunctionView(f, childData));
					}
				}
			};
		}

		public ITreeData GetData() {
			return data;
		}

		public void SetData(ITreeData value) {
			data = value as ExplorerGraphData;
		}
	}

	public class VariableView : TreeViewItem, ITreeDataSystem, IDragableTree {
		public IVariableSystem owner;
		public VariableData variable;
		public ExplorerVariableData data = new ExplorerVariableData();

		public Image typeIcon;

		public VariableView(VariableData variable, IVariableSystem owner, ExplorerVariableData data = null) {
			if(data == null) {
				data = new ExplorerVariableData();
			}
			data.guid = variable.Name;
			this.data = data;
			this.variable = variable;
			this.owner = owner;
			title = $"{variable.Name} : {variable.type.DisplayName(false, false)}";
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FieldIcon));
			expandedElement.visible = false;

			titleContainer.RegisterCallback<MouseDownEvent>(evt => {
				if(evt.clickCount >= 2 && evt.button == 0 && owner is uNodeRoot) {
					uNodeEditor.Open(owner as uNodeRoot);
					uNodeEditor.window.ChangeEditorSelection(GraphUtility.GetTempGraphVariable(variable.Name, owner as uNodeRoot) ?? variable);
				}
			});
			if(ExplorerManager.explorerData.showSummary) {
				if(!string.IsNullOrWhiteSpace(variable.summary)) {
					var summary = new SummaryView(variable.summary);
					headerContainer.Add(summary);
				}
			}
			if(ExplorerManager.explorerData.showTypeIcon) {
				typeIcon = new Image() {
					name = "type-icon",
					image = uNodeEditorUtility.GetTypeIcon(variable.type),
				};
				titleContainer.Add(typeIcon);
				typeIcon.PlaceInFront(titleIcon);
			}
		}

		protected override bool IsValidSearch(string searchText) {
			return IsValidSearch(variable.Name, searchText);
		}

		public ITreeData GetData() {
			return data;
		}

		public void SetData(ITreeData value) {
			data = value as ExplorerVariableData;
		}
	}

	public class FunctionView : TreeViewItem, ITreeDataSystem, IDragableTree {
		public uNodeFunction function;
		public ExplorerFunctionData data = new ExplorerFunctionData();
		public Image typeIcon;

		public FunctionView(uNodeFunction function, ExplorerFunctionData data = null) {
			if(data == null) {
				data = new ExplorerFunctionData();
			}
			data.guid = uNodeUtility.GetObjectID(function).ToString();
			expanded = data.expanded;
			this.data = data;
			this.function = function;
			title = $"{function.Name}({ string.Join(", ", function.Parameters.Select(p => p.type.DisplayName(false, false))) }) : {function.returnType.DisplayName(false, false)}";
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon));
			expanded = data.expanded;
			expandedElement.visible = false;

			titleContainer.RegisterCallback<MouseDownEvent>(evt => {
				if(evt.clickCount >= 2 && evt.button == 0) {
					uNodeEditor.Open(function);
					uNodeEditor.window.Refresh();
				}
			});
			onExpandClicked += () => {
				if(!isInSearch) {
					data.expanded = expanded;
					Save();
				}
			};
			if(ExplorerManager.explorerData.showSummary) {
				if(!string.IsNullOrWhiteSpace(function.summary)) {
					var summary = new SummaryView(function.summary);
					headerContainer.Add(summary);
				}
			}
			if(ExplorerManager.explorerData.showTypeIcon) {
				typeIcon = new Image() {
					name = "type-icon",
					image = uNodeEditorUtility.GetIcon(function.returnType),
				};
				titleContainer.Add(typeIcon);
				typeIcon.PlaceInFront(titleIcon);
			}
		}

		protected override bool IsValidSearch(string searchText) {
			return IsValidSearch(function.Name, searchText);
		}

		public ITreeData GetData() {
			return data;
		}

		public void SetData(ITreeData value) {
			data = value as ExplorerFunctionData;
		}
	}

	public class PropertyView : TreeViewItem, ITreeDataSystem, IDragableTree {
		public uNodeProperty property;
		public ExplorerPropertyData data = new ExplorerPropertyData();
		public Image typeIcon;

		public PropertyView(uNodeProperty property, ExplorerPropertyData data = null) {
			if(data == null) {
				data = new ExplorerPropertyData();
			}
			data.guid = uNodeUtility.GetObjectID(property).ToString();
			expanded = data.expanded;
			this.data = data;
			this.property = property;
			title = $"{property.Name} : {property.type.DisplayName(false, false)}";
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.PropertyIcon));
			expanded = data.expanded;
			expandedElement.visible = false;

			onExpandClicked += () => {
				if(!isInSearch) {
					data.expanded = expanded;
					Save();
				}
			};
			if(ExplorerManager.explorerData.showSummary) {
				if(!string.IsNullOrWhiteSpace(property.summary)) {
					var summary = new SummaryView(property.summary);
					headerContainer.Add(summary);
				}
			}
			if(ExplorerManager.explorerData.showTypeIcon) {
				typeIcon = new Image() {
					name = "type-icon",
					image = uNodeEditorUtility.GetIcon(property.type),
				};
				titleContainer.Add(typeIcon);
				typeIcon.PlaceInFront(titleIcon);
			}
		}

		protected override bool IsValidSearch(string searchText) {
			return IsValidSearch(property.Name, searchText);
		}

		public ITreeData GetData() {
			return data;
		}

		public void SetData(ITreeData value) {
			data = value as ExplorerPropertyData;
		}
	}

	public class EnumView : TreeViewItem, IDragableTree {
		public uNodeData owner;
		public EnumData enumData;

		public EnumView(EnumData enumData, uNodeData owner) {
			this.enumData = enumData;
			this.owner = owner;
			title = enumData.name;
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.EnumIcon));
			onFirstExpanded += () => {
				foreach(var e in enumData.enumeratorList) {
					var item = new SimpleTreeView(uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.EnumItemIcon)), null) {
						title = e.name,
					};
					AddChild(item);
				}
			};
		}
	}

	public class InterfaceView : TreeViewItem, IDragableTree {
		public uNodeData owner;
		public InterfaceData interfaceData;

		public InterfaceView(InterfaceData interfaceData, uNodeData owner) {
			this.owner = owner;
			this.interfaceData = interfaceData;
			title = interfaceData.name;
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.InterfaceIcon));

			foreach(var p in interfaceData.properties) {
				AddChild(new InterfacePropertyView(p, interfaceData));
			}
			foreach(var f in interfaceData.functions) {
				AddChild(new InterfaceFunctionView(f, interfaceData));
			}
		}
	}

	public class InterfacePropertyView : TreeViewItem {
		public InterfaceData owner;
		public InterfaceProperty property;

		public InterfacePropertyView(InterfaceProperty property, InterfaceData owner) {
			this.owner = owner;
			this.property = property;
			title = $"{property.name} : {property.returnType.DisplayName(false, false)}";
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.PropertyIcon));
			expandedElement.visible = false;
		}

		protected override bool IsValidSearch(string searchText) {
			return IsValidSearch(property.name, searchText);
		}
	}

	public class InterfaceFunctionView : TreeViewItem {
		public InterfaceData owner;
		public InterfaceFunction function;

		public InterfaceFunctionView(InterfaceFunction function, InterfaceData owner) {
			this.owner = owner;
			this.function = function;
			title = $"{function.name}({ string.Join(", ", function.parameters.Select(p => p.type.DisplayName(false, false))) }) : {function.returnType.DisplayName(false, false)}";
			titleIcon.image = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MethodIcon));
			expandedElement.visible = false;
		}

		protected override bool IsValidSearch(string searchText) {
			return IsValidSearch(function.name, searchText);
		}
	}
	#endregion

	public class ExplorerManager : TreeManager, IRefreshable {
		public List<TreeViewItem> items = new List<TreeViewItem>();

		public List<uNodeMacro> macros = new List<uNodeMacro>();
		public List<UnityEngine.Object> graphs = new List<UnityEngine.Object>();

		public SimpleTreeView graphTree;
		public SimpleTreeView macroTree;

		public static ExplorerEditorData explorerData = new ExplorerEditorData();

		public override SearchKind searchKind {
			get {
				return explorerData.searchKind;
			}
		}

		public ExplorerManager() {
			name = "exploler";
			styleSheets.Add(Resources.Load<StyleSheet>("ExplorerStyles/ExplorerView"));
			UIElementUtility.ForceDarkStyleSheet(this);
		}

		public void Refresh() {
			foreach(var item in items) {
				item.RemoveFromHierarchy();
			}
			var prefabs = GraphUtility.FindGraphPrefabs().ToList();
			prefabs.Sort((x, y) => string.CompareOrdinal(x.gameObject.name, y.gameObject.name));
			macros.Clear();
			graphs.Clear();
			foreach(var p in prefabs) {
				var comp = p.GetComponent<uNodeComponentSystem>();
				if(comp is uNodeMacro) {
					macros.Add(comp as uNodeMacro);
				} else {
					var comps = p.GetComponents<uNodeComponentSystem>();
					if(comps.Length == 1 && comps[0] is IIndependentGraph) {
						graphs.Add(comps[0]);
					} else {
						graphs.Add(p);
					}
				}
			}

			if(explorerData.showGraph) {
				graphTree = new SimpleTreeView() {
					title = "Graphs",
					manager = this,
					expanded = explorerData.expandGraphs,
				};
				graphTree.onExpandClicked += () => {
					if(!graphTree.isInSearch) {
						explorerData.expandGraphs = graphTree.expanded;
						Save();
					}
				};
				foreach(var g in graphs) {
					if (g is GameObject) {
						string uid = uNodeUtility.GetObjectID(g).ToString();
						var childData = explorerData.objects.FirstOrDefault(d => d.guid == uid);
						if (childData == null) {
							childData = new ExplorerObjectData();
							explorerData.objects.Add(childData);
						}
						graphTree.AddChild(new ObjectTreeView(g as GameObject, childData));
					} else if(g is uNodeRoot graph) {
						string uid = uNodeUtility.GetObjectID(graph).ToString();
						var childData = explorerData.graphs.FirstOrDefault(d => d.guid == uid);
						if(childData == null) {
							childData = new ExplorerGraphData();
							explorerData.graphs.Add(childData);
						}
						graphTree.AddChild(new GraphTreeView(graph, childData));
					}
				}
				graphTree.Initialize();
				Add(graphTree);
				items.Add(graphTree);
			}
			if (explorerData.showMacro) {
				macroTree = new SimpleTreeView() {
					title = "Macros",
					manager = this,
					expanded = explorerData.expandMacros,
				};
				macroTree.onExpandClicked += () => {
					if (!macroTree.isInSearch) {
						explorerData.expandMacros = macroTree.expanded;
						Save();
					}
				};
				foreach (var m in macros) {
					string uid = uNodeUtility.GetObjectID(m).ToString();
					var childData = explorerData.graphs.FirstOrDefault(d => d.guid == uid);
					if (childData == null) {
						childData = new ExplorerGraphData();
						explorerData.graphs.Add(childData);
					}
					macroTree.AddChild(new GraphTreeView(m, childData));
				}
				macroTree.Initialize();
				Add(macroTree);
				items.Add(macroTree);
			}
		}

		public void Search(string value) {
			foreach(var item in items) {
				item.Search(value);
			}
		}

		public event Action onSavePerformed;

		public override void Save() {
			onSavePerformed?.Invoke();
		}
	}

	public class SummaryView : VisualElement {
		public string text {
			get {
				return label.text;
			}
			set {
				label.text = value;
			}
		}

		public Label label;

		public SummaryView() {
			name = "summary";
			label = new Label();
		}

		public SummaryView(string text) {
			name = "summary";
			label = new Label(text);
			Add(label);
		}
	}
}