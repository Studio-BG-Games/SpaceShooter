using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MaxyGames.uNode;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "NavMeshAgent.Patrol", true)]
	public class NavMeshAgentPatrol : CoroutineAction {
		[ObjectType(typeof(NavMeshAgent))]
		public MemberData agent;
		[Filter(typeof(IList<GameObject>), InvalidTargetType = MemberData.TargetType.Null)]
		public MemberData targets = new MemberData(new List<GameObject>());
		public bool randomPatrol;

		private int patrolIndex = -1;

		protected override IEnumerator ExecuteCoroutine() {
			var targetPatrols = targets.Get<IList<GameObject>>();
			if(targetPatrols.Count == 0) {
				yield break;
			} else if(targetPatrols.Count == 1) {
				patrolIndex = 0;
			} else {
				if(randomPatrol) {
					var oldIndex = patrolIndex;
					while(patrolIndex == oldIndex) {
						patrolIndex = Random.Range(0, targetPatrols.Count);
					}
				} else {
					patrolIndex = (int)Mathf.Repeat(patrolIndex + 1, targetPatrols.Count);
				}
			}
			var navAgent = agent.Get<NavMeshAgent>();
			var target = targetPatrols[patrolIndex];
			while(Vector3.Distance(navAgent.transform.position, target.transform.position) >= navAgent.stoppingDistance) {
				navAgent.SetDestination(target.transform.position);
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
			string result = null;
			uNode.VariableData[] variables = CG.GetUserObject(this) as uNode.VariableData[];
			if(variables == null) {
				variables = new uNode.VariableData[1];
				variables[0] = new uNode.VariableData("patrolIndex", typeof(int), -1);
				variables[0].modifier.SetPrivate();
				CG.RegisterUserObject(variables, this);
				CG.RegisterVariable(variables[0]);
			}
			string patrolIndex = CG.GetVariableName(variables[0]);
			var targetPatrols = "targetPatrols".CGName(this);
			result += CG.DeclareVariable(targetPatrols, typeof(IList<GameObject>), targets).AddLineInFirst();
			bool flag = false;
			if(targets.targetType == MemberData.TargetType.Values) {
				var targetList = targets.Get<IList<GameObject>>();
				if(targetList.Count == 0) {
					return null;
				} else if(targetList.Count == 1) {
					result += patrolIndex.CGSet(0.CGValue()).AddLineInFirst();
					flag = true;
				}
			}
			if(!flag) {
				if(randomPatrol) {
					var oldIndex = "oldIndex".CGName(this);
					result += CG.DeclareVariable(oldIndex, typeof(int), patrolIndex, false).AddLineInFirst();
					result += CG.Condition("while", patrolIndex.CGCompare(oldIndex),
						patrolIndex.CGSet(typeof(Random).CGInvoke(
							"Range",
							0.CGValue(),
							targetPatrols.CGAccess("Count")))).AddLineInFirst();
				} else {
					result += patrolIndex.CGSet(typeof(Mathf).
						CGInvoke(
							"Repeat",
							patrolIndex.CGAdd(1.CGValue()),
							targetPatrols.CGAccess("Count")).CGConvert(typeof(int))).AddLineInFirst();
				}
			}
			var navAgent = "navAgent".CGName(this);
			var target = "target".CGName(this);
			result += CG.DeclareVariable(navAgent, typeof(NavMeshAgent), agent).AddLineInFirst();
			result += CG.DeclareVariable(target, typeof(GameObject), targetPatrols.CGAccessElement(patrolIndex), false).AddLineInFirst();
			string condition = typeof(Vector3).CGInvoke("Distance",
				navAgent.CGAccess("transform", "position"),
				target.CGAccess("transform", "position")).
				CGCompare(navAgent.CGAccess("stoppingDistance"), ComparisonType.GreaterThanOrEqual);
			string contents = navAgent.CGFlowInvoke("SetDestination",
				target.CGAccess("transform", "position"));
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