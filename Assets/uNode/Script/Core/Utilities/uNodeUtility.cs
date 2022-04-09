using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode {
	/// <summary>
	/// Class Utility for uNode mostly Function are wrapped to another Class in Editor.
	/// </summary>
	public static class uNodeUtility {
		#region Temp Management
		/// <summary>
		/// A subset of function to management tempolary object (specialized for editor).
		/// </summary>
		public static class TempManagement {
			private static GameObject _managerObject;
			public static GameObject managerObject {
				get {
					if(_managerObject == null) {
						var go = GameObject.Find("[uNode_Temp_ObjectManager]");
						if(go == null) {
							go = new GameObject("[uNode_Temp_ObjectManager]");
#if !UNODE_DEBUG
							go.hideFlags = HideFlags.HideInHierarchy;
#endif
						}
						_managerObject = go;
					}
					return _managerObject;
				}
			}

			private static Transform _tempTransfrom;
			private static Transform tempTransfrom {
				get {
					if(_tempTransfrom == null) {
						_tempTransfrom = managerObject.transform.Find("[TEMP]");
						if(_tempTransfrom == null) {
							var go = new GameObject("[TEMP]");
							go.SetActive(false);
							_tempTransfrom = go.transform;
							_tempTransfrom.SetParent(managerObject.transform);
						}
					}
					return _tempTransfrom;
				}
			}

			public static GameObject CreateTempObject(GameObject instance) {
				if(instance != null) {
					var comp = UnityEngine.Object.Instantiate(instance);
					comp.name = instance.gameObject.name;
					comp.SetActive(false);
					comp.transform.SetParent(tempTransfrom);
#if UNITY_EDITOR
					comp.AddComponent<TemporaryGraph>().prefab = instance;
#endif
					return comp;
				}
				return null;
			}

			public static T CreateTempObject<T>(T instance) where T : Component {
				if(instance != null) {
					var comp = UnityEngine.Object.Instantiate(instance);
					comp.gameObject.name = instance.gameObject.name;
					comp.gameObject.SetActive(false);
					comp.transform.SetParent(tempTransfrom);
#if UNITY_EDITOR
					comp.gameObject.AddComponent<TemporaryGraph>().prefab = instance.gameObject;
#endif
					return comp;
				}
				return null;
			}

			public static T GetTempObject<T>(T instance) where T : Component {
				if(instance != null) {
					var tr = managerObject.transform.Find(instance.gameObject.GetInstanceID().ToString());
					if(tr == null) {
						tr = new GameObject(instance.gameObject.GetInstanceID().ToString()).transform;
						tr.SetParent(managerObject.transform);
					}
					if(tr.childCount == 1) {
						return tr.GetChild(0).GetComponent<T>();
					}
					var comp = UnityEngine.Object.Instantiate(instance);
					comp.gameObject.name = instance.gameObject.name;
					comp.gameObject.SetActive(false);
					comp.transform.SetParent(tr);
#if UNITY_EDITOR
					comp.gameObject.AddComponent<TemporaryGraph>().prefab = instance.gameObject;
#endif
					return comp;
				}
				return null;
			}

			public static bool IsTempObject(Transform transform) {
				if(transform == null || _managerObject == null)
					return false;
				return transform.root == _managerObject.transform;
			}

			public static void DestroyTempObjets() {
				UnityEngine.Object.DestroyImmediate(managerObject);
			}
		}
		#endregion

		#region Callback
		/// <summary>
		/// The callback for GetObjectID, this will filled from uNodeEditorInitializer.
		/// </summary>
		public static Func<UnityEngine.Object, int> getObjectID;
		public static Func<UnityEngine.Object, UnityEngine.Object> getActualObject;
		/// <summary>
		/// The callback for GUIChanged, this will filled from uNodeEditorInitializer.
		/// </summary>
		public static Action guiChanged;
		#endregion

		#region Texture
		private static Texture2D _whiteTexture;
		public static Texture2D WhiteTexture {
			get {
				if(_whiteTexture == null) {
					_whiteTexture = new Texture2D(1, 1);
					_whiteTexture.SetPixel(0, 0, Color.white);
					_whiteTexture.Apply();
				}
				return _whiteTexture;
			}
		}

		private static Texture2D _debugPoint;
		public static Texture2D DebugPoint {
			get {
				if(_debugPoint == null) {
					_debugPoint = Resources.Load<Texture2D>("Debug_Point");
				}
				return _debugPoint;
			}
		}
		#endregion

		#region Editor
		public static Action<UnityEngine.Object, string> RegisterCompleteObjectUndo;
		/// <summary>
		/// this will filled from uNodeEditorInitializer
		/// </summary>
		public static Func<object> debugObject;
		/// <summary>
		/// this will filled from uNodeEditorInitializer
		/// </summary>
		public static Func<EditorTextSetting> richTextColor;
		/// <summary>
		/// this will filled from uNodeEditorInitializer
		/// </summary>
		public static Func<Type, Color> getColorForType;
		/// <summary>
		/// this will filled from uNodeEditorInitializer
		/// </summary>
		public static Action<MemberData, GUIContent, UnityEngine.Object, FilterAttribute> renderVariable;
		/// <summary>
		/// True, if running in editor.
		/// </summary>
		public static bool isInEditor = false;
		public static bool preferredLongName {
			get {
				return preferredDisplay == DisplayKind.Full;
			}
		}
		public static DisplayKind preferredDisplay = DisplayKind.Default;
		public static Func<bool> hideRootObject;

		public static bool isOSXPlatform => Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;

		public static bool isPlaying = true;
		public static int undoID;

		public static GUIContent GetDisplayContent(object value) {
			string name = GetDisplayName(value);
			if(string.IsNullOrEmpty(name)) {
				return GUIContent.none;
			}
			return new GUIContent(name, name);
		}

		public static EditorTextSetting GetRichTextSetting() {
			return richTextColor();
		}

		/// <summary>
		/// Wrap Text with HTML color code
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		private static string WrapWithColor(this string text, Color color) {
			return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
		}

		/// <summary>
		/// Wrap Text with HTML bold code
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string WrapTextWithBold(string text) {
			return string.Format("<b>{0}</b>", text);
		}

		/// <summary>
		/// Wrap Text with HTML italic code
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string WrapTextWithItalic(string text) {
			return string.Format("<i>{0}</i>", text);
		}

		/// <summary>
		/// Wrap Text with HTML color code, the color is keyword color
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string WrapTextWithKeywordColor(string text) {
			return WrapTextWithColor(text, GetRichTextSetting().keywordColor);
		}

		/// <summary>
		/// Wrap Text with HTML color code, the color is type color
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string WrapTextWitTypeColor(string text) {
			return WrapTextWithColor(text, GetRichTextSetting().typeColor);
		}

		/// <summary>
		/// Wrap Text with HTML color code, the color is other color
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string WrapTextWithOtherColor(string text) {
			return WrapTextWithColor(text, GetRichTextSetting().otherColor);
		}

		/// <summary>
		/// Wrap Text with HTML color code
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public static string WrapTextWithColor(string text, Color color) {
			return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
		}

		/// <summary>
		/// Wrap Text with HTML color code
		/// </summary>
		/// <param name="text"></param>
		/// <param name="color"></param>
		/// <param name="ignoreClearColor"></param>
		/// <returns></returns>
		public static string WrapTextWithColor(string text, Color color, bool ignoreClearColor) {
			if(!ignoreClearColor && color.a == 0) {
				return text;
			}
			return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(color), text);
		}

		#region GetNicelyDisplayName
		public static string GetNicelyDisplayName(MultipurposeMember member, bool richName = false) {
			var editorColor = richTextColor();
			if(!editorColor.useRichText) {
				return GetFullDisplayName(member);
			}
			if(member.target == null || !member.target.isAssigned) {
				return "(none)".WrapWithColor(editorColor.otherColor);
			}
			if(member.target.isTargeted) {
				switch(member.target.targetType) {
					case MemberData.TargetType.None:
					case MemberData.TargetType.SelfTarget:
					case MemberData.TargetType.Null:
					case MemberData.TargetType.ValueNode:
					case MemberData.TargetType.NodeField:
					case MemberData.TargetType.NodeFieldElement:
					case MemberData.TargetType.FlowInput:
					case MemberData.TargetType.FlowNode:
						return GetNicelyDisplayName(member.target, richName: richName);
					case MemberData.TargetType.Values:
						return GetNicelyDisplayName(member.target.Get(), richName: richName);
				}
				string result = null;
				var mTarget = member.target;
				string[] names = mTarget.namePath;
				if(mTarget.SerializedItems.Length == names.Length) {
					if(mTarget.targetType == MemberData.TargetType.Constructor) {
						result += "new".WrapWithColor(editorColor.keywordColor) + " " + mTarget.type.PrettyName().WrapWithColor(editorColor.typeColor);
					}
					int skipIndex = 0;
					Color typeColor;
					if(mTarget.type != null) {
						typeColor = getColorForType(mTarget.type);
					} else {
						typeColor = editorColor.typeColor;
					}
					for(int i = 0; i < names.Length; i++) {
						if(!string.IsNullOrEmpty(result) && (mTarget.targetType != MemberData.TargetType.Constructor)) {
							result += ".";
						}
						if(mTarget.targetType != MemberData.TargetType.uNodeGenericParameter &&
							mTarget.targetType != MemberData.TargetType.Type &&
							mTarget.targetType != MemberData.TargetType.Constructor) {

							if(i == 0) {
								switch(mTarget.targetType) {
									case MemberData.TargetType.uNodeVariable:
									case MemberData.TargetType.uNodeGroupVariable:
									case MemberData.TargetType.uNodeLocalVariable:
									case MemberData.TargetType.uNodeProperty:
										result += ("$" + names[i]).WrapWithColor(getColorForType(mTarget.startType));
										break;
									case MemberData.TargetType.uNodeFunction:
									case MemberData.TargetType.uNodeGenericParameter:
									case MemberData.TargetType.uNodeParameter:
										result += names[i].WrapWithColor(getColorForType(mTarget.startType));
										break;
									default:
										if(!member.target.isDeepTarget && preferredDisplay == DisplayKind.Partial && names.Length > 1 && !member.target.IsTargetingUNode) {
											break;
										}
										if(mTarget.isStatic) {
											result += names[i].WrapWithColor(editorColor.typeColor);
										} else if(names.Length > 1) {
											result += names[i].WrapWithColor(getColorForType(mTarget.startType));
										} else {
											result += names[i].WrapWithColor(typeColor);
										}
										break;
								}
							} else if(i + 1 == names.Length) {
								result += names[i].WrapWithColor(typeColor);
							} else {
								result += names[i].WrapWithColor(editorColor.misleadingColor);
							}
						}
						MemberData.ItemData iData = mTarget.Items[i];
						if(iData != null) {
							string[] paramsType;
							string[] genericType;
							MemberDataUtility.GetItemName(mTarget.Items[i],
								mTarget.targetReference,
								out genericType,
								out paramsType);
							if(genericType.Length > 0) {
								for(int x = 0; x < genericType.Length; x++) {
									Type t = genericType[x].ToType(false);
									genericType[x] = t.PrettyName().WrapWithColor(getColorForType(t));
								}
								if(mTarget.targetType != MemberData.TargetType.uNodeGenericParameter &&
									mTarget.targetType != MemberData.TargetType.Type) {

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
								mTarget.targetType == MemberData.TargetType.uNodeFunction ||
								mTarget.targetType == MemberData.TargetType.uNodeConstructor ||
								mTarget.targetType == MemberData.TargetType.Constructor ||
								mTarget.targetType == MemberData.TargetType.Method && !mTarget.isDeepTarget) {
								if(member.parameters.Length - skipIndex >= paramsType.Length) {
									for(int x = 0; x < paramsType.Length; x++) {
										paramsType[x] = GetNicelyDisplayName(member.parameters[x + skipIndex], preferredLongName, true, richName: richName);
									}
									result += string.Format("({0})", string.Join(", ", paramsType));
									skipIndex += paramsType.Length;
								}
							}
						}
					}
				}
				if(!string.IsNullOrEmpty(result)) {
					return result;
				}
			}
			return GetNicelyDisplayName(member.target, true, true, richName: richName);
		}

		public static string GetNicelyDisplayName(NodeComponent node) {
			return node?.GetRichName();
		}

		public static string GetNicelyDisplayName(this MemberData member, bool longName, bool typeTargetWithTypeof = true, bool richName = false) {
			return GetNicelyDisplayName(member, longName ? DisplayKind.Full : DisplayKind.Default, typeTargetWithTypeof, richName);
		}

		public static string GetNicelyDisplayName(this MemberData member, DisplayKind displayKind = DisplayKind.Default, bool typeTargetWithTypeof = true, bool richName = false) {
			var editorColor = richTextColor();
			if(!editorColor.useRichText) {
				return GetDisplayName(member, displayKind, typeTargetWithTypeof);
			}
			if(member == null)
				return "(none)".WrapWithColor(editorColor.otherColor);
			switch(member.targetType) {
				case MemberData.TargetType.None:
					return "(none)".WrapWithColor(editorColor.otherColor);
				case MemberData.TargetType.SelfTarget:
					return "this".WrapWithColor(editorColor.keywordColor);
				case MemberData.TargetType.Null:
					return "null".WrapWithColor(editorColor.keywordColor);
				case MemberData.TargetType.FlowNode:
				case MemberData.TargetType.ValueNode:
					if(member.GetTargetNode() == null) {
						goto case MemberData.TargetType.None;
					}
					if(richName) {
						return GetNicelyDisplayName(member.GetTargetNode());
					}
					return "#Node".WrapWithColor(member.type != null ? getColorForType(member.type) : editorColor.otherColor);
				case MemberData.TargetType.NodeField:
				case MemberData.TargetType.NodeFieldElement:
				case MemberData.TargetType.FlowInput:
					if(member.GetTargetNode() == null) {
						goto case MemberData.TargetType.None;
					}
					return "#Port".WrapWithColor(member.type != null ? getColorForType(member.type) : editorColor.otherColor);
				case MemberData.TargetType.Values:
					return GetNicelyDisplayName(member.Get());
					//if(member.type != null) {
					//	return member.type.PrettyName().WrapWithColor(editorColor.typeColor);
					//}
					//return member.targetTypeName.WrapWithColor(editorColor.typeColor);

			}
			string[] names = member.namePath;
			string result = null;
			if(member.isTargeted && member.SerializedItems?.Length > 0) {
				if(member.SerializedItems.Length == names.Length) {
					if(member.targetType == MemberData.TargetType.Constructor) {
						result += "new".WrapWithColor(editorColor.keywordColor) + " " + member.type.PrettyName().WrapWithColor(editorColor.typeColor);
					}
					Color typeColor;
					if(member.type != null) {
						typeColor = getColorForType(member.type);
					} else {
						typeColor = editorColor.typeColor;
					}
					for(int i = 0; i < names.Length; i++) {
						if(!string.IsNullOrEmpty(result) && (member.targetType != MemberData.TargetType.Constructor)) {
							result += ".";
						}
						if(member.targetType != MemberData.TargetType.uNodeGenericParameter &&
							member.targetType != MemberData.TargetType.Type &&
							member.targetType != MemberData.TargetType.Constructor) {
							if(i == 0) {
								switch(member.targetType) {
									case MemberData.TargetType.uNodeVariable:
									case MemberData.TargetType.uNodeGroupVariable:
									case MemberData.TargetType.uNodeLocalVariable:
										result += ("$" + member.startName).WrapWithColor(getColorForType(member.startType));
										break;
									case MemberData.TargetType.uNodeProperty:
										result += ("$" + member.startName).WrapWithColor(getColorForType(member.startType));
										break;
									case MemberData.TargetType.uNodeFunction:
									case MemberData.TargetType.uNodeGenericParameter:
									case MemberData.TargetType.uNodeParameter:
										result += member.startName.WrapWithColor(getColorForType(member.startType));
										break;
									default:
										if(!member.isDeepTarget && displayKind == DisplayKind.Partial && names.Length > 1 && !member.IsTargetingUNode) {
											break;
										}
										MemberDataUtility.UpdateMemberNames(member);
										if(member.isStatic) {
											result += member.startName.WrapWithColor(editorColor.typeColor);
										} else if(names.Length > 1) {
											result += member.startName.WrapWithColor(getColorForType(member.startType));
										} else {
											result += member.startName.WrapWithColor(typeColor);
										}
										break;
								}
							} else if(i + 1 == names.Length) {
								result += names[i].WrapWithColor(typeColor);
							} else {
								result += names[i].WrapWithColor(editorColor.misleadingColor);
							}
						}
						MemberData.ItemData iData = member.Items[i];
						if(iData != null) {
							string[] paramsType;
							string[] genericType;
							MemberDataUtility.GetItemName(member.Items[i],
								member.targetReference,
								out genericType,
								out paramsType);
							if(genericType.Length > 0) {
								for(int x = 0; x < genericType.Length; x++) {
									Type t = genericType[x].ToType(false);
									if(t != null) {
										genericType[x] = t.PrettyName().WrapWithColor(getColorForType(t));
									}
								}
								if(member.targetType != MemberData.TargetType.uNodeGenericParameter && member.targetType != MemberData.TargetType.Type) {
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
							if(displayKind == DisplayKind.Full) {
								if(paramsType.Length > 0 ||
									member.targetType == MemberData.TargetType.uNodeFunction ||
									member.targetType == MemberData.TargetType.uNodeConstructor ||
									member.targetType == MemberData.TargetType.Constructor ||
									member.targetType == MemberData.TargetType.Method && !member.isDeepTarget) {
									for(int x = 0; x < paramsType.Length; x++) {
										Type t = paramsType[x].ToType(false);
										if(t == null) {
											paramsType[x] = paramsType[x].WrapWithColor(getColorForType(null));
										} else {
											paramsType[x] = t.PrettyName().WrapWithColor(getColorForType(t));
										}
									}
									result += string.Format("({0})", string.Join(", ", paramsType));
								}
							}
						}
					}
				}
			}
			if(!string.IsNullOrEmpty(result)) {
				if(member.targetType == MemberData.TargetType.Type) {
					if(!typeTargetWithTypeof) {
						return result;
					}
					return "typeof".WrapWithColor(editorColor.keywordColor) + "(" + result + ")";
				}
				return result;
			}
			switch(member.targetType) {
				case MemberData.TargetType.uNodeVariable:
				case MemberData.TargetType.uNodeGroupVariable:
				case MemberData.TargetType.uNodeLocalVariable:
				case MemberData.TargetType.uNodeProperty:
					return ("$" + member.name).WrapWithColor(getColorForType(member.type));
				case MemberData.TargetType.uNodeGenericParameter:
				case MemberData.TargetType.uNodeParameter:
					return member.name.WrapWithColor(getColorForType(member.type));
				case MemberData.TargetType.Type:
					if(!typeTargetWithTypeof) {
						return member.startType.PrettyName().WrapWithColor(editorColor.typeColor);
					}
					return "typeof".WrapWithColor(editorColor.keywordColor) + "(" + member.startType.PrettyName().WrapWithColor(editorColor.typeColor) + ")";
			}
			return !string.IsNullOrEmpty(member.name) ? member.name : "(none)".WrapWithColor(editorColor.otherColor);
		}

		public static string GetNicelyDisplayName(object value, bool richName = false) {
			var editorColor = richTextColor();
			if(!editorColor.useRichText) {
				return GetFullDisplayName(value);
			}
			if(!object.ReferenceEquals(value, null)) {
				if(value is MemberData) {
					MemberData member = value as MemberData;
					if(member.isTargeted) {
						switch(member.targetType) {
							case MemberData.TargetType.Values:
								return GetNicelyDisplayName(member.Get(), richName: richName);
							default:
								return GetNicelyDisplayName(member, preferredLongName, true, richName: richName);
						}
					}
					return "(none)".WrapWithColor(editorColor.otherColor);
				} else if(value is MultipurposeMember) {
					MultipurposeMember member = value as MultipurposeMember;
					return GetNicelyDisplayName(member, richName: richName);
				} else if(value is UnityEngine.Object) {
					if(!(value is Component)) {
						return (value as UnityEngine.Object).name;
					}
				} else if(value is string) {
					return ("\"" + value.ToString() + "\"").WrapWithColor(getColorForType(typeof(string)));
				} else if(value is Type) {//Type
					Type type = value as Type;
					if(type.IsInterface) {
						return type.PrettyName().WrapWithColor(editorColor.interfaceColor);
					} else if(type.IsEnum) {
						return type.PrettyName().WrapWithColor(editorColor.enumColor);
					}
					return type.PrettyName().WrapWithColor(editorColor.typeColor);
				} else if(value is Vector2) {//Vector2
					Vector2 vec = (Vector2)value;
					if(vec == Vector2.down) {
						return "Vector2".WrapWithColor(editorColor.typeColor) + "." + "down".WrapWithColor(getColorForType(typeof(Vector2)));
					} else if(vec == Vector2.left) {
						return "Vector2".WrapWithColor(editorColor.typeColor) + "." + "left".WrapWithColor(getColorForType(typeof(Vector2)));
					} else if(vec == Vector2.one) {
						return "Vector2".WrapWithColor(editorColor.typeColor) + "." + "one".WrapWithColor(getColorForType(typeof(Vector2)));
					} else if(vec == Vector2.right) {
						return "Vector2".WrapWithColor(editorColor.typeColor) + "." + "right".WrapWithColor(getColorForType(typeof(Vector2)));
					} else if(vec == Vector2.up) {
						return "Vector2".WrapWithColor(editorColor.typeColor) + "." + "up".WrapWithColor(getColorForType(typeof(Vector2)));
					} else if(vec == Vector2.zero) {
						return "Vector2".WrapWithColor(editorColor.typeColor) + "." + "zero".WrapWithColor(getColorForType(typeof(Vector2)));
					}
					string result = "new".WrapWithColor(editorColor.keywordColor) + " Vector2".WrapWithColor(editorColor.typeColor);
					result += "(";
					result += vec.x.ToString().WrapWithColor(getColorForType(typeof(float))) + ", ";
					result += vec.y.ToString().WrapWithColor(getColorForType(typeof(float))) + ")";
					return result;
				} else if(value is Vector3) {//Vector3
					Vector3 vec = (Vector3)value;
					if(vec == Vector3.back) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "back".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.down) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "down".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.forward) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "forward".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.left) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "left".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.one) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "one".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.right) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "right".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.up) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "up".WrapWithColor(getColorForType(typeof(Vector3)));
					} else if(vec == Vector3.zero) {
						return "Vector3".WrapWithColor(editorColor.typeColor) + "." + "zero".WrapWithColor(getColorForType(typeof(Vector3)));
					}
					string result = "new".WrapWithColor(editorColor.keywordColor) + " Vector3".WrapWithColor(editorColor.typeColor);
					result += "(";
					result += vec.x.ToString().WrapWithColor(getColorForType(typeof(float))) + ", ";
					result += vec.y.ToString().WrapWithColor(getColorForType(typeof(float))) + ", ";
					result += vec.z.ToString().WrapWithColor(getColorForType(typeof(float))) + ")";
					return result;
				} else if(value is Color) {//Color
					Color color = (Color)value;
					if(color == Color.black) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "black".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.blue) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "blue".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.clear) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "clear".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.cyan) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "cyan".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.gray) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "gray".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.green) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "green".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.grey) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "grey".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.magenta) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "magenta".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.red) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "red".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.white) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "white".WrapWithColor(getColorForType(typeof(Color)));
					} else if(color == Color.yellow) {
						return "Color".WrapWithColor(editorColor.typeColor) + "." + "yellow".WrapWithColor(getColorForType(typeof(Color)));
					}
					string result = "new".WrapWithColor(editorColor.keywordColor) + " Color".WrapWithColor(editorColor.typeColor);
					result += "(";
					result += color.r.ToString("0.##").WrapWithColor(getColorForType(typeof(float))) + ", ";
					result += color.g.ToString("0.##").WrapWithColor(getColorForType(typeof(float))) + ", ";
					result += color.b.ToString("0.##").WrapWithColor(getColorForType(typeof(float))) + ", ";
					result += color.a.ToString("0.##").WrapWithColor(getColorForType(typeof(float))) + ")";
					return result;
				} else if(value is Enum) {//Enum
					if(value is ComparisonType || value is ArithmeticType || value is SetType) {
						return GetDisplayName(value);
					} else {
						return value.ToString().WrapWithColor(editorColor.enumColor);
					}
				} else if(value is MethodInfo) {
					return GetNicelyMethodName(value as MethodInfo);
				}
				if(value.GetType().IsPrimitive) {
					return value.ToString().WrapWithColor(getColorForType(value.GetType()));
				}
				return value.GetType().PrettyName().WrapWithColor(getColorForType(value.GetType()));
			}
			return "null".WrapWithColor(editorColor.keywordColor);
		}

		public static string GetNicelyMethodName(this MethodInfo method, bool includeReturnType = true) {
			ParameterInfo[] info = method.GetParameters();
			string mConstructur = null;
			if(method.IsGenericMethod) {
				foreach(Type arg in method.GetGenericArguments()) {
					if(string.IsNullOrEmpty(mConstructur)) {
						mConstructur += "<" + GetNicelyDisplayName(arg);
						continue;
					}
					mConstructur += "," + GetNicelyDisplayName(arg);
				}
				mConstructur += ">";
			}
			mConstructur += "(";
			for(int i = 0; i < info.Length; i++) {
				mConstructur += GetNicelyDisplayName(info[i].ParameterType) + " " + info[i].Name;
				if(i + 1 < info.Length) {
					mConstructur += ", ";
				}
			}
			mConstructur += ")";
			if(includeReturnType) {
				return GetNicelyDisplayName(method.ReturnType) + " " + method.Name + mConstructur;
			} else {
				return method.Name + mConstructur;
			}
		}
		#endregion

		#region GetDisplayName
		public static string GetFullDisplayName(object value) {
			if(value is MemberData) {
				return GetDisplayName(value as MemberData, true, true);
			} else if(value is MultipurposeMember) {
				MultipurposeMember member = value as MultipurposeMember;
				if(member.target != null) {
					if(member.target.isAssigned) {
						switch(member.target.targetType) {
							case MemberData.TargetType.None:
							case MemberData.TargetType.SelfTarget:
							case MemberData.TargetType.Null:
							case MemberData.TargetType.ValueNode:
							case MemberData.TargetType.NodeField:
							case MemberData.TargetType.NodeFieldElement:
							case MemberData.TargetType.FlowInput:
							case MemberData.TargetType.FlowNode:
							case MemberData.TargetType.Values:
								return GetFullDisplayName(member.target.Get());
						}
						string result = null;
						var mTarget = member.target;
						string[] names = mTarget.namePath;
						if(mTarget.SerializedItems.Length == names.Length) {
							if(mTarget.targetType == MemberData.TargetType.Constructor) {
								result += "new" + " " + mTarget.type.PrettyName();
							}
							int skipIndex = 0;
							for(int i = 0; i < names.Length; i++) {
								if(i != 0 && (mTarget.targetType != MemberData.TargetType.Constructor)) {
									result += ".";
								}
								if(mTarget.targetType != MemberData.TargetType.uNodeGenericParameter &&
									mTarget.targetType != MemberData.TargetType.Type &&
									mTarget.targetType != MemberData.TargetType.Constructor) {

									if(i == 0) {
										switch(mTarget.targetType) {
											case MemberData.TargetType.uNodeVariable:
											case MemberData.TargetType.uNodeGroupVariable:
											case MemberData.TargetType.uNodeLocalVariable:
											case MemberData.TargetType.uNodeProperty:
												result += ("$" + names[i]);
												break;
											case MemberData.TargetType.uNodeFunction:
											case MemberData.TargetType.uNodeGenericParameter:
											case MemberData.TargetType.uNodeParameter:
												result += names[i];
												break;
											default:
												if(names.Length > 1) {
													result += names[i];
												} else {
													result += names[i];
												}
												break;
										}
									} else {
										result += names[i];
									}
								}
								MemberData.ItemData iData = mTarget.Items[i];
								if(iData != null) {
									string[] paramsType;
									string[] genericType;
									MemberDataUtility.GetItemName(mTarget.Items[i],
										mTarget.targetReference,
										out genericType,
										out paramsType);
									if(genericType.Length > 0) {
										for(int x = 0; x < genericType.Length; x++) {
											Type t = genericType[x].ToType(false);
											genericType[x] = t.PrettyName();
										}
										if(mTarget.targetType != MemberData.TargetType.uNodeGenericParameter &&
											mTarget.targetType != MemberData.TargetType.Type) {

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
										mTarget.targetType == MemberData.TargetType.uNodeFunction ||
										mTarget.targetType == MemberData.TargetType.uNodeConstructor ||
										mTarget.targetType == MemberData.TargetType.Constructor ||
										mTarget.targetType == MemberData.TargetType.Method && !mTarget.isDeepTarget) {
										if(member.parameters.Length - skipIndex >= paramsType.Length) {
											for(int x = 0; x < paramsType.Length; x++) {
												paramsType[x] = GetFullDisplayName(member.parameters[x + skipIndex]);
											}
											result += string.Format("({0})", string.Join(", ", paramsType));
											skipIndex += paramsType.Length;
										}
									}
								}
							}
						}
						if(!string.IsNullOrEmpty(result)) {
							return result;
						}
					}
					return GetDisplayName(member.target, true, true);
				} else {
					return "Unassigned";
				}
			}
			return GetDisplayName(value);
		}

		public static string GetDisplayName(this MemberData member, bool longName = false, bool typeTargetWithTypeof = true) {
			if(member == null || !member.isAssigned) {
				return "(none)";
			}
			return member.DisplayName(longName, typeTargetWithTypeof);
		}

		public static string GetDisplayName(this MemberData member, DisplayKind displayKind, bool typeTargetWithTypeof = true) {
			if(member == null || !member.isAssigned) {
				return "(none)";
			}
			if(displayKind != DisplayKind.Partial || member.IsTargetingUNode) {
				return member.DisplayName(displayKind == DisplayKind.Full, typeTargetWithTypeof);
			}
			string result = member.DisplayName(displayKind == DisplayKind.Full, typeTargetWithTypeof);
			if(!member.isDeepTarget && member.namePath.Length > 1) {
				int index = result.IndexOf('.');
				return result.Substring(0, index > 0 ? index : 0);
			}
			return result;
		}

		public static string GetDebugName(object value) {
			if(!object.ReferenceEquals(value, null)) {
				if(value is MemberData) {
					MemberData member = value as MemberData;
					if(member.isAssigned) {
						return GetDisplayName(member, preferredLongName, true);
					}
					return "(none)";
				} else if(value is MultipurposeMember) {
					MultipurposeMember member = value as MultipurposeMember;
					if(member.target != null) {
						return GetDisplayName(member.target, preferredLongName, true);
					} else {
						return "Unassigned";
					}
				} else if(value is UnityEngine.Object uobj && uobj != null) {
					if(!(value is Component)) {
						return uobj.name;
					}
				} else if(value is string) {
					return "\"" + value.ToString() + "\"";
				} else if(value is Type) {//Type
					return (value as Type).PrettyName();
				} else if(value is Vector2) {//Vector2
					Vector2 vec = (Vector2)value;
					return $"({vec.x}, {vec.y})";
				} else if(value is Vector2Int) {//Vector2Int
					Vector2Int vec = (Vector2Int)value;
					return $"({vec.x}, {vec.y})";
				} else if(value is Vector3) {//Vector3
					Vector3 vec = (Vector3)value;
					return $"({vec.x}, {vec.y}, {vec.z})";
				} else if(value is Vector3Int) {//Vector3Int
					Vector3 vec = (Vector3Int)value;
					return $"({vec.x}, {vec.y}, {vec.z})";
				} else if(value is Vector4) {//Vector4
					Vector4 vec = (Vector4)value;
					return $"({vec.x}, {vec.y}, {vec.z}, {vec.w})";
				} else if(value is Color) {//Color
					Color color = (Color)value;
					if(color == Color.black) {
						return "Color.black";
					} else if(color == Color.blue) {
						return "Color.blue";
					} else if(color == Color.clear) {
						return "Color.clear";
					} else if(color == Color.cyan) {
						return "Color.cyan";
					} else if(color == Color.gray) {
						return "Color.gray";
					} else if(color == Color.green) {
						return "Color.green";
					} else if(color == Color.grey) {
						return "Color.grey";
					} else if(color == Color.magenta) {
						return "Color.magenta";
					} else if(color == Color.red) {
						return "Color.red";
					} else if(color == Color.white) {
						return "Color.white";
					} else if(color == Color.yellow) {
						return "Color.yellow";
					}
					return $"({color.r}, {color.g}, {color.b}, {color.a})";
				} else if(value is Enum) {//Enum
					if(value is ComparisonType) {
						switch((ComparisonType)value) {
							case ComparisonType.Equal:
								return "==";
							case ComparisonType.GreaterThan:
								return ">";
							case ComparisonType.GreaterThanOrEqual:
								return ">=";
							case ComparisonType.LessThan:
								return "<";
							case ComparisonType.LessThanOrEqual:
								return "<=";
							case ComparisonType.NotEqual:
								return "!=";
						}
					} else if(value is ArithmeticType) {
						switch((ArithmeticType)value) {
							case ArithmeticType.Add:
								return "+";
							case ArithmeticType.Divide:
								return "/";
							case ArithmeticType.Modulo:
								return "%";
							case ArithmeticType.Multiply:
								return "*";
							case ArithmeticType.Subtract:
								return "-";
						}
					} else if(value is SetType) {
						switch((SetType)value) {
							case SetType.Add:
								return "+=";
							case SetType.Change:
								return "=";
							case SetType.Divide:
								return "/=";
							case SetType.Modulo:
								return "%=";
							case SetType.Multiply:
								return "*=";
							case SetType.Subtract:
								return "-=";
						}
					} else {
						return value.ToString();
					}
				}
				return value.ToString();
			}
			return "null";
		}

		public static string GetDisplayName(object value) {
			if(!object.ReferenceEquals(value, null)) {
				if(value is MemberData) {
					MemberData member = value as MemberData;
					if(member.isAssigned) {
						return GetDisplayName(member, preferredLongName, true);
					}
					return "(none)";
				} else if(value is MultipurposeMember) {
					MultipurposeMember member = value as MultipurposeMember;
					if(member.target != null) {
						return GetDisplayName(member.target, preferredLongName, true);
					} else {
						return "Unassigned";
					}
				} else if(value is UnityEngine.Object uobj && uobj != null) {
					if(!(value is Component)) {
						return uobj.name;
					}
				} else if(value is string) {
					return "\"" + value.ToString() + "\"";
				} else if(value is Type) {//Type
					return (value as Type).PrettyName();
				} else if(value is Vector2) {//Vector2
					Vector2 vec = (Vector2)value;
					if(vec == Vector2.down) {
						return "Vector2.down";
					} else if(vec == Vector2.left) {
						return "Vector2.left";
					} else if(vec == Vector2.one) {
						return "Vector2.one";
					} else if(vec == Vector2.right) {
						return "Vector2.right";
					} else if(vec == Vector2.up) {
						return "Vector2.up";
					} else if(vec == Vector2.zero) {
						return "Vector2.zero";
					}
					return $"({vec.x}, {vec.y})";
				} else if(value is Vector3) {//Vector3
					Vector3 vec = (Vector3)value;
					if(vec == Vector3.back) {
						return "Vector3.back";
					} else if(vec == Vector3.down) {
						return "Vector3.down";
					} else if(vec == Vector3.forward) {
						return "Vector3.forward";
					} else if(vec == Vector3.left) {
						return "Vector3.left";
					} else if(vec == Vector3.one) {
						return "Vector3.one";
					} else if(vec == Vector3.right) {
						return "Vector3.right";
					} else if(vec == Vector3.up) {
						return "Vector3.up";
					} else if(vec == Vector3.zero) {
						return "Vector3.zero";
					}
					return $"({vec.x}, {vec.y}, {vec.z})";
				} else if(value is Color) {//Color
					Color color = (Color)value;
					if(color == Color.black) {
						return "Color.black";
					} else if(color == Color.blue) {
						return "Color.blue";
					} else if(color == Color.clear) {
						return "Color.clear";
					} else if(color == Color.cyan) {
						return "Color.cyan";
					} else if(color == Color.gray) {
						return "Color.gray";
					} else if(color == Color.green) {
						return "Color.green";
					} else if(color == Color.grey) {
						return "Color.grey";
					} else if(color == Color.magenta) {
						return "Color.magenta";
					} else if(color == Color.red) {
						return "Color.red";
					} else if(color == Color.white) {
						return "Color.white";
					} else if(color == Color.yellow) {
						return "Color.yellow";
					}
					return $"({color.r}, {color.g}, {color.b}, {color.a})";
				} else if(value is Enum) {//Enum
					if(value is ComparisonType) {
						switch((ComparisonType)value) {
							case ComparisonType.Equal:
								return "==";
							case ComparisonType.GreaterThan:
								return ">";
							case ComparisonType.GreaterThanOrEqual:
								return ">=";
							case ComparisonType.LessThan:
								return "<";
							case ComparisonType.LessThanOrEqual:
								return "<=";
							case ComparisonType.NotEqual:
								return "!=";
						}
					} else if(value is ArithmeticType) {
						switch((ArithmeticType)value) {
							case ArithmeticType.Add:
								return "+";
							case ArithmeticType.Divide:
								return "/";
							case ArithmeticType.Modulo:
								return "%";
							case ArithmeticType.Multiply:
								return "*";
							case ArithmeticType.Subtract:
								return "-";
						}
					} else if(value is SetType) {
						switch((SetType)value) {
							case SetType.Add:
								return "+=";
							case SetType.Change:
								return "=";
							case SetType.Divide:
								return "/=";
							case SetType.Modulo:
								return "%=";
							case SetType.Multiply:
								return "*=";
							case SetType.Subtract:
								return "-=";
						}
					} else {
						return value.ToString();
					}
				}
				if(value.GetType().IsPrimitive) {
					return value.ToString();
				}
				return (value.GetType()).PrettyName();
			}
			return "null";
		}
		#endregion

		/// <summary>
		/// Are node to node is stack overflow
		/// </summary>
		/// <param name="fromNode"></param>
		/// <param name="nodeToFind"></param>
		/// <returns></returns>
		public static bool IsStackOverflow(NodeComponent fromNode, NodeComponent nodeToFind = null) {
			return IsStackOverflow(fromNode, nodeToFind, new HashSet<NodeComponent>());
		}

		/// <summary>
		/// Are node to node is stack overflow
		/// </summary>
		/// <param name="fromNode"></param>
		/// <param name="nodeToFind"></param>
		/// <param name="connections"></param>
		/// <returns></returns>
		public static bool IsStackOverflow(NodeComponent fromNode, NodeComponent nodeToFind, HashSet<NodeComponent> connections) {
			if(nodeToFind == null) {
				nodeToFind = fromNode;
			}
			connections.Add(fromNode);
			if(fromNode is StateNode) {
				StateNode eventNode = fromNode as StateNode;
				TransitionEvent[] TE = eventNode.GetTransitions();
				foreach(TransitionEvent transition in TE) {
					if(transition.GetTargetNode()) {
						if(transition.GetTargetNode() == nodeToFind)
							return true;
						if(!connections.Contains(transition.GetTargetNode()) && IsStackOverflow(transition.GetTargetNode(), nodeToFind, connections)) {
							return true;
						}
					}
				}
			}
			bool isFound = false;
			Func<object, bool> validation = delegate (object obj) {
				if(isFound)
					return true;
				if(obj is MemberData) {
					MemberData member = obj as MemberData;
					if(member.targetType.HasFlags(MemberData.TargetType.FlowNode | MemberData.TargetType.FlowInput)) {
						var tNode = member.GetTargetNode();
						if(!connections.Contains(tNode)) {
							isFound = tNode == nodeToFind;
							if(isFound)
								return false;
							isFound = IsStackOverflow(tNode, nodeToFind, connections);
							if(isFound)
								return false;
						}
					}
				}
				return false;
			};
			if(fromNode is IMacro) {
				var macro = fromNode as IMacro;
				var flows = macro.OutputFlows;
			}
			AnalizerUtility.AnalizeObject(fromNode, validation);
			return isFound;
		}

		public static Dictionary<Object, List<ErrorMessage>> editorErrorMap;

		public static void RegisterEditorError(Object owner, string message, Action<Vector2> onClicked = null) {
			if(editorErrorMap == null) {
				editorErrorMap = new Dictionary<Object, List<ErrorMessage>>(EqualityComparer<Object>.Default);
			}
			var error = new ErrorMessage() { message = message, obj = owner, onClicked = onClicked };
			List<ErrorMessage> list;
			if(!editorErrorMap.TryGetValue(owner, out list)) {
				list = new List<ErrorMessage>();
				editorErrorMap[owner] = list;
			}
			list.Add(error);
		}

		public static void RegisterEditorError(uNodeRoot owner, Object obj, string message, Action<Vector2> onClicked = null) {
			if(editorErrorMap == null) {
				editorErrorMap = new Dictionary<Object, List<ErrorMessage>>(EqualityComparer<Object>.Default);
			}
			var error = new ErrorMessage() { message = message, obj = obj, onClicked = onClicked };
			List<ErrorMessage> list;
			if(!editorErrorMap.TryGetValue(owner, out list)) {
				list = new List<ErrorMessage>();
				editorErrorMap[owner] = list;
			}
			list.Add(error);
		}

		public static void ClearEditorError(UnityEngine.Object owner) {
			if(editorErrorMap == null)
				return;
			if(editorErrorMap.TryGetValue(owner, out var list)) {
				list.Clear();
			}
		}

		public static List<ErrorMessage> GetEditorError(UnityEngine.Object owner) {
			if(editorErrorMap == null)
				return null;
			if(editorErrorMap.TryGetValue(owner, out var list)) {
				return list;
			}
			return null;
		}
		#endregion

		#region Runtime
		private static uNodeResourceDatabase resourceDatabase;
		public static uNodeResourceDatabase GetDatabase() {
			if(resourceDatabase == null) {
				resourceDatabase = Resources.Load<uNodeResourceDatabase>("uNodeDatabase");
			}
			return resourceDatabase;
		}

		public static T RuntimeGetValue<T>(Func<T> func) {
			return func();
		}
		#endregion

		#region Others
		/// <summary>
		/// Class setting for edit value
		/// </summary>
		public class EditValueSettings {
			/// <summary>
			/// The parent object.
			/// </summary>
			public object parentValue;
			/// <summary>
			/// Allow UnityReference to be edited.
			/// </summary>
			public bool acceptUnityObject = true;
			/// <summary>
			/// Allow reference/class object to be null.
			/// </summary>
			public bool nullable = false;
			public UnityEngine.Object unityObject;
			public object[] attributes;

			public string Tooltip {
				get {
					if(attributes != null) {
						for(int i = 0; i < attributes.Length; i++) {
							if(attributes[i] is TooltipAttribute tooltip) {
								return tooltip.tooltip;
							}
						}
					}
					return string.Empty;
				}
			}

			public bool drawDecorator = true;

			public EditValueSettings() { }

			public EditValueSettings(object parentInstance) {
				this.parentValue = parentInstance;
			}

			public EditValueSettings(bool acceptUnityObject, bool nullable, object parentInstance = null) {
				this.acceptUnityObject = acceptUnityObject;
				this.nullable = nullable;
				this.parentValue = parentInstance;
			}

			public EditValueSettings(EditValueSettings other) {
				this.parentValue = other.parentValue;
				this.nullable = other.nullable;
				this.acceptUnityObject = other.acceptUnityObject;
				this.attributes = other.attributes;
				this.unityObject = other.unityObject;
				this.drawDecorator = other.drawDecorator;
			}
		}

		/// <summary>
		/// Validate that variable name is valid
		/// </summary>
		/// <param name="Name">The name to validate</param>
		/// <param name="otherNames">Optional: other name to validate that contains Name</param>
		/// <returns></returns>
		public static bool IsValidVariableName(string variableName, IList<string> otherNames = null) {
			if(string.IsNullOrEmpty(variableName) || otherNames != null && otherNames.Contains(variableName)) {
				return false;
			}
			if(variableName.Length == 0)
				return false;
			int n;
			if(variableName.Length > 0 && int.TryParse(variableName[0].ToString(), out n)) {
				return false;
			}
			if(variableName.Contains('.') ||
				variableName.Contains(' ') ||
				variableName.Contains(',') ||
				variableName.Contains('!') ||
				variableName.Contains('@') ||
				variableName.Contains('#') ||
				variableName.Contains('$') ||
				variableName.Contains('%') ||
				variableName.Contains('^') ||
				variableName.Contains('&') ||
				variableName.Contains('*') ||
				variableName.Contains('(') ||
				variableName.Contains(')') ||
				variableName.Contains('-') ||
				variableName.Contains('+') ||
				variableName.Contains('|') ||
				variableName.Contains('[') ||
				variableName.Contains(']') ||
				variableName.Contains('{') ||
				variableName.Contains('}') ||
				variableName.Contains('?') ||
				variableName.Contains('<') ||
				variableName.Contains('>') ||
				variableName.Contains('=') ||
				variableName.Contains('`') ||
				variableName.Contains('~') ||
				variableName.Contains(':') ||
				variableName.Contains(';') ||
				variableName.Contains("'") ||
				variableName.Contains("\"") ||
				variableName.Contains("\\") ||
				variableName.Contains('/')) {
				return false;
			}
			return true;
		}

		public static string AutoCorrectName(string name) {
			if(name == null)
				return "_";
			var strs = name.ToCharArray().ToList();
			for(int i = 0; i < strs.Count; i++) {
				var c = strs[i];
				if(i == 0 && char.IsDigit(c)) {
					strs.Insert(0, '_');
					i--;
				} else if(c == ' ') {
					strs[i] = '_';
				} else if(char.IsSymbol(c) && c != '@') {
					strs.RemoveAt(i);
					i--;
				}
			}
			return string.Join("", strs);
		}

		private static System.Security.Cryptography.MD5 MD5;
		/// <summary>
		/// Get unique identifier based on string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int GetUIDFromString(string str) {
			if(MD5 == null) {
				MD5 = System.Security.Cryptography.MD5.Create();
			}
			var hashed = MD5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
			return BitConverter.ToInt32(hashed, 0);
		}

		/// <summary>
		/// Generate a unique number from a 2 value
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int GenerateUID(int a, int b) {
			return ((a + b) * (a + b + 1) / 2) + b;
		}

		/// <summary>
		/// Generate a unique number from a 2 value
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static long GenerateUID(long a, long b) {
			return ((a + b) * (a + b + 1) / 2) + b;
		}

		/// <summary>
		/// Get the actual persistence object if any
		/// </summary>
		/// <param name="obj"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetActualObject<T>(T obj) where T : UnityEngine.Object {
			return getActualObject(obj) as T;
		}

		/// <summary>
		/// Get object unique identifier.
		/// This will return local file identifier if the obj is Prefab.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static int GetObjectID(UnityEngine.Object obj) {
			if(getObjectID == null) {
#if UNITY_EDITOR
				throw new Exception("uNode is not initialized");
#else
				return obj.GetHashCode();
#endif
			}
			return getObjectID(obj);
		}

		/// <summary>
		/// Remove incorrect name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string RemoveIncorrectName(string name) {
			if(!IsValidVariableName(name)) {
				name = name.Replace(" ", "_");
				name = name.Replace(".", "");
				name = name.Replace(",", "");
				name = name.Replace("!", "");
				name = name.Replace("@", "");
				name = name.Replace("#", "");
				name = name.Replace("$", "");
				name = name.Replace("%", "");
				name = name.Replace("^", "");
				name = name.Replace("&", "");
				name = name.Replace("*", "");
				name = name.Replace("(", "");
				name = name.Replace(")", "");
				name = name.Replace("-", "");
				name = name.Replace("+", "");
				name = name.Replace("|", "");
				name = name.Replace("[", "");
				name = name.Replace("]", "");
				name = name.Replace("{", "");
				name = name.Replace("}", "");
				name = name.Replace("?", "");
				name = name.Replace("<", "");
				name = name.Replace(">", "");
				name = name.Replace("=", "");
				name = name.Replace("`", "");
				name = name.Replace("~", "");
				name = name.Replace(":", "");
				name = name.Replace(";", "");
				name = name.Replace("'", "");
				name = name.Replace("\"", "");
				name = name.Replace("\\", "");
				name = name.Replace("/", "");
			}
			return name;
		}

		public static bool IsTargetingValueNode(MemberData member) {
			return member != null && member.isAssigned && member.targetType == MemberData.TargetType.ValueNode;
		}
		#endregion

		#region Classes
		public class ErrorMessage {
			public string message;
			public Object obj;
			public Action<Vector2> onClicked;
		}
		#endregion

		#region NodeUtility
		public static bool IsInStateGraph(NodeComponent node) {
			if(!node) {
				throw new ArgumentNullException("node");
			}
			if(!node.owner) {
				node.owner = node.GetComponentInParent<uNodeRoot>();
				if(!node.owner) {
					throw new Exception("Null owner of node:" + node.gameObject.name);
				}
			}
			return node.IsInRoot || !FindParentComponent<RootObject>(node.transform, node.owner.transform);
		}

		public static T FindParentComponent<T>(Transform from, Transform end) where T : Component {
			T comp = null;
			Transform tr = from;
			while(tr.parent && tr != end) {
				comp = tr.parent.GetComponent<T>();
				if(comp) {
					break;
				}
				tr = tr.parent;
			}
			return comp;
		}

		public static T FindParentComponent<T>(Transform from) where T : Component {
			return FindParentComponent<T>(from, from.root);
		}
		#endregion

		#region ListUtility
		//public static void AddList(ref IList list, object value) {
		//	if(list is Array) {
		//		Array arr = list as Array;
		//		AddArray(ref arr, value);
		//		list = arr;
		//	} else {
		//		list.Add(value);
		//	}
		//}

		//public static void RemoveListAt(ref IList list, int index) {
		//	if(list is Array) {
		//		Array arr = list as Array;
		//		RemoveArrayAt(ref arr, index);
		//		list = arr;
		//	} else {
		//		list.RemoveAt(index);
		//	}
		//}

		public static void ReorderList(IList list, int oldIndex, int newIndex) {
			if(oldIndex == newIndex)
				return;
			var val = list[oldIndex];
			if(oldIndex < newIndex) {
				for(int i = oldIndex; i < newIndex; i++) {
					list[i] = list[i + 1];
				}
				list[newIndex] = val;
			} else {
				for(int i = oldIndex; i > newIndex; i--) {
					list[i] = list[i - 1];
				}
				list[newIndex] = val;
			}
		}


		public static void InsertList<T>(ref IList<T> list, int index, T value) {
			if(list is Array) {
				var obj = new List<T>(list);
				obj.Insert(index, value);
				list = obj.ToArray();
			} else {
				list.Insert(index, value);
			}
		}

		public static void AddList<T>(ref IList<T> list, T value) {
			if(list is Array) {
				var obj = new List<T>(list);
				obj.Add(value);
				list = obj.ToArray();
			} else {
				list.Add(value);
			}
		}

		public static void RemoveList<T>(ref IList<T> list, T value) {
			if(list is Array) {
				var obj = new List<T>(list);
				obj.Remove(value);
				list = obj.ToArray();
			} else {
				list.Remove(value);
			}
		}

		public static void RemoveListAt<T>(ref IList<T> list, int index) {
			if(list is Array) {
				var obj = new List<T>(list);
				obj.RemoveAt(index);
				list = obj.ToArray();
			} else {
				list.RemoveAt(index);
			}
		}
		#endregion

		#region ArrayUtility
		public static T[] CreateArrayFrom<T>(IList<T> from) {
			if(from == null)
				return default;
			T[] value = new T[from.Count];
			for(int i = 0; i < from.Count; i++) {
				value[i] = from[i];
			}
			return value;
		}

		public static void AddArray(ref Array array, object value) {
			Array array2 = Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
			for(int i = 0; i < array.Length; i++) {
				array2.SetValue(array.GetValue(i), i);
			}
			array2.SetValue(value, array.Length);
			array = array2;
		}

		public static void AddArray<T>(ref T[] array, T value) {
			Array array2 = Array.CreateInstance(typeof(T), array.Length + 1);
			for(int i = 0; i < array.Length; i++) {
				array2.SetValue(array.GetValue(i), i);
			}
			array2.SetValue(value, array.Length);
			array = array2 as T[];
		}

		public static void AddArrayAt<T>(ref T[] array, T value, int index) {
			T[] array2 = new T[array.Length + 1];
			for(int i = 0; i < array2.Length; i++) {
				if(i == index) {
					array2[i] = value;
					continue;
				}
				array2[i] = array[i > index ? i - 1 : i];
			}
			array = array2;
		}

		public static void RemoveArray(ref Array array, object value) {
			for(int i = 0; i < array.Length; i++) {
				if(array.GetValue(i) == value) {
					RemoveArrayAt(ref array, i);
					break;
				}
			}
		}

		public static void RemoveArray<T>(ref T[] array, T value) {
			for(int i = 0; i < array.Length; i++) {
				if(object.Equals(array[i], value)) {
					RemoveArrayAt(ref array, i);
					break;
				}
			}
		}

		public static void RemoveArrayAt(ref Array array, int index) {
			Array array2 = Array.CreateInstance(array.GetType().GetElementType(), array.Length - 1);
			int skipped = 0;
			for(int i = 0; i < array.Length; i++) {
				if(index == i) { skipped++; continue; }
				array2.SetValue(array.GetValue(i), i - skipped);
			}
			array = array2;
		}

		public static void RemoveArrayAt<T>(ref T[] array, int index) {
			Array array2 = Array.CreateInstance(typeof(T), array.Length - 1);
			int skipped = 0;
			for(int i = 0; i < array.Length; i++) {
				if(index == i) { skipped++; continue; }
				array2.SetValue(array.GetValue(i), i - skipped);
			}
			array = array2 as T[];
		}
		#endregion

		#region CheckErrors
		/// <summary>
		/// Check the possibly error for MemberData.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="owner"></param>
		/// <param name="fieldName"></param>
		public static bool CheckError(MemberData member, UnityEngine.Object owner, string fieldName, bool allowNull = true) {
			if(member == null)
				return false;
			if(!member.isAssigned) {
				RegisterEditorError(owner, "Unassigned " + fieldName);
				return true;
			} else {
				if(!member.isStatic && member.targetType != MemberData.TargetType.Null &&
					member.targetType != MemberData.TargetType.Type &&
					member.targetType != MemberData.TargetType.Values && member.instance == null) {
					RegisterEditorError(owner, "Instance of " + fieldName + " is unassigned/null");
					return true;
				} else if(!allowNull && !member.isStatic) {
					if(member.targetType == MemberData.TargetType.Null ||
						member.targetType == MemberData.TargetType.Values && member.Get() == null) {
						RegisterEditorError(owner, fieldName + " cannot be null");
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Check the possibly error for MultipurposeMember.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="owner"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool CheckError(MultipurposeMember member, UnityEngine.Object owner, string fieldName) {
			if(member == null)
				return false;
			if(member.target.isAssigned) {
				return CheckError(member.target, owner, fieldName);
			}
			return false;
		}

		/// <summary>
		/// Check the possibly error for MemberData.
		/// </summary>
		/// <param name="members"></param>
		/// <param name="owner"></param>
		/// <param name="fieldName"></param>
		public static bool CheckError(IList<MemberData> members, UnityEngine.Object owner, string fieldName) {
			if(members == null)
				return false;
			bool flag = false;
			for(int i = 0; i < members.Count; i++) {
				if(CheckError(members[i], owner, fieldName + "[" + i + "]")) {
					flag = true;
				}
			}
			return flag;
		}

		/// <summary>
		/// Check the possibly error for MultipurposeMember.
		/// </summary>
		/// <param name="mMember"></param>
		/// <param name="owner"></param>
		/// <param name="fieldName"></param>
		public static void CheckError(MultipurposeMember mMember, NodeComponent owner, string fieldName) {
			if(owner == null)
				return;
			if(mMember.target.targetType == MemberData.TargetType.Null) {
				return;
			}
			if(!mMember.target.isAssigned) {
				RegisterEditorError(owner, "Unassigned " + fieldName);
			} else {
				if(mMember.target.instance is MemberData) {
					CheckError(mMember.target.instance as MemberData, owner, "Instance", false);
				}
				if(mMember.target.targetType != MemberData.TargetType.Values) {
					if(mMember.target.startType == null) {
						RegisterEditorError(owner, "Missing type: " + mMember.target.StartSerializedType.typeName);
					}
				} else {
					if(mMember.target.type == null) {
						RegisterEditorError(owner, "Missing type: " + mMember.target.TargetSerializedType.typeName);
					}
				}
			}
			try {
				if(mMember.target.isAssigned && mMember.target.SerializedItems?.Length > 0) {
					MemberInfo[] members = mMember.target.GetMembers();
					if(members != null && members.Length > 0 && members.Length + 1 != mMember.target.SerializedItems.Length) {
						members = null;
					}
					int totalParam = 0;
					for(int z = 0; z < mMember.target.SerializedItems.Length; z++) {
						if(z != 0) {
							if(members != null && (mMember.target.isDeepTarget || !mMember.target.IsTargetingUNode)) {
								MemberInfo member = members[z - 1];
								if(member is MethodInfo || member is ConstructorInfo) {
									var method = member as MethodInfo;
									var parameters = method != null ? method.GetParameters() : (member as ConstructorInfo).GetParameters();
									if(parameters.Length > 0) {
										while(parameters.Length + totalParam > mMember.parameters.Length) {
											AddArray(ref mMember.parameters, MemberData.none);
										}
										for(int x = 0; x < parameters.Length; x++) {
											Type PType = parameters[x].ParameterType;
											if(PType != null) {
												var param = mMember.parameters[totalParam];
												CheckError(param, owner, "parameter : " + parameters[x].Name);
												if(PType.IsByRef && param.isTargeted) {
													if(param.targetType.IsTargetingValue()) {
														if(param.targetType != MemberData.TargetType.Field && !param.IsTargetingVariable) {
															var tNode = param.GetTargetNode() as Node;
															if(tNode != null && !tNode.CanSetValue()) {
																RegisterEditorError(
																	owner,
																	$"parameter : {parameters[x].Name} must be targeting assignable variable");
															}
														} else {
															RegisterEditorError(
																owner,
																$"parameter : {parameters[x].Name} must be targeting assignable variable");
														}
													}
												}
											}
											totalParam++;
										}
										continue;
									}
								}
							}
						}
						Type[] paramsType = MemberData.Utilities.SafeGetParameterTypes(mMember.target)[z];
						if(paramsType != null && paramsType.Length > 0) {
							while(paramsType.Length + totalParam > mMember.parameters.Length) {
								AddArray(ref mMember.parameters, MemberData.none);
							}
							for(int x = 0; x < paramsType.Length; x++) {
								Type PType = paramsType[x];
								if(PType != null) {
									var param = mMember.parameters[totalParam];
									var paramName = "parameter :  P" + (x + 1);
									CheckError(param, owner, paramName);
									if(PType.IsByRef && param.isTargeted) {
										if(PType is MissingType) {
											RegisterEditorError(
												owner,
												$"parameter : {paramName} has a missing type: {PType.FullName}");
										}
										if(param.targetType != MemberData.TargetType.Field && !param.IsTargetingVariable) {
											if(param.targetType == MemberData.TargetType.ValueNode) {
												var tNode = param.GetTargetNode() as Node;
												if(tNode != null && !tNode.CanSetValue()) {
													RegisterEditorError(
														owner,
														$"parameter : {paramName} must be targeting assignable variable");
												}
											} else {
												RegisterEditorError(
													owner,
													$"parameter : {paramName} must be targeting assignable variable");
											}
										}
									}
								}
								totalParam++;
							}
						}
					}
				}
			}
			catch(System.Exception ex) {
				RegisterEditorError(owner, ex.Message);
			}
		}
		#endregion

		/// <summary>
		/// Are this uNode have variable named name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool HasVariable(string name, IList<VariableData> variables) {
			if(variables != null) {
				for(int i = 0; i < variables.Count; i++) {
					if(variables[i].Name.Equals(name)) {
						return true;
					}
				}
			}
			return false;
		}

		public static VariableData GetVariableData(string name, IList<VariableData> variables) {
			if(variables != null) {
				for(int i = 0; i < variables.Count; i++) {
					if(variables[i].Name.Equals(name)) {
						return variables[i];
					}
				}
			}
			return null;
		}

		public static T CloneObject<T>(T value) {
			return SerializerUtility.Duplicate(value);
		}
	}

	[Serializable]
	public class EditorTextSetting {
		public bool useRichText = true;
		public Color typeColor = new Color(0, 0.8f, 0.37f);
		public Color keywordColor = new Color(0, 0.46f, 0.83f);
		public Color interfaceColor = new Color(0.8f, 0.77f, 0);
		public Color enumColor = new Color(0.8f, 0.8f, 0);
		public Color otherColor = new Color(0.95f, 0.33f, 0.32f);
		public Color misleadingColor = new Color(0.85f, 0.85f, 0.85f);
		public Color summaryColor = new Color(0, 0.7f, 0);
	}

	/// <summary>
	/// Provides useful function for string manipulation.
	/// </summary>
	public static class StringHelper {
		static readonly IDictionary<string, string> m_replaceDict = new Dictionary<string, string>();

		const string ms_regexEscapes = @"[\a\b\f\n\r\t\v\\""]";

		public static string StringLiteral(string input) {
			return Regex.Replace(input, ms_regexEscapes, match);
		}

		public static string CharLiteral(char c) {
			return c == '\'' ? @"'\''" : string.Format("'{0}'", c);
		}

		private static string match(Match m) {
			string match = m.ToString();
			if(m_replaceDict.ContainsKey(match)) {
				return m_replaceDict[match];
			}

			throw new NotSupportedException();
		}

		static StringHelper() {
			m_replaceDict.Add("\a", @"\a");
			m_replaceDict.Add("\b", @"\b");
			m_replaceDict.Add("\f", @"\f");
			m_replaceDict.Add("\n", @"\n");
			m_replaceDict.Add("\r", @"\r");
			m_replaceDict.Add("\t", @"\t");
			m_replaceDict.Add("\v", @"\v");

			m_replaceDict.Add("\\", @"\\");
			m_replaceDict.Add("\0", @"\0");

			m_replaceDict.Add("\"", "\\\"");
		}
	}
}