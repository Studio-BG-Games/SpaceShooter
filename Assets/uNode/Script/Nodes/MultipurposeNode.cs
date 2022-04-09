using System;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	public class MultipurposeNode : ValueNode {
		[Hide, FlowOut("", true, hideOnNotFlowNode = true)]
		public MemberData onFinished = new MemberData();

		[Hide, ValueIn]
		public MultipurposeMember target = new MultipurposeMember();

		public override void OnExecute() {
			Value();
			Finish(onFinished);
		}

		public override System.Type ReturnType() {
			if(target != null && target.target != null && target.target.isAssigned) {
				switch(target.target.targetType) {
					case MemberData.TargetType.Type:
					case MemberData.TargetType.uNodeGenericParameter:
						return typeof(System.Type);
					default:
						if(target.target.isAssigned) {
							return target.target.type;
						}
						break;
				}
			}
			return typeof(object);
		}

		protected override object Value() {
			if(target != null && target.target != null) {
				if(target.target.isAssigned) {
					return target.Get();
				}
			}
			return null;
		}

		public override bool IsFlowNode() {
			if(target.target.isTargeted) {
				if(target.target.targetType == MemberData.TargetType.Method ||
					target.target.targetType == MemberData.TargetType.uNodeFunction ||
					target.target.targetType == MemberData.TargetType.uNodeConstructor)
					return true;
				if(target.target.targetType == MemberData.TargetType.Constructor) {
					var members = target.target.GetMembers(false);
					if(members != null) {
						var lastMember = members[members.Length - 1];
						if(lastMember is System.Reflection.ConstructorInfo ctor) {
							return !ctor.DeclaringType.IsArray;
						}
					}
					return true;
				}
				if(target.target.targetType == MemberData.TargetType.uNodeVariable ||
					target.target.targetType == MemberData.TargetType.uNodeLocalVariable ||
					target.target.targetType == MemberData.TargetType.uNodeProperty ||
					target.target.targetType == MemberData.TargetType.uNodeParameter ||
					target.target.targetType == MemberData.TargetType.uNodeGroupVariable) {
					if(target.target.isDeepTarget) {
						try {
							var members = target.target.GetMembers(false);
							if (members != null) {
								var lastMember = members[members.Length - 1];
								if (lastMember != null) {
									return lastMember is System.Reflection.MethodInfo || lastMember is System.Reflection.ConstructorInfo;
								}
							} else {
								return true;
							}
						} catch {
							return true;
						}
					}
				}
			}
			return false;
		}

		public override void SetValue(object value) {
			if(CanSetValue()) {
				target.target.Set(value);
				return;
			}
			throw new Exception($"Cannot Set Value for: {target.target.DisplayName(true)}");
		}

		public override bool CanSetValue() {
			if(target != null && target.target != null && target.target.isAssigned && target.target.CanSetValue()) {
				return true;
			}
			return base.CanSetValue();
		}

		public override bool CanGetValue() {
			if(target.target.isAssigned) {
				return target.target.targetType != MemberData.TargetType.Event && base.CanGetValue();
			}
			return base.CanGetValue();
		}

		public override string GenerateValueCode() {
			return CG.Value(target, null, true);
		}

		public override string GenerateCode() {
			return CG.Value(target).AddSemicolon() + CG.FlowFinish(this, true, false, false, onFinished).AddLineInFirst();
		}

		public override string GetNodeName() {
			if(target != null && target.target != null) {
				if(target.target.targetType == MemberData.TargetType.Values) {
					return uNodeUtility.GetDisplayName(target.target.type);
				}
				return uNodeUtility.GetDisplayName(target.target, uNodeUtility.preferredDisplay, true);
			}
			return base.GetNodeName();
		}

		public override string GetRichName() {
			if(target != null && target.target != null) {
				return uNodeUtility.GetNicelyDisplayName(target, richName:true);
			}
			return base.GetNodeName();
		}

		public override Type GetNodeIcon() {
			if(target != null && target.target != null) {
				switch(target.target.targetType) {
					case MemberData.TargetType.Null:
						return typeof(TypeIcons.NullTypeIcon);
					case MemberData.TargetType.Method:
					case MemberData.TargetType.Field:
					case MemberData.TargetType.Property:
					case MemberData.TargetType.Event:
						return target.target.startType;
					case MemberData.TargetType.Values:
						return base.GetNodeIcon();
				}
				if(target.target.isDeepTarget) {
					return target.target.startType;
				} else if(target.target.IsTargetingUNode) {
					return base.GetNodeIcon();
				}
			}
			return typeof(void);
		}

		public override bool IsCoroutine() {
			return HasCoroutineInFlow(onFinished);
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}

		public override string ToString() {
			return GetNodeName() + "\n" + base.ToString();
		}

#if UNITY_EDITOR
		void Awake() {
			//For easier logging.
			target.owner = this;
		}
#endif
	}
}