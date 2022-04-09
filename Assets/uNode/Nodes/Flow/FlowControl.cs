using UnityEngine;
using System.Collections.Generic;
using System;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "FlowControl")]
	[AddComponentMenu("")]
	public class FlowControl : Node {
		[HideInInspector, FlowOut(finishedFlow = true)]
		public List<MemberData> nextNode = new List<MemberData>() { new MemberData() };

		public override void OnExecute() {
			Finish(nextNode);
		}

		public override string GenerateCode() {
			string data = null;
			for(int i = 0; i < nextNode.Count; i++) {
				if(!nextNode[i].isAssigned) continue;
				data += CG.Flow(nextNode[i], this).AddLineInFirst();
			}
			return data;
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(nextNode);
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.BranchIcon);
		}
	}
}
