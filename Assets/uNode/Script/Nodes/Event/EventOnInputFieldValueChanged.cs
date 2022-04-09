using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Input Field Value Changed")]
	public class EventOnInputFieldValueChanged : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(InputField), typeof(GameObject))]
		public MemberData target = MemberData.none;
		[ValueOut]
		public string value;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<string>(UEventID.OnInputFieldValueChanged, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<string>(UEventID.OnInputFieldValueChanged, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<string>(UEventID.OnInputFieldValueChanged, owner, OnTriggered);
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
				string parameter;
				if(CG.CanDeclareLocal(this, nameof(value), GetFlows())) {
					parameter = CG.GetOutputName(this, nameof(value));
				} else {
					parameter = CG.GenerateVariableName("parameter", this);
					CG.RegisterInstanceVariable(this, nameof(value));
					contents = CG.Flow(
						CG.Set(CG.GetOutputName(this, nameof(value)), parameter),
						contents
					);
				}

				if(target.isAssigned) {
					mData.AddCodeForEvent(
						CG.FlowInvoke(
							typeof(UEvent),
							nameof(UEvent.Register),
							CG.Value(UEventID.OnInputFieldValueChanged),
							CG.Value(target),
							CG.Lambda(new[] { typeof(string) }, new[] { parameter }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<InputField>(CG.This, nameof(GameObject.GetComponent))
							.CGAccess(nameof(InputField.onValueChanged))
							.CGFlowInvoke(
								nameof(InputField.onValueChanged.AddListener),
								CG.Lambda(new[] { typeof(string) }, new[] { parameter }, contents)
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