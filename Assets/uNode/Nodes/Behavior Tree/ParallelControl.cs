using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Other", "ParallelControl", HideOnFlow = true)]
	[AddComponentMenu("")]
	public class ParallelControl : Node {
		[HideInInspector, FlowOut(finishedFlow =true)]
		public List<MemberData> nextNode = new List<MemberData>() { new MemberData() };

		public override void OnExecute() {
			Finish(nextNode, false);
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterAsStateNode(nextNode);
		}

		public override string GenerateCode() {
			string data = null;
			for(int i = 0; i < nextNode.Count; i++) {
				if(!nextNode[i].isAssigned)
					continue;
				data += CG.Flow(nextNode[i], this, false).AddLineInFirst();
			}
			return data;
		}
	}
}
