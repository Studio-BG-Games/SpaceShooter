using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("GUI", "On Button Click")]
	public class EventOnButtonClick : BaseGraphEvent {
		[ValueIn("Target"), Filter(typeof(Button), typeof(GameObject))]
		public MemberData target = MemberData.none;

		public override void OnRuntimeInitialize() {
			var obj = target.Get();
			if(obj != null) {
				if(obj is GameObject) {
					UEvent.Register(UEventID.OnButtonClick, obj as GameObject, OnClick);
				} else if(obj is Component) {
					UEvent.Register(UEventID.OnButtonClick, obj as Component, OnClick);
				}
			} else {
				UEvent.Register(UEventID.OnButtonClick, owner, OnClick);
			}
		}

		void OnClick() {
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
							CG.Value(UEventID.OnButtonClick),
							CG.Value(target),
							CG.Lambda(contents)
						)
					);
				} else {
					mData.AddCodeForEvent(
						CG.GenericInvoke<Button>(CG.This, nameof(GameObject.GetComponent)).CGAccess(nameof(Button.onClick)).CGFlowInvoke(nameof(Button.onClick.AddListener), CG.Lambda(contents))
					);
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(Button);
		}
	}
}