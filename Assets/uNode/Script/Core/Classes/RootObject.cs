using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Base class for all Root Object that can have node.
	/// </summary>
	public abstract class RootObject : uNodeComponent, INode<uNodeRoot>, IParameterSystem, ILocalVariableSystem {
		/// <summary>
		/// The name of this root.
		/// </summary>
		[Hide]
		public string Name;
		/// <summary>
		/// The start node for execute when this root called.
		/// </summary>
		[Hide]
		public Node startNode;
		/// <summary>
		/// The owner of this root.
		/// </summary>
		[Hide]
		public uNodeRoot owner;

		[SerializeField, Hide]
		private string _guid;
		public string guid {
			get {
				if(string.IsNullOrEmpty(_guid)) {
					_guid = System.Guid.NewGuid().ToString();
				}
				return _guid;
			}
		}


		[HideInInspector]
		public ParameterData[] parameters = new ParameterData[0];
		[HideInInspector]
		public List<VariableData> localVariable = new List<VariableData>();

		public List<VariableData> LocalVariables {
			get {
				return localVariable;
			}
		}

		public IList<ParameterData> Parameters {
			get {
				return parameters;
			}
		}

		protected struct LocalVar {
			public object value;
			public VariableData variable;

			public string name => variable.Name;
		}

		[System.NonSerialized]
		protected IList<LocalVar> _localVar;

		protected void InitLocalVariable() {
			if(_localVar == null) {
				var variables = new List<LocalVar>();
				foreach(var var in localVariable) {
					if(var.value == null && var.Type.IsValueType) {
						var.value = ReflectionUtils.CreateInstance(var.Type);
					}
					variables.Add(new LocalVar() { value = var.value, variable = var });
				}
				_localVar = variables;
			} else {
				for(int x = 0; x < localVariable.Count; x++) {
					if(!localVariable[x].resetOnEnter)
						continue;
					for(int y = 0; y < _localVar.Count; y++) {
						if(localVariable[x] == _localVar[y].variable) {
							localVariable[x].value = SerializerUtility.Duplicate(_localVar[y].value);
							goto NEXT;
						}
					}
					_localVar.Add(new LocalVar() { value = localVariable[x].value, variable = localVariable[x] });//Add a new local variable if not exist
				NEXT:
					continue;
				}
			}
		}

		/// <summary>
		/// Get LocalVariable by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual VariableData GetLocalVariableData(string name) {
			if(CG.isGenerating || !uNodeUtility.isPlaying) {
				if(localVariable != null && localVariable.Count > 0) {
					for(int i = 0; i < localVariable.Count; i++) {
						if(localVariable[i].Name == name) {
							return localVariable[i];
						}
					}
				}
			} else {
				if(_localVar == null) {
					InitLocalVariable();
				}
				if(_localVar != null && _localVar.Count > 0) {
					for(int i = 0; i < _localVar.Count; i++) {
						if(_localVar[i].name == name) {
							return _localVar[i].variable;
						}
					}
				}
			}
			return null;
			//throw new System.Exception("No local variable with name : " + name);
		}

		public abstract System.Type ReturnType();

		/// <summary>
		/// Get parameter data by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public ParameterData GetParameterData(string name) {
			for(int i = 0; i < parameters.Length; i++) {
				if(parameters[i].name == name) {
					return parameters[i];
				}
			}
			return null;
			//throw new System.Exception(name + " Parameter not found");
		}

		/// <summary>
		/// Set parameter value by name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetParameterValue(string name, object value) {
			GetParameterData(name).value = value;
		}

		/// <summary>
		/// Get parameter value by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public object GetParameterValue(string name) {
			return GetParameterData(name).value;
		}

		/// <summary>
		/// Are this root can have coroutine node.
		/// </summary>
		/// <returns></returns>
		public abstract bool CanHaveCoroutine();

		/// <summary>
		/// Get the owner of this root.
		/// </summary>
		/// <returns></returns>
		public uNodeRoot GetOwner() {
			return owner;
		}

		INodeRoot INode.GetNodeOwner() {
			return GetOwner();
		}
	}
}