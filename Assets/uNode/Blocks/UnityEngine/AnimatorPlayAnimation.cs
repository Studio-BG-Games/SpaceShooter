using System.Collections;
using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Animator.PlayAnimation", isCoroutine = true)]
	public class AnimatorPlayAnimation : CoroutineAction {
		[ObjectType(typeof(Animator))]
		public MemberData animator;
		[ObjectType(typeof(int))]
		public MemberData layerIndex;
		[ObjectType(typeof(string))]
		public MemberData stateName;
		[Tooltip("If true, play and wait animation until finish.")]
		public bool waitUntilFinish = true;

		protected override void OnExecute() {
			if(waitUntilFinish) {
				base.OnExecute();//Throw an exception.
			}
			var anim = animator.Get<Animator>();
			var stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex.Get<int>());
			anim.CrossFade(stateName.Get<string>(), stateInfo.length, layerIndex.Get<int>());
		}

		protected override IEnumerator ExecuteCoroutine() {
			var anim = animator.Get<Animator>();
			var stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex.Get<int>());
			anim.CrossFade(stateName.Get<string>(), stateInfo.length, layerIndex.Get<int>());
			while(waitUntilFinish) {
				if(stateInfo.IsName(stateName.Get<string>())) {
					yield return new WaitForSeconds(stateInfo.length / anim.playbackTime);
					break;
				}
				yield return null;
			}
		}

		public override bool IsCoroutine() {
			return waitUntilFinish;
		}

		public override string GenerateCode(Object obj) {
			string result = null;
			uNode.VariableData[] variables = CG.GetUserObject(this) as uNode.VariableData[];
			if(variables == null) {
				variables = new uNode.VariableData[2];
				variables[0] = new uNode.VariableData("anim", typeof(Animator));
				variables[1] = new uNode.VariableData("stateInfo", typeof(AnimatorStateInfo));
				CG.RegisterUserObject(variables, this);
			}
			string anim = CG.GetVariableName(variables[0]);
			string stateInfo = CG.GetVariableName(variables[1]);
			result += CG.DeclareVariable(variables[0], animator);
			result += CG.DeclareVariable(variables[1],
				CG.Invoke(variables[0].CGValue(), "GetCurrentAnimatorStateInfo", layerIndex.CGValue()), false).AddLineInFirst();
			result += CG.FlowInvoke(anim, "CrossFade", CG.Value(stateName),
				stateInfo + ".length",
				CG.Value(layerIndex)).AddLineInFirst();
			if(waitUntilFinish) {
				string contents = CG.YieldReturn(CG.Arithmetic(stateInfo + ".length", anim + ".playbackTime", ArithmeticType.Divide));
				contents += CG.Break().AddLineInFirst();
				string code = CG.If(CG.Invoke(stateInfo, "IsName", CG.Value(stateName)), contents);
				code += CG.GetYieldReturn(null).AddLineInFirst();
				result += CG.Condition("while", "true", code).AddLineInFirst();
			}
			return result;
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(animator, owner, Name + " - animator");
			uNode.uNodeUtility.CheckError(layerIndex, owner, Name + " - layerIndex");
			uNode.uNodeUtility.CheckError(stateName, owner, Name + " - stateName");
		}
	}
}