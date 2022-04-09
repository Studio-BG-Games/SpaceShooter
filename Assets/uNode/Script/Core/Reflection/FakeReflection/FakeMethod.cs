using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MaxyGames.uNode {
	class FakeMethod : RuntimeMethod<MethodInfo>, IFakeMember {
		private readonly Type returnType;
		private readonly ParameterInfo[] parameters;

		public FakeMethod(FakeType owner, MethodInfo target, Type returnType, ParameterInfo[] parameters) : base(owner, target) {
			//if(returnType is RuntimeGraphType) {
			//	returnType = ReflectionFaker.FakeGraphType(returnType as RuntimeGraphType);
			//}
			this.returnType = returnType;
			this.parameters = parameters;
		}

		public override string Name => target.Name;

		public override ParameterInfo[] GetParameters() {
			return parameters ?? target.GetParameters();
		}

		public override Type ReturnType => returnType ?? target.ReturnType;

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			return target.Invoke(obj, invokeAttr, binder, parameters, culture);
		}

		public override string ToString() {
			return ReturnType.ToString() + " " + Name + "(" + string.Join(", ", GetParameters().Select(p => p.ParameterType.ToString())) + ")";
		}
	}
}