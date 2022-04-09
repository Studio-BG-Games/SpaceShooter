using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Slider Value Changed")]
	public class EventOnSliderValueChanged : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(Slider), typeof(GameObject))]
		public MemberData target = MemberData.none;
		[ValueOut("Value"), Hide]
		public float value;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<float>(UEventID.OnSliderValueChanged, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<float>(UEventID.OnSliderValueChanged, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<float>(UEventID.OnSliderValueChanged, owner, OnTriggered);
			}
		}

		void OnTriggered(float value) {
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
							CG.Value(UEventID.OnSliderValueChanged),
							CG.Value(target),
							CG.Lambda(new[] { typeof(float) }, new[] { parameter }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<Slider>(CG.This, nameof(GameObject.GetComponent))
							.CGAccess(nameof(Slider.onValueChanged))
							.CGFlowInvoke(
								nameof(Slider.onValueChanged.AddListener), 
								CG.Lambda(new[] { typeof(float) }, new[] { parameter }, contents)
							)
					);
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(Slider);
		}
	}
}