using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	// [NodeMenu("Operator", "Arithmetic {+} {-} {/} {*} {%} {^}")]
	[AddComponentMenu("")]
	public class MultiArithmeticNode : ValueNode, IRefreshable {
		public ArithmeticType operatorType = ArithmeticType.Add;

		[HideInInspector]
		public List<MemberData> targets = new List<MemberData>() { new MemberData(0), new MemberData(0) };
		[HideInInspector]
		public List<SerializedType> targetTypes = new List<SerializedType>();

		public override System.Type ReturnType() {
			Refresh();
			try {
				bool isDivide = operatorType == ArithmeticType.Divide || operatorType == ArithmeticType.Modulo;
				object obj = ReflectionUtils.CreateInstance(targetTypes[0].type);
				if(isDivide) {
					//For fix zero divide error.
					obj = Operator.IncrementPrimitive(obj);
				}
				for(int i = 1; i < targetTypes.Count; i++) {
					object obj2 = ReflectionUtils.CreateInstance(targetTypes[i].type);
					if(isDivide) {
						//For fix zero divide error.
						obj2 = Operator.IncrementPrimitive(obj2);
					}
					obj = uNodeHelper.ArithmeticOperator(obj, obj2, operatorType);
				}
				if(!object.ReferenceEquals(obj, null)) {
					return obj.GetType();
				}
			}
			catch { }
			return typeof(object);
		}

		protected override object Value() {
			Refresh();
			object obj = targets[0].Get(targetTypes[0].type);
			for(int i = 1; i < targets.Count; i++) {
				obj = uNodeHelper.ArithmeticOperator(obj, targets[i].Get(targetTypes[i].type), operatorType);
			}
			if(!object.ReferenceEquals(obj, null)) {
				return obj;
			}
			throw null;
		}

		public override string GenerateValueCode() {
			string contents = targets[0].CGValue();
			for(int i = 1; i < targets.Count; i++) {
				contents = CG.Arithmetic(contents, targets[i].CGValue(), operatorType).Wrap();
			}
			return contents;
		}

		public override string GetNodeName() {
			return operatorType.ToString();
		}

		public override string GetRichName() {
			string separator = null;
			switch(operatorType) {
				case ArithmeticType.Add:
					separator = " + ";
					break;
				case ArithmeticType.Divide:
					separator = " / ";
					break;
				case ArithmeticType.Modulo:
					separator = " % ";
					break;
				case ArithmeticType.Multiply:
					separator = " * ";
					break;
				case ArithmeticType.Subtract:
					separator = " - ";
					break;
			}
			return string.Join(separator, from target in targets select target.GetNicelyDisplayName(richName: true));
		}

		public override System.Type GetNodeIcon() {
			switch(operatorType) {
				case ArithmeticType.Add:
					return typeof(TypeIcons.AddIcon2);
				case ArithmeticType.Divide:
					return typeof(TypeIcons.DivideIcon2);
				case ArithmeticType.Subtract:
					return typeof(TypeIcons.SubtractIcon2);
				case ArithmeticType.Multiply:
					return typeof(TypeIcons.MultiplyIcon2);
				case ArithmeticType.Modulo:
					return typeof(TypeIcons.ModuloIcon2);
			}
			return typeof(TypeIcons.CalculatorIcon);
		}

		public override void CheckError() {
			base.CheckError();
			bool flag = uNodeUtility.CheckError(targets, this, "targets");
			if(!flag) {
				try {
					bool isDivide = operatorType == ArithmeticType.Divide || operatorType == ArithmeticType.Modulo;
					object obj = ReflectionUtils.CreateInstance(targetTypes[0].type);
					//if(isDivide) {
					//	//For fix zero divide error.
					//	obj = Operator.IncrementPrimitive(obj);
					//}
					for(int i = 1; i < targetTypes.Count; i++) {
						object obj2 = ReflectionUtils.CreateInstance(targetTypes[i].type);
						if(isDivide) {
							//For fix zero divide error.
							obj2 = Operator.IncrementPrimitive(obj2);
						}
						obj = uNodeHelper.ArithmeticOperator(obj, obj2, operatorType);
					}
				}
				catch(System.Exception ex) {
					RegisterEditorError(ex.Message);
				}
			}
		}

		public void Refresh() {
			if(targets.Count < 2) {
				targets.Add(MemberData.CreateFromValue(0));
			}
			while(targetTypes.Count != targets.Count) {
				if(targetTypes.Count > targets.Count) {
					targetTypes.RemoveAt(targetTypes.Count - 1);
				} else {
					var type = targets[targetTypes.Count].type;
					if(type == null) {
						type = typeof(int);
					}
					targetTypes.Add(type);
				}
			}
		}
	}
}