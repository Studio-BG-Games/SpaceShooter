using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Preprocessor")]
	[Description("This node doesn't work when running with reflection mode.")]
	public class NodePreprocessor : Node {
		public List<string> preprocessors = new List<string>() { "" };

		[HideInInspector, FlowOut]
		public List<MemberData> targetFlows = new List<MemberData>() { MemberData.none };
		[Hide, FlowOut("Default")]
		public MemberData defaultTarget = new MemberData();

		public override void OnExecute() {
			throw new Exception("This node doesn't support run in reflection mode.");
		}

		public override string GenerateCode() {
			string result = null;
			for(int i = 0; i < preprocessors.Count; i++) {
				var content = CG.Flow(targetFlows[i], this);
				if(!string.IsNullOrEmpty(content)) {
					result += CG.Flow(
						(result == null ? "#if " : "#elif ") + preprocessors[i],
						CG.FlowFinish(this, true, targetFlows[i])
					);
				}
			}
			if(defaultTarget != null && defaultTarget.isAssigned) {
				if(result == null) {
					return CG.Flow(
						"#if !(" + string.Join(" && ", preprocessors) + ")",
						CG.FlowFinish(this, true, defaultTarget)
					);
				} else {
					result += CG.Flow(
						"#else",
						CG.FlowFinish(this, true, defaultTarget)
					);
				}
			}
			return result.AddLineInEnd().Add("#endif");
		}

		public override string GetNodeName() {
			return "Preprocessor";
		}
	}
}

#if UNITY_EDITOR
namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(Nodes.NodePreprocessor))]
	public class NodePreprocessorView : BaseNodeView {
		protected override void InitializeView() {
			InitializeDefaultPort();
			var node = targetNode as Nodes.NodePreprocessor;
			if(node.preprocessors.Count == 0) {
				node.preprocessors.Add("");
			}
			while(node.preprocessors.Count != node.targetFlows.Count) {
				if(node.preprocessors.Count > node.targetFlows.Count) {
					node.targetFlows.Add(MemberData.none);
				} else if(node.preprocessors.Count < node.targetFlows.Count) {
					node.targetFlows.RemoveAt(node.targetFlows.Count - 1);
				}
			}
			for(int i = 0; i < node.preprocessors.Count; i++) {
				int x = i;
				var member = node.preprocessors[i];
				var port = AddOutputFlowPort(
					new PortData() {
						portID = "preprocessors#" + x,
						getPortName = () => x.ToString(),
						getPortValue = () => node.targetFlows[x],
						onValueChanged = (val) => {
							node.targetFlows[x] = val as MemberData;
						},
					}
				);
			}
			{//Default
				var member = node.defaultTarget;
				AddOutputFlowPort(
					new PortData() {
						portID = "default",
						getPortName = () => "Default",
						getPortValue = () => node.defaultTarget,
						onValueChanged = (val) => {
							node.defaultTarget = val as MemberData;
						},
					}
				);
			}
		}

		public override void OnValueChanged() {
			//Ensure to repaint every value changed.
			MarkRepaint();
		}
	}
}
#endif