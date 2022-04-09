using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("uNode/uNode Spawner")]
	public class uNodeSpawner : RuntimeComponent, IVariableSystem, IRuntimeComponent, IRuntimeClassContainer {
		/// <summary>
		/// The target reference object
		/// </summary>
		[Tooltip("The target reference object")]
		public uNodeRoot target;

		/// <summary>
		/// The main object of uNode, null mean self target
		/// </summary>
		/// <value></value>
		public GameObject mainObject { get; protected set; }

		/// <summary>
		/// The list of variable.
		/// </summary>
		[HideInInspector, SerializeField]
		protected List<VariableData> variable = new List<VariableData>();

		public uNodeRuntime runtimeInstance { get; private set; }
		public RuntimeBehaviour runtimeBehaviour { get; private set; }

		IRuntimeClass IRuntimeClassContainer.RuntimeClass {
			get {
				EnsureInitialized();
				if(runtimeBehaviour != null) {
					return runtimeBehaviour;
				}
				return runtimeInstance;
			}
		}

		protected bool hasInitialize = false;

		public string uniqueIdentifier => target != null ? target.GraphName : string.Empty;

		IEnumerable<Type> IRuntimeInterface.GetInterfaces() {
			if(target is IInterfaceSystem iface) {
				return iface.Interfaces.Select(i => i.Get<Type>());
			}
			return Type.EmptyTypes;
		}

		public RuntimeComponent GetRuntimeInstance() {
			EnsureInitialized();
			return runtimeBehaviour ?? this as RuntimeComponent;
		}

		#region Variables
		List<VariableData> IVariableSystem.Variables {
			get {
				return variable;
			}
		}

		VariableData IVariableSystem.GetVariableData(string name) {
			return uNodeUtility.GetVariableData(name, variable);
		}
		#endregion

		public void EnsureInitialized() {
			if(hasInitialize) return;
			if(!Application.isPlaying) {
				throw new System.Exception("Can't initialize event when not playing.");
			} else if(target == null) {
				throw new Exception("Target graph can't be null");
			}
			hasInitialize = true;
			if(target is uNodeComponentSingleton singleton) {
				bool flag = singleton.EnsureInitialized(false);
				runtimeBehaviour = singleton.runtimeBehaviour;
				if(flag && runtimeBehaviour != null) {
					//Initialize the variable
					for(int i = 0; i < target.Variables.Count; i++) {
						VariableData var = target.Variables[i];
						for(int x = 0; x < variable.Count; x++) {
							if(var.Name.Equals(variable[x].Name)) {
								var = variable[x];
								break;
							}
						}
						SetVariable(var.Name, SerializerUtility.Duplicate(var.value));
					}
					runtimeBehaviour.OnAwake();
					runtimeBehaviour.enabled = enabled;
				}
				runtimeInstance = singleton.runtimeInstance;
				if(flag && runtimeInstance != null) {
					//Initialize the variable
					for(int i = 0; i < target.Variables.Count; i++) {
						VariableData var = target.Variables[i];
						for(int x = 0; x < variable.Count; x++) {
							if(var.Name.Equals(variable[x].Name)) {
								var = variable[x];
								break;
							}
						}
						SetVariable(var.Name, SerializerUtility.Duplicate(var.value));
					}
					variable = runtimeInstance.Variables;
					runtimeInstance.Initialize();
					runtimeInstance.enabled = enabled;
				}
				return;
			}
			var type = target.GeneratedTypeName.ToType(false);
            if(type != null) {
				//Instance native c# graph, native graph will call Awake immediately
				runtimeBehaviour = gameObject.AddComponent(type) as RuntimeBehaviour;
				runtimeBehaviour.hideFlags = HideFlags.HideInInspector;
				//Initialize the references
				var references = target.graphData.unityObjects;
				for (int i = 0; i < references.Count;i++) {
					SetVariable(references[i].name, references[i].value);
				}
				//Initialize the variable
				for(int i = 0; i < target.Variables.Count; i++) {
					VariableData var = target.Variables[i];
					for(int x = 0; x < variable.Count; x++) {
						if(var.Name.Equals(variable[x].Name)) {
							var = variable[x];
							break;
						}
					}
					SetVariable(var.Name, SerializerUtility.Duplicate(var.value));
				}
				//Call awake
				runtimeBehaviour.OnAwake();
				runtimeBehaviour.enabled = enabled;
			} else {
				//Instance reflection graph
				var main = RuntimeGraphManager.InstantiateGraph(target, gameObject, variable);
				variable = main.Variables;
				runtimeInstance = main;
				main.Initialize();//Initialize the graph, so it will call Awake after created.
				main.manualHandlingEvent = true;
			}
		}

		void Awake() {
			EnsureInitialized();
		}

		void Start() {
			if(runtimeInstance != null && runtimeInstance.manualHandlingEvent) {
				if(runtimeInstance is IGraphWithUnityEvent graph) {
					try {
						graph.onStart?.Invoke();
					}
					catch(System.Exception ex) {
						if(ex is uNodeException) {
							throw;
						}
						throw uNodeDebug.LogException(ex, this);
					}
				}
			}
		}

		void OnEnable() {
			if(runtimeBehaviour != null) {
				runtimeBehaviour.enabled = true;
				runtimeBehaviour.OnBehaviourEnable();
			} else if(runtimeInstance != null) {
				runtimeInstance.enabled = true;
				if(runtimeInstance.manualHandlingEvent) {
					runtimeInstance.DoOnEnable();
				}
			}
		}

		void OnDisable() {
			if(target is uNodeComponentSingleton singleton && singleton.IsPersistence) {
				//If the target graph is a persistence singleton then we don't need to disable it as it might cause a bugs.
				return;
			}
			if(runtimeBehaviour != null) {
				runtimeBehaviour.enabled = false;
			} else if(runtimeInstance != null) {
				runtimeInstance.enabled = false;
			}
		}

		// void Start() {
		// 	// Initialize();
		// }

		/// <summary>
		/// Execute function and event by name without parameter
		/// </summary>
		/// <param name="Name"></param>
		public void ExecuteEvent(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.ExecuteFunction(Name);
			} else runtimeInstance.ExecuteFunction(Name);
		}

		public override object InvokeFunction(string Name, object[] values) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.InvokeFunction(Name, values);
			}
			return runtimeInstance.InvokeFunction(Name, values);
		}

		public override object InvokeFunction(string Name, Type[] parameters, object[] values) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.InvokeFunction(Name, parameters, values);
			}
			return runtimeInstance.InvokeFunction(Name, parameters, values);
		}

		public override void SetVariable(string Name, object value) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetVariable(Name, value);
			} else runtimeInstance.SetVariable(Name, value);
		}

		public override void SetVariable(string Name, object value, char @operator) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetVariable(Name, value, @operator);
			} else runtimeInstance.SetVariable(Name, value, @operator);
		}

		public override object GetVariable(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.GetVariable(Name);
			}
			return runtimeInstance.GetVariable(Name);
		}

		public override T GetVariable<T>(string Name) {
			EnsureInitialized();
			if (runtimeBehaviour != null) {
				return runtimeBehaviour.GetVariable<T>(Name);
			}
			return runtimeInstance.GetVariable<T>(Name);
		}

		public override void SetProperty(string Name, object value) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetProperty(Name, value);
			} else runtimeInstance.SetProperty(Name, value);
		}
		
		public override void SetProperty(string Name, object value, char @operator) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetProperty(Name, value, @operator);
			} else {
				runtimeInstance.SetProperty(Name, value, @operator);
			}
		}

		public override object GetProperty(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.GetProperty(Name);
			}
			return runtimeInstance.GetProperty(Name);
		}

		public override T GetProperty<T>(string Name) {
			EnsureInitialized();
			if (runtimeBehaviour != null) {
				return runtimeBehaviour.GetProperty<T>(Name);
			}
			return runtimeInstance.GetProperty<T>(Name);
		}
	}
}