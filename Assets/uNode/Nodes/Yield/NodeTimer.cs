using UnityEngine;
using System;
using MaxyGames.uNode;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Yield", "Timer", IsCoroutine = true, HideOnFlow =true)]
	[AddComponentMenu("")]
	public class NodeTimer : Node, IExtendedOutput {
		[NonSerialized]
		public FlowInput start = new FlowInput("Start");
		[NonSerialized]
		public FlowInput pause = new FlowInput("Pause");
		[NonSerialized]
		public FlowInput resume = new FlowInput("Resume");
		[NonSerialized]
		public FlowInput reset = new FlowInput("Reset");

		[Hide, ValueIn, Filter(typeof(float))]
		public MemberData waitTime = new MemberData(1f);

		[Hide, FlowOut("On Start")]
		public MemberData onStart = new MemberData();
		[Hide, FlowOut("On Update")]
		public MemberData onUpdate = new MemberData();
		[Hide, FlowOut("On Finished", true)]
		public MemberData onFinished = new MemberData();

		private bool timerOn;
		private float elapsed;
		private float duration;
		private bool paused;

		public override void OnRuntimeInitialize() {
			start.onExecute = () => {
				if(!timerOn) {
					timerOn = true;
					paused = false;
					elapsed = 0;
					duration = waitTime.Get<float>();
					if(onStart.isAssigned) {
						onStart.InvokeFlow();
					}
				}
			};
			pause.onExecute = () => {
				paused = true;
			};
			resume.onExecute = () => {
				paused = false;
			};
			reset.onExecute = () => {
				timerOn = false;
				paused = false;
				elapsed = 0;
			};
			UEvent.Register(UEventID.Update, owner, DoUpdate);
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterFlowNode(this);
			start.codeGeneration += () => {
				return CG.If(
					"timerOn".CGName(this).CGNot(),
					CG.Flow(
						"timerOn".CGName(this).CGSet(true.CGValue()),
						"elapsed".CGName(this).CGSet(0.CGValue()),
						"paused".CGName(this).CGSet(false.CGValue()),
						"duration".CGName(this).CGSet(waitTime.CGValue()),
						onStart.CGFlow(this, CG.allowYieldStatement)
					)
				);
			};
			pause.codeGeneration += () => {
				return CG.Set("paused".CGName(this), true.CGValue());
			};
			resume.codeGeneration += () => {
				return CG.Set("paused".CGName(this), false.CGValue());
			};
			reset.codeGeneration += () => {
				return CG.Flow(
					CG.Set("timerOn".CGName(this), false.CGValue()),
					CG.Set("paused".CGName(this), false.CGValue()),
					CG.Set("elapsed".CGName(this), 0.CGValue()),
					CG.Set("duration".CGName(this), 0.CGValue())
				);
			};
			CG.RegisterNodeSetup(this, InitCodeGeneration);
		}

		void DoUpdate() {
			if(timerOn && !paused) {
				elapsed += Time.deltaTime;
				if(elapsed >= duration) {
					elapsed = 0;
					timerOn = false;
					if(onFinished.isAssigned) {
						onFinished.InvokeFlow();
					}
				} else if(onUpdate.isAssigned) {
					onUpdate.InvokeFlow();
				}
			}
		}

		void InitCodeGeneration() {
			var isActive = CG.RegisterPrivateVariable("timerOn".CGName(this), typeof(bool), false);
			var elapsed = CG.RegisterPrivateVariable("elapsed".CGName(this), typeof(float), 0);
			var paused = CG.RegisterPrivateVariable("paused".CGName(this), typeof(bool), false);
			var duration = CG.RegisterPrivateVariable("duration".CGName(this), typeof(float), 0);
			var updateContents = 
				CG.If(
					CG.And(
						isActive,
						paused.CGNot()),
					CG.Flow(
						elapsed.CGSet(typeof(Time).CGAccess(nameof(Time.deltaTime)), SetType.Add),
						CG.If(
							elapsed.CGCompare(duration, ComparisonType.GreaterThanOrEqual),
							CG.Flow(
								elapsed.CGSet(0.CGValue()),
								isActive.CGSet(false.CGValue()),
								onFinished.CGFlow(this, false)
							), onUpdate.CGFlow(this, false)
						)
					)
				);
			if(CG.includeGraphInformation) {
				//Wrap the update contents with information of this node.
				updateContents = CG.WrapWithInformation(updateContents, this);
			}
			CG.InsertCodeToFunction("Update", typeof(void), updateContents);
		}

		public override bool IsFlowNode() {
			return false;
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(waitTime, this, "waitTime");
		}

		public override Type ReturnType() {
			return typeof(TypeIcons.ClockIcon);
		}

		int IExtendedOutput.OutputCount => 4;

		object IExtendedOutput.GetOutputValue(string name) {
			switch(name) {
				case "Elapsed":
					return elapsed;
				case "Elapsed %":
					return Mathf.Clamp01(elapsed / duration);
				case "Remaining":
					return Mathf.Max(0, duration - elapsed);
				case "Remaining %":
					return Mathf.Clamp01((duration - elapsed) / duration);
			}
			throw new InvalidOperationException();
		}

		string IExtendedOutput.GetOutputName(int index) {
			switch(index) {
				case 0:
					return "Elapsed";
				case 1:
					return "Elapsed %";
				case 2:
					return "Remaining";
				case 3:
					return "Remaining %";
			}
			throw new InvalidOperationException();
		}

		Type IExtendedOutput.GetOutputType(string name) {
			return typeof(float);
		}

		string IExtendedOutput.GenerateOutputCode(string name) {
			switch(name) {
				case "Elapsed":
					return "elapsed".CGName(this);
				case "Elapsed %":
					return CG.Invoke(typeof(Mathf), nameof(Mathf.Clamp01), CG.Divide("elapsed".CGName(this), "duration".CGName(this)));
				case "Remaining":
					return CG.Invoke(typeof(Mathf), nameof(Mathf.Max), CG.Value(0), CG.Subtract("duration".CGName(this), "elapsed".CGName(this)));
				case "Remaining %":
					return CG.Invoke(typeof(Mathf), nameof(Mathf.Clamp01),
						CG.Divide(
							CG.Wrap(CG.Subtract("duration".CGName(this), "elapsed".CGName(this))), 
							"duration".CGName(this)));
			}
			throw new InvalidOperationException();
		}
	}
}
