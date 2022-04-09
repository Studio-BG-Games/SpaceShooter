using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MaxyGames.uNode {
	/// <summary>
	/// Class for generating C# struct.
	/// </summary>
	[AddComponentMenu("")]
	[GraphSystem("C# Struct", supportGeneric = true, generationKind = GenerationKind.Compatibility)]
	public class uNodeStruct : uNodeBase, IInterfaceSystem, IClassModifier {
		/// <summary>
		/// The modifier of this class/struct
		/// </summary>
		public ClassModifier modifier = new ClassModifier() { Public = true };
		/// <summary>
		/// List of interface this class/struct implements.
		/// </summary>
		[HideInInspector]
		public MemberData[] interfaces = new MemberData[0];

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

		public override Type GetInheritType() {
			return typeof(object);
		}

		public override bool IsStruct {
			get {
				return true;
			}
		}

		ClassModifier IClassModifier.GetModifier() {
			return modifier;
		}
	}
}