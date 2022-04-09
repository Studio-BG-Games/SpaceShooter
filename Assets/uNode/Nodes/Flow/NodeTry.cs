using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Try-Catch-Finally", HideOnStateMachine = true)]
	[AddComponentMenu("")]
	public class NodeTry : Node {
		[Hide]
		public MemberData onFinished = new MemberData();
		[HideInInspector, FlowOut("Try", true)]
		public MemberData Try = new MemberData();
		[HideInInspector]
		public List<MemberData> Flows = new List<MemberData>();
		[HideInInspector]
		public List<MemberData> ExceptionTypes = new List<MemberData>();
		[HideInInspector]
		public MemberData Finally = new MemberData();

		[HideInInspector]
		public List<System.Exception> Exceptions = new List<System.Exception>();

		public override void OnRuntimeInitialize() {
			while(Exceptions.Count != ExceptionTypes.Count) {
				if(Exceptions.Count > ExceptionTypes.Count) {
					Exceptions.RemoveAt(Exceptions.Count - 1);
				} else {
					Exceptions.Add(null);
				}
			}
		}

		public override void OnExecute() {
			if(Finally.isAssigned) {
				if(Flows.Count > 0) {
					try {
						ExecuteFlow(Try);
					}
					catch(System.Exception ex) {
						if(jumpState != null)
							return;
						int index = 0;
						foreach(var member in ExceptionTypes) {
							if(member.isAssigned) {
								if(member.startType.IsAssignableFrom(ex.GetType())) {
									Exceptions[index] = ex;
									ExecuteFlow(Flows[index]);
									break;
								}
							} else {
								ExecuteFlow(Flows[index]);
								break;
							}
							index++;
						}
					}
					finally {
						if(jumpState == null) {
							ExecuteFlow(Finally);
						}
					}
				} else {
					try {
						ExecuteFlow(Try);
					}
					finally {
						if(jumpState == null) {
							ExecuteFlow(Finally);
						}
					}
				}
			} else if(Flows.Count > 0) {
				try {
					ExecuteFlow(Try);
				}
				catch(System.Exception ex) {
					if(jumpState != null)
						return;
					int index = 0;
					foreach(var member in ExceptionTypes) {
						if(member.isAssigned) {
							if(member.startType.IsAssignableFrom(ex.GetType())) {
								Exceptions[index] = ex;
								ExecuteFlow(Flows[index]);
								break;
							}
						} else {
							ExecuteFlow(Flows[index]);
							break;
						}
						index++;
					}
				}
			}
			if(jumpState == null) {
				Finish(onFinished);
			}
		}

		public override string GenerateCode() {
			string T = null;
			string F = null;
			if(Try.isAssigned) {
				T = CG.Flow(Try, this);
			}
			if(Finally.isAssigned) {
				F = CG.Flow(Finally, this);
			}
			string data = "try " + CG.Block(T);
			for(int i = 0; i < ExceptionTypes.Count; i++) {
				var member = ExceptionTypes[i];
				string varName = null;
				System.Type type = null;
				if(member.isAssigned) {
					type = member.startType;
				}
				if(type != null) {
					string contents = CG.Flow(Flows[i], this);
					if(!CG.CanDeclareLocal(this, nameof(Exceptions), i, Flows[i])) {
						varName = CG.GenerateVariableName("tempVar", this);
						contents = CG.RegisterInstanceVariable(this, nameof(Exceptions), i, type) + " = " + varName + ";" + contents.AddLineInFirst();
					} else {
						varName = CG.GetOutputName(this, nameof(Exceptions), i);
					}
					string declaration = CG.Type(type) + " " + varName;
					data += "\n" + CG.Condition("catch", declaration, contents);
				} else {
					data += "\ncatch " + CG.Block(CG.Flow(Flows[i], this));
					break;
				}
			}
			data += "\nfinally " + CG.Block(F);
			return data;
		}

		public override string GetNodeName() {
			return "Try-Catch-Finally";
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(Try, Finally, onFinished);
		}
	}
}