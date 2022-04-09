﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MaxyGames.uNode {
    public class RuntimeGraphInterface : RuntimeType, ISummary, ICustomIcon {
		public readonly uNodeInterface target;

		public RuntimeGraphInterface(uNodeInterface target) {
			this.target = target;
		}

		public override string Name => target != null ? target.name : string.Empty;
		public override string Namespace => string.IsNullOrEmpty(target.@namespace) ? RuntimeType.RuntimeNamespace : target.@namespace;

		public override Type BaseType => null;

		public override bool IsValid() {
			try {
				return target != null;
			} catch {
				return false;
			}
		}
		
		#region Build
		/// <summary>
		/// Rebuild members so the member are up to date
		/// </summary>
		public void RebuildMembers() {
			properties = null;
			methods = null;
		}

		List<PropertyInfo> properties;
		private void BuildProperties() {
			List<PropertyInfo> members = new List<PropertyInfo>();
			foreach (var m in target.properties) {
				members.Add(new RuntimeInterfaceProperty(this, m));
			}
			properties = members;
		}

		List<MethodInfo> methods;
		private void BuildMethods() {
			List<MethodInfo> members = new List<MethodInfo>();
			foreach (var m in target.functions) {
				members.Add(new RuntimeInterfaceMethod(this, m));
			}
			methods = members;
		}
		#endregion
		
		public override int GetHashCode() {
			return target.GetHashCode();
		}

		public override object[] GetCustomAttributes(bool inherit) {
			if (target is IAttributeSystem) {
				return (target as IAttributeSystem).GetAttributes();
			}
			return new object[0];
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			if (target is IAttributeSystem) {
				return (target as IAttributeSystem).GetAttributes(attributeType);
			}
			return new object[0];
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr) {
			return null;
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
			return new FieldInfo[0];
		}

		public override Type[] GetInterfaces() {
			if (target is IInterfaceSystem interfaceSystem) {
				var ifaces = interfaceSystem.Interfaces;
				if(ifaces != null) {
					List<Type> types = new List<Type>();
					foreach(var iface in ifaces) {
						if(iface != null && iface.isAssigned) {
							types.Add(iface.startType);
						}
					}
					return types.ToArray();
				}
			}
			return Type.EmptyTypes;
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr) {
			if (methods == null) {
				BuildMethods();
			}
			return methods.ToArray();
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
			if (properties == null) {
				BuildProperties();
			}
			return properties.ToArray();
		}
		
		public override bool IsInstanceOfType(object o) {
			if(o == null) return false;
			if (o is IRuntimeInterface runtime) {
				return runtime.GetInterfaces().Contains(this);
			}
			Type nativeType = FullName.ToType(false);
			if(nativeType != null) {
				return nativeType.IsInstanceOfType(o);
			}
			return false;
		}

		protected override TypeAttributes GetAttributeFlagsImpl() {
			return TypeAttributes.Public | TypeAttributes.ClassSemanticsMask | TypeAttributes.Interface;
		}

		public string GetSummary() {
			return target.summary;
		}

		public Texture GetIcon() {
			if(target is ICustomIcon customIcon) {
				return customIcon.GetIcon();
			}
			return null;
		}
	}
}