using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// The base class for all uNode components
	/// </summary>
	public abstract class uNodeComponentSystem : MonoBehaviour { }

	[Serializable]
	public class GraphData {
		//General Data
		public string fileName = "";
		public GraphLayout graphLayout = GraphLayout.Vertical;
		//Generation Data
		[Serializable]
		public class ObjectData {
			public string name;
			public UnityEngine.Object value;
		}
		public List<ObjectData> unityObjects = new List<ObjectData>();
		public string typeName;
		//Generator Settings
		[Tooltip("If true, the script will be able to debug from editor.\nRecommended using prefab for generate with debug mode.")]
		public bool debug = false;
		[Hide("debug", false)]
		[Tooltip("if true, the value node will be able to debug.\nSince this not always success for debug a script, if you have error when using debug try to disable this.")]
		public bool debugValueNode = true;

		/// <summary>
		/// If true, the graph will be compiled to C# to run using native c# performance on build or in editor using ( Generate C# Scripts ) menu.
		/// </summary>
		public bool compileToScript = true;
	}

	/// <summary>
	/// Base class for all classes which implement graphs system.
	/// </summary>
	public abstract class uNodeRoot : uNodeComponentSystem, IGraphSystem, IRefreshable, ISummary {
		/// <summary>
		/// The name of this uNode.
		/// </summary>
		public string Name;
		/// <summary>
		/// The summary of this uNode.
		/// </summary>
		[TextArea]
		public string summary;
		/// <summary>
		/// The individual graph setting and data for independent graph
		/// </summary>
		[Hide]
		public GraphData graphData = new GraphData();

		#region Properties
		[SerializeField, Hide]
		private GameObject _rootObject;
		/// <summary>
		/// The root of node.
		/// </summary>
		public GameObject RootObject {
			get {
				return _rootObject;
			}
			set {
				_rootObject = value;
			}
		}

		/// <summary>
		/// Return the Full Type name of the generated script, empty if it never generated into script.
		/// </summary>
		public string GeneratedTypeName => graphData?.typeName;

		/// <summary>
		/// The display name of this class/struct
		/// </summary>
		public virtual string DisplayName => GraphName;
		/// <summary>
		/// The graph name, by default this should be same with the DisplayName.
		/// This also used for the class name for generating script so this should be unique without spaces or symbol.
		/// </summary>
		public virtual string GraphName {
			get {
				//return !string.IsNullOrEmpty(Name) ? Name.Replace(' ', '_') : "_" + Mathf.Abs(GetHashCode());
				if(string.IsNullOrEmpty(Name)) {
					if(string.IsNullOrEmpty(graphData.fileName)) {
						graphData.fileName = gameObject.name;
					}
					return graphData.fileName;
				}
				return Name;
			}
		}

		/// <summary>
		/// The full graph name including the namespaces
		/// </summary>
		public virtual string FullGraphName {
			get {
				string ns = Namespace;
				if(!string.IsNullOrEmpty(ns)) {
					return ns + "." + GraphName;
				} else {
					return GraphName;
				}
			}
		}

		/// <summary>
		/// Te graph namespaces
		/// </summary>
		public virtual string Namespace {
			get {
				if(this is IIndependentGraph) {
					return (this as IIndependentGraph).Namespace;
				} else {
					var data = GetComponent<uNodeData>();
					if(data != null) {
						return data.Namespace;
					}
					return string.Empty;
				}
			}
		}
		#endregion

		public abstract List<VariableData> Variables { get; }
		public abstract IList<uNodeProperty> Properties { get; }
		public abstract IList<uNodeFunction> Functions { get; }
		public abstract IList<uNodeConstuctor> Constuctors { get; }

		/// <summary>
		/// The list of Node in this uNode.
		/// </summary>
		[HideInInspector]
		public Node[] nodes = new Node[0];
		public IList<Node> Nodes {
			get {
				return nodes;
			}
		}
		
		public abstract Type GetInheritType();

		/// <summary>
		/// Get the used namespaces.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetNamespaces() {
			if(this is IIndependentGraph graph) {
				return graph.UsingNamespaces;
			}
			var nsSystem = GetComponent<uNodeData>();
			if(nsSystem != null) {
				return nsSystem.GetNamespaces();
			}
			return new string[] { "UnityEngine", "System.Collections.Generic" };
		}
		
		/// <summary>
		/// Get function
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public virtual uNodeFunction GetFunction(string name, params System.Type[] parameters) {
			return GetFunction(name, 0, parameters);
		}

		/// <summary>
		/// Get function
		/// </summary>
		/// <param name="name"></param>
		/// <param name="genericParameterLength"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public virtual uNodeFunction GetFunction(string name, int genericParameterLength, params System.Type[] parameters) {
			int parameterLength = parameters != null ? parameters.Length : 0;
			for(int i = 0; i < Functions.Count; i++) {
				uNodeFunction function = Functions[i];
				if(function.Name == name && function.parameters.Length == parameterLength && function.genericParameters.Length == genericParameterLength) {
					bool isValid = true;
					if(parameterLength != 0) {
						for(int x = 0; x < parameters.Length; x++) {
							if(genericParameterLength > 0) {
								if(function.parameters[x].Type != null && function.parameters[x].type.targetType != MemberData.TargetType.uNodeGenericParameter && function.parameters[x].Type != parameters[x]) {
									isValid = false;
									break;
								}
							} else if(function.parameters[x].Type != null &&
								!function.parameters[x].Type.IsGenericTypeDefinition &&
								 function.parameters[x].Type != parameters[x]) {
								isValid = false;
								break;
							}
						}
					}
					if(isValid) {
						return function;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Get property data by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual uNodeProperty GetPropertyData(string name) {
			for(int i = 0; i < Properties.Count; i++) {
				uNodeProperty property = Properties[i];
				if(property.Name.Equals(name)) {
					return property;
				}
			}
			return null;
		}

		/// <summary>
		/// Get variable by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual VariableData GetVariableData(string name) {
			return uNodeUtility.GetVariableData(name, Variables);
		}

		#region Coroutines
		private Dictionary<object, List<Coroutine>> routineMap = new Dictionary<object, List<Coroutine>>();
		/// <summary>
		/// Start a coroutine.
		/// </summary>
		/// <param name="routine"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public Coroutine StartCoroutine(IEnumerator routine, object owner) {
			var result = StartCoroutine(routine);
			if(!routineMap.ContainsKey(owner)) {
				routineMap[owner] = new List<Coroutine>();
			}
			routineMap[owner].Add(result);
			return result;
		}

		/// <summary>
		/// Stop all coroutines running on owner.
		/// </summary>
		/// <param name="owner"></param>
		public void StopAllCoroutines(object owner) {
			List<Coroutine> coroutineList;
			if(routineMap.TryGetValue(owner, out coroutineList)) {
				foreach(var routine in coroutineList) {
					if(routine != null) {
						StopCoroutine(routine);
					}
				}
				//Clear after stoping all coroutine.
				coroutineList.Clear();
			}
		}
		#endregion

		#region Editors
		private void OnValidate() {
			if(RootObject != null && uNodeUtility.isInEditor && uNodeUtility.hideRootObject != null) {
				if(uNodeUtility.hideRootObject()) {
					RootObject.hideFlags = HideFlags.HideInHierarchy;
				} else {
					RootObject.hideFlags = HideFlags.None;
				}
			}
		}

		/// <summary>
		/// Used for refresh uNode in Editor.
		/// </summary>
		public virtual void Refresh() {
			graphData.fileName = gameObject.name;
			if(RootObject != null) {
				OnValidate();
				nodes = RootObject.GetComponentsInChildren<Node>(true);
				foreach(var node in nodes) {
					if(node != null) {
						if(node.owner != this) {
							node.owner = this;
						}
						if(node.enabled) {
							node.enabled = false;
						}
					}
				}
			}
		}

		string ISummary.GetSummary() {
			return summary;
		}

		protected uNodeProperty[] GetProperties() {
			var properties = RootObject.GetComponentsInChildren<uNodeProperty>(true);
			foreach(uNodeProperty prop in properties) {
				if(prop == null)
					continue;
				if(prop.gameObject.name != prop.Name) {
					prop.gameObject.name = prop.Name;
				}
				if(prop.owner != this) {
					prop.owner = this;
				}
				if(prop.setRoot != null) {
					prop.setRoot.parameters = new ParameterData[1] { new ParameterData("value", prop.ReturnType()) };
					prop.setRoot.owner = prop.owner;
				}
				if(prop.getRoot != null) {
					prop.getRoot.returnType = new MemberData(prop.ReturnType(), MemberData.TargetType.Type);
					prop.getRoot.owner = prop.owner;
				}
			}
			return properties;
		}

		protected uNodeConstuctor[] GetConstuctors() {
			var constructors = RootObject.GetComponentsInChildren<uNodeConstuctor>(true);
			foreach(uNodeConstuctor ctor in constructors) {
				if(ctor == null)
					continue;
				if(ctor.gameObject.name != ctor.Name) {
					ctor.gameObject.name = ctor.Name;
				}
				if(ctor.owner != this) {
					ctor.owner = this;
				}
			}
			return constructors;
		}

		protected uNodeFunction[] GetFunctions(){
			List<uNodeFunction> func = new List<uNodeFunction>();
			RootObject.GetComponentsInChildren(true, func);
			func.RemoveAll(f => f.transform.parent != RootObject.transform);
			foreach(uNodeFunction function in func) {
				if(function == null)
					continue;
				if(function.gameObject.name != function.Name) {
					function.gameObject.name = function.Name;
				}
				if(function.owner != this) {
					function.owner = this;
				}
			}
			return func.ToArray();
		}
		#endregion
	}
}