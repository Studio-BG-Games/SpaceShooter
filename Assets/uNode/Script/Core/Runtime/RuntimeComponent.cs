using System;
using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames {
	public abstract class RuntimeComponent : MonoBehaviour, IRuntimeClass {
		// public string uniqueIdentifier { get; set; }
		
		public abstract object GetProperty(string Name);
		public abstract T GetProperty<T>(string Name);

		public abstract object GetVariable(string Name);
		public abstract T GetVariable<T>(string Name);

		public abstract object InvokeFunction(string Name, object[] values);
		public abstract object InvokeFunction(string Name, Type[] parameters, object[] values);

		//public T InvokeFunction<T>(string Name, object[] values) {
		//	object val = InvokeFunction(Name, values);
		//	if(val == null) {
		//		return default(T);
		//	}
		//	return (T)val;
		//}

		//public T InvokeFunction<T>(string Name, Type[] parameters, object[] values) {
		//	object val = InvokeFunction(Name, parameters, values);
		//	if(val == null) {
		//		return default(T);
		//	}
		//	return (T)val;
		//}

		public abstract void SetProperty(string Name, object value);
		public abstract void SetProperty(string Name, object value, char @operator);

		public abstract void SetVariable(string Name, object value);
		public abstract void SetVariable(string Name, object value, char @operator);
		public override bool Equals(object other) {
			if(other is RuntimeComponent runtime) {
				return CompareRuntimeObjects(this, runtime);
			}
			return base.Equals(other);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static bool operator ==(RuntimeComponent x, RuntimeComponent y) {
			return CompareRuntimeObjects(x, y);
		}

		public static bool operator !=(RuntimeComponent x, RuntimeComponent y) {
			return !CompareRuntimeObjects(x, y);
		}

		private static bool CompareRuntimeObjects(UnityEngine.Object x, UnityEngine.Object y) {
			if(x == null && y == null)
				return true;
			if(x is uNodeSpawner) {
				if(y is uNodeSpawner) {
					return x == y;
				}
				return (x as uNodeSpawner).GetRuntimeInstance() == y;
			} else if(y is uNodeSpawner) {
				return (y as uNodeSpawner).GetRuntimeInstance() == x;
			}
			return x == y;
		}
	}
}