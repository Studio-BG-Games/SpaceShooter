using MaxyGames.uNode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MaxyGames {
	public static partial class CG {
		static class Cached {
			static List<Type> _types;
			public static List<Type> types {
				get {
					if(_types == null) {
						var assemblies = ReflectionUtils.GetStaticAssemblies();
						_types = new List<Type>();
						for(int i = 0; i < assemblies.Length; i++) {
							try {
								var t = ReflectionUtils.GetAssemblyTypes(assemblies[i]);
								_types.AddRange(t);
							}
							catch { }
						}
					}
					return _types;
				}
			}
		}

        public sealed class StringWrapper {
			public string value;

			public StringWrapper(string value) {
				this.value = value;
			}
		}

		public enum State {
			None,
			Function,
			Classes,
			Constructor,
			Property,
		}

		public sealed class GeneratorState {
			public bool isStatic;
			public State state = State.Classes;
		}

		public class CoroutineData {
			public string variableName;
			public string onStop;
			public string contents;
			public Func<string> customExecution;

			public override string ToString() {
				return variableName;
			}
		}

		internal class BlockStack {
			internal bool allowYield;
			// internal Type returnType;
		}

		//Node data
		public class NData {
			public NodeComponent node;

			public List<NPData> valueInputs = new List<NPData>();
			public List<NPData> valueOutputs = new List<NPData>();
			public List<NPData> flowInputs = new List<NPData>();
			public List<NPData> flowOutputs = new List<NPData>();

			public NData(NodeComponent node) {
				this.node = node;
			}

			private HashSet<NodeComponent> _flowNodes;
			public HashSet<NodeComponent> flowNodes {
				get {
					if(_flowNodes == null) {
						var result = new HashSet<NodeComponent>();
						foreach(var d in flowInputs) {
							result.Add(d.owner);
						}
						foreach(var d in flowOutputs) {
							result.Add(d.target);
						}
						_flowNodes = result;
					}
					return _flowNodes;
				}
			}

			private HashSet<NodeComponent> _flowOutputNodes;
			public HashSet<NodeComponent> flowOutputNodes {
				get {
					if(_flowOutputNodes == null) {
						var result = new HashSet<NodeComponent>();
						foreach(var d in flowOutputs) {
							result.Add(d.target);
						}
						_flowOutputNodes = result;
					}
					return _flowOutputNodes;
				}
			}

			private HashSet<NodeComponent> _flowInputNodes;
			public HashSet<NodeComponent> flowInputNodes {
				get {
					if(_flowInputNodes == null) {
						var result = new HashSet<NodeComponent>();
						foreach(var d in flowInputs) {
							result.Add(d.owner);
						}
						_flowInputNodes = result;
					}
					return _flowInputNodes;
				}
			}
		}

		public class NPData {
			/// <summary>
			/// The owner of the connection.
			/// -Flow: upper / output port.
			/// -Value: right / input port.
			/// </summary>
			public NodeComponent owner;
			/// <summary>
			/// The target node that's connected to.
			/// -Flow: lower / input port.
			/// -Value: left / input port.
			/// </summary>
			public NodeComponent target;
			/// <summary>
			/// The connection value
			/// </summary>
			public MemberData connection;

			/// <summary>
			/// The owner data.
			/// -Flow: upper / output port.
			/// -Value: right / input port.
			/// Note: this data is only exist after initialization completed.
			/// </summary>
			public NData ownerData;
			/// <summary>
			/// The target data.
			/// -Flow: lower / input port.
			/// -Value: left / input port.
			/// Note: this data is only exist after initialization completed.
			/// </summary>
			public NData targetData;

			public bool flowIsYield;
			public bool flowIsState;
			public bool localFunction;

			/// <summary>
			/// Get the sender node data.
			/// -Flow: upper / output port.
			/// -Value: left / input port.
			/// </summary>
			/// <returns></returns>
			public NData GetSenderData() {
				if(connection.targetType.IsTargetingFlowPort()) {
					return ownerData;
				} else {
					return targetData;
				}
			}


			/// <summary>
			/// Get the receiver node data.
			/// -Flow: lower / input port.
			/// -Value: right / input port.
			/// </summary>
			/// <returns></returns>
			public NData GetReceiverData() {
				if(connection.targetType.IsTargetingFlowPort()) {
					return targetData;
				} else {
					return ownerData;
				}
			}

			/// <summary>
			/// Get the sender node.
			/// -Flow: upper / output port.
			/// -Value: left / input port.
			/// </summary>
			/// <returns></returns>
			public NodeComponent GetSenderNode() {
				if(connection.targetType.IsTargetingFlowPort()) {
					return owner;
				} else {
					return target;
				}
			}

			/// <summary>
			/// Get the receiver node.
			/// -Flow: lower / input port
			/// -Value: right / input port
			/// </summary>
			/// <returns></returns>
			public NodeComponent GetReceiverNode() {
				if(connection.targetType.IsTargetingFlowPort()) {
					return target;
				} else {
					return owner;
				}
			}
		}

		public class GData {
			public GeneratorSetting setting;
			public GeneratorState state = new GeneratorState();
			public bool hasError = false;

			public string typeName {
				get;
				internal set;
			}

			public void ValidateTypes(string Namespace, HashSet<string> usingNamespace, Func<Type, bool> func) {
				for(int i = 0; i < Cached.types.Count; i++) {
					var type = Cached.types[i];
					if(type.IsPublic && Namespace != type.Namespace && (usingNamespace.Contains(type.Namespace) || type.Namespace == null)) {
						if(func(type)) {
							return;
						}
					}
				}
			}

			internal BlockStack currentBlock => blockStacks.Count > 0 ? blockStacks[blockStacks.Count - 1] : null;

			internal List<BlockStack> blockStacks = new List<BlockStack>();

			private List<VData> variables = new List<VData>();
			public List<PData> properties = new List<PData>();
			public List<CData> constructors = new List<CData>();
			public List<MData> methodData = new List<MData>();

			public Dictionary<NodeComponent, NData> nodeConnections = new Dictionary<NodeComponent, NData>();

			public List<BaseGraphEvent> eventNodes = new List<BaseGraphEvent>();
			public List<NodeComponent> allNode = new List<NodeComponent>();
			public HashSet<Node> flowNode = new HashSet<Node>();
			public HashSet<NodeComponent> regularNodes = new HashSet<NodeComponent>();
			public HashSet<NodeComponent> stateNodes = new HashSet<NodeComponent>();
			public HashSet<Node> portableActionInNode = new HashSet<Node>();

			public List<Exception> errors = new List<Exception>();

			public HashSet<NodeComponent> registeredFlowNodes = new HashSet<NodeComponent>();

			public Dictionary<Type, string> typesMap = new Dictionary<Type, string>();
			public Dictionary<NodeComponent, HashSet<NodeComponent>> FlowConnectedTo = new Dictionary<NodeComponent, HashSet<NodeComponent>>();
			public Dictionary<NodeComponent, string> generatedData = new Dictionary<NodeComponent, string>();
			public Dictionary<uNodeComponent, string> eventCoroutineData = new Dictionary<uNodeComponent, string>();
			public Dictionary<object, string> invokeCode = new Dictionary<object, string>();
			public Dictionary<NodeComponent, string> methodName = new Dictionary<NodeComponent, string>();

			internal Dictionary<NodeComponent, bool> stackOverflowMap = new Dictionary<NodeComponent, bool>();

			internal Dictionary<Block, string> eventActions = new Dictionary<Block, string>();
			internal Dictionary<object, CoroutineData> coroutineEvent = new Dictionary<object, CoroutineData>();

			internal Dictionary<MemberData, KeyValuePair<int, string>> debugMemberMap = new Dictionary<MemberData, KeyValuePair<int, string>>();
			internal Dictionary<Transform, HashSet<NodeComponent>> nodesMap = new Dictionary<Transform, HashSet<NodeComponent>>();

			public Dictionary<object, object> userObjectMap = new Dictionary<object, object>();
			public Dictionary<object, Dictionary<string, string>> variableNamesMap = new Dictionary<object, Dictionary<string, string>>();

			internal Dictionary<string, Dictionary<Type, Dictionary<string, string>>> customUIDMethods = new Dictionary<string, Dictionary<Type, Dictionary<string, string>>>();

			public Dictionary<UnityEngine.Object, string> unityVariableMap = new Dictionary<UnityEngine.Object, string>();

			internal Dictionary<object, Dictionary<FieldInfo, Dictionary<int, string>>> fieldVariableMap = new Dictionary<object, Dictionary<FieldInfo, Dictionary<int, string>>>();
			public Dictionary<string, int> VarNames = new Dictionary<string, int>();
			private Dictionary<string, int> generatedNames = new Dictionary<string, int>();
			private Dictionary<string, int> generatedMethodNames = new Dictionary<string, int>();

			public Dictionary<NodeComponent, Action> initActionForNodes = new Dictionary<NodeComponent, Action>();
			public Dictionary<UnityEngine.Object, HashSet<int>> initializedUserObject = new Dictionary<UnityEngine.Object, HashSet<int>>();

			public Dictionary<object, Dictionary<string, VariableData>> variableAliases = new Dictionary<object, Dictionary<string, VariableData>>();

			public void AddVariable(VData variable) {
				variables.Add(variable);
			}

			public void AddVariableAlias(string name, VariableData variable, object owner) {
				Dictionary<string, VariableData> map;
				if(!variableAliases.TryGetValue(owner, out map)) {
					map = new Dictionary<string, VariableData>();
					variableAliases[owner] = map;
				}
				map[name] = variable;
			}

			public VariableData GetVariableAlias(string name, object owner) {
				Dictionary<string, VariableData> map;
				if(variableAliases.TryGetValue(owner, out map)) {
					VariableData variable;
					if(map.TryGetValue(name, out variable)) {
						return variable;
					}
				}
				return null;
			}

			public void AddEventCoroutineData(uNodeComponent comp, string contents) {
				eventCoroutineData[comp] = contents;
			}

			public List<VData> GetVariables() {
				return variables;
			}

			public string GenerateName(string startName = "variable") {
				if(string.IsNullOrEmpty(startName)) {
					startName = "variable";
				}
				startName = uNodeUtility.AutoCorrectName(startName);
				if(generatedNames.ContainsKey(startName)) {
					string name;
					while(true) {
						name = startName + (++generatedNames[startName]).ToString();
						if(!generatedNames.ContainsKey(name)) {
							break;
						}
					}
					return name;
				} else {
					string name = startName;
					if(generatedNames.ContainsKey(name)) {
						while(true) {
							name = startName + (++generatedNames[startName]).ToString();
							if(!generatedNames.ContainsKey(name)) {
								break;
							}
						}
					}
					generatedNames.Add(name, 0);
					return name;
				}
			}

			/// <summary>
			/// Function for generating correctly method name
			/// </summary>
			/// <param name="startName"></param>
			/// <returns></returns>
			public string GenerateMethodName(string startName = "Method") {
				if(string.IsNullOrEmpty(startName)) {
					startName = "Method";
				}
				startName = uNodeUtility.AutoCorrectName(startName);
				if(generatedMethodNames.ContainsKey(startName)) {
					string name;
					while(true) {
						name = startName + (++generatedMethodNames[startName]).ToString();
						if(!generatedMethodNames.ContainsKey(name)) {
							break;
						}
					}
					return name;
				} else {
					string name = startName;
					if(generatedMethodNames.ContainsKey(name)) {
						while(true) {
							name = startName + (++generatedMethodNames[startName]).ToString();
							if(!generatedMethodNames.ContainsKey(name)) {
								break;
							}
						}
					}
					generatedMethodNames.Add(name, 0);
					return name;
				}
			}

			public string GetEventID(object target) {
				if(target == null)
					throw new System.ArgumentNullException("target cannot be null");
				if(target is MemberData member) {
					var node = member.GetTargetNode();
					if(node != null) {
						target = node;
					} else {
						throw new Exception("Unsupported target type for event.\nType:" + target.GetType().FullName);
					}
				}
				if(target is UnityEngine.Object) {
					return target.GetHashCode().ToString();
				} else if(!target.GetType().IsValueType) {
					int index = 1;
					while(invokeCode.ContainsValue(index.ToString())) {
						index++;
					}
					invokeCode.Add(target, index.ToString());
					return index.ToString();
				}
				throw new Exception("Unsupported value for event.\nType:" + target.GetType().FullName);
			}

			public string GetMethodName(BaseGraphEvent method) {
				if(method == null)
					throw new System.Exception("method can't null");
				if(methodName.ContainsKey(method)) {
					return methodName[method];
				}
				string name = generatorData.GenerateMethodName(method.GetNodeName());
				methodName.Add(method, name);
				return name;
			}

			/// <summary>
			/// Get Correct Method Data.
			/// </summary>
			/// <param name="methodName"></param>
			/// <param name="returnType"></param>
			/// <param name="parametersType"></param>
			/// <returns></returns>
			public MData GetMethodData(string methodName, IList<string> parametersType = null, int genericParameterLength = -1) {
				if(parametersType == null || parametersType.Count == 0) {
					foreach(MData m in methodData) {
						if(m.name == methodName && (parametersType == null || (m.parameters == null || m.parameters.Count == 0))) {
							if(genericParameterLength >= 0 && 
								(m.genericParameters == null ? 0 : m.genericParameters.Count) != genericParameterLength) {
								continue;
							}
							return m;
						}
					}
				} else {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.parameters != null && m.parameters.Count == parametersType.Count) {
							bool correct = true;
							for(int i = 0; i < m.parameters.Count; i++) {
								if(m.parameters[i].type != parametersType[i]) {
									correct = false;
									break;
								}
							}
							if(correct) {
								if(genericParameterLength >= 0 && 
									(m.genericParameters == null ? 0 : m.genericParameters.Count) != genericParameterLength) {
									continue;
								}
								return m;
							}
						}
					}
				}
				return null;
			}

			public void InsertMethodCode(string methodName, string code, int priority = 0) {
				foreach(MData m in methodData) {
					if(m.name == methodName) {
						m.AddCode(code, priority);
						return;
					}
				}
				throw new System.Exception("No Method data found to insert code");
			}

			public MData AddMethod(string methodName, string returnType, params string[] parametersType) {
				return AddMethod(methodName, returnType, parametersType as IList<string>);
			}

			public MData AddMethod(string methodName, string returnType, IList<string> parametersType) {
				if(string.IsNullOrEmpty(methodName) || returnType == null)
					throw new System.Exception("Method name or return type can't null");
				if(parametersType == null || parametersType.Count == 0) {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType) {
							return m;
						}
					}
				} else {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType && m.parameters != null && m.parameters.Count == parametersType.Count) {
							bool correct = true;
							for(int i = 0; i < m.parameters.Count; i++) {
								if(m.parameters[i].type != parametersType[i]) {
									correct = false;
									break;
								}
							}
							if(correct) {
								return m;
							}
						}
					}
				}
				MData mData = new MData(methodName, returnType, parametersType);
				methodData.Add(mData);
				return mData;
			}

			public MData AddMethod(string methodName, string returnType, MPData[] parametersType) {
				if(string.IsNullOrEmpty(methodName) || returnType == null)
					throw new System.Exception("Method name or return type can't null");
				if(parametersType == null || parametersType.Length == 0) {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType) {
							return m;
						}
					}
				} else {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType && m.parameters != null && m.parameters.Count == parametersType.Length) {
							bool correct = true;
							for(int i = 0; i < m.parameters.Count; i++) {
								if(m.parameters[i].type != parametersType[i].type) {
									correct = false;
									break;
								}
							}
							if(correct) {
								return m;
							}
						}
					}
				}
				MData mData = new MData(methodName, returnType, parametersType);
				methodData.Add(mData);
				return mData;
			}

			public MData AddMethod(string methodName, string returnType, MPData[] parametersType, GPData[] genericParameters) {
				if(string.IsNullOrEmpty(methodName) || returnType == null)
					throw new System.Exception("Method name or return type can't null");
				if(parametersType == null || parametersType.Length == 0) {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType &&
							(m.genericParameters == null || genericParameters == null || m.genericParameters.Count == genericParameters.Length)) {
							return m;
						}
					}
				} else {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType &&
							m.parameters != null && m.parameters.Count == parametersType.Length &&
							(m.genericParameters == null || genericParameters == null || m.genericParameters.Count == genericParameters.Length)) {
							bool correct = true;
							for(int i = 0; i < m.parameters.Count; i++) {
								if(m.parameters[i].type != parametersType[i].type) {
									correct = false;
									break;
								}
							}
							if(correct) {
								return m;
							}
						}
					}
				}
				MData mData = new MData(methodName, returnType, parametersType, genericParameters);
				methodData.Add(mData);
				return mData;
			}

			public void InsertCustomUIDMethod(string methodName, Type returnType, string ID, string contents) {
				Dictionary<Type, Dictionary<string, string>> map;
				if(!customUIDMethods.TryGetValue(methodName, out map)) {
					map = new Dictionary<Type, Dictionary<string, string>>();
					customUIDMethods[methodName] = map;
				}
				Dictionary<string, string> map2;
				if(!map.TryGetValue(returnType, out map2)) {
					map2 = new Dictionary<string, string>();
					map[returnType] = map2;
				}
				map2[ID] = contents;
			}

			public void InsertMethodCode(string methodName, string returnType, string code, params string[] parametersType) {
				if(parametersType == null || parametersType.Length == 0) {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType) {
							m.code += code;
							return;
						}
					}
				} else {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType && m.parameters != null && m.parameters.Count == parametersType.Length) {
							bool correct = true;
							for(int i = 0; i < m.parameters.Count; i++) {
								if(m.parameters[i].type != parametersType[i]) {
									correct = false;
									break;
								}
							}
							if(correct) {
								m.code += code;
								return;
							}
						}
					}
				}
				MData mData = new MData(methodName, returnType, parametersType) { code = code };
				methodData.Add(mData);
			}

			public void InsertMethodCode(string methodName, string returnType, string code, params MPData[] parameters) {
				if(parameters == null || parameters.Length == 0) {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType) {
							m.code += code;
							return;
						}
					}
				} else {
					foreach(MData m in methodData) {
						if(m.name == methodName && m.type == returnType && m.parameters != null && m.parameters.Count == parameters.Length) {
							bool correct = true;
							for(int i = 0; i < m.parameters.Count; i++) {
								if(m.parameters[i].type != parameters[i].type) {
									correct = false;
									break;
								}
							}
							if(correct) {
								m.code += code;
								return;
							}
						}
					}
				}
				MData mData = new MData(methodName, returnType, parameters) { code = code };
				methodData.Add(mData);
			}
		}

        public class GeneratorSetting {
			private string _filename;
			public string fileName {
				get {
					if(string.IsNullOrEmpty(_filename)) {
						return targetData?.gameObject.name ?? graphs.First().gameObject.name;
					}
					return _filename;
				}
				set {
					_filename = value;
				}
			}
			public string nameSpace;
			public uNodeData targetData;
			public ICollection<uNodeRoot> graphs;

			private IEnumerable<InterfaceData> _interfaces;
			public IEnumerable<InterfaceData> interfaces {
				get {
					if(_interfaces == null && targetData != null) {
						return targetData.interfaces;
					}
					return _interfaces;
				}
				set {
					_interfaces = value;
				}
			}

			private IEnumerable<EnumData> _enums;
			public IEnumerable<EnumData> enums {
				get {
					if(_enums == null && targetData != null) {
						return targetData.enums;
					}
					return _enums;
				}
				set {
					_enums = value;
				}
			}

			public HashSet<string> usingNamespace;
			public HashSet<string> scriptHeaders = new HashSet<string>();
			public bool fullTypeName;
			public bool fullComment = false;
			public bool runtimeOptimization = false;

			public GenerationKind generationMode = GenerationKind.Default;

			public bool debugScript;
			public bool debugValueNode;
			public bool debugPreprocessor = false;
			public bool includeGraphInformation = true;
			[NonSerialized]
			public int debugID;

			public bool isPreview;
			public bool isAsync;
			public int maxQueue = 1;

			public Dictionary<int, int> objectInformations = new Dictionary<int, int>();

			public int GetSettingUID() {
				string str = string.Empty;
				str += generationMode.ToString();
				if(fullTypeName) {
					str += nameof(fullTypeName);
				}
				if(fullComment) {
					str += nameof(fullComment);
				}
				if(runtimeOptimization) {
					str += nameof(runtimeOptimization);
				}
				if(debugScript) {
					str += nameof(debugScript);
				}
				if(debugValueNode) {
					str += nameof(debugValueNode);
				}
				return uNodeUtility.GetUIDFromString(str);
			}

			public Action<float, string> updateProgress;

			public GeneratorSetting(params uNodeRoot[] graphs) {
				this.graphs = graphs;
				if(graphs != null) {
					var g = graphs.FirstOrDefault();
					if(g != null && g is IIndependentGraph iGraph) {
						nameSpace = iGraph.Namespace;
						usingNamespace = new HashSet<string>(iGraph.UsingNamespaces);
					}
					debugScript = g.graphData.debug;
					debugValueNode = g.graphData.debugValueNode;
				}
			}

			public GeneratorSetting(ICollection<uNodeRoot> graphs, string nameSpace = null, IList<string> usingNamespace = null) {
				this.graphs = graphs != null ? graphs : new uNodeRoot[0];
				this.nameSpace = nameSpace;
				this.usingNamespace = usingNamespace == null ? new HashSet<string>() { "UnityEngine", "System.Collections.Generic" } : new HashSet<string>(usingNamespace);
			}

			public GeneratorSetting(uNodeInterface ifaceAsset) {
				if(ifaceAsset == null) {
					throw new ArgumentNullException(nameof(ifaceAsset));
				}
				graphs = new uNodeRoot[0];
				fileName = ifaceAsset.name;
				nameSpace = string.IsNullOrEmpty(ifaceAsset.@namespace) ? RuntimeType.RuntimeNamespace : ifaceAsset.@namespace;
				usingNamespace = ifaceAsset.usingNamespaces.ToHashSet();
				interfaces = new InterfaceData[] { new InterfaceData() {
					name = ifaceAsset.name,
					summary = ifaceAsset.summary,
					modifiers = ifaceAsset.modifiers,
					functions = ifaceAsset.functions,
					properties = ifaceAsset.properties,
				} };
			}

			public GeneratorSetting(uNodeData uNodeData, ICollection<uNodeRoot> graphs) {
				if(graphs == null) {
					throw new ArgumentNullException(nameof(graphs));
				}
				this.graphs = graphs;
				targetData = uNodeData;
				if(targetData) {
					nameSpace = targetData.generatorSettings.Namespace;
					usingNamespace = new HashSet<string>(targetData.generatorSettings.usingNamespace);
					debugScript = targetData.generatorSettings.debug;
					debugValueNode = targetData.generatorSettings.debugValueNode;
				} else if(graphs != null) {
					var g = graphs.FirstOrDefault();
					if(g != null && g is IIndependentGraph iGraph) {
						nameSpace = iGraph.Namespace;
						usingNamespace = new HashSet<string>(iGraph.UsingNamespaces);
					}
					debugScript = g.graphData.debug;
					debugValueNode = g.graphData.debugValueNode;
				}
			}

			//public GeneratorSetting(GameObject gameObject, uNodeData.GeneratorSettings settings) {
			//	if(!gameObject) {
			//		throw new ArgumentNullException("gameObject");
			//	}
			//	targetData = gameObject.GetComponent<uNodeData>();
			//	graphs = gameObject.GetComponents<uNodeRoot>();
			//	if(settings != null) {
			//		nameSpace = settings.Namespace;
			//		usingNamespace = new HashSet<string>(settings.usingNamespace);
			//		fullTypeName = settings.fullTypeName;
			//		enableOptimization = settings.enableOptimization;
			//		resolveUnityObject = settings.resolveUnityObject;
			//		fullComment = settings.fullComment;
			//		generateTwice = settings.generateNodeTwice;
			//		debugScript = settings.debug;
			//		debugValueNode = settings.debugValueNode;
			//		forceGenerateAllNode = settings.forceGenerateAllNode;
			//	} else if(graphs != null) {
			//		var g = graphs.FirstOrDefault();
			//		if(g != null && g is IIndependentGraph iGraph) {
			//			nameSpace = iGraph.Namespace;
			//			usingNamespace = new HashSet<string>(iGraph.UsingNamespaces);
			//		}
			//		debugScript = g.graphData.debug;
			//		debugValueNode = g.graphData.debugValueNode;
			//	}
			//}

			public GeneratorSetting(GameObject gameObject, GeneratorSetting settings) {
				if(!gameObject) {
					throw new ArgumentNullException(nameof(gameObject));
				}
				targetData = gameObject.GetComponent<uNodeData>();
				graphs = gameObject.GetComponents<uNodeRoot>();
				if(settings != null) {
					nameSpace = settings.nameSpace;
					usingNamespace = settings.usingNamespace;
					fullTypeName = settings.fullTypeName;
					fullComment = settings.fullComment;
					debugScript = settings.debugScript;
					debugValueNode = settings.debugValueNode;
				} else if(graphs != null) {
					var g = graphs.FirstOrDefault();
					if(g != null && g is IIndependentGraph iGraph) {
						nameSpace = iGraph.Namespace;
						usingNamespace = new HashSet<string>(iGraph.UsingNamespaces);
					}
					debugScript = g.graphData.debug;
					debugValueNode = g.graphData.debugValueNode;
				}
			}
		}

		static class ThreadingUtil {
			private static List<Action> actions = new List<Action>();
			private static int maxQueue = 1;

			public static void Do(Action action) {
				if(setting.isAsync) {
					uNodeThreadUtility.QueueOnFrame(() => {
						action();
					});
					uNodeThreadUtility.WaitUntilEmpty();
				} else {
					action();
				}
			}

			public static void WaitOneFrame() {
				if(setting.isAsync) {
					uNodeThreadUtility.WaitOneFrame();
				}
			}

			public static void WaitQueue() {
				if(setting.isAsync) {
					if(actions.Count > 0) {
						List<Action> list = new List<Action>(actions);
						uNodeThreadUtility.QueueOnFrame(() => {
							foreach(var a in list) {
								if(a != null) {
									a();
								}
							}
						});
						actions.Clear();
					}
					uNodeThreadUtility.WaitUntilEmpty();
				}
			}

			public static void Queue(Action action) {
				if(setting.isAsync) {
					if(maxQueue > 1) {
						if(actions.Count < maxQueue) {
							actions.Add(action);
						} else {
							List<Action> list = new List<Action>(actions);
							uNodeThreadUtility.QueueOnFrame(() => {
								foreach(var a in list) {
									if(a != null) {
										a();
									}
								}
							});
							actions.Clear();
						}
					} else {
						uNodeThreadUtility.QueueOnFrame(() => {
							action();
						});
					}
				} else {
					action();
				}
			}

			public static void SetMaxQueue(int max) {
				maxQueue = max;
			}
		}

		/// <summary>
		/// Used for store Attribute Data
		/// </summary>
		public class AData {
			public Type attributeType;
			public string[] attributeParameters;
			public Dictionary<string, string> namedParameters;

			public AData() { }

			public AData(Type attributeType, params string[] attributeParameters) {
				this.attributeType = attributeType;
				this.attributeParameters = attributeParameters;
			}
		}

		/// <summary>
		/// Used for store Variable Data
		/// </summary>
		public class VData {
			/// <summary>
			/// The name of variable
			/// </summary>
			public string name;
			/// <summary>
			/// The summary of variable.
			/// </summary>
			public string summary;
			/// <summary>
			/// The object reference of variable.
			/// </summary>
			public object variableRef;

			private Type _type;
			/// <summary>
			/// The variable type.
			/// </summary>
			public Type type {
				get {
					if(_type == null) {
						if(variableRef is VariableData) {
							_type = (variableRef as VariableData).Type;
						} else if(variableRef is object[]) {
							_type = ((variableRef as object[])[1] as FieldInfo).FieldType;
						}
					}
					return _type;
				}
				set {
					_type = value;
				}
			}
			/// <summary>
			/// The default value
			/// </summary>
			public object defaultValue;
			/// <summary>
			/// Is the variable is instance or owned by classes.
			/// </summary>
			public bool isInstance;
			/// <summary>
			/// The variable modifiers
			/// </summary>
			public FieldModifier modifier;
			/// <summary>
			/// The variable attributes.
			/// </summary>
			public IList<AData> attributes;

			#region Constructor
			public VData(VariableData var, bool autoCorrectVariable, IList<AData> attribute = null) {
				if(autoCorrectVariable) {
					name = GenerateVariableName(var.Name);
				} else {
					name = var.Name;
				}
				variableRef = var;
				summary = var.summary;
				this.attributes = attribute;
				this.type = var.Type;
				this.defaultValue = var.variable;
				this.isInstance = true;
			}

			public VData(VariableData var, IList<AData> attribute = null) {
				name = GenerateVariableName(var.Name);
				variableRef = var;
				summary = var.summary;
				this.attributes = attribute;
				this.type = var.Type;
				this.defaultValue = var.variable;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, IList<AData> attribute = null) {
				name = GenerateVariableName(field.Name);
				variableRef = new object[] { from, field, 0 };
				this.attributes = attribute;
				this.type = field.FieldType;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, Type type, IList<AData> attribute = null) {
				name = GenerateVariableName(field.Name);
				variableRef = new object[] { from, field, 0 };
				this.attributes = attribute;
				this.type = type;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, string name, IList<AData> attribute = null) {
				this.name = name;
				variableRef = new object[] { from, field, 0 };
				this.attributes = attribute;
				this.type = field.FieldType;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, Type type, string name, IList<AData> attribute = null) {
				this.name = name;
				variableRef = new object[] { from, field, 0 };
				this.attributes = attribute;
				this.type = type;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, int index, IList<AData> attribute = null) {
				name = GenerateVariableName(field.Name);
				variableRef = new object[] { from, field, index };
				this.attributes = attribute;
				this.type = field.FieldType;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, int index, Type type, IList<AData> attribute = null) {
				name = GenerateVariableName(field.Name);
				variableRef = new object[] { from, field, index };
				this.attributes = attribute;
				this.type = type;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, int index, string name, IList<AData> attribute = null) {
				this.name = name;
				variableRef = new object[] { from, field, index };
				this.attributes = attribute;
				this.type = field.FieldType;
				this.defaultValue = null;
				this.isInstance = true;
			}

			public VData(object from, FieldInfo field, int index, Type type, string name, IList<AData> attribute = null) {
				this.name = name;
				variableRef = new object[] { from, field, index };
				this.attributes = attribute;
				this.type = type;
				this.defaultValue = null;
				this.isInstance = true;
			}
			#endregion

			public string GenerateCode() {
				string result = null;
				if(attributes != null) {
					foreach(AData att in attributes) {
						string code = TryParseAttribute(att);
						if(!string.IsNullOrEmpty(code)) {
							result += code.AddFirst("\n", !string.IsNullOrEmpty(result));
						}
					}
				}
				string m = null;
				if(modifier != null) {
					m = modifier.GenerateCode();
				}
				bool isGeneric = false;
				string vType;
				if(variableRef is VariableData) {
					vType = Type((variableRef as VariableData).type);
					isGeneric = (variableRef as VariableData).type.targetType == MemberData.TargetType.uNodeGenericParameter;
				} else {
					vType = Type(type);
				}
				if(type is FakeType || type == null) {
					defaultValue = null;
				}
				if(isGeneric) {
					if(defaultValue != null) {
						result += (m + vType + " " + name + " = default(" + vType + ");").AddFirst("\n", !string.IsNullOrEmpty(result));
					} else {
						result += (m + vType + " " + name + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
					}
				} else {
					if(!ReflectionUtils.IsNullOrDefault(defaultValue) && !(graph is uNodeStruct)) {
						if(defaultValue is UnityEngine.Object obj && obj != graph) {
							result += (m + vType + " " + name + " = " + Value(defaultValue) + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
						} else {
							result += (m + vType + " " + name + " = " + Value(defaultValue) + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
						}
					} else {
						result += (m + vType + " " + name + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
					}
				}
				if(!string.IsNullOrEmpty(summary)) {
					result = "/// <summary>".AddLineInEnd() +
						"/// " + summary.Replace("\n", "\n" + "/// ").AddLineInEnd() +
						"/// </summary>" +
						result.AddLineInFirst();
				}
				return result;
			}

			public bool IsStatic {
				get {
					return modifier != null && modifier.Static;
				}
			}
		}

		/// <summary>
		/// Used for store Constructor Data
		/// </summary>
		public class CData {
			public string name {
				get {
					return obj.owner.GraphName;
				}
			}
			public string summary;
			public uNodeConstuctor obj;
			public ConstructorModifier modifier;

			public CData(uNodeConstuctor constructor) {
				this.obj = constructor;
				this.summary = constructor.summary;
			}

			public string GenerateCode() {
				string result = null;
				string m = null;
				if(modifier != null) {
					m = modifier.GenerateCode();
				}
				string code = null;
				if(obj.startNode) {
					code = GenerateNode(obj.startNode);
				}
				string parameters = null;
				if(obj.parameters != null && obj.parameters.Length > 0) {
					int index = 0;
					var parametersData = obj.parameters.Select(i => new MPData(i.name, Type(i.type), i.refKind));
					foreach(MPData data in parametersData) {
						if(index != 0) {
							parameters += ", ";
						}
						parameters += data.type + " ";
						if(string.IsNullOrEmpty(data.name)) {
							parameters += "parameter" + index;
						} else {
							parameters += data.name;
						}
						index++;
					}
				}
				string lv = null;
				if(obj.localVariable != null && obj.localVariable.Count > 0) {
					foreach(var vdata in obj.localVariable) {
						if(IsInstanceVariable(vdata)) {
							continue;
						}
						if(vdata.type.isAssigned && vdata.type.targetType == MemberData.TargetType.Type && vdata.type.startType.IsValueType && vdata.value == null) {
							lv += (Type(vdata.type) + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
							continue;
						}
						if(vdata.type.targetType == MemberData.TargetType.uNodeGenericParameter) {
							string vType = Type(vdata.type);
							if(vdata.variable != null) {
								lv += (vType + " " + GetVariableName(vdata) + " = default(" + vType + ");").AddFirst("\n", !string.IsNullOrEmpty(lv));
							} else {
								lv += (vType + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
							}
							continue;
						}
						lv += (Type(vdata.type) + " " + GetVariableName(vdata) +
							" = " + Value(vdata.value) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
					}
					lv = lv.AddLineInFirst().AddTabAfterNewLine();
				}
				result += (m + name + "(" + parameters + ") {" + lv.Add("\n", string.IsNullOrEmpty(code)) + code.AddTabAfterNewLine().AddLineInEnd() + "}").AddFirst("\n", !string.IsNullOrEmpty(result));
				if(!string.IsNullOrEmpty(summary)) {
					result = "/// <summary>".AddLineInEnd() +
						"/// " + summary.Replace("\n", "\n" + "/// ").AddLineInEnd() +
						"/// </summary>" +
						result.AddLineInFirst();
				}
				return result;
			}
		}

		/// <summary>
		/// Used for store Property Data
		/// </summary>
		public class PData {
			public string name {
				get {
					return obj.Name;
				}
			}
			public string summary;
			public uNodeProperty obj;
			public PropertyModifier modifier;
			public IList<AData> attributes;

			public PData(uNodeProperty property) {
				obj = property;
				summary = property.summary;
			}

			public PData(uNodeProperty property, IList<AData> attributes) {
				obj = property;
				summary = property.summary;
				this.attributes = attributes;
			}

			public string GenerateCode() {
				string result = null;
				foreach(AData a in attributes) {
					string code = TryParseAttribute(a);
					if(!string.IsNullOrEmpty(code)) {
						result += code.AddFirst("\n", !string.IsNullOrEmpty(result));
					}
				}
				string m = null;
				if(modifier != null) {
					m = modifier.GenerateCode();
				}
				string p = null;
				if(obj.AutoProperty) {
					p = "{ get; set; }";
				} else {
					p += "{\n";
					if(obj.CanGetValue()) {
						var str = "get {\n";
						if(!obj.getterModifier.isPublic) {
							str = str.Insert(0, obj.getterModifier.GenerateCode());
						}
						if(obj.getRoot.localVariable != null && obj.getRoot.localVariable.Count > 0) {
							string lv = null;
							foreach(var vdata in obj.getRoot.localVariable) {
								if(IsInstanceVariable(vdata)) {
									continue;
								}
								if(vdata.type.isAssigned && vdata.type.targetType == MemberData.TargetType.Type && vdata.type.startType.IsValueType && vdata.value == null) {
									lv += (Type(vdata.type) + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
									continue;
								}
								lv += (Type(vdata.type) + " " + GetVariableName(vdata) +
									" = " + Value(vdata.value) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
							}
							str += lv.AddLineInFirst().AddTabAfterNewLine();
						}
						if(!obj.getRoot.startNode) {
							if(setting.isAsync) {
								str += "/* getter start node is not set. */".AddTabAfterNewLine();
							} else {
								throw new System.Exception("getter start node is not set in property: " + obj.Name);
							}
						} else {
							str += GenerateNode(obj.getRoot.startNode).AddTabAfterNewLine();
						}
						str += "\n}";
						p += str.AddTabAfterNewLine() + "\n";
					}
					if(obj.CanSetValue()) {
						var str = "set {\n";
						if(!obj.setterModifier.isPublic) {
							str = str.Insert(0, obj.setterModifier.GenerateCode());
						}
						if(obj.setRoot.localVariable != null && obj.setRoot.localVariable.Count > 0) {
							string lv = null;
							foreach(var vdata in obj.setRoot.localVariable) {
								if(IsInstanceVariable(vdata)) {
									continue;
								}
								if(vdata.type.isAssigned && vdata.type.targetType == MemberData.TargetType.Type && vdata.type.startType.IsValueType && vdata.value == null) {
									lv += (Type(vdata.type) + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
									continue;
								}
								if(vdata.type.targetType == MemberData.TargetType.uNodeGenericParameter) {
									string vType = Type(vdata.type);
									if(vdata.variable != null) {
										lv += (vType + " " + GetVariableName(vdata) + " = default(" + vType + ");").AddFirst("\n", !string.IsNullOrEmpty(lv));
									} else {
										lv += (vType + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
									}
									continue;
								}
								lv += (Type(vdata.type) + " " + GetVariableName(vdata) +
									" = " + Value(vdata.value) + ";").AddFirst("\n", !string.IsNullOrEmpty(lv));
							}
							str += lv.AddLineInFirst().AddTabAfterNewLine();
						}
						if(!obj.setRoot.startNode) {
							//throw new System.Exception("setter start node is not set in property: " + obj.Name);
						} else {
							str += GenerateNode(obj.setRoot.startNode).AddTabAfterNewLine();
						}
						str += "\n}";
						p += str.AddTabAfterNewLine() + "\n";
					}
					p += "}";
				}
				result += (m + Type(obj.ReturnType()) + " " + name + " " + p).AddFirst("\n", !string.IsNullOrEmpty(result));
				if(!string.IsNullOrEmpty(summary)) {
					result = "/// <summary>".AddLineInEnd() +
						"/// " + summary.Replace("\n", "\n" + "/// ").AddLineInEnd() +
						"/// </summary>" +
						result.AddLineInFirst();
				}
				return result;
			}
		}

		/// <summary>
		/// Used for store Parameter Data
		/// </summary>
		public class MPData {
			public string name;
			public string type;
			public ParameterData.RefKind refKind;

			public MPData(string name, string type, ParameterData.RefKind refKind = ParameterData.RefKind.None) {
				this.name = name;
				this.type = type;
				this.refKind = refKind;
			}

			public MPData(string name, Type type, ParameterData.RefKind refKind = ParameterData.RefKind.None) {
				this.name = name;
				this.type = CG.Type(type);
				this.refKind = refKind;
			}
		}

		/// <summary>
		/// Used for store Generic Parameter Data
		/// </summary>
		public class GPData {
			public string name;
			public string type;

			public GPData() { }
			public GPData(string name) {
				this.name = name;
			}
			public GPData(string name, Type type) {
				this.name = name;
				if(type != typeof(object)) {
					this.type = type.FullName;
				}
			}
			public GPData(string name, string type) {
				this.name = name;
				if(type != typeof(object).FullName) {
					this.type = type;
				}
			}
		}

		/// <summary>
		/// Used for store Method Data
		/// </summary>
		public class MData {
			public string name;
			public string type;
			public IList<MPData> parameters;
			public IList<GPData> genericParameters;
			public List<AData> attributes;
			public string code;
			public FunctionModifier modifier;
			public string summary;

			public RootObject owner;

			private List<KeyValuePair<int, string>> codeList = new List<KeyValuePair<int, string>>();
			private HashSet<float> ownerUID = new HashSet<float>();

			public void AddCodeForEvent(string code) {
				AddCode(code, -100);
			}

			public void AddCode(string code, int priority = 0) {
				codeList.Add(new KeyValuePair<int, string>(priority, code));
			}

			public void AddCode(string code, UnityEngine.Object owner, int priority = 0) {
				AddCode(code, owner.GetInstanceID(), priority);
			}

			public void AddCode(string code, float ownerID, int priority) {
				if(ownerUID.Contains(ownerID)) {
					return;
				}
				ownerUID.Add(ownerID);
				codeList.Add(new KeyValuePair<int, string>(priority, code));
			}

			public void ClearCode() {
				codeList.Clear();
				ownerUID.Clear();
			}

			public string GenerateCode() {
				string result = null;
				if(attributes != null && attributes.Count > 0) {
					foreach(AData attribute in attributes) {
						string a = TryParseAttribute(attribute);
						if(!string.IsNullOrEmpty(a)) {
							result += a.AddLineInEnd();
						}
					}
				}
				if(graph is IIndependentGraph) {
					if(name == "Awake") {
						//Ensure to change the Awake to OnAwake for IndependentGraph
						name = "OnAwake";
						if(modifier == null)
							modifier = new FunctionModifier();
						else
							modifier = SerializerUtility.Duplicate(modifier);
						modifier.SetPublic();
						modifier.Override = true;
					} else if(name == "OnEnable") {
						//Ensure to change the OnEnable to OnEnabled for IndependentGraph
						name = "OnBehaviourEnable";
						if(modifier == null)
							modifier = new FunctionModifier();
						else
							modifier = SerializerUtility.Duplicate(modifier);
						modifier.SetPublic();
						modifier.Override = true;
					}
				}
				if(modifier != null)
					result += modifier.GenerateCode();
				result += type + " " + name;
				if(genericParameters != null && genericParameters.Count > 0) {
					result += "<";
					for(int i = 0; i < genericParameters.Count; i++) {
						if(i != 0)
							result += ", ";
						result += genericParameters[i].name;
					}
					result += ">";
				}
				result += "(";
				if(parameters != null) {
					int index = 0;
					foreach(MPData data in parameters) {
						if(index != 0) {
							result += ", ";
						}
						if(data.refKind != ParameterData.RefKind.None) {
							if(data.refKind == ParameterData.RefKind.Ref) {
								result += "ref ";
							} else if(data.refKind == ParameterData.RefKind.Out) {
								result += "out ";
							}
						}
						result += data.type + " ";
						if(string.IsNullOrEmpty(data.name)) {
							result += "parameter" + index;
						} else {
							result += data.name;
						}
						index++;
					}
				}
				string genericData = null;
				if(genericParameters != null && genericParameters.Count > 0) {
					for(int i = 0; i < genericParameters.Count; i++) {
						if(!string.IsNullOrEmpty(genericParameters[i].type) &&
							!"object".Equals(genericParameters[i].type) &&
							!"System.Object".Equals(genericParameters[i].type)) {
							genericData += "where " + genericParameters[i].name + " : " +
								ParseType(genericParameters[i].type) + " ";
						}
					}
				}
				if(modifier != null && modifier.Abstract) {
					result += ");";
					if(owner != null && includeGraphInformation) {
						result = WrapWithInformation(result, owner);
					}
					return result;
				}
				codeList.Insert(0, new KeyValuePair<int, string>(0, code));
				codeList.Sort((x, y) => string.Compare(x.Key.ToString(), x.Key.ToString(), StringComparison.Ordinal));
				code = null;
				foreach(var pair in codeList) {
					code += pair.Value.AddFirst("\n");
				}
				result += ") " + genericData + "{" + code.AddTabAfterNewLine(1);
				if(string.IsNullOrEmpty(code)) {
					result += "}";
				} else {
					result += "\n}";
				}
				if(!string.IsNullOrEmpty(summary)) {
					result = "/// <summary>".AddLineInEnd() +
						"/// " + summary.Replace("\n", "\n" + "/// ").AddLineInEnd() +
						"/// </summary>" +
						result.AddLineInFirst();
				}
				if(owner != null && includeGraphInformation) {
					result = WrapWithInformation(result, owner);
				}
				return result;
			}

			public MData(string name, string returnType) {
				this.code = "";
				this.name = name;
				this.type = returnType;
			}

			public MData(string name, string returnType, IList<string> parametersType) {
				this.code = "";
				this.name = name;
				this.type = returnType;
				if(parametersType != null && parametersType.Count > 0) {
					List<MPData> pData = new List<MPData>();
					for(int i = 0; i < parametersType.Count; i++) {
						pData.Add(new MPData("parameter" + i, parametersType[i]));
					}
					this.parameters = pData;
				}
			}

			public MData(string name, string returnType, IList<string> parametersType, IList<string> genericParameters = null) {
				this.code = "";
				this.name = name;
				this.type = returnType;
				if(parametersType != null && parametersType.Count > 0) {
					List<MPData> pData = new List<MPData>();
					for(int i = 0; i < parametersType.Count; i++) {
						pData.Add(new MPData("parameter" + i, parametersType[i]));
					}
					this.parameters = pData;
				}
				if(genericParameters != null) {
					this.genericParameters = genericParameters.Select(i => new GPData(i)).ToList();
				}
			}

			public MData(string name, string returnType, IList<MPData> parameters, IList<GPData> genericParameters = null) {
				this.code = "";
				this.name = name;
				this.type = returnType;
				this.parameters = parameters;
				if(genericParameters != null) {
					this.genericParameters = genericParameters;
				}
			}

			public MData(string name, string returnType, string code, IList<MPData> parameters, IList<GPData> genericParameters = null) {
				this.name = name;
				this.type = returnType;
				this.code = code;
				this.parameters = parameters;
				if(genericParameters != null) {
					this.genericParameters = genericParameters;
				}
			}
		}

		public class GeneratedData {
			public Dictionary<uNodeRoot, string> classNames = new Dictionary<uNodeRoot, string>();
			public string fileName {
				get {
					if(graphOwner != null) {
						return graphOwner.name;
					}
					return setting.fileName;
				}
			}
			public string Namespace => setting.nameSpace;

			public GameObject graphOwner;
			public uNodeRoot[] graphs;
			public List<Exception> errors;

			public int graphUID => uNodeUtility.GetObjectID(graphOwner);

			public bool hasError => errors != null && errors.Count > 0;

			public bool isValid => classes != null && !hasError;

			private GeneratorSetting setting;
			private StringBuilder classes;

			public GeneratedData(StringBuilder classes, GeneratorSetting setting) {
				this.classes = classes;
				this.setting = setting;
			}
			
			public void InitOwner() {
				if(graphOwner == null) {
					if(setting.targetData != null) {
						graphOwner = uNodeUtility.GetActualObject(setting.targetData.gameObject);
					} else {
						var obj = setting.graphs.Where(g => g != null).Select(g => g.gameObject).FirstOrDefault();
						graphOwner = uNodeUtility.GetActualObject(obj);
					}
				}
				graphs = setting.graphs.ToArray();
				for(int i = 0; i < graphs.Length; i++) {
					graphs[i] = uNodeUtility.GetActualObject(graphs[i]);
				}
			}

			public int GetSettingUID() {
				return setting.GetSettingUID();
			}

			/// <summary>
			/// Generate Script
			/// </summary>
			/// <returns></returns>
			public string ToScript() {
				if(setting.includeGraphInformation) {
					string str = DoToScript();
					CollectGraphInformations(str, out str);
					return str;
				}
				return DoToScript();
			}

			/// <summary>
			/// Generate Script
			/// </summary>
			/// <returns></returns>
			public string ToScript(out List<ScriptInformation> informations) {
				if(setting.includeGraphInformation) {
					string str = DoToScript();
					informations = CollectGraphInformations(str, out str);
					return str;
				}
				informations = null;
				return DoToScript();
			}

			/// <summary>
			/// Generate Script
			/// </summary>
			/// <returns></returns>
			public string ToScript(out List<ScriptInformation> informations, bool polishInformation) {
				if(setting.includeGraphInformation) {
					string str = DoToScript();
					informations = CollectGraphInformations(str, out str);
					if(polishInformation) {
						PolishInformations(informations);
					}
					return str;
				}
				informations = null;
				return DoToScript();
			}

			/// <summary>
			/// Polish informations for persistance ID so it can be saved to file for future use.
			/// </summary>
			/// <param name="informations"></param>
			public void PolishInformations(List<ScriptInformation> informations) {
				foreach(var info in informations) { 
					if(int.TryParse(info.id, out var id)) {
						if(setting.objectInformations.TryGetValue(id, out var localID)) {
							info.ghostID = info.id;
							info.id = localID.ToString();
						}
					}
				}
			} 

			public string ToRawScript() {
				return DoToScript();
			}
			
			private string DoToScript() {
				string script = classes.ToString();
				StringBuilder builder = new StringBuilder();
				if(setting.scriptHeaders != null) {
					foreach(var header in setting.scriptHeaders) {
						builder.AppendLine(header);
					}
				}
				if(setting.usingNamespace != null) {
					foreach(var ns in setting.usingNamespace) {
						builder.AppendLine("using " + ns + ";");
					}
					builder.AppendLine("");
				}
				if(!string.IsNullOrEmpty(setting.nameSpace)) {
					builder.AppendLine("namespace " + setting.nameSpace + " {");
					builder.Append(script.AddTabAfterNewLine(1, false));
					builder.Append("\n}");
				} else {
					builder.Append(script);
				}

				StringBuilder result = new StringBuilder();
				result.AppendLine(builder.ToString());
				return result.ToString();
			}

			private class GraphInformationToken {
				public string value;
				public int line;
				public int column;

				public bool isStart => value.StartsWith("/*" + KEY_INFORMATION_HEAD, StringComparison.Ordinal);
				public bool isEnd => value.StartsWith("/*" + KEY_INFORMATION_TAIL, StringComparison.Ordinal);

				private string id;

				public string GetID() {
					if(id == null) {
						id = value.RemoveLast(2).Remove(0, 3);
					}
					return id;
				}
			}

			public List<ScriptInformation> CollectGraphInformations(string input, out string output) {
				var strs = input.Split('\n').ToList();
				var information = new List<GraphInformationToken>();
				int addedInformation = 0;
				for (int x = 0; x < strs.Count;x++) {
					string str = strs[x];
					addedInformation = 0;
					string match = null;
					int index = -1;
					for (int y = 0; y < str.Length;y++) {
						char c = str[y];
						match += c;
						if (0 > index) {
							if (c == '/') {
								match = null;
								match += c;
							} else if (match.Length == 3) {
								if (match == "/*" + KEY_INFORMATION_HEAD || match == "/*" + KEY_INFORMATION_TAIL) {
									index = y - 2;
								}
							}
						} else {
							if(c == '/' && match.EndsWith("*/")) {
								addedInformation++;
								information.Add(new GraphInformationToken() {
									value = match,
									line = x,
									column = index,
								});
								str = str.Remove(index, match.Length);
								strs[x] = str;
								if(string.IsNullOrWhiteSpace(str)) {
									for (int i = 1; i - 1 < addedInformation; i++) {
										information[information.Count - i].column = 0;
									}
									strs.RemoveAt(x);
									x--;
									break;
								} else {
									// if(index + 2 > str.Length) {
									// 	information[information.Count - 1].line++;
									// 	information[information.Count - 1].column = 0;
									// }
									y = index - 1;
									match = null;
									index = -1;
								}
							}
						}
					}
				}
				List<ScriptInformation> infos = new List<ScriptInformation>();
				for (int x = 0; x < information.Count;x++) {
					if(information[x].isEnd) continue;
					var startID = information[x].GetID();
					int deep = 0;
					for (int y = x + 1; y < information.Count;y++) {
						var endID = information[y].GetID();
						if(startID == endID) {
							if(information[y].isStart) {
								deep++;
							} else if(information[y].isEnd) {
								deep--;
								if(0 > deep) {
									infos.Add(new ScriptInformation() {
										id = startID,
										startLine = information[x].line,
										startColumn = information[x].column,
										endLine = information[y].line,
										endColumn = information[y].column,
									});
									information.RemoveAt(y);
									break;
								}
							}
						}
					}
				}
				output = string.Join("\n", strs);
				// Debug.Log(input);
				// foreach(var info in infos) {
				// 	Debug.Log($"{info.id} on line {info.startLine} : {info.startColumn} - {info.endLine} : {info.endColumn}");
				// }
				// var lines = input.Split('\n');
				// for (int i = 0;i<lines.Length;i++) {
				// 	Debug.Log($"line {i + 1} : {lines[i]}");
				// }
				return infos;
			}

			/// <summary>
			/// Generate Full Script without any namespace
			/// </summary>
			/// <returns></returns>
			public string FullTypeScript() {
				string script = classes.ToString();
				return script;
			}
		}
	}
}