using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MaxyGames.uNode {
	/// <summary>
	/// Class for generating C# classes.
	/// </summary>
	[AddComponentMenu("")]
	[GraphSystem("C# Class", supportGeneric = true, generationKind = GenerationKind.Compatibility)]
	public class uNodeClass : uNodeBase, IInterfaceSystem, IClassModifier, IStateGraph { 
		/// <summary>
		/// The modifier of this class/struct
		/// </summary>
		public ClassModifier modifier = new ClassModifier() { Public = true };
		[Filter(OnlyGetType = true, DisplaySealedType = false,
			DisplayValueType = false, DisplayInterfaceType = false,
			UnityReference = false, ArrayManipulator = false,
			DisplayRuntimeType = false)]
		public MemberData inheritFrom = new MemberData(typeof(object), MemberData.TargetType.Type);
		/// <summary>
		/// List of interface this class/struct implements.
		/// </summary>
		[HideInInspector]
		public MemberData[] interfaces = new MemberData[0];

		[HideInInspector, SerializeField]
		private BaseGraphEvent[] events = new BaseGraphEvent[0];

		public IList<MemberData> Interfaces {
			get {
				return interfaces;
			}
			set {
				if(value is MemberData[]) {
					interfaces = value as MemberData[];
					return;
				}
				interfaces = value.ToArray();
			}
		}

		IList<BaseGraphEvent> IStateGraph.eventNodes => events;
		bool IStateGraph.canCreateGraph => GetInheritType().IsCastableTo(typeof(MonoBehaviour));

		/// <summary>
		/// Get the inherith type.
		/// </summary>
		/// <returns></returns>
		public override Type GetInheritType() {
			if(inheritFrom != null && inheritFrom.isAssigned) {
				return inheritFrom.Get<Type>() ?? typeof(object);
			}
			return typeof(object);
		}

		ClassModifier IClassModifier.GetModifier() {
			return modifier;
		}

		public override void Refresh() {
			base.Refresh();
			if(RootObject == null)
				return;
			events = RootObject.GetComponentsInChildren<BaseGraphEvent>(true);
			var eventNodes = RootObject.GetComponentsInChildren<BaseEventNode>(true);
			var TRevents = RootObject.GetComponentsInChildren<TransitionEvent>(true);
			foreach(var transition in TRevents) {
				if(transition.owner != this) {
					transition.owner = this;
				}
			}
			foreach(var m in eventNodes) {
				if(m.owner != this) {
					m.owner = this;
				}
			}
		}
	}
}