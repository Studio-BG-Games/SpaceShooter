using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace MaxyGames.Events {
	[BlockMenu("★General", "TypeConverter")]
	public class TypeConverter : Action {
		public MemberData target;
		[Filter(OnlyGetType = true, ArrayManipulator = false)]
		public MemberData type;
		[Filter(SetMember = true)]
		[ObjectType("type")]
		public MemberData storeResult;

		public override string Name {
			get {
				return string.Format("Convert: <b>{2}</b> to <b>{1}</b> store to <b>{0}</b>",
					uNode.uNodeUtility.GetNicelyDisplayName(storeResult),
					uNode.uNodeUtility.GetNicelyDisplayName(type, true, false),
					uNode.uNodeUtility.GetNicelyDisplayName(target));
			}
		}

		protected override void OnExecute() {
			if(target.isAssigned && type.isAssigned && storeResult.isAssigned) {
				storeResult.Set(Operator.Convert(target.Get(), type.Get<System.Type>()));
			}
		}

		public override string GenerateCode(Object obj) {
			if(target.isAssigned && type.isAssigned && storeResult.isAssigned) {
				Type Type = type.Get() as System.Type;
				return CG.Set(storeResult, "(" + CG.Type(Type) + ")" + CG.Value((object)target));
			}
			return null;
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(target, owner, Name + " - target");
			uNode.uNodeUtility.CheckError(type, owner, Name + " - type");
		}
	}
}