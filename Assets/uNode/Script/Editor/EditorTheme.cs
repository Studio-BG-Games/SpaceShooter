using System;
using UnityEngine;

namespace MaxyGames.uNode {
	public abstract class BaseEditorTheme : ScriptableObject {
		public string themeName;
		public EditorTextSetting textSettings = new EditorTextSetting();

		[HideInInspector]
		public bool expanded = false;

		public abstract System.Type GetGraphType();
	}
}