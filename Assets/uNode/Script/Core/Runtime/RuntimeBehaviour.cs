using System;

namespace MaxyGames {
	public abstract class RuntimeBehaviour : RuntimeComponent {
		public virtual void OnAwake() { }

		public virtual void OnBehaviourEnable() { }

		public override void SetVariable(string Name, object value) {
			var field = this.GetType().GetFieldCached(Name);
			if (field == null) {
				throw new Exception($"Variable with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			try {
				field.SetValueOptimized(this, value);
			}
			catch(Exception ex) {
				throw new Exception($"Error on performing set variable: '{Name}'\nName:{Name}\nType:{field.FieldType.FullName}\nValue:{value?.GetType().FullName}\nErrors:{ex.ToString()}", ex);
			}
		}

		public override void SetVariable(string Name, object value, char @operator) {
			var field = this.GetType().GetFieldCached(Name);
			if (field == null) {
				throw new Exception($"Variable with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			switch(@operator) {
				case '+':
				case '-':
				case '/':
				case '*':
				case '%':
					var val = field.GetValueOptimized(this);
					value = uNodeHelper.ArithmeticOperator(val, value, @operator, field.FieldType, value?.GetType());
					break;
			}
			try {
				field.SetValueOptimized(this, value);
			}
			catch (Exception ex) {
				throw new Exception($"Error on performing set variable: '{Name}'\nName:{Name}\nType:{field.FieldType.FullName}\nValue:{value?.GetType().FullName}\nErrors:{ex.ToString()}", ex);
			}
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
			try {
				property.SetValueOptimized(this, value);
			}
			catch(Exception ex) {
				throw new Exception($"Error on performing set property: '{Name}'\nName:{Name}\nType:{property.PropertyType.FullName}\nValue:{value?.GetType().FullName}\nErrors:{ex.ToString()}", ex);
			}
		}
		
		public override void SetProperty(string Name, object value, char @operator) {
			var property = this.GetType().GetPropertyCached(Name);
			if (property == null) {
				throw new Exception($"Property with name:{Name} not found from type {this.GetType().FullName}." + 
					"\nIt may because of outdated generated script, try to generate the script again.");
			}
			switch(@operator) {
				case '+': 
				case '-':
				case '/':
				case '*':
				case '%':
					var val = property.GetValueOptimized(this);
					value = uNodeHelper.ArithmeticOperator(val, value, @operator, property.PropertyType, value?.GetType());
					break;
			}
			value = uNodeHelper.GetActualRuntimeValue(value);
			try {
				property.SetValueOptimized(this, value);
			}
			catch(Exception ex) {
				throw new Exception($"Error on performing set property: '{Name}'\nName:{Name}\nType:{property.PropertyType.FullName}\nValue:{value?.GetType().FullName}\nErrors:{ex.ToString()}", ex);
			}
		}

		/// <summary>
		/// Execute function without parameters
		/// </summary>
		/// <param name="Name"></param>
		public void ExecuteFunction(string Name) {
			var func = this.GetType().GetMethod(Name, Type.EmptyTypes);
			if(func != null) {
				func.InvokeOptimized(this, null);
			}
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
			try {
				return func.InvokeOptimized(this, values);
			}
			catch(Exception ex) {
				throw new Exception($"Error on invoking function: '{Name}'\nErrors:{ex.ToString()}", ex);
			}
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
			try {
				return func.InvokeOptimized(this, values);
			}
			catch(Exception ex) {
				throw new Exception($"Error on invoking function: '{Name}'\nErrors:{ex.ToString()}", ex);
			}
		}
	}
}