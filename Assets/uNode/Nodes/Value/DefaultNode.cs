using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "Default")]
	[AddComponentMenu("")]
	public class DefaultNode : ValueNode {
		[Hide, FieldDrawer(), Filter(OnlyGetType = true)]
		public MemberData type = new MemberData(typeof(object), MemberData.TargetType.Type);

		public override System.Type ReturnType() {
			if(type.isAssigned) {
				try {
					System.Type t = type.Get<System.Type>();
					if(!object.ReferenceEquals(t, null)) {
						return t;
					}
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			return Operator.Default(type.Get<System.Type>());
		}

		public override string GenerateValueCode() {
			if(type.isAssigned) {
				return "default(" + CG.Type(type) + ")";
			}
			throw new System.Exception();
		}

		public override string GetNodeName() {
			return "Default";
		}

		public override string GetRichName() {
			return $"default({type.GetNicelyDisplayName(richName:true, typeTargetWithTypeof:false)})";
		}
	}
}