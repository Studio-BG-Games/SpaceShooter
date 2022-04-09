using System.Collections.Generic;
using UnityEngine;
using MaxyGames.Serializer;

namespace MaxyGames.uNode {
	public class GraphAsset : ScriptableObject {
		[HideInInspector]
		public SerializedGraph[] serializedGraph;
		[HideInInspector]
		public SerializedData serializedGraphData;
	}
}