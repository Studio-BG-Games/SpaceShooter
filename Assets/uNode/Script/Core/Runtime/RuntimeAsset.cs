using System;
using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames {
	public abstract class BaseRuntimeAsset : ScriptableObject, IRuntimeClass {
		public abstract object GetProperty(string Name);
		public abstract T GetProperty<T>(string Name);

		public abstract object GetVariable(string Name);
		public abstract T GetVariable<T>(string Name);

		public abstract object InvokeFunction(string Name, object[] values);
		public abstract object InvokeFunction(string Name, Type[] parameters, object[] values);

		public abstract void SetProperty(string Name, object value);
		public abstract void SetProperty(string Name, object value, char @operator);

		public abstract void SetVariable(string Name, object value);
		public abstract void SetVariable(string Name, object value, char @operator);

		public override bool Equals(object other) {
			if(other is BaseRuntimeAsset runtimeAsset) {
				return CompareRuntimeObjects(this, runtimeAsset);
			}
			return base.Equals(other);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static bool operator ==(BaseRuntimeAsset x, BaseRuntimeAsset y) {
			return CompareRuntimeObjects(x, y);
		}

		public static bool operator !=(BaseRuntimeAsset x, BaseRuntimeAsset y) {
			return !CompareRuntimeObjects(x, y);
		}

		private static bool CompareRuntimeObjects(UnityEngine.Object x, UnityEngine.Object y) {
			if(x == null && y == null)
				return true;
			if(x is uNodeAssetInstance) {
				if(y is uNodeAssetInstance) {
					return x == y;
				}
				return (x as uNodeAssetInstance).GetRuntimeInstance() == y;
			} else if(y is uNodeAssetInstance) {
				return (y as uNodeAssetInstance).GetRuntimeInstance() == x;
			}
			return x == y;
		}
	}

	public abstract class RuntimeAsset : BaseRuntimeAsset {
		public virtual void OnAwake() { }

		public override void SetVariable(string Name, object value) {
			var field = this.GetType().GetFieldCached(Name);
			if (field == null) {
				throw new Exception($"Variable with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			field.SetValueOptimized(this, value);
		}

		public override void SetVariable(string Name, object value, char @operator) {
			var field = this.GetType().GetFieldCached(Name);
			if (field == null) {
				throw new Exception($"Variable with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			switch (@operator) {
				case '+':
				case '-':
				case '/':
				case '*':
				case '%':
					var val = field.GetValueOptimized(this);
					value = uNodeHelper.ArithmeticOperator(val, value, @operator, field.FieldType, value?.GetType());
					break;
			}
			field.SetValueOptimized(this, value);
		}

		public override object GetVariable(string Name) {
			var field = this.GetType().GetFieldCached(Name);
			if (field == null) {
				throw new Exception($"Variable with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			return field.GetValueOptimized(this);
		}

		public override T GetVariable<T>(string Name) {
			var obj = GetVariable(Name);
			if (obj != null) {
				return (T)obj;
			}
			return default;
		}

		public override object GetProperty(string Name) {
			var property = this.GetType().GetPropertyCached(Name);
			if (property == null) {
				throw new Exception($"Property with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			return property.GetValueOptimized(this);
		}

		public override T GetProperty<T>(string Name) {
			var obj = GetProperty(Name);
			if (obj != null) {
				return (T)obj;
			}
			return default;
		}

		public override void SetProperty(string Name, object value) {
			var property = this.GetType().GetPropertyCached(Name);
			if (property == null) {
				throw new Exception($"Property with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			property.SetValueOptimized(this, value);
		}

		public override void SetProperty(string Name, object value, char @operator) {
			var property = this.GetType().GetPropertyCached(Name);
			if (property == null) {
				throw new Exception($"Property with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			switch (@operator) {
				case '+':
				case '-':
				case '/':
				case '*':
				case '%':
					var val = property.GetValue(this);
					value = uNodeHelper.ArithmeticOperator(val, value, @operator, property.PropertyType, value?.GetType());
					break;
			}
			property.SetValueOptimized(this, value);
		}

		public override object InvokeFunction(string Name, object[] values) {
			Type[] types = new Type[values != null ? values.Length : 0];
			if (values != null) {
				for (int i = 0; i < types.Length; i++) {
					types[i] = values[i] != null ? values[i].GetType() : typeof(object);
				}
				for (int i = 0; i < values.Length;i++) {
					values[i] = uNodeHelper.GetActualRuntimeValue(values[i]);
				}
			}
			var func = this.GetType().GetMethod(Name, types);
			if (func == null) {
				throw new Exception($"Function with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			return func.InvokeOptimized(this, values);
		}

		public override object InvokeFunction(string Name, Type[] parameters, object[] values) {
			var func = this.GetType().GetMethod(Name, parameters);
			if (func == null) {
				throw new Exception($"Function with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			if(values != null) {
				for (int i = 0; i < values.Length;i++) {
					values[i] = uNodeHelper.GetActualRuntimeValue(values[i]);
				}
			}
			return func.InvokeOptimized(this, values);
		}
	}
}