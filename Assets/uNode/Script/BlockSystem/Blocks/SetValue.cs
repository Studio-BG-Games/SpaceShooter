using UnityEngine;
using MaxyGames.uNode;
using System;
using Object = UnityEngine.Object;

namespace MaxyGames.Events {
	[BlockMenu("★General", "SetValue")]
	public class SetValue : Action {
		public SetType setType = SetType.Change;
		[Filter(SetMember = true)]
		public MemberData target = new MemberData();
		[ObjectType("target")]
		public MemberData value = new MemberData();

		protected override void OnExecute() {
			System.Type type = target.type;
			if(type != null) {
				object obj = value.Get();
				switch(setType) {
					case SetType.Change:
						target.Set(obj);
						break;
					case SetType.Add:
						if(obj is Delegate) {
							object targetVal = target.Get();
							if(targetVal is MemberData.Event) {
								MemberData.Event e = targetVal as MemberData.Event;
								if(e != null) {
									e.eventInfo.AddEventHandler(e.instance, ReflectionUtils.ConvertDelegate(obj as Delegate, e.eventInfo.EventHandlerType));
								}
							} else {
								target.Set(Operator.Add(targetVal, obj));
							}
						} else {
							target.Set(Operator.Add(target.Get(), obj));
						}
						break;
					case SetType.Divide:
						target.Set(Operator.Divide(target.Get(), obj));
						break;
					case SetType.Subtract:
						if(obj is Delegate) {
							object targetVal = target.Get();
							if(targetVal is MemberData.Event) {
								MemberData.Event e = targetVal as MemberData.Event;
								if(e.eventInfo != null) {
									e.eventInfo.RemoveEventHandler(e.instance, ReflectionUtils.ConvertDelegate(obj as Delegate, e.eventInfo.EventHandlerType));
								}
							} else {
								target.Set(Operator.Add(targetVal, obj));
							}
						} else {
							target.Set(Operator.Subtract(target.Get(), obj));
						}
						break;
					case SetType.Multiply:
						target.Set(Operator.Multiply(target.Get(), obj));
						break;
					case SetType.Modulo:
						target.Set(Operator.Modulo(target.Get(), obj));
						break;
				}
			}
		}

		public override string GenerateCode(Object obj) {
			if(target.isAssigned) {
				System.Type type = target.type;
				if(type != null) {
					return CG.Set(target, value, setType, target.type, value.type);
				}
			}
			throw new System.Exception("Target is unassigned");
		}

		public override string Name {
			get {
				if(setType != SetType.Change) {
				return setType.ToString() + ": <b>" +
					uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b> to <b>" +
					uNode.uNodeUtility.GetNicelyDisplayName(value) + "</b>";
				}
				return "Set: <b>" + uNode.uNodeUtility.GetNicelyDisplayName(target) + "</b> to <b>" + uNode.uNodeUtility.GetNicelyDisplayName(value) + "</b>";
			}
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNodeUtility.CheckError(target, owner, Name + " - target");
			uNodeUtility.CheckError(value, owner, Name + " - value");
			if(target.isAssigned && value.isAssigned && target.type != null) {
				try {
					bool isDelegate = target.type.IsSubclassOf(typeof(Delegate));
					if(isDelegate) {
						if(setType != SetType.Add && setType != SetType.Subtract) {
							if(target.targetType == MemberData.TargetType.Event) {
								uNodeUtility.RegisterEditorError(owner, "Event can only be Add or Remove");
							} else if(target.targetType == MemberData.TargetType.ValueNode && target.instance is MultipurposeNode) {
								var node = target.instance as MultipurposeNode;
								if(node.target.target.targetType == MemberData.TargetType.Event) {
									uNodeUtility.RegisterEditorError(owner, "Event can only be Add or Remove");
								}
							}
						}
					} else if(value.type != null) {
						if(setType == SetType.Change) {
							if(!value.type.IsCastableTo(typeof(uNodeRoot)) && 
								!value.type.IsCastableTo(target.type) && 
								!target.type.IsCastableTo(value.type) && 
								value.type != typeof(object)) {
								uNodeUtility.RegisterEditorError(owner, value.type.PrettyName(true) + " can't be set to " + target.type.PrettyName(true));
							}
						} else if(ReflectionUtils.CanCreateInstance(target.type) && ReflectionUtils.CanCreateInstance(value.type)) {
							uNodeHelper.SetObject(
								ReflectionUtils.CreateInstance(target.type),
								ReflectionUtils.CreateInstance(value.type), setType);
						}
					}
				} catch(System.Exception ex) {
					uNodeUtility.RegisterEditorError(owner, ex.Message);
				}
			}
		}
	}
}