using UnityEngine;
using System.Collections;
using MaxyGames.uNode;

namespace MaxyGames.Runtime {
	[AddComponentMenu("")]
	public class RuntimeSMHelper : MonoBehaviour {
		private static RuntimeSMHelper _instance;
		public static RuntimeSMHelper Instance {
			get {
				if(_instance == null) {
					_instance = FindObjectOfType<RuntimeSMHelper>();
					if(_instance == null) {
						GameObject go = new GameObject("Helper");
						_instance = go.AddComponent<RuntimeSMHelper>();
					}
				}
				return _instance;
			}
		}
	}

	/// <summary>
	/// Provides function for debug uNode.
	/// Note: this only exist in editor.
	/// </summary>
	public static class uNodeDEBUG {
		public static System.Action<object, int, int, object> invokeValueNode;
		public static System.Action<EventCoroutine, int, int> InvokeEvent;
		public static System.Action<object, int, int, int> InvokeFlowNode;
		public static System.Action<object, int, int, bool?> InvokeEventNode;
		public static System.Action<object, int, int> InvokeTransition;

		public static void InvokeValueNode(object owner, int objectUID, int nodeUID, int valueID, object value, bool isSet = false) {
			invokeValueNode(owner, objectUID, nodeUID, new object[] { valueID, value, isSet });
		}
	}
}