using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode {
	[System.Serializable]
	public class GlobalVariable : ScriptableObject, IVariableSystem {
		public List<VariableData> variable = new List<VariableData>();

		[System.NonSerialized]
		private bool hasInitialize = false;
		[System.NonSerialized]
		public List<VariableData> InstanceVeriable;
		[System.NonSerialized]
		private Dictionary<string, VariableData> variableMap;

		public List<VariableData> Variables {
			get {
				return InstanceVeriable;
			}
		}

		/// <summary>
		/// Used for initialize
		/// </summary>
		public void Initialize() {
			if(hasInitialize) return;
			InstanceVeriable = new List<VariableData>();
			variableMap = new Dictionary<string, VariableData>();
			foreach(VariableData v in variable) {
				InstanceVeriable.Add(new VariableData(v));
			}
			foreach(VariableData v in InstanceVeriable) {
				variableMap.Add(v.Name, v);
			}
#if UNITY_EDITOR
			if(Application.isPlaying)
#endif
			hasInitialize = true;
		}

		/// <summary>
		/// Function for Getting variable.
		/// </summary>
		/// <param name="Name">The variable to get</param>
		/// <returns></returns>
		public VariableData GetVariableData(string name) {
			if(!Application.isPlaying) {
				for(int i = 0; i < variable.Count; i++) {
					if(variable[i].Name.Equals(name)) {
						return variable[i];
					}
				}
				return null;
			}
			if(!hasInitialize) {
				Initialize();
			}
			if(variableMap.ContainsKey(name)) {
				return variableMap[name];
			}
			Debug.LogException(new System.Exception("No variable with name: " + name), this);
			throw null;
		}

		/// <summary>
		/// Function for set variable.
		/// </summary>
		/// <param name="Name">The variable name to set</param>
		/// <param name="value">The value to apply to variable</param>
		public void SetVariable(string variableName, object value) {
			if(!hasInitialize) {
				Initialize();
			}
			if(variableMap.ContainsKey(variableName)) {
				variableMap[variableName].Set(value);
			} else {
				Debug.LogException(new System.Exception("No variable with name: " + variableName), this);
				throw null;
			}
		}

		public void SetVariableValue(string name, object value) {
			GetVariableData(name).Set(value);
		}

		public object GetVariableValue(string name) {
			return GetVariableData(name).Get();
		}
	}
}