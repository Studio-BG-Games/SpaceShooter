using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections.List", "Remove Item")]
	[AddComponentMenu("")]
	public class RemoveListItem : Node {
		[Hide, ValueIn("List"), Filter(typeof(IList))]
		public MemberData target = new MemberData();
		[Hide, ValueIn("Value"), ObjectType(nameof(target), isElementType =true)]
		public MemberData value = MemberData.none;

		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			target.Get<IList>().Remove(value.Get());
			Finish(onFinished);
		}

		public override string GenerateCode() {
			return CG.Flow(
				CG.FlowInvoke(target, "Remove", value.CGValue()),
				CG.FlowFinish(this, true, onFinished)
			);
		}

		public override string GetNodeName() {
			return "Remove Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add($".Remove({value.GetNicelyDisplayName(richName:true)})");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target", false);
		}
	}
}