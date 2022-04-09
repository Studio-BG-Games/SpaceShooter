using UnityEngine;

namespace MaxyGames.Events {
	//[EventMenu("★General/IsComparer", "IsComparer")]
	public class IsComparer : Condition {
		public MemberData target = MemberData.none;
		[Filter(OnlyGetType=true)]
		public MemberData Type = MemberData.none;
		public bool inverted;

		public override string Name {
			get {
				if(inverted) {
					return "!(<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b> is <b>" + uNode.uNodeUtility.GetNicelyDisplayName(Type) + "</b>)";
				}
				return "<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b> is <b>" + uNode.uNodeUtility.GetNicelyDisplayName(Type) + "</b>";
			}
		}

		protected override bool OnValidate() {
			if(target.isAssigned && Type.isAssigned) {
				System.Type type = Type.type;
				object compareObj = target.Get();
				if(type != null) {
					if(compareObj != null && (compareObj.GetType() == type || compareObj.GetType().IsSubclassOf(type))) {
						return !inverted;
					}
					return inverted;
				} else {
					return false;
				}
			}
			throw new System.Exception();
		}

		public override string GenerateConditionCode(Object obj) {
			if(target.isAssigned && Type.isAssigned) {
				if(inverted) {
					return "!(" + CG.Value((object)target) + " is " + CG.Type(Type) + ")";
				}
				return CG.Value((object)target) + " is " + CG.Type(Type);
			}
			return base.GenerateConditionCode(obj);
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(target, owner, Name + " - target");
			uNode.uNodeUtility.CheckError(Type, owner, Name + " - Type");
		}
	}
}