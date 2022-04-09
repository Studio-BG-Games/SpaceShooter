using UnityEngine;
using MaxyGames.Serializer;

namespace MaxyGames.uNode {
	[System.Serializable]
	public class uNodeTemplate : ScriptableObject {
		public string path;

		[Hide]
		public SerializedData serializedData;
	}
}