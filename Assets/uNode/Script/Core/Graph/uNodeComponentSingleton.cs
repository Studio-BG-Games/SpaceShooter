using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	[GraphSystem("Component Singleton", order = 200,
		inherithFrom = typeof(RuntimeBehaviour),
		supportAttribute = false,
		supportGeneric = false,
		supportModifier = false,
		supportConstructor = false,
		allowAutoCompile = true,
		allowCompileToScript = false)]
	public class uNodeComponentSingleton : uNodeClassComponent, ISingletonGraph, IRuntimeClassContainer {
		[SerializeField, Tooltip(
@"True: the graph will be persistence mean that the graph will not be destroyed on Load a scene this is useful for global settings so the value is persistence between scenes.

False: The graph will be destroyed on Loading a scene, this usefull for Scene Management so every enter new scene the value will be reset to default value")]
		protected bool persistence;

		public bool IsPersistence => persistence;
		public RuntimeBehaviour runtimeBehaviour { get; private set; }
		public uNodeRuntime runtimeInstance { get; private set; }

		public IRuntimeClass RuntimeClass {
			get {
				EnsureInitialized();
				if(runtimeBehaviour != null) {
					return runtimeBehaviour;
				}
				return runtimeInstance;
			}
		}

		IEnumerable<Type> IRuntimeInterface.GetInterfaces() {
			var iface = Interfaces;
			if(iface != null) {
				return iface.Select(i => i.Get<Type>());
			}
			return Type.EmptyTypes;
		}

		/// <summary>
		/// Ensure the singleton is initialized, return false if singleton has been initialized.
		/// </summary>
		/// <param name="callAwake"></param>
		/// <returns></returns>
		public bool EnsureInitialized(bool callAwake = true) {
			if(runtimeInstance != null || runtimeBehaviour != null) return false;
			#if UNITY_EDITOR
			if(!Application.isPlaying) {
				throw new Exception("Singleton graph can only be run in playmode");
			}
			#endif
			var type = GeneratedTypeName.ToType(false);
            if(type != null) {
				runtimeBehaviour = uNodeSingleton.GetNativeGraph(this);
				if(callAwake) {
					runtimeBehaviour.OnAwake();//Call awake
					runtimeBehaviour.OnBehaviourEnable();//Call enable
				}
			} else {
				runtimeInstance = uNodeSingleton.GetRuntimeGraph(this);
				if(callAwake)
					runtimeInstance.Initialize();//Initialize singleton and call Awake function
			}
			return true;
		}

		public void ExecuteFunction(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.InvokeFunction(Name, null);
				return;
			}
			runtimeInstance.ExecuteFunction(Name);
		}

		public object GetProperty(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.GetProperty(Name);
			}
			return runtimeInstance.GetProperty(Name);
		}

		public T GetProperty<T>(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.GetProperty<T>(Name);
			}
			return runtimeInstance.GetProperty<T>(Name);
		}

		public object GetVariable(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.GetVariable(Name);
			}
			return runtimeInstance.GetVariable(Name);
		}

		public T GetVariable<T>(string Name) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.GetVariable<T>(Name);
			}
			return runtimeInstance.GetVariable<T>(Name);
		}

		public object InvokeFunction(string Name, object[] values) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.InvokeFunction(Name, values);
			}
			return runtimeInstance.InvokeFunction(Name, values);
		}

		public object InvokeFunction(string Name, Type[] parameters, object[] values) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				return runtimeBehaviour.InvokeFunction(Name, parameters, values);
			}
			return runtimeInstance.InvokeFunction(Name, parameters, values);
		}

		public void SetProperty(string Name, object value) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetProperty(Name, value);
				return;
			}
			runtimeInstance.SetProperty(Name, value);
		}

		public void SetVariable(string Name, object value) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetVariable(Name, value);
				return;
			}
			runtimeInstance.SetVariable(Name, value);
		}

		public void SetVariable(string Name, object value, char @operator) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetVariable(Name, value, @operator);
				return;
			}
			runtimeInstance.SetVariable(Name, value);
		}

		public void SetProperty(string Name, object value, char @operator) {
			EnsureInitialized();
			if(runtimeBehaviour != null) {
				runtimeBehaviour.SetProperty(Name, value, @operator);
				return;
			}
			runtimeInstance.SetProperty(Name, value, @operator);
		}
	}
}