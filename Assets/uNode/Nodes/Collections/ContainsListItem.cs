using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections.List", "Contains Item", typeof(bool))]
	[AddComponentMenu("")]
	public class ContainsListItem : ValueNode {
		[Hide, ValueIn("List"), Filter(typeof(IList))]
		public MemberData target = new MemberData();
		[Hide, ValueIn("Value"), ObjectType(nameof(target), isElementType =true)]
		public MemberData value = MemberData.none;

		public override System.Type ReturnType() {
			return typeof(bool);
		}

		protected override object Value() {
			return target.Get<IList>().Contains(value.Get());
		}

		public override string GenerateValueCode() {
			return CG.Invoke(target, "Contains", value.CGValue());
		}

		public override string GetNodeName() {
			return "Contains Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add($".Contains({value.GetNicelyDisplayName(richName:true)})");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target", false);
		}
	}
}