using System.Collections.Generic;

namespace MaxyGames.uNode {
	/// <summary>
	/// The base class for all group.
	/// </summary>
	public abstract class GroupNode : Node, IVariableSystem, ISuperNode {
		/// <summary>
		/// The node to execute on this group is called.
		/// </summary>
		[Hide]
		public Node nodeToExecute;

		/// <summary>
		/// Are this group is coroutine?
		/// </summary>
		/// <returns></returns>
		public override bool IsCoroutine() {
			return AcceptCoroutine() || (nodeToExecute != null && nodeToExecute.IsCoroutine());
		}

		/// <summary>
		/// Are this group accept coroutine node?
		/// </summary>
		/// <returns></returns>
		public virtual bool AcceptCoroutine() {
			if(parentComponent == null) {
				return true;
			} else {
				var pComp = parentComponent;
				if(pComp as RootObject) {
					return (pComp as RootObject).CanHaveCoroutine();
				} else if(pComp is StateNode) {
					return true;
				} else if(pComp is ISuperNode) {
					return (pComp as ISuperNode).AcceptCoroutine();
				}
			}
			return false;
		}

		/// <summary>
		/// The list of Variable data this group have.
		/// </summary>
		public virtual List<VariableData> Variables {
			get {
				return null;
			}
		}

		public IList<NodeComponent> nestedFlowNodes {
			get {
				return new NodeComponent[] { nodeToExecute };
			}
		}

		[System.NonSerialized]
		protected IList<VariableData> _var;
		/// <summary>
		/// Get Variable by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual VariableData GetVariableData(string name) {
			if(_var == null) {
				_var = Variables;
			}
			if(_var != null && _var.Count > 0) {
				for(int i = 0; i < _var.Count; i++) {
					if(_var[i].Name == name) {
						return _var[i];
					}
				}
			}
			throw new System.Exception("No variable with name : " + name);
		}

		public void SetVariableValue(string name, object value) {
			GetVariableData(name).Set(value);
		}

		public object GetVariableValue(string name) {
			return GetVariableData(name).Get();
		}
	}
}