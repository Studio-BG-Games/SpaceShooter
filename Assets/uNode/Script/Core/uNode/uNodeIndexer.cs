using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	public class uNodeIndexer : uNodeComponent {
		public Node setIntialize;
		public Node getIntialize;
		public Transform setRoot;
		public Transform getRoot;

		public AttributeData[] attributes;
		public IndexerModifier modifiers;
		public ParameterData[] parameters;
	}
}