using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	[Serializable]
	public class ScriptInformation {
		public string id;
		public string ghostID;
		public int startLine;
		public int startColumn;
		public int endLine;
		public int endColumn;

		public int lineRange => endLine - startLine;
		public int columnRange {
			get {
				if(startLine == endLine) {
					return endColumn - startColumn;
				} else {
					return (lineRange * 1000) + (endColumn - startColumn);
				}
			}
		}
	}
	
	[System.Serializable]
	public class ControlPin {
		public string name;
		public ControlPoint[] points;
	}

	[System.Serializable]
	public class ControlPoint {
		public Vector2 point;

		public ControlPoint() { }

		public ControlPoint(Vector2 point) {
			this.point = point;
		}
	}

	[System.Serializable]
	public class uNodeException : System.Exception {
		public const string KEY_REFERENCE = "[UNODE_GRAPH_REFERENCE]:";

		public UnityEngine.Object graphReference;

		public override string StackTrace {
			get {
				return base.StackTrace.AddFirst(KEY_REFERENCE + graphReference?.GetInstanceID()).AddLineInFirst();
			}
		}

		public uNodeException(UnityEngine.Object graphReference) {
			this.graphReference = graphReference;
		}
		public uNodeException(System.Exception inner, UnityEngine.Object graphReference) : base("", inner) { 
			this.graphReference = graphReference;
		}
		public uNodeException(string message, UnityEngine.Object graphReference) : base(message) {
			this.graphReference = graphReference;
		 }
		public uNodeException(string message, System.Exception inner, UnityEngine.Object graphReference) : base(message, inner) {
			this.graphReference = graphReference;
		}
		protected uNodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		public override string ToString() {
			string msg;
			if(string.IsNullOrEmpty(Message) && InnerException != null) {
				msg = InnerException.ToString();
			} else {
				msg = base.ToString();
			}
			if(graphReference != null) {
				msg = msg.AddLineInEnd().Add(KEY_REFERENCE + graphReference?.GetInstanceID());
			}
			return msg;
		}
	}

	[Serializable]
	public class SerializedType : ISerializationCallbackReceiver {
		[SerializeField]
		private string serializedString;
		[SerializeField]
		private byte[] serializedBytes;
		[SerializeField]
		public List<UnityEngine.Object> references;
		[SerializeField]
		private bool isNativeType;

		public SerializedType() {
			isNativeType = true;
		}

		public SerializedType(Type type) {
			this.type = type;
		}

		public bool isFilled {
			get {
				return isNativeType ? !string.IsNullOrEmpty(serializedString) : serializedBytes != null && serializedBytes.Length > 0;
			}
		}

		[NonSerialized]
		private Type _type;
		public Type type {
			get {
				if((_type == null || _type.Equals(null)) && isFilled) {
					if(isNativeType) {
						_type = TypeSerializer.Deserialize(serializedString, false);
					} else {
						var data = SerializerUtility.Deserialize<TypeData>(serializedBytes, references);
						_type = MemberDataUtility.GetParameterType(data, throwError:false);
					}
				}
				return _type;
			}
			set {
				if(value is RuntimeType) {
					isNativeType = false;
					serializedString = string.Empty;
					serializedBytes = SerializerUtility.Serialize(MemberDataUtility.GetTypeData(value), out references);
				} else {
					isNativeType = true;
					if(value != null) {
						serializedString = value.FullName;
					} else {
						serializedString = string.Empty;
					}
					serializedBytes = new byte[0];
					references = null;
				}
			}
		}

		public string typeName {
			get {
				if(isNativeType) {
					return serializedString;
				} else {
					return MemberDataUtility.GetParameterName(SerializerUtility.Deserialize<TypeData>(serializedBytes, references), null, null);
				}
			}
		}

		public string prettyName {
			get {
				if(isNativeType) {
					return type?.PrettyName() ?? serializedString;
				} else {
					return typeName;
				}
			}
		}

		public bool isNative => isNativeType;

		public TypeData GetTypeData() {
			if(isNativeType) {
				return new TypeData(serializedString);
			} else {
				return SerializerUtility.Deserialize<TypeData>(serializedBytes, references);
			}
		}

		public void SetTypeData(TypeData typeData) {
			if(typeData == null)
				throw new ArgumentNullException(nameof(typeData));
			if(typeData.isNative) {
				isNativeType = true;
				serializedString = typeData.name;
				serializedBytes = new byte[0];
				references = null;
			} else {
				isNativeType = false;
				serializedString = string.Empty;
				serializedBytes = SerializerUtility.Serialize(typeData, out references);
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize() {

		}

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
#if UNITY_EDITOR
			_type = null;
#endif
		}

		public static implicit operator SerializedType(Type type) {
            return new SerializedType(type);
        }
	}

	/// <summary>
	/// This class used to save type data
	/// </summary>
	public class TypeData {
		/// <summary>
		/// The Full Type Name or the name of referenced id.
		/// </summary>
		public string name = "";
		/// <summary>
		/// The unity reference objects
		/// </summary>
		public List<UnityEngine.Object> references;
		/// <summary>
		/// The list of type parameters
		/// </summary>
		public TypeData[] parameters;

		/// <summary>
		/// True if the type is native c# type
		/// </summary>
		public bool isNative {
			get {
				if(name != null && name.Length > 0) {
					if(name[0] == '#' || name[0] == '@' || name[0] == '$') {
						return false;
					}
				}
				return true;
			}
		}

		public TypeData() { }

		public TypeData(string name) {
			this.name = name;
		}

		public TypeData(string name, TypeData[] parameters) {
			this.name = name;
			this.parameters = parameters;
		}
	}

	/// <summary>
	/// Class for save type information.
	/// </summary>
	public class TypeInfo {
		public GenericParameterData parameterData;
		public System.Type type;

		public System.Type Type {
			get {
				if(parameterData != null) {
					return parameterData.value;
				}
				return type;
			}
		}

		public static implicit operator TypeInfo(System.Type type) {
			return new TypeInfo() {
				type = type,
			};
		}

		public static implicit operator System.Type(TypeInfo type) {
			return type.Type;
		}
	}

	/// <summary>
	/// This class used to save parameter data
	/// </summary>
	[System.Serializable]
	public class ParameterData {
		public enum RefKind {
			None,
			Ref,
			Out,
		}

		public string name;
		[Filter(OnlyGetType = true)]
		public MemberData type = new MemberData(typeof(object), MemberData.TargetType.Type);
		public RefKind refKind;
		public bool isByRef {
			get {
				return refKind != RefKind.None;
			}
		}
		public object value;

		public System.Type Type {
			get {
				if(type.isAssigned) {
					return type.Get<System.Type>();
				}
				return typeof(object);
			}
		}

		public ParameterData() { }
		public ParameterData(string name, System.Type type) {
			this.name = name;
			this.type = new MemberData(type, MemberData.TargetType.Type);
		}
	}

	/// <summary>
	/// This class used to save generic parameter data
	/// </summary>
	[System.Serializable]
	public class GenericParameterData {
		public string name;
		[Filter(OnlyGetType = true, ArrayManipulator = false, UnityReference = false, DisplaySealedType = false)]
		public MemberData typeConstraint = new MemberData(typeof(object), MemberData.TargetType.Type);

		private System.Type _value;
		public System.Type value {
			get {
				if(_value == null) {
					if(typeConstraint != null && typeConstraint.isAssigned) {
						return typeConstraint.Get<System.Type>();
					}
					return typeof(System.Object);
				}
				return _value;
			}
			set {
				_value = value;
			}
		}

		public GenericParameterData() { }
		public GenericParameterData(string name) {
			this.name = name;
		}
	}

	/// <summary>
	/// This class used to save attribute data
	/// </summary>
	[System.Serializable]
	public class AttributeData {
		[ObjectType(typeof(System.Attribute))]
		[Filter(DisplayAbstractType = false, ArrayManipulator = false, OnlyGetType = true)]
		public MemberData type = new MemberData(typeof(object));
		[ObjectType("type", isElementType = true)]
		public ValueData value;

		public Type Type => type.startType ?? typeof(Attribute);
	}

	/// <summary>
	/// This class used to save delegate data
	/// </summary>
	[System.Serializable]
	public class DelegateData {
		public string name;
		public EventModifier modifierData;
	}

	/// <summary>
	/// This class used to save class modifier
	/// </summary>
	[System.Serializable]
	public class ClassModifier : AccessModifier {
		[Hide("Static", true)]
		[Hide("Sealed", true)]
		public bool Abstract;
		[Hide("Abstract", true)]
		[Hide("Sealed", true)]
		public bool Static;
		[Hide("Abstract", true)]
		[Hide("Static", true)]
		public bool Sealed;

		private string GenerateAccessModifier() {
			if(Public) {
				return "public ";
			} else if(Private) {
				return string.Empty;
			} else if(Internal) {
				if(Protected) {
					return "protected internal ";
				}
				return "internal ";
			} else if(Protected) {
				return "protected ";
			}
			return string.Empty;
		}

		public override string GenerateCode() {
			string data = GenerateAccessModifier();
			if(Abstract) {
				data += "abstract ";
			}
			if(Sealed) {
				data += "sealed ";
			}
			if(Static) {
				data += "static ";
			}
			return data;
		}
	}

	/// <summary>
	/// This class used to save field modifier
	/// </summary>
	[System.Serializable]
	public class FieldModifier : AccessModifier {
		[Hide("Const", true)]
		public bool Static;
		[Hide("Const", true)]
		public bool Event;
		[Hide("Const", true)]
		public bool ReadOnly;
		[Hide("ReadOnly", true)]
		[Hide("Event", true)]
		[Hide("Static", true)]
		public bool Const;

		public override string GenerateCode() {
			string data = base.GenerateCode();
			if(Const) {
				data += "const ";
			} else {
				if(Static) {
					data += "static ";
				}
				if(Event) {
					data += "event ";
				}
				if(ReadOnly) {
					data += "readonly ";
				}
			}
			return data;
		}

		public static FieldModifier PrivateModifier {
			get {
				return new FieldModifier() {
					Private = true,
					Public = false,
				};
			}
		}

		public static FieldModifier InternalModifier {
			get {
				return new FieldModifier() {
					Private = false,
					Public = false,
					Internal = true,
				};
			}
		}

		public static FieldModifier ProtectedModifier {
			get {
				return new FieldModifier() {
					Private = false,
					Public = false,
					Internal = false,
					Protected = true,
				};
			}
		}

		public static FieldModifier ProtectedInternalModifier {
			get {
				return new FieldModifier() {
					Private = false,
					Public = false,
					Internal = true,
					Protected = true,
				};
			}
		}
	}

	/// <summary>
	/// This class used to save property modifier
	/// </summary>
	[System.Serializable]
	public class PropertyModifier : AccessModifier {
		[Hide("Static", true)]
		[Hide("Virtual", true)]
		public bool Abstract;
		[Hide("Abstract", true)]
		[Hide("Virtual", true)]
		public bool Static;
		[Hide("Static", true)]
		[Hide("Abstract", true)]
		public bool Virtual;
		public bool Override;

		public override string GenerateCode() {
			string data = base.GenerateCode();
			if(Static) {
				data += "static ";
			}
			if(Abstract) {
				data += "abstract ";
			} else if(Virtual) {
				data += "virtual ";
			} else if(Override) {
				data += "override ";
			}
			return data;
		}
	}

	/// <summary>
	/// This class used to save indexer modifier
	/// </summary>
	[System.Serializable]
	public class IndexerModifier : AccessModifier {

	}

	/// <summary>
	/// This class used to save function modifier
	/// </summary>
	[System.Serializable]
	public class FunctionModifier : AccessModifier {
		[Hide("Abstract", true)]
		[Hide("Unsafe", true)]
		[Hide("Virtual", true)]
		public bool Static;
		[Hide("Abstract", true)]
		[Hide("Static", true)]
		[Hide("Virtual", true)]
		public bool Unsafe;
		[Hide("Abstract", true)]
		[Hide("Static", true)]
		[Hide("Unsafe", true)]
		public bool Virtual;
		[Hide("Unsafe", true)]
		[Hide("Virtual", true)]
		[Hide("Static", true)]
		public bool Abstract;

		[Hide("Static", false)]
		public bool Extern;
		[Hide("Unsafe", true)]
		[Hide("Partial", true)]
		[Hide("Virtual", true)]
		[Hide("Override", true)]
		public bool New;
		[Hide("Unsafe", true)]
		[Hide("Static", true)]
		[Hide("Partial", true)]
		[Hide("Virtual", true)]
		[Hide("New", true)]
		public bool Override;
		[Hide("Unsafe", true)]
		[Hide("Static", true)]
		[Hide("Abstract", true)]
		[Hide("Virtual", true)]
		[Hide("Extern", true)]
		[Hide("New", true)]
		[Hide("Override", true)]
		[Hide("Sealed", true)]
		[Hide("Public", true)]
		[Hide("Private", true)]
		[Hide("Protected", true)]
		[Hide("Internal", true)]
		public bool Partial;
		[Hide("Unsafe", true)]
		[Hide("Static", true)]
		[Hide("Abstract", true)]
		[Hide("Virtual", true)]
		[Hide("New", true)]
		public bool Sealed;
		[Hide("Unsafe", true)]
		[Hide("Static", true)]
		[Hide("Partial", true)]
		public bool Async;

		public override string GenerateCode() {
			string data = base.GenerateCode();
			if(Static) {
				data += "static ";
				if(Extern) {
					data += " extern ";
				}
			} else if(Unsafe) {
				data += "unsave ";
			} else if(Virtual) {
				data += "virtual ";
			} else if(Abstract) {
				data += "abstract ";
			} else if(Override) {
				data += "override ";
			} else if(Async) {
				data += "async ";
			} else if(Partial && string.IsNullOrEmpty(data)) {
				data += "partial ";
			}
			if(Sealed) {
				data = data.Insert(0, "sealed ");
			} else if(New) {
				data = data.Insert(0, "new ");
			}
			return data;
		}
	}

	/// <summary>
	/// This class used to save constructor modifier
	/// </summary>
	[System.Serializable]
	public class ConstructorModifier : AccessModifier {

	}

	/// <summary>
	/// This class used to save event modifier
	/// </summary>
	[System.Serializable]
	public class EventModifier : AccessModifier {
		public bool Abstract;

		[Hide("Unsafe", true)]
		[Hide("Virtual", true)]
		public bool Static;
		[Hide("Static", true)]
		[Hide("Virtual", true)]
		public bool Unsafe;
		[Hide("Static", true)]
		[Hide("Unsafe", true)]
		public bool Virtual;
	}

	/// <summary>
	/// This class used to save operator modifier
	/// </summary>
	[System.Serializable]
	public class OperatorModifier : AccessModifier {
		[Hide("Unsafe", true)]
		[Hide("Virtual", true)]
		public bool Static;
		[Hide("Static", true)]
		[Hide("Virtual", true)]
		public bool Unsafe;
		[Hide("Static", true)]
		[Hide("Unsafe", true)]
		public bool Virtual;

		public override string GenerateCode() {
			string data = base.GenerateCode().Add(" ");
			if(Static) {
				return data += "static";
			} else if(Unsafe) {
				return data += "unsave";
			} else if(Virtual) {
				return data += "virtual";
			}
			return data;
		}
	}

	/// <summary>
	/// This class used to save access modifier
	/// </summary>
	[System.Serializable]
	public class AccessModifier {
		[Hide("Private", true)]
		[Hide("Internal", true)]
		[Hide("Protected", true)]
		public bool Public = true;
		[Hide("Public", true)]
		[Hide("Internal", true)]
		[Hide("Protected", true)]
		public bool Private;
		[Hide("Public", true)]
		[Hide("Private", true)]
		public bool Protected;
		[Hide("Public", true)]
		[Hide("Private", true)]
		public bool Internal;

		public bool isPublic => Public;
		public bool isPrivate => Private || !Public && !Protected;
		public bool isProtected => Protected;

		public void SetPublic() {
			Public = true;
			Private = false;
			Protected = false;
			Internal = false;
		}

		public void SetPrivate() {
			Public = false;
			Private = true;
			Protected = false;
			Internal = false;
		}

		public void SetProtected() {
			Public = false;
			Private = false;
			Protected = true;
			Internal = false;
		}

		public void SetProtectedInternal() {
			Public = false;
			Private = false;
			Protected = true;
			Internal = true;
		}

		public virtual string GenerateCode() {
			if(Public) {
				return "public ";
			} else if(Private) {
				return "private ";
			} else if(Internal) {
				if(Protected) {
					return "protected internal ";
				}
				return "internal ";
			} else if(Protected) {
				return "protected ";
			}
			return "private ";
		}
	}

	/// <summary>
	/// This class used to save enum data
	/// </summary>
	[System.Serializable]
	public class EnumData {
		[System.Serializable]
		public class Element {
			public string name;
		}
		public string name;
		public Element[] enumeratorList = new Element[0];
		[Filter(typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), Inherited = false, OnlyGetType = true, UnityReference = false)]
		public MemberData inheritFrom = new MemberData(typeof(int), MemberData.TargetType.Type);
		public EnumModifier modifiers = new EnumModifier();
	}

	[System.Serializable]
	public class EnumModifier : AccessModifier {

	}
}