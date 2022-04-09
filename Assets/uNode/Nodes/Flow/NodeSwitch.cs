using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Flow", "Switch")]
	[AddComponentMenu("")]
	public class NodeSwitch : Node {
		[Hide, FlowOut(finishedFlow = true)]
		public MemberData onFinished = new MemberData();
		[Hide, ValueIn, Filter(typeof(int), typeof(float), typeof(double), typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(uint), typeof(string), typeof(System.Enum), InvalidTargetType = MemberData.TargetType.Null)]
		public MemberData target = new MemberData();
		[HideInInspector]
		public List<MemberData> values = new List<MemberData>();
		[HideInInspector, FlowOut]
		public List<MemberData> targetNodes = new List<MemberData>();
		[HideInInspector, FlowOut]
		public MemberData defaultTarget = new MemberData();

		public override void OnExecute() {
			if(target == null || !target.isAssigned)
				return;
			object val = target.Get();
			if(object.ReferenceEquals(val, null))
				return;
			for(int i = 0; i < values.Count; i++) {
				MemberData member = values[i];
				if(member == null || !member.isAssigned)
					continue;
				object mVal = member.Get();
				if(mVal.Equals(val)) {
					Finish(targetNodes[i], onFinished);
					return;
				}
			}
			Finish(defaultTarget, onFinished);
		}

		public override void OnGeneratorInitialize() {
			if(CG.Nodes.HasStateFlowInput(this)) {
				CG.RegisterAsStateNode(this);
				for(int i = 0; i < targetNodes.Count; i++) {
					CG.RegisterAsStateNode(targetNodes[i].GetTargetNode());
				}
				CG.RegisterAsStateNode(defaultTarget.GetTargetNode());
				CG.SetStateInitialization(this, () => {
					if(target.isAssigned) {
						string data = CG.Value(target);
						if(!string.IsNullOrEmpty(data)) {
							bool hasDefault = defaultTarget != null && defaultTarget.isAssigned;
							string[] cases = new string[values.Count];
							string[] contents = new string[values.Count];
							for(int i = 0; i < cases.Length; i++) {
								cases[i] = CG.Value(values[i]);
							}
							for(int i = 0; i < contents.Length; i++) {
								contents[i] = CG.ReturnEvent(targetNodes[i]);
							}
							return CG.Routine(
								CG.Routine(
									CG.Lambda(
										CG.Flow(
											CG.Switch(data, cases, contents, hasDefault ? CG.ReturnEvent(defaultTarget) : null),
											CG.Return(null)
										)
									)
								),
								CG.Routine(CG.GetEvent(onFinished))
							);
						}
						throw new System.Exception("Can't Parse target");
					}
					throw new System.Exception("Target is unassigned");
				});
			}
		}

		public override string GenerateCode() {
			if(target.isAssigned) {
				string data = CG.Value(target);
				if(!string.IsNullOrEmpty(data)) {
					bool hasDefault = defaultTarget != null && defaultTarget.isAssigned;
					string[] cases = new string[values.Count];
					string[] contents = new string[values.Count];
					for(int i = 0; i < cases.Length; i++) {
						cases[i] = CG.Value(values[i]);
					}
					for(int i = 0; i < contents.Length; i++) {
						contents[i] = CG.Flow(targetNodes[i], this);
					}
					return CG.Flow(
						CG.Switch(data, cases, contents, hasDefault ? CG.Flow(defaultTarget, this) : null),
						CG.FlowFinish(this, true, false, false, onFinished)
					);
				}
				throw new System.Exception("Can't Parse target");
			}
			throw new System.Exception("Target is unassigned");
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.BranchIcon);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(targetNodes) || HasCoroutineInFlow(onFinished);
		}

		public override string GetRichName() {
			return $"{uNodeUtility.WrapTextWithKeywordColor("switch")}: {target.GetNicelyDisplayName(richName: true)}";
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}

#if UNITY_EDITOR
namespace MaxyGames.uNode.Editors.Commands {
	using System.Collections.Generic;
	using MaxyGames.uNode.Nodes;

	public class CustomInputSwitchItem : CustomInputPortItem {
		public override IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type) {
			var items = new List<ItemSelector.CustomItem>();
			items.Add(ItemSelector.CustomItem.Create("Switch", () => {
				NodeEditorUtility.AddNewNode(graph.editorData, null, null, mousePositionOnCanvas, (NodeSwitch n) => {
					n.target = data;
					graph.Refresh();
				});
			}, "Flows", icon: uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.FlowIcon))));
			return items;
		}

		public override bool IsValidPort(Type type) {
			return type.IsPrimitive || type.IsEnum || type == typeof(string);
		}
	}
}
#endif