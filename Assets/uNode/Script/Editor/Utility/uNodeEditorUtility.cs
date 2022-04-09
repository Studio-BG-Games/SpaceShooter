#pragma warning disable
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// Provides useful Utility for Editor
	/// </summary>
	public static class uNodeEditorUtility {
		#region Properties
		private static MonoScript[] _monoScripts;
		/// <summary>
		/// Find all MonoScript in the project
		/// </summary>
		public static MonoScript[] MonoScripts {
			get {
				if(_monoScripts == null) {
					_monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
				}
				return _monoScripts;
			}
		}
		#endregion

		#region Icons
		public static class Icons {
			internal static readonly MethodInfo EditorGUIUtility_GetScriptObjectFromClass;
			internal static readonly MethodInfo EditorGUIUtility_GetIconForObject;

			static Icons() {
				EditorGUIUtility_GetScriptObjectFromClass = typeof(EditorGUIUtility).GetMethod("GetScript", BindingFlags.Static | BindingFlags.NonPublic);
				EditorGUIUtility_GetIconForObject = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			}


			private static Texture2D _flowIcon;
			public static Texture2D flowIcon {
				get {
					if(_flowIcon == null) {
						_flowIcon = Resources.Load<Texture2D>("Icons/IconFlow");
					}
					return _flowIcon;
				}
			}

			private static Texture2D _valueIcon;
			public static Texture2D valueIcon {
				get {
					if(_valueIcon == null) {
						_valueIcon = Resources.Load<Texture2D>("Icons/IconValueWhite");
					}
					return _valueIcon;
				}
			}

			private static Texture2D _valueBlueIcon;
			public static Texture2D valueBlueIcon {
				get {
					if(_valueBlueIcon == null) {
						_valueBlueIcon = Resources.Load<Texture2D>("Icons/IconValueBlue");
					}
					return _valueBlueIcon;
				}
			}

			private static Texture2D _valueYellowIcon;
			public static Texture2D valueYellowIcon {
				get {
					if(_valueYellowIcon == null) {
						_valueYellowIcon = Resources.Load<Texture2D>("Icons/IconValueYellow");
					}
					return _valueYellowIcon;
				}
			}

			private static Texture2D _valueYellowRed;
			public static Texture2D valueYellowRed {
				get {
					if(_valueYellowRed == null) {
						_valueYellowRed = Resources.Load<Texture2D>("Icons/IconValueRed");
					}
					return _valueYellowRed;
				}
			}

			private static Texture2D _valueGreenIcon;
			public static Texture2D valueGreenIcon {
				get {
					if(_valueGreenIcon == null) {
						_valueGreenIcon = Resources.Load<Texture2D>("Icons/IconValueGreen");
					}
					return _valueGreenIcon;
				}
			}

			private static Texture2D _divideIcon;
			public static Texture2D divideIcon {
				get {
					if(_divideIcon == null) {
						_divideIcon = Resources.Load<Texture2D>("Icons/IconFlowDivide");
					}
					return _divideIcon;
				}
			}

			private static Texture2D _clockIcon;
			public static Texture2D clockIcon {
				get {
					if(_clockIcon == null) {
						_clockIcon = Resources.Load<Texture2D>("Icons/IconTime");
					}
					return _clockIcon;
				}
			}

			private static Texture2D _repeatIcon;
			public static Texture2D repeatIcon {
				get {
					if(_repeatIcon == null) {
						_repeatIcon = Resources.Load<Texture2D>("Icons/IconRepeat");
					}
					return _repeatIcon;
				}
			}

			private static Texture2D _repeatOnceIcon;
			public static Texture2D repeatOnceIcon {
				get {
					if(_repeatOnceIcon == null) {
						_repeatOnceIcon = Resources.Load<Texture2D>("Icons/IconRepeatOnce");
					}
					return _repeatOnceIcon;
				}
			}

			private static Texture2D _switchIcon;
			public static Texture2D switchIcon {
				get {
					if(_switchIcon == null) {
						_switchIcon = Resources.Load<Texture2D>("Icons/IconSwitch");
					}
					return _switchIcon;
				}
			}

			private static Texture2D _colorIcon;
			public static Texture2D colorIcon {
				get {
					if(_colorIcon == null) {
						_colorIcon = Resources.Load<Texture2D>("Icons/IconColor");
					}
					return _colorIcon;
				}
			}

			private static Texture2D _rotateIcon;
			public static Texture2D rotateIcon {
				get {
					if(_rotateIcon == null) {
						_rotateIcon = Resources.Load<Texture2D>("Icons/IconRotate");
					}
					return _rotateIcon;
				}
			}

			private static Texture2D _objectIcon;
			public static Texture2D objectIcon {
				get {
					if(_objectIcon == null) {
						_objectIcon = Resources.Load<Texture2D>("Icons/IconObject");
					}
					return _objectIcon;
				}
			}

			private static Texture2D _mouseIcon;
			public static Texture2D mouseIcon {
				get {
					if(_mouseIcon == null) {
						_mouseIcon = Resources.Load<Texture2D>("Icons/mouse_pc");
					}
					return _mouseIcon;
				}
			}

			private static Texture2D _keyIcon;
			public static Texture2D keyIcon {
				get {
					if(_keyIcon == null) {
						_keyIcon = Resources.Load<Texture2D>("Icons/key");
					}
					return _keyIcon;
				}
			}

			private static Texture2D _dateIcon;
			public static Texture2D dateIcon {
				get {
					if(_dateIcon == null) {
						_dateIcon = Resources.Load<Texture2D>("Icons/date");
					}
					return _dateIcon;
				}
			}

			private static Texture2D _listIcon;
			public static Texture2D listIcon {
				get {
					if(_listIcon == null) {
						_listIcon = Resources.Load<Texture2D>("Icons/IconList");
					}
					return _listIcon;
				}
			}

			private static Texture2D _eventIcon;
			public static Texture2D eventIcon {
				get {
					if(_eventIcon == null) {
						_eventIcon = Resources.Load<Texture2D>("Icons/IconEvent");
					}
					return _eventIcon;
				}
			}

			private static Texture2D _bookIcon;
			public static Texture2D bookIcon {
				get {
					if(_bookIcon == null) {
						_bookIcon = Resources.Load<Texture2D>("Icons/book_key");
					}
					return _bookIcon;
				}
			}

			static Dictionary<string, Texture2D> iconMap;
			public static Texture2D GetIcon(string path) {
				if(iconMap == null) {
					iconMap = new Dictionary<string, Texture2D>();
				}
				Texture2D tex;
				if(iconMap.TryGetValue(path, out tex)) {
					return tex;
				}
				tex = Resources.Load<Texture2D>(path);
				if(tex != null) {
					iconMap[path] = tex;
				}
				return tex;
			}
		}

		private static Dictionary<Type, Texture> _iconsMap = new Dictionary<Type, Texture>();

		public static Texture GetIcon(MemberInfo member) {
			switch(member.MemberType) {
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					return GetTypeIcon(typeof(TypeIcons.MethodIcon));
				case MemberTypes.Field:
				case MemberTypes.Event:
					return GetTypeIcon(typeof(TypeIcons.FieldIcon));
				case MemberTypes.NestedType:
				case MemberTypes.TypeInfo:
					Type type = member as Type;
					if(type is ICustomIcon) {
						return GetTypeIcon(type);
					}
					if(type.IsClass) {
						if(type.IsCastableTo(typeof(Delegate))) {
							return GetTypeIcon(typeof(TypeIcons.DelegateIcon));
						}
						return GetTypeIcon(typeof(TypeIcons.ClassIcon));
					} else if(type.IsInterface) {
						return GetTypeIcon(typeof(TypeIcons.InterfaceIcon));
					} else if(type.IsEnum) {
						return GetTypeIcon(typeof(TypeIcons.EnumIcon));
					} else {
						return GetTypeIcon(typeof(TypeIcons.StructureIcon));
					}
				case MemberTypes.Property:
					return GetTypeIcon(typeof(TypeIcons.PropertyIcon));
				default:
					return GetTypeIcon(typeof(TypeIcons.KeywordIcon));
			}
		}

		public static Texture GetIcon(MemberData member) {
			switch(member.targetType) {
				case MemberData.TargetType.SelfTarget:
				case MemberData.TargetType.Values:
					return GetTypeIcon(typeof(TypeIcons.KeywordIcon));
				case MemberData.TargetType.uNodeVariable:
				case MemberData.TargetType.Field:
				case MemberData.TargetType.Event:
					return GetTypeIcon(typeof(TypeIcons.FieldIcon));
				case MemberData.TargetType.uNodeLocalVariable:
				case MemberData.TargetType.uNodeGroupVariable:
					return GetTypeIcon(typeof(TypeIcons.LocalVariableIcon));
				case MemberData.TargetType.uNodeProperty:
				case MemberData.TargetType.Property:
					return GetTypeIcon(typeof(TypeIcons.PropertyIcon));
				case MemberData.TargetType.uNodeConstructor:
				case MemberData.TargetType.uNodeFunction:
				case MemberData.TargetType.Method:
				case MemberData.TargetType.Constructor:
					return GetTypeIcon(typeof(TypeIcons.MethodIcon));
				case MemberData.TargetType.uNodeParameter:
				case MemberData.TargetType.uNodeGenericParameter:
					return GetTypeIcon(typeof(TypeIcons.LocalVariableIcon));
				case MemberData.TargetType.Type:
					Type type = member.startType;
					if(type == null) {
						return GetTypeIcon(typeof(TypeIcons.ClassIcon));
					} else if(type.IsClass) {
						if(type is ICustomIcon) {
							return GetTypeIcon(type);
						}
						if(type.IsSubclassOf(typeof(Delegate))) {
							return GetTypeIcon(typeof(TypeIcons.DelegateIcon));
						}
						return GetTypeIcon(typeof(TypeIcons.ClassIcon));
					} else if(type.IsInterface) {
						return GetTypeIcon(typeof(TypeIcons.InterfaceIcon));
					} else if(type.IsEnum) {
						return GetTypeIcon(typeof(TypeIcons.EnumIcon));
					} else if(type == typeof(void)) {
						return GetTypeIcon(typeof(TypeIcons.VoidIcon));
					} else {
						return GetTypeIcon(typeof(TypeIcons.StructureIcon));
					}
				default:
					return GetTypeIcon(typeof(TypeIcons.KeywordIcon));
			}
		}

		/// <summary>
		/// Return a icon for the type of a MemberData.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static Texture GetTypeIcon(MemberData member) {
			switch(member.targetType) {
				case MemberData.TargetType.Type:
				case MemberData.TargetType.uNodeType:
					var type = member.startType;
					return GetTypeIcon(type);
				default:
					return GetIcon(member);
			}
		}

		/// <summary>
		/// Return a icon for the object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static Texture GetTypeIcon(object obj) {
			if(obj is ICustomIcon) {
				var icon = (obj as ICustomIcon).GetIcon();
				if(icon != null) {
					return icon;
				}
				return GetTypeIcon(typeof(TypeIcons.RuntimeTypeIcon));
			} else if(obj is MemberData) {
				return GetTypeIcon(obj as MemberData);
			} else if(obj is uNodeClass) {
				return GetTypeIcon(typeof(TypeIcons.ClassIcon));
			} else if(obj is uNodeStruct) {
				return GetTypeIcon(typeof(TypeIcons.StructureIcon));
			} else if(obj is uNodeMacro) {
				return GetTypeIcon(typeof(TypeIcons.UNodeIcon));
			} else if(obj is NodeComponent) {
				return GetTypeIcon((obj as NodeComponent).GetNodeIcon());
			} else if(obj is RootObject) {
				return GetTypeIcon((obj as RootObject).ReturnType());
			}
			if(obj == null) {
				return GetTypeIcon(typeof(object));
			}
			return GetTypeIcon(obj.GetType());
		}

		private static Texture GetDefaultIcon(Type type) {
			if(typeof(MonoBehaviour).IsAssignableFrom(type)) {
				var icon = EditorGUIUtility.ObjectContent(null, type).image;
				if(icon == EditorGUIUtility.FindTexture("DefaultAsset Icon")) {
					icon = null;
				}
				if(icon != null) {
					return icon;
				} else {
					icon = GetScriptTypeIcon(type.Name);
					if(icon != null) {
						return icon;
					}
				}
			}
			if(typeof(UnityEngine.Object).IsAssignableFrom(type)) {
				Texture icon = EditorGUIUtility.ObjectContent(null, type).image;
				if(icon == EditorGUIUtility.FindTexture("DefaultAsset Icon")) {
					icon = EditorGUIUtility.FindTexture("ScriptableObject Icon");
				}
				if(icon != null) {
					return icon;
				}
			}
			return null;
		}

		private static Texture GetScriptTypeIcon(string scriptName) {
			var scriptObject = (UnityEngine.Object)Icons.EditorGUIUtility_GetScriptObjectFromClass.Invoke(null, new object[] { scriptName });
			if(scriptObject != null) {
				var scriptIcon = Icons.EditorGUIUtility_GetIconForObject.Invoke(null, new object[] { scriptObject }) as Texture;

				if(scriptIcon != null) {
					return scriptIcon;
				}
			}
			var scriptPath = AssetDatabase.GetAssetPath(scriptObject);
			if(scriptPath != null) {
				switch(Path.GetExtension(scriptPath)) {
					case ".js":
						return EditorGUIUtility.IconContent("js Script Icon").image;
					case ".cs":
						return EditorGUIUtility.IconContent("cs Script Icon").image;
					case ".boo":
						return EditorGUIUtility.IconContent("boo Script Icon").image;
				}
			}
			return null;
		}

		/// <summary>
		/// Return a icon for the type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Texture GetTypeIcon(Type type) {
			if(type == null)
				return null;
			if(type.IsByRef) {
				return GetTypeIcon(type.GetElementType());
			}
			Texture result = null;
			if(_iconsMap.TryGetValue(type, out result)) {
				return result;
			}
			if(type == null) {
				_iconsMap[type] = Icons.valueIcon;
				return Icons.valueIcon;
			}
			if(type is ICustomIcon) {
				var icon = (type as ICustomIcon).GetIcon();
				if(icon != null) {
					return icon;
				}
			} 
			if(type is RuntimeType) {
				if(type.IsInterface) {
					return GetTypeIcon(typeof(TypeIcons.InterfaceIcon));
				}
				var rType = type as RuntimeType;
				return GetTypeIcon(typeof(TypeIcons.RuntimeTypeIcon));
			}
			Texture texture = GetDefaultIcon(type);
			if(texture != null) {
				_iconsMap[type] = texture;
				return texture;
			}
			TypeIcons.IconPathAttribute att = null;
			if(type.IsDefinedAttribute(typeof(TypeIcons.IconPathAttribute))) {
				att = type.GetCustomAttributes(typeof(TypeIcons.IconPathAttribute), true)[0] as TypeIcons.IconPathAttribute;
			}
			if(att != null) {
				result = Icons.GetIcon(att.path);
			} else if(type == typeof(TypeIcons.FlowIcon)) {
				result = Icons.flowIcon;
			} else if(type == typeof(TypeIcons.ValueIcon)) {
				result = Icons.valueIcon;
			} else if(type == typeof(TypeIcons.BranchIcon)) {
				result = Icons.divideIcon;
			} else if(type == typeof(TypeIcons.ClockIcon)) {
				result = Icons.clockIcon;
			} else if(type == typeof(TypeIcons.RepeatIcon)) {
				result = Icons.repeatIcon;
			} else if(type == typeof(TypeIcons.RepeatOnceIcon)) {
				result = Icons.repeatOnceIcon;
			} else if(type == typeof(TypeIcons.SwitchIcon)) {
				result = Icons.switchIcon;
			} else if(type == typeof(TypeIcons.MouseIcon)) {
				result = Icons.mouseIcon;
			} else if(type == typeof(TypeIcons.EventIcon)) {
				result = Icons.eventIcon;
			} else if(type == typeof(TypeIcons.RotationIcon) || type == typeof(Quaternion)) {
				result = Icons.rotateIcon;
			} else if(type == typeof(Color) || type == typeof(Color32)) {
				result = Icons.colorIcon;
			} else if(type == typeof(int)) {
				result = GetTypeIcon(typeof(TypeIcons.IntegerIcon));
			} else if(type == typeof(float)) {
				result = GetTypeIcon(typeof(TypeIcons.FloatIcon));
			}else if(type == typeof(Vector3)) {
				result = GetTypeIcon(typeof(TypeIcons.Vector3Icon));
			} else if(type == typeof(Vector2)) {
				result = GetTypeIcon(typeof(TypeIcons.Vector2Icon));
			} else if(type == typeof(Vector4)) {
				result = GetTypeIcon(typeof(TypeIcons.Vector4Icon));
			} else if(type.IsCastableTo(typeof(UnityEngine.Object))) {
				result = Icons.objectIcon;
			} else if(type.IsCastableTo(typeof(IList))) {
				result = Icons.listIcon;
			} else if(type.IsCastableTo(typeof(IDictionary))) {
				result = Icons.bookIcon;
			} else if(type == typeof(void)) {
				result = GetTypeIcon(typeof(TypeIcons.VoidIcon));
			} else if(type.IsCastableTo(typeof(KeyValuePair<,>))) {
				result = Icons.keyIcon;
			} else if(type == typeof(DateTime) || type == typeof(Time)) {
				result = Icons.dateIcon;
			} else if(type.IsInterface) {
				result = GetTypeIcon(typeof(TypeIcons.InterfaceIcon));
			} else if(type.IsEnum) {
				result = GetTypeIcon(typeof(TypeIcons.EnumIcon));
			} else if(type == typeof(object)) {
				result = Icons.valueBlueIcon;
			} else if(type == typeof(bool)) {
				result = Icons.valueYellowRed;
			} else if(type == typeof(string)) {
				result = GetTypeIcon(typeof(TypeIcons.StringIcon));
			} else if(type == typeof(Type)) {
				result = Icons.valueGreenIcon;
			} else if(type == typeof(UnityEngine.Random) || type == typeof(System.Random)) {
				result = GetTypeIcon(typeof(TypeIcons.RandomIcon));
			} 
			// else if(type == typeof(UnityEngine.Debug)) {
			// 	result = GetTypeIcon(typeof(TypeIcons.BugIcon));
			// } 
			else {
				result = GetIcon(type);
			}
			if(result != null) {
				_iconsMap[type] = result;
			}
			return result;
		}
		#endregion

		#region Styles
		static Dictionary<int, GUIStyle> sizeStyle = new Dictionary<int, GUIStyle>();
		public static GUIStyle GetStyle(int fontSize = 15, int top = 1) {
			if(sizeStyle.ContainsKey((fontSize * 100) + top)) {
				return sizeStyle[(fontSize * 100) + top];
			} else {
				GUIStyle style = null;
				style = new GUIStyle(EditorStyles.label);
				style.padding.top = top;
				style.margin.top -= top;
				style.fontSize = fontSize;
				style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.grey;
				sizeStyle.Add((fontSize * 100) + top, style);
				return style;
			}
		}

		static Dictionary<int, GUIStyle> sizeWhiteStyle = new Dictionary<int, GUIStyle>();
		public static GUIStyle GetWhiteStyle(int fontSize = 15, int top = 1) {
			if(sizeWhiteStyle.ContainsKey((fontSize * 100) + top)) {
				return sizeWhiteStyle[(fontSize * 100) + top];
			} else {
				GUIStyle style = null;
				style = new GUIStyle(EditorStyles.label);
				style.padding.top = top;
				style.margin.top -= top;
				style.fontSize = fontSize;
				style.normal.textColor = Color.white;
				sizeWhiteStyle.Add((fontSize * 100) + top, style);
				return style;
			}
		}

		public static Texture2D MakeTexture(int width, int height, Color color) {
			Color[] pix = new Color[width * height];

			for(int i = 0; i < pix.Length; i++)
				pix[i] = color;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}
		#endregion

		#region Drag & Drop
		public static void GUIDropArea(Rect rect, Action onDragPerform, Action repaintAction = null) {
			var currentEvent = Event.current;
			if(rect.Contains(currentEvent.mousePosition)) {
				if(currentEvent.type == EventType.DragUpdated) {
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}
				if(DragAndDrop.visualMode == DragAndDropVisualMode.Copy && (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.Layout)) {
					repaintAction?.Invoke();
				}
				if(currentEvent.type == EventType.DragPerform) {
					DragAndDrop.AcceptDrag();
					onDragPerform();
					Event.current.Use();
				}
			}
		}
		
		public static void AcceptDrag(
			Rect position,
			string genericName,
			Action<object> onDrop,
			Action<object> onHover = null,
			Func<object, bool> validation = null) {

			if(position.Contains(Event.current.mousePosition)) {
				if(IsDragging(position, genericName, validation)) {
					if(onHover != null && Event.current.type == EventType.DragUpdated) {
						onHover(GetDraggedObject(genericName));
					}
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}
				if(Event.current.type == EventType.DragPerform && (validation == null || validation(GetDraggedObject(genericName)))) {
					DragAndDrop.AcceptDrag();
					onDrop(GetDraggedObject(genericName));
				}
			}
		}

		public static void AcceptDrag(
			Rect position,
			Action<UnityEngine.Object[]> onDrop,
			Action<object> onHover = null,
			Func<UnityEngine.Object[], bool> validation = null) {

			if(position.Contains(Event.current.mousePosition)) {
				if(IsDragging(position, validation)) {
					if(onHover != null && Event.current.type == EventType.DragUpdated) {
						onHover(GetDraggedObject());
					}
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}
				if(Event.current.type == EventType.DragPerform && (validation == null || validation(GetDraggedObject()))) {
					DragAndDrop.AcceptDrag();
					onDrop(GetDraggedObject());
				}
			}
		}

		public static bool IsDragging(Rect position, string genericName, Func<object, bool> validation = null) {
			if(position.Contains(Event.current.mousePosition) && Event.current.button == 0) {
				var generic = DragAndDrop.GetGenericData(genericName);
				if(generic != null) {
					if(validation == null || validation(generic)) {
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsDragging(Rect position, Func<UnityEngine.Object[], bool> validation = null) {
			if(position.Contains(Event.current.mousePosition) && Event.current.button == 0) {
				if(DragAndDrop.visualMode == DragAndDropVisualMode.None && DragAndDrop.objectReferences.Length > 0) {
					if(validation == null || validation(DragAndDrop.objectReferences)) {
						return true;
					}
				}
			}
			return false;
		}

		public static UnityEngine.Object[] GetDraggedObject() {
			return DragAndDrop.objectReferences;
		}

		public static object GetDraggedObject(string name) {
			return DragAndDrop.GetGenericData(name);
		}

		public static void ClearDragAndDrop() {

		}
		#endregion

		#region MapUtils
		public static List<object> GetKeys(IDictionary map) {
			List<object> keys = new List<object>();
			foreach(var k in map.Keys) {
				keys.Add(k);
			}
			return keys;
		}

		public static List<object> GetValues(IDictionary map) {
			List<object> values = new List<object>();
			foreach(var k in map.Values) {
				values.Add(k);
			}
			return values;
		}

		public static object GetKeyMap(IDictionary map, int index) {
			int i = 0;
			foreach(var k in map.Keys) {
				if(i == index) {
					return k;
				}
				i++;
			}
			return null;
		}

		public static object GetValueMap(IDictionary map, int index) {
			int i = 0;
			foreach(var k in map.Values) {
				if(i == index) {
					return k;
				}
				i++;
			}
			return null;
		}
		#endregion

		#region Transfroms
		/// <summary>
		/// Get the prefab transfrom.
		/// </summary>
		/// <param name="from">The transform to find</param>
		/// <param name="rootFrom">The root transfrom from</param>
		/// <param name="rootPrefab">The root transfrom for find it's child</param>
		/// <returns></returns>
		public static Transform GetPrefabTransform(Transform from, Transform rootFrom, Transform rootPrefab) {
			if(from == rootFrom) {
				return rootPrefab;
			}
			List<int> indexChild = new List<int>();
			Transform t = from;
			while(t.parent != null) {
				indexChild.Insert(0, t.GetSiblingIndex());
				if(t.parent == rootFrom) {
					break;
				}
				t = t.parent;
			}
			t = rootPrefab;
			foreach(var index in indexChild) {
				if(t.childCount <= index)
					return null;
				t = t.GetChild(index);
			}
			return t;
		}
		#endregion

		#region Others
		public static void DisplayErrorMessage(string message = "Something went wrong.") {
			EditorUtility.DisplayDialog("Error", message, "OK");
		}

		public static void DisplayMessage(string title, string message) {
			EditorUtility.DisplayDialog(title, message, "OK");
		}

		static string _uNodePath;
		/// <summary>
		/// Get uNode Plugin Path
		/// </summary>
		/// <returns></returns>
		public static string GetUNodePath() {
			if(string.IsNullOrEmpty(_uNodePath)) {
				var path = AssetDatabase.GetAssetPath(uNodeEditorUtility.GetMonoScript(typeof(uNodeRoot)));
				_uNodePath = path.Remove(path.IndexOf("uNode") + 5);
			}
			return _uNodePath;
		}

		public static string GetRelativePath(string absolutePath) {
			if(absolutePath.Replace('\\', '/').StartsWith(Application.dataPath)) {
				return "Assets" + absolutePath.Substring(Application.dataPath.Length);
			}
			return string.Empty;
		}

		public static bool IsSceneObject(UnityEngine.Object target) {
			if(target == null)
				return false;
			if(!EditorUtility.IsPersistent(target)) {
				Transform root = null;
				if(target is Component) {
					root = (target as Component).transform.root;
				} else if(target is GameObject) {
					root = (target as GameObject).transform.root;
				}
				if(root != null && root.gameObject.name.StartsWith(GraphUtility.KEY_TEMP_OBJECT)) {
					//Ensure to return false when the target is a temporary object
					return false;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Get the source of the 'obj' from the original prefab that's instantiate it.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="prefab"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetComponentSource<T>(T obj, GameObject prefab) where T : Component {
			if(obj == null) {
				return null;
			} else if(IsPrefab(obj)) {
				return obj;
			} else if(IsPrefabInstance(obj)) {
				return PrefabUtility.GetCorrespondingObjectFromSource(obj);
			} 
			Transform tr = obj.transform;
			Transform root = tr.root;
			if(prefab == null) {
				prefab = GraphUtility.GetOriginalObject(obj.gameObject, out root);
				if(prefab == null) return null;
			}
			var ptr = uNodeEditorUtility.GetPrefabTransform(tr, root, prefab.transform);
			if(ptr != null) {
				if(ptr is T) {
					return ptr as T;
				} else {
					return ptr.GetComponent<T>();
				}
			}
			return null;
		}

		/// <summary>
		/// Get the source of the 'obj' from the original prefab that's instantiate it.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="prefab"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static GameObject GetGameObjectSource<T>(T obj, GameObject prefab) where T : UnityEngine.Object {
			if(obj == null) {
				return null;
			} else if(IsPrefab(obj)) {
				if(obj is Component) {
					return (obj as Component).gameObject;
				}
				return obj as GameObject;
			} else if(IsPrefabInstance(obj)) {
				var result = PrefabUtility.GetCorrespondingObjectFromSource(obj);
				if(result is Component) {
					return (result as Component).gameObject;
				}
				return result as GameObject;
			}
			Transform tr = null;
			if(obj is Component) {
				tr = (obj as Component).transform;
			} else if(obj is GameObject) {
				tr = (obj as GameObject).transform;
			}
			Transform root = tr.root;
			if(prefab == null) {
				if(obj is Component) {
					prefab = GraphUtility.GetOriginalObject((obj as Component).gameObject, out root);
				} else {
					prefab = GraphUtility.GetOriginalObject((obj as GameObject).gameObject, out root);
				}
				if(prefab == null) return null;
			}
			var ptr = uNodeEditorUtility.GetPrefabTransform(tr, root, prefab.transform);
			if(ptr != null) {
				return ptr.gameObject;
			}
			return null;
		}

		public static UnityEngine.Object GetObjectSource(UnityEngine.Object obj, GameObject prefab, Type type) {
			if(obj == null) {
				return null;
			} else if(IsPrefab(obj) || EditorUtility.IsPersistent(obj)) {
				return obj;
			} else if(IsPrefabInstance(obj)) {
				return PrefabUtility.GetCorrespondingObjectFromSource(obj);
			}
			Transform tr = null;
			if(obj is Component) {
				tr = (obj as Component).transform;
			} else if(obj is GameObject) {
				tr = (obj as GameObject).transform;
			}
			Transform root = tr.root;
			if(prefab == null) {
				if(obj is Component) {
					prefab = GraphUtility.GetOriginalObject((obj as Component).gameObject, out root);
				} else if(obj is GameObject) {
					prefab = GraphUtility.GetOriginalObject((obj as GameObject).gameObject, out root);
				}
				if(prefab == null) return null;
			}
			var ptr = uNodeEditorUtility.GetPrefabTransform(tr, root, prefab.transform);
			if(ptr != null) {
				if(type == typeof(GameObject)) {
					return ptr.gameObject;
				} else if(obj is uNodeRoot sourceRoot) {
					var graphs = ptr.GetComponents<uNodeRoot>();
					foreach(var g in graphs) {
						if(g.GraphName == sourceRoot.GraphName) {
							return g;
						}
					}
				}
				return ptr.GetComponent(type);
			}
			return null;
		}
		
		/// <summary>
		/// Get the source of the 'obj' from the original prefab that's instantiate it.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="prefab"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetObjectSource<T>(UnityEngine.Object obj, GameObject prefab) where T : UnityEngine.Object {
			return GetObjectSource(obj, prefab, typeof(T)) as T;
		}

		/// <summary>
		/// Copy string value to clipboard.
		/// </summary>
		/// <param name="value"></param>
		public static void CopyToClipboard(string value) {
			TextEditor te = new TextEditor();
			te.text = value;
			te.SelectAll();
			te.Copy();
		}

		/// <summary>
		/// Get unique identifier based on string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int GetUIDFromString(string str) {
			return uNodeUtility.GetUIDFromString(str);
		}

		/// <summary>
		/// Get unique name for graph variable, property, and function
		/// </summary>
		/// <param name="name"></param>
		/// <param name="graph"></param>
		/// <returns></returns>
		public static string GetUniqueNameForGraph(string name, uNodeRoot graph) {
			string result = name;
			int index = 1;
			bool hasSameName = false;
			do {
				if(hasSameName) {
					result = name + index;
					index++;
				}
				hasSameName = false;
				foreach(var var in graph.Variables) {
					if(var.Name.Equals(result)) {
						hasSameName = true;
						break;
					}
				}
				foreach(var var in graph.Properties) {
					if(var.Name.Equals(result)) {
						hasSameName = true;
						break;
					}
				}
				foreach(var var in graph.Functions) {
					if(var.Name.Equals(result)) {
						hasSameName = true;
						break;
					}
				}
			} while(hasSameName);
			return result;
		}

		/// <summary>
		/// Get full script name including namespace from the graph if exist.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static string GetFullScriptName(uNodeRoot root) {
			if(root == null)
				return "";
			return root.GeneratedTypeName;
		}

		/// <summary>
		/// Get generated script from class system
		/// </summary>
		/// <param name="classSystem"></param>
		/// <returns></returns>
		public static Type GetGeneratedScript(IClassSystem classSystem) {
			if(classSystem == null)
				return null;
			return GetFullScriptName(classSystem as uNodeRoot).ToType(false);
		}

		/// <summary>
		/// Get correct control modifier for current OS.
		/// </summary>
		/// <returns>Return Command on OSX otherwise will return Control</returns>
		public static EventModifiers GetControlModifier() {
			if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {
				return EventModifiers.Command;
			}
			return EventModifiers.Control;
		}

		/// <summary>
		/// Mark UnityObject as dirty so Unity should save it.
		/// </summary>
		/// <param name="target"></param>
		public static void MarkDirty(UnityEngine.Object target) {
			if(target == null) return;
			EditorUtility.SetDirty(target);
			var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			if(prefabStage != null) {
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefabStage.scene);
			}
		}

		public static void RegisterUndo(UnityEngine.Object obj, string name = "") {
			if(obj == null) return;
			Undo.RegisterCompleteObjectUndo(obj, name);
			MarkDirty(obj);
			//if(IsPrefabInstance(obj)) {
			//	uNodeThreadUtility.Queue(() => {
			//		if(obj != null) {
			//			PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
			//		}
			//	});
			//}
		}

		public static bool IsTransfromIsChildOf(Transform transform, Transform parent) {
			if(transform == null)
				return false;
			if(transform == parent) {
				return true;
			}
			return IsTransfromIsChildOf(transform.parent, parent);
		}

		public static void RegisterUndoSetTransformParent(Transform transform, Transform newParent, string name = "") {
			if(IsPrefabInstance(transform)) {
				var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(transform);
				if(IsTransfromIsChildOf(newParent, instanceRoot.transform)) {
					if(!IsPrefabInstance(newParent)) {
						PrefabUtility.ApplyPrefabInstance(instanceRoot, InteractionMode.UserAction);
					}
					var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
					var prefabContent = PrefabUtility.LoadPrefabContents(prefabPath);
					var objToMove = uNodeEditorUtility.GetPrefabTransform(transform, instanceRoot.transform, prefabContent.transform);
					var parent = uNodeEditorUtility.GetPrefabTransform(newParent, instanceRoot.transform, prefabContent.transform);
					if(objToMove != null && parent != null) {
						Undo.SetTransformParent(objToMove, parent, name);
						PrefabUtility.SaveAsPrefabAsset(prefabContent, prefabPath);
					}
					PrefabUtility.UnloadPrefabContents(prefabContent);
				} else {
					UnlockPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(transform));
					Undo.SetTransformParent(transform, newParent, name);
					//uNodeThreadUtility.Queue(() => {
					//	if(transform != null) {
					//		PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
					//	}
					//});
				}
			} else {
				Undo.SetTransformParent(transform, newParent, name);
			}
		}

		public static void RegisterFullHierarchyUndo(UnityEngine.Object obj, string name = "") {
			if(obj == null) return;
			Undo.RegisterFullObjectHierarchyUndo(obj, name);
			MarkDirty(obj);
			//if(IsPrefabInstance(obj)) {
			//	uNodeThreadUtility.Queue(() => {
			//		if(obj != null) {
			//			PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
			//		}
			//	});
			//}
		}

		public static MonoScript GetMonoScript(object instance) {
			if(instance != null) {
				if(instance is MonoBehaviour) {
					return MonoScript.FromMonoBehaviour(instance as MonoBehaviour);
				} else if(instance is ScriptableObject) {
					return MonoScript.FromScriptableObject(instance as ScriptableObject);
				}
				return GetMonoScript(instance.GetType());
			}
			return null;
		}

		public static MonoScript GetMonoScript(Type type) {
			foreach(var s in MonoScripts) {
				if(s != null && s.GetClass() == type) {
					return s;
				}
			}
			return null;
		}

		private static System.Text.RegularExpressions.Regex _removeHTMLTagRx = new System.Text.RegularExpressions.Regex("<[^>]*>");
		public static string RemoveHTMLTag(string str) {
			return _removeHTMLTagRx.Replace(str, "");
		}

		private static List<CustomGraphAttribute> _customGraphs;
		/// <summary>
		/// Find command pin menu.
		/// </summary>
		/// <returns></returns>
		public static List<CustomGraphAttribute> FindCustomGraph() {
			if(_customGraphs == null) {
				_customGraphs = new List<CustomGraphAttribute>();

				foreach(System.Reflection.Assembly assembly in EditorReflectionUtility.GetAssemblies()) {
					foreach(System.Type type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
						var atts = type.GetCustomAttributes(typeof(CustomGraphAttribute), true);
						if(atts.Length > 0) {
							foreach(var a in atts) {
								var control = a as CustomGraphAttribute;
								control.type = type;
								_customGraphs.Add(control);
							}
						}
					}
				}
				_customGraphs.Sort((x, y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase));
			}
			return _customGraphs;
		}

		/// <summary>
		/// Load Asset by Guid
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="guid"></param>
		/// <returns></returns>
		public static T LoadAssetByGuid<T>(string guid) where T : UnityEngine.Object {
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if(!string.IsNullOrEmpty(path)) {
				var asset = AssetDatabase.LoadAssetAtPath<T>(path);
				return asset;
			}
			return default(T);
		}

		/// <summary>
		/// Find an object by its unique identifier (file identifier for asset and instance id for scene).
		/// This will search from a 'source' to its children.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="uid"></param>
		/// <returns></returns>
		public static UnityEngine.Object FindObjectByUniqueIdentifier(Transform source, int uid) {
			if(uNodeUtility.GetObjectID(source.gameObject) == uid) {
				return source.gameObject;
			}
			var comps = source.GetComponents<Component>();
			foreach(var c in comps) {
				if(uNodeUtility.GetObjectID(c) == uid) {
					return source.gameObject;
				}
			}
			foreach(Transform t in source) {
				var obj = FindObjectByUniqueIdentifier(t, uid);
				if(obj != null) {
					return obj;
				}
			}
			return null;
		}

		/// <summary>
		/// Find all asset of type T in project
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
			List<T> assets = new List<T>();
			string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)), new[] {"Assets"});
			if(guids.Length == 0) {
				guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name), new[] {"Assets"});
			}
			for(int i = 0; i < guids.Length; i++) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if(asset != null) {
					assets.Add(asset);
				}
			}
			return assets;
		}

		/// <summary>
		/// Find all asset of type T in project indlucing all assets in the sub asset
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindAllAssetsByType<T>() where T : UnityEngine.Object {
			List<T> assets = new List<T>();
			string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)), new[] {"Assets"});
			if(guids.Length == 0) {
				guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name), new[] {"Assets"});
			}
			for(int i = 0; i < guids.Length; i++) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				
				if(asset != null) {
					assets.Add(asset);
                }
			}
			return assets;
		}

		/// <summary>
		/// Find all prefabs in project
		/// </summary>
		/// <returns></returns>
		public static List<GameObject> FindPrefabs() {
			var guids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
			var result = new List<GameObject>();
			foreach(var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if(go != null) {
					result.Add(go);
				}
			}
			return result;
		}

		/// <summary>
		/// Find all component of type T in prefab assets in the project.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> FindComponentInPrefabs<T>() {
			var result = new List<T>();
			var guids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
			foreach(var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if(go != null) {
					var comp = go.GetComponent<T>();
					if(comp != null) {
						result.Add(comp);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Find all component of type type in prefab assets in the project.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<Component> FindComponentInPrefabs(Type type) {
			var result = new List<Component>();
			var guids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
			foreach(var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if(go != null) {
					var comp = go.GetComponents(type);
					if(comp != null) {
						result.AddRange(comp);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Find all prefab that have T component in project.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<GameObject> FindPrefabsOfType<T>(bool includeChildren = false) where T : Component {
			var guids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
			var result = new List<GameObject>();
			foreach(var guid in guids) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				if(go != null) {
					var comp = go.GetComponent(typeof(T));
					if(comp != null) {
						result.Add(go);
					} else if(includeChildren) {
						var comps = go.GetComponentsInChildren(typeof(T));
						if(comps.Length > 0) {
							result.Add(go);
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// True if the target is a prefab.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsPrefab(UnityEngine.Object target) {
			if(target != null && target) {
				return PrefabUtility.GetPrefabType(target) == PrefabType.Prefab;
			}
			return false;
		}

		/// <summary>
		/// True if the target is a prefab.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool IsPrefabInstance(UnityEngine.Object target) {
			if(target != null && target) {
				return PrefabUtility.GetPrefabType(target) == PrefabType.PrefabInstance;
			}
			return false;
		}

		/// <summary>
		/// Save prefab asset.
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="prefab"></param>
		public static GameObject SavePrefabAsset(GameObject gameObject, GameObject prefab) {
			if(IsPrefabInstance(gameObject)) {
				PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
				return prefab;
			}
			if(gameObject != null && prefab != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prefab))) {
				var result = PrefabUtility.SaveAsPrefabAsset(gameObject, AssetDatabase.GetAssetPath(prefab));
				MarkDirty(prefab);
				return result;
			} else {
				return null;
			}
		}

		public static void UnlockPrefabInstance(GameObject gameObject) {
			if (gameObject != null && PrefabUtility.IsPartOfNonAssetPrefabInstance(gameObject)) {
				PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
			}
		}

		/// <summary>
		/// Set the transform parent.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		public static void SetParent(Transform from, Transform to) {
			if(!from || !to)
				return;
			if(IsPrefab(to)) {
				Transform tr = PrefabUtility.InstantiatePrefab(to) as Transform;
				from.SetParent(tr);
				SavePrefabAsset(tr.root.gameObject, to.root.gameObject);
				UnityEngine.Object.DestroyImmediate(tr.root.gameObject);
			} else {
				from.SetParent(to);
			}
		}

		/// <summary>
		/// Get owner of node.
		/// </summary>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		public static uNodeRoot GetOwnerNode(UnityEngine.Object targetObject) {
			if(targetObject) {
				if(targetObject is uNodeRoot)
					return targetObject as uNodeRoot;
				if(targetObject is GameObject || targetObject is Component) {
					if(targetObject is GameObject) {
						GameObject go = targetObject as GameObject;
						var UNR = go.GetComponent<uNodeRoot>();
						if(UNR == null) {
							NodeComponent com = go.GetComponent<NodeComponent>();
							if(com != null) {
								return com.owner;
							}
						}
						return UNR;
					} else {
						Component comp = targetObject as Component;
						var UNR = comp.GetComponent<uNodeRoot>();
						if(UNR == null) {
							NodeComponent com = comp.GetComponent<NodeComponent>();
							if(com != null) {
								return com.owner;
							} else if(comp is RootObject) {
								return (comp as RootObject).owner;
							}
						}
						return UNR;
					}
				}
			}
			return null;
		}

		public static VariableData AddVariable(Type type, List<VariableData> ListVariable, UnityEngine.Object target = null) {
			VariableData variable = new VariableData();
			variable.Type = type;
			variable.Name = "variable" + ListVariable.Count;
			if(uNodePreference.preferenceData.newVariableAccessor == uNodePreference.DefaultAccessor.Private) {
				variable.modifier.SetPrivate();
			}
			if(type.IsValueType || type == typeof(string)) {
				variable.value = ReflectionUtils.CreateInstance(type);
				variable.Serialize();
			}
			int i = 1;
			while(ListVariable.Count > 0) {
				bool correct = true;
				foreach(VariableData var in ListVariable) {
					if(var.Name == variable.Name) {
						variable.Name = "variable" + (ListVariable.Count + i++);
						correct = false;
						break;
					}
				}
				if(correct) {
					break;
				}
			}
			if(target) {
				RegisterUndo(target, "Add Variable: " + variable.type);
			}
			ListVariable.Add(variable);
			if(target is uNodeRoot graph) {
				if(GraphUtility.IsTempGraphObject(graph.gameObject)) {
					uNodeThreadUtility.ExecuteOnce(() => {//Autosave
						GraphUtility.AutoSaveGraph(graph.gameObject);
					}, "UNODE_ADD_VARIABLE_AUTOSAVE" + graph.gameObject.GetInstanceID());
				}
			}
			return variable;
		}

		public static void ResizeList(IList array, Type elementType, int newSize, UnityEngine.Object unityObject = null) {
			if(newSize < 0) {
				newSize = 0;
			}
			if(array == null || newSize == array.Count) {
				return;
			}
			while(newSize > array.Count) {
				if(array.Count == 0) {
					array.Add(ReflectionUtils.CreateInstance(elementType));
				}
				uNodeEditorUtility.RegisterUndo(unityObject, "");
				object newObj = array[array.Count - 1];
				if(newObj != null) {
					if(!newObj.GetType().IsValueType && !(newObj.GetType() == typeof(UnityEngine.Object) || newObj.GetType().IsSubclassOf(typeof(UnityEngine.Object)))) {
						newObj = SerializerUtility.Duplicate(newObj);
					}
				}
				array.Add(newObj);
			}
			while(newSize < array.Count) {
				if(array.Count == 0) {
					break;
				}
				uNodeEditorUtility.RegisterUndo(unityObject, "");
				array.RemoveAt(array.Count - 1);
			}
		}

		public static Array ResizeArray(Array array, Type elementType, int newSize) {
			if(newSize < 0) {
				newSize = 0;
			}
			if(array == null || newSize == array.Length) {
				return array;
			}
			Array array2 = Array.CreateInstance(elementType, newSize);
			int num = Math.Min(newSize, array.Length);
			for(int i = 0; i < num; i++) {
				array2.SetValue(array.GetValue(i), i);
			}
			return array2;
		}

		public static void DuplicateArrayAt(ref Array array, int index) {
			Array array2 = Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
			int skipped = 0;
			for(int i = 0; i < array2.Length; i++) {
				array2.SetValue(array.GetValue(i - skipped), i);
				if(index == i) {
					object obj = array.GetValue(i);
					if(obj != null) {
						if(!obj.GetType().IsValueType && !(obj.GetType() == typeof(UnityEngine.Object) || obj.GetType().IsSubclassOf(typeof(UnityEngine.Object)))) {
							obj = SerializerUtility.Duplicate(obj);
						}
					}
					array2.SetValue(obj, i + 1);
					skipped++;
					i += 1;
				}
			}
			array = array2;
		}

		/// <summary>
		/// Save editor data.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="fileName"></param>
		public static void SaveEditorData<T>(T value, string fileName) {
			Directory.CreateDirectory("uNode2Data");
			char separator = Path.DirectorySeparatorChar;
			string path = "uNode2Data" + separator + fileName + ".byte";
			File.WriteAllBytes(path, SerializerUtility.Serialize(value));
		}

		/// <summary>
		/// Save editor data.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="fileName"></param>
		/// <param name="references"></param>
		public static void SaveEditorData<T>(T value, string fileName, out List<UnityEngine.Object> references) {
			references = new List<UnityEngine.Object>();
			Directory.CreateDirectory("uNode2Data");
			char separator = Path.DirectorySeparatorChar;
			string path = "uNode2Data" + separator + fileName + ".bytes";
			File.WriteAllBytes(path, SerializerUtility.Serialize(value, out references));
		}

		/// <summary>
		/// Load editor data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static T LoadEditorData<T>(string fileName) {
			char separator = Path.DirectorySeparatorChar;
			string path = "uNode2Data" + separator + fileName + ".byte";
			T value;
			if(File.Exists(path)) {
				value = SerializerUtility.Deserialize<T>(File.ReadAllBytes(path));
			} else {
				value = default(T);
			}
			return value;
		}

		/// <summary>
		/// Load editor data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fileName"></param>
		/// <param name="references"></param>
		/// <returns></returns>
		public static T LoadEditorData<T>(string fileName, List<UnityEngine.Object> references) {
			char separator = Path.DirectorySeparatorChar;
			string path = "uNode2Data" + separator + fileName + ".byte";
			T value;
			if(File.Exists(path)) {
				var obj = SerializerUtility.Deserialize<T>(File.ReadAllBytes(path), references);
				if(obj != null) {
					value = (T)obj;
				} else {
					value = default(T);
				}
			} else {
				value = default(T);
			}
			return value;
		}

		public static void AddDefineSymbols(IEnumerable<string> symbols) {
			string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> allDefines = definesString.Split(';').ToList();
			allDefines.AddRange(symbols.Except(allDefines));
			PlayerSettings.SetScriptingDefineSymbolsForGroup(
				EditorUserBuildSettings.selectedBuildTargetGroup,
				string.Join(";", allDefines.ToArray()));
		}

		public static void RemoveDefineSymbols(IEnumerable<string> symbols) {
			string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> allDefines = definesString.Split(';').ToList();
			allDefines.RemoveAll(d => symbols.Contains(d));
			PlayerSettings.SetScriptingDefineSymbolsForGroup(
				EditorUserBuildSettings.selectedBuildTargetGroup,
				string.Join(";", allDefines.ToArray()));
		}
		#endregion

		#region GenericMenuUtils
		public static void ShowGenericOptionMenu(IList array, int index, Action<IList> action, UnityEngine.Object unityObject = null) {
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Duplicate Element"), false, delegate (object obj) {
				uNodeEditorUtility.RegisterUndo(unityObject, "Duplicate Array Element");
				object newObj = array[index];
				if(newObj != null) {
					if(!newObj.GetType().IsValueType && !(newObj.GetType() == typeof(UnityEngine.Object) || newObj.GetType().IsSubclassOf(typeof(UnityEngine.Object)))) {
						newObj = SerializerUtility.Duplicate(newObj);
					}
				}
				array.Insert(index, newObj);
				if(action != null) {
					action(array);
				}
			}, index);
			menu.AddItem(new GUIContent("Delete Element"), false, delegate (object obj) {
				uNodeEditorUtility.RegisterUndo(unityObject, "Delete Array Element");
				array.RemoveAt(index);
				if(action != null) {
					action(array);
				}
			}, index);
			menu.AddSeparator("");
			if(index != 0) {
				menu.AddItem(new GUIContent("Move To Top"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element To Top");
					int nums = (int)obj;
					ListMoveToTop(array, nums);
					if(action != null) {
						action(array);
					}
				}, index);
				menu.AddItem(new GUIContent("Move Up"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element Up");
					int nums = (int)obj;
					ListMoveUp(array, nums);
					if(action != null) {
						action(array);
					}
				}, index);
			}
			if(index + 1 != array.Count) {
				menu.AddItem(new GUIContent("Move Down"), false, delegate (object obj) {
					if(unityObject)
						uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element Down");
					int nums = (int)obj;
					ListMoveDown(array, nums);
					if(action != null) {
						action(array);
					}
				}, index);
				menu.AddItem(new GUIContent("Move To End"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element To End");
					int nums = (int)obj;
					ListMoveToBottom(array, nums);
					if(action != null) {
						action(array);
					}
				}, index);
			}
			menu.ShowAsContext();
		}

		public static void ListMoveUp(IList list, int index) {
			if(index != 0) {
				object self = list[index];
				list.RemoveAt(index);
				list.Insert(index - 1, self);
			}
		}

		public static void ListMoveDown(IList list, int index) {
			if(index + 1 != list.Count) {
				object self = list[index];
				list.RemoveAt(index);
				list.Insert(index + 1, self);
			}
		}

		public static void ListMoveToTop(IList list, int index) {
			if(index != 0) {
				var self = list[index];
				list.RemoveAt(index);
				list.Insert(0, self);
			}
		}

		public static void ListMoveToBottom(IList list, int index) {
			if(index + 1 != list.Count) {
				var self = list[index];
				list.RemoveAt(index);
				list.Add(self);
			}
		}

		public static void ShowArrayOptionMenu(Array array, int index, Action<Array> action, UnityEngine.Object unityObject = null) {
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Duplicate Element"), false, delegate (object obj) {
				uNodeEditorUtility.RegisterUndo(unityObject, "Duplicate Array Element");
				int nums = (int)obj;
				DuplicateArrayAt(ref array, nums);
				if(action != null)
					action(array);
			}, index);
			menu.AddItem(new GUIContent("Delete Element"), false, delegate (object obj) {
				uNodeEditorUtility.RegisterUndo(unityObject, "Delete Array Element");
				int nums = (int)obj;
				uNodeUtility.RemoveArrayAt(ref array, nums);
				if(action != null)
					action(array);
			}, index);
			menu.AddSeparator("");
			if(index != 0) {
				menu.AddItem(new GUIContent("Move To Top"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element To Top");
					int nums = (int)obj;
					object self = array.GetValue(nums);
					object target = null;
					for(int i = 0; i < array.Length; i++) {
						object element = target;
						target = array.GetValue(i);
						if(i == 0) {
							array.SetValue(self, i);
							continue;
						}
						if(i <= nums) {
							array.SetValue(element, i);
							continue;
						}
						array.SetValue(target, i);
					}
					if(action != null)
						action(array);
				}, index);
				menu.AddItem(new GUIContent("Move Up"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element Up");
					int nums = (int)obj;
					object self = array.GetValue(nums);
					object target = array.GetValue(nums - 1);
					array.SetValue(self, nums - 1);
					array.SetValue(target, nums);
					if(action != null)
						action(array);
				}, index);
			}
			if(index + 1 != array.Length) {
				menu.AddItem(new GUIContent("Move Down"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element Down");
					int nums = (int)obj;
					object self = array.GetValue(nums);
					object target = array.GetValue(nums + 1);
					array.SetValue(self, nums + 1);
					array.SetValue(target, nums);
					if(action != null)
						action(array);
				}, index);
				menu.AddItem(new GUIContent("Move To End"), false, delegate (object obj) {
					uNodeEditorUtility.RegisterUndo(unityObject, "Move Array Element To End");
					int nums = (int)obj;
					object self = array.GetValue(nums);
					object target = null;
					for(int i = 0; i < array.Length; i++) {
						if(i + 1 == array.Length) {
							array.SetValue(self, i);
							continue;
						}
						if(i < nums)
							continue;
						if(i + 1 != array.Length) {
							target = array.GetValue(i + 1);
						}
						array.SetValue(target, i);
					}
					if(action != null)
						action(array);
				}, index);
			}
			menu.ShowAsContext();
		}

		public static void ShowTypeMenu(object userData, Action<object, Type> onClick) {
			GenericMenu menu = new GenericMenu();
			ShowTypeMenu(userData, onClick, menu);
			menu.ShowAsContext();
		}

		public static void ShowTypeMenu(object userData, Action<object, Type> onClick, GenericMenu menu) {
			menu.AddItem(new GUIContent("String"), false, delegate (object obj) {
				onClick(obj, typeof(string));
			}, userData);
			menu.AddItem(new GUIContent("Bool"), false, delegate (object obj) {
				onClick(obj, typeof(bool));
			}, userData);
			menu.AddItem(new GUIContent("Int"), false, delegate (object obj) {
				onClick(obj, typeof(int));
			}, userData);
			menu.AddItem(new GUIContent("Float"), false, delegate (object obj) {
				onClick(obj, typeof(float));
			}, userData);
			menu.AddItem(new GUIContent("Vector2"), false, delegate (object obj) {
				onClick(obj, typeof(Vector2));
			}, userData);
			menu.AddItem(new GUIContent("Vector3"), false, delegate (object obj) {
				onClick(obj, typeof(Vector3));
			}, userData);
			menu.AddItem(new GUIContent("Vector4"), false, delegate (object obj) {
				onClick(obj, typeof(Vector4));
			}, userData);
			menu.AddItem(new GUIContent("Quaternion"), false, delegate (object obj) {
				onClick(obj, typeof(Quaternion));
			}, userData);
			menu.AddItem(new GUIContent("Rect"), false, delegate (object obj) {
				onClick(obj, typeof(Rect));
			}, userData);
			menu.AddItem(new GUIContent("Color"), false, delegate (object obj) {
				onClick(obj, typeof(Color));
			}, userData);
			menu.AddItem(new GUIContent("Transform"), false, delegate (object obj) {
				onClick(obj, typeof(Transform));
			}, userData);
			menu.AddItem(new GUIContent("GameObject"), false, delegate (object obj) {
				onClick(obj, typeof(GameObject));
			}, userData);
			menu.AddItem(new GUIContent("uNodeRuntime"), false, delegate (object obj) {
				onClick(obj, typeof(uNodeRuntime));
			}, userData);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Object"), false, delegate (object obj) {
				onClick(obj, typeof(UnityEngine.Object));
			}, userData);
			menu.AddItem(new GUIContent("System.Object"), false, delegate (object obj) {
				onClick(obj, typeof(object));
			}, userData);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Array/String"), false, delegate (object obj) {
				onClick(obj, typeof(string[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Bool"), false, delegate (object obj) {
				onClick(obj, typeof(bool[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Int"), false, delegate (object obj) {
				onClick(obj, typeof(int[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Float"), false, delegate (object obj) {
				onClick(obj, typeof(float[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Vector2"), false, delegate (object obj) {
				onClick(obj, typeof(Vector2[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Vector3"), false, delegate (object obj) {
				onClick(obj, typeof(Vector3[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Vector4"), false, delegate (object obj) {
				onClick(obj, typeof(Vector4[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Quaternion"), false, delegate (object obj) {
				onClick(obj, typeof(Quaternion[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Rect"), false, delegate (object obj) {
				onClick(obj, typeof(Rect[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Color"), false, delegate (object obj) {
				onClick(obj, typeof(Color[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/Transform"), false, delegate (object obj) {
				onClick(obj, typeof(Transform[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/GameObject"), false, delegate (object obj) {
				onClick(obj, typeof(GameObject[]));
			}, userData);
			menu.AddSeparator("Array/");
			menu.AddItem(new GUIContent("Array/Object"), false, delegate (object obj) {
				onClick(obj, typeof(UnityEngine.Object[]));
			}, userData);
			menu.AddItem(new GUIContent("Array/System.Object"), false, delegate (object obj) {
				onClick(obj, typeof(object[]));
			}, userData);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Generic List/String"), false, delegate (object obj) {
				onClick(obj, typeof(List<string>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Bool"), false, delegate (object obj) {
				onClick(obj, typeof(List<bool>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Int"), false, delegate (object obj) {
				onClick(obj, typeof(List<int>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Float"), false, delegate (object obj) {
				onClick(obj, typeof(List<float>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Vector2"), false, delegate (object obj) {
				onClick(obj, typeof(List<Vector2>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Vector3"), false, delegate (object obj) {
				onClick(obj, typeof(List<Vector3>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Vector4"), false, delegate (object obj) {
				onClick(obj, typeof(List<Vector4>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Quaternion"), false, delegate (object obj) {
				onClick(obj, typeof(List<Quaternion>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Rect"), false, delegate (object obj) {
				onClick(obj, typeof(List<Rect>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Color"), false, delegate (object obj) {
				onClick(obj, typeof(List<Color>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/Transform"), false, delegate (object obj) {
				onClick(obj, typeof(List<Transform>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/GameObject"), false, delegate (object obj) {
				onClick(obj, typeof(List<GameObject>));
			}, userData);
			menu.AddSeparator("Generic List/");
			menu.AddItem(new GUIContent("Generic List/Object"), false, delegate (object obj) {
				onClick(obj, typeof(List<UnityEngine.Object>));
			}, userData);
			menu.AddItem(new GUIContent("Generic List/System.Object"), false, delegate (object obj) {
				onClick(obj, typeof(List<object>));
			}, userData);
		}

		public static void ShowAddVariableMenu(Vector2 position, List<VariableData> variables, UnityEngine.Object target) {
			var customItmes = ItemSelector.MakeCustomTypeItems(new Type[] {
				typeof(string),
				typeof(float),
				typeof(bool),
				typeof(int),
				typeof(Vector2),
				typeof(Vector3),
				typeof(Rect),
				typeof(Transform),
				typeof(GameObject),
				typeof(uNodeRuntime),
				typeof(List<>),
			}, "General");
			var window = ItemSelector.ShowWindow( 
				target, 
				new FilterAttribute() { OnlyGetType = true, UnityReference = false }, (m) => {
					AddVariable(m.Get<Type>(), variables, target);
				},  true,  customItmes).ChangePosition(position);
			window.displayNoneOption = false;
			window.displayGeneralType = false;
		}

		public static void ShowAddVariableMenu(GenericMenu menu, List<VariableData> ESVariable, UnityEngine.Object target) {
			ShowTypeMenu(ESVariable, delegate (object obj, Type type) {
				List<VariableData> UNR = (List<VariableData>)obj;
				AddVariable(type, UNR, target);
			}, menu);
			menu.ShowAsContext();
		}
		#endregion
	}
	#region PropertyDrawer
	public class PropertyDrawerUtility {
		public static T GetActualObjectFromPath<T>(string propertyPath, UnityEngine.Object targetObject) where T : class {
			object obj = null;
			if(obj == null) {
				obj = EditorReflectionUtility.GetMemberValue(propertyPath, targetObject);
				return obj as T;
			} else {
				T actualObject = null;
				if(obj.GetType().IsArray) {
					var index = Convert.ToInt32(new string(propertyPath.Where(c => char.IsDigit(c)).ToArray()));
					actualObject = ((T[])obj)[index];
				} else {
					actualObject = obj as T;
				}
				return actualObject;
			}
		}
		public static T GetActualObjectForSerializedProperty<T>(SerializedProperty property) where T : class {
			object obj = null;
			obj = EditorReflectionUtility.GetMemberValue(property.propertyPath, property.serializedObject.targetObject);
			return obj as T;
		}
		public static T GetParentObjectFromSerializedProperty<T>(SerializedProperty property) where T : class {
			object[] obj = null;
			obj = EditorReflectionUtility.GetMemberValues(property.propertyPath, property.serializedObject.targetObject);
			if(obj == null || obj.Length < 2) {
				if(obj != null && obj.Length == 1 && property.depth > 0) {
					return obj[obj.Length - 1] as T;
				}
				return property.serializedObject.targetObject as T;
			}
			return obj[obj.Length - 2] as T;
		}

		public static object[] GetObjectFromSerializedProperty(SerializedProperty property) {
			object[] obj = null;
			obj = EditorReflectionUtility.GetMemberValues(property.propertyPath, property.serializedObject.targetObject);
			return obj;
		}
	}
	#endregion
	
	#region EditorProgressBar
	public static class EditorProgressBar {
		static bool isDisplaying;

		public static bool IsInProgress() {
			return isDisplaying;
		}

#if UNITY_2020_1_OR_NEWER
		static int progressId;
		public static void ShowProgressBar(string description, float progress) {
			if(!isDisplaying) {
				isDisplaying = true;
				progressId = Progress.Start("uNode");
			}
			Progress.Report(progressId, progress, description);
		}

		public static void ClearProgressBar() {
			if(isDisplaying) {
				isDisplaying = false;
				Progress.Remove(progressId);
			}
		}
#else
		static MethodInfo m_Display = null;
		static MethodInfo m_Clear = null;
		static EditorProgressBar() {
			var type = typeof(Editor).Assembly.GetTypes().Where(t => t.Name == "AsyncProgressBar").FirstOrDefault();
			if(type != null) {
				m_Display = type.GetMethod("Display");
				m_Clear = type.GetMethod("Clear");
			}
		}

		public static void ShowProgressBar(string description, float progress) {
			try {
				if(m_Display != null) {
					m_Display.InvokeOptimized(null, new object[] { description, progress });
					isDisplaying = true;
				}
			}
			catch { }
		}

		public static void ClearProgressBar() {
			try {
				if(m_Clear != null) {
					m_Clear.InvokeOptimized(null, null);
					isDisplaying = false;
				}
			}
			catch { }
		}
#endif
	}
	#endregion
}