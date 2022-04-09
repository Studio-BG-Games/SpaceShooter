using System;
using System.Globalization;
using System.Reflection;

namespace MaxyGames.uNode {
    public class RuntimeGraphField : RuntimeField<VariableData>, ISummary {
		public RuntimeGraphField(RuntimeType owner, VariableData target) : base(owner, target) {

		}

		public override Type FieldType => target.Type;

		public override string Name => target.Name;

		public string GetSummary() {
			return target.GetSummary();
		}

		public override object GetValue(object obj) {
			if(IsStatic) {
				if(owner is RuntimeGraphType runtimeGraph) {
					if(runtimeGraph.target != null) {
						return DoGetValue(runtimeGraph.target);
					}
					throw new NullReferenceException("The graph reference cannot be null");
				}
				throw new NotImplementedException();
			} else if(obj == null) {
				throw new NullReferenceException("The instance member cannot be null");
			}
			return DoGetValue(obj);
		}

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture) {
			if(IsStatic) {
				if(owner is RuntimeGraphType runtimeGraph) {
					if(runtimeGraph.target != null) {
						DoSetValue(runtimeGraph.target, value);
						return;
					}
					throw new NullReferenceException("The graph reference cannot be null");
				}
				throw new NotImplementedException();
			} else if(obj == null) {
				throw new NullReferenceException("The instance member cannot be null");
			}
			DoSetValue(obj, value);
		}

		protected object DoGetValue(object obj) {
			if(obj is IRuntimeVariable runtime) {
				return runtime.GetVariable(target.Name);
			}
			var data = (obj as IVariableSystem).GetVariableData(target.Name);
			if(data == null) {
				throw new Exception($"Variable: {target.Name} not found in object: {obj}");
			}
			return data.Get();
		}

		protected void DoSetValue(object obj, object value) {
			if(obj is IRuntimeVariable runtime) {
				runtime.SetVariable(target.Name, value);
				return;
			}
			var data = (obj as IVariableSystem).GetVariableData(target.Name);
			if(data == null) {
				throw new Exception($"Variable: {target.Name} not found in object: {obj}");
			}
			data.Set(value);
		}
	}
}