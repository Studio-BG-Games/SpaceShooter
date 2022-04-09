using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Input.GetMouseButton", hideOnBlock = true)]
	public class InputGetMouseButton : AnyBlock {
		public enum GetMouseButtonType {
			GetMouseButton,
			GetMouseButtonDown,
			GetMouseButtonUp,
		}
		public GetMouseButtonType getMouseButtonType;
		[ObjectType(typeof(int))]
		public MemberData button = new MemberData(0);
		[Filter(typeof(bool), SetMember = true)]
		public MemberData storeValue;

		private bool condition;

		protected override void OnExecute() {
			if(button.isAssigned && storeValue.isAssigned) {
				storeValue.Set(GetButton(button.Get<int>()));
			}
		}

		protected override bool OnValidate() {
			if(button.isAssigned) {
				condition = GetButton(button.Get<int>());
				if(storeValue.isAssigned) {
					storeValue.Set(condition);
				}
				return condition;
			}
			return true;
		}

		public bool GetButton(int buttonIndex) {
			if(getMouseButtonType == GetMouseButtonType.GetMouseButton) {
				return Input.GetMouseButton(buttonIndex);
			} else if(getMouseButtonType == GetMouseButtonType.GetMouseButtonDown) {
				return Input.GetMouseButtonDown(buttonIndex);
			} else if(getMouseButtonType == GetMouseButtonType.GetMouseButtonUp) {
				return Input.GetMouseButtonUp(buttonIndex);
			}
			return false;
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			if(getMouseButtonType == GetMouseButtonType.GetMouseButton) {
				data = CG.FlowInvoke(typeof(Input), "GetMouseButton", button.CGValue());
			} else if(getMouseButtonType == GetMouseButtonType.GetMouseButtonDown) {
				data = CG.FlowInvoke(typeof(Input), "GetMouseButtonDown", button.CGValue());
			} else if(getMouseButtonType == GetMouseButtonType.GetMouseButtonUp) {
				data = CG.FlowInvoke(typeof(Input), "GetMouseButtonUp", button.CGValue());
			}
			if(storeValue.isAssigned) {
				return CG.Value((object)storeValue) + " = " + data;
			}
			return data;
		}

		public override string GenerateConditionCode(Object obj) {
			return GenerateCode(obj).RemoveSemicolon();
		}

		public override string GetDescription() {
			if(getMouseButtonType == GetMouseButtonType.GetMouseButton) {
				return "Returns whether the given mouse button is held down.";
			} else if(getMouseButtonType == GetMouseButtonType.GetMouseButtonDown) {
				return "Returns true during the frame the user pressed the given mouse button.";
			} else if(getMouseButtonType == GetMouseButtonType.GetMouseButtonUp) {
				return "Returns true during the frame the user releases the given mouse button.";
			}
			return null;
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(button, owner, Name + " - button");
			uNode.uNodeUtility.CheckError(storeValue, owner, Name + " - storeValue");
		}
	}
}