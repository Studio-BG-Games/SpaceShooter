using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	public class NodeReturn : Node {
		public bool returnAnyType = false;
		[Hide]
		public MemberData returnValue;

		public override void OnExecute() {
			jumpState = new JumpStatement(JumpStatementType.Return, this);
			Finish();
		}

		public override string GenerateCode() {
			if(returnValue.isAssigned && returnValue.type != typeof(void)) {
				return "return " + CG.Value((object)returnValue) + ";";
			}
			return "return;";
		}

		public override string GetNodeName() {
			return "Return";
		}

		public override string GetRichName() {
			if(returnValue.isAssigned && returnValue.type != typeof(void)) {
				return base.GetRichName() + " " + returnValue.GetNicelyDisplayName(richName: true);
			}
			return base.GetRichName();
		}

		public object GetReturnValue() {
			return returnValue.Get();
		}

		public override System.Type GetNodeIcon() {
			if(returnAnyType) {
				return typeof(object);
			} else {
				var type = rootObject?.ReturnType();
				if (type != null) {
					return type;
				}
			}
			return typeof(void);
		}
	}
}