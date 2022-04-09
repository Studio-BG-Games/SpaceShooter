using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames.uNode.Editors {
	public static class EditorReflectionUtility {
		private static Dictionary<Type, FieldInfo[]> fieldsInfoMap = new Dictionary<Type, FieldInfo[]>();
		private static Dictionary<MemberInfo, object[]> attributesMap = new Dictionary<MemberInfo, object[]>();
		private static Dictionary<Assembly, Type[]> assemblyTypeMap = new Dictionary<Assembly, Type[]>();
		private static Dictionary<Assembly, HashSet<string>> assemblyNamespaces = new Dictionary<Assembly, HashSet<string>>();
		private static Dictionary<Assembly, List<MethodInfo>> extensionsMap = new Dictionary<Assembly, List<MethodInfo>>();
		private static Dictionary<Assembly, List<MethodInfo>> operatorsMap = new Dictionary<Assembly, List<MethodInfo>>();
		private static HashSet<string> namespaces;
		//private static Dictionary<Assembly, Dictionary<string, Type[]>> assemblyTypeMap2 = new Dictionary<Assembly, Dictionary<string, Type[]>>();
		private static Assembly[] assemblies;
		//private static object lockObject = new object();
		private static Type[] generalTypes;
		private static readonly HashSet<string> editorAssemblyNames;

		static EditorReflectionUtility() {
			editorAssemblyNames = new HashSet<string> {
				"Assembly-CSharp-Editor",
				"Assembly-UnityScript-Editor",
				"Assembly-Boo-Editor",
				"Assembly-CSharp-Editor-firstpass",
				"Assembly-UnityScript-Editor-firstpass",
				"Assembly-Boo-Editor-firstpass",
				typeof(Editor).Assembly.GetName().Name,
				typeof(uNodeEditor).Assembly.GetName().Name
			};
		}

		public static Type[] GetCommonType() {
			if(generalTypes == null) {
				generalTypes = new Type[] {
					typeof(string),
					typeof(bool),
					typeof(int),
					typeof(float),
					typeof(Vector2),
					typeof(Vector3),
					typeof(Color),
					typeof(Transform),
					typeof(GameObject),
				};
			}
			return generalTypes;
		}

		private static RuntimeType[] _runtimeTypes;
		/// <summary>
		/// Rebuild runtime types.
		/// </summary>
		public static void BuildRuntimeTypes() {
			var prefabs = GraphUtility.FindGraphPrefabs();
			var types = new List<RuntimeType>();
			foreach(var prefab in prefabs) {
				if(prefab == null) continue;
				var graphs = prefab.GetComponents<uNodeRoot>();
				for(int i=0;i< graphs.Length;i++) {
					if(graphs[i] is IMacroGraph || !(graphs[i] is IIndependentGraph))
						continue;
					types.Add(ReflectionUtils.GetRuntimeType(graphs[i]));
				}
			}
			var ifaces = GraphUtility.FindGraphInterfaces();
			foreach(var iface in ifaces) {
				types.Add(ReflectionUtils.GetRuntimeType(iface));
			}
			_runtimeTypes = types.Where(t => t.IsValid()).ToArray();
		}

		/// <summary>
		/// Return true if the type is the editor-only type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsInEditorAssembly(Type type) {
			if(type.IsGenericType) {
				var types = type.GetGenericArguments();
				foreach(var t in types) {
					if(IsInEditorAssembly(t)) {
						return true;
					}
				}
			}
			return IsInEditorAssembly(type.Assembly);
		}

		/// <summary>
		/// Return true if the assembly is the editor-only assembly
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public static bool IsInEditorAssembly(Assembly assembly) {
			return editorAssemblyNames.Contains(assembly.GetName().Name);
		}

		/// <summary>
		/// Get all in runtime type that exist in the editor.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<RuntimeType> GetRuntimeTypes() {
			//if(_runtimeTypes == null) {
			//	BuildRuntimeTypes();
			//}
			BuildRuntimeTypes();
			return _runtimeTypes;
		}

		/// <summary>
		/// Get all available assemblies.
		/// </summary>
		/// <returns></returns>
		public static Assembly[] GetAssemblies() {
			return ReflectionUtils.GetStaticAssemblies();
		}

		public static Type[] GetAssemblyTypes(Assembly assembly) {
			return ReflectionUtils.GetAssemblyTypes(assembly);
		}

		public static HashSet<string> GetAssemblyNamespaces(Assembly assembly) {
			HashSet<string> hash;
			if(!assemblyNamespaces.TryGetValue(assembly, out hash)) {
				Type[] types = GetAssemblyTypes(assembly);
				hash = new HashSet<string>(types.Select(item => item.Namespace).Distinct());
				//for(int i = 0; i < types.Length; i++) {
				//	string ns = types[i].Namespace;

				//}
				assemblyNamespaces[assembly] = hash;
			}
			//Type[] result = new Type[types.Length];
			//Array.Copy(types, result, types.Length);
			return hash;
		}

		public static HashSet<string> GetNamespaces() {
			if(namespaces != null)
				return namespaces;
			namespaces = new HashSet<string>();
			var assemblies = GetAssemblies();
			foreach(var ass in assemblies) {
				var ns = GetAssemblyNamespaces(ass);
				foreach(var n in ns) {
					if(ns == null)
						continue;
					if(!namespaces.Contains(n)) {
						namespaces.Add(n);
					}
				}
			}
			return namespaces;
		}

		class TypeComparer : IEqualityComparer<Type> {
			public bool Equals(Type x, Type y) {
				if(x == null) {
					return y == null;
				} else if(y == null) {
					return x == null;
				}
				return x.GetHashCode() == y.GetHashCode();
			}

			public int GetHashCode(Type obj) {
				return obj.GetHashCode();
			}
		}

		static Dictionary<Type, Dictionary<BindingFlags, MemberInfo[]>> _sortedTypeList;
		public static MemberInfo[] GetSortedMembers(Type type, BindingFlags flags) {
			if(_sortedTypeList == null) {
				_sortedTypeList = new Dictionary<Type, Dictionary<BindingFlags, MemberInfo[]>>(new TypeComparer());
			}
			if(!_sortedTypeList.TryGetValue(type, out var dic)) {
				dic = new Dictionary<BindingFlags, MemberInfo[]>();
				_sortedTypeList[type] = dic;
			}
			if(!dic.TryGetValue(flags, out var map)) {
				map = type.GetMembers(flags);
				Array.Sort(map, (x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
				dic.Add(flags, map);
			}
			return map;
		}

		public static List<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType = null, Func<MemberInfo, bool> validation = null) {
			List<MethodInfo> methods;
			if(!extensionsMap.TryGetValue(assembly, out methods)) {
				methods = new List<MethodInfo>();
				foreach(Type t in GetAssemblyTypes(assembly)) {
					if(t.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)) {
						foreach(MethodInfo mi in t.GetMethods()) {
							if(mi.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)) {
								methods.Add(mi);
							}
						}
					}
				}
				extensionsMap[assembly] = methods;
			}
			if(methods.Count > 0 && (extendedType != null || validation != null)) {
				List<MethodInfo> infos = new List<MethodInfo>();
				Type elementType = extendedType;
				if(extendedType != null) {
					if(extendedType.IsArray || extendedType.IsGenericType) {
						elementType = extendedType.ElementType();
					}
				}
				foreach(MethodInfo mi in methods) {
					try {
						MethodInfo method = mi;
						if(extendedType != null) {
							if(method.IsGenericMethodDefinition) {
								var gp = method.GetGenericArguments();
								if(gp.Length == 1) {
									method = method.MakeGenericMethod(elementType);
								}
							}
							if(!extendedType.IsCastableTo(method.GetParameters()[0].ParameterType)) {
								continue;
							}
						}
						if(validation == null || validation(method)) {
							infos.Add(method);
						}
					}
					catch { };
				}
				return infos;
			}
			return methods;
		}

		public static List<MethodInfo> GetOperators(Assembly assembly, Func<MemberInfo, bool> validation = null) {
			List<MethodInfo> methods;
			if(!operatorsMap.TryGetValue(assembly, out methods)) {
				methods = new List<MethodInfo>();
				foreach(Type t in GetAssemblyTypes(assembly)) {
					if(t.IsValueType || t.IsClass) {
						foreach(MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
							if(mi.Name.StartsWith("op_", StringComparison.Ordinal)) {
								methods.Add(mi);
							}
						}
					}
				}
				operatorsMap[assembly] = methods;
			}
			if(validation == null) {
				return new List<MethodInfo>(methods);
			}
			List<MethodInfo> infos = new List<MethodInfo>();
			foreach(MethodInfo mi in methods) {
				try {
					MethodInfo method = mi;
					if(validation(method)) {
						infos.Add(method);
					}
				}
				catch { };
			}
			return infos;
		}

		//public static Type[] GetAssemblyTypes(Assembly assembly, string ns) {
		//	Dictionary<string, Type[]> map;
		//	if(!assemblyTypeMap2.TryGetValue(assembly, out map)) {
		//		map = new Dictionary<string, Type[]>();
		//		assemblyTypeMap2[assembly] = map;
		//	}
		//	Type[] types;
		//	if(!map.TryGetValue(ns, out types)) {
		//		types = GetAssemblyTypes(assembly);
		//		List<Type> t = new List<Type>();
		//		for(int i = 0; i < types.Length; i++) {
		//			if(types[i].Namespace.Equals(ns)) {
		//				t.Add(types[i]);
		//			}
		//		}
		//		types = t.ToArray();
		//		map[ns] = types;
		//	}
		//	return types;
		//}

		public static List<T> GetListOfType<T>() where T : class {
			List<T> objects = new List<T>();
			foreach(var assembly in GetAssemblies()) {
				foreach(var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && (typeof(T).IsAssignableFrom(t)))) {
					objects.Add((T)Activator.CreateInstance(type));
				}
			}
			return objects;
		}

		public static FieldInfo[] GetFields(Type type) {
			if(type == null)
				return null;
			FieldInfo[] fields;
			if(fieldsInfoMap.TryGetValue(type, out fields)) {
				return fields;
			}
			fields = type.GetFields();
			if(fields.Length > 1) {
				Array.Sort(fields, (x, y) => {
					if(x.DeclaringType != y.DeclaringType) {
						return string.Compare(x.DeclaringType.IsSubclassOf(y.DeclaringType).ToString(), y.DeclaringType.IsSubclassOf(x.DeclaringType).ToString(), StringComparison.OrdinalIgnoreCase);
					}
					return string.Compare(x.MetadataToken.ToString(), y.MetadataToken.ToString(), StringComparison.OrdinalIgnoreCase);
				});
			}
			fieldsInfoMap[type] = fields;
			return fields;
		}

		public static bool HasFieldDependencies(string[] fieldNames, IEnumerable<FieldInfo> fields) {
			foreach(var f in fields) {
				if(f != null) {
					ObjectTypeAttribute objectType = GetAttributes(f).OfType<ObjectTypeAttribute>().FirstOrDefault();
					if(objectType != null && !string.IsNullOrEmpty(objectType.targetFieldPath) && fieldNames.Contains(objectType.targetFieldPath)) {
						return true;
					}
					HideAttribute hideAttribute = GetAttributes(f).OfType<HideAttribute>().FirstOrDefault();
					if(hideAttribute != null && !string.IsNullOrEmpty(hideAttribute.targetField) && fieldNames.Contains(hideAttribute.targetField)) {
						return true;
					}
				}
			}
			return false;
		}

		public static List<FieldInfo> GetFieldDependencies(string[] fieldNames, IEnumerable<FieldInfo> fields) {
			List<FieldInfo> values = new List<FieldInfo>();
			foreach(var f in fields) {
				if(f != null && fieldNames.Contains(f.Name)) {
					values.Add(f);
				}
			}
			return values;
		}

		public static object[] GetAttributes(MemberInfo member) {
			if(member == null)
				return null;
			object[] attributes = null;
			if(attributesMap.TryGetValue(member, out attributes)) {
				return attributes;
			}
			attributes = member.GetCustomAttributes(true);
			attributesMap[member] = attributes;
			return attributes;
		}

		public static MemberInfo[] GetMemberInfo(string path, object start) {
			if(string.IsNullOrEmpty(path)) {
				throw new System.Exception();
			}

			Type type = start.GetType();
			string[] paths = path.Split('.');
			List<string> pathsList = paths.ToList();

			PropertyInfo pinfo = null;
			FieldInfo finfo = null;
			List<MemberInfo> members = new List<MemberInfo>();

			for(int i = 0; i < pathsList.Count; i++) {
				string subpath = pathsList[i];
				int arrayIndex = -1;
				if(i + 2 < pathsList.Count) {
					if(pathsList[i + 1] == "Array") {
						arrayIndex = System.Convert.ToInt32(new string(pathsList[i + 2].Where(c => char.IsDigit(c)).ToArray()));
						pathsList.RemoveAt(i + 2);
						pathsList.RemoveAt(i + 1);
					}
				}
				pinfo = type.GetProperty(subpath);
				if(pinfo == null) {
					finfo = type.GetField(subpath);
					if(finfo == null) {
						return null;
					}
					members.Add(finfo);
				} else {
					members.Add(pinfo);
				}
				if(i < pathsList.Count) {
					Type obj = pinfo == null ? finfo.FieldType : pinfo.PropertyType;
					if(obj != null) {
						if(obj.IsArray && arrayIndex >= 0) {
							type = obj.GetElementType();
						} else if(obj.IsGenericType && arrayIndex >= 0) {
							type = obj.GetGenericArguments()[0];
						} else {
							type = obj;
						}
					}
				}
			}
			return members.ToArray();
		}

		public static bool GetPropertyOrField(string path, object start, out PropertyInfo pinfo, out FieldInfo finfo, out object source) {
			if(string.IsNullOrEmpty(path)) {
				throw new System.Exception();
			}

			Type type = start.GetType();
			string[] paths = path.Split('.');
			List<string> pathsList = paths.ToList();

			source = start;
			pinfo = null;
			finfo = null;

			for(int i = 0; i < pathsList.Count; i++) {
				string subpath = pathsList[i];
				int arrayIndex = -1;
				if(i + 2 < pathsList.Count) {
					if(pathsList[i + 1] == "Array") {
						arrayIndex = System.Convert.ToInt32(new string(pathsList[i + 2].Where(c => char.IsDigit(c)).ToArray()));
						pathsList.RemoveAt(i + 2);
						pathsList.RemoveAt(i + 1);
					}
				}
				pinfo = type.GetProperty(subpath);
				if(pinfo == null) {
					finfo = type.GetField(subpath);
					if(finfo == null)
						return false;
				}
				if(i < pathsList.Count) {
					object obj = pinfo == null ? finfo.GetValue(source) : pinfo.GetValue(source);
					if(obj != null) {
						if(obj.GetType().IsArray && arrayIndex >= 0) {
							object[] array = (object[])(pinfo == null ? finfo.GetValue(source) : pinfo.GetValue(source));
							if(array != null && array.Length != 0 && array.Length > arrayIndex) {
								source = array[arrayIndex];
								type = source.GetType();
							}
						} else if(obj.GetType().IsGenericType && arrayIndex >= 0) {
							var array = (IEnumerable)(pinfo == null ? finfo.GetValue(source) : pinfo.GetValue(source));
							array.Cast<object>().ToList();
							List<object> objList = array.Cast<object>().ToList();
							if(objList != null && objList.Count >= arrayIndex) {
								source = objList[arrayIndex];
								type = source.GetType();
							}
						} else {
							source = obj;
							type = source.GetType();
						}
					}
				}
			}
			return true;
		}

		public static bool GetPropertyOrField(string path, object start, out PropertyInfo pinfo, out FieldInfo finfo, out object[] result) {
			if(string.IsNullOrEmpty(path)) {
				throw new System.Exception();
			}

			Type type = start.GetType();
			string[] paths = path.Split('.');
			List<string> pathsList = paths.ToList();

			object source = start;
			List<object> results = new List<object>();
			result = null;
			pinfo = null;
			finfo = null;

			for(int i = 0; i < pathsList.Count; i++) {
				string subpath = pathsList[i];
				int arrayIndex = -1;
				if(i + 2 < pathsList.Count) {
					if(pathsList[i + 1] == "Array") {
						arrayIndex = System.Convert.ToInt32(new string(pathsList[i + 2].Where(c => char.IsDigit(c)).ToArray()));
						pathsList.RemoveAt(i + 2);
						pathsList.RemoveAt(i + 1);
					}
				}
				pinfo = type.GetProperty(subpath);
				if(pinfo == null) {
					finfo = type.GetField(subpath);
					if(finfo == null)
						return false;
				}
				if(i < pathsList.Count) {
					object obj = pinfo == null ? finfo.GetValue(source) : pinfo.GetValue(source);
					if(obj != null) {
						results.Add(obj);
						if(obj.GetType().IsArray && arrayIndex >= 0) {
							object[] array = (object[])(pinfo == null ? finfo.GetValue(source) : pinfo.GetValue(source));
							if(array != null && array.Length != 0 && array.Length > arrayIndex) {
								source = array[arrayIndex];
								type = source.GetType();
								if(i + 1 < pathsList.Count) {
									results.Add(source);
								}
							} else {
								source = obj;
								type = source.GetType();
							}
						} else if(obj.GetType().IsGenericType && arrayIndex >= 0) {
							var array = (IEnumerable)(pinfo == null ? finfo.GetValue(source) : pinfo.GetValue(source));
							array.Cast<object>().ToList();
							List<object> objList = array.Cast<object>().ToList();
							if(objList != null && objList.Count > arrayIndex) {
								source = objList[arrayIndex];
								type = source.GetType();
								if(i + 1 < pathsList.Count) {
									results.Add(source);
								}
							} else {
								source = obj;
								type = source.GetType();
							}
						} else {
							source = obj;
							type = source.GetType();
						}
					}
				}
			}
			result = results.ToArray();
			return true;
		}

		public static PropertyInfo GetProperty(string path, object start) {
			object source = null;
			PropertyInfo pinfo = null;
			FieldInfo finfo = null;
			if(GetPropertyOrField(path, start, out pinfo, out finfo, out source)) {
				return pinfo;
			}
			return null;
		}

		public static FieldInfo GetField(string path, object start) {
			object source = null;
			PropertyInfo pinfo = null;
			FieldInfo finfo = null;
			if(GetPropertyOrField(path, start, out pinfo, out finfo, out source)) {
				return finfo;
			}
			return null;
		}

		public static object GetMemberValue(string path, object start) {
			object source = null;
			PropertyInfo pinfo = null;
			FieldInfo finfo = null;
			if(GetPropertyOrField(path, start, out pinfo, out finfo, out source)) {
				return source;
			}
			return null;
		}

		public static object[] GetMemberValues(string path, object start) {
			object[] source = null;
			PropertyInfo pinfo = null;
			FieldInfo finfo = null;
			if(GetPropertyOrField(path, start, out pinfo, out finfo, out source)) {
				return source;
			}
			return null;
		}

		public static void RenderVariable(MemberData variable, GUIContent label, UnityEngine.Object unityObject, FilterAttribute filter = null, Action<MemberData> onChange = null) {
			Rect rect = uNodeGUIUtility.GetRect();
			RenderVariable(rect, variable, label, unityObject, filter, onChange);
		}

		public static void RenderVariable(MemberData variable, GUIContent label, UnityEngine.Object unityObject, FilterAttribute filter, float height, Action<MemberData> onChange = null) {
			Rect rect = uNodeGUIUtility.GetRectCustomHeight(height);
			RenderVariable(rect, variable, label, unityObject, filter, onChange);
		}

		public static void RenderVariable(Rect position, MemberData variable, GUIContent label, UnityEngine.Object unityObject, FilterAttribute filter = null, Action<MemberData> onChange = null) {
			if(variable == null)
				return;
			if(filter == null) {
				filter = new FilterAttribute();
			}
			position = EditorGUI.PrefixLabel(position, label);
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if(filter.UnityReference &&
				!filter.OnlyGetType &&
				!variable.isStatic &&
				!variable.targetType.HasFlags(MemberData.TargetType.Values |
					MemberData.TargetType.Type |
					MemberData.TargetType.Null |
					MemberData.TargetType.uNodeGenericParameter)) {
				Rect rect = position;
				DrawInstanceValue(ref rect, GUIContent.none, variable, unityObject, onChange);
				ShowGUI(rect, variable, filter, unityObject, onChange);
			} else {
				if(variable.instance != null && variable.targetType != MemberData.TargetType.uNodeGenericParameter) {
					variable.instance = null;
				}
				ShowGUI(position, variable, filter, unityObject, onChange);
			}

			EditorGUI.indentLevel = oldIndent;
		}

		private static void DrawInstanceValue(ref Rect position, GUIContent label, MemberData variable, UnityEngine.Object unityObject = null, Action<MemberData> onChange = null) {
			if(variable.IsTargetingPortOrNode)
				return;
			object target = variable.instance;
			if(target != null) {
				if(target is MemberData) {
					var targetType = (target as MemberData).targetType;
					if(targetType != MemberData.TargetType.Values &&
						targetType != MemberData.TargetType.None &&
						targetType != MemberData.TargetType.SelfTarget &&
						targetType != MemberData.TargetType.Type) {
						position.width = position.width / 2;
						EditorGUI.HelpBox(position, uNodeUtility.GetDisplayName(target), MessageType.None);
						position.x += position.width;
						return;
					} else {
						target = (target as MemberData).Get();
					}
				} else if(target is BaseValueData) {
					target = (target as BaseValueData).Get();
				}
				if(target is uNodeRoot && unityObject is INode<uNodeRoot> comp && comp.GetOwner() == target as UnityEngine.Object) {
					return;
				}
			}
			Type t = variable.IsTargetingUNode ? typeof(uNodeRoot) : variable.startType;
			if(t == null && target != null) {
				t = target.GetType();
			}
			bool flag = (target == null || !target.GetType().IsCastableTo(t)) &&
				!variable.isStatic && variable.targetType != MemberData.TargetType.None;
			if(flag && target != null && variable.IsTargetingUNode)
				flag = false;
			if(flag)
				GUI.backgroundColor = Color.red;
			position.width = position.width / 2;
			if(t != null && !t.IsCastableTo(typeof(UnityEngine.Object)) || target is MemberData) {
				if(target == null && ReflectionUtils.CanCreateInstance(t)) {
					target = ReflectionUtils.CreateInstance(t) ?? MemberData.CreateFromValue(null, t);
					variable.instance = target;
					if(onChange != null) {
						onChange(variable);
					}
					uNodeGUIUtility.GUIChanged(unityObject);
				}
				uNodeGUIUtility.EditValue(position, label, target, t, delegate (object val) {
					target = val;
					variable.instance = target;
					if(onChange != null) {
						onChange(variable);
					}
				}, new uNodeUtility.EditValueSettings() {
					acceptUnityObject = true,
					nullable = true,
					unityObject = unityObject,
				});
			} else {
				if(position.Contains(Event.current.mousePosition) && Event.current.button == 1 && Event.current.type == EventType.MouseUp) {
					GUI.changed = false;
					GenericMenu menu = new GenericMenu();
					if(target != null) {
						bool isGo = true;
						GameObject go = null;
						Component comp = null;
						if(target is GameObject) {
							go = target as GameObject;
						}
						if(target is Component || target.GetType().IsSubclassOf(typeof(Component))) {
							comp = target as Component;
							isGo = false;
						}
						if(go != null || comp != null) {
							if(isGo) {
								Component[] comps = go.GetComponents<Component>();
								menu.AddItem(new GUIContent("0-" + go.GetType().Name), true, delegate (object obj) {
									variable.instance = obj;
									if(onChange != null) {
										onChange(variable);
									}
									uNodeGUIUtility.GUIChanged(unityObject);
								}, go);
								int index = 1;
								foreach(Component com in comps) {
									menu.AddItem(new GUIContent(index + "-" + com.GetType().Name), false, delegate (object obj) {
										variable.instance = obj;
										if(onChange != null) {
											onChange(variable);
										}
										uNodeGUIUtility.GUIChanged(unityObject);
									}, com);
									index++;
								}
							} else {
								GameObject g = comp.gameObject;
								menu.AddItem(new GUIContent("0-" + g.GetType().Name), false, delegate (object obj) {
									variable.instance = obj;
									if(onChange != null) {
										onChange(variable);
									}
									uNodeGUIUtility.GUIChanged(unityObject);
								}, g);
								Component[] comps = comp.GetComponents<Component>();
								int index = 1;
								foreach(Component com in comps) {
									menu.AddItem(new GUIContent(index + "-" + com.GetType().Name), com.Equals(target), delegate (object obj) {
										variable.instance = obj;
										if(onChange != null) {
											onChange(variable);
										}
										uNodeGUIUtility.GUIChanged(unityObject);
									}, com);
									index++;
								}
							}
						}
					}
					if(unityObject is uNodeRoot || unityObject is NodeComponent) {
						if(target != null)
							menu.AddSeparator("");
						if(unityObject is uNodeRoot) {
							GameObject go = (unityObject as uNodeRoot).gameObject;
							menu.AddItem(new GUIContent("this uNode/0-" + go.GetType().Name), go.Equals(target), delegate (object obj) {
								variable.instance = obj;
								if(onChange != null) {
									onChange(variable);
								}
								uNodeGUIUtility.GUIChanged(unityObject);
							}, go);
							Component[] comps = go.GetComponents<Component>();
							int index = 1;
							foreach(Component com in comps) {
								menu.AddItem(new GUIContent("this uNode/" + index + "-" + com.GetType().Name), com.Equals(target), delegate (object obj) {
									variable.instance = obj;
									if(onChange != null) {
										onChange(variable);
									}
									uNodeGUIUtility.GUIChanged(unityObject);
								}, com);
								index++;
							}
						} else if(unityObject is NodeComponent) {
							uNodeRoot UNR = (unityObject as NodeComponent).owner;
							if(UNR != null) {
								GameObject go = UNR.gameObject;
								menu.AddItem(new GUIContent("this uNode/0-" + go.GetType().Name), go.Equals(target), delegate (object obj) {
									variable.instance = obj;
									if(onChange != null) {
										onChange(variable);
									}
									uNodeGUIUtility.GUIChanged(unityObject);
								}, go);
								Component[] comps = go.GetComponents<Component>();
								int index = 1;
								foreach(Component com in comps) {
									menu.AddItem(new GUIContent("this uNode/" + index + "-" + com.GetType().Name), com.Equals(target), delegate (object obj) {
										variable.instance = obj;
										if(onChange != null) {
											onChange(variable);
										}
										uNodeGUIUtility.GUIChanged(unityObject);
									}, com);
									index++;
								}
							}
						}
					} else if(unityObject.GetType().GetInterface("INodeSystem`1") != null) {
						Type targetType = unityObject.GetType();
						Type interfaceType = targetType.GetInterface("INodeSystem`1");
						Type rootType = interfaceType.GetGenericArguments()[0];
						MethodInfo mInfo = targetType.GetMethod("GetOwner");
						if(mInfo != null && mInfo.ReturnType == rootType) {
							var tObj = mInfo.Invoke(unityObject, null) as MonoBehaviour;
							if(tObj != null) {
								GameObject g = tObj.gameObject;
								menu.AddItem(new GUIContent("0-" + g.GetType().Name), false, delegate (object obj) {
									variable.instance = obj;
									if(onChange != null) {
										onChange(variable);
									}
									uNodeGUIUtility.GUIChanged(unityObject);
								}, g);
								Component[] comps = tObj.GetComponents<Component>();
								int index = 1;
								foreach(Component com in comps) {
									menu.AddItem(new GUIContent(index + "-" + com.GetType().Name), com.Equals(target), delegate (object obj) {
										variable.instance = obj;
										if(onChange != null) {
											onChange(variable);
										}
										uNodeGUIUtility.GUIChanged(unityObject);
									}, com);
									index++;
								}
							}
						}
					}
					menu.ShowAsContext();
				}
				EditorGUI.BeginChangeCheck();
				var newVar = target as UnityEngine.Object;
				newVar = EditorGUI.ObjectField(position, newVar, typeof(UnityEngine.Object), uNodeEditorUtility.IsSceneObject(unityObject));
				if(EditorGUI.EndChangeCheck()) {
					if(unityObject)
						uNodeEditorUtility.RegisterUndo(unityObject, label.text);
					variable.instance = newVar;
					if(onChange != null) {
						onChange(variable);
					}
					uNodeGUIUtility.GUIChanged(unityObject);
				}
			}
			position.x += position.width;
			if(flag)
				GUI.backgroundColor = Color.white;
		}

		private static Dictionary<MemberData, object> _memberValueMap = new Dictionary<MemberData, object>();

		public static void DrawVariableValues(Rect position, MemberData member, Type type, FilterAttribute filter,
			UnityEngine.Object unityObject = null, Action<MemberData> onChange = null) {
			Type t = type;
			if(t == null) {
				t = member.type;
			}
			if(t != null) {
				if(filter == null)
					filter = new FilterAttribute();
				bool flag = type != null && !filter.SetMember && filter.IsValidTarget(MemberData.TargetType.Values) &&
					(filter.IsValidTarget(MemberData.TargetType.Constructor | MemberData.TargetType.Event | MemberData.TargetType.Field | MemberData.TargetType.Method | MemberData.TargetType.Type | MemberData.TargetType.Property) || !filter.IsValueTypes());
				if(flag)
					position.width -= 16;
				if(type != null && member.targetType != MemberData.TargetType.Values) {
					DrawVariableReference(position, member, filter, unityObject, onChange);
					if(filter.ValidTargetType == MemberData.TargetType.Values && !filter.InvalidTargetType.HasFlags(MemberData.TargetType.Values) && filter.IsValueTypes()) {
						member.targetType = MemberData.TargetType.Values;
						member.type = t;
						if(onChange != null) {
							onChange(member);
						}
						uNodeGUIUtility.GUIChanged(unityObject);
					}
				} else {
					object obj;
					if(ReflectionUtils.CanCreateInstance(t) || t.IsCastableTo(typeof(UnityEngine.Object))) {
						if(!member.TargetSerializedType.isFilled) {
							member.type = t;
						}
						if(!_memberValueMap.TryGetValue(member, out obj)) {
							obj = member.Get();
							_memberValueMap[member] = obj;
						}
						if(obj != null && t.IsCastableTo(typeof(UnityEngine.Object)) && !obj.GetType().IsCastableTo(typeof(UnityEngine.Object))) {
							obj = null;
							member.CopyFrom(MemberData.CreateFromValue(obj));
							if(onChange != null) {
								onChange(member);
							}
							if(_memberValueMap.ContainsKey(member)) {
								_memberValueMap.Remove(member);
							}
							uNodeGUIUtility.GUIChanged(unityObject);
						}
						if(t.IsValueType && obj == null || !t.IsCastableTo(typeof(UnityEngine.Object)) && obj != null && !obj.GetType().IsCastableTo(t)) {
							obj = ReflectionUtils.CreateInstance(t);
							member.CopyFrom(MemberData.CreateFromValue(obj));
							if(onChange != null) {
								onChange(member);
							}
							if(_memberValueMap.ContainsKey(member)) {
								_memberValueMap.Remove(member);
							}
							uNodeGUIUtility.GUIChanged(unityObject);
						}
						uNodeGUIUtility.EditValue(position, GUIContent.none, obj, t, delegate (object val) {
							member.CopyFrom(MemberData.CreateFromValue(val));
							if(onChange != null) {
								onChange(member);
							}
							if(_memberValueMap.ContainsKey(member)) {
								_memberValueMap.Remove(member);
							}
						}, new uNodeUtility.EditValueSettings() {
							acceptUnityObject = true,
							nullable = filter.IsValidTarget(MemberData.TargetType.Null),
							unityObject = unityObject,
						});
					} else {
						uNodeEditorGUI.Label(position, "null", (GUIStyle)"HelpBox");
					}
				}
				if(flag) {
					position.x += position.width;
					position.width = 16;
					bool check = EditorGUI.Toggle(position, member.targetType != MemberData.TargetType.Values, EditorStyles.radioButton);
					if(check && member.targetType == MemberData.TargetType.Values) {
						//string tName = variable.targetTypeName;
						if(unityObject)
							uNodeEditorUtility.RegisterUndo(unityObject, "");
						//variable.Reset();
						//variable.targetTypeName = tName;
						member.targetType = t.IsValueType ? MemberData.TargetType.None : MemberData.TargetType.Null;
						member.ResetCache();
						if(onChange != null) {
							onChange(member);
						}
						uNodeGUIUtility.GUIChanged(unityObject);
					} else if(!check && member.targetType != MemberData.TargetType.Values) {
						if(unityObject)
							uNodeEditorUtility.RegisterUndo(unityObject, "");
						member.targetType = MemberData.TargetType.Values;
						member.type = t;
						member.ResetCache();
						if(onChange != null) {
							onChange(member);
						}
						uNodeGUIUtility.GUIChanged(unityObject);
					}
				}
			}
		}

		public static void DrawMemberValues(GUIContent label, MemberData member, Type type, FilterAttribute filter,
			UnityEngine.Object unityObject = null, Action<MemberData> onChange = null) {
			Type t = type;
			if(t == null) {
				t = member.type;
			}
			if(t != null) {
				if(filter == null)
					filter = new FilterAttribute();
				object obj;
				if(ReflectionUtils.CanCreateInstance(t) || t.IsCastableTo(typeof(UnityEngine.Object))) {
					if(!member.TargetSerializedType.isFilled) {
						member.type = t;
					}
					if(!_memberValueMap.TryGetValue(member, out obj)) {
						obj = member.Get();
						_memberValueMap[member] = obj;
					}
					if(obj != null && t.IsCastableTo(typeof(UnityEngine.Object)) && !obj.GetType().IsCastableTo(typeof(UnityEngine.Object))) {
						obj = null;
						member.CopyFrom(MemberData.CreateFromValue(obj));
						if(onChange != null) {
							onChange(member);
						}
						if(_memberValueMap.ContainsKey(member)) {
							_memberValueMap.Remove(member);
						}
						uNodeGUIUtility.GUIChanged(unityObject);
					}
					if(t.IsValueType && obj == null || !t.IsCastableTo(typeof(UnityEngine.Object)) &&
						obj != null && !obj.GetType().IsCastableTo(t)) {
						obj = ReflectionUtils.CreateInstance(t);
						member.CopyFrom(MemberData.CreateFromValue(obj));
						if(onChange != null) {
							onChange(member);
						}
						if(_memberValueMap.ContainsKey(member)) {
							_memberValueMap.Remove(member);
						}
						uNodeGUIUtility.GUIChanged(unityObject);
					}
					uNodeGUIUtility.EditValueLayouted(label, obj, t, delegate (object val) {
						obj = val;
						member.CopyFrom(MemberData.CreateFromValue(obj));
						if(onChange != null) {
							onChange(member);
						}
						if(_memberValueMap.ContainsKey(member)) {
							_memberValueMap.Remove(member);
						}
						uNodeGUIUtility.GUIChanged(unityObject);
					}, new uNodeUtility.EditValueSettings() { 
						acceptUnityObject = true, 
						unityObject = unityObject,
						nullable = filter.IsValidTarget(MemberData.TargetType.Null) });
				} else {
					uNodeEditorGUI.Label(uNodeGUIUtility.GetRect(), "null", (GUIStyle)"HelpBox");
				}
			}
		}

		public static void DrawVariableReference(Rect position, MemberData variable, FilterAttribute filter = null, UnityEngine.Object unityObject = null, Action<MemberData> onChange = null) {
			DrawVariableReference(position, new GUIContent(variable.DisplayName(), variable.Tooltip), variable, filter, unityObject, onChange);
		}

		public static void DrawVariableReference(Rect position, GUIContent label, MemberData variable, FilterAttribute filter = null, UnityEngine.Object unityObject = null, Action<MemberData> onChange = null) {
			if(filter == null) {
				filter = new FilterAttribute();
			}
			bool enabled = filter.DisplayDefaultStaticType;
			if(!enabled)
				EditorGUI.BeginDisabledGroup(true);
			if(EditorGUI.DropdownButton(position, label, FocusType.Keyboard)) {
				if(Event.current.button == 0) {
					GUI.changed = false;
					if(filter.OnlyGetType && filter.CanManipulateArray()) {
						TypeSelectorWindow.ShowWindow(position, filter, delegate (MemberData[] types) {
							uNodeEditorUtility.RegisterUndo(unityObject);
							variable.CopyFrom(types[0]);
							if(onChange != null) {
								onChange(types[0]);
							}
							uNodeGUIUtility.GUIChanged(unityObject);
						}, new TypeItem[1] { variable }).targetObject = unityObject;
					} else {
						ItemSelector.ShowWindow(unityObject, variable, filter, (m) => {
							m.ResetCache();
							if(onChange != null) {
								onChange(m);
							}
							uNodeGUIUtility.GUIChanged(unityObject);
						}).ChangePosition(position.ToScreenRect());
					}
				} else if(Event.current.button == 1 && (variable.targetType == MemberData.TargetType.Method || variable.targetType == MemberData.TargetType.Constructor)) {
					if(filter.ValidMemberType.HasFlags(MemberTypes.Constructor | MemberTypes.Method)) {
						var mPos = Event.current.mousePosition;
						var members = variable.GetMembers(false);
						if(members != null && members.Length == 1) {
							var member = members[members.Length - 1];
							if(variable.targetType == MemberData.TargetType.Method) {
								BindingFlags flag = BindingFlags.Public;
								if(variable.isStatic) {
									flag |= BindingFlags.Static;
								} else {
									flag |= BindingFlags.Instance;
								}
								var memberName = member.Name;
								var mets = member.ReflectedType.GetMember(memberName, flag);
								List<MethodInfo> methods = new List<MethodInfo>();
								foreach(var m in mets) {
									if(m is MethodInfo) {
										methods.Add(m as MethodInfo);
									}
								}
								GenericMenu menu = new GenericMenu();
								foreach(var m in methods) {
									menu.AddItem(new GUIContent("Change Methods/" + GetOverloadingMethodNames(m)), member == m, delegate (object obj) {
										object[] objs = obj as object[];
										MemberData mem = objs[0] as MemberData;
										MethodInfo method = objs[1] as MethodInfo;
										UnityEngine.Object UO = objs[2] as UnityEngine.Object;
										if(member != m) {
											if(method.IsGenericMethodDefinition) {
												TypeSelectorWindow.ShowAsNew(mPos, new FilterAttribute() { UnityReference = false },
												delegate (MemberData[] types) {
													uNodeEditorUtility.RegisterUndo(UO);
													method = method.MakeGenericMethod(types.Select(i => i.Get<Type>()).ToArray());
													MemberData d = new MemberData(method);
													mem.CopyFrom(d);
													mem.instance = null;
												}, new TypeItem[method.GetGenericArguments().Length]).targetObject = UO;
											} else {
												uNodeEditorUtility.RegisterUndo(UO);
												MemberData d = new MemberData(method);
												mem.CopyFrom(d);
												mem.instance = null;
											}
										}
									}, new object[] { variable, m, unityObject });
								}
								menu.ShowAsContext();
							} else if(variable.targetType == MemberData.TargetType.Constructor) {
								GenericMenu menu = new GenericMenu();
								BindingFlags flag = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
								var ctors = member.ReflectedType.GetConstructors(flag);
								foreach(var m in ctors) {
									menu.AddItem(new GUIContent("Change Constructors/" + GetOverloadingConstructorNames(m)), member == m, delegate (object obj) {
										object[] objs = obj as object[];
										MemberData mem = objs[0] as MemberData;
										ConstructorInfo ctor = objs[1] as ConstructorInfo;
										UnityEngine.Object UO = objs[2] as UnityEngine.Object;
										if(member != m) {
											MemberData d = new MemberData(ctor);
											mem.CopyFrom(d);
											mem.instance = null;
										}
									}, new object[] { variable, m, unityObject });
								}
							}
						}
					}
				}
			}

			if(!enabled)
				EditorGUI.EndDisabledGroup();
		}

		public static void ShowGUI(Rect position, MemberData variable, FilterAttribute filter = null, UnityEngine.Object unityObject = null, Action<MemberData> onChange = null) {
			if(filter == null) {
				filter = new FilterAttribute();
			}
			Type t = filter.Types.Count == 1 && (variable.type == null || !variable.type.IsCastableTo(filter.Types[0])) ? filter.Types[0] : variable.type;
			if(!filter.OnlyGetType &&
				filter.IsValidTarget(MemberData.TargetType.Values) &&
				!filter.SetMember &&
				(variable.targetType == MemberData.TargetType.Values || t != null && variable.targetType != MemberData.TargetType.Type)) {
				DrawVariableValues(position, variable, t, filter, unityObject, onChange);
			} else {
				DrawVariableReference(position, variable, filter, unityObject, onChange);
			}
		}

		#region Member
		public static bool ValidateMember(MemberInfo member, FilterAttribute filter) {
			if(!uNodePreference.GetPreference().showObsoleteItem && member.IsDefinedAttribute(typeof(ObsoleteAttribute))) {
				return false;
			}
			if(filter != null) {
				bool valid = filter.IsValidType(ReflectionUtils.GetMemberType(member));
				if(!valid) {
					return false;
				}
				bool flag = filter.ValidTargetType != MemberData.TargetType.None;
				bool flag2 = filter.InvalidTargetType != MemberData.TargetType.None;
				if((flag || flag2) && !filter.ValidTargetType.HasFlags(MemberData.TargetType.ValueNode)) {
					switch(member.MemberType) {
						case MemberTypes.Field:
							if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Field)) {
								return false;
							}
							if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Field)) {
								return false;
							}
							break;
						case MemberTypes.Property:
							if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Property)) {
								return false;
							}
							if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Property)) {
								return false;
							}
							break;
						case MemberTypes.Method:
							if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Method)) {
								return false;
							}
							if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Method)) {
								return false;
							}
							break;
						case MemberTypes.Event:
							if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Event)) {
								return false;
							}
							if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Event)) {
								return false;
							}
							break;
						case MemberTypes.Constructor:
							if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Constructor)) {
								return false;
							}
							if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Constructor)) {
								return false;
							}
							break;
						case MemberTypes.NestedType:
							if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Type)) {
								return false;
							}
							if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Type)) {
								return false;
							}
							break;
					}
				}
				if(filter.NonPublic && !filter.Private && !ReflectionUtils.IsPublicMember(member) && !ReflectionUtils.IsProtectedMember(member)) {
					return false;
				}
				if(filter.SetMember && !ReflectionUtils.CanSetMember(member)) {
					return false;
				}
			}
			switch(member.MemberType) {
				case MemberTypes.Method: {
					MethodInfo info = member as MethodInfo;
					int minMethodParam = 0;
					int maxMethodParam = 0;
					if(filter != null) {
						minMethodParam = filter.MinMethodParam;
						maxMethodParam = filter.MaxMethodParam;
					}
					if(!ReflectionUtils.IsValidMethod(info, maxMethodParam, minMethodParam, filter)) {
						return false;
					}
					if(member.Name.StartsWith("get_", StringComparison.Ordinal) || 
						member.Name.StartsWith("set_", StringComparison.Ordinal) || 
						member.Name.StartsWith("add_", StringComparison.Ordinal) || 
						member.Name.StartsWith("remove_", StringComparison.Ordinal)) {
						if(!member.Name.StartsWith("get_Item", StringComparison.Ordinal) && !member.Name.StartsWith("set_Item", StringComparison.Ordinal)) {
							return false;
						}
					} else if(member.Name.StartsWith("op_", StringComparison.Ordinal)) {
						switch(member.Name) {
							case "op_Addition":
							case "op_Subtraction":
							case "op_Multiply":
							case "op_Division":
								break;
							default:
								return false;
						}
					}
				}
				break;
				case MemberTypes.Property: {
					PropertyInfo info = member as PropertyInfo;
					if(info != null) {
						ParameterInfo[] pInfo = info.GetIndexParameters();
						if(pInfo != null && pInfo.Length > 0) {
							return false;
						}
					}
				}
				break;
				case MemberTypes.Custom:
				case MemberTypes.TypeInfo:
					return false;
				case MemberTypes.NestedType:
					if(!filter.Static || !filter.NestedType) {
						return false;
					}
					break;
			}
			if(!filter.Boxing) {
				return !member.DeclaringType.IsValueType;
			}
			return filter.ValidMemberType.HasFlags(member.MemberType);
		}

		public static bool ValidateNextMember(MemberInfo member, FilterAttribute filter) {
			if(!uNodePreference.GetPreference().showObsoleteItem && member.IsDefinedAttribute(typeof(ObsoleteAttribute))) {
				return false;
			}
			bool flag = filter.ValidTargetType != MemberData.TargetType.None;
			bool flag2 = filter.InvalidTargetType != MemberData.TargetType.None;
			if(flag || flag2) {
				switch(member.MemberType) {
					case MemberTypes.Field:
						if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Field)) {
							return false;
						}
						if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Field)) {
							return false;
						}
						break;
					case MemberTypes.Property:
						if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Property)) {
							return false;
						}
						if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Property)) {
							return false;
						}
						break;
					case MemberTypes.Method:
						if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Method)) {
							return false;
						}
						if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Method)) {
							return false;
						}
						break;
					case MemberTypes.NestedType:
						if(flag2 && filter.InvalidTargetType.HasFlags(MemberData.TargetType.Type)) {
							return false;
						}
						if(flag && !filter.ValidTargetType.HasFlags(MemberData.TargetType.Type)) {
							return false;
						}
						break;
					default:
						return false;
				}
			}
			if(filter.NonPublic && !filter.Private && !ReflectionUtils.IsPublicMember(member) && !ReflectionUtils.IsProtectedMember(member)) {
				return false;
			}
			switch(member.MemberType) {
				case MemberTypes.Method: {
					MethodInfo info = member as MethodInfo;
					int minMethodParam = 0;
					int maxMethodParam = 0;
					if(filter != null) {
						minMethodParam = filter.MinMethodParam;
						maxMethodParam = filter.MaxMethodParam;
					}
					if(!ReflectionUtils.IsValidMethod(info, maxMethodParam, minMethodParam, filter)) {
						return false;
					}
					if(((MethodInfo)member).ReturnType == typeof(void)) {
						return false;
					}
					if((member.Name.StartsWith("get_", StringComparison.Ordinal) || member.Name.StartsWith("set_", StringComparison.Ordinal)) &&
						info.GetParameters().Length == 0 || member.Name.StartsWith("op_", StringComparison.Ordinal)) {
						if(!member.Name.StartsWith("get_Item", StringComparison.Ordinal) && !member.Name.StartsWith("set_Item", StringComparison.Ordinal)) {
							return false;
						}
					}
				}
				break;
				case MemberTypes.Property: {
					PropertyInfo info = member as PropertyInfo;
					if(info != null) {
						ParameterInfo[] pInfo = info.GetIndexParameters();
						if(pInfo != null && pInfo.Length > 0) {
							return false;
						}
					}
				}
				break;
				case MemberTypes.NestedType:
				case MemberTypes.TypeInfo:
					if((member as Type).IsEnum) {
						return false;
					}
					break;
				case MemberTypes.Custom:
				case MemberTypes.Constructor:
				case MemberTypes.Event:
					return false;
			}
			if(!filter.Boxing || filter.SetMember) {
				return !member.DeclaringType.IsValueType;
			}
			return filter.ValidNextMemberTypes.HasFlags(member.MemberType);
		}

		public static List<ReflectionItem> AddAssemblyItems(Assembly assembly, FilterAttribute filter, Func<Type, bool> typeValidation = null) {
			List<ReflectionItem> Items = new List<ReflectionItem>();
			Type[] types = GetAssemblyTypes(assembly);
			bool showObsolete = uNodePreference.GetPreference().showObsoleteItem;
			if(filter.Types != null && filter.Types.Count == 1 && filter.Types[0].IsGenericType && filter.Types[0].IsInterface) {
				var genericArgument = filter.Types[0].GetGenericArguments();
				List<Type> typesList = new List<Type>();
				foreach(var type in types) {
					if(type.IsGenericTypeDefinition) {
						if(type.GetGenericArguments().Length == genericArgument.Length) {
							try {
								typesList.Add(ReflectionUtils.MakeGenericType(type, genericArgument));
							}
							catch { }
						}
					}
					typesList.Add(type);
				}
				foreach(Type type in typesList) {
					if(typeValidation != null && !typeValidation(type)) {
						continue;
					}
					if(!type.IsVisible ||
						!showObsolete && type.IsDefined(typeof(ObsoleteAttribute), true) ||
						!filter.DisplayGenericType && type.IsGenericType) {
						continue;
					}
					Items.Add(new ReflectionItem() {
						isStatic = true,
						memberInfo = type,
						hasNextItems = !type.IsEnum,
						canSelectItems = 
							filter.CanSelectType && filter.IsValidType(type) || 
							filter.IsValidTarget(MemberData.TargetType.Values) && filter.IsValidTypeForValue(type),
						memberType = type,
						//childItems = GetChildItems(type, filter, subMemberValidation) 
					});
				}
			} else {
				foreach(Type type in types) {
					if(typeValidation != null && !typeValidation(type)) {
						continue;
					}
					if(!type.IsVisible || !showObsolete && type.IsDefined(typeof(ObsoleteAttribute), true) || !filter.DisplayGenericType && type.IsGenericType) {
						continue;
					}
					Items.Add(new ReflectionItem() {
						isStatic = true,
						memberInfo = type,
						hasNextItems = !type.IsEnum,
						canSelectItems = 
							filter.CanSelectType && filter.IsValidType(type) ||
							filter.IsValidTarget(MemberData.TargetType.Values) && filter.IsValidTypeForValue(type) ||
							filter.Types?.Count == 1 && filter.Types[0] == typeof(Type) && !(type is RuntimeType),
						memberType = type,
						//childItems = GetChildItems(type, filter, subMemberValidation) 
					});
				}
			}
			return Items;
		}

		public static List<ReflectionItem> AddGeneralReflectionItems(Type memberType, FilterAttribute filter, Func<ReflectionItem, bool> validation = null) {
			if(memberType != null) {
				return GetReflectionItems(memberType, filter.validBindingFlags, filter, validation);
			}
			return null;
		}

		public static List<ReflectionItem> AddGeneralReflectionItems(Type memberType, IEnumerable<MemberInfo> members, FilterAttribute filter, Func<ReflectionItem, bool> validation = null) {
			return GetReflectionItems(memberType, members, filter.validBindingFlags, filter, validation);
		}

		public static ReflectionItem GetReflectionItems(MemberInfo info,
			FilterAttribute filter = null,
			Func<ReflectionItem, bool> validation = null,
			Func<MemberInfo, bool> memberValidation = null) {
			if(memberValidation != null && !memberValidation(info))
				return null;
			if(filter == null)
				filter = FilterAttribute.Default;
			if(info is Type) {
				Type type = info as Type;
				return new ReflectionItem() {
					isStatic = true,
					memberInfo = type,
					hasNextItems = !type.IsEnum,
					canSelectItems = 
						filter.CanSelectType && filter.IsValidType(type) ||
						filter.IsValidTarget(MemberData.TargetType.Values) && filter.IsValidTypeForValue(type) ||
						filter.Types?.Count == 1 && filter.Types[0] == typeof(Type) && !(type is RuntimeType),
					memberType = type,
					//childItems = GetChildItems(type, filter, subMemberValidation) 
				};
			}
			if(info.DeclaringType.IsGenericTypeDefinition)
				return null;
			bool canSelect = filter.ValidMemberType.HasFlags(info.MemberType);
			if(canSelect) {
				canSelect = ValidateMember(info, filter);
				//if(!canSelect && info.MemberType == MemberTypes.Method && filter.Types != null && filter.Types.Count == 1) {
				//	MethodInfo method = info as MethodInfo;
				//	var genericType = method.GetGenericArguments();
				//	if(method.IsGenericMethodDefinition && genericType.Length == 1) {
				//		try {
				//			info = method.MakeGenericMethod(filter.Types[0]);
				//			canSelect = ValidateMember(info, filter, bindingFlags);
				//		} catch { }
				//	}
				//}
			}
			if(canSelect && filter.ValidMemberType.HasFlags(info.MemberType)) {
				bool flag = filter.SetMember ? ReflectionUtils.CanSetMemberValue(info) : ReflectionUtils.CanGetMember(info, filter);
				if(flag) {
					bool hasNextItem = ValidateNextMember(info, filter);
					if(info.MemberType == MemberTypes.Field ||
						info.MemberType == MemberTypes.Property ||
						info.MemberType == MemberTypes.Event ||
						info.MemberType == MemberTypes.NestedType) {
						ReflectionItem item = new ReflectionItem() {
							hasNextItems = hasNextItem,
							memberInfo = info,
							canSelectItems = true,
							isStatic = ReflectionUtils.GetMemberIsStatic(info),
						};
						if(validation != null && !validation(item)) {
							return null;
						}
						return item;
					} else if(info.MemberType == MemberTypes.Method) {
						MethodInfo method = info as MethodInfo;
						if(ReflectionUtils.IsValidMethod(method, filter.MaxMethodParam, filter.MinMethodParam, filter)) {
							ReflectionItem item = new ReflectionItem() {
								hasNextItems = hasNextItem,
								memberInfo = info,
								canSelectItems = true,
								isStatic = ReflectionUtils.GetMemberIsStatic(info),
							};
							if(validation != null && !validation(item)) {
								return null;
							}
							return item;
						}
					} else if(info.MemberType == MemberTypes.Constructor) {
						ConstructorInfo ctor = info as ConstructorInfo;
						if(ReflectionUtils.IsValidConstructor(ctor, filter.MaxMethodParam, filter.MinMethodParam)) {
							ReflectionItem item = new ReflectionItem() {
								hasNextItems = hasNextItem,
								memberInfo = info,
								canSelectItems = true,
								isStatic = ReflectionUtils.GetMemberIsStatic(info),
							};
							if(validation != null && !validation(item)) {
								return null;
							}
							return item;
						}
					}
				} else if(info.MemberType != MemberTypes.Constructor) {
					bool canGet = ReflectionUtils.CanGetMember(info, filter);
					if(canGet) {
						ReflectionItem item = new ReflectionItem() {
							hasNextItems = true,
							memberInfo = info,
							isStatic = ReflectionUtils.GetMemberIsStatic(info),
						};
						if(validation != null && !validation(item)) {
							return null;
						}
						return item;
					}
				}
			} else if(ValidateNextMember(info, filter)) {
				bool flag = ReflectionUtils.CanGetMember(info, filter);
				if(flag) {
					ReflectionItem item = new ReflectionItem() {
						hasNextItems = true,
						memberInfo = info,
						isStatic = ReflectionUtils.GetMemberIsStatic(info),
					};
					if(validation != null && !validation(item)) {
						return null;
					}
					return item;
				}
			}
			return null;
		}

		public static List<ReflectionItem> GetReflectionItems(Type type,
			BindingFlags bindingFlags,
			FilterAttribute filter = null,
			Func<ReflectionItem, bool> validation = null,
			Func<MemberInfo, bool> memberValidation = null) {
			if(filter == null) {
				filter = new FilterAttribute();
			}
			IEnumerable<MemberInfo> members;
			if(type.IsInterface) {
				members = ReflectionUtils.GetMembers(type, bindingFlags).Distinct();
			} else {
				members = type.GetMembers(bindingFlags);
			}
			List<ReflectionItem> Items = new List<ReflectionItem>();
			if(filter.Static && !filter.SetMember && filter.ValidMemberType.HasFlags(MemberTypes.Constructor) && !type.IsCastableTo(typeof(Delegate))) {
				BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
				if(type.IsValueType) {
					flag |= BindingFlags.Static | BindingFlags.NonPublic;
				}
				ConstructorInfo[] ctor = type.GetConstructors(flag);
				for(int i = ctor.Length - 1; i >= 0; i--) {
					ReflectionItem item = GetReflectionItems(ctor[i], filter, validation, memberValidation);
					if(item != null) {
						Items.Add(item);
					}
				}
			}
			foreach(var m in members) { 
				ReflectionItem item = GetReflectionItems(m, filter, validation, memberValidation);
				if(item != null && (item.memberInfo == null || item.memberInfo.MemberType != MemberTypes.Constructor)) {
					Items.Add(item);
				}
			}
			return Items;
		}

		public static List<ReflectionItem> GetReflectionItems(Type type, IEnumerable<MemberInfo> members, BindingFlags bindingFlags,
			FilterAttribute filter = null,
			Func<ReflectionItem, bool> validation = null,
			Func<MemberInfo, bool> memberValidation = null) {
			if(filter == null) {
				filter = new FilterAttribute();
			}
			List<ReflectionItem> Items = new List<ReflectionItem>();
			foreach(var member in members) {
				ReflectionItem item = GetReflectionItems(member, filter, validation, memberValidation);
				if(item != null && (item.memberInfo == null || item.memberInfo.MemberType != MemberTypes.Constructor)) {
					Items.Add(item);
				}
			}
			return Items;
		}

		public static string GetOverloadingMethodNames(MethodInfo method, bool includeReturnType = true) {
			ParameterInfo[] info = method.GetParameters();
			string mConstructur = null;
			if(method.IsGenericMethod) {
				foreach(Type arg in method.GetGenericArguments()) {
					if(string.IsNullOrEmpty(mConstructur)) {
						mConstructur += "<" + arg.ToString();
						continue;
					}
					mConstructur += "," + arg.ToString();
				}
				mConstructur += ">";
			}
			mConstructur += "(";
			for(int i = 0; i < info.Length; i++) {
				mConstructur += info[i].ParameterType.PrettyName(false, info[i].IsOut).Split('.').Last() + " " + info[i].Name;
				if(i + 1 < info.Length) {
					mConstructur += ", ";
				}
			}
			mConstructur += ")";
			//string name = method.Name;
			//switch(name) {
			//	case "op_Addition":
			//		name = "Add";
			//		break;
			//	case "op_Subtraction":
			//		name = "Subtract";
			//		break;
			//	case "op_Multiply":
			//		name = "Multiply";
			//		break;
			//	case "op_Division":
			//		name = "Divide";
			//		break;
			//}
			if(includeReturnType) {
				return method.ReturnType.PrettyName() + " " + method.Name + mConstructur;
			} else {
				return method.Name + mConstructur;
			}
		}

		public static string GetOverloadingFunctionNames(uNodeFunction function, bool includeReturnType = true) {
			var info = function.parameters;
			string mConstructur = null;
			if(function.genericParameters.Length > 0) {
				foreach(var arg in function.genericParameters) {
					if(string.IsNullOrEmpty(mConstructur)) {
						mConstructur += "<" + arg.name;
						continue;
					}
					mConstructur += "," + arg.name;
				}
				mConstructur += ">";
			}
			mConstructur += "(";
			for(int i = 0; i < info.Length; i++) {
				mConstructur += info[i].Type.PrettyName(false, info[i].isByRef).Split('.').Last() + " " + info[i].name;
				if(i + 1 < info.Length) {
					mConstructur += ", ";
				}
			}
			mConstructur += ")";
			if(includeReturnType) {
				return function.ReturnType().PrettyName() + " " + function.Name + mConstructur;
			} else {
				return function.Name + mConstructur;
			}
		}

		public static string GetOverloadingConstructorNames(ConstructorInfo ctor) {
			ParameterInfo[] info = ctor.GetParameters();
			string mConstructur = "(";
			for(int i = 0; i < info.Length; i++) {
				mConstructur += info[i].ParameterType.PrettyName(false, info[i].IsOut).Split('.').Last() + " " + info[i].Name;
				if(i + 1 < info.Length) {
					mConstructur += ", ";
				}
			}
			mConstructur += ")";
			return "new " + ctor.DeclaringType.PrettyName() + mConstructur;
		}

		public static string GetConstructorPrettyName(ConstructorInfo ctor) {
			ParameterInfo[] info = ctor.GetParameters();
			string mConstructur = "(";
			for(int i = 0; i < info.Length; i++) {
				mConstructur += info[i].Name;
				if(i + 1 < info.Length) {
					mConstructur += ", ";
				}
			}
			mConstructur += ")";
			return ctor.DeclaringType.PrettyName() + mConstructur;
		}

		public static List<MemberInfo> GetOverrideMembers(Type type) {
			var result = new List<MemberInfo>();
			result.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(info => {
				var method = info.GetGetMethod() ?? info.GetSetMethod();
				if(method != null) {
					if(!method.IsAbstract && !method.IsVirtual)
						return false;
					if(method.IsStatic)
						return false;
					if(info.IsSpecialName)
						return false;
					if(method.IsPrivate)
						return false;
					if(method.IsConstructor)
						return false;
					if(method.ContainsGenericParameters)
						return false;
					if(!method.IsPublic && !method.IsFamily)
						return false;
					if(method.IsFamilyAndAssembly)
						return false;
					if(info.IsDefinedAttribute(typeof(ObsoleteAttribute)))
						return false;
					if(info.IsDefinedAttribute(typeof(System.Runtime.ConstrainedExecution.ReliabilityContractAttribute))) {
						return false;
					}
				}
				return true;
			}));
			result.AddRange(type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate (MethodInfo info) {
				if(!info.IsAbstract && !info.IsVirtual)
					return false;
				if(info.IsStatic)
					return false;
				if(info.IsSpecialName)
					return false;
				if(info.IsPrivate)
					return false;
				if(info.IsConstructor)
					return false;
				if(info.Name.StartsWith("get_", StringComparison.Ordinal))
					return false;
				if(info.Name.StartsWith("set_", StringComparison.Ordinal))
					return false;
				if(info.ContainsGenericParameters)
					return false;
				if(!info.IsPublic && !info.IsFamily)
					return false;
				if(info.IsFamilyAndAssembly)
					return false;
				if(info.IsDefinedAttribute(typeof(ObsoleteAttribute)))
					return false;
				if(info.IsDefinedAttribute(typeof(System.Runtime.ConstrainedExecution.ReliabilityContractAttribute))) {
					return false;
				}
				return true;
			}));
			return result;
		}
		#endregion

		public class ReflectionItem {
			public object instance;
			public MemberInfo memberInfo;
			public bool isStatic;

			public bool hasNextItems;
			public bool canSelectItems;
			
			private Type _memberType;
			public Type memberType {
				get {
					if(_memberType == null && memberInfo != null) {
						_memberType = ReflectionUtils.GetMemberType(memberInfo);
					}
					return _memberType;
				}
				set {
					_memberType = value;
				}
			}

			public MemberData.TargetType targetType {
				get{
					if(memberInfo != null) {
						switch(memberInfo.MemberType) {
							case MemberTypes.Constructor:
								return MemberData.TargetType.Constructor;
							case MemberTypes.Event:
								return MemberData.TargetType.Event;
							case MemberTypes.Field:
								return MemberData.TargetType.Field;
							case MemberTypes.Method:
								return MemberData.TargetType.Method;
							case MemberTypes.NestedType:
							case MemberTypes.TypeInfo:
								return MemberData.TargetType.Type;
							case MemberTypes.Property:
								return MemberData.TargetType.Property;
						}
					} else if(instance is MemberData mData) {
						return mData.targetType;
					}
					throw null;
				}
			}

			private string _name;
			public string name {
				get {
					if(_name == null) {
						if(memberInfo != null) {
							_name = memberInfo.Name;
						} else {
							_name = displayName;
						}
					}
					return _name;
				}
			}

			private string _displayName;
			public string displayName {
				get {
					if(_displayName == null) {
						if(memberInfo != null) {
							switch(memberInfo.MemberType) {
								case MemberTypes.Constructor:
									_displayName = GetOverloadingConstructorNames(memberInfo as ConstructorInfo);
									break;
								case MemberTypes.Method:
									_displayName = GetOverloadingMethodNames(memberInfo as MethodInfo, false);
									break;
								case MemberTypes.NestedType:
								case MemberTypes.TypeInfo:
									_displayName = (memberInfo as Type).PrettyName();
									break;
								default:
									_displayName = memberInfo.Name;
									break;
							}
						} else if(instance is MemberData mData) {
							switch(mData.targetType) {
								case MemberData.TargetType.Type:
									if(memberType != null) {
										_displayName = memberType.PrettyName();
									} else {
										_displayName = mData.Get<Type>().PrettyName();
									}
									break;
								case MemberData.TargetType.SelfTarget:
									_displayName = "this";
									break;
								default:
									_displayName = mData.startName;
									break;
							}
						} else if(memberType != null) {
							_displayName = memberType.PrettyName();
						}
					}
					return _displayName;
				}
			}

			public ReflectionItem() { }
		}
	}
}