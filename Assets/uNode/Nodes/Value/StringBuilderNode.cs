using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "StringBuilder", typeof(string))]
	[AddComponentMenu("")]
	public class StringBuilderNode : ValueNode {
		[HideInInspector, ObjectType(typeof(string))]
		public List<MemberData> stringValues = new List<MemberData>() { new MemberData(""), new MemberData("") };

		public override System.Type ReturnType() {
			return typeof(string);
		}

		protected override object Value() {
			string builder = null;
			for(int i = 0; i < stringValues.Count; i++) {
				builder += stringValues[i].Get<string>();
			}
			return builder;
		}

		public override string GenerateValueCode() {
			if(stringValues.Count > 0) {
				string builder = null;
				for(int i = 0; i < stringValues.Count; i++) {
					if(i != 0)
						builder += " + ";
					builder += CG.Value((object)stringValues[i]);
				}
				return builder;
			}
			return "null";
		}

		public override string GetNodeName() {
			return "StringBuilder";
		}

		public override string GetRichName() {
			if(stringValues.Count > 0) {
				return string.Join(" + ", from s in stringValues select s.GetNicelyDisplayName(richName: true));
			}
			return "null";
		}
	}
}