using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections.List", "Set Item")]
	[AddComponentMenu("")]
	public class SetListItem : Node {
		[Hide, ValueIn("List"), Filter(typeof(IList))]
		public MemberData target = new MemberData();
		[Hide, ValueIn("Index"), Filter(typeof(int))]
		public MemberData index = MemberData.none;
		[Hide, ValueIn("Value"), ObjectType(nameof(target), isElementType =true)]
		public MemberData value = MemberData.none;

		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			var val = target.Get<IList>();
			val[index.Get<int>()] = value.Get();
			Finish(onFinished);
		}

		public override string GenerateCode() {
			return CG.Flow(
				CG.Set(CG.AccessElement(target, CG.Value(index)), CG.Value(value)),
				CG.FlowFinish(this, true, onFinished)
			);
		}

		public override string GetNodeName() {
			return "Set Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add($".Set({index.GetNicelyDisplayName(richName: true)}, {value.GetNicelyDisplayName(richName:true)}");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target", false);
		}
	}
}