using UnityEngine;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Data", "GetComponent")]
	[AddComponentMenu("")]
	public class GetComponentNode : ValueNode {
		[Filter(typeof(Component), OnlyGetType = true, ArrayManipulator = false, AllowInterface =true)]
		public MemberData type = new MemberData(typeof(Component), MemberData.TargetType.Type);
		[Hide, ValueIn("Value"), Filter(typeof(Component), typeof(GameObject))]
		public MemberData target;
		public GetComponentKind getComponentKind;
		[Hide(nameof(getComponentKind), GetComponentKind.GetComponent)]
		public bool includeInactive;

		public enum GetComponentKind {
			GetComponent,
			GetComponentInChildren,
			GetComponentInParent,
		}

		public override System.Type ReturnType() {
			if(type.isAssigned) {
				try {
					System.Type t = type.Get<System.Type>();
					if(!object.ReferenceEquals(t, null)) {
						return t;
					}
				}
				catch { }
			}
			return typeof(object);
		}

		protected override object Value() {
			var value = target.Get();
			System.Type t = type.Get<System.Type>();
			if (value != null) {
				if(value.GetType() == t) return value;
				if(t.IsInterface || t.IsCastableTo(typeof(Component))) {
					if(value is GameObject gameObject) {
						if(t is RuntimeType) {
							switch(getComponentKind) {
								case GetComponentKind.GetComponent:
									return gameObject.GetGeneratedComponent(t as RuntimeType);
								case GetComponentKind.GetComponentInChildren:
									return gameObject.GetGeneratedComponentInChildren(t as RuntimeType, includeInactive);
								case GetComponentKind.GetComponentInParent:
									return gameObject.GetGeneratedComponentInParent(t as RuntimeType, includeInactive);
							}
						}
						return gameObject.GetComponent(t);
					} else if(value is Component component) {
						if(t is RuntimeType) {
							switch(getComponentKind) {
								case GetComponentKind.GetComponent:
									return component.GetGeneratedComponent(t as RuntimeType);
								case GetComponentKind.GetComponentInChildren:
									return component.GetGeneratedComponentInChildren(t as RuntimeType, includeInactive);
								case GetComponentKind.GetComponentInParent:
									return component.GetGeneratedComponentInParent(t as RuntimeType, includeInactive);
							}
						}
						return component.GetComponent(t);
					}
				} else {
					throw new System.InvalidOperationException("The type is not supported to use GetComponent: " + t.FullName);
				}
			}
			return value;
		}

		public override string GenerateValueCode() {
			if(target.isAssigned && type.isAssigned) {
				System.Type t = type.startType;
				if(t != null) {
					if(t.IsInterface || t.IsCastableTo(typeof(Component))) {
						if(t is RuntimeType runtimeType) {
							CG.RegisterUsingNamespace("MaxyGames");//Register namespace to make sure Extensions work for GameObject or Component target type.
							if(CG.generatePureScript) {
								switch(getComponentKind) {
									case GetComponentKind.GetComponent:
										return CG.Value(target).CGInvoke(
											nameof(uNodeHelper.GetGeneratedComponent), 
											new[] { t }, 
											null
										);
									case GetComponentKind.GetComponentInChildren:
										return CG.Value(target).CGInvoke(
											nameof(uNodeHelper.GetGeneratedComponentInChildren), 
											new[] { t }, 
											new[] { includeInactive.CGValue() }
										);
									case GetComponentKind.GetComponentInParent:
										return CG.Value(target).CGInvoke(
											nameof(uNodeHelper.GetGeneratedComponentInChildren), 
											new[] { t }, 
											new[] {includeInactive.CGValue() }
										);
								}
							} else {
								switch(getComponentKind) {
									case GetComponentKind.GetComponent:
										return CG.Value(target).CGInvoke(
											nameof(uNodeHelper.GetGeneratedComponent), 
											new[] { CG.GetUniqueNameForComponent(runtimeType)
										});
									case GetComponentKind.GetComponentInChildren:
										return CG.Value(target).CGInvoke(
											nameof(uNodeHelper.GetGeneratedComponentInChildren), 
											new[] { CG.GetUniqueNameForComponent(runtimeType), includeInactive.CGValue() } 
										);
									case GetComponentKind.GetComponentInParent:
										return CG.Value(target).CGInvoke(
											nameof(uNodeHelper.GetGeneratedComponentInChildren), 
											new[] { CG.GetUniqueNameForComponent(runtimeType), includeInactive.CGValue()
										});
								}
							}
						} else {
							switch(getComponentKind) {
								case GetComponentKind.GetComponent:
									return CG.Value(target).CGInvoke(nameof(Component.GetComponent), new[] { t }, null);
								case GetComponentKind.GetComponentInChildren:
									return CG.Value(target).CGInvoke(nameof(Component.GetComponentInChildren), new[] { t }, new[] { includeInactive.CGValue() });
								case GetComponentKind.GetComponentInParent:
									return CG.Value(target).CGInvoke(nameof(Component.GetComponentInParent), new[] { t }, new[] { includeInactive.CGValue() });
							}
						}
					} else {
						throw new System.InvalidOperationException("The type is not supported to use GetComponent: " + t.FullName);
					}
				}
			}
			throw new System.Exception();
		}

		public override string GetNodeName() {
			return getComponentKind.ToString();
		}

		public override string GetRichName() {
			return $"({type.GetNicelyDisplayName(richName:true, typeTargetWithTypeof:false)})" + target.GetNicelyDisplayName(richName:true);
		}

		public override void CheckError() {
			uNodeUtility.CheckError(type, this, nameof(type));
			uNodeUtility.CheckError(target, this, nameof(target));
			if(type.isAssigned && target.isAssigned) {
				System.Type t = type.startType;
				System.Type targetType = target.type;
				if(t != null && targetType != null) {
					if (t.IsCastableTo(typeof(Component)) || t.IsInterface) {
						bool valid = false;
						if (targetType.IsCastableTo(typeof(Component)) || targetType == typeof(GameObject)) {
							valid = true;
						}
						if(!valid) {
							RegisterEditorError($"The target type:{targetType.PrettyName()} is not castable to type:{t.PrettyName()}");
						}
					} else {
						RegisterEditorError($"The type must targeting 'UnityEngine.Component' or a interface");
					}
				}
			}
		}
	}
}