using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// The field control for editing values
	/// </summary>
	public abstract class FieldControl {
		public virtual int order => 0;
		
		public abstract bool IsValidControl(Type type, bool layouted);

		public virtual void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {

		}
		
		public virtual void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			if(string.IsNullOrEmpty(label.tooltip)) {
				label.tooltip = settings?.Tooltip;
			}
			Draw(uNodeGUIUtility.GetRect(), label, value, type, onChanged, settings);
		}

		protected void DrawDecorators(uNodeUtility.EditValueSettings settings) {
			if(settings.drawDecorator)
				FieldDecorator.DrawDecorators(settings.attributes);
		}

		protected void ValidateValue<T>(ref object value, bool nullable = false) {
			if (!(value is T)) {
				if (value != null && value.GetType().IsCastableTo(typeof(T))) {
					value = (T)value;
					GUI.changed = true;
				} else {
					value = default(T);
					if(value == null && !nullable && ReflectionUtils.CanCreateInstance(typeof(T))) {
						value = ReflectionUtils.CreateInstance(typeof(T));
					}
					GUI.changed = value != null;
				}
			}
		}

		private static List<FieldControl> _fieldControls;
		public static List<FieldControl> FindControls() {
			if(_fieldControls == null) {
				_fieldControls = new List<FieldControl>();
				foreach(var assembly in EditorReflectionUtility.GetAssemblies()) {
					try {
						foreach(System.Type type in EditorReflectionUtility.GetAssemblyTypes(assembly)) {
							if(type.IsSubclassOf(typeof(FieldControl)) && ReflectionUtils.CanCreateInstance(type)) {
								var control = ReflectionUtils.CreateInstance(type) as FieldControl;
								_fieldControls.Add(control);
							}
						}
					}
					catch { continue; }
				}
				_fieldControls.Sort((x, y) => CompareUtility.Compare(x.order, y.order));
			}
			return _fieldControls;
		}

		private static Dictionary<Type, FieldControl> _fieldControlMap = new Dictionary<Type, FieldControl>();
		private static Dictionary<Type, FieldControl> _fieldLayoutedControlMap = new Dictionary<Type, FieldControl>();
		private static FieldControl unsupportedControl = new UnsupportedFieldControl();

		public static FieldControl FindControl(Type type, bool layouted) {
			if(type == null) return unsupportedControl;
			FieldControl control;
			if(layouted) {
				if(_fieldLayoutedControlMap.TryGetValue(type, out control)) {
					return control;
				}
			} else {
				if(_fieldControlMap.TryGetValue(type, out control)) {
					return control;
				}
			}
			var controls = FindControls();
			for(int i=0;i<controls.Count;i++) {
				if(controls[i].IsValidControl(type, layouted)) {
					control = controls[i];
					break;
				}
			}
			if(layouted) {
				_fieldLayoutedControlMap[type] = control;
			} else {
				_fieldControlMap[type] = control;
			}
			return control;
		}
	}

	class UnsupportedFieldControl : FieldControl {
		public override bool IsValidControl(Type type, bool layouted) {
			return false;
		}

		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			position = EditorGUI.PrefixLabel(position, label);
			EditorGUI.SelectableLabel(position, label.text);
		}
	}
	
	public abstract class FieldControl<T> : FieldControl {
		public override bool IsValidControl(Type type, bool layouted) {
			if (type == typeof(T)) {
				return true;
			}
			return false;
		}

		protected void ValidateValue(ref object value, bool nullable = false) {
			if (!(value is T)) {
				if (value != null && value.GetType().IsCastableTo(typeof(T))) {
					value = Operators.Convert(value, typeof(T));
					GUI.changed = true;
				} else {
					value = default(T);
					if(value == null && !nullable && ReflectionUtils.CanCreateInstance(typeof(T))) {
						value = ReflectionUtils.CreateInstance(typeof(T));
					}
					GUI.changed = value != null;
				}
			}
		}
	}

	public class IntFieldControl : FieldControl<int> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = (int)value;
			var att = ReflectionUtils.GetAttribute<RangeAttribute>(settings.attributes);
			if(att != null) {
				fieldValue = EditorGUI.IntSlider(position, label, fieldValue, (int)att.min, (int)att.max);
			} else {
				fieldValue = EditorGUI.IntField(position, label, fieldValue);
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class ByteFieldControl : FieldControl<byte> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (byte)value;
			var newValue =  (byte)EditorGUI.IntField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class SByteFieldControl : FieldControl<sbyte> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (sbyte)value;
			var newValue =  (sbyte)EditorGUI.IntField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class CharFieldControl : FieldControl<char> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (char)value;
			var newValue = EditorGUI.TextField(position, label, oldValue.ToString());
			if(!string.IsNullOrEmpty(newValue)) {
				oldValue = newValue[0];
			} else {
				oldValue = new char();
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(oldValue);
			}
		}
	}

	public class UIntFieldControl : FieldControl<uint> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (uint)value;
			var newValue = (uint)EditorGUI.IntField(position, label, (int)oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class ShortFieldControl : FieldControl<short> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (short)value;
			var newValue = (short)EditorGUI.IntField(position, label, (int)oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class UShortFieldControl : FieldControl<ushort> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (ushort)value;
			var newValue = (ushort)EditorGUI.IntField(position, label, (int)oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class FloatFieldControl : FieldControl<float> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = (float)value;
			var att = ReflectionUtils.GetAttribute<RangeAttribute>(settings.attributes);
			if(att != null) {
				fieldValue = EditorGUI.Slider(position, label, fieldValue, att.min, att.max);
			} else {
				fieldValue = EditorGUI.FloatField(position, label, fieldValue);
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class BoolFieldControl : FieldControl<bool> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (bool)value;
			var newValue = EditorGUI.Toggle(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class DoubleFieldControl : FieldControl<double> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (double)value;
			var newValue = EditorGUI.DoubleField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class DecimalFieldControl : FieldControl<decimal> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (decimal)value;
			var newValue = (decimal)EditorGUI.DoubleField(position, label, (double)oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class LongFieldControl : FieldControl<long> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (long)value;
			var newValue = EditorGUI.LongField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class ULongFieldControl : FieldControl<ulong> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (ulong)value;
			var newValue = (ulong)EditorGUI.LongField(position, label, (long)oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class RectFieldControl : FieldControl<Rect> {
		private static GUIContent[] contents = new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("W"), new GUIContent("H") };

		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Rect)value;
			var arr = new[] { oldValue.x, oldValue.y, oldValue.width, oldValue.height };
			EditorGUI.MultiFloatField(position, label, contents, arr);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(new Rect(arr[0], arr[1], arr[2], arr[3]));
			}
		}
	}

	public class ColorFieldControl : FieldControl<Color> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = (Color)value;
			var usage = ReflectionUtils.GetAttribute<ColorUsageAttribute>(settings.attributes);
			if(usage != null) {
            	fieldValue = EditorGUI.ColorField(position, label, fieldValue, true, usage.showAlpha, usage.hdr);
			} else {
				fieldValue = EditorGUI.ColorField(position, label, fieldValue);
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class Color32FieldControl : FieldControl<Color32> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Color32)value;
			var newValue = (Color32)EditorGUI.ColorField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class BoundsFieldControl : FieldControl<Bounds> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Bounds)value;
			var newValue = EditorGUI.BoundsField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Bounds)value;
			var newValue = EditorGUILayout.BoundsField(label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class Vector2FieldControl : FieldControl<Vector2> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector2)value;
			var newValue = EditorGUI.Vector2Field(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector2)value;
			var newValue = EditorGUILayout.Vector2Field(label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class Vector2IntFieldControl : FieldControl<Vector2Int> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector2Int)value;
			var newValue = EditorGUI.Vector2IntField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector2Int)value;
			var newValue = EditorGUILayout.Vector2IntField(label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class Vector3FieldControl : FieldControl<Vector3> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector3)value;
			var newValue = EditorGUI.Vector3Field(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector3)value;
			var newValue = EditorGUILayout.Vector3Field(label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class Vector3IntFieldControl : FieldControl<Vector3Int> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector3Int)value;
			var newValue = EditorGUI.Vector3IntField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector3Int)value;
			var newValue = EditorGUILayout.Vector3IntField(label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class Vector4FieldControl : FieldControl<Vector4> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector4)value;
			var newValue = EditorGUI.Vector4Field(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector4)value;
			var newValue = EditorGUILayout.Vector4Field(label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class QuaternionFieldControl : FieldControl<Quaternion> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Quaternion)value;
			var newValue = Quaternion.Euler(EditorGUI.Vector3Field(position, label, oldValue.eulerAngles));
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Quaternion)value;
			var newValue = Quaternion.Euler(EditorGUILayout.Vector3Field(label, oldValue.eulerAngles));
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class LayerMaskFieldControl : FieldControl<LayerMask> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (LayerMask)value;
			var newValue = EditorGUI.LayerField(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged((LayerMask)newValue);
			}
		}
	}

	public class GradientFieldControl : FieldControl<Gradient> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = value as Gradient;
			var usage = ReflectionUtils.GetAttribute<GradientUsageAttribute>(settings.attributes);
			if(usage != null) {
            	fieldValue = EditorGUI.GradientField(position, label, fieldValue, usage.hdr);
			} else {
				fieldValue = EditorGUI.GradientField(position, label, fieldValue);
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class EnumFieldControl : FieldControl {
		public override bool IsValidControl(Type type, bool layouted) {
			if (type.IsEnum) {
				return true;
			}
			return false;
		}

		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			if(value == null) {
				value = ReflectionUtils.CreateInstance(type) as Enum;
				GUI.changed = true;
			} else if(value is int) {
				value = Enum.ToObject(type, value);
			} else if(value.GetType() != type) {
				value = ReflectionUtils.CreateInstance(type);
			}
			var oldValue = (Enum)value;
			var newValue = EditorGUI.EnumPopup(position, label, oldValue);
			if (EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}

	public class AnimationCurveFieldControl : FieldControl<AnimationCurve> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value, settings != null ? settings.nullable : false);
			var fieldValue = value as AnimationCurve;
			if(value != null) {
				if(settings.nullable)
					position.width -= 16;
				fieldValue = EditorGUI.CurveField(position, label, fieldValue);
				if(settings.nullable) {
					position.x += position.width;
					position.width = 16;
					if(GUI.Button(position, GUIContent.none) && Event.current.button == 0) {
						fieldValue = null;
						GUI.changed = true;
					}
				}
			} else {
				uNodeGUIUtility.DrawNullValue(position, label, type, delegate (object o) {
					fieldValue = o as AnimationCurve;
					onChanged(o);
				});
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class StringFieldControl : FieldControl<string> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			var attributes = settings.attributes;
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value, settings != null ? settings.nullable : false);
			var fieldValue = value as string;
			if(value != null) {
				if(settings.nullable)
					position.width -= 16;
				ObjectTypeAttribute drawer = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(attributes);
				FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(attributes);
				if(drawer != null) {
					if(filter != null) {
						if(drawer.type != null)
							filter.Types.Add(drawer.type);
						filter.ArrayManipulator = false;
						filter.OnlyGetType = true;
						filter.UnityReference = false;
					}
					uNodeGUIUtility.DrawTypeDrawer(position, TypeSerializer.Deserialize(fieldValue, false), drawer, label, delegate (Type t) {
						if(t != null) {
							fieldValue = t.FullName;
						} else {
							fieldValue = "";
						}
						onChanged(fieldValue);
					}, filter);
				} else if(filter != null) {
					filter.ArrayManipulator = false;
					filter.OnlyGetType = true;
					filter.UnityReference = false;
					uNodeGUIUtility.DrawTypeDrawer(position, TypeSerializer.Deserialize(fieldValue, false), null, label, delegate (Type t) {
						if(t != null) {
							fieldValue = t.FullName;
						} else {
							fieldValue = "";
						}
						onChanged(fieldValue);
					}, filter);
				} else {
					TextAreaAttribute textAtt = ReflectionUtils.GetAttribute<TextAreaAttribute>(attributes);
					if(textAtt != null) {
						position = EditorGUI.PrefixLabel(position, label);
						fieldValue = EditorGUI.TextArea(position, fieldValue);
					} else {
						fieldValue = EditorGUI.TextField(position, label, fieldValue);
					}
				}
				if(settings.nullable) {
					position.x += position.width;
					position.width = 16;
					if(GUI.Button(position, GUIContent.none) && Event.current.button == 0) {
						fieldValue = null;
						GUI.changed = true;
					}
				}
			} else {
				uNodeGUIUtility.DrawNullValue(position, label, type, delegate (object o) {
					onChanged(fieldValue = o as string);
				});
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
		
		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			var attributes = settings.attributes;
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value, settings != null ? settings.nullable : false);
			var fieldValue = value as string;
			if(value != null) {
				if(settings.nullable) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
				}
				ObjectTypeAttribute drawer = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(attributes);
				FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(attributes);
				if(drawer != null) {
					if(filter != null) {
						if(drawer.type != null)
							filter.Types.Add(drawer.type);
						filter.ArrayManipulator = false;
						filter.OnlyGetType = true;
						filter.UnityReference = false;
					}
					uNodeGUIUtility.DrawTypeDrawer(uNodeGUIUtility.GetRect(), TypeSerializer.Deserialize(fieldValue, false), drawer, label, delegate (Type t) {
						if(t != null) {
							fieldValue = t.FullName;
						} else {
							fieldValue = "";
						}
						onChanged(fieldValue);
					}, filter);
				} else if(filter != null) {
					filter.ArrayManipulator = false;
					filter.OnlyGetType = true;
					filter.UnityReference = false;
					uNodeGUIUtility.DrawTypeDrawer(uNodeGUIUtility.GetRect(), TypeSerializer.Deserialize(fieldValue, false), null, label, delegate (Type t) {
						if(t != null) {
							fieldValue = t.FullName;
						} else {
							fieldValue = "";
						}
						onChanged(fieldValue);
					}, filter);
				} else {
					TextAreaAttribute textAtt = ReflectionUtils.GetAttribute<TextAreaAttribute>(attributes);
					if(textAtt != null) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel(label);
						fieldValue = EditorGUILayout.TextArea(fieldValue);
						EditorGUILayout.EndHorizontal();
					} else {
						fieldValue = EditorGUILayout.TextField(label, fieldValue);
					}
				}
				if(settings.nullable) {
					EditorGUILayout.EndVertical();
					if(GUILayout.Button(GUIContent.none, GUILayout.Width(16)) && Event.current.button == 0) {
						fieldValue = null;
						GUI.changed = true;
					}
					EditorGUILayout.EndHorizontal();
				}
			} else {
				uNodeGUIUtility.DrawNullValue(uNodeGUIUtility.GetRect(), label, type, delegate (object o) {
					onChanged(fieldValue = o as string);
				});
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class MemberDataFieldControl : FieldControl<MemberData> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			var attributes = settings.attributes;
			EditorGUI.BeginChangeCheck();
			var fieldValue = value as MemberData;
			if(fieldValue == null) {
				if(settings.nullable) {
					if(value != null)
						GUI.changed = true;
					fieldValue = null;
				} else {
					fieldValue = MemberData.none;
					ObjectTypeAttribute OTA = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(attributes);
					FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(attributes);
					if(OTA != null && (filter == null || !filter.SetMember)) {
						fieldValue = MemberData.empty;
						if(OTA.type == typeof(string)) {
							fieldValue = new MemberData("");
						}
					} else {
						if(filter != null && !filter.SetMember) {
							fieldValue = MemberData.empty;
							if(filter.IsValidType(typeof(string))) {
								fieldValue = new MemberData("");
							}
						}
					}
					GUI.changed = true;
				}
			}
			if(fieldValue != null) {
				if(settings.nullable)
					position.width -= 16;
				FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(attributes);
				if(filter == null) {
					var OTA = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(attributes);
					if(OTA != null) {
						if(OTA.isElementType) {
							if(OTA.type != null) {
								filter = new FilterAttribute(OTA.type.ElementType());
							}
						} else {
							filter = new FilterAttribute(OTA.type);
						}
					}
				}
				EditorReflectionUtility.RenderVariable(position, fieldValue, label, settings?.unityObject, filter, (m) => {
					onChanged(m);
				});
				if(fieldValue.targetType == MemberData.TargetType.Values && fieldValue.type != null &&
					(fieldValue.type.IsArray || fieldValue.type.IsCastableTo(typeof(IList)))) {
					EditorGUI.indentLevel++;
					EditorReflectionUtility.DrawMemberValues(new GUIContent("Values"), fieldValue, fieldValue.type, filter, settings?.unityObject, (m) => {
						onChanged(m);
					});
					EditorGUI.indentLevel--;
				}
				if(settings.nullable) {
					position.x += position.width;
					position.width = 16;
					if(GUI.Button(position, GUIContent.none) && Event.current.button == 0) {
						fieldValue = null;
						GUI.changed = true;
					}
				}
			} else {
				uNodeGUIUtility.DrawNullValue(position, label, type, delegate (object o) {
					fieldValue = o as MemberData;
					onChanged(o);
				});
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}
	
	public class MultipurposeMemberFieldControl : FieldControl<MultipurposeMember> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			var attributes = settings.attributes;
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value, settings != null ? settings.nullable : false);
			var fieldValue = value as MultipurposeMember;
			if(fieldValue != null) {
				if(settings.nullable)
					position.width -= 16;
				if(settings.nullable) {
					Rect bRect = position;
					bRect.width -= 16;
					bRect.x += bRect.width;
					bRect.width = 16;
					if(GUI.Button(bRect, GUIContent.none, EditorStyles.miniButton) && Event.current.button == 0) {
						fieldValue = null;
						GUI.changed = true;
					}
					position.width -= 16;
				}
				FilterAttribute filter = ReflectionUtils.GetAttribute<FilterAttribute>(attributes);
				VariableEditorUtility.DrawMultipurposeMember(position, fieldValue, settings?.unityObject, label, filter);
			} else {
				uNodeGUIUtility.DrawNullValue(position, label, type, delegate (object o) {
					fieldValue = o as MultipurposeMember;
					onChanged(o);
				});
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}
	
	public class ParameterValueDataFieldControl : FieldControl<ParameterValueData> {
		public override bool IsValidControl(Type type, bool layouted) {
			return layouted && base.IsValidControl(type, layouted);
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value, false);
			var fieldValue = value as ParameterValueData;
			Type t = fieldValue.type;
			if(t != null) {
				if(t.IsValueType && fieldValue.value == null) {
					fieldValue.value = ReflectionUtils.CreateInstance(t);
					GUI.changed = true;
				}
				uNodeGUIUtility.EditValueLayouted(label, fieldValue.value, t, delegate (object val) {
					fieldValue.value = val;
					onChanged(fieldValue);
				}, new uNodeUtility.EditValueSettings() { nullable = true, unityObject = settings.unityObject });
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(value);
			}
		}
	}

	public class ValueDataFieldControl : FieldControl<ValueData> {
		public override bool IsValidControl(Type type, bool layouted) {
			return layouted && base.IsValidControl(type, layouted);
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value, false);
			var fieldValue = value as ValueData;
			Rect position = uNodeGUIUtility.GetRect();
			position = EditorGUI.PrefixLabel(position, label);
			string bLabel = fieldValue.ToString();
			if(EditorGUI.DropdownButton(position, fieldValue == null ? new GUIContent("null") : new GUIContent(bLabel, bLabel), FocusType.Keyboard) && Event.current.button == 0) {
				ReflectionUtils.TryCorrectingAttribute(settings.parentValue, ref settings.attributes);
				ObjectTypeAttribute drawer = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(settings.attributes);
				if(drawer != null) {
					if(drawer.type != null) {
						fieldValue.type = drawer.type;
					} else if(!string.IsNullOrEmpty(drawer.targetFieldPath) && settings.parentValue != null) {
						Type t = uNodeGUIUtility.GetFieldValueType(settings.parentValue, drawer.targetFieldPath);
						if(t != null) {
							fieldValue.type = t;
						}
					}
				}
				GenericMenu menu = new GenericMenu();
				if(fieldValue.type != null) {
					/*
					if(Value.type.IsPrimitive || IsSupportedType(Value.type)) {
						menu.AddItem(new GUIContent("Dirrect Edit Value"), false, delegate() {
							uNodeEditorUtility.RegisterUndo(unityObject, "");
							Value.value = new ObjectValueData() { value = ReflectionUtils.CreateInstance(Value.type) };
						});
					}*/
					ConstructorInfo[] ctors = fieldValue.type.GetConstructors();
					foreach(ConstructorInfo ctor in ctors) {
						ParameterInfo[] pInfo = ctor.GetParameters();
						if(pInfo.Length > 0) {
							bool valid = true;
							foreach(var p in pInfo) {
								//Ignore out and ref parameter.
								if(p.IsOut || p.ParameterType.IsByRef) {
									valid = false;
									break;
								}
							}
							if(!valid) {
								continue;
							}
						}
						menu.AddItem(new GUIContent(EditorReflectionUtility.GetOverloadingConstructorNames(ctor)), false, delegate (object obj) {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Value = new ConstructorValueData(obj as ConstructorInfo);
							onChanged(fieldValue);
						}, ctor);
					}
				}
				menu.ShowAsContext();
				GUI.changed = false;
			}
			if(fieldValue.Value != null) {
				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				uNodeGUIUtility.ShowField(type.GetField("value"), fieldValue, settings.unityObject);
				if(EditorGUI.EndChangeCheck()) {
					fieldValue.OnBeforeSerialize();
					onChanged(fieldValue);
				}
				EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class HLActionFieldControl : FieldControl<Events.HLAction> {
		public override bool IsValidControl(Type type, bool layouted) {
			return layouted && base.IsValidControl(type, layouted);
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = (Events.HLAction)value;
			Rect position = uNodeGUIUtility.GetRect();
			position = EditorGUI.PrefixLabel(position, label);
			Type instanceType = fieldValue.type.startType;
			if (instanceType != null) {
				EditorGUI.indentLevel++;
				var fields = EditorReflectionUtility.GetFields(instanceType);
				foreach(var field in fields) {
					if(field.IsDefined(typeof(NonSerializedAttribute)) || field.IsDefined(typeof(HideAttribute))) continue;
					var option = field.GetCustomAttribute(typeof(NodePortAttribute), true) as NodePortAttribute;
					if(option != null && option.hideInNode) continue;
					var val = fieldValue.initializers.FirstOrDefault(d => d.name == field.Name);
					if(val == null) {
						val = new FieldValueData() {
							name = field.Name,
							value = MemberData.CreateFromValue(
								field.GetValueOptimized(ReflectionUtils.CreateInstance(instanceType)), 
								field.FieldType),
						};
						fieldValue.initializers.Add(val);
						GUI.changed = true;
					}
					uNodeGUIUtility.EditValueLayouted(new GUIContent(field.Name), val.value, typeof(MemberData), (obj) => {
						val.value = obj as MemberData;
					}, new uNodeUtility.EditValueSettings() {
						attributes = new object[] { new FilterAttribute(field.FieldType) },
					});
				}
				if(instanceType.HasImplementInterface(typeof(IStateNode)) ||
					instanceType.HasImplementInterface(typeof(IStateCoroutineNode))) {
					uNodeGUIUtility.ShowField(
						fieldValue.GetType().GetField(nameof(Events.HLAction.storeResult)), 
						fieldValue, 
						settings.unityObject);
				}
				EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class HLConditionFieldControl : FieldControl<Events.HLCondition> {
		public override bool IsValidControl(Type type, bool layouted) {
			return layouted && base.IsValidControl(type, layouted);
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = (Events.HLCondition)value;
			Rect position = uNodeGUIUtility.GetRect();
			position = EditorGUI.PrefixLabel(position, label);
			Type instanceType = fieldValue.type.startType;
			if (instanceType != null) {
				EditorGUI.indentLevel++;
				var fields = EditorReflectionUtility.GetFields(instanceType);
				foreach(var field in fields) {
					if(field.IsDefined(typeof(NonSerializedAttribute)) || field.IsDefined(typeof(HideAttribute))) continue;
					var option = field.GetCustomAttribute(typeof(NodePortAttribute), true) as NodePortAttribute;
					if(option != null && option.hideInNode) continue;
					var val = fieldValue.initializers.FirstOrDefault(d => d.name == field.Name);
					if(val == null) {
						val = new FieldValueData() {
							name = field.Name,
							value = MemberData.CreateFromValue(
								field.GetValueOptimized(ReflectionUtils.CreateInstance(instanceType)), 
								field.FieldType),
						};
						fieldValue.initializers.Add(val);
						GUI.changed = true;
					}
					uNodeGUIUtility.EditValueLayouted(new GUIContent(field.Name), val.value, typeof(MemberData), (obj) => {
						val.value = obj as MemberData;
					}, new uNodeUtility.EditValueSettings() {
						attributes = new object[] { new FilterAttribute(field.FieldType) },
					});
				}
				EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class FieldModifierFieldControl : FieldControl<FieldModifier> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = value as FieldModifier;
			if(value != null) {
				position = EditorGUI.PrefixLabel(position, label);
				if(EditorGUI.DropdownButton(position, new GUIContent(fieldValue.GenerateCode()), FocusType.Keyboard)) {
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Public"), fieldValue.isPublic, () => {
						uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
						if(fieldValue.isPublic) {
							fieldValue.Public = false;
						} else {
							fieldValue.SetPublic();
						}
						onChanged(fieldValue);
					});
					menu.AddItem(new GUIContent("Private"), fieldValue.isPrivate && !fieldValue.Internal, () => {
						uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
						fieldValue.SetPrivate();
						onChanged(fieldValue);
					});
					bool flag = false;
					if(settings.unityObject != null) {
						GraphSystemAttribute graphSystem = null;
						if(settings.unityObject is uNodeRoot root) {
							graphSystem = GraphUtility.GetGraphSystem(root);
						} else if(settings.unityObject is INode<uNodeRoot> node) {
							graphSystem = GraphUtility.GetGraphSystem(node.GetOwner());
						}
						if(graphSystem != null) {
							flag = graphSystem.supportModifier;
						}
					}
					if (flag) {
						menu.AddItem(new GUIContent("Protected"), fieldValue.isProtected, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							if(fieldValue.isProtected) {
								fieldValue.Protected = false;
							} else {
								fieldValue.SetProtected();
							}
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Internal"), fieldValue.Internal, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							if(fieldValue.Internal) {
								fieldValue.Internal = false;
							} else {
								fieldValue.Public = false;
								fieldValue.Private = false;
								fieldValue.Internal = true;
							}
							onChanged(fieldValue);
						});
						menu.AddSeparator("");
						menu.AddItem(new GUIContent("Static"), fieldValue.Static, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Static = !fieldValue.Static;
							fieldValue.Const = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Event"), fieldValue.Event, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Event = !fieldValue.Event;
							fieldValue.Const = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("ReadOnly"), fieldValue.ReadOnly, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.ReadOnly = !fieldValue.ReadOnly;
							fieldValue.Const = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Const"), fieldValue.Const, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Const = !fieldValue.Const;
							fieldValue.Static = false;
							fieldValue.ReadOnly = false;
							fieldValue.Event = false;
							onChanged(fieldValue);
						});
					}
					menu.ShowAsContext();
				}
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class PropertyModifierFieldControl : FieldControl<PropertyModifier> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = value as PropertyModifier;
			if(value != null) {
				position = EditorGUI.PrefixLabel(position, label);
				if(EditorGUI.DropdownButton(position, new GUIContent(fieldValue.GenerateCode()), FocusType.Keyboard)) {
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Public"), fieldValue.isPublic, () => {
						uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
						if(fieldValue.isPublic) {
							fieldValue.Public = false;
						} else {
							fieldValue.SetPublic();
						}
						onChanged(fieldValue);
					});
					menu.AddItem(new GUIContent("Private"), fieldValue.isPrivate && !fieldValue.Internal, () => {
						uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
						fieldValue.SetPrivate();
						onChanged(fieldValue);
					});
					bool flag = false;
					if(settings.unityObject != null) {
						GraphSystemAttribute graphSystem = null;
						if(settings.unityObject is uNodeRoot root) {
							graphSystem = GraphUtility.GetGraphSystem(root);
						} else if(settings.unityObject is INode<uNodeRoot> node) {
							graphSystem = GraphUtility.GetGraphSystem(node.GetOwner());
						} else if(settings.unityObject is uNodeProperty prop) {
							graphSystem = GraphUtility.GetGraphSystem(prop.owner);
						}
						if(graphSystem != null) {
							flag = graphSystem.supportModifier;
						}
					}
					if (flag) {
						menu.AddItem(new GUIContent("Protected"), fieldValue.isProtected, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							if(fieldValue.isProtected) {
								fieldValue.Protected = false;
							} else {
								fieldValue.SetProtected();
							}
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Internal"), fieldValue.Internal, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							if(fieldValue.Internal) {
								fieldValue.Internal = false;
							} else {
								fieldValue.Public = false;
								fieldValue.Private = false;
								fieldValue.Internal = true;
							}
							onChanged(fieldValue);
						});
						menu.AddSeparator("");
						menu.AddItem(new GUIContent("Abstract"), fieldValue.Abstract, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Abstract = !fieldValue.Abstract;
							fieldValue.Static = false;
							fieldValue.Virtual = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Static"), fieldValue.Static, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Static = !fieldValue.Static;
							fieldValue.Abstract = false;
							fieldValue.Virtual = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Virtual"), fieldValue.Virtual, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Virtual = !fieldValue.Virtual;
							fieldValue.Static = false;
							fieldValue.Abstract = false;
							onChanged(fieldValue);
						});
					}
					menu.ShowAsContext();
				}
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}

	public class FunctionModifierFieldControl : FieldControl<FunctionModifier> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var fieldValue = value as FunctionModifier;
			if(value != null) {
				position = EditorGUI.PrefixLabel(position, label);
				if(EditorGUI.DropdownButton(position, new GUIContent(fieldValue.GenerateCode()), FocusType.Keyboard)) {
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Public"), fieldValue.isPublic, () => {
						uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
						if(fieldValue.isPublic) {
							fieldValue.Public = false;
						} else {
							fieldValue.SetPublic();
						}
						onChanged(fieldValue);
					});
					menu.AddItem(new GUIContent("Private"), fieldValue.isPrivate && !fieldValue.Internal, () => {
						uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
						fieldValue.SetPrivate();
						onChanged(fieldValue);
					});
					bool flag = false;
					if(settings.unityObject != null) {
						GraphSystemAttribute graphSystem = null;
						if(settings.unityObject is uNodeRoot root) {
							graphSystem = GraphUtility.GetGraphSystem(root);
						} else if(settings.unityObject is INode<uNodeRoot> node) {
							graphSystem = GraphUtility.GetGraphSystem(node.GetOwner());
						}
						if(graphSystem != null) {
							flag = graphSystem.supportModifier;
						}
					}
					if (flag) {
						menu.AddItem(new GUIContent("Protected"), fieldValue.isProtected, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							if(fieldValue.isProtected) {
								fieldValue.Protected = false;
							} else {
								fieldValue.SetProtected();
							}
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Internal"), fieldValue.Internal, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							if(fieldValue.Internal) {
								fieldValue.Internal = false;
							} else {
								fieldValue.Public = false;
								fieldValue.Private = false;
								fieldValue.Internal = true;
							}
							onChanged(fieldValue);
						});
						menu.AddSeparator("");
						menu.AddItem(new GUIContent("Static"), fieldValue.Static, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Abstract = false;
							fieldValue.Static = !fieldValue.Static;
							fieldValue.Virtual = false;
							fieldValue.Override = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Abstract"), fieldValue.Abstract, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Abstract = !fieldValue.Abstract;
							fieldValue.Static = false;
							fieldValue.Virtual = false;
							fieldValue.Override = false;
							fieldValue.New = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Override"), fieldValue.Override, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Abstract = false;
							fieldValue.Static = false;
							fieldValue.Virtual = false;
							fieldValue.Override = !fieldValue.Override;
							fieldValue.New = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("Virtual"), fieldValue.Virtual, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Abstract = false;
							fieldValue.Static = false;
							fieldValue.Virtual = !fieldValue.Virtual;
							fieldValue.Override = false;
							fieldValue.New = false;
							onChanged(fieldValue);
						});
						menu.AddItem(new GUIContent("New"), fieldValue.New, () => {
							uNodeEditorUtility.RegisterUndo(settings.unityObject, "");
							fieldValue.Abstract = false;
							fieldValue.Virtual = false;
							fieldValue.Override = false;
							fieldValue.New = !fieldValue.New;
							onChanged(fieldValue);
						});
					}
					menu.ShowAsContext();
				}
			}
			if (EditorGUI.EndChangeCheck()) {
				onChanged(fieldValue);
			}
		}
	}
}