using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Pointer Down")]
	public class EventOnPointerDown : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(GameObject), typeof(Component))]
		public MemberData target = MemberData.none;
		[ValueOut("Value"), Hide]
		public PointerEventData value;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<PointerEventData>(UEventID.OnPointerDown, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<PointerEventData>(UEventID.OnPointerDown, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<PointerEventData>(UEventID.OnPointerDown, owner, OnTriggered);
			}
		}

		void OnTriggered(PointerEventData value) {
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
							CG.Value(UEventID.OnPointerDown),
							CG.Value(target),
							CG.Lambda(new[] { typeof(PointerEventData) }, new[] { parameter }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.FlowInvoke(
							typeof(UEvent),
							nameof(UEvent.Register),
							CG.Value(UEventID.OnPointerDown),
							CG.This,
							CG.Lambda(new[] { typeof(PointerEventData) }, new[] { parameter }, contents)
						)
					);
				}
			}
		}
	}
}