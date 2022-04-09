using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("★General", "GetValue")]
	public class GetValue : Action {
		public MultipurposeMember target = new MultipurposeMember();
		[ObjectType("target.target")]
		[Filter(SetMember = true)]
		public MemberData storeValue = new MemberData();

		protected override void OnExecute() {
			object val = target.Get();
			if(storeValue != null && storeValue.isTargeted) {
				storeValue.Set(val);
			}
		}

		public override string GenerateCode(Object obj) {
			string header = null;
			string footer = null;
			var rezult = CG.TryParseValue(target, storeValue, (x, y) => {
				if(!string.IsNullOrEmpty(x)) {
					header += x.AddLineInEnd();
				}
				if(!string.IsNullOrEmpty(y)) {
					footer += y.AddLineInFirst();
				}
			}).Add(";");
			return header + rezult + footer;
		}

		public override string Name {
			get {
				switch(target.target.targetType) {
					case MemberData.TargetType.Constructor:
					case MemberData.TargetType.Method:
					case MemberData.TargetType.uNodeFunction: {
						string store = null;
						if(storeValue != null && storeValue.isAssigned) {
							store = " store to <b>" + uNode.uNodeUtility.GetNicelyDisplayName(storeValue) + "</b>";
						}
						return "<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b>" + store;
					}
					case MemberData.TargetType.uNodeVariable:
					case MemberData.TargetType.uNodeProperty:
					case MemberData.TargetType.uNodeLocalVariable:
					case MemberData.TargetType.uNodeGroupVariable:
					case MemberData.TargetType.uNodeParameter: {
						if(target.target.isDeepTarget) {
							if(target.target.type == typeof(void)) {
								return "<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b>";
							} else if(storeValue == null || !storeValue.isAssigned) {
								return "<b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b>";
							}
						}
						break;
					}
				}
				return "Get: <b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) +
					"</b> store to <b>" + uNode.uNodeUtility.GetNicelyDisplayName(storeValue) + "</b>";
			}
		}

		public override string ToolTip {
			get {
				if(target != null && target.target != null) {
					return target.target.Tooltip;
				}
				return base.ToolTip;
			}
		}
	}
}