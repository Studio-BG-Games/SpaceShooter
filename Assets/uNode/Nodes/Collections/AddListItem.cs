using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections.List", "Add Item")]
	[AddComponentMenu("")]
	public class AddListItem : Node {
		[Hide, ValueIn("List"), Filter(typeof(IList))]
		public MemberData target = new MemberData();
		[Hide, ValueIn("Value"), ObjectType(nameof(target), isElementType =true)]
		public MemberData value = MemberData.none;

		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			target.Get<IList>().Add(value.Get());
			Finish(onFinished);
		}

		public override string GenerateCode() {
			return CG.Flow(
				CG.FlowInvoke(target, "Add", value.CGValue()),
				CG.FlowFinish(this, true, onFinished)
			);
		}

		public override string GetNodeName() {
			return "Add Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add($".Add({value.GetNicelyDisplayName(richName: true)})");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target", false);
		}
	}
}