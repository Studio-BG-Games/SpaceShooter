namespace MaxyGames.uNode {
	/// <summary>
	/// The base class for all Value Node.
	/// </summary>
	public abstract class ValueNode : Node {
		/// <summary>
		/// The type of value will return.
		/// </summary>
		/// <returns></returns>
		public override System.Type ReturnType() {
			return typeof(object);
		}

		/// <summary>
		/// Invoke node and get the value.
		/// Don't override this function, override Value() instead.
		/// </summary>
		/// <returns></returns>
		public override object GetValue() {
			/*if(uNodeUtils.isInEditor && uNodeUtils.useDebug) {
				object val = Value();
				uNodeUtils.InvokeValueNode(uNodeObject, uNodeObject.GetInstanceID(), this.GetInstanceID(), val);
				return val;
			}*/
			try {
				return Value();
			}
			catch (System.Exception ex){
				if(ex is uNodeException) {
					throw;
				}
				throw uNodeDebug.LogException(new uNodeException("Error on getting value of node:" + gameObject.name, ex, this), this);
			}
		}

		public override bool CanGetValue() {
			return ReturnType() != typeof(void);
		}

		/// <summary>
		/// Override this function to return a value.
		/// </summary>
		/// <returns></returns>
		protected abstract object Value();

		public override bool IsFlowNode() {
			return false;
		}

		public override string GetNodeName() {
			return this.GetType().Name;
		}

		public override bool IsSelfCoroutine() {
			return false;
		}

		public override string GenerateCode() {
			string str = GenerateValueCode();
			if(!string.IsNullOrEmpty(str)) {
				return str.Add(";");
			}
			return base.GenerateCode();
		}

		//public override System.Type GetNodeIcon() {
		//	System.Type t = ReturnType();
		//	if(t == typeof(object) || t == typeof(System.Type)) {
		//		return typeof(void);
		//	}
		//	return t;
		//}
	}
}