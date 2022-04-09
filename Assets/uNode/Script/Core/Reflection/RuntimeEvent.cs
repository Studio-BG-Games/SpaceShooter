using System;
using System.Globalization;
using System.Reflection;
using UnityEngine.Events;

namespace MaxyGames.uNode {
	public class RuntimeEventValue : RuntimeEvent<IValue> {
		public RuntimeEventValue(RuntimeType owner, IValue target) : base(owner, target) {
			if(target == null) {
				throw new ArgumentNullException(nameof(target));
			}
		 }

		public override string Name {
			get {
				if(target is VariableData variable) {
					return variable.Name;
				} else if(target is uNodeProperty property) {
					return property.Name;
				}
				return target.ToString();
			}
		}

		public override void DoAddMethod(Delegate evt) {
			if(evt == null) return;
			var val = target.Get();
			if(val == null) {
				target.Set(evt);
			} else if(val is Delegate) {
				Delegate dlg = val as Delegate;
				Delegate.Combine(dlg, evt as Delegate);
			} else if(val is UnityEventBase) {
				UnityEventBase uevt = val as UnityEventBase;
				var method = val.GetType().GetMethod("AddListener");
				method.InvokeOptimized(uevt, new object[] { evt });
			}
		}

		public override void DoRaiseMethod() {
			var val = target.Get();
			if(val != null) {
				if(val is Delegate) {
					Delegate dlg = val as Delegate;
					Delegate.RemoveAll(dlg, null);
				} else if(val is UnityEventBase) {
					UnityEventBase uevt = val as UnityEventBase;
					var method = val.GetType().GetMethod("RemoveAllListeners");
					method.InvokeOptimized(uevt);
				}	
			}
		}

		public override void DoRemoveMethod(Delegate evt) {
			if(evt == null) return;
			var val = target.Get();
			if(val == null) {
				val = evt;
			} else if(val is Delegate) {
				Delegate dlg = val as Delegate;
				Delegate.Remove(dlg, evt as Delegate);
			} else if(val is UnityEventBase) {
				UnityEventBase uevt = val as UnityEventBase;
				var method = val.GetType().GetMethod("RemoveListener");
				method.InvokeOptimized(uevt, new object[] { evt });
			}
		}
	}

	class RuntimeEventGetAddMethod : RuntimeMethod<RuntimeEvent> {
		public override string Name => target.Name.AddFirst("add_");
		public override Type ReturnType => target.EventHandlerType;

		public RuntimeEventGetAddMethod(RuntimeType owner, RuntimeEvent target) : base(owner, target) { }

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			target.DoAddMethod(obj is object[] objs ? objs[0] as Delegate : null);
			return null;
		}

		public override ParameterInfo[] GetParameters() {
			return new ParameterInfo[0];
		}
	}

	class RuntimeEventGetRaiseMethod : RuntimeMethod<RuntimeEvent> {
		public override string Name => target.Name.AddFirst("add_");
		public override Type ReturnType => target.EventHandlerType;

		public RuntimeEventGetRaiseMethod(RuntimeType owner, RuntimeEvent target) : base(owner, target) { }

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			target.DoRaiseMethod();
			return null;
		}

		public override ParameterInfo[] GetParameters() {
			return new ParameterInfo[0];
		}
	}

	class RuntimeEventGetRemoveMethod : RuntimeMethod<RuntimeEvent> {
		public override string Name => target.Name.AddFirst("add_");
		public override Type ReturnType => target.EventHandlerType;

		public RuntimeEventGetRemoveMethod(RuntimeType owner, RuntimeEvent target) : base(owner, target) { }

		public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			target.DoRemoveMethod(obj is object[] objs ? objs[0] as Delegate : null);
			return null;
		}

		public override ParameterInfo[] GetParameters() {
			return new ParameterInfo[0];
		}
	}

	public class RuntimeEventCallback : RuntimeEvent {
		public Action<Delegate> addAction, removeAction;
		public Action raiseAction;

		public RuntimeEventCallback(RuntimeType owner, Action<Delegate> addAction, Action<Delegate> removeAction, Action raiseAction) : base(owner) {
			this.addAction = addAction;
			this.removeAction = removeAction;
			this.raiseAction = raiseAction;
		}

		public override string Name => "runtime_callback";

		public override void DoAddMethod(Delegate evt) {
			addAction(evt);
		}

		public override void DoRaiseMethod() {
			raiseAction();
		}

		public override void DoRemoveMethod(Delegate evt) {
			removeAction(evt);
		}
	}

	public abstract class RuntimeEvent<T> : RuntimeEvent {
		public readonly T target;

		public RuntimeEvent(RuntimeType owner, T target) : base(owner) {
			this.target = target;
		}

		public override object[] GetCustomAttributes(bool inherit) {
			if(typeof(T) is IAttributeSystem) {
				return (target as IAttributeSystem).GetAttributes();
			}
			return new object[0];
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			if(typeof(T) is IAttributeSystem) {
				return (target as IAttributeSystem).GetAttributes(attributeType);
			}
			return new object[0];
		}
	}

	public abstract class RuntimeEvent : EventInfo, IRuntimeMember {
		public readonly RuntimeType owner;

        public RuntimeEvent(RuntimeType owner) {
			this.owner = owner;
		}

		public override EventAttributes Attributes {
			get {
				return EventAttributes.None;
			}
		}

		public override Type DeclaringType => owner;

		public override Type ReflectedType => DeclaringType;
		
		public override int MetadataToken => 0;

		public override bool IsDefined(Type attributeType, bool inherit) {
			return false;
		}

		public abstract void DoAddMethod(Delegate evt);

		public abstract void DoRaiseMethod();

		public abstract void DoRemoveMethod(Delegate evt);

		private MethodInfo _addMethod;
		public override MethodInfo GetAddMethod(bool nonPublic) {
			if(_addMethod == null) {
				_addMethod = new RuntimeEventGetAddMethod(owner, this);
			}
			return _addMethod;
		}

		private MethodInfo _raiseMethod;
		public override MethodInfo GetRaiseMethod(bool nonPublic) {
			if(_raiseMethod == null) {
				_raiseMethod = new RuntimeEventGetAddMethod(owner, this);
			}
			return _raiseMethod;
		}

		private MethodInfo _removeMethod;
		public override MethodInfo GetRemoveMethod(bool nonPublic) {
			if(_removeMethod == null) {
				_removeMethod = new RuntimeEventGetAddMethod(owner, this);
			}
			return _removeMethod;
		}

		public override void AddEventHandler(object target, Delegate handler) {
			DoAddMethod(handler);
		}

		public override object[] GetCustomAttributes(bool inherit) {
			return new object[0];
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return new object[0];
		}
	}
}