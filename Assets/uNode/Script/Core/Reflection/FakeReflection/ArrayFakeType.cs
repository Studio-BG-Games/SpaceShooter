using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MaxyGames.uNode {
	class ArrayFakeType : FakeType {
		private Type arrayType;

		public ArrayFakeType(Type type) : base(typeof(IRuntimeClass[])) {
			arrayType = type;
		}

		protected override void Initialize() {
			var members = target.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			foreach(var member in members) {
				switch(member.MemberType) {
					case MemberTypes.Field:
						declaredMembers[member] = null;
						break;
					case MemberTypes.Property:
						declaredMembers[member] = null;
						break;
					case MemberTypes.Method:
						var method = member as MethodInfo;
						switch(member.Name) {
							case "Get":
								declaredMembers[member] = new FakeMethod(this, method, arrayType, null);
								break;
							case "Set":
								declaredMembers[member] = new FakeMethod(this, method, null, new ParameterInfo[] { new FakeParameter(method.GetParameters()[0], arrayType) });
								break;
							default:
								declaredMembers[member] = null;
								break;
						}
						break;
				}
			}
		}

		public override Type GetElementType() {
			return arrayType;
		}
	}
}