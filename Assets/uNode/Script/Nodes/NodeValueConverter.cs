using System;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class NodeValueConverter : ValueNode {
		public SerializedType type = new SerializedType(typeof(object));
		[Hide]
		public MemberData target = MemberData.none;

		public enum ConvertKind {
			As,
			Convert,
		}
		public ConvertKind kind;

		public override Type ReturnType() {
			return type.type;
		}

		protected override object Value() {
			var value = target.Get();
			Type t = type.type;
			if(value != null) {
				if(value.GetType() == t)
					return value;
				if(t == typeof(string)) {
					return value.ToString();
				} else if(t == typeof(GameObject)) {
					if(value is Component component) {
						return component.gameObject;
					}
				} else if(t.IsCastableTo(typeof(Component))) {
					if(value is GameObject gameObject) {
						if(t is RuntimeType) {
							return gameObject.GetGeneratedComponent(t as RuntimeType);
						}
						return gameObject.GetComponent(t);
					} else if(value is Component component) {
						if(t is RuntimeType) {
							return component.GetGeneratedComponent(t as RuntimeType);
						}
						return component.GetComponent(t);
					}
				}
			}
			if(kind == ConvertKind.Convert || t is RuntimeType || t.IsValueType) {
				value = Operator.Convert(value, t);
			} else {
				value = Operator.TypeAs(value, t);
			}
			return value;
		}

		public override bool CanGetValue() {
			return true;
		}

		public override bool CanSetValue() {
			return false;
		}

		public override string GenerateValueCode() {
			if(target.isAssigned && type.type != null) {
				System.Type t = type.type;
				System.Type targetType = target.type;
				if(t != null && targetType != null) {
					if(!targetType.IsCastableTo(t) && !t.IsCastableTo(targetType)) {
						if(t == typeof(string)) {
							return CG.Value(target).CGInvoke(nameof(object.ToString));
						} else if(t == typeof(GameObject)) {
							if(targetType.IsCastableTo(typeof(Component))) {
								return CG.Value(target).CGAccess(nameof(Component.gameObject));
							}
						} else if(t.IsCastableTo(typeof(Component))) {
							if(targetType.IsCastableTo(typeof(Component)) || targetType == typeof(GameObject)) {
								if(t == typeof(Transform)) {
									return CG.Value(target).CGAccess(nameof(Component.transform));
								} else {
									return CG.Value(target).CGInvoke(nameof(Component.GetComponent), new System.Type[] { t }, null);
								}
							}
						}
					}
				} else if(t == null) {
					return CG.Convert(target.CGValue(), CG.Type(type.type));
				}
				if(kind == ConvertKind.Convert || t.IsValueType) {
					return CG.Convert(target, t);
				}
				return CG.As(target, t);
			}
			throw new System.Exception();
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.RefreshIcon);
		}

		public override string GetNodeName() {
			if(kind == ConvertKind.As && type.type != null && !type.type.IsValueType) {
				return "AS";
			}
			return "Convert";
		}

		public override string GetRichName() {
			return $"({type.typeName})" + target.GetNicelyDisplayName(richName: true);
		}

		public override void CheckError() {
			uNodeUtility.CheckError(target, this, nameof(target));
			if(type.type != null && target.isAssigned) {
				Type t = type.type;
				Type targetType = target.type;
				if(t != null && targetType != null) {
					if(!targetType.IsCastableTo(t, true) && !t.IsCastableTo(targetType)) {
						bool valid = false;
						if(t == typeof(string)) {
							valid = true;
						} else if(t == typeof(GameObject)) {
							if(targetType.IsCastableTo(typeof(Component))) {
								valid = true;
							}
						} else if(t.IsCastableTo(typeof(Component))) {
							if(targetType.IsCastableTo(typeof(Component)) || targetType == typeof(GameObject)) {
								valid = true;
							}
						} else if(t.IsEnum && targetType.IsPrimitive) {
							valid = true;
						}
						if(!valid) {
							RegisterEditorError($"The target type:{targetType.PrettyName()} is not castable to type:{t.PrettyName()}");
						}
					}
				}
			}
		}
	}
}