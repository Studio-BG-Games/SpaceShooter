using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "MakeArray", typeof(System.Array))]
	[AddComponentMenu("")]
	public class MakeArrayNode : ValueNode {
		[Filter(OnlyGetType = true)]
		public MemberData elementType = new MemberData(typeof(object), MemberData.TargetType.Type);
		[Filter(typeof(int)), ValueIn("Length"), Hide]
		public MemberData arrayLength = new MemberData();
		[HideInInspector, ObjectType("elementType")]
		public List<MemberData> values = new List<MemberData>() { new MemberData(new object()) };

		public override System.Type ReturnType() {
			if(elementType.isAssigned) {
				System.Type type = elementType.Get<System.Type>();
				if(type != null) {
					return type.MakeArrayType();
				}
			}
			return typeof(System.Array);
		}

		protected override object Value() {
			System.Array array = System.Array.CreateInstance(elementType.Get<System.Type>(),
				arrayLength.isAssigned ? arrayLength.Get<int>() : values.Count);
			for(int i = 0; i < values.Count; i++) {
				array.SetValue(values[i].Get(), i);
			}
			return array;
		}

		public override string GenerateValueCode() {
			if(elementType.isAssigned && elementType.Get<System.Type>() != null) {
				return CG.MakeArray(
					elementType.Get<System.Type>(), 
					arrayLength,
					values.Select(item => CG.Value(item)).ToArray());
			}
			return null;
		}

		public override string GetNodeName() {
			return "MakeArray";
		}

		public override string GetRichName() {
			string length = null;
			if(arrayLength.isAssigned) {
				length = arrayLength.GetNicelyDisplayName(richName: true);
			}
			return $"{uNodeUtility.WrapTextWithKeywordColor("new")} {elementType.GetNicelyDisplayName(typeTargetWithTypeof:false)}[{length}] ( {string.Join(", ", from val in values select val.GetNicelyDisplayName(richName: true))} )";
		}
	}
}