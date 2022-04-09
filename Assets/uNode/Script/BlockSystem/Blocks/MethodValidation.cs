using UnityEngine;

namespace MaxyGames.Events {
	//[EventMenu("★General/MethodValidation", "MethodValidation")]
	public class MethodValidation : Condition {
		[Filter(typeof(bool), MaxMethodParam = int.MaxValue)]
		public MultipurposeMember target = new MultipurposeMember();

		protected override bool OnValidate() {
			if(target.target.isAssigned) {
				return (bool)target.Get();
			}
			return false;
		}

		public override string GenerateConditionCode(Object obj) {
			if(target.target.isAssigned) {
				return CG.Value(target);
			}
			throw new System.Exception("Unassigned target");
		}

		public override string Name {
			get {
				if(target != null && target.target != null) {
					return $"<b>{uNode.uNodeUtility.GetNicelyDisplayName((object)target)}</b> is <b>{uNode.uNodeUtility.WrapTextWithOtherColor("True")}</b>";
				}
				return base.Name;
			}
		}

		public override string ToolTip {
			get {
				if(target != null && target.target != null) {
					return target.target.Tooltip;
				}
				return base.ToolTip;
			}
		}
	}
}