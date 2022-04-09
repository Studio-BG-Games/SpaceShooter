using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Data", "IS", typeof(bool))]
	[AddComponentMenu("")]
	public class ISNode : ValueNode, IExtendedOutput {
		[Hide, FieldDrawer("Type"), Filter(OnlyGetType = true, DisplayRuntimeType =true, ArrayManipulator =true)]
		public MemberData type = new MemberData(typeof(object), MemberData.TargetType.Type);
		[Hide, ValueIn]
		public MemberData target;

		public override System.Type ReturnType() {
			return typeof(bool);
		}

		protected override object Value() {
			return Operator.TypeIs(target.Get(), type.Get<System.Type>());
		}

		public override string GenerateValueCode() {
			if(target.isAssigned && type.isAssigned) {
				return CG.Is(target, type.startType);
			}
			throw new System.Exception();
		}
		public override string GetNodeName() {
			return "IS";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true) + uNodeUtility.WrapTextWithKeywordColor(" is ") + target.GetNicelyDisplayName(richName:true, typeTargetWithTypeof:false);
		}

		public override System.Type GetNodeIcon() {
			if(type.isAssigned) {
				return type.startType;
			}
			return typeof(bool);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}

		int IExtendedOutput.OutputCount => 1;

		object IExtendedOutput.GetOutputValue(string name) {
			return target.Get(type.startType);
		}

		string IExtendedOutput.GetOutputName(int index) {
			return "Value";
		}

		Type IExtendedOutput.GetOutputType(string name) {
			return type.startType;
		}

		string IExtendedOutput.GenerateOutputCode(string name) {
			return CG.As(target, type.startType);
		}
	}
}