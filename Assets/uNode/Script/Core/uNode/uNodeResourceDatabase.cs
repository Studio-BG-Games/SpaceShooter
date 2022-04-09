using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	public class uNodeResourceDatabase : ScriptableObject {
		[System.Serializable]
		public class RuntimeGraphDatabase {
			public string uniqueID => graph is IClassIdentifier ? (graph as IClassIdentifier).uniqueIdentifier : graph.GraphName;
			public uNodeRoot graph;
		}
		public List<RuntimeGraphDatabase> graphDatabases = new List<RuntimeGraphDatabase>();

		private Dictionary<string, RuntimeGraphDatabase> graphDBMap = new Dictionary<string, RuntimeGraphDatabase>();
		public RuntimeGraphDatabase GetGraphDatabase(string graphUID) {
			RuntimeGraphDatabase data;
			if(!graphDBMap.TryGetValue(graphUID, out data)) {
				foreach(var db in graphDatabases) {
					if(db.graph?.GeneratedTypeName == graphUID) {
						data = db;
						graphDBMap[graphUID] = data;
						break;
					}
				}
			}
			return data;
		}

		public RuntimeGraphDatabase GetGraphDatabase<T>() where T : class {
			return GetGraphDatabase(typeof(T).FullName);
		}

		public void ClearCache() {
			graphDBMap.Clear();
		}
	}
}