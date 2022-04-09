using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using MaxyGames.uNode;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "NavMeshAgent.Seek", true)]
	public class NavMeshAgentSeek : CoroutineAction {
		[ObjectType(typeof(NavMeshAgent))]
		public MemberData agent;
		[ObjectType(typeof(GameObject))]
		public MemberData target;

		protected override IEnumerator ExecuteCoroutine() {
			var navAgent = agent.Get<NavMeshAgent>();
			while(Vector3.Distance(navAgent.transform.position, target.Get<GameObject>().transform.position) >= navAgent.stoppingDistance) {
				navAgent.SetDestination(target.Get<GameObject>().transform.position);
				if(!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance) {
					break;
				}
				yield return null;
			}
		}

		protected override void OnStop() {
			agent.Get<NavMeshAgent>().ResetPath();
		}

		public override string GenerateCode(Object obj) {
			var navAgent = "navAgent".CGName(this);
			string result = null;
			result += CG.DeclareVariable(navAgent, typeof(NavMeshAgent), agent);
			string condition = typeof(Vector3).CGInvoke("Distance", 
				navAgent.CGAccess("transform", "position"), 
				target.CGValue().CGAccess("transform", "position")).
				CGCompare(navAgent.CGAccess("stoppingDistance"), ComparisonType.GreaterThanOrEqual);
			string contents = navAgent.CGFlowInvoke("SetDestination", 
				target.CGValue().CGAccess("transform", "position"));
			contents += CG.If(
				navAgent.CGAccess("pathPending").CGNot().
				CGAnd(navAgent.CGAccess("remainingDistance")).
				CGCompare(navAgent.CGAccess("stoppingDistance"), ComparisonType.LessThanOrEqual), 
				CG.Break()).AddLineInFirst();
			contents += CG.GetYieldReturn(null).AddLineInFirst();
			result += CG.Condition("while", condition, contents).AddLineInFirst();
			return result;
		}

		public override string GenerateStopCode(Object obj) {
			return CG.FlowInvoke(agent, "ResetPath");
		}

		public override string GetDescription() {
			return "Move the agent to target until reached.";
		}
	}
}