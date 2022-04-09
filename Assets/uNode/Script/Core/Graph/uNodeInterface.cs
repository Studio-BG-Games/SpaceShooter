using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	public class uNodeInterface : ScriptableObject, IInterfaceSystem, ICustomIcon, IIndependentGraph {
		/// <summary>
		/// The summary of interface.
		/// </summary>
		[TextArea]
		public string summary;
        /// <summary>
        /// The interface icon
        /// </summary>
		public Texture2D icon;
        /// <summary>
        /// The namespace of the interface
        /// </summary>
		public string @namespace;
		[HideInInspector]
		public List<string> usingNamespaces = new List<string>() { "UnityEngine", "System.Collections.Generic" };
		[HideInInspector]
		public InterfaceModifier modifiers = new InterfaceModifier();
		[HideInInspector]
		public InterfaceFunction[] functions = new InterfaceFunction[0];
		[HideInInspector]
		public InterfaceProperty[] properties = new InterfaceProperty[0];

		[HideInInspector, SerializeField]
		private MemberData[] interfaces = new MemberData[0];

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

		string IIndependentGraph.Namespace => @namespace;
		List<string> IIndependentGraph.UsingNamespaces { get => usingNamespaces; set => usingNamespaces = value; }

		public Texture GetIcon() {
			return icon;
		}
	}
}