using System.Collections;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Statement", "Foreach")]
	[Description("The foreach statement repeats a body of embedded statements for each element in an array or a generic type.")]
	[AddComponentMenu("")]
	public class ForeachLoop : Node {
		[Hide, ValueIn("Collection"), Filter(typeof(IEnumerable))]
		[Tooltip("The target array list or generic list for the loop")]
		public MemberData target = new MemberData();

		[HideInInspector, FlowOut("Body", displayFlowInHierarchy =false)]
		public MemberData body = new MemberData();
		[Hide, ValueOut("Value"), ObjectType("target", isElementType = true)]
		public object loopObject;

		[Hide, FlowOut("Next", true)]
		public MemberData onFinished = new MemberData();

		#region Runtime
		public override void OnExecute() {
			if(!HasCoroutineInFlow(body)) {
				IEnumerable lObj = target.Get() as IEnumerable;
				if(lObj != null) {
					foreach(object obj in lObj) {
						if(body == null || !body.isAssigned) continue;
						loopObject = obj;
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
				} else {
					Debug.LogError("The target must be IEnumerable");
				}
				Finish(onFinished);
			} else {
				owner.StartCoroutine(OnUpdate(), this);
			}
		}

		IEnumerator OnUpdate() {
			IEnumerable lObj = target.Get() as IEnumerable;
			if(lObj != null) {
				foreach(object obj in lObj) {
					if(body == null || !body.isAssigned) continue;
					loopObject = obj;
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
			} else {
				Debug.LogError("The target must be IEnumerable");
			}
			Finish(onFinished);
		}
		#endregion

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(body, onFinished);
		}

		public override string GenerateCode() {
			string ta = CG.Value((object)target);
			if(!string.IsNullOrEmpty(ta)) {
				string contents = CG.Flow(body, this).AddLineInFirst();
				string additionalContents = null;
				var targetType = target.type;
				var elementType = targetType.ElementType();
				string vName;
				if(targetType is FakeType && CG.generatePureScript) {
					if(!CG.CanDeclareLocal(this, nameof(loopObject), body)) {
						vName = CG.GenerateVariableName("tempVar", this);
						additionalContents = CG.RegisterInstanceVariable(this, nameof(loopObject), elementType) + " = " + CG.As(vName, elementType) + ";";
					} else {
						vName = CG.GenerateVariableName("tempVar", this);
						additionalContents = "var " + CG.GetOutputName(this, nameof(loopObject)) + " = " + CG.As(vName, elementType) + ";";
					}
				} else {
					if(!CG.CanDeclareLocal(this, nameof(loopObject), body)) {
						vName = CG.GenerateVariableName("tempVar", this);
						additionalContents = CG.RegisterInstanceVariable(this, nameof(loopObject), elementType) + " = " + vName + ";";
					} else {
						vName = CG.GetOutputName(this, nameof(loopObject));
					}
				}
				string loopType = targetType.GetInterface(typeof(System.Collections.Generic.IEnumerable<>).FullName) != null ? "var " : CG.Type(elementType) + " ";
				return CG.Flow(
						CG.Condition("foreach", loopType + vName + " in " + ta, additionalContents + contents),
						CG.FlowFinish(this, true, false, false, onFinished)
					);
			}
			return null;
		}

		public override string GetRichName() {
			return uNodeUtility.WrapTextWithKeywordColor("foreach:") + target.GetNicelyDisplayName(richName:true);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "Collection");
			uNodeUtility.CheckError(body, this, "body");
		}
	}
}

#if UNITY_EDITOR
namespace MaxyGames.uNode.Editors.Commands {
	using System.Collections.Generic;
	using MaxyGames.uNode.Nodes;

	public class CustomInputFEItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(ItemSelector.CustomItem.Create("Foreach", () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (ForeachLoop n) => {
					n.target = data;
					graph.Refresh();
				});
			}, "Flows", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
			return items;
		}

		public override bool IsValidPort(System.Type type) {
			return type.IsArray || type.IsCastableTo(typeof(IEnumerable));
		}
	}
}
#endif