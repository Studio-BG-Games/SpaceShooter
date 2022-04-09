using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MaxyGames.uNode {
	public abstract class RuntimeType<T> : RuntimeType {
		public readonly T target;

		public RuntimeType(T target) {
			this.target = target;
		}

		public override Type BaseType => typeof(T);
	}

	public class MissingType : RuntimeType, ICustomIcon {
		public string missingType;

		public override Type BaseType => typeof(object);

		public override string Name => missingType;

		public override string ToString() {
			return "MissingType";
		}

		public MissingType() {
			missingType = "Missing Type";
		}

		public MissingType(string missingType) {
			if(string.IsNullOrEmpty(missingType)) {
				missingType = "Missing Type";
			}
			this.missingType = missingType;
		}

		public override FieldInfo GetField(string name, BindingFlags bindingAttr) {
			return null;
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
			return new FieldInfo[0];
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr) {
			return new MethodInfo[0];
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
			return new PropertyInfo[0];
		}

		protected override TypeAttributes GetAttributeFlagsImpl() {
			return TypeAttributes.NotPublic;
		}

		public Texture GetIcon() {
			return Resources.Load<Texture2D>("Icons/IconMissing");
		}
	}

	/// <summary>
	/// The base class for all RuntimeType
	/// </summary>
	public abstract class RuntimeType : Type, IRuntimeMember {
		public const string CompanyNamespace = "MaxyGames";
		public const string RuntimeNamespace = "MaxyGames.Generated";
		
		#region Operators
		public static bool operator ==(RuntimeType x, RuntimeType y) {
			if(ReferenceEquals(x, null)) {
				return ReferenceEquals(y, null);
			} else if(ReferenceEquals(y, null)) {
				return ReferenceEquals(x, null);
			}
			return x.FullName == y.FullName;//This will ensure the type should be same when the name is same.
		}

		public static bool operator !=(RuntimeType x, RuntimeType y) {
			return !(x == y);
		}


		public override bool Equals(Type o) {
			return Equals(o as object);
		}

		public override bool Equals(object obj) {
			var val = obj as RuntimeType;
			return !ReferenceEquals(val, null) && FullName == val.FullName;
		}
		#endregion

		class DefaultRuntimeType : RuntimeType {
			public override Type BaseType => typeof(object);

			public override string Name => "Default";

			public override string ToString() {
				return "default";
			}

			public override FieldInfo GetField(string name, BindingFlags bindingAttr) {
				return null;
			}

			public override FieldInfo[] GetFields(BindingFlags bindingAttr) {
				return new FieldInfo[0];
			}

			public override MethodInfo[] GetMethods(BindingFlags bindingAttr) {
				return new MethodInfo[0];
			}

			public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) {
				return new PropertyInfo[0];
			}

			protected override TypeAttributes GetAttributeFlagsImpl() {
				return TypeAttributes.NotPublic;
			}
		}

		private static RuntimeType _Default;
		public static RuntimeType Default {
			get {
				if(_Default == null) {
					_Default = new DefaultRuntimeType();
				}
				return _Default;
			}
		}

		/// <summary>
		/// Is the RuntimeType is valid?
		/// </summary>
		/// <returns></returns>
		public virtual bool IsValid() {
			return true;
		}

		public override Assembly Assembly => null;

		public override string AssemblyQualifiedName => string.Empty;

		public override string FullName => $"{Namespace}.{Name}";
		
		public override Guid GUID => Guid.Empty;

		public override Module Module => null;

		public override string Namespace {
			get {
				return RuntimeNamespace;
			}
		}

		public override Type UnderlyingSystemType => BaseType ?? this;

		public override Type DeclaringType => null;

		public override Type ReflectedType => null;

		public override GenericParameterAttributes GenericParameterAttributes => GenericParameterAttributes.None;
		
		public override Type[] GenericTypeArguments => Type.EmptyTypes;

		public override bool IsEnum => false;
		
		public override bool IsGenericParameter => false;

		public override bool IsGenericType => false;

		public override bool IsGenericTypeDefinition => false;

		public override bool ContainsGenericParameters => false;

		public override bool IsSerializable => true;

		public override bool IsSecurityCritical => false;
		public override bool IsSecuritySafeCritical => false;
		public override bool IsSecurityTransparent => true;
		public override int MetadataToken => 0;

		public override StructLayoutAttribute StructLayoutAttribute => null;

		public override RuntimeTypeHandle TypeHandle => default;

		public override bool IsConstructedGenericType => true;

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override Type[] GetGenericArguments() {
			return Type.EmptyTypes;
		}

		public override Type[] GetGenericParameterConstraints() {
			return Type.EmptyTypes;
		}

		public override Type GetGenericTypeDefinition() {
			return null;
		}

		// public virtual bool IsAssignableTo(Type type) {
		// 	if(type == null) return false;
		// 	if(this == type) return true;
		// 	if(type is RuntimeType) {
		// 		return type.IsAssignableFrom(this);
		// 	}
		// 	if(IsSubclassOf(type)) {
		// 		return true;
		// 	}
		// 	if(type.IsInterface) {
		// 		return HasImplementInterface(this, type);
		// 	}
		// 	if (type.IsGenericParameter) {
		// 		Type[] genericParameterConstraints = type.GetGenericParameterConstraints();
		// 		for (int i = 0; i < genericParameterConstraints.Length; i++) {
		// 			if (!genericParameterConstraints[i].IsAssignableFrom(this)) {
		// 				return false;
		// 			}
		// 		}
		// 		return true;
		// 	}
		// 	return BaseType != null && type.IsAssignableFrom(BaseType);
		// }

		public override bool IsAssignableFrom(Type c) {
			if (c == null) {
				return false;
			}
			if (this == c) {
				return true;
			}
			// RuntimeType runtimeType = UnderlyingSystemType as RuntimeType;
			// if (runtimeType != null) {
			// 	return runtimeType.IsAssignableFrom(c);
			// }
			if (c.IsSubclassOf(this)) {
				return true;
			}
			if (IsInterface) {
				return HasImplementInterface(c, this);
			}
			if (IsGenericParameter) {
				Type[] genericParameterConstraints = GetGenericParameterConstraints();
				for (int i = 0; i < genericParameterConstraints.Length; i++) {
					if (!genericParameterConstraints[i].IsAssignableFrom(c)) {
						return false;
					}
				}
				return true;
			}
			if(c is RuntimeType) {
				return false;
			}
			return BaseType != null && BaseType.IsAssignableFrom(c);
		}

		public static bool HasImplementInterface(Type type, Type ifaceType) {
			while (type != null) {
				Type[] interfaces = type.GetInterfaces();
				if (interfaces != null) {
					for (int i = 0; i < interfaces.Length; i++) {
						if (interfaces[i] == ifaceType || (interfaces[i] != null && HasImplementInterface(interfaces[i], ifaceType))) {
							return true;
						}
					}
				}
				type = type.BaseType;
			}
			return false;
		}

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) {
			return new ConstructorInfo[0];
		}

		public override object[] GetCustomAttributes(bool inherit) {
			return new object[0];
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return new object[0];
		}

		public override IList<CustomAttributeData> GetCustomAttributesData() {
			return new CustomAttributeData[0];
		}

		public override Type GetElementType() {
			return null;
		}

		public override EventInfo GetEvent(string name, BindingFlags bindingAttr) {
			return null;
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr) {
			return new EventInfo[0];
		}

		public override Type GetInterface(string name, bool ignoreCase) {
			return null;
		}

		public override Type[] GetInterfaces() {
			return Type.EmptyTypes;
		}

		public IEnumerable<FieldInfo> GetRuntimeFields() {
			return GetFields().Where(p => p is IRuntimeMember);
		}

		public IEnumerable<PropertyInfo> GetRuntimeProperties() {
			return GetProperties().Where(p => p is IRuntimeMember);
		}

		public IEnumerable<MethodInfo> GetRuntimeMethods() {
			return GetMethods().Where(p => p is IRuntimeMember);
		}

		public virtual MemberInfo[] GetRuntimeMembers() {
			List<MemberInfo> members = new List<MemberInfo>();
			members.AddRange(GetRuntimeFields());
			members.AddRange(GetRuntimeProperties());
			members.AddRange(GetRuntimeMethods());
			return members.ToArray();
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr) {
			var fields = GetFields();
			var properties = GetProperties();
			var methods = GetMethods();
			var members = new MemberInfo[fields.Length + properties.Length + methods.Length];
			for(int i=0;i<fields.Length;i++) {
				members[i] = fields[i];
			}
			for(int i = 0; i < properties.Length; i++) {
				members[i + fields.Length] = properties[i];
			}
			for(int i = 0; i < methods.Length; i++) {
				members[i + fields.Length + properties.Length] = methods[i];
			}
			return members;
		}

		public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr) {
			List<MemberInfo> members = new List<MemberInfo>();
			var list = GetMembers();
			foreach(var m in list) {
				if(m.Name == name && m.MemberType.HasFlags(type)) {
					members.Add(m);
				}
			}
			return members.ToArray();
		}

		public override Type GetNestedType(string name, BindingFlags bindingAttr) {
			return null;
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr) {
			return new Type[0];
		}

		public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) {
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit) {
			return false;
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			return null;
		}

		protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			if(types == null) {
				types = Type.EmptyTypes;
			}
			var members = GetMethods();
			for (int i = 0; i < members.Length;i++) {
				var method = members[i];
				if(method.Name == name) {
					var parameters = method.GetParameters();
					if(types.Length == parameters.Length) {
						bool flag = true;
						for (int x = 0; x < parameters.Length;x++) {
							if(types[x] != parameters[x].ParameterType) {
								flag = false;
								break;
							}
						}
						if(flag) {
							return method;
						}
					}
				}
			}
			return null;
		}

		protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) {
			return null;
		}

		protected override bool HasElementTypeImpl() {
			return false;
		}

		protected override bool IsArrayImpl() {
			return false;
		}

		protected override bool IsByRefImpl() {
			return false;
		}

		protected override bool IsCOMObjectImpl() {
			return false;
		}

		protected override bool IsPointerImpl() {
			return false;
		}

		protected override bool IsPrimitiveImpl() {
			return false;
		}
	}
}