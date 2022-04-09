using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MaxyGames.uNode {
    public class RuntimeGraphType : RuntimeType, ISummary, ICustomIcon {
		public uNodeRoot target { get; internal set; }

		public RuntimeGraphType(uNodeRoot target) {
			this.target = target;
		}

		public override string Name {
			get {
				if(target != null) {
					return target.GraphName;
				}
				return string.Empty;
			}
		}

		public override string Namespace {
			get {
				if(target is IIndependentGraph graph) {
					return graph.Namespace;
				}
				return base.Namespace;
			}
		}

		public override Type BaseType {
			get {
				if (target is IClass) {
					return (target as IClass).GetInheritType() ?? typeof(object);
				}
				return typeof(object);
			}
		}

        public override bool IsValid() {
			try {
				return target != null && target.gameObject != null;
			} catch {
				return false;
			}
		}

		public bool IsSingleton => target is ISingletonGraph;

		#region Build
		/// <summary>
		/// Rebuild members so the member are up to date
		/// </summary>
		public void RebuildMembers() {
			fields = null;
			properties = null;
			methods = null;
		}

		List<FieldInfo> fields;
		private void BuildFields() {
			List<FieldInfo> members = new List<FieldInfo>();
			foreach (var m in target.Variables) {
				if (m.modifier.isPublic) {
					members.Add(new RuntimeGraphField(this, m));
				}
			}
			members.AddRange(BaseType.GetFields(ReflectionUtils.publicFlags));
			fields = members;
		}

		List<PropertyInfo> properties;
		private void BuildProperties() {
			List<PropertyInfo> members = new List<PropertyInfo>();
			foreach (var m in target.Properties) {
				if (m.modifier.isPublic) {
					members.Add(new RuntimeGraphProperty(this, m));
				}
			}
			members.AddRange(BaseType.GetProperties(ReflectionUtils.publicFlags));
			properties = members;
		}

		List<MethodInfo> methods;
		private void BuildMethods() {
			List<MethodInfo> members = new List<MethodInfo>();
			foreach (var m in target.Functions) {
				if (m.modifiers.isPublic) {
					members.Add(new RuntimeGraphMethod(this, m));
				}
			}
			members.AddRange(BaseType.GetMethods(ReflectionUtils.publicFlags));
			methods = members;
		}
		#endregion

		public override bool Equals(object obj) {
			if(obj == null) {
				return target == null || base.Equals(obj);
			}
			return base.Equals(obj);
		}

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
			if (fields == null)  {
				BuildFields();
			}
			for (int i = 0; i < fields.Count;i++) {
				if(fields[i].Name == name) {
					return fields[i];
				}
			}
			return null;
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
			if (fields == null) 
			{
				BuildFields();
			}
			return fields.ToArray();
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

		public override bool IsAssignableFrom(Type c) {
			if(c == null) return false;
			if(this == c) {
				return true;
			}
			if(c.IsSubclassOf(this)) {
				return true;
			}
			if(target is IClassComponent) {
				if(c == typeof(GameObject) || c.IsSubclassOf(typeof(Component))) {
					return IsSubclassOf(typeof(Component));
				}
			}
			return false;
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

		protected override TypeAttributes GetAttributeFlagsImpl() {
			if(IsSingleton) {
				return TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed;
			}
			return TypeAttributes.Public | TypeAttributes.Class;
		}

		public override bool IsInstanceOfType(object o) {
			if(o == null || o.Equals(target)) return false;
			if (target is IClassComponent component) {
				if (o is IRuntimeComponent runtime) {
					return component.uniqueIdentifier == runtime.uniqueIdentifier;
				}
				//return false;
			} else if (target is IClassAsset asset) {
				if (o is IRuntimeAsset runtime) {
					return asset.uniqueIdentifier == runtime.uniqueIdentifier;
				}
				//return false;
			}
			var c = o.GetType();
			if(this == c) {
				return true;
			}
			if(c.IsSubclassOf(this)) {
				return true;
			}
			return false;
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