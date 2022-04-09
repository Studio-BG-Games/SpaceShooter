using System.Collections.Generic;
using UnityEngine;
using MaxyGames.OdinSerializer;

namespace MaxyGames {
	[System.Serializable]
	public class OdinSerializedData {
		public byte[] data;
		public List<Object> references;
		public string serializedType;

		private System.Type _type;
		public System.Type type {
			get {
				if(_type == null) {
					if(string.IsNullOrEmpty(serializedType)) {
						return typeof(object);
					}
					_type = TypeSerializer.Deserialize(serializedType, false);
				}
				return _type;
			}
			set {
				_type = type;
				serializedType = type.FullName;
			}
		}

		public bool isFilled => data != null && data.Length > 0;

		public object ToValue() {
			return SerializerUtility.Deserialize(this);
		}

		public void FromValue<T>(T value) {
			var serialized = CreateFrom(value);
			data = serialized.data;
			references = serialized.references;
			type = serialized.type;
		}

		public void CopyFrom(OdinSerializedData serializedData) {
			data = serializedData.data;
			references = serializedData.references;
			serializedType = serializedData.serializedType;
		}

		public static OdinSerializedData CreateFrom<T>(T value) {
			return SerializerUtility.SerializeValue(value);
		}
	}

	public static class SerializerUtility {
		public static T Duplicate<T>(T value) {
			if(value == null) {
				return default;
			} else if(value is Object || value.GetType().IsValueType) {
				return value;
			}
			return Deserialize<T>(Serialize(value, out var references), references);
		}

		public static OdinSerializedData SerializeValue<T>(T value) {
			OdinSerializedData data = new OdinSerializedData();
			data.data = Serialize(value, out data.references);
			//Since odin might fail on deserializing primitive type, we need save the original type so when deserializing value odin should not fail 
			if(typeof(T) == typeof(object) && value != null) {
				//Ensure we get the correct value type when the value is object
				var type = value.GetType();
				if(type.IsPrimitive) {
					data.type = type;
				}
			} else if(typeof(T).IsPrimitive) {
				data.type = typeof(T);
			}
			return data;
		}

		public static OdinSerializedData SerializeValue<T>(T value, out List<Object> references) {
			OdinSerializedData data = SerializeValue(value);
			references = data.references;
			return data;
		}

		public static object Deserialize(OdinSerializedData serializedData) {
			if(serializedData == null)
				return null;
			return Deserialize(serializedData.data, serializedData.references, serializedData.type);
		}

		public static byte[] Serialize<T>(T value) {
			return SerializationUtility.SerializeValue(value, DataFormat.Binary);
		}

		public static byte[] Serialize<T>(T value, out List<Object> unityReferences) {
			if(value == null) {
				unityReferences = new List<Object>();
				return new byte[0];
			}
			if(value is Object) {
				unityReferences = new List<Object>() { value as Object };
				return new byte[0];
			} 
			//else if(typeof(T) == typeof(object)) {
			//	return SerializationUtility.SerializeValueWeak(value, DataFormat.Binary, out unityReferences);
			//}
			return SerializationUtility.SerializeValue(value, DataFormat.Binary, out unityReferences);
		}

		public static byte[] SerializeWeak(object value, out List<Object> unityReferences) {
			return SerializationUtility.SerializeValueWeak(value, DataFormat.Binary, out unityReferences);
		}

		public static object DeserializeWeak(byte[] data, List<Object> unityReferences) {
			return SerializationUtility.DeserializeValueWeak(data, DataFormat.Binary, unityReferences);
		}

		public static T Deserialize<T>(byte[] data) {
			return SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary);
		}

		public static T Deserialize<T>(byte[] data, List<Object> unityReferences) {
			return SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary, unityReferences);
		}

		public static T Deserialize<T>(OdinSerializedData serializedData) {
			if(serializedData == null)
				return default;
			if(typeof(T).IsCastableTo(typeof(Object)) || !serializedData.isFilled) {
				if(serializedData.references?.Count > 0) {
					object value = serializedData.references[0];
					return value != null ? (T)value : default;
				}
				return default;
			}
			return SerializationUtility.DeserializeValue<T>(serializedData.data, DataFormat.Binary, serializedData.references);
		}

		public static object Deserialize(byte[] data, List<Object> unityReferences, System.Type type = null) {
			if(data == null) {
				if(type != null && type.IsValueType) {
					//Ensure we create new value if the type is value type
					return ReflectionUtils.CreateInstance(type);
				}
				return null;
			}
			if(type.IsCastableTo(typeof(Object)) || data.Length == 0) {
				if(unityReferences != null && unityReferences.Count > 0) {
					return unityReferences[0];
				}
				return null;
			} else if(type != null) {
				//This will fix some incorrect type result for primitive type
				switch(type.FullName) {
					case "System.Char":
						return SerializationUtility.DeserializeValue<char>(data, DataFormat.Binary, unityReferences);
					case "System.Single":
						return SerializationUtility.DeserializeValue<float>(data, DataFormat.Binary, unityReferences);
					case "System.Int32":
						return SerializationUtility.DeserializeValue<int>(data, DataFormat.Binary, unityReferences);
					case "System.Int64":
						return SerializationUtility.DeserializeValue<long>(data, DataFormat.Binary, unityReferences);
					case "System.Byte":
						return SerializationUtility.DeserializeValue<byte>(data, DataFormat.Binary, unityReferences);
					case "System.Boolean":
						return SerializationUtility.DeserializeValue<bool>(data, DataFormat.Binary, unityReferences);
					case "System.String":
						return SerializationUtility.DeserializeValue<string>(data, DataFormat.Binary, unityReferences);
				}
				return OdinSerializer.Unity_Integration.uNodeSerializationUtility.DeserializeValueWeak(data, DataFormat.Binary, unityReferences, type);
			}
			return SerializationUtility.DeserializeValueWeak(data, DataFormat.Binary, unityReferences);
		}

		//public static string SerializeToJson<T>(T value) {
		//	if(value == null) {
		//		return string.Empty;
		//	}
		//	if(value is Object) {
		//		return string.Empty;
		//	}
		//	return System.Text.Encoding.UTF8.GetString(SerializationUtility.SerializeValue(value, DataFormat.JSON));
		//}

		//public static string SerializeToJson<T>(T value, out List<Object> unityReferences) {
		//	if(value == null) {
		//		unityReferences = new List<Object>();
		//		return string.Empty;
		//	}
		//	if(value is Object) {
		//		unityReferences = new List<Object>() { value as Object };
		//		return string.Empty;
		//	}
		//	UnitySerializationUtility.DeserializeUnityObject
		//	return System.Text.Encoding.UTF8.GetString(SerializationUtility.SerializeValue(value, DataFormat.JSON, out unityReferences));
		//}

		//public static T DeserializeFromJson<T>(string json) {
		//	if(string.IsNullOrEmpty(json)) {
		//		return default(T);
		//	}
		//	return SerializationUtility.DeserializeValue<T>(System.Text.Encoding.UTF8.GetBytes(json), DataFormat.JSON);
		//}

		//public static T DeserializeFromJson<T>(string json, List<Object> unityReferences) {
		//	if(string.IsNullOrEmpty(json)) {
		//		return default(T);
		//	}
		//	return SerializationUtility.DeserializeValue<T>(System.Text.Encoding.UTF8.GetBytes(json), DataFormat.JSON, unityReferences);
		//}

		//public static object DeserializeFromJson(string json, List<Object> unityReferences, System.Type type = null) {
		//	if(string.IsNullOrEmpty(json)) {
		//		if(type != null && type.IsValueType) {
		//			//Ensure we create new value if the type is value type
		//			return ReflectionUtils.CreateInstance(type);
		//		}
		//		return null;
		//	}
		//	var data = System.Text.Encoding.UTF8.GetBytes(json);
		//	if(type.IsCastableTo(typeof(Object)) || data.Length == 0) {
		//		if(unityReferences != null && unityReferences.Count > 0) {
		//			return unityReferences[0];
		//		}
		//		return null;
		//	} else if(type != null) {
		//		//This will fix some incorrect type result for primitive type
		//		switch(type.FullName) {
		//			case "System.Char":
		//				return SerializationUtility.DeserializeValue<char>(data, DataFormat.JSON, unityReferences);
		//			case "System.Single":
		//				return SerializationUtility.DeserializeValue<float>(data, DataFormat.JSON, unityReferences);
		//			case "System.Int32":
		//				return SerializationUtility.DeserializeValue<int>(data, DataFormat.JSON, unityReferences);
		//			case "System.Int64":
		//				return SerializationUtility.DeserializeValue<long>(data, DataFormat.JSON, unityReferences);
		//			case "System.Byte":
		//				return SerializationUtility.DeserializeValue<byte>(data, DataFormat.JSON, unityReferences);
		//			case "System.Boolean":
		//				return SerializationUtility.DeserializeValue<bool>(data, DataFormat.JSON, unityReferences);
		//			case "System.String":
		//				return SerializationUtility.DeserializeValue<string>(data, DataFormat.JSON, unityReferences);
		//		}
		//	}
		//	return SerializationUtility.DeserializeValueWeak(data, DataFormat.JSON, unityReferences);
		//}
	}
}