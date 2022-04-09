using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Scroll Rect Value Changed")]
	public class EventOnScrollRectValueChanged : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(ScrollRect), typeof(GameObject))]
		public MemberData target = MemberData.none;
		[ValueOut("Value"), Hide]
		public Vector2 value;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<Vector2>(UEventID.OnScrollRectValueChanged, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<Vector2>(UEventID.OnScrollRectValueChanged, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<Vector2>(UEventID.OnScrollRectValueChanged, owner, OnTriggered);
			}
		}

		void OnTriggered(Vector2 value) {
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
							CG.Value(UEventID.OnScrollRectValueChanged),
							CG.Value(target),
							CG.Lambda(new[] { typeof(Vector2) }, new[] { parameter }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<ScrollRect>(CG.This, nameof(GameObject.GetComponent))
							.CGAccess(nameof(ScrollRect.onValueChanged))
							.CGFlowInvoke(
								nameof(ScrollRect.onValueChanged.AddListener), 
								CG.Lambda(new[] { typeof(Vector2) }, new[] { parameter }, contents)
							)
					);
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(ScrollRect);
		}
	}
}