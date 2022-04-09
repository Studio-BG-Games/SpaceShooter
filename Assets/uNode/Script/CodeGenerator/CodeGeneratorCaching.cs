using MaxyGames.uNode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MaxyGames {
	public static partial class CG {
        /// <summary>
		/// Mark object as initialized with specific ID so it never be entered again by using HasInitialized().
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="id"></param>
		public static void SetInitialized(UnityEngine.Object owner, int id = 0) {
			HashSet<int> hash;
			if(!generatorData.initializedUserObject.TryGetValue(owner, out hash)) {
				hash = new HashSet<int>();
				generatorData.initializedUserObject[owner] = hash;
			}
			if(!hash.Contains(id)) {
				hash.Add(id);
			}
		}

		/// <summary>
		/// Are the owner with specific ID has been initialized?
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static bool HasInitialized(UnityEngine.Object owner, int id = 0) {
			HashSet<int> hash;
			if(generatorData.initializedUserObject.TryGetValue(owner, out hash)) {
				return hash.Contains(id);
			}
			return false;
		}

        #region GetVariableName
		/// <summary>
		/// Get the variable name from variable.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static string GetVariableName(VariableData variable) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef == variable) {
					return vdata.name;
				}
			}
			string name = variable.Name;
			name = GenerateVariableName(variable.Name);
			generatorData.AddVariable(new VData(variable, false) { name = name, isInstance = false, variableRef = variable, modifier = variable.modifier });
			return name;
		}

		/// <summary>
		/// Get the variable name from output port field.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="fieldName"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string GetOutputName(object from, string fieldName, int index = 0) {
			return GetOutputName(from, from.GetType().GetField(fieldName, MemberData.flags), index);
		}

		private static string GetOutputName(object from, FieldInfo field, int index = 0) {
			if(from == null || field == null)
				return null;
			Dictionary<FieldInfo, Dictionary<int, string>> dic;
			Dictionary<int, string> map;
			string name;
			if(generatorData.fieldVariableMap.TryGetValue(from, out dic)) {
				if(dic.TryGetValue(field, out map)) {
					if(!map.TryGetValue(index, out name)) {
						name = GenerateVariableName(field.Name);
						map[index] = name;
					}
					return name;
				} else {
					name = GenerateVariableName(field.Name);
					map = new Dictionary<int, string>();
					map.Add(index, name);
					dic[field] = map;
					generatorData.fieldVariableMap[from] = dic;
					return name;
				}
			}
			name = GenerateVariableName(field.Name);
			dic = new Dictionary<FieldInfo, Dictionary<int, string>>();
			map = new Dictionary<int, string>();
			map.Add(index, name);
			dic[field] = map;
			generatorData.fieldVariableMap[from] = dic;
			return name;
		}
		#endregion

        #region GenerateVariableName
		/// <summary>
		/// Generate new variable name and auto correct wrong names.
		/// </summary>
		/// <returns></returns>
		public static string GenerateVariableName(string variableName) {
			if(string.IsNullOrEmpty(variableName)) {
				variableName = "variable";
			}
			variableName = uNodeUtility.AutoCorrectName(variableName);
			if(generatorData.VarNames.ContainsKey(variableName)) {
				string name;
				while(true) {
					name = variableName + (++generatorData.VarNames[variableName]).ToString();
					if(!generatorData.VarNames.ContainsKey(name)) {
						break;
					}
				}
				return name;
			} else {
				generatorData.VarNames.Add(variableName, 0);
				return variableName;
			}
		}

		/// <summary>
		/// Generate new variable name and auto correct wrong names.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static string GenerateVariableName(string name, object owner) {
			if(owner != null) {
				Dictionary<string, string> map;
				if(generatorData.variableNamesMap.TryGetValue(owner, out map)) {
					string result;
					if(map.TryGetValue(name, out result)) {
						return result;
					} else {
						result = GenerateVariableName(name);
						map.Add(name, result);
						return result;
					}
				} else {
					map = new Dictionary<string, string>();
					generatorData.variableNamesMap[owner] = map;
					string result = GenerateVariableName(name);
					map.Add(name, result);
					return result;
				}
			}
			return GenerateVariableName(name);
		}
		#endregion

        #region UserObject
		/// <summary>
		/// Register new user object data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static T RegisterUserObject<T>(T value, object owner) {
			generatorData.userObjectMap[owner] = value;
			return value;
		}

		/// <summary>
		/// Get user object data.
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static object GetUserObject(object owner) {
			if(generatorData.userObjectMap.ContainsKey(owner)) {
				return generatorData.userObjectMap[owner];
			}
			return null;
		}

		/// <summary>
		/// Get user object data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static T GetUserObject<T>(object owner) {
			if(generatorData.userObjectMap.ContainsKey(owner)) {
				return (T)generatorData.userObjectMap[owner];
			}
			return default(T);
		}

		/// <summary>
		/// Get user object data if exist otherwise register new user object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static T GetOrRegisterUserObject<T>(T value, object owner) {
			if(generatorData.userObjectMap.ContainsKey(owner)) {
				return (T)generatorData.userObjectMap[owner];
			}
			return RegisterUserObject(value, owner);
		}

		/// <summary>
		/// Are the owner has user object data.
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static bool HasUserObject(object owner) {
			return generatorData.userObjectMap.ContainsKey(owner);
		}
		#endregion

		#region Get Functions
		public static MData GetOrRegisterFunction(string name, Type returnType, params Type[] parameterTypes) {
			var param = new string[parameterTypes.Length];
			for(int i=0;i<parameterTypes.Length;i++) {
				param[i] = Type(parameterTypes[i]);
			}
			return GetOrRegisterFunction(name, Type(returnType), param);
		}

		public static MData GetOrRegisterFunction(string name, string returnType, IList<string> parameterTypes) {
			var mData = generatorData.GetMethodData(name, parameterTypes);
			if(mData == null) {
				mData = generatorData.AddMethod(name, returnType, parameterTypes);
			}
			return mData;
		}
		#endregion
	}
}