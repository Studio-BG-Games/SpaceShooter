using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MaxyGames.uNode;

namespace MaxyGames {
	public static class Extensions {
		#region Variables
		private static readonly Type[][] typeHierarchy = {
			new Type[] { typeof(Byte), typeof(SByte), typeof(Char)},
			new Type[] { typeof(Int16), typeof(UInt16)},
			new Type[] { typeof(Int32), typeof(UInt32)},
			new Type[] { typeof(Int64), typeof(UInt64)},
			new Type[] { typeof(Single)},
			new Type[] { typeof(Double)}
		};
		private static Dictionary<Type, string> shorthandMap = new Dictionary<Type, string> {
			{ typeof(Boolean), "bool" },
			{ typeof(Byte), "byte" },
			{ typeof(Char), "char" },
			{ typeof(Decimal), "decimal" },
			{ typeof(Double), "double" },
			{ typeof(Single), "float" },
			{ typeof(Int32), "int" },
			{ typeof(Int64), "long" },
			{ typeof(SByte), "sbyte" },
			{ typeof(Int16), "short" },
			{ typeof(string), "string" },
			{ typeof(UInt32), "uint" },
			{ typeof(UInt64), "ulong" },
			{ typeof(UInt16), "ushort" },
			{ typeof(void), "void" },
			{ typeof(object), "object" },
		};
		private static Dictionary<Type, Dictionary<Type, bool>> castableMap = new Dictionary<Type, Dictionary<Type, bool>>();
		private static Dictionary<Type, Dictionary<Type, bool>> castableMap2 = new Dictionary<Type, Dictionary<Type, bool>>();
		private static Dictionary<MemberInfo, Dictionary<Type, bool>> definedMap = new Dictionary<MemberInfo, Dictionary<Type, bool>>();
		private static Dictionary<Enum, Dictionary<Enum, bool>> enumsMap = new Dictionary<Enum, Dictionary<Enum, bool>>();
		private static object _lockObject = new object();
		private static object _lockObject2 = new object();
		#endregion

		#region Reflection
		private static Dictionary<long, FieldInfo> cachedFields = new Dictionary<long, FieldInfo>();
		private static Dictionary<long, PropertyInfo> cachedProperties = new Dictionary<long, PropertyInfo>();
		private static Dictionary<long, MemberInfo> cachedMembers = new Dictionary<long, MemberInfo>();

		public static FieldInfo GetFieldCached(this Type type, string name) {
			var uid = uNodeUtility.GenerateUID((long)type.GetHashCode(), (long)name.GetHashCode());
			if(!cachedFields.TryGetValue(uid, out var result)) {
				result = type.GetField(name, MemberData.flags);
				cachedFields.Add(uid, result);
			}
			return result;
		}

		public static PropertyInfo GetPropertyCached(this Type type, string name) {
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			var uid = uNodeUtility.GenerateUID((long)type.GetHashCode(), (long)name.GetHashCode());
			if(!cachedProperties.TryGetValue(uid, out var result)) {
				result = type.GetProperty(name, MemberData.flags);
				cachedProperties.Add(uid, result);
			}
			return result;
		}

		public static MemberInfo GetMemberCached(this Type type, string name) {
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			var uid = uNodeUtility.GenerateUID((long)type.GetHashCode(), (long)name.GetHashCode());
			if(!cachedMembers.TryGetValue(uid, out var result)) {
				result = type.GetMember(name, MemberData.flags).FirstOrDefault();
				cachedMembers.Add(uid, result);
			}
			return result;
		}
		#endregion

		public static int GetTimeUID(this DateTime date) {
			return int.Parse(string.Concat(date.Day, date.Hour, date.Minute, date.Second));
		}

		//public static bool Contains<T>(this T[] array, T value) {
		//	return (array as IList<T>).Contains(value);
		//}

		//public static void AddRange<T>(this HashSet<T> hash, IEnumerable<T> values) {
		//	foreach(var value in values) {
		//		hash.Add(value);
		//	}
		//}

		/// <summary>
		/// Indicates whether custom attributes of a specified type are applied to a specified member.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="attributeType"></param>
		/// <returns></returns>
		public static bool IsDefinedAttribute(this MemberInfo info, Type attributeType) {
			Dictionary<Type, bool> map = null;
			lock(_lockObject) {
				if(definedMap.TryGetValue(info, out map)) {
					bool val;
					if(map.TryGetValue(attributeType, out val)) {
						return val;
					}
				}
			}
			bool result = info.IsDefined(attributeType, true);
			lock(_lockObject) {
				if(map == null) {
					map = new Dictionary<Type, bool>();
				}
				map[attributeType] = result;
				definedMap[info] = map;
			}
			return result;
		}
		
		/// <summary>
		/// Get the attributes from a attribute system
		/// </summary>
		/// <param name="attributeSystem"></param>
		/// <returns></returns>
		public static object[] GetAttributes(this uNode.IAttributeSystem attributeSystem) {
			var attribut = attributeSystem.Attributes;
			if(attribut == null) {
				return new object[0];
			}
			object[] att = new object[attribut.Count];
			for (int i = 0; i < att.Length;i++) {
				att[i] = attribut[i].value?.Get();
			}
			return att;
		}

		/// <summary>
		/// Get the attributes from a attribute system
		/// </summary>
		/// <param name="attributeSystem"></param>
		/// <returns></returns>
		public static object[] GetAttributes(this uNode.IAttributeSystem attributeSystem, Type attributeType) {
			var attribut = attributeSystem.Attributes;
			if(attribut == null) {
				return new object[0];
			}
			for (int i = 0; i < attribut.Count;i++) {
				if(attribut[i].type.startType == attributeType) {
					return new object[] { attribut[i].value?.Get() };
				}
			}
			return new object[0];
		}

		/// <summary>
		/// Is the type is Runtime Type?
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsRuntimeType(this Type type) {
			return type is uNode.RuntimeType;
		}

		/// <summary>
		/// Convert the type into RuntimeType
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static uNode.RuntimeType AsRuntimeType(this Type type) {
			return type as uNode.RuntimeType;
		}

		/// <summary>
		/// Is the obj is instance of type of T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsTypeOf<T>(this object obj) {
			if(obj is uNodeSpawner spawner) {
				return spawner.GetRuntimeInstance() is T;
			} else if(obj is uNodeAssetInstance asset) {
				return asset.GetRuntimeInstance() is T;
			}
			return obj is T;
		}

		/// <summary>
		/// Is the obj is instance of type of 'type'
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsTypeOf(this object obj, Type type) {
			if(obj is uNodeSpawner spawner) {
				return Operator.TypeIs(spawner.GetRuntimeInstance(), type);
			} else if(obj is uNodeAssetInstance asset) {
				return Operator.TypeIs(asset.GetRuntimeInstance(), type);
			}
			return Operator.TypeIs(obj, type);
		}

		/// <summary>
		/// Is the obj is instance of type with unique ID of 'uniqueID'
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="uniqueID"></param>
		/// <returns></returns>
		public static bool IsTypeOf(this object obj, string uniqueID) {
			if(obj is IRuntimeComponent comp) {
				return comp.uniqueIdentifier == uniqueID;
			}
			return false;
		}

		/// <summary>
		/// Convert the instance of obj into T
		/// </summary>
		/// <param name="obj"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T ConvertTo<T>(this object obj) {
			if(object.ReferenceEquals(obj, null)) {
				return default;
			}
			try {
				return Operator.Convert<T>(obj);
			} catch (InvalidCastException ex) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to : {typeof(T)}", ex);
			}
		}

		/// <summary>
		/// Convert the instance of obj into T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static T ToRuntimeInstance<T>(this object obj) {
			if(object.ReferenceEquals(obj, null)) {
				return default;
			}
			if(obj is IRuntimeClassContainer container) {
				return (T)container.RuntimeClass;
			}
			try {
				return (T)obj;
			} catch(InvalidCastException) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to : {typeof(T)}");
			}
		}

		/// <summary>
		/// Convert the instance of obj into T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static T ToRuntimeInstance<T>(this UnityEngine.Component obj) {
			if(obj == null) {
				return default;
			}
			if(obj is IRuntimeClassContainer container) {
				return (T)container.RuntimeClass;
			}
			try {
				if(obj is T) {
					return (T)(object)obj;
				} else if(typeof(T) == typeof(UnityEngine.GameObject)) {
					return (T)(object)obj.gameObject;
				} else if(typeof(T).IsSubclassOf(typeof(UnityEngine.Component))) {
					return obj.GetComponent<T>();
				}
				return (T)(object)obj;
			}
			catch(InvalidCastException) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to : {typeof(T)}");
			}
		}

		public static RuntimeComponent ToRuntimeInstance(this UnityEngine.Component obj, string uniqueID) {
			if(obj == null) {
				return default;
			}
			try {
				if(obj is IRuntimeComponent container && container.uniqueIdentifier == uniqueID) {
					return container as RuntimeComponent;
				}
				return obj.GetGeneratedComponent(uniqueID);
			}
			catch(InvalidCastException) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to Runtime Instance with UID : {uniqueID}");
			}
		}

		public static BaseRuntimeAsset ToRuntimeInstance(this BaseRuntimeAsset obj, string uniqueID) {
			if(obj == null) {
				return default;
			}
			if(obj is IRuntimeAsset container && container.uniqueIdentifier == uniqueID) {
				return container as BaseRuntimeAsset;
			}
			return obj as BaseRuntimeAsset;
		}

		public static IRuntimeClass ToRuntimeInstance(this object obj, string uniqueID) {
			if(obj == null) {
				return default;
			}
			try {
				if(obj is IRuntimeComponent container && container.uniqueIdentifier == uniqueID) {
					return obj as IRuntimeClass;
				} else if(obj is IRuntimeAsset runtimeAsset && runtimeAsset.uniqueIdentifier == uniqueID) {
					return obj as IRuntimeClass;
				} else if(obj is UnityEngine.Component comp) {
					return comp.GetGeneratedComponent(uniqueID);
				} else if(obj is UnityEngine.GameObject go) {
					return go.GetGeneratedComponent(uniqueID);
				}
				return null;
			}
			catch(InvalidCastException) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to Runtime Instance with UID : {uniqueID}");
			}
		}

		/// <summary>
		/// Convert the instance of obj into T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static T ToRuntimeInstance<T>(this UnityEngine.GameObject obj) {
			if(obj == null) {
				return default;
			}
			try {
				if(typeof(T) == typeof(UnityEngine.GameObject)) {
					return (T)(object)obj;
				}
				return obj.GetComponent<T>();
			}
			catch(InvalidCastException) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to : {typeof(T)}");
			}
		}

		public static RuntimeComponent ToRuntimeInstance(this UnityEngine.GameObject obj, string uniqueID) {
			if(obj == null) {
				return default;
			}
			try {
				return obj.GetGeneratedComponent(uniqueID);
			}
			catch(InvalidCastException) {
				throw new InvalidCastException($"Cannot convert: {obj.GetType()} to Runtime Instance with UID : {uniqueID}");
			}
		}

		/// <summary>
		/// Is the 'type' is implement an interface 'interfaceType'
		/// </summary>
		/// <param name="type"></param>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public static bool HasImplementInterface(this Type type, Type interfaceType) {
			if(type == null || interfaceType == null) return false;
			while (type != null) {
				bool generic = interfaceType.IsGenericTypeDefinition;
				Type[] interfaces = type.GetInterfaces();
				if (interfaces != null) {
					for (int i = 0; i < interfaces.Length; i++) {
						if(generic) {
							var iface = interfaces[i];
							if(iface.IsConstructedGenericType) {
								iface = iface.GetGenericTypeDefinition();
							}
							if (iface == interfaceType || (iface != null && HasImplementInterface(iface, interfaceType))) {
								return true;
							}
						} else {
							if (interfaces[i] == interfaceType || (interfaces[i] != null && HasImplementInterface(interfaces[i], interfaceType))) {
								return true;
							}
						}
					}
				}
				type = type.BaseType;
			}
			return false;
		}

		public static bool IsSubclassOfRawGeneric(this Type type, Type generic) {
			while(type != null && type != typeof(object)) {
				var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
				if(generic == cur) {
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Determines whether an instance of the current Type can be assigned to an instance of the specified Type.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool IsCastableTo(this Type from, Type to) {
			if(from == null || to == null) return false;//Force false on from or to type is null.
			if(from.IsByRef) {
				from = from.GetElementType();
			}
			if(to.IsByRef) {
				to = to.GetElementType();
			}
			Dictionary<Type, bool> map = null;
			lock(_lockObject) {
				if(castableMap.TryGetValue(from, out map)) {
					bool val;
					if(map.TryGetValue(to, out val)) {
						return val;
					}
				}
			}
			if(from is RuntimeType || to is RuntimeType) {
				if(from is RuntimeType) {
					if(from == to) return true;
					if(to.IsInterface) {
						return from.HasImplementInterface(to);
					} else if(from.IsInterface) {
						return to.HasImplementInterface(from) || to == typeof(object);
					} else if(to is RuntimeType) {
						return to.IsAssignableFrom(from);
					}
					return from.BaseType.IsCastableTo(to);
				}
				if(to.IsInterface) {
					return from.HasImplementInterface(to);
				} else if(to is RuntimeType) {
					return to.IsAssignableFrom(from);
				}
				return from.IsCastableTo(to.BaseType);
			}
			if(to.IsAssignableFrom(from)) {
				lock(_lockObject) {
					if(map == null) {
						map = new Dictionary<Type, bool>();
					}
					map[to] = true;
					castableMap[from] = map;
				}
				return true;
			}
			if(from.IsPrimitive && to.IsPrimitive) {
				IEnumerable<Type> lowerTypes = Enumerable.Empty<Type>();
				foreach(Type[] types in typeHierarchy) {
					if(types.Any(t => t == to)) {
						bool r = lowerTypes.Any(t => t == from);
						lock(_lockObject) {
							if(map == null) {
								map = new Dictionary<Type, bool>();
							}
							map[to] = r;
							castableMap[from] = map;
						}
						return r;
					}
					lowerTypes = lowerTypes.Concat(types);
				}
				lock(_lockObject) {
					if(map == null) {
						map = new Dictionary<Type, bool>();
					}
					map[to] = false;
					castableMap[from] = map;
				}
				return false; // IntPtr, UIntPtr, Enum, Boolean
			}
			if(from.IsSubclassOf(typeof(Delegate)) && to.IsSubclassOf(typeof(Delegate))) {
				var a = from.GetMethod("Invoke");
				var b = to.GetMethod("Invoke");
				if(a.ReturnType != b.ReturnType) {
					lock(_lockObject) {
						if(map == null) {
							map = new Dictionary<Type, bool>();
						}
						map[to] = false;
						castableMap[from] = map;
					}
					return false;
				}
				var aParams = a.GetParameters();
				var bParams = b.GetParameters();
				if(aParams.Length != bParams.Length) {
					lock(_lockObject) {
						if(map == null) {
							map = new Dictionary<Type, bool>();
						}
						map[to] = false;
						castableMap[from] = map;
					}
					return false;
				}
				for(int i = 0; i < aParams.Length; i++) {
					if(aParams[i].ParameterType != bParams[i].ParameterType) {
						lock(_lockObject) {
							if(map == null) {
								map = new Dictionary<Type, bool>();
							}
							map[to] = false;
							castableMap[from] = map;
						}
						return false;
					}
				}
				lock(_lockObject) {
					if(map == null) {
						map = new Dictionary<Type, bool>();
					}
					map[to] = true;
					castableMap[from] = map;
				}
				return true;
			}
			bool result = from.FindImplicitOperator(to).Any();
			lock(_lockObject) {
				if(map == null) {
					map = new Dictionary<Type, bool>();
				}
				map[to] = result;
				castableMap[from] = map;
			}
			return result;
		}

		/// <summary>
		/// Get the implicit operators
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> FindImplicitOperator(this Type from, Type to) {
			var methods = from.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(m => 
				m.ReturnType == to && 
				(m.Name == "op_Implicit")
			);
			if(methods.Any()) {
				return methods;
			}
			return to.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(m =>
				m.ReturnType == to &&
				(m.Name == "op_Implicit") &&
				ReflectionUtils.IsValidMethodParameter(m, from)
			);
		}

		/// <summary>
		/// Determines whether an instance of the current Type can be assigned to an instance of the specified Type.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static bool IsCastableTo(this Type from, Type to, bool Explicit) {
			if(!Explicit)
				return from.IsCastableTo(to);
			if(from == null || to == null) return false;//Force false on from or to type is null.
			Dictionary<Type, bool> map = null;
			lock(_lockObject) {
				if(castableMap2.TryGetValue(from, out map)) {
					bool val;
					if(map.TryGetValue(to, out val)) {
						return val;
					}
				}
			}
			if(from.IsEnum && to.IsPrimitive) {
				return to != typeof(bool);
			}
			if(to.IsAssignableFrom(from)) {
				lock(_lockObject) {
					if(map == null) {
						map = new Dictionary<Type, bool>();
					}
					map[to] = true;
					castableMap2[from] = map;
				}
				return true;
			}
			if((from.IsPrimitive) && (to.IsPrimitive)) {
				//bool flag1 = false;
				//bool flag2 = false;
				//foreach(Type[] types in typeHierarchy) {
				//	if(types.Contains(from)) {
				//		flag1 = true;
				//	}
				//	if(types.Contains(to)) {
				//		flag2 = true;
				//	}
				//}
				lock(_lockObject) {
					if(map == null) {
						map = new Dictionary<Type, bool>();
					}
					map[to] = from != typeof(bool) && to != typeof(bool);
					castableMap2[from] = map;
				}
				return false; // IntPtr, UIntPtr, Enum, Boolean
			}
			if(from.IsSubclassOf(typeof(Delegate)) && to.IsSubclassOf(typeof(Delegate))) {
				var a = from.GetMethod("Invoke");
				var b = to.GetMethod("Invoke");
				if(a.ReturnType != b.ReturnType) {
					lock(_lockObject) {
						if(map == null) {
							map = new Dictionary<Type, bool>();
						}
						map[to] = false;
						castableMap2[from] = map;
					}
					return false;
				}
				var aParams = a.GetParameters();
				var bParams = b.GetParameters();
				if(aParams.Length != bParams.Length) {
					lock(_lockObject) {
						if(map == null) {
							map = new Dictionary<Type, bool>();
						}
						map[to] = false;
						castableMap2[from] = map;
					}
					return false;
				}
				for(int i = 0; i < aParams.Length; i++) {
					if(aParams[i] != bParams[i]) {
						lock(_lockObject) {
							if(map == null) {
								map = new Dictionary<Type, bool>();
							}
							map[to] = false;
							castableMap2[from] = map;
						}
						return false;
					}
				}
				lock(_lockObject) {
					if(map == null) {
						map = new Dictionary<Type, bool>();
					}
					map[to] = true;
					castableMap2[from] = map;
				}
				return true;
			}
			var methods = from.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(m => m.ReturnType == to && (m.Name == "op_Implicit" || Explicit && m.Name == "op_Explicit"));
			bool result = methods.Any();
			lock(_lockObject) {
				if(map == null) {
					map = new Dictionary<Type, bool>();
				}
				map[to] = result;
				castableMap2[from] = map;
			}
			return result;
		}

		/// <summary>
		/// Get the element type of a type if available.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type ElementType(this Type type) {
			if(type == null)
				return null;
			if(type.IsArray) {
				return type.GetElementType();
			} else if(type.IsGenericType && type.GetGenericArguments().Length == 1) {
				return type.GetGenericArguments()[0];
			} else if(type.IsGenericType && type.GetGenericArguments().Length == 2 && type.IsCastableTo(typeof(IDictionary))) {
				return typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments()[0], type.GetGenericArguments()[1]);
			} else {
				Type inter = type.GetInterface(typeof(IEnumerable<>).FullName);
				if(inter != null && inter.IsGenericType && inter.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>))) {
					return inter.GetGenericArguments()[0];
				} else {
					if(type == typeof(UnityEngine.Transform)) {
						return type;
					} else if(type.GetInterface(typeof(IEnumerable).FullName) != null) {
						return typeof(object);
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Determines if an enum has the given flag defined bitwise.
		/// Fallback equivalent to .NET's Enum.HasFlag().
		/// </summary>
		public static bool HasFlags<T>(this T value, T flag) where T : Enum {
			//return (Convert.ToInt64(value) & Convert.ToInt64(flag)) != 0;
			Dictionary<Enum, bool> map = null;
			lock(_lockObject2) {
				if(enumsMap.TryGetValue(value, out map)) {
					bool val;
					if(map.TryGetValue(flag, out val)) {
						return val;
					}
				}
			}
			long lValue = System.Convert.ToInt64(value);
			long lFlag = System.Convert.ToInt64(flag);
			bool result = (lValue & lFlag) != 0;
			lock(_lockObject2) {
				if(map == null) {
					map = new Dictionary<Enum, bool>();
				}
				map[flag] = result;
				enumsMap[value] = map;
			}
			return result;
		}

		/// <summary>
		/// Indicate is targeting node.
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static bool IsTargetingNode(this MemberData.TargetType targetType) {
			switch(targetType) {
				case MemberData.TargetType.FlowNode:
				case MemberData.TargetType.ValueNode:
					return true;
			}
			return false;
		}

		/// <summary>
		/// Indicate is targeting pin or node.
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static bool IsTargetingPortOrNode(this MemberData.TargetType targetType) {
				switch(targetType) {
					case MemberData.TargetType.FlowNode:
					case MemberData.TargetType.ValueNode:

					case MemberData.TargetType.NodeField:
					case MemberData.TargetType.NodeFieldElement:
					case MemberData.TargetType.FlowInput:
					case MemberData.TargetType.NodeOutputValue:
					case MemberData.TargetType.FlowInputExtended:
						return true;
				}
				return false;
		}

		public static bool IsTargetingFlowPort(this MemberData.TargetType targetType) {
			switch(targetType) {
				case MemberData.TargetType.FlowNode:
				case MemberData.TargetType.FlowInput:
				case MemberData.TargetType.FlowInputExtended:
					return true;
			}
			return false;
		}

		public static bool IsTargetingValuePort(this MemberData.TargetType targetType) {
			switch(targetType) {
				case MemberData.TargetType.ValueNode:
				case MemberData.TargetType.NodeField:
				case MemberData.TargetType.NodeFieldElement:
				case MemberData.TargetType.NodeOutputValue:
					return true;
			}
			return false;
		}

		/// <summary>
		/// Indicate is targeting uNode Member whether it's graph, pin, or nodes.
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static bool IsTargetingUNode(this MemberData.TargetType targetType) {
			switch (targetType) {
				case MemberData.TargetType.FlowNode:
				case MemberData.TargetType.ValueNode:

				case MemberData.TargetType.NodeField:
				case MemberData.TargetType.NodeFieldElement:
				case MemberData.TargetType.FlowInput:
				case MemberData.TargetType.NodeOutputValue:
				case MemberData.TargetType.FlowInputExtended:

				case MemberData.TargetType.uNodeConstructor:
				case MemberData.TargetType.uNodeFunction:
				case MemberData.TargetType.uNodeGenericParameter:
				case MemberData.TargetType.uNodeGroupVariable:
				case MemberData.TargetType.uNodeLocalVariable:
				case MemberData.TargetType.uNodeIndexer:
				case MemberData.TargetType.uNodeParameter:
				case MemberData.TargetType.uNodeProperty:
				case MemberData.TargetType.uNodeVariable:
				case MemberData.TargetType.uNodeType:
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// True if targetType is Type or uNodeType
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static bool IsTargetingType(this MemberData.TargetType targetType) {
			switch (targetType) {
				case MemberData.TargetType.Type:
				case MemberData.TargetType.uNodeType:
					return true;
			}
			return false;
		}

		public static bool IsTargetingReflection(this MemberData.TargetType targetType) {
			switch(targetType) {
				case MemberData.TargetType.Constructor:
				case MemberData.TargetType.Event:
				case MemberData.TargetType.Field:
				case MemberData.TargetType.Method:
				case MemberData.TargetType.Property:
					return true;
			}
			return false;
		}

		/// <summary>
		/// True if targetType is Values, SelfTarget, or Null
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>	
		public static bool IsTargetingValue(this MemberData.TargetType targetType) {
			switch (targetType) {
				case MemberData.TargetType.Values:
				case MemberData.TargetType.SelfTarget:
				case MemberData.TargetType.Null:
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// True if targetType is uNodeVariable or uNodeLocalVariable
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static bool IsTargetingVariable(this MemberData.TargetType targetType) {
			switch (targetType) {
				case MemberData.TargetType.uNodeVariable:
				case MemberData.TargetType.uNodeGroupVariable:
				case MemberData.TargetType.uNodeLocalVariable:
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// The target is targeting graph that's return a value except targeting pin or nodes.
		/// </summary>
		/// <param name="targetType"></param>
		/// <returns></returns>
		public static bool IsTargetingGraphValue(this MemberData.TargetType targetType) {
			switch (targetType) {
				case MemberData.TargetType.uNodeConstructor:
				case MemberData.TargetType.uNodeFunction:
				case MemberData.TargetType.uNodeGenericParameter:
				case MemberData.TargetType.uNodeGroupVariable:
				case MemberData.TargetType.uNodeLocalVariable:
				case MemberData.TargetType.uNodeIndexer:
				case MemberData.TargetType.uNodeParameter:
				case MemberData.TargetType.uNodeProperty:
				case MemberData.TargetType.uNodeVariable:
				case MemberData.TargetType.uNodeType:
					return true;
			}
			return false;
		}

		/// <summary>
		/// Add tab after new line.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="tabCount"></param>
		/// <param name="removeEmptyLines">if true, empty line will be removed</param>
		/// <returns></returns>
		public static string AddTabAfterNewLine(this string str, bool removeEmptyLines = true) {
			return AddTabAfterNewLine(str, 1, removeEmptyLines);
		}

		/// <summary>
		/// Add tab after new line.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="tabCount"></param>
		/// <param name="removeEmptyLines">if true, empty line will be removed</param>
		/// <returns></returns>
		public static string AddTabAfterNewLine(this string str, int tabCount, bool removeEmptyLines = true) {
			if(!string.IsNullOrEmpty(str)) {
				List<string> s = str.Split('\n').ToList();
				for(int x = 0; x < s.Count; x++) {
					if(removeEmptyLines && string.IsNullOrEmpty(s[x]) && x != 0) {
						s.RemoveAt(x);
						x--;
						continue;
					}
					if(string.IsNullOrEmpty(s[x]))
						continue;
					s[x] = Tab(tabCount) + s[x];
				}
				str = string.Join("\n", s.ToArray());
			}
			return str;
		}

		/// <summary>
		/// Add Tab
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		private static string Tab(int count) {
			string data = null;
			for(int i = 0; i < count; i++) {
				data += "\t";
			}
			return data;
		}

		/// <summary>
		/// Add string value if value is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Add(this string str, string value) {
			if(!string.IsNullOrEmpty(str)) {
				str += value;
			}
			return str;
		}

		/// <summary>
		/// Add string value if value and targetValue is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Add(this string str, string value, string targetValue, bool inverted = false) {
			if(inverted) {
				if(!string.IsNullOrEmpty(str) && string.IsNullOrEmpty(targetValue)) {
					str += value;
				}
			} else {
				if(!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(targetValue)) {
					str += value;
				}
			}
			return str;
		}

		/// <summary>
		/// Add string value if value is not null and addIfCondition is true
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <param name="addIfCondition"></param>
		/// <returns></returns>
		public static string Add(this string str, string value, bool addIfCondition) {
			if(!string.IsNullOrEmpty(str) && addIfCondition) {
				str += value;
			}
			return str;
		}

		/// <summary>
		/// Add string value in specific index if value is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Add(this string str, int index, string value) {
			if(!string.IsNullOrEmpty(str)) {
				if(index >= str.Length) {
					return str + value;
				}
				str = str.Insert(index, value);
			}
			return str;
		}

		/// <summary>
		/// Add string value on first if value is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string AddFirst(this string str, string value) {
			if(!string.IsNullOrEmpty(str)) {
				str = value + str;
			}
			return str;
		}

		/// <summary>
		/// Add a new line in first.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string AddLineInFirst(this string str) {
			return str.AddFirst("\n");
		}

		/// <summary>
		/// Add a new line in end
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string AddLineInEnd(this string str) {
			return str.Add("\n");
		}

		/// <summary>
		/// Add a new statement in the end.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public static string AddStatement(this string str, string contents) {
			if(string.IsNullOrEmpty(contents)) {
				return str;
			}
			return str.Add("\n" + contents);
		}

		/// <summary>
		/// Add string value on first if value and targetValue is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string AddFirst(this string str, string value, string targetValue, bool inverted = false) {
			if(inverted) {
				if(!string.IsNullOrEmpty(str) && string.IsNullOrEmpty(targetValue)) {
					str = value + str;
				}
			} else {
				if(!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(targetValue)) {
					str = value + str;
				}
			}
			return str;
		}

		/// <summary>
		/// Add string value on first if value is not null and addIfCondition is true
		/// </summary>
		/// <param name="str"></param>
		/// <param name="value"></param>
		/// <param name="addIfCondition"></param>
		/// <returns></returns>
		public static string AddFirst(this string str, string value, bool addIfCondition) {
			if(!string.IsNullOrEmpty(str) && addIfCondition) {
				str = value + str;
			}
			return str;
		}

		/// <summary>
		/// Remove last string if value is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static string RemoveLast(this string str, int count = 1) {
			if(!string.IsNullOrEmpty(str)) {
				str = str.Remove(str.Length - count, count);
			}
			return str;
		}

		/// <summary>
		/// Add semicolon if value is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string AddSemicolon(this string str) {
			if(!string.IsNullOrEmpty(str)) {
				str = str + ";";
			}
			return str;
		}

		/// <summary>
		/// Remove semicolon if value is not null and empty
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string RemoveSemicolon(this string str) {
			if(!string.IsNullOrEmpty(str)) {
				str = str.Replace(";", "");
			}
			return str;
		}

		/// <summary>
		/// Remove line and tab before the first character
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string RemoveLineAndTabOnFirst(this string str) {
			if(!string.IsNullOrEmpty(str)) {
				string value = "";
				bool flag = false;
				for(int i = 0; i < str.Length; i++) {
					if(str[i] != '\n' && str[i] != '\t') {
						flag = true;
					}
					if(flag) {
						value += str[i];
					}
				}
				return value;
			}
			return str;
		}

		public static string PrettyName(this string typeName, bool fullName = false) {
			Type t = TypeSerializer.Deserialize(typeName, false);
			if(t != null) {
				return t.PrettyName(fullName);
			}
			return typeName;
		}

		public static string PrettyName(this Type type, bool fullName = false, bool? isOut = false) {
			return CSharpTypeName(type, fullName, isOut);
		}

		private static string CSharpTypeName(Type type, bool fullName = false, bool? isOut = false) {
			if(type == null)
				return "null";
			if(type is uNode.RuntimeType) {
				return type.Name;
			}
			if(type.IsByRef) {
				return string.Format("{0} {1}", (isOut == null ? "&" : (isOut.Value ? "out" : "ref")), CSharpTypeName(type.GetElementType(), fullName, isOut));
			}
			if(type.IsGenericType) {
				if(type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
					return string.Format("{0}?", CSharpTypeName(Nullable.GetUnderlyingType(type), fullName, isOut));
				} else if(!type.ContainsGenericParameters) {
					return string.Format("{0}<{1}>", (fullName ? type.FullName : type.Name).Split('`')[0],
						string.Join(", ", type.GetGenericArguments().Select(a => CSharpTypeName(a, fullName, isOut)).ToArray()));
				}
			}
			if(type.IsArray) {
				return string.Format("{0}[]", CSharpTypeName(type.GetElementType(), fullName, isOut));
			}

			return shorthandMap.ContainsKey(type) ? shorthandMap[type] : fullName ? type.FullName : type.Name;
		}

		public static bool CanGet(this PortAccessibility accessibility) {
			return accessibility == PortAccessibility.GetSet || accessibility == PortAccessibility.OnlyGet;
		}

		public static bool CanSet(this PortAccessibility accessibility) {
			return accessibility == PortAccessibility.GetSet || accessibility == PortAccessibility.OnlySet;
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collections) {
			return new HashSet<T>(collections);
		}
	}
}