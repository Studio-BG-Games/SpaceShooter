using System;
using System.Reflection;

namespace MaxyGames.uNode {
	/// <summary>
	/// A generic implementation of RuntimeMethod with T is a instance of the method
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class RuntimeConstructor<T> : RuntimeConstructor {
		public T target;

		public RuntimeConstructor(RuntimeType owner, T target) : base(owner) {
			this.target = target;
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
	}

	public abstract class RuntimeConstructor : ConstructorInfo, IRuntimeMember {
		public readonly RuntimeType owner;

		public RuntimeConstructor(RuntimeType owner) {
			this.owner = owner;
		}

		public override MethodAttributes Attributes {
			get {
				if(owner.IsAbstract && owner.IsSealed) {
					return MethodAttributes.Public | MethodAttributes.Static;
				}
				return MethodAttributes.Public;
			}
		}
		
		public override Type DeclaringType => owner;
		public override Type ReflectedType => DeclaringType;
		public override RuntimeMethodHandle MethodHandle => default;
		public override int MetadataToken => 0;

		public override object[] GetCustomAttributes(bool inherit) {
			return new object[0];
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return new object[0];
		}

		public override MethodImplAttributes GetMethodImplementationFlags() {
			return MethodImplAttributes.Runtime;
		}

		public override bool IsDefined(Type attributeType, bool inherit) {
			return false;
		}

		public override Type[] GetGenericArguments() {
			return Type.EmptyTypes;
		}
	}
}