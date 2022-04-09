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
		public const string KEY_INFORMATION_HEAD = "@";
		public const string KEY_INFORMATION_TAIL = "#";
		public const string KEY_INFORMATION_VARIABLE = "V:";

		/// <summary>
		/// Wrap 'input' string with information of 'obj' so uNode can suggest what is the object that generates the code.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string WrapWithInformation(string input, object obj) {
			if(!string.IsNullOrWhiteSpace(input)) {
				int firstIndex = 0;
				int lastIndex = input.Length;
				for (int i = 0; i < input.Length;i++) {
					if(!char.IsWhiteSpace(input[i])) {
						firstIndex = i;
						break;
					}
				}
				for (int i = input.Length - 1; i > 0; i--) {
					if(!char.IsWhiteSpace(input[i])) {
						lastIndex = i + 1;
						break;
					}
				}
				return input.Add(lastIndex, EndGenerateInformation(obj)).Add(firstIndex, BeginGenerateInformation(obj));
			}
			return null;
			// return input.AddFirst(BeginGenerateInformation(obj)).Add(EndGenerateInformation(obj));
		}

		static string BeginGenerateInformation(object obj) {
			if(obj is UnityEngine.Object o) {
				return Comment(o.GetInstanceID().ToString().AddFirst(KEY_INFORMATION_HEAD));
			} else if(obj is VariableData) {
				return Comment((obj as VariableData).Name.AddFirst(KEY_INFORMATION_HEAD + KEY_INFORMATION_VARIABLE));
			}
			return null;
		}

		static string EndGenerateInformation(object obj) {
			if(obj is UnityEngine.Object) {
				return Comment((obj as UnityEngine.Object).GetInstanceID().ToString().AddFirst(KEY_INFORMATION_TAIL));
			} else if(obj is VariableData) {
				return Comment((obj as VariableData).Name.AddFirst(KEY_INFORMATION_TAIL + KEY_INFORMATION_VARIABLE));
			}
			return null;
		}

		/// <summary>
		/// Generate debug code.
		/// </summary>
		/// <param name="comp"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public static string Debug(NodeComponent comp, StateType state) {
			string data = setting.debugPreprocessor ? "\n#if UNITY_EDITOR" : "";
			string s = state == StateType.Success ? "true" : (state == StateType.Failure ? "false" : "null");
			data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.FlowNode),
				"this",
				Value(uNodeUtility.GetObjectID(graph)),
				Value(uNodeUtility.GetObjectID(comp)),
				s).AddLineInFirst();
			if(setting.debugPreprocessor)
				data += "#endif".AddLineInFirst();
			return data;
		}

		/// <summary>
		/// Generate debug code.
		/// </summary>
		/// <param name="valueNode"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Debug(Node valueNode, string value) {
			string data = setting.debugPreprocessor ? "\n#if UNITY_EDITOR" : "";
			data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.ValueNode),
				"this",
				Value(uNodeUtility.GetObjectID(graph)),
				Value(uNodeUtility.GetObjectID(valueNode)),
				value).AddLineInFirst();
			if(setting.debugPreprocessor)
				data += "#endif".AddLineInFirst();
			return data;
		}

		/// <summary>
		/// Generate debug code.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static string Debug(MemberData member) {
			if(!member.isAssigned)
				return null;
			if(member.targetType != MemberData.TargetType.FlowNode &&
				member.targetType != MemberData.TargetType.ValueNode &&
				member.targetType != MemberData.TargetType.FlowInput)
				return null;
			string data = setting.debugPreprocessor ? "\n#if UNITY_EDITOR" : "";
			if(member.targetType == MemberData.TargetType.FlowNode) {
				data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.FlowTransition),
					"this",
					Value(uNodeUtility.GetObjectID(graph)),
					Value(uNodeUtility.GetObjectID(member.GetInstance() as UnityEngine.Object)),
					Value(int.Parse(member.startName))).AddLineInFirst();
			} else if(member.targetType == MemberData.TargetType.FlowInput) {
				data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.FlowTransition),
					"this",
					Value(uNodeUtility.GetObjectID(graph)),
					Value(uNodeUtility.GetObjectID(member.GetInstance() as UnityEngine.Object)),
					Value(member.startName)).AddLineInFirst();
			} else {
				throw new System.NotSupportedException($"Target type:{member.targetType} is not supported to generate debug code");
			}
			if(setting.debugPreprocessor)
				data += "#endif".AddLineInFirst();
			return data;
		}

		/// <summary>
		/// Generate debug code.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <param name="isSet"></param>
		/// <returns></returns>
		public static string Debug(MemberData member, string value, bool isSet = false) {
			if(!member.isAssigned)
				return null;
			if(member.targetType != MemberData.TargetType.FlowNode &&
				member.targetType != MemberData.TargetType.ValueNode &&
				member.targetType != MemberData.TargetType.FlowInput)
				return null;
			string data = setting.debugPreprocessor ? "\n#if UNITY_EDITOR" : "";
			if(member.targetType == MemberData.TargetType.FlowNode) {
				data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.FlowTransition),
					"this",
					Value(uNodeUtility.GetObjectID(graph)),
					Value(uNodeUtility.GetObjectID(member.GetInstance() as UnityEngine.Object)),
					Value(int.Parse(member.startName))).AddLineInFirst();
			} else if(member.targetType == MemberData.TargetType.ValueNode) {
				data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.ValueNode),
					"this",
					Value(uNodeUtility.GetObjectID(graph)),
					Value(uNodeUtility.GetObjectID(member.GetInstance() as UnityEngine.Object)),
					Value(int.Parse(member.startName)),
					value,
					Value(isSet)).AddLineInFirst();
			} else {
				throw new System.NotSupportedException("Target type is not supported to generate debug code");
			}
			if(setting.debugPreprocessor)
				data += "#endif".AddLineInFirst();
			return data;
		}

		/// <summary>
		/// Generate debug code.
		/// </summary>
		/// <param name="comp"></param>
		/// <param name="transition"></param>
		/// <returns></returns>
		public static string Debug(NodeComponent comp, TransitionEvent transition) {
			string data = setting.debugPreprocessor ? "\n#if UNITY_EDITOR" : "";
			data += FlowInvoke(typeof(GraphDebug), nameof(GraphDebug.Transition),
				"this",
				Value(uNodeUtility.GetObjectID(graph)),
				Value(uNodeUtility.GetObjectID(transition))).AddLineInFirst();
			if(setting.debugPreprocessor)
				data += "#endif".AddLineInFirst();
			return data;
		}
	}
}