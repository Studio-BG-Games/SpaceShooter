using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Dropdown Value Changed")]
	public class EventOnDropdownValueChanged : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(Dropdown), typeof(GameObject))]
		public MemberData target = MemberData.none;
		[ValueOut("Index"), Hide]
		public int index;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register<int>(UEventID.OnDropdownValueChanged, obj as GameObject, OnTriggered);
				} else if(obj is Component) {
					UEvent.Register<int>(UEventID.OnDropdownValueChanged, obj as Component, OnTriggered);
				}
			} else {
				UEvent.Register<int>(UEventID.OnDropdownValueChanged, owner, OnTriggered);
			}
		}

		void OnTriggered(int index) {
			this.index = index;
			Trigger();
		}

		public override void GenerateCode() {
			var mData = CG.GetOrRegisterFunction("Start", typeof(void));
			var contents = GenerateFlows();
			if(!string.IsNullOrEmpty(contents)) {
				string parameter;
				if(CG.CanDeclareLocal(this, nameof(index), GetFlows())) {
					parameter = CG.GetOutputName(this, nameof(index));
				} else {
					parameter = CG.GenerateVariableName("parameter", this);
					CG.RegisterInstanceVariable(this, nameof(index));
					contents = CG.Flow(
						CG.Set(CG.GetOutputName(this, nameof(index)), parameter), 
						contents
					);
				}
				if(target.isAssigned) {
					mData.AddCodeForEvent(
						CG.FlowInvoke(
							typeof(UEvent),
							nameof(UEvent.Register),
							CG.Value(UEventID.OnDropdownValueChanged),
							CG.Value(target),
							CG.Lambda(new[] { typeof(int) }, new[] { parameter }, contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<Dropdown>(CG.This, nameof(GameObject.GetComponent))
							.CGAccess(nameof(Dropdown.onValueChanged))
							.CGFlowInvoke(
								nameof(Dropdown.onValueChanged.AddListener), 
								CG.Lambda(new[] { typeof(int) }, new[] { parameter }, contents)
							)
					);
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(Dropdown);
		}
	}
}