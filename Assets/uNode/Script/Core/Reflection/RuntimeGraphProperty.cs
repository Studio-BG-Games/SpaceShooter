using System;
using System.Globalization;
using System.Reflection;

namespace MaxyGames.uNode {
	public class RuntimeGraphProperty : RuntimeProperty<uNodeProperty>, ISummary {
        public RuntimeGraphProperty(RuntimeType owner, uNodeProperty target) : base(owner, target) { }

		public override string Name => target.Name;

		public override Type PropertyType => target.ReturnType();

		public override bool IsStatic => owner.IsAbstract | owner.IsSealed;

		public string GetSummary() {
			return target.GetSummary();
		}

		private RuntimePropertyGetMethod _getMethod;
		public override MethodInfo GetGetMethod(bool nonPublic) {
			if(_getMethod == null && target.CanGetValue()) {
				_getMethod = new RuntimePropertyGetMethod(owner, this);
			}
			return target.CanGetValue() ? _getMethod : null;
		}

		private RuntimePropertySetMethod _setMethod;
		public override MethodInfo GetSetMethod(bool nonPublic) {
			if(_setMethod == null && target.CanSetValue()) {
				_setMethod = new RuntimePropertySetMethod(owner, this);
			}
			return target.CanSetValue() ? _setMethod : null;
		}

		public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
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

		public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			if(IsStatic) {
				if(owner is RuntimeGraphType runtimeGraph) {
					if(runtimeGraph.target != null) {
						DoSetValue(obj, value);
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
			if(obj is IRuntimeProperty runtime) {
				return runtime.GetProperty(target.Name);
			} else if(obj is IPropertySystem prop) {
				var data = prop.GetPropertyData(target.Name);
				if(data == null) {
					throw new Exception($"Property: {target.Name} not found in object: {obj}");
				}
				return data.Get();
			} else if(obj is RuntimeGraphType type) {
				if(type.target is IRuntimeProperty runtimeGraph) {
					return runtimeGraph.GetProperty(target.Name);
				} else if(type.target is IPropertySystem propGraph) {
					var data = propGraph.GetPropertyData(target.Name);
					if(data == null) {
						throw new Exception($"Property: {target.Name} not found in object: {propGraph}");
					}
					return data.Get();
				} else {
					throw new Exception($"Attempt to get property from invalid source: {obj}");
				}
			} else {
				throw new Exception($"Attempt to get property from invalid source: {obj}");
			}
		}

		protected void DoSetValue(object obj, object value) {
			if(obj is IRuntimeProperty runtime) {
				runtime.SetProperty(target.Name, value);
				return;
			} else if(obj is IPropertySystem prop) {
				var data = prop.GetPropertyData(target.Name);
				if(data == null) {
					throw new Exception($"Property: {target.Name} not found in object: {obj}");
				}
				data.Set(value);
			} else if(obj is RuntimeGraphType type) {
				if(type.target is IRuntimeProperty runtimeGraph) {
					runtimeGraph.SetProperty(target.Name, value);
				} else if(type.target is IPropertySystem propGraph) {
					var data = propGraph.GetPropertyData(target.Name);
					if(data == null) {
						throw new Exception($"Property: {target.Name} not found in object: {propGraph}");
					}
					data.Set(value);
				} else {
					throw new Exception($"Attempt to set property from invalid source: {obj}");
				}
			} else {
				throw new Exception($"Attempt to set property from invalid source: {obj}");
			}
		}
	}

	public class RuntimePropertyGetMethod : RuntimeMethod<RuntimeProperty> {
		public override string Name => target.Name.AddFirst("get_");
		public override Type ReturnType => target.PropertyType;

		public RuntimePropertyGetMethod(RuntimeType owner, RuntimeProperty target) : base(owner, target) { }

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			return target.GetValue(obj);
		}

		public override ParameterInfo[] GetParameters() {
			return new ParameterInfo[0];
		}
	}

	public class RuntimePropertySetMethod : RuntimeMethod<RuntimeProperty> {
		public override string Name => target.Name.AddFirst("set_");
		public override Type ReturnType => target.PropertyType;

		public RuntimePropertySetMethod(RuntimeType owner, RuntimeProperty target) : base(owner, target) { }

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			target.SetValue(obj, parameters[0]);
			return null;
		}

		public override ParameterInfo[] GetParameters() {
			return new ParameterInfo[] { new RuntimeParameterInfo("value", target.PropertyType) };
		}
	}
}