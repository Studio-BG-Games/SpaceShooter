using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Input.GetKey", hideOnBlock = true)]
	public class InputGetKey : AnyBlock {
		public enum GetKeyType {
			GetKey,
			GetKeyDown,
			GetKeyUp,
		}
		public GetKeyType getKeyType;
		[Filter(typeof(KeyCode))]
		public MemberData keyCode = new MemberData(KeyCode.None);
		[Filter(typeof(bool), SetMember = true)]
		public MemberData storeValue = new MemberData();

		public override string Name {
			get {
				switch(getKeyType) {
					case GetKeyType.GetKey:
						return "Input.GetKey";
					case GetKeyType.GetKeyDown:
						return "Input.GetKeyDown";
					case GetKeyType.GetKeyUp:
						return "Input.GetKeyUp";
				}
				return base.Name;
			}
		}

		protected override void OnExecute() {
			if(storeValue.isAssigned) {
				storeValue.Set(GetKey(keyCode.Get<KeyCode>()));
			}
		}

		protected override bool OnValidate() {
			bool condition = GetKey(keyCode.Get<KeyCode>());
			if(storeValue.isAssigned) {
				storeValue.Set(condition);
			}
			return condition;
		}

		public bool GetKey(KeyCode keyCode) {
			if(getKeyType == GetKeyType.GetKey) {
				return Input.GetKey(keyCode);
			} else if(getKeyType == GetKeyType.GetKeyDown) {
				return Input.GetKeyDown(keyCode);
			} else if(getKeyType == GetKeyType.GetKeyUp) {
				return Input.GetKeyUp(keyCode);
			}
			return false;
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			if(getKeyType == GetKeyType.GetKey) {
				data = CG.FlowInvoke(typeof(Input), "GetKey", CG.Value(keyCode));
			} else if(getKeyType == GetKeyType.GetKeyDown) {
				data = CG.FlowInvoke(typeof(Input), "GetKeyDown", CG.Value(keyCode));
			} else if(getKeyType == GetKeyType.GetKeyUp) {
				data = CG.FlowInvoke(typeof(Input), "GetKeyUp", CG.Value(keyCode));
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
			if(getKeyType == GetKeyType.GetKey) {
				return "Returns true while the user holds down the key identified by name. Think auto fire.";
			} else if(getKeyType == GetKeyType.GetKeyDown) {
				return "Returns true during the frame the user starts pressing down the key identified by name.";
			} else if(getKeyType == GetKeyType.GetKeyUp) {
				return "Returns true during the frame the user releases the key identified by name.";
			}
			return null;
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(keyCode, owner, Name + " - keyCode");
			uNode.uNodeUtility.CheckError(storeValue, owner, Name + " - storeValue");
		}
	}
}