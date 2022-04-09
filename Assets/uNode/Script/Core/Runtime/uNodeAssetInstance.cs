using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	public class uNodeAssetInstance : BaseRuntimeAsset, IVariableSystem, IRuntimeAsset, ICustomIcon, IRuntimeClassContainer {
		/// <summary>
		/// The target reference object
		/// </summary>
		[Tooltip("The target reference object")]
		public uNodeClassAsset target;
		/// <summary>
		/// The list of variable.
		/// </summary>
		public List<VariableData> variable = new List<VariableData>();

		public uNodeRuntime runtimeInstance { get; private set; }
		public RuntimeAsset runtimeAsset { get; private set; }

		IRuntimeClass IRuntimeClassContainer.RuntimeClass {
			get {
				return GetRuntimeInstance();
			}
		}

		private bool hasInitialize => runtimeAsset != null || runtimeInstance != null;

		public string uniqueIdentifier => target != null ? target.GraphName : string.Empty;

		IEnumerable<Type> IRuntimeInterface.GetInterfaces() {
			var iface = target.Interfaces;
			if(iface != null) {
				return iface.Select(i => i.Get<Type>());
			}
			return Type.EmptyTypes;
		}

		#region Variables
		List<VariableData> IVariableSystem.Variables {
			get {
				if(runtimeInstance != null) {
					return runtimeInstance.Variables;
				}
				return variable;
			}
		}

		VariableData IVariableSystem.GetVariableData(string name) {
			return uNodeUtility.GetVariableData(name, variable);
		}
		#endregion

		public void Initialize() {
			if(!Application.isPlaying) {
				throw new System.Exception("Can't initialize when the aplication is not playing.");
			}
			if(target == null) {
				Debug.LogError("Missing graph references, please re-assign it.", this);
				throw new System.Exception("Target graph can't be null");
			}
			var type = target.GeneratedTypeName.ToType(false);
            if(type != null) {
				runtimeAsset = ScriptableObject.CreateInstance(type) as RuntimeAsset;
				for(int i = 0; i < target.Variables.Count; i++) {
					VariableData var = target.Variables[i];
					for(int x = 0; x < variable.Count; x++) {
						if(var.Name.Equals(variable[x].Name)) {
							SetVariable(var.Name, variable[x].value);
						}
					}
				}
			} else {
				var mainObject = new GameObject(name);
				mainObject.SetActive(false);

				uNodeRoot graph = Instantiate(target);
				uNodeRuntime main = mainObject.AddComponent<uNodeRuntime>();
				main.originalGraph = target;
				main.Name = target.GraphName;
				main.Variables = graph.Variables;
				main.RootObject = graph.RootObject;
				main.RootObject.transform.SetParent(mainObject.transform);
				AnalizerUtility.RetargetNodeOwner(graph, main, main.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
				main.Refresh();
				Destroy(graph.gameObject);
				for(int i = 0; i < main.Variables.Count; i++) {
					VariableData var = main.Variables[i];
					for(int x = 0; x < variable.Count; x++) {
						if(var.Name.Equals(variable[x].Name)) {
							main.Variables[i] = new VariableData(variable[x]);
						}
					}
				}
				//Uncomment this to prevent resetting variable you're exiting play mode
				// this.variable = main.variable;
				this.runtimeInstance = main;
				GameObject.DontDestroyOnLoad(mainObject);
			}
		}

		public BaseRuntimeAsset GetRuntimeInstance() {
			if (!hasInitialize) {
				Initialize();
			}
			return runtimeAsset ?? this as BaseRuntimeAsset;
		}
		
		public override object InvokeFunction(string Name, object[] values) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				return runtimeAsset.InvokeFunction(Name, values);
			}
			return runtimeInstance.InvokeFunction(Name, values);
		}

		public override object InvokeFunction(string Name, Type[] parameters, object[] values) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				return runtimeAsset.InvokeFunction(Name, parameters, values);
			}
			return runtimeInstance.InvokeFunction(Name, parameters, values);
		}

		public override void SetVariable(string Name, object value) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				runtimeAsset.SetVariable(Name, value);
			} else runtimeInstance.SetVariable(Name, value);
		}

		public override void SetVariable(string Name, object value, char @operator) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				runtimeAsset.SetVariable(Name, value, @operator);
			} else {
				runtimeInstance.SetVariable(Name, value, @operator);
			}
		}

		public override object GetVariable(string Name) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				return runtimeAsset.GetVariable(Name);
			}
			return runtimeInstance.GetVariable(Name);
		}

		public override T GetVariable<T>(string Name) {
			if (!hasInitialize) {
				Initialize();
			}
			if (runtimeAsset != null) {
				return runtimeAsset.GetVariable<T>(Name);
			}
			return runtimeInstance.GetVariable<T>(Name);
		}

		public override void SetProperty(string Name, object value) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				runtimeAsset.SetProperty(Name, value);
			} else runtimeInstance.SetProperty(Name, value);
		}
		
		public override void SetProperty(string Name, object value, char @operator) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				runtimeAsset.SetProperty(Name, value, @operator);
			} else {
				runtimeInstance.SetProperty(Name, value, @operator);
			}
		}

		public override object GetProperty(string Name) {
			if (!hasInitialize) {
				Initialize();
			}
			if(runtimeAsset != null) {
				return runtimeAsset.GetProperty(Name);
			}
			return runtimeInstance.GetProperty(Name);
		}

		public override T GetProperty<T>(string Name) {
			if (!hasInitialize) {
				Initialize();
			}
			if (runtimeAsset != null) {
				return runtimeAsset.GetProperty<T>(Name);
			}
			return runtimeInstance.GetProperty<T>(Name);
		}

		public Texture GetIcon() {
			return target?.GetIcon();
		}

		public override string ToString() {
			if(runtimeAsset != null) {
				return runtimeInstance.ToString();
			} else if(runtimeInstance != null) {
				return runtimeInstance.ToString();
			}
			return base.ToString();
		}
	}
}