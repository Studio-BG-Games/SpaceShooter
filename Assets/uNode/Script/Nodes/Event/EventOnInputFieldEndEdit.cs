using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Input Field End Edit")]
	public class EventOnInputFieldEndEdit : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(InputField), typeof(GameObject))]
		public MemberData target = MemberData.none;
		[ValueOut]
		public string value;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<string>(UEventID.OnInputFieldEndEdit, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<string>(UEventID.OnInputFieldEndEdit, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<string>(UEventID.OnInputFieldEndEdit, owner, OnTriggered);
			}
		}

		void OnTriggered(string value) {
			this.value = value;
			Trigger();
		}

		public override void GenerateCode() {
			var mData = CG.GetOrRegisterFunction("Start", typeof(void));
			var contents = GenerateFlows();
			if(!string.IsNullOrEmpty(contents)) {
				if(target.isAssigned) {
					mData.AddCodeForEvent(
						CG.FlowInvoke(
							typeof(UEvent),
							nameof(UEvent.Register),
							CG.Value(UEventID.OnInputFieldEndEdit),
							CG.Value(target),
							CG.Lambda(new[] { typeof(string) }, new[] { CG.GetOutputName(this, nameof(value)) }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<InputField>(CG.This, nameof(GameObject.GetComponent))
							.CGAccess(nameof(InputField.onEndEdit))
							.CGFlowInvoke(
								nameof(InputField.onEndEdit.AddListener), 
								CG.Lambda(new[] { typeof(string) }, new[] { CG.GetOutputName(this, nameof(value)) }, contents)
							)
					);
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(InputField);
		}
	}
}