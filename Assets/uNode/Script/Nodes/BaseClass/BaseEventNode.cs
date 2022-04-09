using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Base class for all event node.
	/// </summary>
	public abstract class BaseEventNode : NodeComponent {
		[HideInInspector, SerializeField]
		private List<Node> nodes = new List<Node>();
		[HideInInspector, SerializeField]
		public List<MemberData> targetNodes = new List<MemberData>();

		/// <summary>
		/// Get the flows
		/// </summary>
		/// <returns></returns>
		public List<MemberData> GetFlows() {
			if(nodes.Count > 0) {
				targetNodes.Clear();
				for(int i = 0; i < nodes.Count; i++) {
					targetNodes.Add(new MemberData(nodes[i], MemberData.TargetType.FlowNode));
				}
				nodes.Clear();
			}
			return targetNodes;
		}

		/// <summary>
		/// Trigger the event so that the event will execute the flows.
		/// </summary>
		public virtual void Trigger() {
			if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
				GraphDebug.FlowNode(owner, owner.GetInstanceID(), this.GetInstanceID(), true);
			}
			var flows = GetFlows();
			int nodeCount = flows.Count;
			if(nodeCount > 0) {
				for(int x = 0; x < nodeCount; x++) {
					if(flows[x] != null && flows[x].isAssigned) {
						flows[x].InvokeFlow();
					}
				}
			}
		}

		public override string GetNodeName() {
			if(GetType().IsDefined(typeof(EventMenuAttribute), true)) {
				return (GetType().GetCustomAttributes(typeof(EventMenuAttribute), true)[0] as EventMenuAttribute).name;
			}
			return base.GetNodeName();
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.EventIcon);
		}

		#region Generation
		protected virtual string GenerateFlows() {
			string contents = "";
			var flows = GetFlows();
			if(flows != null && flows.Count > 0) {
				foreach(var flow in flows) {
					if(flow == null || flow.GetTargetNode() == null)
						continue;
					try {
						if(flow.targetType == MemberData.TargetType.FlowNode) {
							contents += CG.GetInvokeNodeCode(flow.GetTargetNode()).AddLineInFirst();
						} else {
							contents += CG.Flow(flow, this, false);
						}
					}
					catch(Exception ex) {
						Debug.LogException(ex, flow.GetTargetNode());
					}
				}
			}
			return contents;
		}
		#endregion
	}

	/// <summary>
	/// This is the base class for all graph event.
	/// </summary>
	public abstract class BaseGraphEvent : BaseEventNode {
		public abstract void GenerateCode();
	}

	/// <summary>
	/// This is the base class for all state event.
	/// </summary>
	public abstract class BaseStateEvent : BaseEventNode {
		[NonSerialized]
		protected StateNode stateNode;

		public override void Trigger() {
			if(stateNode.currentState == StateType.Running) {
				base.Trigger();
			}
		}

		public override void OnRuntimeInitialize() {
			stateNode = uNodeHelper.GetComponentInParent<StateNode>(this);
			if(stateNode == null) {
				Debug.LogError("Parent StateNode not found.", this);
				return;
			}
		}
	}
}