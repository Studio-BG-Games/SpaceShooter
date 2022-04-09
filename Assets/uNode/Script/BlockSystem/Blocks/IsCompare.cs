using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("★General", "IsComparer")]
	public class IsCompare : Condition {
		[Filter(MaxMethodParam = int.MaxValue)]
		public MultipurposeMember target = new MultipurposeMember();
		[Filter(OnlyGetType = true)]
		public MemberData type = MemberData.none;
		public bool inverted;

		public override string Name {
			get {
				if(inverted) {
					return "!(<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b> " +
						uNode.uNodeUtility.WrapTextWithKeywordColor("is") + 
						" <b>" + uNode.uNodeUtility.GetNicelyDisplayName(type, true, false) + "</b>)";
				}
				return "<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b> " +
					uNode.uNodeUtility.WrapTextWithKeywordColor("is") + 
					" <b>" + uNode.uNodeUtility.GetNicelyDisplayName(type, true, false) + "</b>";
			}
		}

		protected override bool OnValidate() {
			if(target.target.isAssigned && type.isAssigned) {
				System.Type t = type.type;
				object compareObj = target.Get();
				if(t != null) {
					if(compareObj != null && (compareObj.GetType() == t || compareObj.GetType().IsSubclassOf(t))) {
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
			if(target.target.isAssigned && type.isAssigned) {
				if(inverted) {
					return "!(" + CG.Value(target) + " is " + CG.Type(type) + ")";
				}
				return CG.Value(target) + " is " + CG.Type(type);
			}
			return base.GenerateConditionCode(obj);
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(target, owner, Name + " - target");
			uNode.uNodeUtility.CheckError(type, owner, Name + " - type");
		}
	}
}