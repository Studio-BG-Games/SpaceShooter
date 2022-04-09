using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Toggle Value Changed")]
	public class EventOnToggleValueChanged : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(Toggle), typeof(GameObject))]
		public MemberData target = MemberData.none;
		[ValueOut("Value"), Hide]
		public bool value;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<bool>(UEventID.OnToggleValueChanged, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<bool>(UEventID.OnToggleValueChanged, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<bool>(UEventID.OnToggleValueChanged, owner, OnTriggered);
			}
		}

		void OnTriggered(bool value) {
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
							CG.Value(UEventID.OnToggleValueChanged),
							CG.Value(target),
							CG.Lambda(new[] { typeof(bool) }, new[] { parameter }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<Toggle>(CG.This, nameof(GameObject.GetComponent))
							.CGAccess(nameof(Toggle.onValueChanged))
							.CGFlowInvoke(
								nameof(Toggle.onValueChanged.AddListener), 
								CG.Lambda(new[] { typeof(bool) }, new[] { parameter }, contents)
							)
					);
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(Toggle);
		}
	}
}