using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	[CustomEditor(typeof(StateEventNode), true)]
	class StateEventNodeEditor : Editor {
		public override void OnInspectorGUI() {
			StateEventNode comp = target as StateEventNode;
			EditorGUI.BeginChangeCheck();
			DrawDefaultInspector();
			switch(comp.eventType) {
				case StateEventNode.EventType.OnAnimatorIK:
					uNodeGUIUtility.ShowField(
						new GUIContent("Store Parameter"),
						"storeParameter",
						comp,
						new object[] { new FilterAttribute(typeof(int)) { SetMember = true } },
						comp);
					break;
				case StateEventNode.EventType.OnApplicationFocus:
				case StateEventNode.EventType.OnApplicationPause:
					uNodeGUIUtility.ShowField(
						new GUIContent("Store Parameter"),
						"storeParameter",
						comp,
						new object[] { new FilterAttribute(typeof(bool)) { SetMember = true } },
						comp);
					break;
				case StateEventNode.EventType.OnCollisionEnter:
				case StateEventNode.EventType.OnCollisionExit:
				case StateEventNode.EventType.OnCollisionStay:
					uNodeGUIUtility.ShowField(
						new GUIContent("Store Collision"),
						"storeParameter",
						comp,
						new object[] { new FilterAttribute(typeof(Collision)) { SetMember = true } },
						comp);
					break;
				case StateEventNode.EventType.OnCollisionEnter2D:
				case StateEventNode.EventType.OnCollisionExit2D:
				case StateEventNode.EventType.OnCollisionStay2D:
					uNodeGUIUtility.ShowField(
						new GUIContent("Store Collision2D"),
						"storeParameter",
						comp,
						new object[] { new FilterAttribute(typeof(Collision2D)) { SetMember = true } },
						comp);
					break;
				case StateEventNode.EventType.OnTriggerEnter:
				case StateEventNode.EventType.OnTriggerExit:
				case StateEventNode.EventType.OnTriggerStay:
					uNodeGUIUtility.ShowField(
						new GUIContent("Store Collider"),
						"storeParameter",
						comp,
						new object[] { new FilterAttribute(typeof(Collider)) { SetMember = true } },
						comp);
					break;
				case StateEventNode.EventType.OnTriggerEnter2D:
				case StateEventNode.EventType.OnTriggerExit2D:
				case StateEventNode.EventType.OnTriggerStay2D:
					uNodeGUIUtility.ShowField(
						new GUIContent("Store Collider2D"),
						"storeParameter",
						comp,
						new object[] { new FilterAttribute(typeof(Collider2D)) { SetMember = true } },
						comp);
					break;
				default:
					if(comp.storeParameter.isAssigned) {
						comp.storeParameter = MemberData.none;
					}
					break;
			}
			if(EditorGUI.EndChangeCheck()) {
				uNodeGUIUtility.GUIChanged(comp);
			}
		}
	}
}