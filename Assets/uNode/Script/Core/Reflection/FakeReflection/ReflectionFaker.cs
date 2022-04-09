using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MaxyGames.uNode {
	public static class ReflectionFaker {
		static Dictionary<int, FakeType> genericFakeTypes = new Dictionary<int, FakeType>();
		static Dictionary<int, FakeType> runtimeFakeTypes = new Dictionary<int, FakeType>();

		public static FakeType FakeGraphType(RuntimeGraphType type) {
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			if(!runtimeFakeTypes.TryGetValue(type.GetHashCode(), out var result)) {
				result = new FakeGraphType(type);
				runtimeFakeTypes[type.GetHashCode()] = result;
			}
			return result;
		}

		public static FakeType FakeInterfaceType(RuntimeGraphInterface type) {
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			if(!runtimeFakeTypes.TryGetValue(type.GetHashCode(), out var result)) {
				result = new FakeGraphInterface(type);
				runtimeFakeTypes[type.GetHashCode()] = result;
			}
			return result;
		}

		public static FakeType FakeGenericType(Type type, Type[] typeArguments) {
			if(typeArguments == null) {
				throw new ArgumentNullException(nameof(typeArguments));
			}
			if(type.IsConstructedGenericType) {
				//throw new ArgumentException("Invalid type: a type must non-constructed type", nameof(type));
				type = type.GetGenericTypeDefinition();
			}
			int hash = type.GetHashCode();
			for(int i = 0; i < typeArguments.Length; i++) {
				if(typeArguments[i] == null) {
					throw new ArgumentNullException(nameof(typeArguments), "Null type argument at index:" + i);
				}
				hash += typeArguments[i].GetHashCode();
			}
			if(!genericFakeTypes.TryGetValue(hash, out var result)) {
				result = new GenericFakeType(type, typeArguments);
				genericFakeTypes[hash] = result;
			}
			return result;
		}

		static Dictionary<int, FakeType> arrayFakeTypes = new Dictionary<int, FakeType>();

		public static FakeType FakeArrayType(Type type) {
			int hash = type.GetHashCode();
			if(!arrayFakeTypes.TryGetValue(hash, out var result)) {
				result = new ArrayFakeType(type);
				//arrayFakeTypes[hash] = result;
			}
			return result;
		}
	}
}