using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MaxyGames.uNode {
	class FakeProperty : RuntimeProperty<PropertyInfo>, IFakeMember {
		private readonly Type propertyType;

		public FakeProperty(FakeType owner, PropertyInfo target, Type propertyType) : base(owner, target) {
			//if(propertyType is RuntimeGraphType) {
			//	propertyType = ReflectionFaker.FakeGraphType(propertyType as RuntimeGraphType);
			//}
			this.propertyType = propertyType;
		}

		public override Type PropertyType => propertyType ?? target.PropertyType;
		public override string Name => target.Name;

		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			return target.GetValue(obj);
		}

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			target.SetValue(obj, value);
		}

		public override MethodInfo GetGetMethod(bool nonPublic) {
			return null;
		}

		public override MethodInfo GetSetMethod(bool nonPublic) {
			return null;
		}

		public override string ToString() {
			return PropertyType.ToString() + " " + Name;
		}
	}

}