using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "Select")]
	[AddComponentMenu("")]
	public class SelectNode : ValueNode {
		[Filter(OnlyGetType = true)]
		public MemberData targetType = new MemberData(typeof(object));
		[Hide, ValueIn("Value"), Filter(typeof(int), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(uint), typeof(string), typeof(System.Enum), InvalidTargetType = MemberData.TargetType.Null)]
		public MemberData target = new MemberData();
		[HideInInspector]
		public List<MemberData> values = new List<MemberData>();
		[HideInInspector, ObjectType("targetType")]
		public List<MemberData> targetNodes = new List<MemberData>();
		[HideInInspector]
		public MemberData defaultTarget = new MemberData();

		public override System.Type ReturnType() {
			if(targetType.isAssigned) {
				try {
					return targetType.Get<Type>();
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			if(!target.isAssigned)
				throw new Exception("target is unassigned");
			object val = target.Get();
			if(object.ReferenceEquals(val, null))
				throw new Exception("value is null");
			for(int i = 0; i < values.Count; i++) {
				MemberData member = values[i];
				if(member == null || !member.isAssigned)
					continue;
				object mVal = member.Get();
				if(mVal.Equals(val)) {
					return targetNodes[i].Get();
				}
			}
			return defaultTarget.Get();
		}

		public override string GenerateValueCode() {
			if(!target.isAssigned)
				throw new Exception("target is unassigned");
			var vName = CG.GenerateVariableName("sVal");
			string result = null;
			for(int i = 0; i < values.Count; i++) {
				var val = values[i];
				if(val.isAssigned) {
					if(result == null) {
						result += CG.And(CG.Value(target) + " is var " + vName, CG.Compare(vName, CG.Value(val)));
					} else {
						result += CG.Compare(vName, CG.Value(val));
					}
					result += " ? " + CG.Value(targetNodes[i]) + " : ";
				}
			}
			if(defaultTarget.isAssigned) {
				result += CG.Value(defaultTarget);
			} else {
				result += CG.Value(ReflectionUtils.CreateInstance(targetType.Get<Type>()));
			}
			return result.Wrap();
		}

		public override string GetNodeName() {
			return "Select";
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}