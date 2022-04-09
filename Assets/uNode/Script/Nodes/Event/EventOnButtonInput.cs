using System;
using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("Input", "On Button Input")]
	public class EventOnButtonInput : BaseGraphEvent {
		public enum ActionState {
			Down,
			Up,
			Hold,
		}
		[ValueIn("Name"), Filter(typeof(string))]
		public MemberData buttonName = MemberData.CreateFromValue("");
		[FieldDrawer("Action")]
		public ActionState action;

		public override void OnRuntimeInitialize() {
			UEvent.Register(UEventID.Update, owner, OnUpdate);
		}

		void OnUpdate() {
			switch(action) {
				case ActionState.Down:
					if(Input.GetButtonDown(buttonName.Get<string>())) {
						Trigger();
					}
					break;
				case ActionState.Up:
					if(Input.GetButtonUp(buttonName.Get<string>())) {
						Trigger();
					}
					break;
				case ActionState.Hold:
					if(Input.GetButton(buttonName.Get<string>())) {
						Trigger();
					}
					break;
			}
		}

		public override void GenerateCode() {
			var mData = CG.GetOrRegisterFunction(UEventID.Update, typeof(void));
			var contents = GenerateFlows();
			if(!string.IsNullOrEmpty(contents)) {
				string code;
				switch(action) {
					case ActionState.Down:
						code = nameof(Input.GetButtonDown);
						break;
					case ActionState.Up:
						code = nameof(Input.GetButtonUp);
						break;
					case ActionState.Hold:
						code = nameof(Input.GetButton);
						break;
					default:
						throw null;
				}
				mData.AddCodeForEvent(CG.If(CG.Invoke(typeof(Input), code, CG.Value(buttonName)), contents));
			}
		}
	}
}