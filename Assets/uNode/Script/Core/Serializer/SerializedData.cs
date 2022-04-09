using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MaxyGames.Serializer {
	[System.Serializable]
	public class SerializedData : ISerializationCallbackReceiver {
		public List<Object> objects = new List<Object>();
		public Dictionary<int, SerializedObject> serializedObjects = new Dictionary<int, SerializedObject>();

		[SerializeField]
		private List<int> keys = new List<int>();
		[SerializeField]
		private List<SerializedObject> values = new List<SerializedObject>();
		//Cached
		[System.NonSerialized]
		public Dictionary<int, Object> deserializedObjects;

		public void OnAfterDeserialize() {
			Event e = Event.current;
			if(e != null && e.type != UnityEngine.EventType.Used &&
				(e.type == UnityEngine.EventType.Repaint ||
				e.type == UnityEngine.EventType.MouseDrag ||
				e.type == UnityEngine.EventType.Layout ||
				e.type == UnityEngine.EventType.ScrollWheel)) {
				return;
			}
			serializedObjects.Clear();
			if(keys != null && values != null && keys.Count == values.Count) {
				for(int i = 0; i < keys.Count; i++) {
					serializedObjects.Add(keys[i], values[i]);
				}
			}
		}

		public void OnBeforeSerialize() {
			keys.Clear();
			values.Clear();
			foreach(var pair in serializedObjects) {
				keys.Add(pair.Key);
				values.Add(pair.Value);
			}
		}
	}

	[System.Serializable]
	public class SerializedGraph {
		public MemberData graphType;
		public uNode.VariableData[] variables;
		public SerializedData serializedGraph;
		public SerializedData nestedTypes;

		public GameObject ToObject(bool includeNestedGraph = true) {
			GameObject gameObject = new GameObject("Graph");
			return ToObject(gameObject, includeNestedGraph);
		}

		public GameObject ToObject(GameObject gameObject, bool includeNestedGraph = true) {
			Serializer.Deserialize(serializedGraph, gameObject);
			if(includeNestedGraph && nestedTypes != null) {
				GameObject nestedGraph = new GameObject("NestedGraph");
				Serializer.Deserialize(nestedTypes, nestedGraph);
			}
			return gameObject;
		}
	}
}