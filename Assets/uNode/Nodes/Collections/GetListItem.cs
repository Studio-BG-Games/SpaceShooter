using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections.List", "Get Item")]
	[AddComponentMenu("")]
	public class GetListItem : ValueNode {
		[Hide, ValueIn("List"), Filter(typeof(IList))]
		public MemberData target = new MemberData();
		[Hide, ValueIn("Index"), Filter(typeof(int))]
		public MemberData index = MemberData.none;

		public override System.Type ReturnType() {
			if(target.isAssigned) {
				return target.type.ElementType();
			}
			return typeof(object);
		}

		protected override object Value() {
			var val = target.Get<IList>();
			return val[index.Get<int>()];
		}

		public override string GenerateValueCode() {
			return CG.AccessElement(target, CG.Value(index));
		}

		public override string GetNodeName() {
			return "Get Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add($".Get({index.GetNicelyDisplayName()})");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}