using UnityEngine;
using System.Collections;
using System;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Behavior Tree.Decorators", "Repeater", IsCoroutine = true)]
	[Description("Repeat target node until target node is run in specified number of time." +
		"\nThis will always return success.")]
	[AddComponentMenu("")]
	public class Repeater : Node {
		[Tooltip("The number repeat time.")]
		public int RepeatCount = 1;
		[Tooltip("If true, this will repeat forever except StopEventOnFailure is true and called event is failure.")]
		public bool RepeatForever = false;
		[Tooltip("If called event Failure, this will stop to repeat event\n" +
		"This will always return success.")]
		public bool StopEventOnFailure = false;

		[Hide, FlowOut("", true)]
		public MemberData targetNode = new MemberData();

		private int repeatNumber;
		private bool canExecuteEvent;

		public IEnumerator OnUpdate() {
			while(state == StateType.Running) {
				if(!targetNode.isAssigned) {
					Debug.LogError("Unassigned target node", this);
					Finish();
					yield break;
				}
				Node n;
				if(canExecuteEvent && (RepeatForever || RepeatCount > repeatNumber)) {
					WaitUntil w;
					if(!targetNode.ActivateFlowNode(out n, out w)) {
						yield return w;
					}
					repeatNumber++;
					canExecuteEvent = false;
				} else {
					n = targetNode.GetFlowNode();
				}
				if(n.IsFinished()) {
					JumpStatement js = n.GetJumpState();
					if(js != null) {
						if(js.jumpType == JumpStatementType.Continue) {
							continue;
						} else if(js.jumpType == JumpStatementType.Break) {
							Finish();
							yield break;
						}
						jumpState = js;
						Finish();
						yield break;
					}
					if(StopEventOnFailure && n.currentState == StateType.Failure) {
						Finish();
						yield break;
					}
					if(!RepeatForever && RepeatCount <= repeatNumber) {
						Finish();
					}
					canExecuteEvent = true;
				}
				yield return null;
			}
		}

		public override void OnExecute() {
			canExecuteEvent = true;
			repeatNumber = 0;
			owner.StartCoroutine(OnUpdate(), this);
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override void OnGeneratorInitialize() {
			//Register this node as state node, because this is coroutine node with state.
			CG.RegisterAsStateNode(this);
			CG.SetStateInitialization(this, () => CG.GenerateNode(this));
			var node = targetNode.GetTargetNode();
			if(node != null) {
				CG.RegisterAsStateNode(node);
			}
		}

		public override string GenerateCode() {
			if(!targetNode.isAssigned)
				throw new System.Exception("Target is not assigned");
			return CG.New(typeof(Runtime.Repeater), CG.GetEvent(targetNode), RepeatForever ? CG.Value(-1) : CG.Value(RepeatCount), CG.Value(StopEventOnFailure));
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.RepeatIcon);
		}
	}
}
