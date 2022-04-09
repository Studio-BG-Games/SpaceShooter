using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode {
	public static class GraphDebug {
		#region Classes
		public struct DebugValue {
			public bool isSet;
			public float time;
			public object value;

			public bool isValid => time > 0;
		}

		/// <summary>
		/// Class that contains Debug data
		/// </summary>
		public class DebugData {
			/// <summary>
			/// Class for save node debug data.
			/// </summary>
			public class NodeDebug {
				private StateType _state;
				public StateType nodeState {
					get {
						if(customCondition != null) {
							return customCondition();
						}
						return _state;
					}
					set {
						_state = value;
					}
				}
				public float calledTime;
				public float breakpointTimes;
				public bool isTransitionRunning;
				public Func<StateType> customCondition;
				public object nodeValue;
			}
			/// <summary>
			/// Class for save value debug data.
			/// </summary>
			public class ValueDebug {
				public float calledTime;
				public bool isSetValue;
				public object value;
			}
			/// <summary>
			/// Data for debug the node
			/// </summary>
			public Dictionary<int, NodeDebug> nodeDebug = new Dictionary<int, NodeDebug>();
			/// <summary>
			/// Data to debug Node Transition
			/// </summary>
			public Dictionary<int, float> transitionDebug = new Dictionary<int, float>();
			/// <summary>
			/// Data to debug ValueNode Transition
			/// </summary>
			public Dictionary<int, Dictionary<int, ValueDebug>> valueNodeDebug = new Dictionary<int, Dictionary<int, ValueDebug>>();
			public Dictionary<int, Dictionary<string, ValueDebug>> valueOutputDebug = new Dictionary<int, Dictionary<string, ValueDebug>>();
			public Dictionary<int, Dictionary<string, ValueDebug>> valueFieldDebug = new Dictionary<int, Dictionary<string, ValueDebug>>();
			/// <summary>
			/// Data to debug EventNode that target another Node.
			/// </summary>
			public Dictionary<int, Dictionary<int, float>> flowTransitionDebug = new Dictionary<int, Dictionary<int, float>>();
			/// <summary>
			/// Data to debug FlowInput connection.
			/// </summary>
			public Dictionary<int, Dictionary<string, float>> flowInputDebug = new Dictionary<int, Dictionary<string, float>>();

			public DebugValue GetDebugValue(MemberData member) {
				switch(member.targetType) {
					case MemberData.TargetType.FlowInput:
					case MemberData.TargetType.FlowNode: {
						float times = -1;
						if(member.targetType == MemberData.TargetType.FlowInput) {
							int ID = uNodeUtility.GetObjectID(member.startTarget as Object);
							if(debugData != null && flowInputDebug.ContainsKey(ID)) {
								if(flowInputDebug[ID].ContainsKey(member.startName)) {
									times = flowInputDebug[ID][member.startName];
								}
							}
						} else {
							int ID = uNodeUtility.GetObjectID(member.startTarget as Object);
							if(debugData != null && flowTransitionDebug.ContainsKey(ID)) {
								if(flowTransitionDebug[ID].ContainsKey(int.Parse(member.startName))) {
									times = flowTransitionDebug[ID][int.Parse(member.startName)];
								}
							}
						}
						return new DebugValue() {
							time = times
						};
					}
					case MemberData.TargetType.ValueNode: {
						int ID = uNodeUtility.GetObjectID(member.startTarget as Object);
						if(debugData != null && valueNodeDebug.TryGetValue(ID, out var map)) {
							if(map.TryGetValue(int.Parse(member.startName), out var vData)) {
								return new DebugValue() {
									isSet = vData.isSetValue,
									time = vData.calledTime,
									value = vData.value,
								};
							}
						}
						break;
					}
					case MemberData.TargetType.NodeField: {
						int ID = uNodeUtility.GetObjectID(member.startTarget as Object);
						if(debugData != null && valueFieldDebug.TryGetValue(ID, out var map)) {
							if(map.TryGetValue(member.startName, out var vData)) {
								return new DebugValue() {
									isSet = vData.isSetValue,
									time = vData.calledTime,
									value = vData.value,
								};
							}
						}
						break;
					}
					case MemberData.TargetType.NodeFieldElement: {

						break;
					}
					case MemberData.TargetType.NodeOutputValue: {
						int ID = uNodeUtility.GetObjectID(member.startTarget as Object);
						if(debugData != null && valueOutputDebug.TryGetValue(ID, out var map)) {
							if(map.TryGetValue(member.startName, out var vData)) {
								return new DebugValue() {
									isSet = vData.isSetValue,
									time = vData.calledTime,
									value = vData.value,
								};
							}
						}
						break;
					}
				}
				return default;
			}
		}
		#endregion

		public static Dictionary<int, Dictionary<object, DebugData>> debugData = new Dictionary<int, Dictionary<object, DebugData>>();
		/// <summary>
		/// Are debug mode is on.
		/// </summary>
		public static bool useDebug = true;
		/// <summary>
		/// The timer for debug.
		/// </summary>
		public static float debugLinesTimer;
		/// <summary>
		/// The callback for HasBreakpoint, this will filled from uNodeEditorInitializer.
		/// </summary>
		public static Func<int, bool> hasBreakpoint;
		/// <summary>
		/// The callback for AddBreakpoint, this will filled from uNodeEditorInitializer.
		/// </summary>
		public static Action<int> addBreakpoint;
		/// <summary>
		/// The callback for RemoveBreakpoint, this will filled from uNodeEditorInitializer.
		/// </summary>
		public static Action<int> removeBreakpoint;

		/// <summary>
		/// Are the node has breakpoint.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <returns></returns>
		public static bool HasBreakpoint(int nodeID) {
			if(hasBreakpoint == null) {
#if UNITY_EDITOR
				throw new Exception("uNode is not initialized");
#else
				return false;
#endif
			}
			return hasBreakpoint(nodeID);
		}

		/// <summary>
		/// Add breakpoint to node.
		/// </summary>
		/// <param name="nodeID"></param>
		public static void AddBreakpoint(int nodeID) {
			if(addBreakpoint == null) {
#if UNITY_EDITOR
				throw new Exception("uNode is not initialized");
#else
				return;
#endif
			}
			addBreakpoint(nodeID);
		}

		/// <summary>
		/// Remove breakpoint from node.
		/// </summary>
		/// <param name="nodeID"></param>
		public static void RemoveBreakpoint(int nodeID) {
			if(removeBreakpoint == null) {
#if UNITY_EDITOR
				throw new Exception("uNode is not initialized");
#else
				return;
#endif
			}
			removeBreakpoint(nodeID);
		}

		/// <summary>
		/// Call this function to debug EventNode that using value node.
		/// </summary>
		public static void ValueNode(object owner, int objectUID, int nodeUID, int valueID, object value, bool isSet = false) {
			if(!useDebug || !Application.isPlaying)
				return;
			Dictionary<object, DebugData> debugMap = null;
			if(!debugData.TryGetValue(objectUID, out debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			DebugData data = null;
			if(!debugMap.TryGetValue(owner, out data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			Dictionary<int, DebugData.ValueDebug> map = null;
			if(!data.valueNodeDebug.TryGetValue(nodeUID, out map)) {
				map = new Dictionary<int, DebugData.ValueDebug>();
				data.valueNodeDebug[nodeUID] = map;
			}
			map[valueID] = new DebugData.ValueDebug() {
				calledTime = Time.unscaledTime,
				value = value,
				isSetValue = isSet,
			};
		}

		public static void ValueField(object owner, int objectUID, int nodeUID, string valueID, object value) {
			if(!useDebug || !Application.isPlaying)
				return;
			if(!debugData.TryGetValue(objectUID, out var debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			if(!debugMap.TryGetValue(owner, out var data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			if(!data.valueFieldDebug.TryGetValue(nodeUID, out var map)) {
				map = new Dictionary<string, DebugData.ValueDebug>();
				data.valueFieldDebug[nodeUID] = map;
			}
			map[valueID] = new DebugData.ValueDebug() {
				calledTime = Time.unscaledTime,
				value = value,
				isSetValue = false,
			};
		}

		public static void ValueOutput(object owner, int objectUID, int nodeUID, string valueID, object value) {
			if(!useDebug || !Application.isPlaying)
				return;
			if(!debugData.TryGetValue(objectUID, out var debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			if(!debugMap.TryGetValue(owner, out var data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			if(!data.valueOutputDebug.TryGetValue(nodeUID, out var map)) {
				map = new Dictionary<string, DebugData.ValueDebug>();
				data.valueOutputDebug[nodeUID] = map;
			}
			map[valueID] = new DebugData.ValueDebug() {
				calledTime = Time.unscaledTime,
				value = value,
				isSetValue = false,
			};
		}

		/// <summary>
		/// Call this function to debug the node.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="objectUID"></param>
		/// <param name="nodeUID"></param>
		/// <param name="state">true : success, false : failure, null : running</param>
		public static void FlowNode(object owner, int objectUID, int nodeUID, bool? state) {
			FlowNode(owner, objectUID, nodeUID, state == null ? StateType.Running : (state.Value ? StateType.Success : StateType.Failure));
		}


		/// <summary>
		/// Call this function to debug the node.
		/// </summary>
		public static void FlowNode(object owner, int objectUID, int nodeUID, StateType state) {
			if(!useDebug || !Application.isPlaying)
				return;
			Dictionary<object, DebugData> debugMap = null;
			if(!debugData.TryGetValue(objectUID, out debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			DebugData data = null;
			if(!debugMap.TryGetValue(owner, out data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			DebugData.NodeDebug nodeDebug = null;
			if(!data.nodeDebug.TryGetValue(nodeUID, out nodeDebug)) {
				nodeDebug = new DebugData.NodeDebug();
				data.nodeDebug[nodeUID] = nodeDebug;
			}
			nodeDebug.calledTime = Time.unscaledTime;
			nodeDebug.nodeState = state;
			if(HasBreakpoint(nodeUID)) {
				nodeDebug.breakpointTimes = Time.unscaledTime;
				Debug.Break();
			}
		}

		/// <summary>
		/// Call this function to debug Flow node.
		/// </summary>
		public static void FlowTransition(object owner, int objectUID, int nodeUID, int valueID) {
			if(!useDebug || !Application.isPlaying)
				return;
			Dictionary<object, DebugData> debugMap = null;
			if(!debugData.TryGetValue(objectUID, out debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			DebugData data = null;
			if(!debugMap.TryGetValue(owner, out data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			Dictionary<int, float> map = null;
			if(!data.flowTransitionDebug.TryGetValue(nodeUID, out map)) {
				map = new Dictionary<int, float>();
				data.flowTransitionDebug[nodeUID] = map;
			}
			map[valueID] = Time.unscaledTime;
		}

		/// <summary>
		/// Call this function to debug Flow node.
		/// </summary>
		public static void FlowTransition(object owner, int objectUID, int nodeUID, string flowName) {
			if(!useDebug || !Application.isPlaying)
				return;
			Dictionary<object, DebugData> debugMap = null;
			if(!debugData.TryGetValue(objectUID, out debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			DebugData data = null;
			if(!debugMap.TryGetValue(owner, out data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			Dictionary<string, float> map = null;
			if(!data.flowInputDebug.TryGetValue(nodeUID, out map)) {
				map = new Dictionary<string, float>();
				data.flowInputDebug[nodeUID] = map;
			}
			map[flowName] = Time.unscaledTime;
		}

		/// <summary>
		/// Call this function to debug the node transition.
		/// </summary>
		public static void Transition(object owner, int objectUID, int transitionUID) {
			if(!useDebug || !Application.isPlaying)
				return;
			Dictionary<object, DebugData> debugMap = null;
			if(!debugData.TryGetValue(objectUID, out debugMap)) {
				debugMap = new Dictionary<object, DebugData>();
				debugData.Add(objectUID, debugMap);
			}
			DebugData data = null;
			if(!debugMap.TryGetValue(owner, out data)) {
				data = new DebugData();
				debugMap.Add(owner, data);
			}
			data.transitionDebug[transitionUID] = Time.unscaledTime;
		}
	}
}