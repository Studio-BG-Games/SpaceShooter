using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames.uNode.Nodes {
	//[NodeMenu("Group", "ForeachLoop")]
	[AddComponentMenu("")]
	public class ForeachLoopGroup : GroupNode {
		[Filter(OnlyArrayType = true, OnlyGenericType = true)]
		[Tooltip("The target array list or generic list for the loop")]
		public MemberData targetArray;

		[System.NonSerialized]
		public VariableData Item = new VariableData("foreachItem") { onlyGet = true };
		[System.NonSerialized]
		private bool hasInitialize = false;

		public override void OnExecute() {
			if(!hasInitialize) {
				if(targetArray.type.IsArray) {
					Item.Type = targetArray.type.GetElementType();
				} else {
					Item.Type = targetArray.type.GetGenericArguments()[0];
				}
				hasInitialize = true;
			}
			if(IsCoroutine()) {
				owner.StartCoroutine(OnUpdate(), this);
			} else {
				object tObj = targetArray.Get();
				if(targetArray.type.IsGenericType || targetArray.type.IsArray) {
					IEnumerable lObj = tObj as IEnumerable;
					foreach(object obj in lObj) {
						Item.Set(obj);
						JumpStatement js = nodeToExecute.ActivateAndFindJumpState();
						if(js != null) {
							if(js.jumpType == JumpStatementType.Continue) {
								continue;
							} else {
								if(js.jumpType == JumpStatementType.Return) {
									jumpState = js;
								}
								break;
							}
						}
					}
				} else {
					throw new System.Exception("The target array must be array list or generic list");
				}
				Finish();
			}
		}

		IEnumerator OnUpdate() {
			object tObj = targetArray.Get();
			if(targetArray.type.IsGenericType || targetArray.type.IsArray) {
				IEnumerable lObj = tObj as IEnumerable;
				foreach(object obj in lObj) {
					Item.Set(obj);
					JumpStatement js = nodeToExecute.ActivateAndFindJumpState();
					if(!nodeToExecute.IsFinished()) {
						yield return nodeToExecute.WaitUntilFinish();
					}
					if(js != null) {
						if(js.jumpType == JumpStatementType.Continue) {
							continue;
						} else {
							if(js.jumpType == JumpStatementType.Return) {
								jumpState = js;
							}
							break;
						}
					}
				}
			} else {
				throw new System.Exception("The target array must be array list or generic list");
			}
			Finish();
		}

		public override VariableData GetVariableData(string variableName) {
			#if UNITY_EDITOR
			if(targetArray.type != null /*&& Item.Type != targetArray.type*/) {
				if(targetArray.type.IsArray) {
					Item.Type = targetArray.type.GetElementType();
				} else {
					Item.Type = targetArray.type.GetGenericArguments()[0];
				}
			}
			#endif
			return Item;
		}

		[System.NonSerialized]
		private List<VariableData> cachedVar;
		public override List<VariableData> Variables {
			get {
				if(targetArray != null && targetArray.isAssigned && targetArray.type != null && (targetArray.type.IsArray || targetArray.type.IsGenericType)) {
					if(cachedVar == null) {
						cachedVar = new List<VariableData>() { Item };
						if(targetArray.type.IsArray) {
							cachedVar[0].Type = targetArray.type.GetElementType();
						} else {
							cachedVar[0].Type = targetArray.type.GetGenericArguments()[0];
						}
					}
					return cachedVar;
				} else {
					Item.type = new MemberData(typeof(object), MemberData.TargetType.Type);
				}
				return new List<VariableData>() { Item };
			}
		}

		public override string GenerateCode() {
			string ta = CG.Value((object)targetArray);
			if(!string.IsNullOrEmpty(ta)) {
				string data = null;
				string varName;
				if(!CG.CanDeclareLocal(this)) {
					Variables[0].modifier.SetPrivate();
					varName = CG.GenerateVariableName("foreach_item", this);
					CG.RegisterVariable(Variables[0]);
					data += CG.GetVariableName(Variables[0]) + " = " + varName + ";";
				} else {
					varName = CG.RegisterVariable(Variables[0], false);
				}
				string result = "foreach(" + CG.Type(targetArray.type.IsArray ? targetArray.type.GetElementType() : targetArray.type.GetGenericArguments()[0]) + " " + varName + " in " + ta + "){";
				if(!string.IsNullOrEmpty(data)) {
					result += ("\n" + data).AddTabAfterNewLine(1);
				}
				string tNode = CG.GenerateFlowCode(nodeToExecute, this);
				if(!string.IsNullOrEmpty(tNode)) {
					result += ("\n" + tNode).AddTabAfterNewLine(1) + "\n";
				} else if(!string.IsNullOrEmpty(data)) {
					result += "\n";
				}
				return result + "}" + CG.FlowFinish(this, true, false, false).AddLineInFirst();
			}
			return null;
		}
	}
}
