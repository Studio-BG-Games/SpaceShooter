using System.Collections;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Statement", "For")]
	[Description("The for number statement can run a node repeatedly until a condition evaluates to false.")]
	[AddComponentMenu("")]
	public class ForNumberLoop : Node {
		[Hide, FlowOut("Body", displayFlowInHierarchy =false)]
		public MemberData body = new MemberData();
		[Hide, FlowOut("Next", true)]
		public MemberData onFinished = new MemberData();

		[Filter(typeof(int), typeof(float), typeof(decimal), typeof(long), typeof(byte), typeof(sbyte),
			typeof(short), typeof(double), typeof(uint), typeof(ulong), typeof(ushort), OnlyGetType = true, UnityReference = false)]
		public MemberData indexType = new MemberData(typeof(int), MemberData.TargetType.Type);
		[Hide, ValueIn("Start"), ObjectType("indexType", isElementType = true)]
		public MemberData startIndex = new MemberData(0);
		public ComparisonType compareType = ComparisonType.LessThan;
		[Hide, ValueIn("Count"), ObjectType("indexType", isElementType = true)]
		public MemberData compareNumber = new MemberData(10);
		public SetType iteratorSetType = SetType.Add;
		[Hide, ValueIn("Step"), ObjectType("indexType", isElementType = true)]
		public MemberData iteratorSetValue = new MemberData(1);
		[Hide, ValueOut("Index"), ObjectType("indexType", isElementType = true)]
		public object index = 0;

		public override void OnExecute() {
			if(!HasCoroutineInFlow(body)) {
				for(index = startIndex.Get(); uNodeHelper.OperatorComparison(index, compareNumber.Get(), compareType);
					uNodeHelper.SetObject(ref index, iteratorSetValue.Get(), iteratorSetType)) {
					if(!body.isAssigned) continue;
					Node n;
					WaitUntil w;
					if(!body.ActivateFlowNode(out n, out w)) {
						throw new System.Exception("body is not coroutine but body is not finished.");
					}
					if(n == null)//Skip on executing flow input pin.
						continue;
					JumpStatement js = n.GetJumpState();
					if(js != null) {
						if(js.jumpType == JumpStatementType.Continue) {
							continue;
						} else {
							if(js.jumpType == JumpStatementType.Return) {
								jumpState = js;
								Finish();
								return;
							}
							break;
						}
					}
				}
				Finish(onFinished);
			} else {
				owner.StartCoroutine(OnUpdate(), this);
			}
		}

		IEnumerator OnUpdate() {
			for(index = startIndex.Get(); uNodeHelper.OperatorComparison(index, compareNumber.Get(), compareType);
				uNodeHelper.SetObject(ref index, iteratorSetValue.Get(), iteratorSetType)) {
				if(!body.isAssigned) continue;
				Node n;
				WaitUntil w;
				if(!body.ActivateFlowNode(out n, out w)) {
					yield return w;
				}
				if(n == null)//Skip on executing flow input pin.
					continue;
				JumpStatement js = n.GetJumpState();
				if(js != null) {
					if(js.jumpType == JumpStatementType.Continue) {
						continue;
					} else {
						if(js.jumpType == JumpStatementType.Return) {
							jumpState = js;
							Finish();
							yield break;
						}
						break;
					}
				}
			}
			Finish(onFinished);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(body, onFinished);
		}

		public override string GenerateCode() {
			if(!startIndex.isAssigned || !compareNumber.isAssigned || !iteratorSetValue.isAssigned) return null;
			string indexName;
			string declaration = CG.Type(indexType) + " ";
			if(!CG.CanDeclareLocal(this, nameof(index), body)) {
				indexName = CG.RegisterInstanceVariable(this, nameof(index), startIndex.type);
				declaration = null;
			} else {
				indexName = CG.GetOutputName(this, nameof(index));
			}
			string data = CG.GetCompareCode(indexName, compareNumber, compareType);
			string iterator = CG.Set(indexName, iteratorSetValue, iteratorSetType);
			if(!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(iterator)) {
				var content = CG.FlowFinish(this, true, false, false, onFinished);
				data = CG.For(declaration +
                    CG.Set(indexName,
						(object)CG.Value((object)startIndex),
                        SetType.Change).RemoveLast(), data, iterator.RemoveSemicolon(),
                    CG.Flow(body, this)) +
					content.AddFirst("\n");
				return data;
			}
			return null;
		}

		public override string GetRichName() {
			if(!startIndex.isAssigned || !iteratorSetValue.isAssigned || !indexType.isAssigned || !compareNumber.isAssigned) {
				return base.GetRichName();
			}
			return $"{uNodeUtility.WrapTextWithKeywordColor("for")}({uNodeUtility.GetNicelyDisplayName(indexType, typeTargetWithTypeof:false)} i={startIndex.GetNicelyDisplayName(richName:true)}; {CG.Compare("i", compareNumber.GetNicelyDisplayName(richName:true), compareType)}; {CG.Set("i", iteratorSetValue.GetNicelyDisplayName(richName:true), iteratorSetType).RemoveSemicolon()})";
		}
	}
}
