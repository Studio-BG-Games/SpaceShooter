using System;
using Object = UnityEngine.Object;

namespace MaxyGames.Events {
	//[EventMenu("★General/TypeAs", "TypeAs")]
	public class TypeAs : Action {
		[Filter(DisplayValueType = false)]
		public MemberData target;
		[Filter(OnlyGetType = true, ArrayManipulator = false, DisplayValueType = false)]
		public MemberData type;
		[Filter(SetMember = true)]
		[ObjectType("type")]
		public MemberData storeResult;

		public override string Name {
			get {
				return string.Format("Convert: <b>{2}</b> as <b>{1}</b> store to <b>{0}</b>",
					uNode.uNodeUtility.GetNicelyDisplayName(storeResult),
					uNode.uNodeUtility.GetNicelyDisplayName(type, true, false),
					uNode.uNodeUtility.GetNicelyDisplayName(target));
			}
		}

		protected override void OnExecute() {
			if(target.isAssigned && type.isAssigned && storeResult.isAssigned) {
				storeResult.Set(Operator.TypeAs(target.Get(), type.Get<System.Type>()));
			}
		}

		public override string GenerateCode(Object obj) {
			if(target.isAssigned && type.isAssigned && storeResult.isAssigned) {
				Type Type = type.Get() as System.Type;
				return CG.Set(storeResult, CG.Value((object)target) + " as " + CG.Type(Type));
			}
			return null;
		}
	}
}