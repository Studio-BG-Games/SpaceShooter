using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using MaxyGames.uNode;

namespace MaxyGames {
	public static class ReflectionUtils {
		public static readonly BindingFlags publicFlags = BindingFlags.Public | BindingFlags.Instance;
		public static readonly BindingFlags publicAndNonPublicFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		public static readonly BindingFlags publicStaticFlags = BindingFlags.Public | BindingFlags.Static;
		public static readonly BindingFlags publicAndNonPublicStaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		private static Dictionary<Assembly, Type[]> assemblyTypeMap = new Dictionary<Assembly, Type[]>();
		private static Dictionary<UnityEngine.Object, RuntimeType> _runtimeTypesMap = new Dictionary<UnityEngine.Object, RuntimeType>(EqualityComparer<UnityEngine.Object>.Default);


		private static Assembly[] loadedAssemblies, assemblies;
		private static Assembly[] runtimeAssembly = new Assembly[0];
		private static HashSet<Assembly> loadedRuntimeAssemblies = new HashSet<Assembly>();

		/// <summary>
		/// Get the generated runtime assembly ( uNode compiled assembly )
		/// </summary>
		/// <returns></returns>
		public static Assembly[] GetRuntimeAssembly() {
			return runtimeAssembly;
		}

		/// <summary>
		/// Register a new Runtime Assembly
		/// </summary>
		/// <param name="assembly"></param>
		public static void RegisterRuntimeAssembly(Assembly assembly) {
			if(assembly == null)
				return;
			uNodeUtility.AddArrayAt(ref runtimeAssembly, assembly, 0);
			loadedRuntimeAssemblies.Add(assembly);
		}

		public static void CleanRuntimeAssembly() {
			runtimeAssembly = new Assembly[0];
		}

		public static IEnumerable<Assembly> GetAssemblies() {
			if(loadedAssemblies == null) {
				List<Assembly> ass = AppDomain.CurrentDomain.GetAssemblies().ToList();
				for(int i = 0; i < ass.Count; i++) {
					try {
						if(loadedRuntimeAssemblies.Contains(ass[i])) {
							ass.RemoveAt(i);
							i--;
						}
					}
					catch {
						ass.RemoveAt(i);
						i--;
					}
				}
				loadedAssemblies = ass.ToArray();
			}
			return loadedAssemblies;
		}

		public static void UpdateAssemblies() {
			loadedAssemblies = null;
			TypeSerializer.CleanCache();
		}

		/// <summary>
		/// Get the available static assembly
		/// </summary>
		/// <returns></returns>
		public static Assembly[] GetStaticAssemblies() {
			if(assemblies == null) {
				List<Assembly> ass = AppDomain.CurrentDomain.GetAssemblies().ToList();
				for(int i = 0; i < ass.Count; i++) {
					try {
						if(/*string.IsNullOrEmpty(ass[i].Location) ||*/ass[i].IsDynamic || loadedRuntimeAssemblies.Contains(ass[i])) {
							ass.RemoveAt(i);
							i--;
						}
					}
					catch {
						ass.RemoveAt(i);
						i--;
					}
				}
				assemblies = ass.ToArray();
			}
			return assemblies;
		}

		public static IEnumerable<T> GetAssemblyAttributes<T>() where T : Attribute {
			foreach(var ass in GetStaticAssemblies()) {
				var atts = GetAssemblyAttribute<T>(ass);
				if(atts != null) {
					foreach(var a in atts) {
						yield return a;
					}
				}
			}
		}

		public static IEnumerable<T> GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute {
			if(assembly.IsDefined(typeof(T), true)) {
				return assembly.GetCustomAttributes<T>();
			}
			return null;
		}

		public static IEnumerable<Attribute> GetAssemblyAttributes(Type attributeType) {
			foreach(var ass in GetStaticAssemblies()) {
				var atts = GetAssemblyAttribute(attributeType, ass);
				if(atts != null) {
					foreach(var a in atts) {
						yield return a;
					}
				}
			}
		}

		public static IEnumerable<Attribute> GetAssemblyAttribute(Type attributeType, Assembly assembly) {
			if(assembly.IsDefined(attributeType, true)) {
				return assembly.GetCustomAttributes(attributeType);
			}
			return null;
		}

		public static Type[] GetAssemblyTypes(Assembly assembly) {
			Type[] types;
			if(!assemblyTypeMap.TryGetValue(assembly, out types)) {
				try {
					types = assembly.GetTypes();
					assemblyTypeMap[assembly] = types;
				}
				catch {
					Debug.LogError("Error on getting type from assembly:" + assembly.FullName);
				}
			}
			//Type[] result = new Type[types.Length];
			//Array.Copy(types, result, types.Length);
			return types;
		}

		/// <summary>
		/// Get specific runtime type.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static RuntimeType GetRuntimeType(UnityEngine.Object obj) {
			if (obj is uNodeRoot) {
				return GetRuntimeType(obj as uNodeRoot);
			} else if (obj is uNodeInterface) {
				return GetRuntimeType(obj as uNodeInterface);
			} else if (obj is uNodeSpawner) {
				return GetRuntimeType((obj as uNodeSpawner).target);
			} else if (obj is uNodeAssetInstance) {
				return GetRuntimeType((obj as uNodeAssetInstance).target);
			} else {
				return null;
			}
		}

		/// <summary>
		/// Get specific runtime type.
		/// </summary>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static RuntimeType GetRuntimeType(uNodeRoot graph) {
			if (!_runtimeTypesMap.TryGetValue(graph, out var type)) {
				if (graph is uNodeRuntime runtime && runtime.originalGraph != null) {
					type = GetRuntimeType(runtime.originalGraph);
					_runtimeTypesMap[graph] = type;
					return type;
				}
				type = new RuntimeGraphType(graph);
				_runtimeTypesMap[graph] = type;
			}
			return type;
		}

		/// <summary>
		/// Get specific runtime type.
		/// </summary>
		/// <param name="graphInterface"></param>
		/// <returns></returns>
		public static RuntimeType GetRuntimeType(uNodeInterface graphInterface) {
			if (!_runtimeTypesMap.TryGetValue(graphInterface, out var type)) {
				type = new RuntimeGraphInterface(graphInterface);
				_runtimeTypesMap[graphInterface] = type;
			}
			return type;
		}

		internal static void ClearInvalidRuntimeType() {
			List<UnityEngine.Object> runtimeTypes = new List<UnityEngine.Object>();
			foreach(var pair in _runtimeTypesMap) {
				if(!pair.Value.IsValid()) {
					runtimeTypes.Add(pair.Key);
				}
			}
			foreach(var obj in runtimeTypes) {
				_runtimeTypesMap.Remove(obj);
			}
		}

		/// <summary>
		/// Is the 'instance' is a valid instance of type 'type'
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsValidRuntimeInstance(object instance, RuntimeType type) {
			return type.IsInstanceOfType(instance);
			// if (type is RuntimeGraphType graphType) {
			// 	if (graphType.target is IClassComponent component) {
			// 		if (instance is IRuntimeComponent runtime) {
			// 			return component.uniqueIdentifier == runtime.uniqueIdentifier;
			// 		}
			// 	} else if (graphType.target is IClassAsset asset) {
			// 		if (instance is IRuntimeAsset runtime) {
			// 			return asset.uniqueIdentifier == runtime.uniqueIdentifier;
			// 		}
			// 	}
			// } else {
			// 	return type.IsInstanceOfType(instance);
			// }
			// return false;
		}

		public static Type MakeGenericType(Type type, params Type[] typeArguments) {
			if(typeArguments == null) {
				throw new ArgumentNullException(nameof(typeArguments));
			}
			bool flag = type is RuntimeType;
			if(!flag) {
				for(int i=0; i< typeArguments.Length;i++) {
					if(typeArguments[i] is RuntimeType) {
						flag = true;
						break;
					}
				}
			}
			if(flag) {
				return ReflectionFaker.FakeGenericType(type, typeArguments);
			} else {
				return type.MakeGenericType(typeArguments);
			}
		}

		/// <summary>
		/// Get the unique id of the instance Component graph or Asset graph
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static string GetInstanceUID(object instance) {
			if (instance is IRuntimeComponent) {
				return (instance as IRuntimeComponent).uniqueIdentifier;
			} else if (instance is IRuntimeAsset) {
				return (instance as IRuntimeAsset).uniqueIdentifier;
			}
			return string.Empty;
		}

		/// <summary>
		/// Get the actual type from instance
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static Type GetActualTypeFromInstance(object instance, bool useStartType = false) {
			if (instance == null) return null;
			if (instance is MemberData) {
				var mData = instance as MemberData;
				if (!useStartType) {
					return mData.type;
				}
				if (mData.IsTargetingGraph) {
					return GetActualTypeFromInstance(mData.startType);
				}
				if (mData.isTargeted) {
					var startTarget = mData.startTarget;
					if(startTarget is uNodeRoot) {
						return GetRuntimeType(startTarget as uNodeRoot);
					} else if(startTarget is MultipurposeNode) {
						return GetActualTypeFromInstance((startTarget as MultipurposeNode).target.target);
					} else if(startTarget is Node) {
						var node = startTarget as Node;
						if(mData.IsTargetingNode) {
							return GetActualTypeFromInstance(startTarget);
						} else {
							return mData.startType;
						}
					} else {
						return GetActualTypeFromInstance(startTarget);
					}
				}
			} else if (instance is uNodeRoot) {
				return GetRuntimeType(instance as uNodeRoot);
			} else if (instance is RuntimeType) {
				return instance as RuntimeType;
			} else if (instance is RuntimeField) {
				return (instance as RuntimeField).owner;
			} else if (instance is RuntimeProperty) {
				return (instance as RuntimeProperty).owner;
			} else if (instance is RuntimeMethod) {
				return (instance as RuntimeMethod).owner;
			} else if (instance is uNodeSpawner) {
				return GetRuntimeType((instance as uNodeSpawner).target);
			} else if (instance is uNodeAssetInstance) {
				return GetRuntimeType((instance as uNodeAssetInstance).target);
			} else if(instance is Node) {
				return (instance as Node).ReturnType();
			} else if(instance is RootObject) {
				return (instance as RootObject).ReturnType();
			}
			return instance.GetType();
		}

		/// <summary>
		/// Get specific runtime type from instance of a graph / object
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static RuntimeType GetRuntimeTypeFromInstance(object instance, bool useStartType = false) {
			if (instance is MemberData) {
				var mData = instance as MemberData;
				if (!useStartType) {
					return mData.type as RuntimeType;
				}
				if (mData.IsTargetingGraph) {
					return GetRuntimeTypeFromInstance(mData.startType);
				}
				if (mData.isTargeted) {
					var startTarget = mData.startTarget;
					if (startTarget is uNodeRoot graph) {
						return GetRuntimeType(graph);
					} else if (startTarget is MultipurposeNode multipurposeNode) {
						return GetRuntimeTypeFromInstance(multipurposeNode.target.target);
					} else {
						return GetRuntimeTypeFromInstance(startTarget);
					}
				}
			} else if (instance is uNodeRoot) {
				return GetRuntimeType(instance as uNodeRoot);
			} else if (instance is RuntimeType) {
				return instance as RuntimeType;
			} else if (instance is RuntimeField) {
				return (instance as RuntimeField).owner;
			} else if (instance is RuntimeProperty) {
				return (instance as RuntimeProperty).owner;
			} else if (instance is RuntimeMethod) {
				return (instance as RuntimeMethod).owner;
			} else if (instance is uNodeSpawner) {
				return GetRuntimeType((instance as uNodeSpawner).target);
			} else if (instance is uNodeAssetInstance) {
				return GetRuntimeType((instance as uNodeAssetInstance).target);
			}
			return null;
		}

		/// <summary>
		/// Is the member targeting runtime type?
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static bool HasRuntimeType(MemberData member) {
			if (member == null) return false;
			if (member.targetType == MemberData.TargetType.uNodeType) {
				return true;
			}
			var instance = member.instance;
			if (instance is MemberData m) {
				return HasRuntimeType(m);
			}
			return false;
		}

		/// <summary>
		/// Is the member targeting runtime type?
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		public static bool HasRuntimeType(params MemberData[] members) {
			if (members == null) return false;
			foreach (var m in members) {
				if (HasRuntimeType(m)) {
					return true;
				}
			}
			return false;
		}

#region Get Members
		/// <summary>
		/// Gets the members (properties, methods, fields, events, and so on) of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<MemberInfo> GetMembers(Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetMember(name, flags)) {
				yield return member;
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetMembers(iface, name, flags | BindingFlags.DeclaredOnly)) {
						yield return member;
					}
				}
			}
		}

		/// <summary>
		/// Gets the members (properties, methods, fields, events, and so on) of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="validation"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<MemberInfo> GetMembers(Type type, string name, Func<MemberInfo, bool> validation, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetMember(name, flags)) {
				if(validation(member)) {
					yield return member;
				}
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetMembers(iface, name, validation, flags | BindingFlags.DeclaredOnly)) {
						if(validation(member)) {
							yield return member;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the members (properties, methods, fields, events, and so on) of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<MemberInfo> GetMembers(Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetMembers(flags)) {
				yield return member;
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetMembers(iface, flags | BindingFlags.DeclaredOnly)) {
						yield return member;
					}
				}
			}
		}

		/// <summary>
		/// Gets the members (properties, methods, fields, events, and so on) of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="validation"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<MemberInfo> GetMembers(Type type, Func<MemberInfo, bool> validation, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetMembers(flags)) {
				if(validation(member)) {
					yield return member;
				}
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetMembers(iface, validation, flags | BindingFlags.DeclaredOnly)) {
						if(validation(member)) {
							yield return member;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the methods of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetMethods(flags)) {
				yield return member;
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetMethods(iface, flags | BindingFlags.DeclaredOnly)) {
						yield return member;
					}
				}
			}
		}

		/// <summary>
		/// Gets the methods of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="validation"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> GetMethods(Type type, Func<MemberInfo, bool> validation, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetMethods(flags)) {
				if(validation(member)) {
					yield return member;
				}
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetMethods(iface, validation, flags | BindingFlags.DeclaredOnly)) {
						if(validation(member)) {
							yield return member;
						}
					}
				}
			}
		}

		/// <summary>
		/// Get specific member info of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static MemberInfo GetMember(Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			var member = type.GetMember(name, flags);
			if(member.Length > 0) {
				return member[0];
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					member = iface.GetMember(name, flags);
					if(member.Length > 0) {
						return member[0];
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets a specific method of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static MethodInfo GetMethod(Type type, string name, params Type[] types) {
			var member = type.GetMethod(name, types);
			if(member != null)
				return member;
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					member = iface.GetMethod(name, types);
					if(member != null) {
						return member;
					}
				}
			}
			return member;
		}

		/// <summary>
		/// Gets a specific method of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static MethodInfo GetMethod(Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			var member = type.GetMethod(name, flags);
			if(member != null)
				return member;
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					member = iface.GetMethod(name, flags);
					if(member != null) {
						return member;
					}
				}
			}
			return member;
		}

		/// <summary>
		/// Gets a specific method of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static MethodInfo GetMethod(Type type, string name, Type[] types, BindingFlags flags, Binder binder = null, ParameterModifier[] modifiers = null) {
			var member = type.GetMethod(name, flags, binder, types, modifiers);
			if(member != null)
				return member;
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					member = iface.GetMethod(name, flags, binder, types, modifiers);
					if(member != null) {
						return member;
					}
				}
			}
			return member;
		}

		/// <summary>
		/// Gets the properties of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			foreach(var member in type.GetProperties(flags)) {
				yield return member;
			}
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					foreach(var member in GetProperties(iface, flags | BindingFlags.DeclaredOnly)) {
						yield return member;
					}
				}
			}
		}

		/// <summary>
		/// Gets a specific property of the current Type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static PropertyInfo GetProperty(Type type, string name, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) {
			var member = type.GetProperty(name, flags);
			if(member != null)
				return member;
			if(type.IsInterface) {
				foreach(var iface in type.GetInterfaces()) {
					member = iface.GetProperty(name, flags);
					if(member != null) {
						return member;
					}
				}
			}
			return member;
		}
#endregion

		public static FieldInfo FindFieldInfo(Type parentType, string path) {
			FieldInfo fInfo = null;
			string[] strArray = path.Split('.');
			for (int i = 0; i < strArray.Length; i++) {
				fInfo = parentType.GetField(strArray[i]);
				parentType = fInfo.FieldType;
			}
			return fInfo;
		}

		public static object GetFieldValue(object start, string path) {
			if (start == null)
				return null;
			object parent = start;
			Type t = parent.GetType();
			FieldInfo fInfo = null;
			string[] strArray = path.Split('.');
			for (int i = 0; i < strArray.Length; i++) {
				fInfo = t.GetField(strArray[i]);
				if (fInfo == null)
					throw new NullReferenceException("could not find field in path : " + path + "\nType:" + t.FullName);
				t = fInfo.FieldType;
				if (parent != null) {
					parent = fInfo.GetValueOptimized(parent);
				} else {
					return null;
				}
			}
			return parent;
		}

		public static object GetFieldValue(object start, string path, BindingFlags flags) {
			if (start == null)
				return null;
			object parent = start;
			Type t = parent.GetType();
			FieldInfo fInfo = null;
			string[] strArray = path.Split('.');
			for (int i = 0; i < strArray.Length; i++) {
				fInfo = t.GetField(strArray[i], flags);
				if (fInfo == null)
					throw new NullReferenceException("could not find field in path : " + path + "\nType:" + t.FullName);
				t = fInfo.FieldType;
				if (parent != null) {
					parent = fInfo.GetValueOptimized(parent);
				} else {
					return null;
				}
			}
			return parent;
		}

		public static object GetPropertyValue(object start, string path, BindingFlags flags) {
			if (start == null)
				return null;
			object parent = start;
			Type t = parent.GetType();
			PropertyInfo info = null;
			string[] strArray = path.Split('.');
			for (int i = 0; i < strArray.Length; i++) {
				info = t.GetProperty(strArray[i], flags);
				if (info == null)
					throw new NullReferenceException("could not find property in path : " + path + "\nType:" + t.FullName);
				t = info.PropertyType;
				if (parent != null) {
					parent = info.GetValueOptimized(parent);
				} else {
					return null;
				}
			}
			return parent;
		}

		public static void SetPropertyValue(object start, string name, object value, BindingFlags flags) {
			if (start == null)
				return;
			object parent = start;
			Type t = parent.GetType();
			var info = t.GetProperty(name, flags);
			if (info == null)
				throw new NullReferenceException("could not find property in path : " + name + "\nType:" + t.FullName);
			info.SetValueOptimized(parent, value);
		}

		public static Type GetActualFieldType(FieldInfo field, object parent) {
			object[] att = field.GetCustomAttributes(false);
			FilterAttribute filter;
			ObjectTypeAttribute objectType;
			TryCorrectingAttribute(parent, ref att, out filter, out objectType);
			if (filter != null && filter.Types.Count == 1 && filter.Types[0] != null) {
				return filter.Types[0];
			} else if (objectType != null && objectType.type != null) {
				return objectType.type;
			}
			return field.FieldType;
		}

		public static Type GetActualFieldType(FieldInfo field, object parent, ref object[] attributes) {
			FilterAttribute filter;
			ObjectTypeAttribute objectType;
			return GetActualFieldType(field, parent, ref attributes, out filter, out objectType);
		}

		public static Type GetActualFieldType(FieldInfo field, object parent, ref object[] attributes, out FilterAttribute filter) {
			ObjectTypeAttribute objectType;
			return GetActualFieldType(field, parent, ref attributes, out filter, out objectType);
		}

		public static Type GetActualFieldType(FieldInfo field, object parent, ref object[] attributes, out ObjectTypeAttribute objectType) {
			FilterAttribute filter;
			return GetActualFieldType(field, parent, ref attributes, out filter, out objectType);
		}

		public static Type GetActualFieldType(FieldInfo field, object parent, ref object[] attributes, out FilterAttribute filter, out ObjectTypeAttribute objectType) {
			TryCorrectingAttribute(parent, ref attributes, out filter, out objectType);
			if (filter != null && filter.Types.Count == 1 && filter.Types[0] != null) {
				return filter.Types[0];
			} else if (objectType != null && objectType.type != null) {
				return objectType.type;
			}
			return field.FieldType;
		}

		public static T GetAttributeFrom<T>(object from, bool inherit = false) where T : Attribute {
			return from.GetType().GetCustomAttribute(typeof(T), inherit) as T;
		}

		public static T GetAttribute<T>(params object[] attributes) where T : Attribute {
			if (attributes == null)
				return null;
			foreach (var v in attributes) {
				if (v is T) {
					return v as T;
				}
			}
			return null;
		}

		public static bool TryCorrectingAttribute(object parentField, ref object[] attributes) {
			if (parentField == null)
				return true;
			ObjectTypeAttribute objectType = GetAttribute<ObjectTypeAttribute>(attributes);
			if (objectType == null)
				return true;
			FilterAttribute filter = GetAttribute<FilterAttribute>(attributes);
			Type memberType = null;
			if (!string.IsNullOrEmpty(objectType.targetFieldPath)) {
				FieldInfo info = FindFieldInfo(parentField.GetType(), objectType.targetFieldPath);
				bool flag = false;
				if (info != null) {
					if (info.FieldType == typeof(MemberData)) {
						MemberData member = GetFieldValue(parentField, objectType.targetFieldPath) as MemberData;
						if (member != null && member.isAssigned) {
							if (objectType.isElementType && member.targetType == MemberData.TargetType.Type) {
								flag = true;
								memberType = member.startType;
							} else {
								memberType = member.type;
							}
						}
						if (memberType != null && memberType == typeof(void) && filter != null && filter.SetMember) {
							return false;
						}
					} else if (info.FieldType == typeof(string)) {
						string val = GetFieldValue(parentField, objectType.targetFieldPath) as string;
						Type tVal = TypeSerializer.Deserialize(val, false);
						if (tVal != null) {
							memberType = tVal;
						}
					} else if (info.FieldType == typeof(MultipurposeMember)) {
						MultipurposeMember member = GetFieldValue(parentField, objectType.targetFieldPath) as MultipurposeMember;
						if (member != null && member.target.isAssigned) {
							if (objectType.isElementType && member.target.targetType == MemberData.TargetType.Type) {
								flag = true;
								memberType = member.target.startType;
							} else {
								memberType = member.target.type;
							}
						}
						if (memberType != null && memberType == typeof(void) && filter != null && filter.SetMember) {
							return false;
						}
					}
				}
				if (memberType != null) {
					if (filter == null) {
						filter = new FilterAttribute();
					}
					filter.Types.Clear();
					if (objectType.isElementType) {
						Type t = flag ? memberType : memberType.ElementType();
						if (t != null) {
							objectType.type = t;
							filter.Types.Add(t);
						} else {
							objectType.type = memberType;
							filter.Types.Add(memberType);
						}
					} else {
						objectType.type = memberType;
						filter.Types.Add(memberType);
					}
				} else {
					return false;
				}
			}
			if (objectType.type != null) {
				if (filter == null) {
					filter = new FilterAttribute();
				}
				if (filter.Types.Count == 0) {
					filter.Types.Add(objectType.type);
				}
				if (attributes != null && attributes.Length > 0) {
					for (int i = 0; i < attributes.Length; i++) {
						if (filter == null && objectType == null)
							break;
						if (filter != null && attributes[i] is FilterAttribute) {
							attributes[i] = filter;
							filter = null;
						}
						if (attributes[i] == objectType) {
							attributes[i] = objectType;
							objectType = null;
						}
					}
					if (filter != null) {
						uNodeUtility.AddArray(ref attributes, filter);
					}
					if (objectType != null) {
						uNodeUtility.AddArray(ref attributes, objectType);
					}
				}
			}
			return true;
		}

		public static bool TryCorrectingAttribute(object parentField, ref object[] attributes, out FilterAttribute filter) {
			ObjectTypeAttribute objectType;
			return TryCorrectingAttribute(parentField, ref attributes, out filter, out objectType);
		}

		public static bool TryCorrectingAttribute(object parentField, ref object[] attributes, out ObjectTypeAttribute objectType) {
			FilterAttribute filter;
			return TryCorrectingAttribute(parentField, ref attributes, out filter, out objectType);
		}

		public static bool TryCorrectingAttribute(object parentField, ref object[] attributes, out FilterAttribute filter, out ObjectTypeAttribute objectType) {
			if (parentField == null) {
				objectType = null;
				filter = null;
				return true;
			}
			objectType = GetAttribute<ObjectTypeAttribute>(attributes);
			if (objectType == null) {
				filter = null;
				return true;
			}
			filter = GetAttribute<FilterAttribute>(attributes);
			Type memberType = null;
			if (!string.IsNullOrEmpty(objectType.targetFieldPath)) {
				FieldInfo info = FindFieldInfo(parentField.GetType(), objectType.targetFieldPath);
				if (info != null) {
					if (info.FieldType == typeof(MemberData)) {
						MemberData member = GetFieldValue(parentField, objectType.targetFieldPath) as MemberData;
						if (member != null && member.isAssigned) {
							if (objectType.isElementType && member.targetType == MemberData.TargetType.Type) {
								memberType = member.startType;
							} else {
								memberType = member.type;
							}
						}
						if (memberType != null && memberType == typeof(void) && filter != null && filter.SetMember) {
							return false;
						}
					} else if (info.FieldType == typeof(string)) {
						string val = GetFieldValue(parentField, objectType.targetFieldPath) as string;
						Type tVal = TypeSerializer.Deserialize(val, false);
						if (tVal != null) {
							memberType = tVal;
						}
					} else if (info.FieldType == typeof(MultipurposeMember)) {
						MultipurposeMember member = GetFieldValue(parentField, objectType.targetFieldPath) as MultipurposeMember;
						if (member != null && member.target.isAssigned) {
							if (objectType.isElementType && member.target.targetType == MemberData.TargetType.Type) {
								memberType = member.target.startType;
							} else {
								memberType = member.target.type;
							}
						}
						if (memberType != null && memberType == typeof(void) && filter != null && filter.SetMember) {
							return false;
						}
					}
				}
				if (memberType != null) {
					if (filter == null) {
						filter = new FilterAttribute();
					}
					if (objectType.isElementType) {
						Type t = memberType.ElementType();
						if (t != null) {
							objectType.type = t;
							filter.Types.Add(t);
						} else {
							objectType.type = memberType;
							filter.Types.Add(memberType);
						}
					} else {
						objectType.type = memberType;
						filter.Types.Add(memberType);
					}
				} else {
					return false;
				}
			}
			if (objectType.type != null) {
				if (filter == null) {
					filter = new FilterAttribute();
				}
				if (filter.Types.Count == 0) {
					filter.Types.Add(objectType.type);
				}
				if (attributes != null && attributes.Length > 0) {
					for (int i = 0; i < attributes.Length; i++) {
						if (filter == null && objectType == null)
							break;
						if (filter != null && attributes[i] is FilterAttribute) {
							attributes[i] = filter;
							filter = null;
						}
						if (attributes[i] == objectType) {
							attributes[i] = objectType;
							objectType = null;
						}
					}
					if (filter != null) {
						uNodeUtility.AddArray(ref attributes, filter);
					}
					if (objectType != null) {
						uNodeUtility.AddArray(ref attributes, objectType);
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Convert Delegate type to another
		/// </summary>
		/// <param name="src"></param>
		/// <param name="targetType"></param>
		/// <param name="doTypeCheck"></param>
		/// <returns></returns>
		public static Delegate ConvertDelegate(Delegate src, Type targetType, bool doTypeCheck = true) {
			//Is it null or of the same type as the target?
			if (src == null || src.GetType() == targetType)
				return src;
			//Is it multiple cast?
			return src.GetInvocationList().Count() == 1
				? Delegate.CreateDelegate(targetType, src.Target, src.Method, doTypeCheck)
				: src.GetInvocationList().Aggregate<Delegate, Delegate>
					(null, (current, d) => Delegate.Combine(current, ConvertDelegate(d, targetType, doTypeCheck)));
		}

		/// <summary>
		/// Is the target null or default.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsNullOrDefault(object target, Type type = null) {
			if (target != null) {
				if (target is UnityEngine.Object) {
					return false;
				}
				if (type == null) {
					type = target.GetType();
				}
				ConstructorInfo[] CInfo = type.GetConstructors();
				foreach (ConstructorInfo info in CInfo) {
					if (info.GetParameters().Length == 0) {
						object obj = CreateInstance(type);
						return obj.Equals(target);
					}
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Are the type can be create a new instance.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanCreateInstance(Type type) {
			if (type == typeof(string) || type.IsPrimitive || type.IsValueType && type != typeof(void))
				return true;
			if (type.IsInterface || type.IsAbstract)
				return false;
			if (typeof(UnityEngine.Object).IsAssignableFrom(type)) {
				return false;
			}
			if (type.IsClass) {
				ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
				if (ctor == null) {
					return type.IsArray;
				}
			}
			return true;
		}

		/// <summary>
		/// Create an instance of type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static object CreateInstance(Type type, params object[] args) {
			if (type == typeof(UnityEngine.Object) || type.IsSubclassOf(typeof(UnityEngine.Object))) {
				return null;
			}
			if (type == typeof(string) || type == typeof(object)) {
				return "";
			} else if (type == typeof(bool)) {
				return default(bool);
			} else if (type == typeof(float)) {
				return default(float);
			} else if (type == typeof(int)) {
				return default(int);
			}
			if (type.IsArray) {
				Array array = Array.CreateInstance(type.GetElementType(), args.Length);
				if (args.Length > 0) {
					for (int i = 0; i < args.Length; i++) {
						array.SetValue(args[i], i);
					}
				}
				return array;
			}
			if (type.IsAbstract) {
				return null;
			}
			if (type.IsClass) {
				ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
				if (ctor == null) {
					if (args.Length > 0) {
						return System.Activator.CreateInstance(type, args);
					}
					return null;
				}
			}
			if(type is RuntimeType) {
				if(type is FakeType) {
					return CreateInstance((type as FakeType).target, args);
				}
				return null;
			}
			return Activator.CreateInstance(type, args);
		}

		public static FieldInfo[] GetFieldsFromType(Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) {
			return type.GetFields(flags);
		}

		public static FieldInfo[] GetFields(object obj, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) {
			return obj.GetType().GetFields(flags);
		}

		public static PropertyInfo[] GetProperties(object obj, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) {
			return obj.GetType().GetProperties(flags);
		}

		public static bool IsPublicMember(MemberInfo member) {
			switch(member.MemberType) {
				case MemberTypes.Constructor:
					var ctor = member as ConstructorInfo;
					return ctor.IsPublic;
				case MemberTypes.Event:
					var evt = member as EventInfo;
					return evt.AddMethod != null ? evt.AddMethod.IsPublic : evt.RemoveMethod.IsPublic;
				case MemberTypes.Field:
					var field = member as FieldInfo;
					return field.IsPublic;
				case MemberTypes.Property:
					var prop = member as PropertyInfo;
					return prop.GetMethod != null && prop.GetMethod.IsPublic || prop.SetMethod != null && prop.SetMethod.IsPublic;
				case MemberTypes.Method:
					var method = member as MethodInfo;
					return method.IsPublic;
				case MemberTypes.TypeInfo:
					var type = member as Type;
					return type.IsPublic;
			}
			return false;
		}

		public static bool IsProtectedMember(MemberInfo member) {
			switch(member.MemberType) {
				case MemberTypes.Constructor:
					var ctor = member as ConstructorInfo;
					return ctor.IsFamily;
				case MemberTypes.Event:
					var evt = member as EventInfo;
					return evt.AddMethod != null ? evt.AddMethod.IsFamily : evt.RemoveMethod.IsFamily;
				case MemberTypes.Field:
					var field = member as FieldInfo;
					return field.IsFamily;
				case MemberTypes.Property:
					var prop = member as PropertyInfo;
					return prop.GetMethod != null && prop.GetMethod.IsFamily || prop.SetMethod != null && prop.SetMethod.IsFamily;
				case MemberTypes.Method:
					var method = member as MethodInfo;
					return method.IsFamily /*&& !method.Attributes.HasFlags(MethodAttributes.VtableLayoutMask)*/;
				case MemberTypes.TypeInfo:
					var type = member as Type;
					return type.IsNestedFamily;
			}
			return false;
		}

		public static bool CanSetMember(MemberInfo member) {
			if(!CanSetMemberValue(member)) {
				return false;
			}
			Type memberType = GetMemberType(member);
			if(memberType != typeof(string) &&
				memberType != typeof(int) &&
				memberType != typeof(float) &&
				memberType != typeof(Vector2) &&
				memberType != typeof(Vector3) &&
				memberType != typeof(Color) &&
				(memberType != typeof(bool)) &&
				memberType != typeof(Quaternion) &&
				memberType != typeof(Rect) &&
				!memberType.IsSubclassOf(typeof(UnityEngine.Object))) {
				return memberType.IsEnum;
			}
			return true;
		}

		public static bool CanGetMember(MemberInfo member, FilterAttribute filter = null) {
			if (!CanGetMemberValue(member)) {
				return false;
			}
			if (member.MemberType == MemberTypes.NestedType)
				return true;
			Type memberType = GetMemberType(member);
			if (memberType != typeof(string) && 
				memberType != typeof(int) && 
				memberType != typeof(float) && 
				memberType != typeof(Vector2) && 
				memberType != typeof(Vector3) && 
				memberType != typeof(Color) && 
				memberType != typeof(bool) && 
				memberType != typeof(Quaternion) && 
				memberType != typeof(Rect) && 
				memberType != typeof(void)) {
				return memberType.IsVisible || 
					filter != null && filter.NonPublic && IsProtectedMember(member) ||
					memberType.IsSubclassOf(typeof(UnityEngine.Object));
			}
			return true;
		}

		public static bool CanGetMemberValue(MemberInfo member) {
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Property) {
				return (member as PropertyInfo).CanRead;
			}
			return true;
		}

		public static bool CanSetMemberValue(MemberInfo member) {
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field) {
				return !(member as FieldInfo).IsInitOnly;
			} else if (memberType == MemberTypes.Property) {
				return (member as PropertyInfo).CanWrite;
			} else if (memberType == MemberTypes.Event) {
				return true;
			}
			return false;
		}
		public static bool IsValidParameters(MethodBase method) {
			var parameters = method.GetParameters();
			return IsValidParameters(parameters);
		}

		public static bool IsValidParameters(ParameterInfo[] parameters) {
			for(int i = 0; i < parameters.Length; i++) {
				if(parameters[i].ParameterType.IsPointer) {
					//Invalid when there's a pointer type, since uNode doesn't support it.
					return false;
				}
			}
			return true;
		}

		public static bool IsValidConstructor(ConstructorInfo ctor, int maxCtorParam = 0, int minCtorParam = 0) {
			if (ctor != null) {
				if(minCtorParam != 0 || maxCtorParam < 20) {
					ParameterInfo[] Pinfo = ctor.GetParameters();
					if(Pinfo.Length > maxCtorParam || Pinfo.Length < minCtorParam) {
						return false;
					}
					if(!IsValidParameters(Pinfo)) {
						return false;
					}
				}
				if (!ctor.IsGenericMethod && !ctor.ContainsGenericParameters) {
					return true;
				}
			}
			return false;
		}

		public static bool IsValidMethod(MethodInfo method, int maxMethodParam = 0, int minMethodParam = 0, FilterAttribute filter = null) {
			if (method != null) {
				if (filter.SetMember && method.ReturnType == typeof(void)) {
					return false;
				}
				bool isValid = true;
				if(minMethodParam != 0 || maxMethodParam < 20) {
					ParameterInfo[] Pinfo = method.GetParameters();
					if(Pinfo.Length > maxMethodParam || Pinfo.Length < minMethodParam) {
						isValid = false;
					}
				}
				if (isValid && (filter != null && filter.DisplayGenericType || !method.IsGenericMethod && !method.ContainsGenericParameters)) {
					return true;
				}
			}
			return false;
		}

		public static bool IsValidMethodParameter(MethodInfo method, Type parameterType) {
			if(method != null) {
				var parameters = method.GetParameters();
				if(parameters.Length == 1 && parameters[0].ParameterType == parameterType) {
					return true;
				}
			}
			return false;
		}

		public static bool IsValidMethodParameter(MethodInfo method, params Type[] parameterTypes) {
			if(method != null) {
				var parameters = method.GetParameters();
				if(parameters.Length == parameterTypes.Length) {
					for(int i=0;i<parameters.Length;i++) {
						if(parameters[i].ParameterType != parameterTypes[i]) {
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}

		public static List<MethodInfo> GetValidMethodInfo(Type type, string method,
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static,
			int maxMethodParam = 0, int minMethodParam = 0) {
			List<MethodInfo> validMethods = new List<MethodInfo>();
			List<MethodInfo> mMethods = type.GetMethods(bindingFlags).ToList();

			for (int b = 0; b < mMethods.Count; ++b) {
				MethodInfo mi = mMethods[b];

				string name = mi.Name;
				if (name != method)
					continue;
				if (IsValidMethod(mi, maxMethodParam, minMethodParam)) {
					validMethods.Add(mi);
				}
			}
			return validMethods;
		}

		private static Dictionary<MethodBase, bool> refOrOutMap = null;
		/// <summary>
		/// Are the method has ref/out parameter.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static bool HasRefOrOutParameter(MethodBase method) {
			if(method == null)
				return false;
			if (refOrOutMap == null)
				refOrOutMap = new Dictionary<MethodBase, bool>();
			bool val;
			if (refOrOutMap.TryGetValue(method, out val)) {
				return val;
			} else {
				ParameterInfo[] paramsInfo = method.GetParameters();
				for (int p = 0; p < paramsInfo.Length; p++) {
					if (paramsInfo[p].IsOut || paramsInfo[p].ParameterType.IsByRef) {
						val = true;
						break;
					}
				}
				refOrOutMap[method] = val;
			}
			return val;
		}

		public static MemberInfo[] GetMemberInfo(Type type, string path, BindingFlags bindingAttr, MemberData reflection = null, bool throwOnFail = true) {
			if (object.ReferenceEquals(type, null)) {
				if (!throwOnFail || !Application.isPlaying)
					return null;
				throw new System.Exception("type can't null");
			}
			string[] strArray = path.Split(new char[] { '.' });
			MemberInfo[] infoArray = new MemberInfo[strArray.Length - 1];
			for (int i = 0; i < strArray.Length; i++) {
				if (i == 0)
					continue;
				string mName = strArray[i];
				//Event
				EventInfo eventInfo = type.GetEvent(mName, bindingAttr);
				if (eventInfo != null) {
					infoArray[i - 1] = eventInfo;
					if (i + 1 == strArray.Length)
						break;
					type = eventInfo.EventHandlerType;
					continue;
				}
				//Field
				FieldInfo field = type.GetField(mName, bindingAttr);
				if (field != null) {
					infoArray[i - 1] = field;
					if (i + 1 == strArray.Length)
						break;
					type = field.FieldType;
					continue;
				}
				//Property
				PropertyInfo property;
				try {
					property = GetProperty(type, mName, bindingAttr);
				} catch (AmbiguousMatchException) {
					property = GetProperty(type, mName, bindingAttr | BindingFlags.DeclaredOnly);
				}
				if (property != null) {
					infoArray[i - 1] = property;
					if (i + 1 == strArray.Length)
						break;
					type = property.PropertyType;
					continue;
				}
				//Method
				Type[] paramsType = Type.EmptyTypes;
				Type[] genericType = Type.EmptyTypes;
				if (reflection != null && reflection.SerializedItems?.Length == strArray.Length) {
					if(throwOnFail) {
						if(reflection.ParameterTypes != null)
							paramsType = reflection.ParameterTypes[i] ?? paramsType;
						if(reflection.GenericTypes != null)
							genericType = reflection.GenericTypes[i] ?? genericType;
					} else {
						if(MemberData.Utilities.SafeGetParameterTypes(reflection) != null)
							paramsType = MemberData.Utilities.SafeGetParameterTypes(reflection)[i] ?? paramsType;
						if(MemberData.Utilities.SafeGetGenericTypes(reflection) != null)
							genericType = MemberData.Utilities.SafeGetGenericTypes(reflection)[i] ?? genericType;
					}
					// try {
					// 	// MemberDataUtility.DeserializeMemberItem(reflection.Items[i], reflection.targetReference, out genericType, out paramsType);
					// } catch {
					// 	if (throwOnFail) {
					// 		throw;
					// 	}
					// 	return null;
					// }
				}
				MethodInfo method = null;
				if (genericType.Length > 0) {
					bool flag = false;
					bool flag2 = false;
					var methods = GetMethods(type, bindingAttr);
					MethodInfo backupMethod = null;
					foreach(var m in methods) { 
						method = m;
						if (!method.Name.Equals(mName) || !method.IsGenericMethodDefinition) {
							continue;
						}
						if (method.GetGenericArguments().Length == genericType.Length && method.GetParameters().Length == paramsType.Length) {
							for (int y = 0; y < genericType.Length;y++) {
								if(genericType[y] == null) {
									if (throwOnFail) {
										if (reflection != null && reflection.SerializedItems?.Length == strArray.Length) {
											throw new Exception("Type not found: " + MemberDataUtility.GetGenericName(reflection.Items[i].genericArguments[y], reflection.targetReference));
										}
										throw new Exception("The generic type was not found.");
									}
									return null;
								}
							}
							if (uNodeUtility.isPlaying) {
								method = method.MakeGenericMethod(genericType);
							} else {
								try {
									method = method.MakeGenericMethod(genericType);
								} catch (Exception ex) {
									if (throwOnFail) {
										Debug.LogException(ex);
									}
									return null;
								}
							}
							backupMethod = method;//for alternatife when method not found.
							try {
								ParameterInfo[] parameters = method.GetParameters();
								bool flag3 = false;
								for(int z = 0; z < parameters.Length; z++) {
									if(parameters[z].ParameterType != paramsType[z]) {
										flag3 = true;
										break;
									}
								}
								if(flag3)
									continue;
							} catch {
								if(throwOnFail) {
									throw;
								}
								return null;
							}
							break;
						}
					}
					if (backupMethod != null) {
						infoArray[i - 1] = backupMethod;
						if (reflection != null && HasRefOrOutParameter(backupMethod)) {
							reflection.hasRefOrOut = true;
						}
						if (i + 1 == strArray.Length) {
							flag2 = true;
							break;
						}
						type = backupMethod.ReturnType;
						flag = true;
					}
					if (flag2)
						break;
					if (flag)
						continue;
				}
				try {
					method = GetMethod(type, mName, paramsType, bindingAttr);
				} catch (AmbiguousMatchException) {
					var methods = type.GetMethods();
					for(int x=0;x<methods.Length;x++) {
						var m = methods[x];
						if(m.Name == mName) {
							var param = m.GetParameters();
							if(paramsType.Length == param.Length) {
								bool mflag = true;
								for(int y = 0; y < param.Length; y++) {
									if(param[y].ParameterType != paramsType[y]) {
										mflag = false;
										break;
									}
								}
								if(mflag) {
									method = m;
									break;
								}
							}
						}
					}
				}
				if (method != null) {
					if (method.IsGenericMethodDefinition && genericType.Length > 0) {
						if (uNodeUtility.isPlaying) {
							method = method.MakeGenericMethod(genericType);
						} else {
							try {
								method = method.MakeGenericMethod(genericType);
							} catch { continue; }
						}
					}
					infoArray[i - 1] = method;
					if (reflection != null && HasRefOrOutParameter(method)) {
						reflection.hasRefOrOut = true;
					}
					if (i + 1 == strArray.Length)
						break;
					type = method.ReturnType;
					continue;
				}
				if (path.EndsWith("ctor")) {
					//Constructor
					ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, paramsType, null);
					if (ctor == null && paramsType.Length == 0) {
						return null;
					}
					infoArray[i - 1] = ctor;
					if (reflection != null && HasRefOrOutParameter(ctor)) {
						reflection.hasRefOrOut = true;
					}
					if (i + 1 == strArray.Length)
						break;
					type = GetMemberType(ctor.DeclaringType);
				} else {
					MemberInfo[] member = type.GetMember(mName, bindingAttr);
					if (member != null && member.Length > 0) {
						infoArray[i - 1] = member[0];
						if (i + 1 == strArray.Length)
							break;
						type = GetMemberType(member[0]);
					} else {
						if (throwOnFail) {
							throw new System.Exception("Member not found at path:" + path +
								", maybe you have wrong type, member name changed or wrong target.\ntype:" +
								type.PrettyName(true));
						} else {
							return null;
						}
					}
				}
			}
			return infoArray;
		}

		public static object GetMemberTargetRef(MemberInfo[] memberInfo, object target) {
			if (object.ReferenceEquals(target, null)) {
				return null;
			}
			if (object.ReferenceEquals(memberInfo, null)) {
				throw new System.Exception("memberInfo can't null");
			}
			object lastObject = target;
			for (int i = 0; i < memberInfo.Length; i++) {
				if (i + 1 == memberInfo.Length)
					break;
				MemberInfo member = memberInfo[i];
				if(object.ReferenceEquals(lastObject, null) && member.MemberType != MemberTypes.NestedType) {
					throw new NullReferenceException("Null value obtained when accessing memebers: " + string.Join(".", memberInfo.Take(i + 1).Select(m => m.Name)));
				}
				switch (member.MemberType) {
					case MemberTypes.Field:
						FieldInfo field = member as FieldInfo;
						lastObject = field.GetValueOptimized(lastObject);
						break;
					case MemberTypes.Property:
						PropertyInfo property = member as PropertyInfo;
						lastObject = property.GetValueOptimized(lastObject);
						break;
					case MemberTypes.Method:
						MethodInfo method = member as MethodInfo;
						lastObject = method.InvokeOptimized(lastObject, null);
						break;
					case MemberTypes.NestedType:
						lastObject = null;
						break;
					case MemberTypes.Constructor:
						lastObject = (member as ConstructorInfo).Invoke(lastObject, null);
						break;
					case MemberTypes.Event:
						lastObject = (member as EventInfo).EventHandlerType.GetMethod("Invoke").Invoke(lastObject, null);
						break;
					default:
						throw new Exception();
				}
				if (memberInfo[i].MemberType != MemberTypes.NestedType && object.ReferenceEquals(lastObject, null)) {
					return null;
				}
			}
			return lastObject;
		}

		public static object GetMemberTargetRef(MemberInfo[] memberInfo, object target, ref object parent, object[] invokeParams) {
			if (object.ReferenceEquals(target, null)) {
				throw new ArgumentNullException(nameof(target));
			}
			if (object.ReferenceEquals(memberInfo, null)) {
				throw new ArgumentNullException(nameof(memberInfo));
			}
			object lastObject = target;
			int lastInvokeNum = 0;
			for (int i = 0; i < memberInfo.Length; i++) {
				if (i + 1 == memberInfo.Length)
					break;
				MemberInfo member = memberInfo[i];
				switch (member.MemberType) {
					case MemberTypes.Field:
						FieldInfo field = member as FieldInfo;
						parent = lastObject;
						lastObject = field.GetValueOptimized(lastObject);
						break;
					case MemberTypes.Property:
						PropertyInfo property = member as PropertyInfo;
						parent = lastObject;
						lastObject = property.GetValueOptimized(lastObject);
						break;
					case MemberTypes.Method:
						MethodInfo method = member as MethodInfo;
						parent = lastObject;
						int paramsLength = method.GetParameters().Length;
						if (invokeParams != null && paramsLength == invokeParams.Length) {
							lastObject = method.InvokeOptimized(lastObject, invokeParams);
						} else if (invokeParams != null && paramsLength > 0 && lastInvokeNum + paramsLength <= invokeParams.Length) {
							object[] obj = new object[paramsLength];
							for (int x = lastInvokeNum; x < paramsLength; x++) {
								obj[x - lastInvokeNum] = invokeParams[x];
							}
							lastObject = method.InvokeOptimized(lastObject, obj);
							if (HasRefOrOutParameter(method)) {
								for (int x = lastInvokeNum; x < paramsLength; x++) {
									invokeParams[x] = obj[x - lastInvokeNum];
								}
							}
							lastInvokeNum += paramsLength;
						} else {
							lastObject = method.InvokeOptimized(lastObject, null);
						}
						break;
					case MemberTypes.NestedType:
						lastObject = null;
						break;
					case MemberTypes.Constructor:
						ConstructorInfo ctor = member as ConstructorInfo;
						paramsLength = ctor.GetParameters().Length;
						if (invokeParams != null && paramsLength == invokeParams.Length) {
							lastObject = ctor.Invoke(lastObject, invokeParams);
						} else if (invokeParams != null && paramsLength > 0 && lastInvokeNum + paramsLength <= invokeParams.Length) {
							object[] obj = new object[paramsLength];
							for (int x = lastInvokeNum; x < paramsLength; x++) {
								obj[x - lastInvokeNum] = invokeParams[x];
							}
							lastObject = ctor.Invoke(lastObject, obj);
							if (HasRefOrOutParameter(ctor)) {
								for (int x = lastInvokeNum; x < paramsLength; x++) {
									invokeParams[x] = obj[x - lastInvokeNum];
								}
							}
							lastInvokeNum += paramsLength;
						} else {
							lastObject = ctor.Invoke(lastObject, null);
						}
						break;
					case MemberTypes.Event:
						MethodInfo minfo = (member as EventInfo).EventHandlerType.GetMethod("Invoke");
						parent = lastObject;
						paramsLength = minfo.GetParameters().Length;
						if (invokeParams != null && paramsLength == invokeParams.Length) {
							lastObject = minfo.Invoke(lastObject, invokeParams);
						} else if (invokeParams != null && paramsLength > 0 && lastInvokeNum + paramsLength <= invokeParams.Length) {
							object[] obj = new object[paramsLength];
							for (int x = lastInvokeNum; x < paramsLength; x++) {
								obj[x - lastInvokeNum] = invokeParams[x];
							}
							lastObject = minfo.Invoke(lastObject, obj);
							if (HasRefOrOutParameter(minfo)) {
								for (int x = lastInvokeNum; x < paramsLength; x++) {
									invokeParams[x] = obj[x - lastInvokeNum];
								}
							}
							lastInvokeNum += paramsLength;
						} else {
							lastObject = minfo.Invoke(lastObject, null);
						}
						break;
					default:
						throw new Exception(member.MemberType.ToString());
				}
				if (memberInfo[i].MemberType != MemberTypes.NestedType && object.ReferenceEquals(lastObject, null)) {
					return null;
				}
			}
			return lastObject;
		}

		public static bool GetMemberIsStatic(MemberInfo member) {
			switch (member.MemberType) {
				case MemberTypes.Field:
					return (member as FieldInfo).IsStatic;
				case MemberTypes.Property:
					var prop = member as PropertyInfo;
					if(prop.GetMethod != null) {
						return prop.GetMethod.IsStatic;
					} else {
						return prop.SetMethod != null && prop.SetMethod.IsStatic;
					}
				case MemberTypes.Method:
					return (member as MethodInfo).IsStatic;
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					return true;
				case MemberTypes.Event:
					return (member as EventInfo).GetAddMethod().IsStatic;
				case MemberTypes.Constructor:
					return (member as ConstructorInfo).IsStatic;
			}
			return false;
		}

		public static Type GetMemberType(MemberInfo member) {
			switch (member.MemberType) {
				case MemberTypes.Field:
					return (member as FieldInfo).FieldType;
				case MemberTypes.Property:
					return (member as PropertyInfo).PropertyType;
				case MemberTypes.Method:
					return (member as MethodInfo).ReturnType;
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					return member as Type;
				case MemberTypes.Event:
					Type delegateType = (member as EventInfo).EventHandlerType;
					return delegateType;
				//MethodInfo invoke = delegateType.GetMethod("Invoke");
				//return invoke.ReturnType;
				case MemberTypes.Constructor:
					return (member as ConstructorInfo).DeclaringType;
			}
			throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, NestedType or Event " + " - " + member.MemberType, "member");
		}

		public static void SetBoxedMemberValue(object parent, MemberInfo targetInfo, object target, MemberInfo propertyInfo, object value) {
			SetMemberValue(propertyInfo, target, value);
			SetMemberValue(targetInfo, parent, target);
		}

		public static void SetMemberValue(MemberInfo member, object target, object value) {
			if (!CanSetMemberValue(member))
				return;
			MemberTypes memberType = member.MemberType;
			if (memberType != MemberTypes.Field) {
				if (memberType != MemberTypes.Property) {
					throw new ArgumentException("MemberInfo must be type FieldInfo or PropertyInfo", "member");
				}
			} else {
				(member as FieldInfo).SetValueOptimized(target, value);
				return;
			}
			(member as PropertyInfo).SetValueOptimized(target, value);
		}

		public static MethodInfo[] GetOverloadingMethod(MethodInfo method) {
			MethodInfo[] memberInfos = null;
			if (method != null) {
				if (method.ReflectedType != null) {
					memberInfos = method.ReflectedType.GetMethods();
				} else if (method.DeclaringType != null) {
					memberInfos = method.DeclaringType.GetMethods();
				}
				if (memberInfos != null) {
					memberInfos = memberInfos.Where(m =>
						m.Name.Equals(method.Name)).ToArray();
				}
			}
			return memberInfos;
		}
	}
}