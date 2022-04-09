using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Other", "StopFlowNode", HideOnFlow = true)]
	[AddComponentMenu("")]
	public class StopFlowNode : Node {
		[HideInInspector, FlowOut]
		public List<MemberData> flowNodes = new List<MemberData>() { new MemberData() };
		[Hide, FlowOut("Next", true)]
		public MemberData nextNode = new MemberData();

		public override void OnExecute() {
			foreach(var flow in flowNodes) {
				if(!flow.isAssigned)
					continue;
				Node node = flow.GetFlowNode();
				if(node) {
					node.Stop();
					if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
						int integer = int.Parse(flow.startName);
						GraphDebug.FlowTransition(node.owner, node.owner.GetInstanceID(), node.GetInstanceID(), integer);
					}
				}
			}
			Finish(nextNode);
		}

		public override string GenerateCode() {
			string data = null;
			for(int i = 0; i < flowNodes.Count; i++) {
				if(!flowNodes[i].isAssigned)
					continue;
				data += CG.StopEvent(flowNodes[i].GetTargetNode(), false).AddLineInFirst();
			}
			return data + CG.FlowFinish(this, true, false, nextNode).AddLineInFirst();
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterAsStateNode(flowNodes);
		}

		public override bool IsCoroutine() {
			//return HasCoroutineInFlow(nextNode);
			return false;
		}
	}
}
