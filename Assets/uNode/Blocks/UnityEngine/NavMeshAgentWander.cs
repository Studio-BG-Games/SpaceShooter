using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "NavMeshAgent.Wander", true)]
	public class NavMeshAgentWander : CoroutineAction {
		[ObjectType(typeof(NavMeshAgent))]
		public MemberData agent;
		[ObjectType(typeof(float))]
		public MemberData minWanderDistance = new MemberData(5f);
		[ObjectType(typeof(float))]
		public MemberData maxWanderDistance = new MemberData(10f);
		[ObjectType(typeof(int))]
		public MemberData layer = new MemberData(-1);
		public bool repeat = true;

		protected override IEnumerator ExecuteCoroutine() {
			var navAgent = agent.Get<NavMeshAgent>();
			if(repeat) {
				while(true) {
					if(!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance) {
						var randomPosition = Runtime.RuntimeUtility.RandomNavSphere(navAgent.transform.position, 
							minWanderDistance.Get<float>(), 
							maxWanderDistance.Get<float>(), 
							layer.Get<int>());
						navAgent.SetDestination(randomPosition);
					}
					yield return null;
				}
			} else {
				var randomPosition = Runtime.RuntimeUtility.RandomNavSphere(navAgent.transform.position, 
					minWanderDistance.Get<float>(), 
					maxWanderDistance.Get<float>(), 
					layer.Get<int>());
				navAgent.SetDestination(randomPosition);
			}
		}

		protected override void OnStop() {
			agent.Get<NavMeshAgent>().ResetPath();
		}

		public override string GenerateCode(Object obj) {
			var navAgent = CG.GenerateVariableName("navAgent", this);
			var randomPosition = CG.GenerateVariableName("randomPosition", this);
			string result = null;
			result += CG.DeclareVariable(navAgent, typeof(NavMeshAgent), agent);
			string content = CG.DeclareVariable(randomPosition, typeof(Vector3), 
				CG.Invoke(typeof(Runtime.RuntimeUtility), "RandomNavSphere", 
					navAgent.CGAccess("transform.position"), 
					minWanderDistance.CGValue(), 
					maxWanderDistance.CGValue(), 
					layer.CGValue()), false).AddLineInFirst();
			content += CG.FlowInvoke(navAgent, "SetDestination", randomPosition).AddLineInFirst();
			if(repeat) {
				string contents = CG.If(
					CG.Compare(
					CG.And("!" + navAgent + ".pathPending",  navAgent + ".remainingDistance"), 
					navAgent + ".stoppingDistance", 
					ComparisonType.LessThanOrEqual), content);
				result += CG.Condition("while", "true", contents).AddLineInFirst();
			} else {
				result += content.AddLineInFirst();
			}
			return result;
		}

		public override string GenerateStopCode(Object obj) {
			return CG.FlowInvoke(agent, "ResetPath");
		}

		public override string GetDescription() {
			return "Makes the agent wander randomly within the navigation map";
		}
	}
}