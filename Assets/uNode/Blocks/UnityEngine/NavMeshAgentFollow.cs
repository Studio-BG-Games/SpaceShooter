using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using MaxyGames.uNode;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "NavMeshAgent.Follow", true)]
	public class NavMeshAgentFollow : CoroutineAction {
		[ObjectType(typeof(NavMeshAgent))]
		public MemberData agent;
		[ObjectType(typeof(GameObject))]
		public MemberData target;
		[ObjectType(typeof(float))]
		public MemberData followDistance = new MemberData(5f);
		public bool repeat = true;

		protected override IEnumerator ExecuteCoroutine() {
			var navAgent = agent.Get<NavMeshAgent>();
			float distance = Vector3.Distance(navAgent.transform.position, target.Get<GameObject>().transform.position);
			if(repeat) {
				while(true) {
					if(followDistance.Get<float>() <= distance) {
						navAgent.SetDestination(target.Get<GameObject>().transform.position);
					}
					yield return null;
				}
			} else {
				if(followDistance.Get<float>() <= distance) {
					navAgent.SetDestination(target.Get<GameObject>().transform.position);
				}
			}
		}

		protected override void OnStop() {
			agent.Get<NavMeshAgent>().ResetPath();
		}

		public override string GenerateCode(Object obj) {
			var navAgent = "navAgent".CGName(this);
			var distance = "distance".CGName(this);
			string result = null;
			result += CG.DeclareVariable(navAgent, typeof(NavMeshAgent), agent);
			result += CG.DeclareVariable(distance, typeof(float), typeof(Vector3).CGInvoke("SetDestination", target.CGValue().CGAccess("transform", "position")), false).AddLineInFirst();
			string contents = CG.If(followDistance.CGValue().
				CGCompare(distance, ComparisonType.LessThanOrEqual),
				navAgent.CGFlowInvoke("SetDestination", target.CGValue().
				CGAccess("transform", "position")));
			if(repeat) {
				result += CG.Condition("while", "true", contents + CG.GetYieldReturn(null).AddLineInFirst()).AddLineInFirst();
			} else {
				result += contents.AddLineInFirst();
			}
			return result;
		}

		public override string GenerateStopCode(Object obj) {
			return CG.FlowInvoke(agent, "ResetPath");
		}

		public override string GetDescription() {
			return "Follow the target";
		}
	}
}