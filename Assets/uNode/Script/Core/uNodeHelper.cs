using UnityEngine;
using UnityEngine.Profiling;
using System;
using System.Collections;
using System.Collections.Generic;
using MaxyGames.uNode;

namespace MaxyGames {
	/// <summary>
	/// Provides useful function.
	/// </summary>
	public static class uNodeHelper {
		/// <summary>
		/// Get the actual runtime object, if the target is uNodeSpawner then get the RuntimeBehaviour
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static object GetActualRuntimeValue(object value) {
			if(value is IRuntimeClassContainer) {
				return (value as IRuntimeClassContainer).RuntimeClass;
			}
			return value;
		}

		/// <summary>
		/// Get UNode Graph Component
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="uniqueIdentifier"></param>
		/// <returns></returns>
		public static uNodeRoot GetGraphComponent(GameObject gameObject, string uniqueIdentifier) {
			var graphs = gameObject.GetComponents<uNodeRoot>();
			foreach(var graph in graphs) {
				if(graph.GraphName == uniqueIdentifier) {
					return graph;
				}
			}
			return null;
		}

		#region GetGeneratedComponent
		/// <summary>
		/// Get Generated Class Component
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		public static T GetGeneratedComponent<T>(this GameObject gameObject) {
			var uniqueIdentifier = typeof(T).Name;
			if(typeof(T).IsInterface) {
				uniqueIdentifier = "i:" + uniqueIdentifier;
			}
			object comp = GetGeneratedComponent(gameObject, uniqueIdentifier);
			if(comp != null) {
				if(comp is T) {
					return (T)comp;
				} else if(comp is IRuntimeClassContainer) {
					var result = (comp as IRuntimeClassContainer).RuntimeClass;
					if(result is T) {
						return (T)result;
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get Generated Class Component
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="component"></param>
		/// <returns></returns>
		public static T GetGeneratedComponent<T>(this Component component) {
			var uniqueIdentifier = typeof(T).Name;
			if(typeof(T).IsInterface) {
				uniqueIdentifier = "i:" + uniqueIdentifier;
			}
			object comp = GetGeneratedComponent(component, uniqueIdentifier);
			if(comp != null) {
				if(comp is T) {
					return (T)comp;
				} else if(comp is IRuntimeClassContainer) {
					var result = (comp as IRuntimeClassContainer).RuntimeClass;
					if(result is T) {
						return (T)result;
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get Generated Class Component
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponent(this GameObject gameObject, RuntimeType type) {
			var comps = gameObject.GetComponents<IRuntimeComponent>();
			foreach(var c in comps) {
				if(type.IsInstanceOfType(c)) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component
		/// </summary>
		/// <param name="component"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponent(this Component component, RuntimeType type) {
			var comps = component.GetComponents<IRuntimeComponent>();
			foreach(var c in comps) {
				if(type.IsInstanceOfType(c)) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}


		/// <summary>
		/// Get Generated Class Component
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponent(this GameObject gameObject, string uniqueID) {
			var comps = gameObject.GetComponents<IRuntimeComponent>();
			if(uniqueID.StartsWith("i:", StringComparison.Ordinal)) {
				uniqueID = uniqueID.Remove(0, 2);
				foreach(var c in comps) {
					if(c.uniqueIdentifier == uniqueID) {
						return c as RuntimeComponent;
					}
					var ifaces = c.GetInterfaces();
					foreach(var iface in ifaces) {
						if(iface.Name == uniqueID) {
							return c as RuntimeComponent;
						}
					}
				}
				return null;
			}
			foreach(var c in comps) {
				if(c.uniqueIdentifier == uniqueID) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component
		/// </summary>
		/// <param name="component"></param>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponent(this Component component, string uniqueID) {
			var comps = component.GetComponents<IRuntimeComponent>();
			if(uniqueID.StartsWith("i:", StringComparison.Ordinal)) {
				uniqueID = uniqueID.Remove(0, 2);
				foreach(var c in comps) {
					if(c.uniqueIdentifier == uniqueID) {
						return c as RuntimeComponent;
					}
					var ifaces = c.GetInterfaces();
					foreach(var iface in ifaces) {
						if(iface.Name == uniqueID) {
							return c as RuntimeComponent;
						}
					}
				}
				return null;
			}
			foreach(var c in comps) {
				if(c.uniqueIdentifier == uniqueID) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}
		#endregion

		#region GetGeneratedComponentInChildren
		/// <summary>
		/// Get Generated Class Component in children
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		public static T GetGeneratedComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false) {
			var uniqueIdentifier = typeof(T).Name;
			if(typeof(T).IsInterface) {
				uniqueIdentifier = "i:" + uniqueIdentifier;
			}
			object comp = GetGeneratedComponentInChildren(gameObject, uniqueIdentifier, includeInactive);
			if(comp != null) {
				if(comp is T) {
					return (T)comp;
				} else if(comp is IRuntimeClassContainer) {
					var result = (comp as IRuntimeClassContainer).RuntimeClass;
					if(result is T) {
						return (T)result;
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get Generated Class Component in children
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <returns></returns>
		public static T GetGeneratedComponentInChildren<T>(this Component component, bool includeInactive = false) {
			var uniqueIdentifier = typeof(T).Name;
			if(typeof(T).IsInterface) {
				uniqueIdentifier = "i:" + uniqueIdentifier;
			}
			object comp = GetGeneratedComponentInChildren(component, uniqueIdentifier, includeInactive);
			if(comp != null) {
				if(comp is T) {
					return (T)comp;
				} else if(comp is IRuntimeClassContainer) {
					var result = (comp as IRuntimeClassContainer).RuntimeClass;
					if(result is T) {
						return (T)result;
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get Generated Class Component in children
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="type"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInChildren(this GameObject gameObject, RuntimeType type, bool includeInactive = false) {
			var comps = gameObject.GetComponentsInChildren<IRuntimeComponent>(includeInactive);
			foreach(var c in comps) {
				if(type.IsInstanceOfType(c)) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}


		/// <summary>
		/// Get Generated Class Component in children
		/// </summary>
		/// <param name="component"></param>
		/// <param name="type"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInChildren(this Component component, RuntimeType type, bool includeInactive = false) {
			var comps = component.GetComponentsInChildren<IRuntimeComponent>(includeInactive);
			foreach(var c in comps) {
				if(type.IsInstanceOfType(c)) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component in children
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="uniqueID"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInChildren(this GameObject gameObject, string uniqueID, bool includeInactive = false) {
			var comps = gameObject.GetComponentsInChildren<IRuntimeComponent>(includeInactive);
			if(uniqueID.StartsWith("i:", StringComparison.Ordinal)) {
				uniqueID = uniqueID.Remove(0, 2);
				foreach(var c in comps) {
					if(c.uniqueIdentifier == uniqueID) {
						return c as RuntimeComponent;
					}
					var ifaces = c.GetInterfaces();
					foreach(var iface in ifaces) {
						if(iface.Name == uniqueID) {
							return c as RuntimeComponent;
						}
					}
				}
				return null;
			}
			foreach(var c in comps) {
				if(c.uniqueIdentifier == uniqueID) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component in children
		/// </summary>
		/// <param name="component"></param>
		/// <param name="uniqueID"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInChildren(this Component component, string uniqueID, bool includeInactive = false) {
			var comps = component.GetComponentsInChildren<IRuntimeComponent>(includeInactive);
			if(uniqueID.StartsWith("i:", StringComparison.Ordinal)) {
				uniqueID = uniqueID.Remove(0, 2);
				foreach(var c in comps) {
					if(c.uniqueIdentifier == uniqueID) {
						return c as RuntimeComponent;
					}
					var ifaces = c.GetInterfaces();
					foreach(var iface in ifaces) {
						if(iface.Name == uniqueID) {
							return c as RuntimeComponent;
						}
					}
				}
				return null;
			}
			foreach(var c in comps) {
				if(c.uniqueIdentifier == uniqueID) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}
		#endregion

		#region GetGeneratedComponentInParent
		/// <summary>
		/// Get Generated Class Component in parent
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="gameObject"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static T GetGeneratedComponentInParent<T>(this GameObject gameObject, bool includeInactive = false) {
			var uniqueIdentifier = typeof(T).Name;
			if(typeof(T).IsInterface) {
				uniqueIdentifier = "i:" + uniqueIdentifier;
			}
			object comp = GetGeneratedComponentInParent(gameObject, uniqueIdentifier, includeInactive);
			if(comp != null) {
				if(comp is T) {
					return (T)comp;
				} else if(comp is IRuntimeClassContainer) {
					var result = (comp as IRuntimeClassContainer).RuntimeClass;
					if(result is T) {
						return (T)result;
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get Generated Class Component in parent
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="component"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static T GetGeneratedComponentInParent<T>(this Component component, bool includeInactive = false) {
			var uniqueIdentifier = typeof(T).Name;
			if(typeof(T).IsInterface) {
				uniqueIdentifier = "i:" + uniqueIdentifier;
			}
			object comp = GetGeneratedComponentInParent(component, uniqueIdentifier, includeInactive);
			if(comp != null) {
				if(comp is T) {
					return (T)comp;
				} else if(comp is IRuntimeClassContainer) {
					var result = (comp as IRuntimeClassContainer).RuntimeClass;
					if(result is T) {
						return (T)result;
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get Generated Class Component in parent
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="type"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInParent(this GameObject gameObject, RuntimeType type, bool includeInactive = false) {
			var comps = gameObject.GetComponentsInParent<IRuntimeComponent>(includeInactive);
			foreach(var c in comps) {
				if(type.IsInstanceOfType(c)) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component in parent
		/// </summary>
		/// <param name="component"></param>
		/// <param name="type"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInParent(this Component component, RuntimeType type, bool includeInactive = false) {
			var comps = component.GetComponentsInParent<IRuntimeComponent>(includeInactive);
			foreach(var c in comps) {
				if(type.IsInstanceOfType(c)) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component in parent
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="uniqueID"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInParent(this GameObject gameObject, string uniqueID, bool includeInactive = false) {
			var comps = gameObject.GetComponentsInParent<IRuntimeComponent>(includeInactive);
			if(uniqueID.StartsWith("i:", StringComparison.Ordinal)) {
				uniqueID = uniqueID.Remove(0, 2);
				foreach(var c in comps) {
					if(c.uniqueIdentifier == uniqueID) {
						return c as RuntimeComponent;
					}
					var ifaces = c.GetInterfaces();
					foreach(var iface in ifaces) {
						if(iface.Name == uniqueID) {
							return c as RuntimeComponent;
						}
					}
				}
				return null;
			}
			foreach(var c in comps) {
				if(c.uniqueIdentifier == uniqueID) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}

		/// <summary>
		/// Get Generated Class Component in parent
		/// </summary>
		/// <param name="component"></param>
		/// <param name="uniqueID"></param>
		/// <param name="includeInactive"></param>
		/// <returns></returns>
		public static RuntimeComponent GetGeneratedComponentInParent(this Component component, string uniqueID, bool includeInactive = false) {
			var comps = component.GetComponentsInParent<IRuntimeComponent>(includeInactive);
			if(uniqueID.StartsWith("i:", StringComparison.Ordinal)) {
				uniqueID = uniqueID.Remove(0, 2);
				foreach(var c in comps) {
					if(c.uniqueIdentifier == uniqueID) {
						return c as RuntimeComponent;
					}
					var ifaces = c.GetInterfaces();
					foreach(var iface in ifaces) {
						if(iface.Name == uniqueID) {
							return c as RuntimeComponent;
						}
					}
				}
				return null;
			}
			foreach(var c in comps) {
				if(c.uniqueIdentifier == uniqueID) {
					return c as RuntimeComponent;
				}
			}
			return null;
		}
		#endregion

		/// <summary>
		/// GetComponentInParent including inactive object
		/// </summary>
		/// <param name="gameObject"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetComponentInParent<T>(GameObject gameObject) {
			if(gameObject == null) return default;
			return GetComponentInParent<T>(gameObject.transform);
		}

		/// <summary>
		/// GetComponentInParent including inactive object
		/// </summary>
		/// <param name="transform"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetComponentInParent<T>(Component component) {
			if(component == null) return default;
			Transform parent = component.transform;
			while(parent != null) {
				var comp = parent.GetComponent<T>();
				if(comp != null) {
					return comp;
				}
				parent = parent.parent;
			}
			return default;
		}

		#region Runtimes
		public static void InitializeRuntimeFunction(IGraphWithUnityEvent graph) {
			var root = graph as uNodeRoot;
			string name = root.GraphName;
			var func = graph as IFunctionSystem;
			{
				uNodeFunction function = func.GetFunction("Awake");
				if(function != null) {
					graph.onAwake += delegate () {
						Profiler.BeginSample(name + "." + "Awake");
						function.Invoke(null, null);
						Profiler.EndSample();
					};
				}
			}
			{
				uNodeFunction function = func.GetFunction("Start");
				if(function != null) {
					graph.onStart += delegate () {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".Start");
						function.Invoke(null, null);
						Profiler.EndSample();
					};
				}
			}
			{
				uNodeFunction function = func.GetFunction("Update");
				if(function != null) {
					UEvent.Register(UEventID.Update, root, () => {
						Profiler.BeginSample(name + ".Update");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("FixedUpdate");
				if(function != null) {
					UEvent.Register(UEventID.FixedUpdate, root, () => {
						Profiler.BeginSample(name + ".FixedUpdate");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("LateUpdate");
				if(function != null) {
					UEvent.Register(UEventID.LateUpdate, root, () => {
						Profiler.BeginSample(name + ".LateUpdate");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnAnimatorIK", typeof(int));
				if(function != null) {
					UEvent.Register(UEventID.OnAnimatorIK, root, (int param1) => {
						Profiler.BeginSample(name + ".OnAnimatorIK");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnAnimatorMove");
				if(function != null) {
					UEvent.Register(UEventID.OnAnimatorMove, root, () => {
						Profiler.BeginSample(name + ".OnAnimatorMove");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnApplicationFocus", typeof(bool));
				if(function != null) {
					UEvent.Register(UEventID.OnApplicationFocus, root, (bool param1) => {
						Profiler.BeginSample(name + ".OnApplicationFocus");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnApplicationPause", typeof(bool));
				if(function != null) {
					UEvent.Register(UEventID.OnApplicationPause, root, (bool param1) => {
						Profiler.BeginSample(name + ".OnApplicationPause");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnApplicationQuit");
				if(function != null) {
					UEvent.Register(UEventID.OnApplicationQuit, root, () => {
						Profiler.BeginSample(name + ".OnApplicationQuit");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnBecameInvisible");
				if(function != null) {
					UEvent.Register(UEventID.OnBecameInvisible, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnBecameInvisible");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnBecameVisible");
				if(function != null) {
					UEvent.Register(UEventID.OnBecameVisible, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnBecameVisible");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnCollisionEnter", typeof(Collision));
				if(function != null) {
					UEvent.Register(UEventID.OnCollisionEnter, root, (Collision param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnCollisionEnter");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnCollisionEnter2D", typeof(Collision2D));
				if(function != null) {
					UEvent.Register(UEventID.OnCollisionEnter2D, root, (Collision2D param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnCollisionEnter2D");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnCollisionExit", typeof(Collision));
				if(function != null) {
					UEvent.Register(UEventID.OnCollisionExit, root, (Collision param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnCollisionExit");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnCollisionExit2D", typeof(Collision2D));
				if(function != null) {
					UEvent.Register(UEventID.OnCollisionExit2D, root, (Collision2D param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnCollisionExit2D");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnCollisionStay", typeof(Collision));
				if(function != null) {
					UEvent.Register(UEventID.OnCollisionStay, root, (Collision param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnCollisionStay");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnCollisionStay2D", typeof(Collision2D));
				if(function != null) {
					UEvent.Register(UEventID.OnCollisionStay2D, root, (Collision2D param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnCollisionStay2D");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnDestroy");
				if(function != null) {
					graph.onDestroy += delegate () {
						Profiler.BeginSample(name + ".OnDestroy");
						function.Invoke(null, null);
						Profiler.EndSample();
					};
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnDisable");
				if(function != null) {
					graph.onDisable += delegate () {
						Profiler.BeginSample(name + ".OnDisable");
						function.Invoke(null, null);
						Profiler.EndSample();
					};
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnEnable");
				if(function != null) {
					graph.onEnable += delegate () {
						Profiler.BeginSample(name + ".OnEnable");
						function.Invoke(null, null);
						Profiler.EndSample();
					};
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnGUI");
				if(function != null) {
					UEvent.Register(UEventID.OnGUI, root, () => {
						Profiler.BeginSample(name + ".OnGUI");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseDown");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseDown, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseDown");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseDrag");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseDrag, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseDrag");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseEnter");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseEnter, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseEnter");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseExit");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseExit, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseExit");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseOver");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseOver, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseOver");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseUp");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseUp, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseUp");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnMouseUpAsButton");
				if(function != null) {
					UEvent.Register(UEventID.OnMouseUpAsButton, root, () => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnMouseUpAsButton");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnParticleCollision", typeof(GameObject));
				if(function != null) {
					UEvent.Register(UEventID.OnParticleCollision, root, (GameObject param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnParticleCollision");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnPostRender");
				if(function != null) {
					UEvent.Register(UEventID.OnPostRender, root, () => {
						Profiler.BeginSample(name + ".OnPostRender");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnPreCull");
				if(function != null) {
					UEvent.Register(UEventID.OnPreCull, root, () => {
						Profiler.BeginSample(name + ".OnPreCull");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnPreRender");
				if(function != null) {
					UEvent.Register(UEventID.OnPreRender, root, () => {
						Profiler.BeginSample(name + ".OnPreRender");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnRenderObject");
				if(function != null) {
					UEvent.Register(UEventID.OnRenderObject, root, () => {
						Profiler.BeginSample(name + ".OnRenderObject");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTransformChildrenChanged");
				if(function != null) {
					UEvent.Register(UEventID.OnTransformChildrenChanged, root, () => {
						Profiler.BeginSample(name + ".OnTransformChildrenChanged");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTransformParentChanged");
				if(function != null) {
					UEvent.Register(UEventID.OnTransformParentChanged, root, () => {
						Profiler.BeginSample(name + ".OnTransformParentChanged");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTriggerEnter", typeof(Collider));
				if(function != null) {
					UEvent.Register(UEventID.OnTriggerEnter, root, (Collider param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnTriggerEnter");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTriggerEnter2D", typeof(Collider2D));
				if(function != null) {
					UEvent.Register(UEventID.OnTriggerEnter2D, root, (Collider2D param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnTriggerEnter2D");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTriggerExit", typeof(Collider));
				if(function != null) {
					UEvent.Register(UEventID.OnTriggerExit, root, (Collider param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnTriggerExit");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTriggerExit2D", typeof(Collider2D));
				if(function != null) {
					UEvent.Register(UEventID.OnTriggerExit2D, root, (Collider2D param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnTriggerExit2D");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTriggerStay", typeof(Collider));
				if(function != null) {
					UEvent.Register(UEventID.OnTriggerStay, root, (Collider param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnTriggerStay");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnTriggerStay2D", typeof(Collider2D));
				if(function != null) {
					UEvent.Register(UEventID.OnTriggerStay2D, root, (Collider2D param1) => {
						System.Type type = function.ReturnType();
						if(type != null && type != typeof(void) && (type.IsCastableTo(typeof(IEnumerable)) || type.IsCastableTo(typeof(IEnumerator)))) {
							function.owner.StartCoroutine(function.Invoke(null, null) as IEnumerator);
							return;
						}
						Profiler.BeginSample(name + ".OnTriggerStay2D");
						function.Invoke(new object[] { param1 }, null);
						Profiler.EndSample();
					});
				}
			}
			{
				uNodeFunction function = func.GetFunction("OnWillRenderObject");
				if(function != null) {
					UEvent.Register(UEventID.OnWillRenderObject, root, () => {
						Profiler.BeginSample(name + ".OnWillRenderObject");
						function.Invoke(null, null);
						Profiler.EndSample();
					});
				}
			}
			//Initialize Function Calers	
		}
		#endregion

		/// <summary>
		/// Set value for the object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		/// <param name="operator"></param>
		/// <returns></returns>
		public static T SetObject<T>(T obj, object value, char @operator) {
			switch(@operator) {
				case '+':
					if(value == null) {
						throw new ArgumentNullException(nameof(value));
					}
					value = Operator.Add(obj, value, typeof(T), value.GetType());
					break;
				case '-':
					if(value == null) {
						throw new ArgumentNullException(nameof(value));
					}
					value = Operator.Subtract(obj, value, typeof(T), value.GetType());
					break;
				case '/':
					if(value == null) {
						throw new ArgumentNullException(nameof(value));
					}
					value = Operator.Divide(obj, value, typeof(T), value.GetType());
					break;
				case '*':
					if(value == null) {
						throw new ArgumentNullException(nameof(value));
					}
					value = Operator.Multiply(obj, value, typeof(T), value.GetType());
					break;
				case '%':
					if(value == null) {
						throw new ArgumentNullException(nameof(value));
					}
					value = Operator.Modulo(obj, value, typeof(T), value.GetType());
					break;
			}
			if(value != null) {
				return (T)value;
			}
			return default;
		}

		/// <summary>
		/// Set value for the object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reference"></param>
		/// <param name="value"></param>
		/// <param name="setType"></param>
		/// <returns></returns>
		public static T SetObject<T>(T reference, object value, SetType setType) {
			switch(setType) {
				case SetType.Change:
					return value != null ? (T)value : default;
				case SetType.Add:
					return (T)ArithmeticOperator(reference, value, ArithmeticType.Add);
				case SetType.Subtract:
					return (T)ArithmeticOperator(reference, value, ArithmeticType.Subtract);
				case SetType.Divide:
					return (T)ArithmeticOperator(reference, value, ArithmeticType.Divide);
				case SetType.Multiply:
					return (T)ArithmeticOperator(reference, value, ArithmeticType.Multiply);
			}
			throw new InvalidOperationException();
		}

		/// <summary>
		/// Set value for the object.
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="value"></param>
		/// <param name="setType"></param>
		public static void SetObject(ref object reference, object value, SetType setType) {
			switch(setType) {
				case SetType.Change:
					reference = value;
					break;
				case SetType.Add:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Add);
					break;
				case SetType.Subtract:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Subtract);
					break;
				case SetType.Divide:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Divide);
					break;
				case SetType.Multiply:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Multiply);
					break;
			}
		}

		/// <summary>
		/// Set value for the object.
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="value"></param>
		/// <param name="setType"></param>
		/// <returns></returns>
		public static object SetObject(object reference, object value, SetType setType) {
			switch(setType) {
				case SetType.Change:
					reference = value;
					break;
				case SetType.Add:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Add);
					break;
				case SetType.Subtract:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Subtract);
					break;
				case SetType.Divide:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Divide);
					break;
				case SetType.Multiply:
					reference = ArithmeticOperator(reference, value, ArithmeticType.Multiply);
					break;
			}
			return reference;
		}

		public static bool OperatorComparison(object a, object b, ComparisonType operatorType) {
			if(a != null && b != null) {
				if(a is Enum && b is Enum) {
					a = Operator.Convert(a, Enum.GetUnderlyingType(a.GetType()));
					b = Operator.Convert(b, Enum.GetUnderlyingType(b.GetType()));
				}
				switch(operatorType) {
					case ComparisonType.Equal:
						return Operator.Equal(a, b, a.GetType(), b.GetType());
					case ComparisonType.NotEqual:
						return Operator.NotEqual(a, b, a.GetType(), b.GetType());
					case ComparisonType.GreaterThan:
						return Operator.GreaterThan(a, b, a.GetType(), b.GetType());
					case ComparisonType.LessThan:
						return Operator.LessThan(a, b, a.GetType(), b.GetType());
					case ComparisonType.GreaterThanOrEqual:
						return Operator.GreaterThanOrEqual(a, b, a.GetType(), b.GetType());
					case ComparisonType.LessThanOrEqual:
						return Operator.LessThanOrEqual(a, b, a.GetType(), b.GetType());
					default:
						throw new System.InvalidCastException();
				}
			} else {
				switch(operatorType) {
					case ComparisonType.Equal:
						return Operator.Equal(a, b);
					case ComparisonType.NotEqual:
						return Operator.NotEqual(a, b);
					case ComparisonType.GreaterThan:
						return Operator.GreaterThan(a, b);
					case ComparisonType.LessThan:
						return Operator.LessThan(a, b);
					case ComparisonType.GreaterThanOrEqual:
						return Operator.GreaterThanOrEqual(a, b);
					case ComparisonType.LessThanOrEqual:
						return Operator.LessThanOrEqual(a, b);
					default:
						throw new System.InvalidCastException();
				}
			}
		}

		public static bool OperatorComparison(object a, object b, ComparisonType operatorType, Type aType, Type bType) {
			if(a is Enum && b is Enum) {
				a = Operator.Convert(a, Enum.GetUnderlyingType(a.GetType()));
				b = Operator.Convert(b, Enum.GetUnderlyingType(b.GetType()));
			}
			switch(operatorType) {
				case ComparisonType.Equal:
					return Operator.Equal(a, b, aType, bType);
				case ComparisonType.NotEqual:
					return Operator.NotEqual(a, b, aType, bType);
				case ComparisonType.GreaterThan:
					return Operator.GreaterThan(a, b, aType, bType);
				case ComparisonType.LessThan:
					return Operator.LessThan(a, b, aType, bType);
				case ComparisonType.GreaterThanOrEqual:
					return Operator.GreaterThanOrEqual(a, b, aType, bType);
				case ComparisonType.LessThanOrEqual:
					return Operator.LessThanOrEqual(a, b, aType, bType);
				default:
					throw new System.InvalidCastException();
			}
		}

		public static object ShiftOperator(object a, int b, ShiftType operatorType) {
			switch(operatorType) {
				case ShiftType.LeftShift:
					return Operators.LeftShift(a, b, a.GetType());
				case ShiftType.RightShift:
					return Operators.RightShift(a, b, a.GetType());
				default:
					throw new System.InvalidCastException();
			}
		}

		public static object BitwiseOperator(object a, object b, BitwiseType operatorType) {
			switch(operatorType) {
				case BitwiseType.And:
					return Operators.And(a, b);
				case BitwiseType.Or:
					return Operators.Or(a, b);
				case BitwiseType.ExclusiveOr:
					return Operators.ExclusiveOr(a, b);
				default:
					throw new System.InvalidCastException();
			}
		}

		public static object ArithmeticOperator(object a, object b, ArithmeticType operatorType) {
			switch(operatorType) {
				case ArithmeticType.Add:
					return Operator.Add(a, b);
				case ArithmeticType.Subtract:
					return Operator.Subtract(a, b);
				case ArithmeticType.Divide:
					return Operator.Divide(a, b);
				case ArithmeticType.Multiply:
					return Operator.Multiply(a, b);
				case ArithmeticType.Modulo:
					return Operator.Modulo(a, b);
				default:
					throw new System.InvalidCastException();
			}
		}

		public static object ArithmeticOperator(object a, object b, ArithmeticType operatorType, Type aType, Type bType) {
			if(aType == null) {
				aType = typeof(object);
			}
			if(bType == null) {
				bType = aType;
			}
			switch(operatorType) {
				case ArithmeticType.Add:
					return Operator.Add(a, b, aType, bType);
				case ArithmeticType.Subtract:
					return Operator.Subtract(a, b, aType, bType);
				case ArithmeticType.Divide:
					return Operator.Divide(a, b, aType, bType);
				case ArithmeticType.Multiply:
					return Operator.Multiply(a, b, aType, bType);
				case ArithmeticType.Modulo:
					return Operator.Modulo(a, b, aType, bType);
				default:
					throw new System.InvalidCastException();
			}
		}

		public static object ArithmeticOperator(object a, object b, char operatorCode, Type aType, Type bType) {
			if(aType == null) {
				aType = a?.GetType() ?? bType ?? b?.GetType();
			}
			if(bType == null) {
				bType = aType;
			}
			switch(operatorCode) {
				case '+':
					return Operator.Add(a, b, aType, bType);
				case '-':
					return Operator.Subtract(a, b, aType, bType);
				case '/':
					return Operator.Divide(a, b, aType, bType);
				case '*':
					return Operator.Multiply(a, b, aType, bType);
				case '%':
					return Operator.Modulo(a, b, aType, bType);
				default:
					throw new System.InvalidCastException();
			}
		}
	}
}