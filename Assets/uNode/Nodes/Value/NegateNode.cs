using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Data", "Negate {-}", typeof(object))]
	[AddComponentMenu("")]
	public class NegateNode : ValueNode {
		[Hide, ValueIn]
		public MemberData target = new MemberData();

		public override System.Type ReturnType() {
			if(target.isAssigned) {
				try {
					object obj = Operator.Negate(ReflectionUtils.CreateInstance(target.type));
					if(!object.ReferenceEquals(obj, null)) {
						return obj.GetType();
					}
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			return Operator.Negate(target.Get());
		}

		public override string GenerateValueCode() {
			if(target.isAssigned) {
				return "-(" + CG.Value((object)target) + ")";
			}
			throw new System.Exception();
		}

		public override string GetNodeName() {
			return "Negate";
		}

		public override string GetRichName() {
			return "-" + target.GetNicelyDisplayName(richName:true);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
			if(target.isAssigned) {
				try {
					Operator.Negate(ReflectionUtils.CreateInstance(target.type));
				}
				catch(System.Exception ex) {
					RegisterEditorError(ex.Message);
				}
			}
		}
	}
}