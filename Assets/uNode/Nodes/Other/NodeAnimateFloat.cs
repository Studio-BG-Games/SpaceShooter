using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Other", "AnimateFloat", IsCoroutine = true)]
	[AddComponentMenu("")]
	public class NodeAnimateFloat : Node {
		[Hide, FlowOut("", true)]
		public MemberData onFinished = new MemberData();
		[Hide, ValueIn("curve"), Filter(typeof(AnimationCurve), InvalidTargetType = MemberData.TargetType.Null)]
		public MemberData curve = new MemberData(new AnimationCurve());
		[Hide, ValueIn("time"), Filter(typeof(float), SetMember = true)]
		public MemberData time = new MemberData();
		[Hide, ValueIn("speed"), Filter(typeof(float))]
		public MemberData speed = new MemberData(1f);

		float currentTime;
		float endTime;

		public override void OnExecute() {
			currentTime = 0;
			AnimationCurve c = curve.Get<AnimationCurve>();
			endTime = c.keys[c.length - 1].time;
			owner.StartCoroutine(OnCall(), this);
		}

		public IEnumerator OnCall() {
			while(currentTime < endTime) {
				currentTime += Time.deltaTime * speed.Get<float>();
				time.Set(curve.Get<AnimationCurve>().Evaluate(currentTime));
				yield return null;
			}
			Finish(onFinished);
		}

		public override string GenerateCode() {
			VariableData[] variables = CG.GetUserObject(this) as VariableData[];
			if(variables == null) {
				variables = CG.RegisterUserObject(new VariableData[] {
					new VariableData("_currentTime", typeof(float), 0),
					new VariableData("_endTime", typeof(float), 0),
					new VariableData("curve", typeof(AnimationCurve), 0),
				}, this);
			}
			string curTime = CG.RegisterVariable(variables[0]);
			string enTime = CG.RegisterVariable(variables[1]);
			string data = CG.Set(curTime, 0);
			data += CG.Set(enTime, (object)(CG.Value((object)curve) + ".keys[" + CG.Value((object)curve) + ".length - 1].time")).AddLineInFirst();
			string contents = CG.Set(curTime, (object)(CG.Type(typeof(Time)) + ".deltaTime * " + CG.Value((object)speed)), SetType.Add);
			contents += CG.Set(time, CG.Invoke(curve, "Evaluate", variables[0].CGValue())).AddLineInFirst();
			contents += CG.GetYieldReturn(null).AddLineInFirst();
			data += CG.Condition("while", CG.Compare(curTime, enTime, ComparisonType.LessThan), contents).AddLineInFirst();
			data += CG.FlowFinish(this, true, onFinished).AddLineInFirst();
			return data;
		}

		public override bool IsSelfCoroutine() {
			return true;
		}
	}
}