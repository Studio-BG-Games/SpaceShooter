using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MaxyGames.uNode;
using Object = UnityEngine.Object;

namespace MaxyGames {
	/// <summary>
	/// Class for get, set, or invoke a fields, properties, method/function, constructor, event/delegates, uNode Component.
	/// </summary>
	[Serializable]
	public class MemberData : IValue, IGetValue, ISetValue, ISerializationCallbackReceiver {
		#region Classes
		public class ItemData {
			public TypeData[] parameters;
			public TypeData[] genericArguments;
		}

		public class Event {
			public readonly EventInfo eventInfo;
			public readonly object instance;

			public Event(EventInfo eventInfo, object instance) {
				this.eventInfo = eventInfo;
				this.instance = instance;
			}
		}

		public delegate object EventCallback(object[] arg);

		public static class Utilities {
			private static void InitializeArguments(MemberData member, out Type[][] parameters, out Type[][] genericParameters, bool throwError = true) {
				if(member.SerializedItems != null) {
					Type[][] paramTypes = new Type[member.SerializedItems.Length][];
					Type[][] genericTypes = new Type[member.SerializedItems.Length][];
					for(int i = 0; i < member.SerializedItems.Length; i++) {
						ItemData iData = member.Items[i];
						if(iData != null) {
							try {
								Type[] paramsType;
								Type[] genericType;
								MemberDataUtility.DeserializeMemberItem(member.Items[i],
									member.targetReference,
									out genericType,
									out paramsType,
									throwError);
								paramTypes[i] = paramsType;
								genericTypes[i] = genericType;
							}
							catch {
								if(uNodeUtility.isPlaying) {
									throw;
								}
								throw;
								// return null;
							}
						}
					}
					parameters = paramTypes;
					genericParameters = genericTypes;
					return;
				}
				parameters = null;
				genericParameters = null;
			}

			public static Type[][] GetParameterTypes(MemberData member) {
				if(member._parameterTypes == null) {
					InitializeArguments(member, out member._parameterTypes, out member._genericTypes);
				}
				return member._parameterTypes;
			}

			public static Type[][] GetGenericTypes(MemberData member) {
				if(member._genericTypes == null) {
					InitializeArguments(member, out member._parameterTypes, out member._genericTypes);
				}
				return member._genericTypes;
			}

			public static Type[][] SafeGetGenericTypes(MemberData member) {
				if(member._genericTypes == null && member.SerializedItems != null) {
					bool cacheValue = true;
					InitializeArguments(member, out var paramTypes, out var genericTypes, false);
					if(paramTypes != null) {
						for(int x = 0; x < paramTypes.Length; x++) {
							if(!cacheValue)
								break;
							if(paramTypes[x] != null) {
								var types = paramTypes[x];
								for(int y = 0; y < types.Length; y++) {
									if(types[y] is MissingType) {
										cacheValue = false;
										break;
									}
								}
							}
						}
					}
					if(genericTypes != null) {
						for(int x = 0; x < genericTypes.Length; x++) {
							if(!cacheValue)
								break;
							if(genericTypes[x] != null) {
								var types = genericTypes[x];
								for(int y = 0; y < types.Length; y++) {
									if(types[y] is MissingType) {
										cacheValue = false;
										break;
									}
								}
							}
						}
					}
					if(cacheValue) {
						member._parameterTypes = paramTypes;
						member._genericTypes = genericTypes;
					} else {
						return genericTypes;
					}
				}
				return member._genericTypes;
			}

			public static Type[][] SafeGetParameterTypes(MemberData member) {
				if(member._parameterTypes == null && member.SerializedItems != null) {
					bool cacheValue = true;
					InitializeArguments(member, out var paramTypes, out var genericTypes, false);
					if(paramTypes != null) {
						for(int x = 0; x < paramTypes.Length; x++) {
							if(!cacheValue)
								break;
							if(paramTypes[x] != null) {
								var types = paramTypes[x];
								for(int y = 0; y < types.Length; y++) {
									if(types[y] is MissingType) {
										cacheValue = false;
										break;
									}
								}
							}
						}
					}
					if(genericTypes != null) {
						for(int x = 0; x < genericTypes.Length; x++) {
							if(!cacheValue)
								break;
							if(genericTypes[x] != null) {
								var types = genericTypes[x];
								for(int y = 0; y < types.Length; y++) {
									if(types[y] is MissingType) {
										cacheValue = false;
										break;
									}
								}
							}
						}
					}
					if(cacheValue) {
						member._parameterTypes = paramTypes;
						member._genericTypes = genericTypes;
					} else {
						return paramTypes;
					}
				}
				return member._parameterTypes;
			}
		}
		#endregion

		#region Enum
		[Flags]
		public enum TargetType {
			//No target
			None = 0,
			//Null target
			Null = 1 << 0,
			//target is Serialized Values.
			Values = 1 << 1,
			//The target is a instance value.
			SelfTarget = 1 << 2,
			//field target - using reflection
			Field = 1 << 3,
			//property target - using reflection
			Property = 1 << 4,
			//method target - using reflection
			Method = 1 << 5,
			//constructor target - using reflection
			Constructor = 1 << 6,
			//event target - using reflection
			Event = 1 << 7,
			//target is a type
			Type = 1 << 8,
			//VariableData target (Using reflection when in deep variable otherwise use direct get and set)
			uNodeVariable = 1 << 9,
			//VariableData target from GroupNode (Using reflection when in deep variable otherwise use direct get and set)
			uNodeGroupVariable = 1 << 10,
			//target is Function in uNode (Runtime, Class, or Struct)
			uNodeFunction = 1 << 11,
			//target is Property in uNode (Runtime, Class, or Struct)
			uNodeProperty = 1 << 12,
			//target is Constructor in uNode (Runtime, Class, or Struct)
			uNodeConstructor = 1 << 13,
			//target is Indexer in uNode (Runtime, Class, or Struct)
			uNodeIndexer = 1 << 14,
			//target is Parameter in uNode (Function, Constructor, or Indexer)
			uNodeParameter = 1 << 15,
			//target is GenericParameter in uNode (Function, or Class)
			uNodeGenericParameter = 1 << 16,
			//target is ValueNode to Get.
			ValueNode = 1 << 17,
			//target is uNode Type.
			uNodeType = 1 << 18,
			//target is Node to call.
			FlowNode = 1 << 19,
			//target is uNodeRoot LocalVariable.
			uNodeLocalVariable = 1 << 20,
			//target is node field
			NodeField = 1 << 21,
			//target is node field in element
			NodeFieldElement = 1 << 22,
			//target is flow pin input
			FlowInput = 1 << 23,
			//target is runtime member (variable, property or method)
			RuntimeMember = 1 << 24,
			//target is node output value that are implement IExtendedOutput
			NodeOutputValue = 1 << 25,
			//target is node input value that are implement IExtendedInput
			FlowInputExtended = 1 << 26,
		}
		#endregion

		#region Variable
		/// <summary>
		/// Indicate what the target type is
		/// </summary>
		public TargetType targetType;
		[SerializeField]
		internal OdinSerializedData odinInstanceData;
		[SerializeField]
		internal OdinSerializedData odinTargetData;

		[SerializeField]
		private SerializedType startSerializedType;
		[SerializeField]
		private SerializedType targetSerializedType;

		public SerializedType StartSerializedType {
			get {
				if(startSerializedType == null) {
					startSerializedType = new SerializedType();
				}
				return startSerializedType;
			}
			set {
				startSerializedType = value;
			}
		}

		public SerializedType TargetSerializedType {
			get {
				if(targetSerializedType == null) {
					targetSerializedType = new SerializedType();
				}
				return targetSerializedType;
			}
			set {
				targetSerializedType = value;
			}
		}

		[SerializeField]
		private string _name;
		/// <summary>
		/// Indicate start target is static
		/// </summary>
		public bool isStatic;

		[SerializeField]
		private OdinSerializedData[] serializedItems;

		/// <summary>
		/// List of UnityObject Reference for targeting member.
		/// </summary>
		public List<UnityEngine.Object> targetReference = new List<UnityEngine.Object>();
		/// <summary>
		/// The control point for connection.
		/// </summary>
		public ControlPoint[] controlPoints;

		public static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		#endregion

		#region Properties
		/// <summary>
		/// The object containing the member.
		/// </summary>
		public object instance {
			get {
				if(uNodeUtility.isInEditor && (_cachedInstanceData != odinInstanceData || _instance == null ||
					_instance is UnityEngine.Object && !(_instance as UnityEngine.Object))) {
					_hasInitializeInstance = false;
					_cachedInstanceData = odinInstanceData;
				}
				if(_hasInitializeInstance) {
					return _instance;
				}
				if(targetType == TargetType.Values || targetType == TargetType.SelfTarget) {
					if(startType != null && startType.IsValueType) {
						_hasInitializeInstance = true;
						_instance = SerializerUtility.Deserialize(odinInstanceData);
						return _instance;
					}
				}
				object obj = SerializerUtility.Deserialize(odinInstanceData);
				if(object.ReferenceEquals(obj, null) || obj.GetType().IsValueType || obj is UnityEngine.Object || obj is string || obj is MemberData) {
					_hasInitializeInstance = true;
					_instance = obj;
				}
				return obj;
			}
			set {
				_instance = null;
				//if(IsRequiredInstance()) {
				odinInstanceData = OdinSerializedData.CreateFrom(value);
				//} else {
				//	odinInstanceData = OdinSerializedData.CreateFrom<object>(null);
				//}
				_hasInitializeInstance = false;
			}
		}

		/// <summary>
		/// True if the member required instance to perform an action
		/// </summary>
		/// <returns></returns>
		public bool IsRequiredInstance() {
			switch(targetType) {
				case TargetType.Constructor:
				case TargetType.None:
				case TargetType.Null:
				case TargetType.Type:
				case TargetType.uNodeConstructor:
				case TargetType.uNodeType:
					return false;
				case TargetType.Event:
				case TargetType.Field:
				case TargetType.Method:
				case TargetType.Property:
					return !isStatic;
			}
			return true;
		}

		/// <summary>
		/// Get preferred instance of this member.
		/// </summary>
		/// <returns></returns>
		public object GetInstance() {
			object obj = instance;
			if(obj is MemberData) {
				MemberData member = obj as MemberData;
				if(member.targetType == TargetType.Values || member.targetType == TargetType.SelfTarget) {
					return (obj as MemberData).Get();
				}
			} else if(obj is BaseValueData) {
				return (obj as BaseValueData).Get();
			}
			return obj;
		}

		/// <summary>
		/// The object that required to perform action, this return StartType if target is static member.
		/// if target is MemberData or BaseValueData it will try to call Get() from the target instance.
		/// </summary>
		public object startTarget {
			get {
				if(!object.ReferenceEquals(_startTarget, null))
					return _startTarget;
				if(isStatic && targetType != TargetType.uNodeGenericParameter || targetType == TargetType.Constructor) {
					_startTarget = startType;
					_oldStartType = startType;
				} else {
					object obj = instance;
					if(obj != null) {
						_oldStartType = obj.GetType();
						if(obj is IGetValue) {
							obj = (obj as IGetValue).Get();
						}
						if(!IsTargetingUNode && !(obj is INode))
							obj = AutoConvertValue(obj, startType);
					}
					return obj;
				}
				return _startTarget;
			}
			set {
				if(_oldStartType != null && !object.ReferenceEquals(value, null)) {
					if(value.GetType() != _oldStartType && !value.GetType().IsSubclassOf(_oldStartType)) {
						ResetCache();
						_oldStartType = value.GetType();
					}
				}
				_startTarget = value;
			}
		}

		/// <summary>
		/// The start name of this member
		/// </summary>
		public string startName {
			get {
				if(namePath != null) {
					return namePath[0];
				}
				return name;
			}
			set {
				string[] str = namePath;
				if(str == null || str.Length == 0) {
					name = value;
					return;
				}
				str[0] = value;
				name = string.Join(".", str);
			}
		}

		/// <summary>
		/// The name of the member.
		/// </summary>
		public string name {
			get { return _name; }
			set { _name = value; isReflected = false; ResetCache(); }
		}

		/// <summary>
		/// The Display name for this member
		/// </summary>
		/// <param name="longName"></param>
		/// <returns></returns>
		public string DisplayName(bool longName = false, bool typeTargetWithTypeof = true) {
			switch(targetType) {
				case TargetType.None:
					return "(none)";
				case TargetType.SelfTarget:
					return "this";
				case TargetType.Null:
					return "null";
				case TargetType.uNodeVariable:
				case TargetType.uNodeLocalVariable:
				case TargetType.uNodeProperty:
				case TargetType.uNodeGroupVariable:
					if(longName)
						break;
					return "$" + _name;
				case TargetType.uNodeFunction:
				case TargetType.uNodeIndexer:
					if(longName)
						break;
					return _name;
				case TargetType.uNodeConstructor:
					if(longName)
						break;
					return "new" + _name;
				case TargetType.Values:
					if(type != null) {
						return "v: " + type.PrettyName();
					}
					return "Missing Type";
				case TargetType.Constructor:
					if(longName)
						break;
					if(type != null) {
						return "new " + type.PrettyName() + "()";
					}
					return !string.IsNullOrEmpty(_name) ? _name : "ctor";
				case TargetType.Method:
					if(longName)
						break;
					return !string.IsNullOrEmpty(_name) ? _name : "()";
				case TargetType.FlowNode:
				case TargetType.ValueNode:
					if(GetTargetNode() == null) {
						goto case MemberData.TargetType.None;
					}
					return "#Node";
				case TargetType.NodeField:
				case TargetType.NodeFieldElement:
				case TargetType.FlowInput:
				case TargetType.NodeOutputValue:
				case TargetType.FlowInputExtended:
					if(GetTargetNode() == null) {
						goto case MemberData.TargetType.None;
					}
					return "#Port";

			}
			string[] names = namePath;
			if(string.IsNullOrEmpty(_displayName) && isTargeted && SerializedItems != null && SerializedItems.Length > 0) {
				string result = null;
				if(SerializedItems.Length == names.Length) {
					if(targetType == TargetType.Constructor) {
						result += "new " + type.PrettyName();
					}
					for(int i = 0; i < names.Length; i++) {
						if(i != 0 && (targetType != TargetType.Constructor)) {
							result += ".";
						}
						if(targetType != TargetType.uNodeGenericParameter && targetType != TargetType.Type && targetType != TargetType.Constructor) {
							if(i == 0) {
								switch(targetType) {
									case TargetType.uNodeVariable:
									case TargetType.uNodeGroupVariable:
									case TargetType.uNodeLocalVariable:
									case TargetType.uNodeProperty:
									case TargetType.uNodeParameter:
										result += "$" + names[i];
										break;
									default:
										if(startType is RuntimeType runtimeType) {
											startName = runtimeType.Name;
											result += runtimeType.PrettyName();
										} else {
											result += names[i];
										}
										break;
								}
							} else {
								result += names[i];
							}
						}
						ItemData iData = Items[i];
						if(iData != null) {
							string[] paramsType;
							string[] genericType;
							MemberDataUtility.GetItemName(Items[i],
								targetReference,
								out genericType,
								out paramsType);
							if(genericType.Length > 0) {
								for(int x = 0; x < genericType.Length; x++) {
									genericType[x] = genericType[x].PrettyName();
								}
								if(targetType != TargetType.uNodeGenericParameter && targetType != TargetType.Type) {
									result += string.Format("<{0}>", string.Join(", ", genericType));
								} else {
									result += string.Format("{0}", string.Join(", ", genericType));
									if(names[i].Contains("[")) {
										bool valid = false;
										for(int x = 0; x < names[i].Length; x++) {
											if(!valid) {
												if(names[i][x] == '[') {
													valid = true;
												}
											}
											if(valid) {
												result += names[i][x];
											}
										}
									}
								}
							}
							if(paramsType.Length > 0 ||
								targetType == TargetType.uNodeFunction ||
								targetType == TargetType.uNodeConstructor ||
								targetType == TargetType.Constructor ||
								targetType == TargetType.Method && !isDeepTarget) {
								for(int x = 0; x < paramsType.Length; x++) {
									paramsType[x] = paramsType[x].PrettyName();
								}
								result += string.Format("({0})", string.Join(", ", paramsType));
							}
						}
					}
				}
				_displayName = result;
			}
			if(!string.IsNullOrEmpty(_displayName)) {
				if(targetType.IsTargetingType()) {
					if(!typeTargetWithTypeof) {
						return _displayName;
					}
					return "typeof(" + _displayName + ")";
				}
				return _displayName;
			}
			if(targetType.IsTargetingType()) {
				if(!typeTargetWithTypeof) {
					return startType.PrettyName();
				}
				return "typeof(" + startType.PrettyName() + ")";
			}
			return !string.IsNullOrEmpty(_name) ? _name : "(none)";
		}

		public string Tooltip {
			get {
				if(isAssigned) {
					switch(targetType) {
						case TargetType.None:
						case TargetType.Type:
							return name +
								"\nTarget	: " + targetType.ToString() +
								"\nType	: " + typeof(System.Type).PrettyName(true);
						default:
							Type t = type;
							if(t == null) {
								return DisplayName(true) +
									"\nTarget	: " + targetType.ToString() +
									"\nType	: " + "Missing Type";
							}
							return DisplayName(true) +
								"\nTarget	: " + targetType.ToString() +
								"\nType	: " + type.PrettyName(true);
					}
				}
				return "Unassigned";
			}
		}

		public string[] namePath {
			get {
				if(_namePath == null || _namePath.Length == 0) {
					if(name != null) {
						_namePath = name.Split(new char[] { '.' });
					} else {
						_namePath = new string[0];
					}
				}
				return _namePath;
			}
		}

		/// <summary>
		/// Indicates whether the reflection has been found and cached.
		/// </summary>
		public bool isReflected { get; private set; }
		public bool isDeepTarget {
			get {
				if(IsTargetingUNode) {
					return namePath != null && namePath.Length > 1;
				}
				return namePath != null && namePath.Length > 2;
			}
		}

		/// <summary>
		/// Indicates whether the member has been targeted.
		/// </summary>
		public bool isTargeted {
			get {
				switch(targetType) {
					case TargetType.None:
						return false;
					case TargetType.uNodeGenericParameter:
					case TargetType.Type:
					case TargetType.Null:
					case TargetType.Values:
						return true;
					case TargetType.SelfTarget:
					case TargetType.ValueNode:
					case TargetType.FlowNode:
						return instance != null;
					default:
						return !string.IsNullOrEmpty(name);
				}
			}
		}

		/// <summary>
		/// Indicates whether the member has been properly assigned.
		/// </summary>
		public bool isAssigned {
			get {
				switch(targetType) {
					case TargetType.None:
						return false;
					case TargetType.Null:
					case TargetType.Values:
						return true;
					case TargetType.SelfTarget:
						return instance != null;
					case TargetType.ValueNode:
					case TargetType.FlowNode:
					case TargetType.NodeField:
					case TargetType.FlowInput:
					case TargetType.NodeOutputValue:
					case TargetType.FlowInputExtended:
						return GetTargetNode() != null;
					case TargetType.NodeFieldElement:
						if(string.IsNullOrEmpty(startName)) {
							return false;
						}
						return GetTargetNode() != null && GetAccessIndex() >= 0;
					case TargetType.uNodeType:
						return startType != null;
					case TargetType.uNodeGenericParameter:
					case TargetType.Type:
					case TargetType.Constructor:
					default:
						return !string.IsNullOrEmpty(name);
				}
			}
		}

		/// <summary>
		/// Indicate is targeting node.
		/// </summary>
		public bool IsTargetingNode => targetType.IsTargetingNode();

		/// <summary>
		/// Indicate is targeting pin or node.
		/// </summary>
		public bool IsTargetingPortOrNode => targetType.IsTargetingPortOrNode();

		/// <summary>
		/// Indicate is targeting uNode Member whether it's graph, pin, or nodes.
		/// </summary>
		public bool IsTargetingUNode => targetType.IsTargetingUNode();

		/// <summary>
		/// True if targetType is uNodeVariable or uNodeLocalVariable
		/// </summary>
		/// <value></value>
		public bool IsTargetingVariable => targetType.IsTargetingVariable();

		/// <summary>
		/// The target is targeting graph that's return a value except targeting pin or nodes.
		/// </summary>
		/// <returns></returns>
		public bool IsTargetingGraph => targetType.IsTargetingGraphValue();

		/// <summary>
		/// True if targetType is Type or uNodeType
		/// </summary>
		/// <value></value>
		public bool IsTargetingType => targetType.IsTargetingType();

		/// <summary>
		///  if targetType is Values, SelfTarget, or Null
		/// </summary>
		public bool IsTargetingValue => targetType.IsTargetingValue();

		/// <summary>
		/// True if targetType is Constructor, Event, Field, Property, or Method
		/// </summary>
		/// <returns></returns>
		public bool IsTargetingReflection => targetType.IsTargetingReflection();

		/// <summary>
		/// The Items currently have
		/// </summary>
		public ItemData[] Items {
			get {
				if(_items == null && SerializedItems != null) {
					_items = new ItemData[SerializedItems.Length];
					for(int i = 0; i < SerializedItems.Length; i++) {
						_items[i] = SerializerUtility.Deserialize<ItemData>(SerializedItems[i]);
					}
				}
				return _items;
			}
		}

		/// <summary>
		/// List of Items serialized data.
		/// Note: you need deserialize this in order to get correct value.
		/// </summary>
		public OdinSerializedData[] SerializedItems {
			get {
				return serializedItems ?? (serializedItems = new OdinSerializedData[0]);
			}
			set {
				_items = null;
				serializedItems = value;
			}
		}

		/// <summary>
		/// The all parameters type this member have, null if does not.
		/// </summary>
		public Type[][] ParameterTypes {
			get {
				return Utilities.GetParameterTypes(this);
			}
		}

		/// <summary>
		/// The all generic type this member have, null if does not.
		/// </summary>
		public Type[][] GenericTypes {
			get {
				return Utilities.GetGenericTypes(this);
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Create new member
		/// </summary>
		public MemberData() { }

		/// <summary>
		/// Create new member
		/// </summary>
		/// <param name="members"></param>
		public MemberData(IList<MemberInfo> members) {
			if(members == null) {
				throw new ArgumentNullException("members");
			}
			if(!(members[0] is Type)) {
				MemberInfo[] memberInfos = new MemberInfo[members.Count + 1];
				for(int i = 0; i < members.Count; i++) {
					memberInfos[i + 1] = members[i];
				}
				memberInfos[0] = members[0].DeclaringType;
				members = memberInfos;
			}
			var lastMember = members[members.Count - 1];
			isStatic = ReflectionUtils.GetMemberIsStatic(lastMember);
			List<ItemData> itemDatas = new List<ItemData>();
			foreach(var m in members) {
				if(m == null)
					throw new NullReferenceException("There's some null members");
				if(!string.IsNullOrEmpty(name)) {
					name += ".";
				}
				name += m.Name;
				itemDatas.Add(MemberDataUtility.GetItemDataFromMemberInfo(m));
			}
			type = ReflectionUtils.GetMemberType(lastMember);
			startType = ReflectionUtils.GetMemberType(members[0]);
			if(lastMember is FieldInfo) {
				targetType = TargetType.Field;
			} else if(lastMember is PropertyInfo) {
				targetType = TargetType.Property;
			} else if(lastMember is MethodInfo) {
				targetType = TargetType.Method;
			} else if(lastMember is ConstructorInfo) {
				targetType = TargetType.Constructor;
				isStatic = true;
			} else if(lastMember is EventInfo) {
				targetType = TargetType.Event;
			} else if(lastMember is Type) {
				targetType = TargetType.Type;
				isStatic = true;
			}
			if(members[0] is RuntimeType) {
				startType = members[0] as Type;
			}
			if(!isStatic && members.Count > 1) {
				for(int i = 1; i < members.Count; i++) {
					if(i + 1 < members.Count && !(members[i] is Type)) {
						isStatic = ReflectionUtils.GetMemberIsStatic(members[i]);
						break;
					}
				}
			}
			while(namePath.Length > itemDatas.Count) {
				itemDatas.Insert(0, null);
			}
			serializedItems = itemDatas.Select(i => SerializerUtility.SerializeValue(i)).ToArray();
		}

		/// <summary>
		/// Create new member
		/// </summary>
		/// <param name="memberInfo"></param>
		public MemberData(MemberInfo memberInfo) {
			if(memberInfo == null) {
				throw new ArgumentNullException("memberInfo");
			}
			if(memberInfo is Type) {
				Type type = memberInfo as Type;
				name = type.PrettyName();
				startType = type;
				targetType = type is RuntimeType ? TargetType.uNodeType : TargetType.Type;
				isStatic = true;
			} else {
				if(memberInfo.MemberType == MemberTypes.Field) {
					FieldInfo field = memberInfo as FieldInfo;
					startType = field.DeclaringType;
					type = field.FieldType;
					name = field.DeclaringType.Name + "." + field.Name;
					isStatic = field.IsStatic;
					targetType = TargetType.Field;
				} else if(memberInfo.MemberType == MemberTypes.Property) {
					PropertyInfo property = memberInfo as PropertyInfo;
					startType = property.DeclaringType;
					type = property.PropertyType;
					name = property.DeclaringType.Name + "." + property.Name;
					if(property.GetGetMethod() != null) {
						isStatic = property.GetGetMethod().IsStatic;
					} else if(property.GetSetMethod() != null) {
						isStatic = property.GetSetMethod().IsStatic;
					}
					targetType = TargetType.Property;
				} else if(memberInfo.MemberType == MemberTypes.Method) {
					MethodInfo method = memberInfo as MethodInfo;
					startType = method.DeclaringType;
					type = method.ReturnType;
					name = method.DeclaringType.Name + "." + method.Name;
					isStatic = method.IsStatic;
					targetType = TargetType.Method;
					if(method.GetParameters().Length > 0 || method.IsGenericMethod) {
						List<OdinSerializedData> iDataList = new List<OdinSerializedData>();
						iDataList.Add(SerializerUtility.SerializeValue(MemberDataUtility.GetItemDataFromMemberInfo(memberInfo)));
						while(namePath.Length > iDataList.Count) {
							iDataList.Insert(0, null);
						}
						serializedItems = iDataList.ToArray();
					}
				} else if(memberInfo.MemberType == MemberTypes.Event) {
					EventInfo eventInfo = memberInfo as EventInfo;
					startType = eventInfo.DeclaringType;
					type = eventInfo.EventHandlerType;
					name = memberInfo.DeclaringType.Name + "." + eventInfo.Name;
					isStatic = eventInfo.GetAddMethod().IsStatic;
					targetType = TargetType.Event;
				} else if(memberInfo.MemberType == MemberTypes.Constructor) {
					ConstructorInfo ctor = memberInfo as ConstructorInfo;
					name = ctor.DeclaringType.Name + ".ctor";
					startType = ctor.DeclaringType;
					type = ctor.DeclaringType;
					isStatic = true;
					targetType = TargetType.Constructor;
					if(ctor.GetParameters().Length > 0) {
						List<OdinSerializedData> iDataList = new List<OdinSerializedData>();
						iDataList.Add(SerializerUtility.SerializeValue(MemberDataUtility.GetItemDataFromMemberInfo(memberInfo)));
						while(namePath.Length > iDataList.Count) {
							iDataList.Insert(0, null);
						}
						serializedItems = iDataList.ToArray();
						return;
					}
				} else {
					throw new Exception("Unsupported MemberType:" + memberInfo.MemberType.ToString());
				}
			}
			var itemData = new List<OdinSerializedData>() { SerializerUtility.SerializeValue(MemberDataUtility.GetItemDataFromMemberInfo(memberInfo)) };
			while(namePath.Length > itemData.Count) {
				itemData.Insert(0, null);
			}
			serializedItems = itemData.ToArray();
		}

		/// <summary>
		/// Create new member
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="instance"></param>
		/// <param name="targetType"></param>
		public MemberData(VariableData variable, object instance, TargetType targetType = TargetType.uNodeVariable) {
			this.name = variable.Name;
			this.targetType = targetType;
			startType = variable.type.Get<Type>();
			isStatic = false;
			this.instance = instance;
		}

		public MemberData(uNodeProperty property, IPropertySystem instance = null) {
			name = property.Name;
			type = property.ReturnType();
			startType = typeof(MonoBehaviour);
			isStatic = false;
			targetType = TargetType.uNodeProperty;
			this.instance = instance ?? property.owner;
		}

		/// <summary>
		/// Create new member
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="targetType"></param>
		public MemberData(string name, Type type, object instance, TargetType targetType) {
			this.name = name;
			this.targetType = targetType;
			startType = type;
			this.instance = instance;
		}

		/// <summary>
		/// Create new member
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		public MemberData(object value, TargetType targetType = TargetType.Values) {
			if(value != null) {
				this.targetType = targetType;
				if(targetType == TargetType.Values) {
					name = string.Empty;
					odinTargetData = OdinSerializedData.CreateFrom(value);
					startType = value.GetType();
					if(value is UnityEngine.Object) {
						if(value is NodeComponent) {
							startType = typeof(NodeComponent);
						} else if(value is uNodeRoot) {
							startType = typeof(uNodeRoot);
						}
					}
				} else if(targetType == TargetType.SelfTarget) {
					if(value != null) {
						startType = value.GetType();
					}
					name = "self";
					instance = value;
				} else if(targetType == TargetType.Type) {
					if(value is Type) {
						name = (value as Type).PrettyName();
						startType = value as Type;
						isStatic = true;
						if(value is RuntimeType) {
							this.targetType = TargetType.uNodeType;
						}
					} else if(value is string) {
						var t = TypeSerializer.Deserialize(value as string);
						name = t.PrettyName();
						startType = t;
						isStatic = true;
					} else if(value is MemberData) {
						object o = (value as MemberData).Get();
						if(o is Type) {
							name = (o as Type).PrettyName();
							startType = (o as Type);
							isStatic = true;
						} else if(o is string) {
							Type t = TypeSerializer.Deserialize(o as string);
							name = t.PrettyName();
							startType = t;
							isStatic = true;
						}
					} else {
						throw new Exception("Invalid value to create type member.\nThe value should be System.Type or string type\nType:" + value.GetType());
					}
				} else if(targetType == TargetType.ValueNode || targetType == TargetType.FlowNode) {
					System.Random random = new System.Random();
					name = random.Next(-987654, 987654).ToString();
					if(targetType == TargetType.ValueNode) {
						startType = (value as Node).ReturnType();
					}
					instance = value;
				} else if(targetType == TargetType.NodeField || targetType == TargetType.NodeOutputValue) {
					if(value is object[]) {
						object[] objs = value as object[];
						if(objs.Length > 1) {
							var node = objs[0] as NodeComponent;
							if(node != null) {
								if(objs[1] is FieldInfo) {
									name = (objs[1] as FieldInfo).Name;
								} else if(objs[1] is string) {
									name = (objs[1] as string);
								}
								instance = node;
							}
						}
					} else {
						name = value.ToString();
						instance = null;
					}
				} else if(targetType == TargetType.NodeFieldElement) {
					if(value is object[]) {
						object[] objs = value as object[];
						if(objs.Length > 2) {
							var node = objs[0] as NodeComponent;
							int index = 0;
							if(objs[2] is int) {
								index = (int)objs[2];
							} else {
								index = (int)objs[3];
							}
							if(node != null) {
								if(objs[1] is FieldInfo) {
									name = (objs[1] as FieldInfo).Name + "#" + index.ToString();
								} else if(objs[1] is string) {
									name = (objs[1] as string) + "#" + index.ToString();
								}
								instance = node;
							}
						}
					} else {
						name = value.ToString();
						instance = null;
					}
				} else if(targetType == TargetType.FlowInput) {
					if(value is object[]) {
						object[] objs = value as object[];
						if(objs.Length > 1) {
							FieldInfo field = objs[1] as FieldInfo;
							if(field != null) {
								name = field.Name;
							} else if(objs[1] is string) {
								name = objs[1] as string;
							}
							var node = objs[0] as Node;
							if(node != null) {
								instance = node;
							}
						}
					} else {
						name = value.ToString();
						instance = null;
					}
				} else {
					throw new Exception("Target type must be Values, SelfTarget, ValueNode, FlowNode, NodeField, NodeFieldElement, FlowInput or Type");
				}
			} else {
				name = "null";
				this.targetType = targetType;
			}
		}

		/// <summary>
		/// Create new member
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="targetType"></param>
		/// <param name="reflectedType"></param>
		public MemberData(string name, Type type, TargetType targetType = TargetType.None, Type reflectedType = null) {
			this.name = name;
			this.targetType = targetType;
			switch(targetType) {
				case TargetType.Constructor:
					isStatic = true;
					break;
			}
			startType = type;
			if(reflectedType != null) {
				this.type = reflectedType;
			}
		}

		/// <summary>
		/// Create new member from other member
		/// </summary>
		/// <param name="member"></param>
		public MemberData(MemberData member) {
			CopyFrom(member);
		}
		#endregion

		#region General Function
		/// <summary>
		/// Reset this member.
		/// </summary>
		/// <param name="fullReset">if true instanced value will be reset too.</param>
		public void Reset(bool fullReset = false) {
			fieldInfo = null;
			propertyInfo = null;
			name = "";
			isStatic = false;
			if(fullReset) {
				instance = null;
			}
			isReflected = false;
			startSerializedType = new SerializedType();
			targetSerializedType = new SerializedType();
			serializedItems = new OdinSerializedData[0];
			targetType = TargetType.None;
			if(targetType == TargetType.SelfTarget ||
				targetType == TargetType.ValueNode ||
				targetType == TargetType.FlowNode ||
				targetType == TargetType.Values) {
				instance = null;
			}
		}

		public IEnumerable<Object> GetUnityReferences() {
			if(targetReference != null) {
				for(int i = 0; i < targetReference.Count; i++) {
					if(targetReference[i] == null)
						continue;
					yield return targetReference[i];
				}
			}
			if(odinTargetData?.references != null) {
				for(int i = 0; i < odinTargetData.references.Count; i++) {
					if(odinTargetData.references[i] == null)
						continue;
					yield return odinTargetData.references[i];
				}
			}
			if(odinInstanceData?.references != null) {
				for(int i = 0; i < odinInstanceData.references.Count; i++) {
					if(odinInstanceData.references[i] == null)
						continue;
					yield return odinInstanceData.references[i];
				}
			}
			if(StartSerializedType.references != null) {
				for(int i = 0; i < StartSerializedType.references.Count; i++) {
					if(StartSerializedType.references[i] == null)
						continue;
					yield return StartSerializedType.references[i];
				}
			}
			if(TargetSerializedType.references != null) {
				for(int i = 0; i < TargetSerializedType.references.Count; i++) {
					if(TargetSerializedType.references[i] == null)
						continue;
					yield return TargetSerializedType.references[i];
				}
			}
		}

		public bool HasUnityReference(Object[] unityReferences) {
			if(unityReferences == null)
				return false;
			if(targetReference != null) {
				for(int i = 0; i < targetReference.Count; i++) {
					Object o = targetReference[i];
					for(int x = 0; x < unityReferences.Length; x++) {
						if(o == unityReferences[x]) {
							return true;
						}
					}
				}
			}
			if(odinTargetData?.references != null) {
				var references = odinTargetData.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < unityReferences.Length; x++) {
						if(o == unityReferences[x]) {
							return true;
						}
					}
				}
			}
			if(odinInstanceData?.references != null) {
				var references = odinInstanceData.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < unityReferences.Length; x++) {
						if(o == unityReferences[x]) {
							return true;
						}
					}
				}
			}
			if(StartSerializedType.references != null) {
				var references = StartSerializedType.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < unityReferences.Length; x++) {
						if(o == unityReferences[x]) {
							return true;
						}
					}
				}
			}
			if(TargetSerializedType.references != null) {
				var references = TargetSerializedType.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < unityReferences.Length; x++) {
						if(o == unityReferences[x]) {
							return true;
						}
					}
				}
			}
			return false;
		}

		public Action<Object[]> GetActionForRefactorUnityObject(Object[] from) {
			Action<Object[]> action = null;
			if(targetReference != null) {
				for(int i = 0; i < targetReference.Count; i++) {
					Object o = targetReference[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							int fromIndex = i;
							int toIndex = x;
							action += (objs) => {
								targetReference[fromIndex] = objs[toIndex];
							};
						}
					}
				}
			}
			if(odinTargetData?.references != null) {
				for(int i = 0; i < odinTargetData.references.Count; i++) {
					Object o = odinTargetData.references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							int fromIndex = i;
							int toIndex = x;
							action += (objs) => {
								odinTargetData.references[fromIndex] = objs[toIndex];
							};
						}
					}
				}
			}
			if(odinInstanceData?.references != null) {
				for(int i = 0; i < odinInstanceData.references.Count; i++) {
					Object o = odinInstanceData.references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							int fromIndex = i;
							int toIndex = x;
							action += (objs) => {
								odinInstanceData.references[fromIndex] = objs[toIndex];
							};
						}
					}
				}
			}
			if(StartSerializedType.references != null) {
				for(int i = 0; i < StartSerializedType.references.Count; i++) {
					Object o = StartSerializedType.references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							int fromIndex = i;
							int toIndex = x;
							action += (objs) => {
								StartSerializedType.references[fromIndex] = objs[toIndex];
							};
						}
					}
				}
			}
			if(TargetSerializedType.references != null) {
				for(int i = 0; i < TargetSerializedType.references.Count; i++) {
					Object o = TargetSerializedType.references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							int fromIndex = i;
							int toIndex = x;
							action += (objs) => {
								TargetSerializedType.references[fromIndex] = objs[toIndex];
							};
						}
					}
				}
			}
			return action;
		}

		public void RefactorUnityObject(Object[] from, Object[] to) {
			if(targetReference != null) {
				for(int i = 0; i < targetReference.Count; i++) {
					Object o = targetReference[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							targetReference[i] = to[x];
						}
					}
				}
			}
			if(odinTargetData?.references != null) {
				var references = odinTargetData.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							references[i] = to[x];
						}
					}
				}
			}
			if(odinInstanceData?.references != null) {
				var references = odinInstanceData.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							references[i] = to[x];
						}
					}
				}
			}
			if(StartSerializedType.references != null) {
				var references = StartSerializedType.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							references[i] = to[x];
						}
					}
				}
			}
			if(TargetSerializedType.references != null) {
				var references = TargetSerializedType.references;
				for(int i = 0; i < references.Count; i++) {
					Object o = references[i];
					for(int x = 0; x < from.Length; x++) {
						if(o == from[x]) {
							references[i] = to[x];
						}
					}
				}
			}
		}

		/// <summary>
		/// Get MemberInfo from this member, null if does not using reflection.
		/// </summary>
		/// <returns></returns>
		public MemberInfo[] GetMembers(bool throwOnFail = true) {
			if(_hasGetMember)
				return memberInfo;
			switch(targetType) {
				case TargetType.uNodeVariable:
				case TargetType.uNodeGroupVariable:
					if(!isDeepTarget)
						return null;
					IVariableSystem IVS = startTarget as IVariableSystem;
					if(IVS != null) {
						var var = IVS.GetVariableData(startName);
						if(var == null) {
							if(throwOnFail) {
								throw new Exception("Variable with name: " + startName + " cannot be found.");
							} else {
								return null;
							}
						}
						memberInfo = ReflectionUtils.GetMemberInfo(var.Type, name, flags, this, throwOnFail);
					} else if(startTarget is ILocalVariableSystem) {
						goto case TargetType.uNodeLocalVariable;
					}
					break;
				case TargetType.uNodeLocalVariable:
					if(!isDeepTarget)
						return null;
					ILocalVariableSystem LVS = startTarget as ILocalVariableSystem;
					if(LVS != null) {
						var var = LVS.GetLocalVariableData(startName);
						if(var == null) {
							if(throwOnFail) {
								throw new Exception("Loval Variable with name: " + startName + " cannot be found.");
							} else {
								return null;
							}
						}
						memberInfo = ReflectionUtils.GetMemberInfo(var.Type, name, flags, this, throwOnFail);
					} else {
						if(throwOnFail) {
							throw new Exception("Cannot get local variable system from target: " + startTarget);
						} else {
							return null;
						}
					}
					break;
				case TargetType.ValueNode:
					if(!isDeepTarget || _disableCache || !uNodeUtility.isPlaying)
						return null;
					Node VN = startTarget as Node;
					if(VN != null) {
						memberInfo = ReflectionUtils.GetMemberInfo(VN.ReturnType(), name, flags, this, throwOnFail);
					} else {
						if(throwOnFail) {
							throw new Exception("Missing target value node: " + startTarget);
						} else {
							return null;
						}
					}
					break;
				case TargetType.uNodeProperty:
					if(isDeepTarget) {
						var IPS = (startTarget as IPropertySystem);
						if(IPS != null) {
							var prop = IPS.GetPropertyData(startName);
							if(prop == null) {
								if(throwOnFail) {
									throw new Exception("Property with name: " + startName + " cannot be found.");
								} else {
									return null;
								}
							}
							memberInfo = ReflectionUtils.GetMemberInfo(
								prop.ReturnType(),
								name,
								flags,
								this,
								throwOnFail
							);
						} else {
							if(throwOnFail) {
								throw new Exception("Missing target property: " + startTarget);
							} else {
								return null;
							}
						}
					} else {
						return null;
					}
					break;
				case TargetType.NodeField:
					if(startTarget != null) {
						fieldInfo = startTarget.GetType().GetField(name, flags);
						if(fieldInfo == null) {
							if(throwOnFail) {
								throw new System.Exception("Member not found at path:" + name +
									", maybe you have wrong type, member name changed or wrong target.\ntype:" +
									startTarget.GetType().PrettyName());
							}
							return null;
						}
						memberInfo = new MemberInfo[] { fieldInfo };
					}
					return memberInfo;
				case TargetType.NodeFieldElement:
					if(startTarget != null) {
						fieldInfo = startTarget.GetType().GetField(startName.Split('#')[0], flags);
						if(fieldInfo == null) {
							if(throwOnFail) {
								throw new System.Exception("Member not found at path:" + name +
									", maybe you have wrong type, member name changed or wrong target.\ntype:" +
									startTarget.GetType().PrettyName());
							}
							return null;
						}
						memberInfo = new MemberInfo[] { fieldInfo };
					}
					return memberInfo;
				case TargetType.NodeOutputValue:
				case TargetType.FlowInputExtended:
					return null;
				case TargetType.FlowInput:
					if(startTarget != null) {
						fieldInfo = startTarget.GetType().GetField(name, flags);
						if(fieldInfo == null) {
							if(throwOnFail) {
								throw new System.Exception("Member not found at path:" + name +
									", maybe you have wrong type, member name changed or wrong target.\ntype:" +
									startTarget.GetType().PrettyName());
							}
							return null;
						}
						memberInfo = new MemberInfo[] { fieldInfo };
					}
					return memberInfo;
				case TargetType.uNodeParameter:
					if(isDeepTarget) {
						var IPS = startTarget as IParameterSystem;
						if(IPS == null) {
							if(throwOnFail) {
								throw new Exception("Parameter with name: " + startName + " cannot be found.");
							} else {
								return null;
							}
						}
						var param = IPS.GetParameterData(startName);
						if(param != null) {
							memberInfo = ReflectionUtils.GetMemberInfo(
								IPS.GetParameterData(startName).Type,
								name,
								flags,
								this,
								throwOnFail
							);
						} else {
							if(throwOnFail) {
								throw new Exception("Missing target parameter: " + startTarget);
							} else {
								return null;
							}
						}
					} else {
						return null;
					}
					break;
				case TargetType.uNodeGenericParameter:
				case TargetType.uNodeFunction:
				case TargetType.Type:
				case TargetType.uNodeType:
				case TargetType.None:
				case TargetType.Null:
				case TargetType.SelfTarget:
				case TargetType.FlowNode:
				case TargetType.Values:
					return null;
				default:
					if(_disableCache || !uNodeUtility.isPlaying) {
						if(startType != null) {
							memberInfo = ReflectionUtils.GetMemberInfo(startType, name, flags, this, throwOnFail);
						} else {
							return null;
						}
						break;
					}
					memberInfo = ReflectionUtils.GetMemberInfo(startType, name, flags, this, throwOnFail);
					break;
			}
			if(memberInfo == null) {
				return null;
			}
			if(memberInfo.Length == 0) {
				//return memberInfo;
				if(_disableCache || !uNodeUtility.isPlaying) {
					return memberInfo;
				}
				throw new Exception(string.Format("No matching member found: '{0}.{1}'", startType.Name, name));
			}
			fieldInfo = memberInfo[memberInfo.Length - 1] as FieldInfo;
			propertyInfo = memberInfo[memberInfo.Length - 1] as PropertyInfo;
			methodInfo = memberInfo[memberInfo.Length - 1] as MethodInfo;
			constructorInfo = memberInfo[memberInfo.Length - 1] as ConstructorInfo;
			eventInfo = memberInfo[memberInfo.Length - 1] as EventInfo;
			_hasGetMember = true;
			return memberInfo;
		}

		/// <summary>
		/// Set the SerializedItem of this member
		/// </summary>
		/// <param name="itemDatas"></param>
		public void SetItems(params ItemData[] itemDatas) {
			ResetCache();
			if(itemDatas == null) {
				serializedItems = new OdinSerializedData[0];
				return;
			}
			OdinSerializedData[] s = new OdinSerializedData[itemDatas.Length];
			for(int i = 0; i < s.Length; i++) {
				s[i] = SerializerUtility.SerializeValue(itemDatas[i]);
			}
			serializedItems = s;
		}

		/// <summary>
		/// Set the SerializedItem of this member
		/// </summary>
		/// <param name="itemDatas"></param>
		public void SetItems(IList<ItemData> itemDatas) {
			ResetCache();
			if(itemDatas == null) {
				serializedItems = new OdinSerializedData[0];
				return;
			}
			OdinSerializedData[] s = new OdinSerializedData[itemDatas.Count];
			for(int i = 0; i < s.Length; i++) {
				s[i] = SerializerUtility.SerializeValue(itemDatas[i]);
			}
			serializedItems = s;
		}

		/// <summary>
		/// Are this member can get a value.
		/// </summary>
		/// <returns></returns>
		public bool CanGetValue() {
			if(isAssigned) {
				switch(targetType) {
					case TargetType.FlowNode:
						return false;
					default:
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Are this member can set a value.
		/// </summary>
		/// <returns></returns>
		public bool CanSetValue() {
			if(isAssigned) {
				MemberInfo[] members;
				switch(targetType) {
					case TargetType.uNodeParameter:
						if(isDeepTarget) {
							members = GetMembers(false);
							if(members != null && members.Length > 0) {
								return ReflectionUtils.CanSetMemberValue(members[members.Length - 1]);
							}
							return false;
						}
						return true;
					case TargetType.NodeField:
					case TargetType.Field:
					case TargetType.NodeFieldElement:
					case TargetType.Event:
						return true;
					case TargetType.Property:
						members = GetMembers(false);
						if(members != null && members.Length > 0) {
							return ReflectionUtils.CanSetMemberValue(members[members.Length - 1]);
						}
						break;
					case TargetType.uNodeVariable:
					case TargetType.uNodeGroupVariable:
					case TargetType.uNodeLocalVariable:
						if(isDeepTarget) {
							members = GetMembers(false);
							if(members != null && members.Length > 0) {
								return ReflectionUtils.CanSetMemberValue(members[members.Length - 1]);
							}
						} else
							return true;
						break;
					case TargetType.ValueNode:
						Node VN = startTarget as Node;
						if(VN != null) {
							if(isDeepTarget) {
								members = GetMembers(false);
								if(members != null && members.Length > 0) {
									return ReflectionUtils.CanSetMemberValue(members[members.Length - 1]);
								}
							} else {
								return VN.CanSetValue();
							}
						}
						break;
				}
			}
			return false;
		}

		public RuntimeEvent CreateRuntimeEvent() {
			return new RuntimeEventValue(RuntimeType.Default, this);
		}

		public uNodeProperty GetProperty() {
			switch(targetType) {
				case TargetType.Property:
					return (startTarget as IPropertySystem)?.GetPropertyData(startName);
				default:
					return null;
			}
		}

		/// <summary>
		/// Get VariableData null if does not targeting variable data or not found the variable.
		/// </summary>
		/// <returns></returns>
		public VariableData GetVariable() {
			switch(targetType) {
				case TargetType.uNodeVariable:
				case TargetType.uNodeGroupVariable:
					IVariableSystem IVS = startTarget as IVariableSystem;
					if(IVS != null) {
						_variableData = IVS.GetVariableData(startName);
					} else {
						goto case TargetType.uNodeLocalVariable;
					}
					break;
				case TargetType.uNodeLocalVariable:
					ILocalVariableSystem LVS = startTarget as ILocalVariableSystem;
					if(LVS != null) {
						_variableData = LVS.GetLocalVariableData(startName);
					}
					break;
			}
			return _variableData;
		}



		/// <summary>
		/// Is this member control points is proxy?
		/// </summary>
		/// <returns></returns>
		public bool IsProxy() {
			return controlPoints != null &&
				controlPoints.Length > 0 &&
				controlPoints[0].point == UnityEngine.Vector2.zero;
		}

		/// <summary>
		/// Copy data from another member.
		/// </summary>
		/// <param name="member"></param>
		public void CopyFrom(MemberData member) {
			if(member == null) {
				Reset();
				return;
			}
			if(member.targetType == TargetType.None) {
				Reset();
				return;
			} else if(member.targetType == TargetType.Null) {
				name = member.name;
				startSerializedType = new SerializedType();
				targetSerializedType = new SerializedType();
				instance = null;
				isStatic = false;
				targetType = member.targetType;
				return;
			}
			name = member.name;
			targetType = member.targetType;
			odinInstanceData = member.odinInstanceData;
			odinTargetData = member.odinTargetData;
			instance = member.instance;
			isStatic = member.isStatic;
			StartSerializedType = member.StartSerializedType;
			targetSerializedType = member.targetSerializedType;
			targetReference.Clear();
			targetReference.AddRange(member.targetReference);
			SerializedItems = SerializerUtility.Duplicate(member.SerializedItems);
			ResetCache();
		}

		/// <summary>
		/// Copy data to another member.
		/// </summary>
		/// <param name="member"></param>
		public void CopyTo(MemberData member) {
			if(member != null)
				member.CopyFrom(this);
		}

		private void EnsureIntialized() {
			if(_disableCache || !uNodeUtility.isPlaying) {
				isReflected = false;
			}
			if(!isReflected) {
				switch(targetType) {
					case TargetType.uNodeVariable:
					case TargetType.uNodeGroupVariable:
						IVariableSystem IVS = startTarget as IVariableSystem;
						if(IVS != null) {
							_variableData = IVS.GetVariableData(startName);
							if(_variableData == null) {
								throw new Exception($"Cannot find variable: {startName} from object: {IVS.ToString()}");
							}
							isReflected = true;
							GetMembers();
						} else {
							goto case TargetType.uNodeLocalVariable;
						}
						break;
					case TargetType.uNodeLocalVariable:
						ILocalVariableSystem LVS = startTarget as ILocalVariableSystem;
						if(LVS != null) {
							_variableData = LVS.GetLocalVariableData(startName);
							if(_variableData == null) {
								throw new Exception($"Cannot find variable: {startName} from object: {LVS.ToString()}");
							}
							isReflected = true;
							GetMembers();
						} else {
							if(startTarget == null) {
								throw new NullReferenceException("The instance / target cannot be null");
							}
							throw new InvalidOperationException($"The object: {startTarget} is not VariableSystem");
						}
						break;
					case TargetType.uNodeProperty:
						isReflected = true;
						if(isDeepTarget)
							GetMembers();
						break;
					case TargetType.ValueNode:
						Node VN = startTarget as Node;
						if(VN != null) {
							isReflected = true;
							if(isDeepTarget)
								GetMembers();
						}
						break;
					case TargetType.uNodeParameter:
						_parameterData = (startTarget as IParameterSystem).GetParameterData(startName);
						if(_parameterData == null) {
							throw new Exception($"Cannot find parameter: {startName} from object: {startTarget}");
						}
						isReflected = true;
						if(isDeepTarget) {
							GetMembers();
							break;
						}
						return;
					case TargetType.uNodeGenericParameter:
						if(Items == null || Items.Length == 0) {
							if(startTarget is IGenericParameterSystem) {
								_genericParameterData = new GenericParameterData[1];
								if(name.Contains('[')) {
									_genericParameterData[0] = (startTarget as IGenericParameterSystem).GetGenericParameter(name.Replace("[]", ""));
								} else {
									_genericParameterData[0] = (startTarget as IGenericParameterSystem).GetGenericParameter(name);
								}
							}
						}
						isReflected = true;
						return;
					case TargetType.uNodeFunction:
						if(_function == null) {
							_function = (startTarget as uNodeRoot).GetFunction(startName, GenericTypes[0] == null ? 0 : GenericTypes[0].Length, ParameterTypes[0]);
							if(_function == null) {
								throw new Exception($"Cannot find function: {startName} from object: {startTarget}");
							}
						}
						isReflected = true;
						break;
					case TargetType.NodeField:
					case TargetType.NodeFieldElement:
					case TargetType.FlowInput:
						GetMembers();
						isReflected = true;
						break;
					case TargetType.Type:
					case TargetType.Values:
					case TargetType.None:
					case TargetType.FlowNode:
					case TargetType.SelfTarget:
					case TargetType.uNodeType:
					case TargetType.NodeOutputValue:
					case TargetType.FlowInputExtended:
						isReflected = true;
						return;
					default:
						GetMembers();
						break;
				}
			}
			if(_disableCache || !uNodeUtility.isPlaying) {
				isReflected = false;
			}
		}
		#endregion

		#region Flow Functions
		/// <summary>
		/// Execute flow connection.
		/// </summary>
		public void InvokeFlow() {
			switch(targetType) {
				case TargetType.FlowNode: {
					ActivateFlowNode();
					break;
				}
				case TargetType.FlowInput: {
					IFlowPort flow = Invoke(null) as IFlowPort;
					if(flow != null) {
						flow.OnExecute();
						if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
							Node N = startTarget as Node;
							if(N) {
								GraphDebug.FlowTransition(N.owner, N.owner.GetInstanceID(), N.GetInstanceID(), startName);
							}
						}
					} else {
						throw new Exception("FlowInput is null");
					}
					break;
				}
				case TargetType.FlowInputExtended: {
					var node = startTarget as IExtendedInput;
					if(node != null) {
						node.InvokeFlowInput(startName);
					} else {
						throw new Exception("The node is null/invalid");
					}
					break;
				}
				default:
					throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
		}

		/// <summary>
		/// Activate flow node, return true if node is finished after activate
		/// </summary>
		/// <returns>true if node is finished after activate.</returns>
		public bool ActivateFlowNode() {
			switch(targetType) {
				case TargetType.FlowNode: {
					Node N = startTarget as Node;
					if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
						int integer = int.Parse(startName);
						GraphDebug.FlowTransition(N.owner, N.owner.GetInstanceID(), N.GetInstanceID(), integer);
					}
					N.Activate();
					return N.IsFinished();
				}
				case TargetType.FlowInput:
				case TargetType.FlowInputExtended: {
					InvokeFlow();
					return true;
				}
				default:
					throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
		}

		/// <summary>
		/// Activate flow node, return true if node is finished after activate
		/// </summary>
		/// <param name="node"></param>
		/// <returns>true if node is finished after activate.</returns>
		public bool ActivateFlowNode(out Node node) {
			if(targetType != TargetType.FlowNode) {
				if(targetType == TargetType.FlowInput) {
					node = null;
					InvokeFlow();
					return true;
				}
				if(targetType == TargetType.None) {
					throw new Exception("Unassigned flow target");
				}
				throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
			Node N = startTarget as Node;
			if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
				int integer = int.Parse(startName);
				GraphDebug.FlowTransition(N.owner, N.owner.GetInstanceID(), N.GetInstanceID(), integer);
			}
			N.Activate();
			node = N;
			return N.IsFinished();
		}

		/// <summary>
		/// Activate flow node, return true if node is finished after activate
		/// </summary>
		/// <param name="waitUntil"></param>
		/// <returns>true if node is finished after activate.</returns>
		public bool ActivateFlowNode(out WaitUntil waitUntil) {
			if(targetType != TargetType.FlowNode) {
				if(targetType == TargetType.FlowInput) {
					waitUntil = null;
					InvokeFlow();
					return true;
				}
				if(targetType == TargetType.None) {
					throw new Exception("Unassigned flow target");
				}
				throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
			Node N = startTarget as Node;
			if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
				int integer = int.Parse(startName);
				GraphDebug.FlowTransition(N.owner, N.owner.GetInstanceID(), N.GetInstanceID(), integer);
			}
			N.Activate();
			if(!N.IsFinished()) {
				waitUntil = N.WaitUntilFinish();
				return false;
			} else {
				waitUntil = null;
				return true;
			}
		}

		/// <summary>
		/// Activate flow node, return true if node is finished after activate
		/// </summary>
		/// <param name="node"></param>
		/// <param name="waitUntil"></param>
		/// <returns>true if node is finished after activate.</returns>
		public bool ActivateFlowNode(out Node node, out WaitUntil waitUntil) {
			if(targetType != TargetType.FlowNode) {
				if(targetType == TargetType.FlowInput) {
					node = null;
					waitUntil = null;
					InvokeFlow();
					return true;
				}
				if(targetType == TargetType.None) {
					throw new Exception("Unassigned flow target");
				}
				throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
			Node N = startTarget as Node;
			if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
				int integer = int.Parse(startName);
				GraphDebug.FlowTransition(N.owner, N.owner.GetInstanceID(), N.GetInstanceID(), integer);
			}
			N.Activate();
			node = N;
			if(!N.IsFinished()) {
				waitUntil = N.WaitUntilFinish();
				return false;
			} else {
				waitUntil = null;
				return true;
			}
		}

		/// <summary>
		/// Wait target node until finished, return null if node not running.
		/// </summary>
		/// <returns></returns>
		public WaitUntil WaitFlowNode() {
			if(targetType != TargetType.FlowNode) {
				if(targetType == TargetType.FlowInput) {
					InvokeFlow();
					return null;
				}
				if(targetType == TargetType.None) {
					throw new Exception("Unassigned flow target");
				}
				throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
			Node N = startTarget as Node;
			if(N.IsFinished()) {
				return null;
			}
			return N.WaitUntilFinish();
		}

		/// <summary>
		/// Wait target node until finished, return null if node not running.
		/// </summary>
		/// <param name="node">The activate node</param>
		/// <returns></returns>
		public WaitUntil WaitFlowNode(out Node node) {
			if(targetType != TargetType.FlowNode) {
				if(targetType == TargetType.FlowInput) {
					node = null;
					InvokeFlow();
					return null;
				}
				if(targetType == TargetType.None) {
					throw new Exception("Unassigned flow target");
				}
				throw new Exception($"Invalid flow target type: {targetType}.\nThe target type must be FlowNode or FlowInput");
			}
			Node N = startTarget as Node;
			node = N;
			if(N.IsFinished()) {
				return null;
			}
			return N.WaitUntilFinish();
		}

		/// <summary>
		/// Get Node target.
		/// </summary>
		/// <returns></returns>
		public NodeComponent GetTargetNode() {
			switch(targetType) {
				case TargetType.FlowNode:
				case TargetType.FlowInput:
				case TargetType.ValueNode:
				case TargetType.uNodeGroupVariable:
				case TargetType.NodeField:
				case TargetType.NodeFieldElement:
				case TargetType.NodeOutputValue:
				case TargetType.FlowInputExtended: {
					var result = instance;
					if(instance is MemberData member) {
						return member.GetTargetNode();
					}
					return result as NodeComponent;
				}
				default:
					return null;
			}
		}

		/// <summary>
		/// Get Flow Node target.
		/// </summary>
		/// <returns></returns>
		public Node GetFlowNode() {
			switch(targetType) {
				case TargetType.FlowNode:
				case TargetType.FlowInput:
				case TargetType.ValueNode:
				case TargetType.uNodeGroupVariable:
				case TargetType.NodeField:
				case TargetType.NodeFieldElement:
				case TargetType.NodeOutputValue:
				case TargetType.FlowInputExtended: {
					var result = instance;
					if(instance is MemberData member) {
						return member.GetFlowNode();
					}
					return result as Node;
				}
				default:
					return null;
			}
		}

		public Object GetUnityObject() {
			if(IsTargetingNode) {
				return GetTargetNode();
			} else if(targetType == TargetType.uNodeFunction) {
				if(startTarget is uNodeRoot) {
					if(_function == null) {
						var GT = Utilities.SafeGetGenericTypes(this);
						_function = (startTarget as uNodeRoot).GetFunction(startName, GT[0] == null ? 0 : GT[0].Length, Utilities.SafeGetParameterTypes(this)[0]);
					}
					if(_function) {
						return _function;
					}
				}
			}
			var obj = instance;
			if(obj is MemberData member) {
				return member.GetUnityObject();
			}
			return obj as Object;
		}

		/// <summary>
		/// Get access index for Node Field Element target.
		/// This will return -1 for unsuccessful operation.
		/// </summary>
		/// <returns></returns>
		public int GetAccessIndex() {
			if(targetType == TargetType.NodeFieldElement) {
				string[] arr = startName.Split('#');
				if(arr.Length == 2) {//Make sure the format is valid.
					int result;
					if(int.TryParse(arr[1], out result)) {//Make sure the string format can be parsed to integer. 
						return result;
					}
				}
			}
			return -1;
		}
		#endregion

		#region Get
		/// <summary>
		/// Get or set the value
		/// </summary>
		public object Value {
			get {
				return Get();
			}
			set {
				Set(value);
			}
		}

		/// <summary>
		/// Retrieves the value of the member or call it.
		/// </summary>
		public object Get() {
			return Invoke(null);
		}

		/// <summary>
		/// Retrieves the value of the member or call it.
		/// </summary>
		public object Get(Type convertType) {
			if(convertType != null) {
				object resultValue = Get();
				if(resultValue != null) {
					if(resultValue.GetType() == type)
						return resultValue;
					return Operator.Convert(resultValue, type);
				}
			}
			return Invoke(null);
		}

		/// <summary>
		/// Generic Wrapper to get value for class type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Get<T>() {
			//if(type == null) return null;
			object resultValue = Get();
			if(!object.ReferenceEquals(resultValue, null)) {
				return Operator.Convert<T>(resultValue);
			}
			return default;
		}

		/// <summary>
		/// Get a value without cache system.
		/// </summary>
		/// <returns></returns>
		public object SafeGet() {
			//if(type == null) return null;
			_disableCache = true;
			object resultValue = Get();
			_disableCache = false;
			if(!object.ReferenceEquals(resultValue, null)) {
				return resultValue;
			}
			return null;
		}

		/// <summary>
		/// Get a value without cache system.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T SafeGet<T>() {
			//if(type == null) return null;
			_disableCache = true;
			object resultValue = Get();
			_disableCache = false;
			if(!object.ReferenceEquals(resultValue, null)) {
				return (T)resultValue;
			}
			return default;
		}
		#endregion

		#region Invoke
		public object Invoke(object[] paramValues) {
			EnsureIntialized();
			AutoConvertParameters(ref paramValues);
			return DoInvoke(paramValues);
		}

		private object DoReflect(object reflectionTarget, object[] paramValues) {
			if(methodInfo != null) {
				reflectionTarget = AutoConvertValue(reflectionTarget, methodInfo.DeclaringType);//Ensure the value is valid
				int paramsLength = methodInfo.GetParameters().Length;
				if(paramValues != null && paramsLength != paramValues.Length) {
					object[] obj = new object[paramsLength];
					for(int x = 0; x < paramsLength; x++) {
						obj[x] = paramValues[(paramValues.Length - paramsLength) + x];
					}
					if(hasRefOrOut) {
						object retVal = methodInfo.InvokeOptimized(reflectionTarget, obj);
						for(int x = 0; x < paramsLength; x++) {
							paramValues[(paramValues.Length - paramsLength) + x] = obj[x];
						}
						return retVal;
					}
					return methodInfo.InvokeOptimized(reflectionTarget, obj);
				} else {
					return methodInfo.InvokeOptimized(reflectionTarget, paramValues);
				}
			} else if(constructorInfo != null) {
				int paramsLength = constructorInfo.GetParameters().Length;
				if(paramValues != null && paramsLength != paramValues.Length) {
					object[] obj = new object[paramsLength];
					for(int x = 0; x < paramsLength; x++) {
						obj[x] = paramValues[(paramValues.Length - paramsLength) + x];
					}
					if(hasRefOrOut) {
						object retVal = constructorInfo.Invoke(obj);
						for(int x = 0; x < paramsLength; x++) {
							paramValues[(paramValues.Length - paramsLength) + x] = obj[x];
						}
						return retVal;
					}
					return constructorInfo.Invoke(obj);
				} else {
					if((paramValues == null || paramValues.Length == 0) && constructorInfo.ReflectedType.IsValueType) {
						return Activator.CreateInstance(constructorInfo.ReflectedType);
					}
					return constructorInfo.Invoke(paramValues);
				}
			} else if(fieldInfo != null) {
				reflectionTarget = AutoConvertValue(reflectionTarget, fieldInfo.ReflectedType);//Ensure the value is valid
				return fieldInfo.GetValueOptimized(reflectionTarget);
			} else if(propertyInfo != null) {
				reflectionTarget = AutoConvertValue(reflectionTarget, propertyInfo.ReflectedType);//Ensure the value is valid
				return propertyInfo.GetValueOptimized(reflectionTarget);
			} else {
				if(_variableData == null) {
					throw new Exception(targetType.ToString());
				}
				return _variableData.Get();
			}
		}

		private void DoReflectSet(object reflectionTarget, object value) {
			if(fieldInfo != null) {
				if(type.IsValueType && memberInfo.Length > 1) {
					ReflectionUtils.SetBoxedMemberValue(parentTarget, memberInfo[memberInfo.Length - 2], reflectionTarget, fieldInfo, value);
				} else {
					fieldInfo.SetValueOptimized(reflectionTarget, value);
				}
				return;
			}
			if(propertyInfo != null) {
				if(type.IsValueType && memberInfo.Length > 1) {
					ReflectionUtils.SetBoxedMemberValue(parentTarget, memberInfo[memberInfo.Length - 2], reflectionTarget, propertyInfo, value);
				} else {
					propertyInfo.SetValueOptimized(reflectionTarget, value);
				}
				return;
			} else if(methodInfo != null) {
				throw new Exception("Method can't be set.");
			}
		}

		/// <summary>
		/// Invoke Member Method with parameter or get field/property
		/// </summary>
		/// <param name="paramValues"></param>
		/// <returns></returns>
		private object DoInvoke(object[] paramValues) {
			switch(targetType) {
				case TargetType.Null:
				case TargetType.None:
					return null;
				case TargetType.uNodeType:
					var startObj = startTarget;
					if(startObj is RuntimeType) {
						return startObj as RuntimeType;
					} else if(startObj is UnityEngine.Object) {
						return ReflectionUtils.GetRuntimeType(startObj as UnityEngine.Object);
					}
					return startType;
				case TargetType.uNodeConstructor:
					throw new Exception("uNodeConstructor doesn't support in runtime, its only for code generation.");
				case TargetType.uNodeLocalVariable:
				case TargetType.uNodeGroupVariable:
				case TargetType.uNodeVariable:
					if(_variableData == null) {
						throw new Exception($"Cannot find variable: {startName} from object: {startTarget}");
					}
					if(isDeepTarget) {
						return DoReflect(ReflectionUtils.GetMemberTargetRef(memberInfo, _variableData.Get(), ref parentTarget, paramValues), paramValues);
					}
					return _variableData.Get();
				case TargetType.uNodeProperty:
					if(isDeepTarget) {
						return DoReflect(ReflectionUtils.GetMemberTargetRef(memberInfo, (startTarget as IPropertySystem).GetPropertyData(startName).Get(), ref parentTarget, paramValues), paramValues);
					}
					return (startTarget as IPropertySystem).GetPropertyData(startName).Get();
				case TargetType.Type:
					return startType;
				case TargetType.Values:
					if(object.ReferenceEquals(_values, null)) {
						var result = SerializerUtility.Deserialize(odinTargetData);
						if(result != null && (result is string || result.GetType().IsValueType)) {
							_values = result;
						}
						return result;
					}
					return _values;
				case TargetType.NodeField: {
					var result = fieldInfo.GetValueOptimized(startTarget);
#if UNITY_EDITOR
					if(GraphDebug.useDebug) {
						var nodeComp = startTarget as NodeComponent;
						if(nodeComp != null) {
							GraphDebug.ValueField(nodeComp.owner, nodeComp.owner.GetInstanceID(), nodeComp.GetInstanceID(), startName, result);
						}
					}
#endif
					return result;
				}
				case TargetType.NodeFieldElement:
					return (fieldInfo.GetValueOptimized(startTarget) as IList)[GetAccessIndex()];
				case TargetType.NodeOutputValue: {
					var node = startTarget as IExtendedOutput;
					if(node != null) {
						var result = node.GetOutputValue(startName);
#if UNITY_EDITOR
						if(GraphDebug.useDebug) {
							var nodeComp = node as NodeComponent;
							GraphDebug.ValueOutput(nodeComp.owner, nodeComp.owner.GetInstanceID(), nodeComp.GetInstanceID(), startName, result);
						}
#endif
						return result;
					} else {
						throw new Exception("The node is null/invalid");
					}
				}
				case TargetType.FlowInput:
					return fieldInfo.GetValueOptimized(startTarget);
				case TargetType.SelfTarget:
					return startTarget;
				case TargetType.ValueNode: {
					object val = (startTarget as Node).GetValue();
#if UNITY_EDITOR
					if(GraphDebug.useDebug) {
						Node VN = startTarget as Node;
						int integer = int.Parse(startName);
						GraphDebug.ValueNode(VN.owner, VN.owner.GetInstanceID(), VN.GetInstanceID(), integer, val);
					}
#endif
					if(isDeepTarget) {
						return DoReflect(ReflectionUtils.GetMemberTargetRef(memberInfo, val, ref parentTarget, paramValues), paramValues);
					}
					return val;
				}
				case TargetType.FlowNode:
				case TargetType.FlowInputExtended:
					throw new System.Exception("Can't get FlowNode target, Use InvokeFlowNode() to invoke/call FlowNode target");
				case TargetType.uNodeParameter:
					if(isDeepTarget) {
						return DoReflect(ReflectionUtils.GetMemberTargetRef(memberInfo, _parameterData.value, ref parentTarget, paramValues), paramValues);
					}
					return _parameterData.value;
				case TargetType.uNodeGenericParameter:
					if(Items != null && Items.Length > 0) {
						ItemData iData = Items[0];
						if(iData != null) {
							Type[] genericTypes = GenericTypes[0];
							if(genericTypes.Length == 1) {
								if(name.Contains('[')) {
									int arrayCount = 0;
									foreach(var n in name) {
										if(n == '[') {
											arrayCount++;
										}
									}
									Type t = genericTypes[0];
									for(int i = 0; i < arrayCount; i++) {
										t = t.MakeArrayType();
									}
									return t;
								}
								return genericTypes[0];
							}

						}
					}
					if(_genericParameterData != null) {
						//if(type != null && type.IsGenericTypeDefinition) {
						//	return type.MakeGenericType(_genericParameterData.Select(item => item.value).ToArray());
						//}
						if(_genericParameterData.Length == 1 && _genericParameterData[0] != null) {
							if(name.Contains('[')) {
								int arrayCount = 0;
								foreach(var n in name) {
									if(n == '[') {
										arrayCount++;
									}
								}
								Type t = _genericParameterData[0].value;
								for(int i = 0; i < arrayCount; i++) {
									t = t.MakeArrayType();
								}
								return t;
							}
							return _genericParameterData[0].value;
						}
					}
					return type;
				case TargetType.uNodeFunction:
					if(startTarget is uNodeRoot) {
						return _function.Invoke(paramValues, GenericTypes[0]);
					} else if(startTarget is uNodeFunction) {
						return (startTarget as uNodeFunction).Invoke(paramValues, GenericTypes[0]);
					}
					break;
				case TargetType.Event:
					return new Event(eventInfo, ReflectionUtils.GetMemberTargetRef(memberInfo, startTarget, ref parentTarget, paramValues));
				case TargetType.Constructor:
					if(constructorInfo != null) {
						goto default;
					}
					return Activator.CreateInstance(startType);
				default:
					return DoReflect(ReflectionUtils.GetMemberTargetRef(memberInfo, startTarget, ref parentTarget, paramValues), paramValues);
			}
			return null;
		}
		#endregion

		#region AutoConvert
		private void AutoConvertValue(ref object value) {
			value = AutoConvertValue(value, type);
		}

		private void AutoConvertParameters(ref object[] values) {
			if(values == null || values.Length == 0 || ParameterTypes == null)
				return;
			int count = 0;
			for(int a = 0; a < ParameterTypes.Length; a++) {
				if(ParameterTypes[a] == null)
					continue;
				for(int b = 0; b < ParameterTypes[a].Length; b++) {
					Type t = ParameterTypes[a][b];
					if(t != null && values.Length > count) {
						object val = values[count];
						if(val == null)
							continue;
						values[count] = AutoConvertValue(val, t);
					}
					count++;
				}
			}
		}

		private static object AutoConvertValue(object value, Type type) {
			if(type == null)
				return value;
			if(value is Delegate) {
				if(value.GetType() != type && type.IsCastableTo(typeof(Delegate))) {
					if(value is EventCallback func) {
						var method = type.GetMethod("Invoke");
						if(method.ReturnType == typeof(void)) {
							var parameters = method.GetParameters();
							Type[] types = new Type[parameters.Length];
							for(int i = 0; i < parameters.Length; i++) {
								types[i] = parameters[i].ParameterType;
							}
							return CustomDelegate.CreateActionDelegate((obj) => {
								func(obj);
							}, types);
						} else {
							var parameters = method.GetParameters();
							Type[] types = new Type[parameters.Length + 1];
							for(int i = 0; i < parameters.Length; i++) {
								types[i] = parameters[i].ParameterType;
							}
							types[parameters.Length] = method.ReturnType;
							return CustomDelegate.CreateFuncDelegate((obj) => {
								return func(obj);
							}, types);
						}
					} else {
						Delegate del = value as Delegate;
						return Delegate.CreateDelegate(type, del.Target, del.Method);
					}
				}
			} else if(value != null) {
				if(type.IsByRef)
					type = type.GetElementType();
				Type valType = value.GetType();
				if(valType != type && (valType.IsValueType || !valType.IsSubclassOf(type) && type.IsCastableTo(valType))) {
					return Operator.Convert(value, type);
				} else if(type is RuntimeType) {
					return Operator.Convert(value, type);
				}
				// else if(type.IsSubclassOf(typeof(Component))) {
				// 	if(value is GameObject gameObject) {
				// 		return gameObject.GetComponent(type);
				// 	} else if(value is Component component) {
				// 		return component.GetComponent(type);
				// 	}
				// }
			}
			return value;
		}
		#endregion

		#region Set
		/// <summary>
		/// Assigns a new value to the variable.
		/// </summary>
		/// <param name="value"></param>
		public void Set(object value) {
			Set(value, null);
		}

		/// <summary>
		/// Assigns a new value to the variable.
		/// </summary>
		/// <param name="value">The value to assign</param>
		/// <param name="paramValues">the parameter list for invoke method.</param>
		public void Set(object value, object[] paramValues) {
			EnsureIntialized();
			AutoConvertValue(ref value);
			switch(targetType) {
				case TargetType.None:
					return;
				case TargetType.NodeField:
					fieldInfo.SetValueOptimized(startTarget, value);
					return;
				case TargetType.NodeFieldElement:
					var list = (fieldInfo.GetValueOptimized(startTarget) as IList);
					list[GetAccessIndex()] = value;
					fieldInfo.SetValueOptimized(startTarget, list);
					return;
				case TargetType.uNodeParameter:
					if(isDeepTarget) {
						DoReflectSet(ReflectionUtils.GetMemberTargetRef(memberInfo, _parameterData.value, ref parentTarget, paramValues), value);
					} else {
						_parameterData.value = value;
					}
					return;
				case TargetType.uNodeGenericParameter:
					throw new Exception("Generic Type can't be set");
				case TargetType.uNodeFunction:
					throw new Exception("Class Function can't be set");
				case TargetType.Type:
					throw new Exception("Target Type : Type can't be set");
				case TargetType.Values:
					throw new Exception("Target Type : Values can't be set");
				case TargetType.SelfTarget:
					throw new Exception("Self target can't be set");
				case TargetType.FlowNode:
					throw new Exception("FlowNode target can't be set");
				case TargetType.FlowInput:
					throw new Exception("FlowInput target can't be set");
				case TargetType.uNodeProperty:
					if(isDeepTarget) {
						goto default;
					} else {
						(startTarget as IPropertySystem).GetPropertyData(startName).Set(value);
					}
					break;
				case TargetType.ValueNode:
					Node VN = startTarget as Node;
					if(VN != null) {
						if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
							int integer = int.Parse(startName);
							GraphDebug.ValueNode(VN.owner, VN.owner.GetInstanceID(), VN.GetInstanceID(), integer, value, true);
						}
						if(isDeepTarget) {
							DoReflectSet(ReflectionUtils.GetMemberTargetRef(memberInfo, VN.GetValue(), ref parentTarget, paramValues), value);
						} else {
							//throw new Exception("Value Node target can't be set");
							VN.SetValue(value);
						}
					}
					break;
				case TargetType.uNodeLocalVariable:
				case TargetType.uNodeGroupVariable:
				case TargetType.uNodeVariable:
					if(isDeepTarget) {
						DoReflectSet(ReflectionUtils.GetMemberTargetRef(memberInfo, _variableData.Get(), ref parentTarget, paramValues), value);
					} else {
						_variableData.Set(value);
					}
					return;
				default:
					DoReflectSet(ReflectionUtils.GetMemberTargetRef(memberInfo, startTarget, ref parentTarget, paramValues), value);
					return;
			}
		}
		#endregion

		#region Types
		/// <summary>
		/// The type of the reflected member.
		/// </summary>
		public Type type {
			get {
				if(_type == null || _type.Equals(null)) {
					switch(targetType) {
						case TargetType.None:
							return null;
						case TargetType.Null:
							//Return System.Object type on target type is Null.
							return typeof(object);
						case TargetType.NodeField:
							if(startTarget != null) {
								FieldInfo field = startTarget.GetType().GetField(name);
								if(field != null) {
									if(field.IsDefined(typeof(ValueOutAttribute), true)) {
										return field.GetCustomAttribute<ValueOutAttribute>().type ?? field.FieldType;
									} else if(uNodeUtility.isInEditor) {
										return ReflectionUtils.GetActualFieldType(field, startTarget);
									} else {
										return field.FieldType;
									}
								}
							}
							return typeof(object);
						case TargetType.NodeFieldElement:
							if(startTarget != null) {
								FieldInfo field = startTarget.GetType().GetField(startName.Split('#')[0]);
								if(field != null) {
									Type fType;
									if(uNodeUtility.isInEditor) {
										fType = ReflectionUtils.GetActualFieldType(field, startTarget).ElementType();
									} else {
										fType = field.FieldType.ElementType();
									}
									if(fType != null) {
										return fType;
									}
								}
							}
							return typeof(object);
						case TargetType.ValueNode:
							if(startTarget is Node && !isDeepTarget) {
								//_type = (startTarget as Node).ReturnType();
								return (startTarget as Node).ReturnType();
							} else {
								goto default;
							}
						//break;
						//case TargetType.uNodeGenericParameter:
						//	//if(startTarget is uNodeFunction) {
						//	//	_genericParameterData = new GenericParameterData[1];
						//	//	_genericParameterData[0] = (startTarget as uNodeFunction).GetGenericParameter(name);
						//	//	return _genericParameterData[0].value;
						//	//}
						//	if(_type == null) {
						//		goto default;
						//	}
						//	break;
						case TargetType.NodeOutputValue:
							if(startTarget is IExtendedOutput) {
								return (startTarget as IExtendedOutput).GetOutputType(startName);
							} else {
								goto default;
							}
						case TargetType.uNodeParameter:
							if(isDeepTarget) {
								goto default;
							}
							if(startTarget is RootObject) {
								var param = (startTarget as RootObject).GetParameterData(startName);
								if(param != null) {
									return param.type.Get<Type>();
								}
							}
							if(_type == null) {
								goto default;
							}
							break;
						case TargetType.FlowNode:
						case TargetType.FlowInput:
						case TargetType.FlowInputExtended:
							return typeof(Node);
						case TargetType.uNodeType:
							return startType;
						case TargetType.uNodeGenericParameter:
						case TargetType.Type:
							return typeof(Type);
						case TargetType.SelfTarget:
							if(startTarget != null) {
								_type = startTarget.GetType();
							}
							break;
						case TargetType.Values:
							return startType;
						case TargetType.uNodeVariable:
						case TargetType.uNodeGroupVariable:
						case TargetType.uNodeLocalVariable:
							if(!isDeepTarget) {
								var variable = GetVariable();
								if(variable != null) {
									return type = variable.Type;
								}
							}
							goto default;
						default:
							if(_type == null) {
								var members = GetMembers(false);
								if(members != null && members.Length > 0) {
									var t = ReflectionUtils.GetMemberType(members[members.Length - 1]);
									type = t;
									_type = t;
								} else {
									_type = TargetSerializedType.type;
								}
								if(_type == null && !TargetSerializedType.isFilled) {
									return startType;
								}
							}
							break;
					}
				}
				return _type;
			}
			set {
				if(value == null)
					return;
				targetSerializedType = new SerializedType(value);
				_type = null;
			}
		}

		/// <summary>
		/// The start target type.
		/// </summary>
		public Type startType {
			get {
				if(_startType == null || _startType.Equals(null)) {
					if(isStatic) {
						var GenericTypes = Utilities.SafeGetGenericTypes(this);
						if(GenericTypes != null && GenericTypes.Length > 0 && GenericTypes[0] != null && GenericTypes[0].Length > 0) {
							_startType = GenericTypes[0][0];
						} else {
							_startType = StartSerializedType.type;
							if(_startType != null && _startType.IsGenericTypeDefinition &&
								GenericTypes != null && GenericTypes.Length > 0 &&
								GenericTypes[0] != null && GenericTypes[0].Length > 0) {
								_startType = ReflectionUtils.MakeGenericType(_startType, GenericTypes[0]);
							}
						}
					} else {
						switch(targetType) {
							case TargetType.ValueNode:
								if(startTarget is Node) {
									_startType = (startTarget as Node).ReturnType();
								} else {
									goto default;
								}
								break;
							case TargetType.NodeOutputValue:
								if(startTarget is IExtendedOutput) {
									_startType = (startTarget as IExtendedOutput).GetOutputType(startName);
								} else {
									goto default;
								}
								break;
							case TargetType.FlowNode:
							case TargetType.FlowInput:
							case TargetType.FlowInputExtended:
								_startType = typeof(Node);
								break;
							case TargetType.NodeField:
							case TargetType.NodeFieldElement:
							default:
								_startType = StartSerializedType.type;
								if(_startType != null && _startType.IsGenericTypeDefinition) {
									var GenericTypes = Utilities.SafeGetGenericTypes(this);
									if(GenericTypes != null && GenericTypes.Length > 0 && GenericTypes[0] != null && GenericTypes[0].Length > 0) {
										_startType = ReflectionUtils.MakeGenericType(_startType, GenericTypes[0]);
									}
								}
								break;
						}
					}
				}
				return _startType;
			}
			set {
				if(value == null)
					return;
				StartSerializedType = new SerializedType(value);
				ResetCache();
			}
		}
		#endregion

		#region Cached Data
		/// <summary>
		/// Used this to reset cached data.
		/// </summary>
		public void ResetCache() {
			isReflected = false;
			hasRefOrOut = false;
			fieldInfo = null;
			propertyInfo = null;
			constructorInfo = null;
			eventInfo = null;
			methodInfo = null;
			memberInfo = null;
			parentTarget = null;
			_variableData = null;
			_parameterData = null;
			_genericParameterData = null;
			genericData = null;
			_hasGetMember = false;
			_type = null;
			_startType = null;
			_startTarget = null;
			_displayName = null;
			_parameterTypes = null;
			_genericTypes = null;
			_oldStartType = null;
			_items = null;
			_namePath = null;
			_hasInitializeInstance = false;
			_cachedInstanceData = null;
			_function = null;
			_values = null;
			_disableCache = false;
		}

		/// <summary>
		/// The underlying reflected field, or null if the variable is not field.
		/// </summary>
		public FieldInfo fieldInfo { get; private set; }
		/// <summary>
		/// The underlying property field, or null if the variable is not property.
		/// </summary>
		public PropertyInfo propertyInfo { get; private set; }
		/// <summary>
		/// The underlying constructor field, or null if the variable is not constructor.
		/// </summary>
		public ConstructorInfo constructorInfo { get; private set; }
		/// <summary>
		/// The underlying event field, or null if the variable is not event.
		/// </summary>
		public EventInfo eventInfo { get; private set; }
		/// <summary>
		/// The underlying method field, or null if the variable is a method.
		/// </summary>
		public MethodInfo methodInfo { get; private set; }
		/// <summary>
		/// The list of MemberInfo.
		/// </summary>
		public MemberInfo[] memberInfo { get; private set; }

		[NonSerialized]
		bool _disableCache;
		[NonSerialized]
		public object parentTarget;
		[NonSerialized]
		public bool hasRefOrOut;
		[NonSerialized]
		public TypeData genericData;
		[NonSerialized]
		private Type _oldStartType;
#if UNITY_EDITOR
		[NonSerialized]
		private int undoID;
#endif
		[NonSerialized]
		private string[] _namePath;
		[NonSerialized]
		private uNodeFunction _function;
		[NonSerialized]
		private VariableData _variableData;
		[NonSerialized]
		private ParameterData _parameterData;
		[NonSerialized]
		private GenericParameterData[] _genericParameterData;
		[NonSerialized]
		private object _startTarget;
		[NonSerialized]
		private string _displayName;
		[NonSerialized]
		private bool _hasGetMember;
		[NonSerialized]
		private Type _type;
		[NonSerialized]
		private Type _startType;
		[NonSerialized]
		private Type[][] _parameterTypes;
		[NonSerialized]
		private Type[][] _genericTypes;
		[NonSerialized]
		private ItemData[] _items;
		[NonSerialized]
		private object _instance;
		[NonSerialized]
		private object _cachedInstanceData;
		[NonSerialized]
		private bool _hasInitializeInstance;
		[NonSerialized]
		private object _values;
		#endregion

		#region Static Functions
		/// <summary>
		/// None target of MemberData.
		/// </summary>
		public static MemberData none {
			get {
				return new MemberData();
			}
		}

		/// <summary>
		/// Empty values of a MemberData.
		/// </summary>
		public static MemberData Null {
			get {
				return new MemberData(null, TargetType.Null);
			}
		}

		/// <summary>
		/// Empty values of a MemberData.
		/// </summary>
		public static MemberData empty {
			get {
				return new MemberData() { targetType = TargetType.Values };
			}
		}

		/// <summary>
		/// Create MemberData that target this.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MemberData This(object value) {
			return new MemberData("this", value.GetType(), TargetType.SelfTarget) { instance = value };
		}

		/// <summary>
		/// Create MemberData that target this.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static MemberData This(object value, Type type) {
			return new MemberData("this", type, TargetType.SelfTarget) { instance = value };
		}

		/// <summary>
		/// Clone the MemberData.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static MemberData Clone(MemberData member) {
			return new MemberData(member);
		}

		/// <summary>
		/// Create new MemberData from MemberInfo
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static MemberData CreateFromMember(MemberInfo member) {
			return new MemberData(member);
		}

		/// <summary>
		/// Create new MemberData from MemberInfos
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static MemberData CreateFromMembers(IList<MemberInfo> members) {
			return new MemberData(members);
		}

		/// <summary>
		/// Create a new MemberData that's targeting a type from a full type name
		/// </summary>
		/// <param name="fullTypeName"></param>
		/// <returns></returns>
		public static MemberData CreateFromType(string fullTypeName) {
			var member = new MemberData();
			member.isStatic = true;
			member.name = fullTypeName;
			member.startSerializedType = fullTypeName.ToType(false);
			member.targetType = MemberData.TargetType.Type;
			return member;
		}

		/// <summary>
		/// Create a new MemberData that's targeting a type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static MemberData CreateFromType(Type type) {
			return new MemberData(type);
		}

		/// <summary>
		/// Create a new MemberData that's targeting a value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(object value) {
			return new MemberData(value);
		}

		/// <summary>
		/// Create a new MemberData that's targeting a value with given type
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(object value, Type type) {
			if(object.ReferenceEquals(value, null) && type.IsValueType) {
				value = ReflectionUtils.CreateInstance(type);
			}
			var m = new MemberData(value);
			m.startType = type;
			return m;
		}

		/// <summary>
		/// Create a new MemberData that's targeting a value with given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static MemberData CreateValueFromType(Type type) {
			if(type == null)
				return none;
			object value = null;
			if(ReflectionUtils.CanCreateInstance(type)) {
				if(type == typeof(object)) {
					type = typeof(string);
				}
				value = ReflectionUtils.CreateInstance(type);
			}
			var m = new MemberData(value);
			m.startType = type;
			return m;
		}

		/// <summary>
		/// Create a new MemberData for Variable
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(VariableData variable, UnityEngine.Object owner) {
			return new MemberData(variable, owner, owner is IVariableSystem ? MemberData.TargetType.uNodeVariable : MemberData.TargetType.uNodeLocalVariable);
		}

		/// <summary>
		/// Create a new MemberData for Parameter
		/// </summary>
		/// <param name="parameter"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(ParameterData parameter, IParameterSystem owner) {
			string typeName = parameter.Type.FullName;
			return new MemberData() {
				name = parameter.name,
				instance = owner,
				targetType = TargetType.uNodeParameter,
				targetSerializedType = parameter.Type,
				startSerializedType = parameter.Type,
				isStatic = false,
			};
		}

		/// <summary>
		/// Create a new MemberData for uNodeProperty
		/// </summary>
		/// <param name="value"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(uNodeProperty value, IPropertySystem owner = null) {
			return new MemberData(value, owner);
		}

		/// <summary>
		/// Create a new MemberData for uNodeFunction
		/// </summary>
		/// <param name="value"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(uNodeFunction value, IFunctionSystem owner = null) {
			var mData = new MemberData();
			mData.instance = value.owner;
			if(owner != null) {
				mData.instance = owner;
			}
			mData.name = value.Name;
			List<MemberData.ItemData> itemDatas = new List<MemberData.ItemData>();
			if(value != null) {
				MemberData.ItemData iData = null;
				GenericParameterData[] genericParamArgs = value.genericParameters;
				if(genericParamArgs.Length > 0) {
					throw new System.Exception("Can't Add Function with generic parameter, try using Add Value Node or manually select function");
				}
				ParameterData[] paramsInfo = value.parameters;
				if(paramsInfo.Length > 0) {
					if(iData == null) {
						iData = new MemberData.ItemData();
					}
					iData.parameters = MemberDataUtility.ParameterDataToTypeDatas(paramsInfo, genericParamArgs);
				}
				itemDatas.Add(iData);
			}
			mData.targetSerializedType = value.ReturnType();
			mData.startSerializedType = typeof(MonoBehaviour);
			mData.isStatic = false;
			mData.targetType = MemberData.TargetType.uNodeFunction;
			mData.serializedItems = itemDatas.Select(i => SerializerUtility.SerializeValue(i)).ToArray();
			return mData;
		}

		/// <summary>
		/// Create a new MemberData for uNodeFunction
		/// </summary>
		/// <param name="value"></param>
		/// <param name="genericTypes"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public static MemberData CreateFromValue(uNodeFunction value, Type[] genericTypes, IFunctionSystem owner = null) {
			var mData = new MemberData();
			mData.instance = value.owner;
			if(owner != null) {
				mData.instance = owner;
			}
			mData.name = value.Name;
			List<UnityEngine.Object> genericObjects = new List<UnityEngine.Object>();
			List<MemberData.ItemData> itemDatas = new List<MemberData.ItemData>();
			if(value != null) {
				MemberData.ItemData iData = null;
				GenericParameterData[] genericParamArgs = value.genericParameters;
				if(genericParamArgs.Length > 0) {
					if(genericTypes != null && genericTypes.Length == genericParamArgs.Length) {
						TypeData[] param = new TypeData[genericTypes.Length];
						for(int i = 0; i < genericTypes.Length; i++) {
							param[i] = MemberDataUtility.GetTypeData(genericTypes[i]);
						}
						iData = new MemberData.ItemData() { genericArguments = param };
						genericObjects.Add(value);
					} else {
						throw new System.Exception("Can't Add Function because incorrect given generic types.");
					}
				}
				ParameterData[] paramsInfo = value.parameters;
				if(paramsInfo.Length > 0) {
					if(iData == null) {
						iData = new MemberData.ItemData();
					}
					iData.parameters = MemberDataUtility.ParameterDataToTypeDatas(paramsInfo, genericParamArgs);
					if(genericObjects.Count == 0)
						genericObjects.Add(value);
				}
				itemDatas.Add(iData);
			}
			mData.targetSerializedType = new SerializedType(value.ReturnType());
			mData.startType = typeof(MonoBehaviour);
			mData.isStatic = false;
			mData.targetType = MemberData.TargetType.uNodeFunction;
			mData.serializedItems = itemDatas.Select(i => SerializerUtility.SerializeValue(i)).ToArray();
			if(genericObjects.Count > 0) {
				mData.targetReference = genericObjects;
			}
			return mData;
		}

		#region Connection
		/// <summary>
		/// Create connection for flow input port.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static MemberData FlowInput(Node node) {
			return new MemberData(node, TargetType.FlowNode);
		}

		/// <summary>
		/// Create connection for flow input port.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="fieldName"></param>
		/// <param name="fieldType"></param>
		/// <returns></returns>
		public static MemberData FlowInput(Node node, string fieldName) {
			return new MemberData(new object[] { node, fieldName }, TargetType.FlowInput);
		}

		/// <summary>
		/// Create connection for flow input port.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MemberData FlowInputExtended(IExtendedInput node, string name) {
			return new MemberData(name, null, node, TargetType.FlowInputExtended);
		}

		/// <summary>
		/// Create connection for value output port.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static MemberData ValueOutput(Node target) {
			return new MemberData(target, TargetType.ValueNode);
		}

		/// <summary>
		/// Create connection for field.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="fieldName"></param>
		/// <param name="fieldType"></param>
		/// <returns></returns>
		public static MemberData ValueOutput(NodeComponent node, string fieldName, Type fieldType) {
			return new MemberData(fieldName, fieldType, node, TargetType.NodeField);
		}

		/// <summary>
		/// Create connection for field.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="fieldName"></param>
		/// <param name="elementIndex"></param>
		/// <param name="fieldType"></param>
		/// <returns></returns>
		public static MemberData ValueOutput(NodeComponent node, string fieldName, int elementIndex, Type fieldType) {
			return new MemberData(fieldName + "#" + elementIndex.ToString(), fieldType, node, TargetType.NodeFieldElement);
		}

		/// <summary>
		/// Create connection for output nodes
		/// </summary>
		/// <param name="node"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MemberData ValueOutputExtended(IExtendedOutput node, string name) {
			return new MemberData(name, node.GetOutputType(name), node, TargetType.NodeOutputValue);
		}
		#endregion

		public static bool CanApplyAutoConvert(MemberData member, Type type) {
			if(member == null || type == null)
				return false;
			var mType = member.type;
			if(mType != null && mType != type && mType.IsCastableTo(type)) {
				return true;
			}
			return false;
		}
		#endregion

		#region Editor
		void ISerializationCallbackReceiver.OnBeforeSerialize() {
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			ResetCache();
		}
		#endregion
	}
}