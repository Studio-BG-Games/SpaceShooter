using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using MaxyGames.uNode;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "NavMeshAgent.Flee", true)]
	public class NavMeshAgentFlee : CoroutineAction {
		[ObjectType(typeof(NavMeshAgent))]
		public MemberData agent;
		[ObjectType(typeof(GameObject))]
		public MemberData target;
		[ObjectType(typeof(float))]
		public MemberData fledDistance = new MemberData(10f);
		[ObjectType(typeof(float))]
		public MemberData lookAhead = new MemberData(2f);

		protected override IEnumerator ExecuteCoroutine() {
			var navAgent = agent.Get<NavMeshAgent>();
			var targetPos = target.Get<GameObject>().transform.position;
			while((navAgent.transform.position - targetPos).magnitude >= fledDistance.Get<float>()) {
				var fleePos = targetPos + (navAgent.transform.position - targetPos).normalized * (fledDistance.Get<float>() +  lookAhead.Get<float>() + navAgent.stoppingDistance);
				if(!navAgent.SetDestination(fleePos)) {
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
			var targetPos = "targetPos".CGName(this);
			string result = null;
			result += CG.DeclareVariable(navAgent, typeof(NavMeshAgent), agent);
			result += CG.DeclareVariable(targetPos, typeof(Vector3),
				target.CGAccess("transform", "position"), false).AddLineInFirst();
			{//While
				var fleePos = "fleePos".CGName(this);
				string fleePosContents = targetPos.CGAdd(navAgent.CGAccess("transform", "position").CGSubtract(targetPos).Wrap().CGAccess("normalized"));
				fleePosContents = fleePosContents.CGMultiply(
					fledDistance.CGValue().CGAdd(lookAhead.CGValue()).CGAdd(navAgent.CGAccess("stoppingDistance")).Wrap());
				string contents = CG.DeclareVariable(fleePos, typeof(Vector3), fleePosContents, false);
				contents += 
					CG.If(
						navAgent.CGInvoke("SetDestination", fleePos).Wrap(),
						CG.Break()).AddLineInFirst() + 
					CG.YieldReturn(null).AddLineInFirst();
				result += CG.Condition("while",
					CG.Arithmetic(
							navAgent.CGAccess("transform", "position"),
							targetPos,
							ArithmeticType.Subtract).Wrap().
						CGAccess("magnitude").
						CGCompare(
							fledDistance.CGValue(),
							ComparisonType.GreaterThanOrEqual),
					contents).AddLineInFirst();
			}
			return result;
		}

		public override string GenerateStopCode(Object obj) {
			return CG.FlowInvoke(agent, "ResetPath");
		}

		public override string GetDescription() {
			return "Flees away from the target";
		}
	}
}