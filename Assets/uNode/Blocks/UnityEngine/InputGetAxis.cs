using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Input.GetAxis", hideOnBlock = true)]
	public class InputGetAxis : Action {
		[ObjectType(typeof(string))]
		public MemberData axisName = new MemberData("");
		[Filter(typeof(float), typeof(int), SetMember = true)]
		public MemberData storeValue;

		protected override void OnExecute() {
			if(axisName.isAssigned && storeValue.isAssigned) {
				if(storeValue.type == typeof(int)) {
					storeValue.Set((int)Input.GetAxis(axisName.Get<string>()));
				} else {
					storeValue.Set(Input.GetAxis(axisName.Get<string>()));
				}
			}
		}

		public override string GenerateCode(Object obj) {
			string data = CG.FlowInvoke(typeof(Input), "GetAxis", axisName.CGValue());
			if(storeValue.isAssigned) {
				return CG.Value((object)storeValue) + " = " + data;
			}
			return data;
		}

		public override string GetDescription() {
			return "Returns the value of the virtual axis identified by axisName.";
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(axisName, owner, Name + " - axisName");
			uNode.uNodeUtility.CheckError(storeValue, owner, Name + " - storeValue");
		}
	}
}