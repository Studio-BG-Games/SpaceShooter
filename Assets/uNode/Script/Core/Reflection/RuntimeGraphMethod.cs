using System;
using System.Linq;
using System.Globalization;
using System.Reflection;

namespace MaxyGames.uNode {
	public class RuntimeGraphMethod : RuntimeMethod<uNodeFunction>, ISummary {
		private Type[] functionTypes;
		
		public RuntimeGraphMethod(RuntimeType owner, uNodeFunction target) : base(owner, target) {
			this.target = target;
		}

		public override string Name => target.Name;

		public override Type ReturnType => target.ReturnType();

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			if(IsStatic) {
				if(owner is RuntimeGraphType runtimeGraph) {
					if(runtimeGraph.target != null) {
						return DoInvoke(runtimeGraph.target, parameters);
					}
					throw new NullReferenceException("The graph reference cannot be null");
				}
				throw new NotImplementedException();
			} else if(obj == null) {
				throw new NullReferenceException("The instance member cannot be null");
			}
			return DoInvoke(obj, parameters);
		}

		protected object DoInvoke(object obj, object[] parameters) {
			if(obj is IRuntimeFunction runtime) {
				return runtime.InvokeFunction(target.Name, GetParamTypes(), parameters);
			}
			var data = (obj as IFunctionSystem).GetFunction(target.Name, GetParamTypes());
			if(data == null) {
				throw new Exception($"Function: {target.Name} not found in object: {obj}");
			}
			return data.Invoke(parameters);
		}

		public override ParameterInfo[] GetParameters() {
			var types = target.Parameters;
			if(types != null) {
				var param = new ParameterInfo[types.Count];
				for (int i = 0; i < types.Count;i++) {
					param[i] = new RuntimeGraphParameter(types[i]);
				}
				return param;
			}
			return new ParameterInfo[0];
		}

		public string GetSummary() {
			return target.GetSummary();
		}

		private Type[] GetParamTypes() {
			if(functionTypes == null) {
				functionTypes = target.Parameters.Select(p => p.Type).ToArray();
			}
			return functionTypes;
		}
	}
}