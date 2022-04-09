using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "Increment-Decrement", typeof(object))]
	[AddComponentMenu("")]
	public class IncrementDecrementNode : ValueNode {
		public bool isDecrement;
		public bool isPrefix;

		[Hide, ValueIn, Filter(SetMember = true)]
		public MemberData target = new MemberData();
		[Hide, FlowOut("", true, hideOnNotFlowNode = true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			Value();
			Finish(onFinished);
		}

		public override System.Type ReturnType() {
			if(target.isAssigned) {
				try {
					return target.type;
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			object obj;
			if(isPrefix) {
				if(isDecrement) {
					obj = Operator.Decrement(target.Get());
				} else {
					obj = Operator.Increment(target.Get());
				}
				target.Set(obj);
			} else {
				obj = target.Get();
				if(isDecrement) {
					target.Set(Operator.Decrement(obj));
				} else {
					target.Set(Operator.Increment(obj));
				}
			}
			return obj;
		}

		public override string GenerateCode() {
			return base.GenerateCode() + CG.FlowFinish(this, true, onFinished).AddLineInFirst();
		}

		public override string GenerateValueCode() {
			if(target.isAssigned) {
				if(isPrefix) {
					if(isDecrement) {
						return "--(" + CG.Value((object)target) + ")";
					}
					return "++(" + CG.Value((object)target) + ")";
				} else {
					if(isDecrement) {
						return "(" + CG.Value((object)target) + ")--";
					}
					return "(" + CG.Value((object)target) + ")++";
				}
			}
			throw new System.Exception();
		}

		public override string GetNodeName() {
			if(isPrefix) {
				if(isDecrement) {
					return "--$Decrement";
				}
				return "++$Increment";
			}
			if(isDecrement) {
				return "$Decrement--";
			}
			return "$Increment++";
		}

		public override string GetRichName() {
			if(isPrefix) {
				if(isDecrement) {
					return "--" + target.GetNicelyDisplayName(richName:true);
				}
				return "++" + target.GetNicelyDisplayName(richName:true);
			}
			if(isDecrement) {
				return target.GetNicelyDisplayName(richName: true) + "--";
			}
			return target.GetNicelyDisplayName(richName:true) + "++";
		}

		public override bool IsFlowNode() {
			return true;
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
			if(target.isAssigned) {
				try {
					if(isDecrement) {
						Operator.Decrement(ReflectionUtils.CreateInstance(target.type));
					} else {
						Operator.Increment(ReflectionUtils.CreateInstance(target.type));
					}
				}
				catch(System.Exception ex) {
					RegisterEditorError(ex.Message);
				}
			}
		}
	}
}