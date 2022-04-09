using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors {
	#region Property Drawer
	[CustomPropertyDrawer(typeof(MultipurposeMember))]
	class MultipurposeMemberDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (fieldInfo.IsDefined(typeof(TooltipAttribute), true)) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true)[0]).tooltip;
			}
			EditorGUI.BeginProperty(position, label, property);
			VariableEditorUtility.DrawMultipurposeMember(position, property, label);
			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(FieldModifier))]
	class FieldModifierDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (fieldInfo.IsDefined(typeof(TooltipAttribute), true)) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true)[0]).tooltip;
			}
			var value = PropertyDrawerUtility.GetActualObjectForSerializedProperty<FieldModifier>(property);
			EditorGUI.BeginProperty(position, label, property);
			uNodeGUIUtility.EditValue(position, label, value, null, new uNodeUtility.EditValueSettings() {
				unityObject = property.serializedObject.targetObject,
			});
			VariableEditorUtility.DrawMultipurposeMember(position, property, label);
			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(PropertyModifier))]
	class PropertyModifierDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (fieldInfo.IsDefined(typeof(TooltipAttribute), true)) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true)[0]).tooltip;
			}
			var value = PropertyDrawerUtility.GetActualObjectForSerializedProperty<PropertyModifier>(property);
			EditorGUI.BeginProperty(position, label, property);
			uNodeGUIUtility.EditValue(position, label, value, null, new uNodeUtility.EditValueSettings() {
				unityObject = property.serializedObject.targetObject,
			});
			VariableEditorUtility.DrawMultipurposeMember(position, property, label);
			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(FunctionModifier))]
	class FunctionModifierDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (fieldInfo.IsDefined(typeof(TooltipAttribute), true)) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true)[0]).tooltip;
			}
			var value = PropertyDrawerUtility.GetActualObjectForSerializedProperty<FunctionModifier>(property);
			EditorGUI.BeginProperty(position, label, property);
			uNodeGUIUtility.EditValue(position, label, value, null, new uNodeUtility.EditValueSettings() {
				unityObject = property.serializedObject.targetObject,
			});
			VariableEditorUtility.DrawMultipurposeMember(position, property, label);
			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(MemberData))]
	class MemberDataDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if (fieldInfo.IsDefined(typeof(ObjectTypeAttribute), true)) {
				var fieldAttributes = fieldInfo.GetCustomAttributes(true);
				object variable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
				if (variable != null && ReflectionUtils.TryCorrectingAttribute(variable, ref fieldAttributes)) {
					var OTA = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(fieldAttributes);
					if (OTA != null && OTA.type != null) {
						return base.GetPropertyHeight(property, label);
					}
				}
				return 0;
			}
			return base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			MemberData variable = PropertyDrawerUtility.GetActualObjectForSerializedProperty<MemberData>(property);
			FilterAttribute filter = null;
			if (fieldInfo.IsDefined(typeof(ObjectTypeAttribute), true)) {
				object pVariable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
				var fieldAttributes = fieldInfo.GetCustomAttributes(true);
				if (pVariable != null && ReflectionUtils.TryCorrectingAttribute(pVariable, ref fieldAttributes)) {
					filter = ReflectionUtils.GetAttribute<FilterAttribute>(fieldAttributes);
				} else
					return;
			} else if (fieldInfo.GetCustomAttributes(typeof(FilterAttribute), false).Length > 0) {
				filter = (FilterAttribute)fieldInfo.GetCustomAttributes(typeof(FilterAttribute), false)[0];
			}
			if (fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false).Length > 0) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false)[0]).tooltip;
			}
			EditorReflectionUtility.RenderVariable(position, variable, label, property.serializedObject.targetObject, filter);
		}
	}

	[CustomPropertyDrawer(typeof(SerializedType))]
	class SerializedTypeDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			if(fieldInfo.IsDefined(typeof(ObjectTypeAttribute), true)) {
				var fieldAttributes = fieldInfo.GetCustomAttributes(true);
				object variable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
				if(variable != null && ReflectionUtils.TryCorrectingAttribute(variable, ref fieldAttributes)) {
					var OTA = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(fieldAttributes);
					if(OTA != null && OTA.type != null) {
						return base.GetPropertyHeight(property, label);
					}
				}
				return 0;
			}
			return base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			SerializedType variable = PropertyDrawerUtility.GetActualObjectForSerializedProperty<SerializedType>(property);
			FilterAttribute filter = null;
			if(fieldInfo.IsDefined(typeof(ObjectTypeAttribute), true)) {
				object pVariable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
				var fieldAttributes = fieldInfo.GetCustomAttributes(true);
				if(pVariable != null && ReflectionUtils.TryCorrectingAttribute(pVariable, ref fieldAttributes)) {
					filter = ReflectionUtils.GetAttribute<FilterAttribute>(fieldAttributes);
				} else
					return;
			} else if(fieldInfo.GetCustomAttributes(typeof(FilterAttribute), false).Length > 0) {
				filter = (FilterAttribute)fieldInfo.GetCustomAttributes(typeof(FilterAttribute), false)[0];
			} else {
				filter = new FilterAttribute(typeof(object));
			}
			filter.OnlyGetType = true;
			if(fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false).Length > 0) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false)[0]).tooltip;
			}
			uNodeGUIUtility.DrawTypeDrawer(position, variable, null, label, (t) => {
				variable.type = t;
			}, filter);
		}
	}

	[CustomPropertyDrawer(typeof(HideAttribute))]
	class HideAttributeDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			object variable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
			if (variable != null) {
				if (uNodeGUIUtility.IsHide(fieldInfo, variable))
					return -EditorGUIUtility.standardVerticalSpacing;
			} else {
				if (fieldInfo.IsDefined(typeof(HideAttribute), true)) {
					HideAttribute[] hide = fieldInfo.GetCustomAttributes(typeof(HideAttribute), true) as HideAttribute[];
					foreach (HideAttribute ha in hide) {
						if (string.IsNullOrEmpty(ha.targetField)) {
							return -EditorGUIUtility.standardVerticalSpacing;
						}
					}
				}
			}
			System.Type type = fieldInfo.FieldType;
			if (fieldInfo.FieldType.IsArray) {
				type = fieldInfo.FieldType.GetElementType();
			} else if (fieldInfo.FieldType.IsGenericType) {
				System.Type[] gType = fieldInfo.FieldType.GetGenericArguments();
				if (gType.Length == 1) {
					type = gType[0];
				}
			}
			if (type == typeof(MemberData)) {
				if (fieldInfo.IsDefined(typeof(ObjectTypeAttribute), true)) {
					var fieldAttributes = fieldInfo.GetCustomAttributes(true);
					if (variable != null && ReflectionUtils.TryCorrectingAttribute(variable, ref fieldAttributes)) {
						var OTA = ReflectionUtils.GetAttribute<ObjectTypeAttribute>(fieldAttributes);
						if (OTA != null && OTA.type != null) {
							return EditorGUI.GetPropertyHeight(property, label, true);
						}
					}
					return -EditorGUIUtility.standardVerticalSpacing;
				}
			}
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			object variable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
			if (variable != null) {
				if (uNodeGUIUtility.IsHide(fieldInfo, variable))
					return;
			} else {
				if (fieldInfo.IsDefined(typeof(HideAttribute), true)) {
					HideAttribute[] hide = fieldInfo.GetCustomAttributes(typeof(HideAttribute), true) as HideAttribute[];
					foreach (HideAttribute ha in hide) {
						if (string.IsNullOrEmpty(ha.targetField)) {
							return;
						}
					}
				}
			}
			if (fieldInfo.IsDefined(typeof(TooltipAttribute), true)) {
				label.tooltip = ((TooltipAttribute)fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true)[0]).tooltip;
			}
			System.Type type = fieldInfo.FieldType;
			if (fieldInfo.FieldType.IsArray) {
				type = fieldInfo.FieldType.GetElementType();
			} else if (fieldInfo.FieldType.IsGenericType) {
				System.Type[] gType = fieldInfo.FieldType.GetGenericArguments();
				if (gType.Length == 1) {
					type = gType[0];
				}
			}
			if (type == typeof(MemberData)) {
				MemberData obj = PropertyDrawerUtility.GetActualObjectForSerializedProperty<MemberData>(property);
				FilterAttribute filter = null;
				if (fieldInfo.GetCustomAttributes(typeof(FilterAttribute), false).Length > 0) {
					filter = (FilterAttribute)fieldInfo.GetCustomAttributes(typeof(FilterAttribute), false)[0];
				} else if (fieldInfo.IsDefined(typeof(ObjectTypeAttribute), true)) {
					var fieldAttributes = fieldInfo.GetCustomAttributes(true);
					object pVariable = PropertyDrawerUtility.GetParentObjectFromSerializedProperty<object>(property);
					if (pVariable != null && ReflectionUtils.TryCorrectingAttribute(pVariable, ref fieldAttributes)) {
						filter = ReflectionUtils.GetAttribute<FilterAttribute>(fieldAttributes);
					} else
						return;
				}
				EditorReflectionUtility.RenderVariable(position, obj, label, property.serializedObject.targetObject, filter);
			} else {
				EditorGUI.PropertyField(position, property, label, true);
			}
		}
	}
	#endregion

	[CustomEditor(typeof(uNodeResourceDatabase), true)]
	class uNodeResourceDatabaseEditor : Editor {
		public override void OnInspectorGUI() {
			var comp = target as uNodeResourceDatabase;
			DrawDefaultInspector();
			if(GUILayout.Button(new GUIContent("Update Database", ""))) {
				var graphs = uNodeEditorUtility.FindComponentInPrefabs<uNodeRoot>();
				foreach(var root in graphs) {
					if(comp.graphDatabases.Any(g => g.graph == root)) {
						continue;
					}
					comp.graphDatabases.Add(new uNodeResourceDatabase.RuntimeGraphDatabase() {
						graph = root,
					});
					EditorUtility.SetDirty(comp);
				}
			}
		}
	}

	[CustomEditor(typeof(uNodeData), true)]
	class uNodeDataEditor : Editor {
		public override void OnInspectorGUI() {
			uNodeData comp = target as uNodeData;
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			EditorGUI.BeginDisabledGroup(uNodeEditorUtility.IsPrefab(comp));
			VariableEditorUtility.DrawNamespace("Using Namespaces", comp.generatorSettings.usingNamespace.ToList(), comp, (arr) => {
				comp.generatorSettings.usingNamespace = arr.ToArray();
				uNodeEditorUtility.MarkDirty(comp);
			});
			EditorGUI.EndDisabledGroup();
			if (EditorGUI.EndChangeCheck()) {
				uNodeGUIUtility.GUIChanged(comp);
			}
			if(comp.GetComponent<uNodeRoot>() == null) {
				if (GUILayout.Button(new GUIContent("Open uNode Editor", "Open uNode Editor to edit this uNode"), EditorStyles.toolbarButton)) {
					uNodeEditor.Open(comp);
				}
			}
		}
	}

	[CustomEditor(typeof(GraphAsset), true)]
	class GraphAssetEditor : Editor {
		public override void OnInspectorGUI() {
			GraphAsset asset = target as GraphAsset;
			DrawDefaultInspector();
			EditorGUILayout.HelpBox("The GraphAsset is not supported anymore", MessageType.Warning);
		}
	}

	[CustomEditor(typeof(uNodeInterface), true)]
	class GraphInterfaceEditor : Editor {
		public override void OnInspectorGUI() {
			uNodeInterface asset = target as uNodeInterface;
			DrawDefaultInspector();
			EditorGUI.BeginChangeCheck();
			VariableEditorUtility.DrawNamespace("Using Namespaces", asset.usingNamespaces, asset, (ns) => {
				asset.usingNamespaces = ns as List<string> ?? ns.ToList();
			});
			VariableEditorUtility.DrawInterfaceFunction(asset.functions, asset, (val) => {
				asset.functions = val.ToArray();
			});
			VariableEditorUtility.DrawInterfaceProperty(asset.properties, asset, (val) => {
				asset.properties = val.ToArray();
			});
			if(EditorGUI.EndChangeCheck()) {
				var runtimeType = ReflectionUtils.GetRuntimeType(asset) as RuntimeGraphInterface;
				if(runtimeType != null) {
					runtimeType.RebuildMembers();
				}
			}
		}
	}

	[CustomEditor(typeof(uNodeProperty), true)]
	class uNodePropertyEditor : Editor {
		public override void OnInspectorGUI() {
			uNodeProperty property = target as uNodeProperty;
			DrawDefaultInspector();
			if(property.CanGetValue()) {
				uNodeGUIUtility.ShowField(nameof(property.getterModifier), property, property);
			}
			if(property.CanSetValue()) {
				uNodeGUIUtility.ShowField(nameof(property.setterModifier), property, property);
			}
		}
	}

	#region Nodes
	[CustomEditor(typeof(NodeGroup), true)]
	class NodeGroupEditor : Editor {
		public override void OnInspectorGUI() {
			NodeGroup comp = target as NodeGroup;
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			if (GUILayout.Button(new GUIContent("Open Variable Editor", "Open Variable Editor to edit this variable"), EditorStyles.toolbarButton)) {
				VariableEditorWindow VEW = VariableEditorWindow.ShowWindow(comp, comp.variable);
				VEW.autoInitializeSupportedType = false;
			}
			uNodeGUIUtility.ShowField("comment", comp, comp);
			if (EditorGUI.EndChangeCheck()) {
				uNodeGUIUtility.GUIChanged(comp);
			}
		}
	}
#endregion
}