using UnityEngine;
using System.Collections;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Statement", "Using")]
	[Description("Provides a convenient node that ensures the correct use of IDisposable objects.")]
	[AddComponentMenu("")]
	public class NodeUsing : Node {
		[Hide, ValueIn("Target"), Filter(typeof(System.IDisposable))]
		public MemberData target = new MemberData();

		[HideInInspector, FlowOut("Body", displayFlowInHierarchy =false)]
		public MemberData body = new MemberData();
		[Hide, ValueOut("Value"), ObjectType("target")]
		public object value;
		[Hide, FlowOut("Next", true)]
		public MemberData onFinished = new MemberData();

		public override void OnExecute() {
			if(!body.isAssigned) {
				throw new System.Exception("body is unassigned");
			}
			if(HasCoroutineInFlow(body)) {
				owner.StartCoroutine(OnUpdate(), this);
			} else {
				using(var val = target.Get<System.IDisposable>()) {
					value = val;
					Node n;
					WaitUntil w;
					if(!body.ActivateFlowNode(out n, out w)) {
						throw new System.Exception("body is not coroutine but body is not finished.");
					}
					if(n != null) {
						jumpState = n.GetJumpState();
						if(jumpState != null) {
							Finish();
							return;
						}
					}
				}
				Finish(onFinished);
			}
		}

		IEnumerator OnUpdate() {
			using(var val = target.Get<System.IDisposable>()) {
				value = val;
				Node n;
				WaitUntil w;
				if(!body.ActivateFlowNode(out n, out w)) {
					yield return w;
				}
				if(n != null) {
					jumpState = n.GetJumpState();
					if(jumpState != null) {
						Finish();
						yield break;
					}
				}
			}
			Finish(onFinished);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(body, onFinished);
		}

		public override string GenerateCode() {
			if(!body.isAssigned) {
				throw new System.Exception("body is unassigned");
			}
			string contents = null;
			string vName = null;
			if(!CG.CanDeclareLocal(this, nameof(value), body)) {
				vName = CG.GenerateVariableName("tempVar", this);
				contents = CG.RegisterInstanceVariable(this, nameof(value), target.type) + " = " + vName + ";";
			} else {
				vName = CG.GetOutputName(this, nameof(value));
			}
			string declaration = CG.DeclareVariable(vName, target.type, target).RemoveSemicolon();
			string data = CG.Condition("using", declaration, contents.AddLineInEnd() + CG.Flow(body, this));
			return data + CG.FlowFinish(this, true, false, false, onFinished);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
			uNodeUtility.CheckError(body, this, "body");
		}

		public override string GetRichName() {
			return uNodeUtility.WrapTextWithKeywordColor("using: ") + target.GetNicelyDisplayName(richName:true);
		}
	}
}

#if UNITY_EDITOR
namespace MaxyGames.uNode.Editors.Commands {
	using System.Collections.Generic;
	using MaxyGames.uNode.Nodes;

	public class CustomInputUsingItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(ItemSelector.CustomItem.Create("Using", () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (NodeUsing n) => {
					n.target = data;
					graph.Refresh();
				});
			}, "Flows", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
			return items;
		}

		public override bool IsValidPort(System.Type type) {
			return type.IsCastableTo(typeof(System.IDisposable));
		}
	}
}
#endif