using System;
using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("Input", "On Keyboard Input")]
	public class EventOnKeyboardInput : BaseGraphEvent {
		public enum ActionState {
			Down,
			Up,
			Hold,
		}
		[ValueIn("Key"), Filter(typeof(KeyCode))]
		public MemberData key = MemberData.CreateFromValue(KeyCode.Space);
		[FieldDrawer("Action")]
		public ActionState action;

		public override void OnRuntimeInitialize() {
			base.OnRuntimeInitialize();
			UEvent.Register(UEventID.Update, owner, OnUpdate);
		}

		void OnUpdate() {
			switch(action) {
				case ActionState.Down:
					if(Input.GetKeyDown(key.Get<KeyCode>())) {
						Trigger();
					}
					break;
				case ActionState.Up:
					if(Input.GetKeyUp(key.Get<KeyCode>())) {
						Trigger();
					}
					break;
				case ActionState.Hold:
					if(Input.GetKey(key.Get<KeyCode>())) {
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
						code = nameof(Input.GetKeyDown);
						break;
					case ActionState.Up:
						code = nameof(Input.GetKeyUp);
						break;
					case ActionState.Hold:
						code = nameof(Input.GetKey);
						break;
					default:
						throw null;
				}
				mData.AddCodeForEvent(CG.If(CG.Invoke(typeof(Input), code, CG.Value(key)), contents));
			}
		}
	}
}