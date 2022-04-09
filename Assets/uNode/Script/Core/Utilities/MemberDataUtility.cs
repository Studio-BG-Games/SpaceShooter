using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames {
	public static class MemberDataUtility {
		public const string RUNTIME_ID = "[runtime]";

		/// <summary>
		/// True if the member can be get dirrectly because the value is constraint and not dynamic
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static bool CanSafeGetValue(this MemberData member) {
			if(member == null) return false;
			switch(member.targetType) {
				case MemberData.TargetType.Null:
				case MemberData.TargetType.SelfTarget:
				case MemberData.TargetType.Type:
				case MemberData.TargetType.Values:
					return true;
				case MemberData.TargetType.ValueNode:
					var node = member.GetTargetNode() as MultipurposeNode;
					if(node != null) {
						return node.target.CanSafeGetValue();
					}
					break;
			}
			return false;
		}

		/// <summary>
		/// True if the member can be get dirrectly because the value is constraint and not dynamic
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static bool CanSafeGetValue(this MultipurposeMember member) {
			if(member == null || member.target == null) return false;
			if(member.target.CanSafeGetValue()) {
				if(member.initializer != null) {
					return CanSafeGetValue(member.initializer);
				}
				return true;
			}
			return false;
		}

		public static bool CanSafeGetValue(this ValueData valueData) {
			if(valueData == null) return false;
			return valueData.Value == null || CanSafeGetValue(valueData.Value);
		}

		public static bool CanSafeGetValue(this BaseValueData baseValueData) {
			if(baseValueData == null) return false;
			if(baseValueData is ConstructorValueData) {
				var ctor = baseValueData as ConstructorValueData;
				if(ctor.parameters != null) {
					for (int i = 0; i < ctor.parameters.Length;i++) {
						var val = ctor.parameters[i]?.value;
						if(val is MemberData) {
							if(!CanSafeGetValue(val as MemberData)) return false;
						} else if(val is BaseValueData) {
							if(!CanSafeGetValue(val as BaseValueData)) return false;
						}
					}
				}
				if(ctor.initializer != null) {
					for (int i = 0; i < ctor.initializer.Length;i++) {
						var val = ctor.initializer[i]?.value;
						if(val is MemberData) {
							if(!CanSafeGetValue(val as MemberData)) return false;
						} else if(val is BaseValueData) {
							if(!CanSafeGetValue(val as BaseValueData)) return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		public static MemberData.ItemData GetItemDataFromMemberInfo(MemberInfo member) {
			MemberData.ItemData iData = null;
			MethodInfo methodInfo = member as MethodInfo;
			if(member is EventInfo) {
				methodInfo = ((EventInfo)(member)).EventHandlerType.GetMethod("Invoke");
			}
			ConstructorInfo ctor = member as ConstructorInfo;
			if(methodInfo != null || ctor != null) {
				Type[] genericMethodArgs = methodInfo != null ? methodInfo.GetGenericArguments() : null;
				if(genericMethodArgs != null && genericMethodArgs.Length > 0) {
					TypeData[] param = new TypeData[genericMethodArgs.Length];
					for(int i = 0; i < genericMethodArgs.Length; i++) {
						param[i] = GetTypeData(genericMethodArgs[i], null);
					}
					iData = new MemberData.ItemData() { genericArguments = param };
				}
				ParameterInfo[] paramsInfo = methodInfo != null ? methodInfo.GetParameters() : ctor.GetParameters();
				if(paramsInfo.Length > 0) {
					TypeData[] paramData = new TypeData[paramsInfo.Length];
					for(int x = 0; x < paramsInfo.Length; x++) {
						TypeData gData = GetTypeData(paramsInfo[x], genericMethodArgs != null ? genericMethodArgs.Select(it => it.Name).ToArray() : null);
						paramData[x] = gData;
					}
					if(iData == null) {
						iData = new MemberData.ItemData();
					}
					iData.parameters = paramData;
				}
			}
			return iData;
		}

		public static bool HasGenericArguments(Type type) {
			return type != null && (type.GetGenericArguments().Length > 0 ||
				type.HasElementType && HasGenericArguments(type.GetElementType()));
		}

		public static TypeData GetTypeData(ParameterData parameter, params string[] genericName) {
			return GetTypeData(parameter.Type, genericName);
		}

		public static TypeData GetTypeData(ParameterInfo parameter, params string[] genericName) {
			string name = null;
			if(parameter.ParameterType.IsGenericParameter) {
				name = "#" + GetGenericIndex(parameter.ParameterType.Name, genericName);
			} else {
				return GetTypeData(parameter.ParameterType, genericName);
			}
			return new TypeData(name);
		}

		public static TypeData GetTypeData(Type type, params string[] genericName) {
			return GetTypeData(type, null, genericName);
		}

		public static TypeData GetTypeData(Type type, List<UnityEngine.Object> references, string[] genericName) {
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			TypeData data = new TypeData();
			int array = 0;
			while(type.IsArray) {
				type = type.GetElementType();
				array++;
			}
			string name;
			if(type.IsGenericParameter) {
				if(genericName != null && genericName.Length > 0) {
					name = "#" + GetGenericIndex(type.Name, genericName);
				} else {
					name = "$" + type.Name;
				}
			} else if(type is RuntimeType) {
				if(type is FakeGraphType) {
					type = (type as FakeGraphType).target;
				} else if(type is FakeGraphInterface) {
					type = (type as FakeGraphInterface).target;
				}
				if(type is RuntimeGraphType runtimeType) {
					if(references == null) {
						name = RUNTIME_ID;
						data.references = new List<UnityEngine.Object>() { runtimeType.target };
					} else {
						name = "@" + references.Count;
						references.Add(runtimeType.target);
					}
				} else if(type is RuntimeGraphInterface graphInterface) {
					if(references == null) {
						name = RUNTIME_ID;
						data.references = new List<UnityEngine.Object>() { graphInterface.target };
					} else {
						name = "@" + references.Count;
						references.Add(graphInterface.target);
					}
				} else if(type is ArrayFakeType fakeArray) {
					data.name = "?";
					var elementType = fakeArray.GetElementType();
					data.parameters = new TypeData[] { GetTypeData(elementType, references, genericName) };
					return data;
				} else if(type is GenericFakeType fakeGeneric) {
					Type[] genericArgs = fakeGeneric.GetGenericArguments();
					if(genericArgs.Length > 0) {
						data.parameters = new TypeData[genericArgs.Length];
						for(int i = 0; i < genericArgs.Length; i++) {
							data.parameters[i] = GetTypeData(genericArgs[i], references, genericName);
						}
					} else if(data.parameters.Length == 0) {
						data.parameters = null;
					}
					if(!type.IsGenericTypeDefinition) {
						type = type.GetGenericTypeDefinition();
					}
					name = "!" + type.FullName;
				} else {
					throw new Exception("Unsupported RuntimeType");
				}
			} else if(type.IsGenericType) {
				Type[] genericArgs = type.GetGenericArguments();
				if(genericArgs.Length > 0) {
					data.parameters = new TypeData[genericArgs.Length];
					for(int i = 0; i < genericArgs.Length; i++) {
						data.parameters[i] = GetTypeData(genericArgs[i], references, genericName);
					}
				} else if(data.parameters.Length == 0) {
					data.parameters = null;
				}
				if(!type.IsGenericTypeDefinition) {
					type = type.GetGenericTypeDefinition();
				}
				name = type.FullName;
			}  else {
				name = type.FullName;
			}
			while(array > 0) {
				name += "[]";
				array--;
			}
			data.name = name;
			return data;
		}

		public static TypeData GetTypeData(MemberData member, List<UnityEngine.Object> references = null) {
			if(member.targetType == MemberData.TargetType.uNodeGenericParameter) {
				return new TypeData("$" + member.name);
			}
			Type type = member.Get<Type>();
			TypeData data = new TypeData();
			int array = 0;
			while(type.IsArray) {
				type = type.GetElementType();
				array++;
			}
			string name;
			if(type.IsGenericParameter) {
				name = "$" + type.Name;
			} else if(type is RuntimeType) {
				if(type is FakeGraphType) {
					type = (type as FakeGraphType).target;
				} else if(type is FakeGraphInterface) {
					type = (type as FakeGraphInterface).target;
				}
				if(type is RuntimeGraphType runtimeType) {
					if(references == null) {
						name = RUNTIME_ID;
						data.references = new List<UnityEngine.Object>() { runtimeType.target };
					} else {
						name = "@" + references.Count;
						references.Add(runtimeType.target);
					}
				} else if(type is RuntimeGraphInterface graphInterface) {
					if(references == null) {
						name = RUNTIME_ID;
						data.references = new List<UnityEngine.Object>() { graphInterface.target };
					} else {
						name = "@" + references.Count;
						references.Add(graphInterface.target);
					}
				} else if(type is ArrayFakeType fakeArray) {
					data.name = "?";
					var elementType = fakeArray.GetElementType();
					data.parameters = new TypeData[] { GetTypeData(elementType, references, null) };
					return data;
				} else if(type is GenericFakeType fakeGeneric) {
					Type[] genericArgs = fakeGeneric.GetGenericArguments();
					if(genericArgs.Length > 0) {
						data.parameters = new TypeData[genericArgs.Length];
						for(int i = 0; i < genericArgs.Length; i++) {
							data.parameters[i] = GetTypeData(genericArgs[i], references, null);
						}
					} else if(data.parameters.Length == 0) {
						data.parameters = null;
					}
					if(!type.IsGenericTypeDefinition) {
						type = type.GetGenericTypeDefinition();
					}
					name = "!" + type.FullName;
				} else {
					throw new Exception("Unsupported RuntimeType");
				}
			} else if(type.IsGenericType) {
				Type[] genericArgs = type.GetGenericArguments();
				if(genericArgs.Length > 0) {
					data.parameters = new TypeData[genericArgs.Length];
					for(int i = 0; i < genericArgs.Length; i++) {
						data.parameters[i] = GetTypeData(genericArgs[i], null);
					}
				} else if(data.parameters.Length == 0) {
					data.parameters = null;
				}
				if(!type.IsGenericTypeDefinition) {
					type = type.GetGenericTypeDefinition();
				}
				name = type.FullName;
			} else {
				name = type.FullName;
			}
			while(array > 0) {
				name += "[]";
				array--;
			}
			data.name = name;
			return data;
		}

		public static int GetGenericIndex(string name, params string[] genericName) {
			if(genericName != null) {
				for(int i = 0; i < genericName.Length; i++) {
					if(genericName[i] == name) {
						return i;
					}
				}
			}
			return 0;
		}

		public static void ReflectGenericData(TypeData data, Action<TypeData> action) {
			action(data);
			if(data.parameters != null) {
				foreach(TypeData d in data.parameters) {
					ReflectGenericData(d, action);
				}
			}
		}
		
		public static TypeData[] ParameterDataToTypeDatas(ParameterData[] parameters, GenericParameterData[] genericParameters = null) {
			if(genericParameters == null) {
				genericParameters = new GenericParameterData[0];
			}
			TypeData[] paramData = new TypeData[parameters.Length];
			for(int x = 0; x < parameters.Length; x++) {
				switch(parameters[x].type.targetType) {
					case MemberData.TargetType.uNodeGenericParameter:
						paramData[x] = new TypeData(
							"#" + GetGenericIndex(parameters[x].type.name,
							genericParameters.Select(it => it.name).ToArray())
						);
						//if(!parameters[x].type.hasGenericType) {
						//} else {
						//	TypeData d = SerializerUtility.Deserialize<MemberData.ItemData>(parameters[x].type.SerializedItems[0]).genericArguments[0];
						//	ReflectGenericData(d, delegate (TypeData TD) {
						//		if(TD.name[0] == '$') {
						//			for(int y = 0; y < genericParameters.Length; y++) {
						//				if(genericParameters[y].name == TD.name.Remove(0, 1)) {
						//					TD.name = "#" + y;
						//					break;
						//				}
						//			}
						//		}
						//	});
						//	paramData[x] = d;
						//}
						break;
					case MemberData.TargetType.Type:
						paramData[x] = new TypeData() { name = parameters[x].type.Get<Type>().FullName };
						break;
					case MemberData.TargetType.uNodeType:
						var runtimeType = parameters[x].type.startType;
						if(runtimeType is FakeGraphType) {
							runtimeType = (runtimeType as FakeGraphType).target;
						} else if(runtimeType is FakeGraphInterface) {
							runtimeType = (runtimeType as FakeGraphInterface).target;
						}
						if(runtimeType is RuntimeGraphType graphType) {
							paramData[x] = new TypeData() {
								name = RUNTIME_ID,
								references = new List<UnityEngine.Object>() { graphType.target }
							};
						} else if(runtimeType is RuntimeGraphInterface graphInterface) {
							paramData[x] = new TypeData() {
								name = RUNTIME_ID,
								references = new List<UnityEngine.Object>() { graphInterface.target }
							};
						} else {
							throw new Exception("Unsupported RuntimeType");
						}
						break;
					default:
						throw new InvalidOperationException();
				}
			}
			return paramData;
		}
        
		/// <summary>
		/// The all items name from ItemData.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <param name="GenericType"></param>
		/// <param name="ParameterType"></param>
		public static void GetItemName(MemberData.ItemData item, IList<UnityEngine.Object> targets, out string[] GenericType, out string[] ParameterType) {
			string[] GType = new string[0];
			string[] PType = new string[0];
			if(item != null) {
				if(item.genericArguments != null) {
					GType = new string[item.genericArguments.Length];
					for(int i = 0; i < GType.Length; i++) {
						GType[i] = GetGenericName(item.genericArguments[i], targets);
					}
				}
				if(item.parameters != null) {
					PType = new string[item.parameters.Length];
					for(int i = 0; i < PType.Length; i++) {
						PType[i] = GetParameterName(item.parameters[i], GType, targets);
					}
				}
			}
			GenericType = GType;
			ParameterType = PType;
		}

		/// <summary>
		/// Get generic name from TypeData.
		/// </summary>
		/// <param name="genericData"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static string GetGenericName(TypeData genericData, IList<UnityEngine.Object> targets) {
			int array = 0;
			string n = genericData.name;
			while(n.EndsWith("[]")) {
				n = n.Remove(n.Length - 2);
				array++;
			}
			System.Type t = null;
			if(n[0] == '$') {
				n = n.Remove(0, 1);
			} else if(n[0] == '@') {
				n = n.Remove(0, 1);
				if(targets != null && targets.Count > int.Parse(n) && targets[int.Parse(n)] is uNodeRoot graph) {
					if(graph != null) {
						n = ReflectionUtils.GetRuntimeType(graph).Name;
					}
				}
			} else if(n == RUNTIME_ID) {
				if(genericData.references?.Count > 0) {
					return ReflectionUtils.GetRuntimeType(genericData.references[0])?.Name ?? "Missing Graph";
				}
			} else {
				t = TypeSerializer.Deserialize(n, false);
				if(t == null) return n;
			}
			if(t != null && t.IsGenericTypeDefinition && genericData.parameters != null && genericData.parameters.Length > 0) {
				string[] T = new string[genericData.parameters.Length];
				for(int i = 0; i < genericData.parameters.Length; i++) {
					T[i] = GetGenericName(genericData.parameters[i], targets);
				}
				n = String.Format("{0}<{1}>", t.Name.Split('`')[0], String.Join(", ", T));
			}
			while(array > 0) {
				n += "[]";
				array--;
			}
			return n;
		}

		/// <summary>
		/// Get parameter name from TypeData.
		/// </summary>
		/// <param name="genericData"></param>
		/// <param name="types"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static string GetParameterName(TypeData genericData, IList<string> types = null, IList<UnityEngine.Object> targets = null) {
			int array = 0;
			string n = genericData.name;
			while(n.EndsWith("[]")) {
				n = n.Remove(n.Length - 2);
				array++;
			}
			Type t = null;
			switch(n[0]) {
				case '$': {//For generic param
					n = n.Remove(0, 1);
					GenericParameterData data = FindGenericData(n, targets);
					if(data != null) {
						t = data.value;
					} else {
						t = typeof(object);
					}
					break;
				}
				case '#': {//for parameter references by index
					n = n.Remove(0, 1);
					if(types != null) {
						n = types[int.Parse(n)];
						t = TypeSerializer.Deserialize(n, false);
					}
					break;
				}
				case '@': {//for runtime type references
					n = n.Remove(0, 1);
					if(targets != null && targets.Count > int.Parse(n) && targets[int.Parse(n)] is uNodeRoot graph) {
						if(graph != null) {
							n = ReflectionUtils.GetRuntimeType(graph).Name;
						}
					}
					break;
				}
				case '[': {//for default runtime type references
					if(n == RUNTIME_ID) {
						if(genericData.references?.Count > 0) {
							return ReflectionUtils.GetRuntimeType(genericData.references[0])?.Name ?? "Missing Graph";
						}
					} else {
						goto default;
					}
					break;
				}
				case '!': {//Generic runtime type
					t = TypeSerializer.Deserialize(n.Remove(0, 1), false);
					if(t != null && t.IsGenericTypeDefinition && genericData.parameters != null && genericData.parameters.Length > 0) {
						string[] T = new string[genericData.parameters.Length];
						for(int i = 0; i < genericData.parameters.Length; i++) {
							T[i] = GetParameterName(genericData.parameters[i], types, targets);
						}
						n = String.Format("{0}<{1}>", t.Name.Split('`')[0], String.Join(", ", T));
					}
					break;
				}
				case '?': {//Array runtime type
					return GetParameterName(genericData.parameters[0], types, targets) + "[]";
				}
				default: {
					t = TypeSerializer.Deserialize(n, false);
					break;
				}
			}
			if(t != null && t.IsGenericTypeDefinition && genericData.parameters != null && genericData.parameters.Length > 0) {
				string[] T = new string[genericData.parameters.Length];
				for(int i = 0; i < genericData.parameters.Length; i++) {
					T[i] = GetParameterName(genericData.parameters[i], types, targets);
				}
				n = String.Format("{0}<{1}>", t.Name.Split('`')[0], String.Join(", ", T));
			}
			while(array > 0) {
				n += "[]";
				array--;
			}
			return n;
		}

		/// <summary>
		/// Get the generic types from ItemData.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static Type[] GetGenericTypes(MemberData.ItemData item, IList<UnityEngine.Object> targets) {
			Type[] GType = Type.EmptyTypes;
			if(item.genericArguments != null) {
				GType = new Type[item.genericArguments.Length];
				for(int i = 0; i < GType.Length; i++) {
					GType[i] = GetGenericType(item.genericArguments[i], targets);
				}
			}
			return GType;
		}

		/// <summary>
		/// Get the parameter types from ItemData.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <param name="genericType"></param>
		/// <returns></returns>
		public static Type[] GetParameterTypes(MemberData.ItemData item, IList<UnityEngine.Object> targets, params Type[] genericType) {
			Type[] PType = Type.EmptyTypes;
			if(item.parameters != null) {
				PType = new Type[item.parameters.Length];
				for(int i = 0; i < PType.Length; i++) {
					PType[i] = GetParameterType(item.parameters[i], genericType, targets);
				}
			}
			return PType;
		}

		/// <summary>
		/// Deserialize the MemberData Item only for Generic Parameter
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static Type[] DeserializeItemGeneric(MemberData.ItemData item, params UnityEngine.Object[] targets) {
			return DeserializeItemGeneric(item, targets as IList<UnityEngine.Object>);
		}

		/// <summary>
		/// Deserialize the MemberData Item only for Generic Parameter
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static Type[] DeserializeItemGeneric(MemberData.ItemData item, IList<UnityEngine.Object> targets) {
			Type[] GType = Type.EmptyTypes;
			if(item != null) {
				if(item.genericArguments != null && item.genericArguments.Length > 0) {
					GType = new Type[item.genericArguments.Length];
					for(int i = 0; i < GType.Length; i++) {
						GType[i] = GetGenericType(item.genericArguments[i], targets);
					}
				}
			}
			return GType;
		}

		/// <summary>
		/// Deserialize the MemberData Item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <param name="ParameterType"></param>
		public static void DeserializeMemberItem(MemberData.ItemData item, IList<UnityEngine.Object> targets, out Type[] ParameterType) {
			Type[] GType = Type.EmptyTypes;
			Type[] PType = Type.EmptyTypes;
			if(item != null) {
				if(item.genericArguments != null && item.genericArguments.Length > 0) {
					GType = new Type[item.genericArguments.Length];
					for(int i = 0; i < GType.Length; i++) {
						GType[i] = GetGenericType(item.genericArguments[i], targets);
					}
				}
				if(item.parameters != null && item.parameters.Length > 0) {
					PType = new Type[item.parameters.Length];
					for(int i = 0; i < PType.Length; i++) {
						PType[i] = GetParameterType(item.parameters[i], GType, targets);
					}
				}
			}
			ParameterType = PType;
		}

		/// <summary>
		/// Deserialize the MemberData Item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="targets"></param>
		/// <param name="GenericType"></param>
		/// <param name="ParameterType"></param>
		public static void DeserializeMemberItem(MemberData.ItemData item, IList<UnityEngine.Object> targets, out Type[] GenericType, out Type[] ParameterType, bool throwError = true) {
			Type[] GType = Type.EmptyTypes;
			Type[] PType = Type.EmptyTypes;
			if(item != null) {
				if(item.genericArguments != null) {
					GType = new Type[item.genericArguments.Length];
					for(int i = 0; i < GType.Length; i++) {
						GType[i] = GetGenericType(item.genericArguments[i], targets, throwError);
					}
				}
				if(item.parameters != null) {
					PType = new Type[item.parameters.Length];
					for(int i = 0; i < PType.Length; i++) {
						PType[i] = GetParameterType(item.parameters[i], GType, targets, throwError);
					}
				}
			}
			GenericType = GType;
			ParameterType = PType;
		}

		/// <summary>
		/// Get the Type of Parameter.
		/// </summary>
		/// <param name="typeData"></param>
		/// <param name="types"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static Type GetParameterType(TypeData typeData, IList<Type> types = null, IList<UnityEngine.Object> targets = null, bool throwError = true) {
			int array = 0;
			string n = typeData.name;
			while(n.EndsWith("[]")) {
				n = n.Remove(n.Length - 2);
				array++;
			}
			Type t = null;
			switch(n[0]) {
				case '$': {//For generic param
					n = n.Remove(0, 1);
					GenericParameterData data = FindGenericData(n, targets);
					if(data != null) {
						t = data.value;
					} else {
						t = typeof(object);
					}
					break;
				}
				case '#': {//for parameter references by index
					n = n.Remove(0, 1);
					if(types != null) {
						t = types[int.Parse(n)];
					} else {
						t = typeof(object);
					}
					break;
				}
				case '@': {//for runtime type references
					var index = int.Parse(n.Remove(0, 1));
					if(targets != null && targets.Count > index) {
						var graph = targets[index] as uNodeRoot;
						if(graph != null) {
							t = ReflectionUtils.GetRuntimeType(graph);
						} else {
							if(throwError) {
								throw new Exception("The graph reference is null");
							}
							return new MissingType("The graph reference is null/missing");
						}
					} else {
						throw new InvalidOperationException();
					}
					break;
				}
				case '[': {//for default runtime type references
					if(n == RUNTIME_ID) {
						if(typeData.references?.Count > 0) {
							return ReflectionUtils.GetRuntimeType(typeData.references[0]);
						}
					} else {
						goto default;
					}
					break;
				}
				case '!': {//Generic runtime type
					t = TypeSerializer.Deserialize(n.Remove(0, 1), throwError);
					if(t == null) {
						return new MissingType(n.Remove(0, 1));
					}
					if(t.IsGenericTypeDefinition && typeData.parameters != null && typeData.parameters.Length > 0) {
						Type[] T = new Type[typeData.parameters.Length];
						for(int i = 0; i < typeData.parameters.Length; i++) {
							T[i] = GetParameterType(typeData.parameters[i], types, targets, throwError);
						}
						t = ReflectionFaker.FakeGenericType(t, T);
						while(array > 0) {
							t = ReflectionFaker.FakeArrayType(t);
							array--;
						}
						return t;
					}
					break;
				}
				case '?': {//Array runtime type
					return ReflectionFaker.FakeArrayType(GetParameterType(typeData.parameters[0], types, targets, throwError));
				}
				default: {
					t = TypeSerializer.Deserialize(n, throwError);
					if(t == null) {
						return new MissingType(n);
					}
					break;
				}
			}
			if(t == null)
				return null;
			if(t.IsGenericTypeDefinition && typeData.parameters != null && typeData.parameters.Length > 0) {
				Type[] T = new Type[typeData.parameters.Length];
				for(int i = 0; i < typeData.parameters.Length; i++) {
					T[i] = GetParameterType(typeData.parameters[i], types, targets, throwError);
				}
				t = t.MakeGenericType(T);
			}
			while(array > 0) {
				t = t.MakeArrayType();
				array--;
			}
			return t;
		}

		/// <summary>
		/// Get the Type of Parameter.
		/// </summary>
		/// <param name="typeData"></param>
		/// <param name="types"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static System.Type GetParameterType(TypeData typeData, IList<GenericParameterData> types, IList<UnityEngine.Object> targets = null) {
			int array = 0;
			string n = typeData.name;
			while(n.EndsWith("[]")) {
				n = n.Remove(n.Length - 2);
				array++;
			}
			System.Type t = null;
			switch(n[0]) {
				case '$': {//For generic param
					n = n.Remove(0, 1);
					GenericParameterData data = FindGenericData(n, targets);
					if(data != null) {
						t = data.value;
					} else {
						t = typeof(object);
					}
					break;
				}
				case '#': {//for parameter references by index
					n = n.Remove(0, 1);
					if(types != null) {
						t = types[int.Parse(n)].value;
					} else {
						t = typeof(object);
					}
					break;
				}
				case '@': {//for runtime type references
					var index = int.Parse(n.Remove(0, 1));
					if(targets != null && targets.Count > index) {
						var graph = targets[index] as uNodeRoot;
						if(graph != null) {
							t = ReflectionUtils.GetRuntimeType(graph);
						} else {
							throw new Exception("The graph reference is null");
						}
					} else {
						throw new InvalidOperationException();
					}
					break;
				}
				case '[': {//for default runtime type references
					if(n == RUNTIME_ID) {
						if(typeData.references?.Count > 0) {
							return ReflectionUtils.GetRuntimeType(typeData.references[0]);
						}
					} else {
						goto default;
					}
					break;
				}
				case '!': {//Generic runtime type
					t = TypeSerializer.Deserialize(n.Remove(0, 1));
					if(t.IsGenericTypeDefinition && typeData.parameters != null && typeData.parameters.Length > 0) {
						Type[] T = new Type[typeData.parameters.Length];
						for(int i = 0; i < typeData.parameters.Length; i++) {
							T[i] = GetParameterType(typeData.parameters[i], types, targets);
						}
						t = ReflectionFaker.FakeGenericType(t, T);
						while(array > 0) {
							t = ReflectionFaker.FakeArrayType(t);
							array--;
						}
						return t;
					}
					break;
				}
				case '?': {//Array runtime type
					return ReflectionFaker.FakeArrayType(GetParameterType(typeData.parameters[0], types, targets));
				}
				default: {
					t = TypeSerializer.Deserialize(n);
					break;
				}
			}
			if(t.IsGenericTypeDefinition && typeData.parameters != null && typeData.parameters.Length > 0) {
				Type[] T = new Type[typeData.parameters.Length];
				for(int i = 0; i < typeData.parameters.Length; i++) {
					T[i] = GetParameterType(typeData.parameters[i], types, targets);
				}
				t = t.MakeGenericType(T);
			}
			while(array > 0) {
				t = t.MakeArrayType();
				array--;
			}
			return t;
		}

		/// <summary>
		/// Find a GenericParameterData by name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static GenericParameterData FindGenericData(string name, IList<UnityEngine.Object> targets) {
			if(targets == null)
				return null;
			GenericParameterData data = null;
			for(int i = targets.Count - 1; i > 0; i--) {
				uNodeFunction function = targets[i] as uNodeFunction;
				if(function) {
					GenericParameterData d = function.GetGenericParameter(name);
					if(d != null) {
						return d;
					}
				} else if(data == null && targets[i] is IGenericParameterSystem) {
					var root = targets[i] as IGenericParameterSystem;
					data = root.GetGenericParameter(name);
					if(data != null) {
						return data;
					}
				}
			}
			return data;
		}

		/// <summary>
		/// Get the Type of Generic Parameter.
		/// </summary>
		/// <param name="genericData"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static System.Type GetGenericType(TypeData genericData, IList<UnityEngine.Object> targets, bool throwError = true) {
			int array = 0;
			string n = genericData.name;
			while(n.EndsWith("[]")) {
				n = n.Remove(n.Length - 2);
				array++;
			}
			System.Type t = null;
			switch(genericData.name[0]) {
				case '$': {//For generic param
					GenericParameterData data = FindGenericData(n.Remove(0, 1), targets);
					if(data != null) {
						t = data.value;
					} else {
						t = typeof(object);
					}
					break;
				}
				case '@': {//for runtime type references
					var index = int.Parse(genericData.name.Remove(0, 1));
					if(targets != null && targets.Count > index) {
						var graph = targets[index] as uNodeRoot;
						if(graph != null) {
							t = ReflectionUtils.GetRuntimeType(graph);
						} else {
							throw new Exception("The graph reference is null");
						}
					} else {
						throw new InvalidOperationException();
					}
					break;
				}
				case '[': {//for default runtime type references
					if(n == RUNTIME_ID) {
						if(genericData.references?.Count > 0) {
							return ReflectionUtils.GetRuntimeType(genericData.references[0]);
						}
					} else {
						goto default;
					}
					break;
				}
				case '!': {//Generic runtime type
					t = TypeSerializer.Deserialize(n.Remove(0, 1), throwError);
					if(t == null) {
						return new MissingType(n.Remove(0, 1));
					}
					if(t.IsGenericTypeDefinition && genericData.parameters != null && genericData.parameters.Length > 0) {
						Type[] T = new Type[genericData.parameters.Length];
						for(int i = 0; i < genericData.parameters.Length; i++) {
							var GP = GetGenericType(genericData.parameters[i], targets, throwError);
							if(GP is MissingType) {
								return GP;
							}
							T[i] = GP;
						}
						t = ReflectionFaker.FakeGenericType(t, T);
					}
					break;
				}
				case '?': {//Array runtime type
					t = GetGenericType(genericData.parameters[0], targets, throwError);
					if(t is MissingType) {
						return t;
					}
					t = ReflectionFaker.FakeArrayType(t);
					break;
				}
				default: {
					t = TypeSerializer.Deserialize(n, throwError);
					if(t == null) {
						return new MissingType(n);
					}
					break;
				}
			}
			if(t != null && t.IsGenericTypeDefinition && genericData.parameters != null && genericData.parameters.Length > 0) {
				Type[] T = new Type[genericData.parameters.Length];
				for(int i = 0; i < genericData.parameters.Length; i++) {
					var GP = GetGenericType(genericData.parameters[i], targets, throwError);
					if(GP is MissingType) {
						return GP;
					}
					T[i] = GP;
				}
				t = t.MakeGenericType(T);
			}
			while(array > 0) {
				t = t.MakeArrayType();
				array--;
			}
			return t;
		}

		/// <summary>
		/// Create list of type data from Members.
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		public static List<TypeData> MakeTypeDatas(IList<MemberData> members, List<UnityEngine.Object> references = null) {
			List<TypeData> typeDatas = new List<TypeData>();
			foreach(MemberData member in members) {
				if(member.genericData != null) {
					typeDatas.Add(member.genericData);
				} else {
					typeDatas.Add(GetTypeData(member, references));
				}
			}
			return typeDatas;
		}

		public static void UpdateMemberInstance(MemberData member, Type type) {
			if(type == null) {
				type = typeof(object);
			}
			if((member.instance == null || !(member.instance is MemberData))) {
				if(ReflectionUtils.CanCreateInstance(type)) {
					if(member.instance != null && !(member.instance is MemberData)) {
						member.instance = MemberData.CreateFromValue(member.instance, type);
					} else {
						member.instance = MemberData.CreateValueFromType(type);
					}
				} else {
					if(member.instance != null) {
						member.instance = MemberData.CreateFromValue(member.instance, type);
					} else {
						member.instance = MemberData.CreateValueFromType(type);
					}
				}
			} else if(member.instance != null && !(member.instance is MemberData)) {
				member.instance = MemberData.CreateFromValue(member.instance, type);
			}
		}

		public static void UpdateMemberNames(MemberData member) {
			if(member == null || !member.isTargeted) return;
			switch (member.targetType) {
				case MemberData.TargetType.Type:
				case MemberData.TargetType.Field:
				case MemberData.TargetType.Property:
				case MemberData.TargetType.Method:
				case MemberData.TargetType.Constructor:
					if(member.startType is RuntimeType runtimeType) {
						member.startName = runtimeType.Name;
					}
					break;
			}
		}

		// private static List<MemberData.ItemData> BuildItemFromMemberInfos(IEnumerable<MemberInfo> members) {
		// 	return null;
		// }

		private static MemberData.ItemData BuildItemFromMemberInfo(MemberInfo member) {
			MemberData.ItemData iData = null;
			MethodBase methodInfo = member as MethodBase;
			if(methodInfo != null) {
				Type[] genericMethodArgs = Type.EmptyTypes;
				if (methodInfo.MemberType != MemberTypes.Constructor) {
					if (genericMethodArgs != null && genericMethodArgs.Length > 0) {
						TypeData[] param = new TypeData[genericMethodArgs.Length];
						for (int i = 0; i < genericMethodArgs.Length; i++) {
							param[i] = MemberDataUtility.GetTypeData(genericMethodArgs[i], null);
						}
						iData = new MemberData.ItemData() { genericArguments = param };
					}
				}
				ParameterInfo[] paramsInfo = methodInfo.GetParameters();
				if(paramsInfo.Length > 0) {
					TypeData[] paramData = new TypeData[paramsInfo.Length];
					for(int x = 0; x < paramsInfo.Length; x++) {
						TypeData gData = MemberDataUtility.GetTypeData(paramsInfo[x],
							genericMethodArgs != null ? genericMethodArgs.Select(it => it.Name).ToArray() : null);
						paramData[x] = gData;
					}
					if(iData == null) {
						iData = new MemberData.ItemData();
					}
					iData.parameters = paramData;
				}
			}
			return iData;
		}

		/// <summary>
		/// Update MemberData to fix commonly issue.
		/// </summary>
		/// <param name="member"></param>
		public static void UpdateMemberData(MemberData member) {
			if(member == null) return;
			if(member.isTargeted) {
				if (member.SerializedItems?.Length > 0) {
					while (member.namePath.Length > member.SerializedItems.Length) {
						var arr = member.SerializedItems;
						uNodeUtility.AddArrayAt(ref arr, null, 0);
						member.SerializedItems = arr;
					}
					while (member.SerializedItems.Length > member.namePath.Length) {
						var arr = member.SerializedItems;
						uNodeUtility.RemoveArrayAt(ref arr, 0);
						member.SerializedItems = arr;
					}
				}
				// var members = member.GetMembers(false);
				// if(members != null && members.Length > 0 && members.Length + 1 == member.items.Length) {
				// 	for (int i = 0; i < member.items.Length; i++) {
				// 		if (member.Items[i] == null)
				// 			continue;
				// 		var mem = members[i - 1];
				// 		if (mem is MethodBase method) {
				// 			var paramsType = member.ParameterTypes[i];
				// 			var parameters = method.GetParameters();
				// 			if (paramsType.Length == parameters.Length) {
				// 				for (int x = 0; x < paramsType.Length; x++) {
				// 					paramsType[x] = parameters[x].ParameterType;
				// 				}
				// 			}
				// 			if (method.MemberType != MemberTypes.Constructor) {
				// 				var genericType = member.GenericTypes[i];
				// 				var genericParameters = method.GetGenericArguments();
				// 				if (genericType.Length == genericParameters.Length) {
				// 					for (int x = 0; x < genericType.Length; x++) {
				// 						genericType[x] = genericParameters[x];
				// 					}
				// 				}
				// 			}
				// 			member.Items[i] = BuildItemFromMemberInfo(mem);
				// 			member.items[i] = JsonHelper.Serialize(member.Items[i]);
				// 		}
				// 	}
				// }
			}
		}

		public static object GetActualInstance(MemberData member) {
			switch(member.targetType) {
				case MemberData.TargetType.uNodeVariable:
				case MemberData.TargetType.uNodeGroupVariable:
				case MemberData.TargetType.uNodeLocalVariable:
					return MemberData.CreateFromValue(member.GetVariable(), member.startTarget as UnityEngine.Object);
				case MemberData.TargetType.uNodeProperty:
					return MemberData.CreateFromValue(member.GetProperty());
				case MemberData.TargetType.uNodeParameter:
					var PS = member.startTarget as IParameterSystem;
					return MemberData.CreateFromValue(PS.GetParameterData(member.startName), PS);
			}
			return member.instance;
		}

		/// <summary>
		/// Update MultipurposeMember to fix commonly issue.
		/// </summary>
		/// <param name="member"></param>
		public static void UpdateMultipurposeMember(MultipurposeMember member) {
			if(member == null) {
				throw new ArgumentNullException("member");
			}
			if(member.target == null) {
				member.target = new MemberData();
			}
			if(member.parameters == null) {
				member.parameters = new MemberData[0];
			}
			if(member.target.isTargeted) {
				UpdateMemberData(member.target);
				if(member.target.SerializedItems?.Length > 0) {
					var ParameterType = MemberData.Utilities.SafeGetParameterTypes(member.target);
					if(ParameterType == null) return;
					int totalParam = 0;
					for(int i = 0; i < member.target.SerializedItems.Length; i++) {
						if(member.target.Items[i] == null)
							continue;
						System.Type[] paramsType = ParameterType[i];
						if(paramsType.Length > 0) {
							while(paramsType.Length + totalParam > member.parameters.Length) {
								uNodeUtility.AddArray(ref member.parameters, null);
							}
							for(int x = 0; x < paramsType.Length; x++) {
								System.Type PType = paramsType[x];
								if(PType == null) {
									totalParam++;
									continue;
								}
								var param = member.parameters[totalParam];
								if(param == null || !param.isTargeted) {
									if(PType is MissingType) {
										if(member.parameters[totalParam] == null) {
											member.parameters[totalParam] = MemberData.none;
										}
									} else {
										member.parameters[totalParam] = MemberData.CreateValueFromType(PType);
									}
								}
								totalParam++;
							}
						}
					}
					while(member.parameters.Length > totalParam) {
						uNodeUtility.RemoveArrayAt(ref member.parameters, member.parameters.Length - 1);
					}
				}
			}
		}
	}
}