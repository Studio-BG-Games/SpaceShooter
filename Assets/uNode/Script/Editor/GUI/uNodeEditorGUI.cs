using UnityEngine;
using UnityEditor;

namespace MaxyGames.uNode.Editors {
	public class uNodeEditorGUI {
		#region Private
		private static readonly GUIContent s_TempContent = new GUIContent();

		private static GUIContent TempContent(string text) {
			s_TempContent.text = text;
			s_TempContent.image = null;
			s_TempContent.tooltip = null;
			return s_TempContent;
		}
		#endregion

		#region Original
		private static readonly int s_LabelHint = "__labelHash".GetHashCode();
		public static void Label(Rect position, GUIContent label, GUIStyle style = null) {
			if(style == null) {
				style = "Label";
			}
			EditorGUI.LabelField(position, label, style);
		}

		public static void Label(Rect position, GUIContent label, GUIContent label2, GUIStyle style = null) {
			if(label2 == GUIContent.none) {
				Label(position, label);
				return;
			}
			if(style == null) {
				style = "Label";
			}
			EditorGUI.LabelField(position, label, label2, style);
		}

		private static readonly int s_ButtonHint = "__buttonsHash".GetHashCode();

		public static bool Button(Rect position, GUIContent label, GUIStyle style = null) {
			if(style == null) {
				style = GUI.skin.button;
			}
			return EditorGUI.DropdownButton(position, label, FocusType.Keyboard, style);
		}
		#endregion

		#region Helper
		public static void Label(Rect position, string label, GUIStyle style = null) {
			Label(position, TempContent(label), style);
		}

		public static bool Button(Rect position, string label, GUIStyle style = null) {
			return Button(position, TempContent(label), style);
		}
		#endregion

		#region Layout Version
		public static string TextInput(string text, string placeholder, bool area = false, params GUILayoutOption[] options) {
			var newText = area ? EditorGUILayout.TextArea(text, options) : EditorGUILayout.TextField(text, options);
			if(string.IsNullOrEmpty(text)) {
				const int textMargin = 2;
				var guiColor = GUI.color;
				GUI.color = Color.grey;
				var textRect = GUILayoutUtility.GetLastRect();
				var position = new Rect(textRect.x + textMargin, textRect.y, textRect.width, textRect.height);
				EditorGUI.LabelField(position, placeholder);
				GUI.color = guiColor;
			}
			return newText;
		}

		public static void Label(GUIContent label, GUIStyle style, params GUILayoutOption[] options) {
			if(style == null) {
				style = "Label";
			}
			Rect position = GUILayoutUtility.GetRect(label, style, options);
			Label(position, label, style);
		}

		public static void Label(GUIContent label, params GUILayoutOption[] options) {
			Label(label, null, options);
		}

		public static void Label(string label, GUIStyle style, params GUILayoutOption[] options) {
			Label(TempContent(label), style, options);
		}

		public static void Label(string label, params GUILayoutOption[] options) {
			Label(label, null, options);
		}

		public static bool Button(GUIContent label, GUIStyle style, params GUILayoutOption[] options) {
			if(style == null) {
				style = GUI.skin.button;
			}
			Rect position = GUILayoutUtility.GetRect(label, style, options);
			return Button(position, label, style);
		}

		public static bool Button(GUIContent label, params GUILayoutOption[] options) {
			return Button(label, null, options);
		}

		public static bool Button(string label, GUIStyle style, params GUILayoutOption[] options) {
			return Button(TempContent(label), style, options);
		}

		public static bool Button(string label, params GUILayoutOption[] options) {
			return Button(label, null, options);
		}
		#endregion
	}
}