using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace MaxyGames.Serializer {
	#region Classes
	[System.Serializable]
	public class SerializedObject {
		public int owner;
		public string type;
        public string name;
        public OdinSerializedData data;
	}
	#endregion

	/// <summary>
	/// Provides Usefull function to Serialize GameObject hierarchy.
	/// </summary>
	public static class Serializer {

		public static uNode.uNodeRoot DeserializeGraph(SerializedGraph graph, IList<uNode.VariableData> variables = null, bool includeNestedGraph = true) {
			return DeserializeGraph(graph, new GameObject("Graph"), variables, includeNestedGraph);
		}

		public static uNode.uNodeRoot DeserializeGraph(SerializedGraph graph, GameObject gameObject, IList<uNode.VariableData> variables = null, bool includeNestedGraph = true) {
			var originalComp = gameObject.GetComponents<uNode.uNodeRoot>();
			Deserialize(graph.serializedGraph, gameObject);
			var components = gameObject.GetComponents<uNode.uNodeRoot>();
			uNode.uNodeRoot addedRoot = null;
			foreach(var comp in components) {
				if(!originalComp.Contains(comp)) {
					addedRoot = comp;
					break;
				}
			}
			if(includeNestedGraph && graph.nestedTypes != null && graph.nestedTypes.serializedObjects.Count > 0) {
				GameObject nestedGraph = new GameObject("NestedGraph");
				nestedGraph.transform.SetParent(gameObject.transform);
				Deserialize(graph.nestedTypes, nestedGraph);
				if(addedRoot as uNode.uNodeBase)
					(addedRoot as uNode.uNodeBase).NestedClass = nestedGraph.GetComponent<uNode.uNodeData>();
			}
			if(variables != null && addedRoot != null) {
				//Initialize variable values.
				foreach(var v in addedRoot.Variables) {
					foreach(var lv in variables) {
						if(v.Name == lv.Name && v.Type == lv.Type) {
							v.value = lv.value;
							break;
						}
					}
				}
			}
			return addedRoot;
		}

		public static SerializedGraph SerializeGraph(uNode.uNodeRoot root) {
			SerializedGraph graph = new SerializedGraph();
			List<Transform> transforms = new List<Transform>();
			{
				if(root.RootObject != null) {
					var children = root.RootObject.GetComponentsInChildren<Transform>(true);
					transforms.AddRange(children);
				}
				graph.graphType = new MemberData(root.GetType(), MemberData.TargetType.Type);
			}
			var graphRoot = Serialize(root.gameObject, new Component[] { root }, transforms);
			if(root as uNode.uNodeBase && (root as uNode.uNodeBase).NestedClass) {//Handle nested types.
				transforms = new List<Transform>();
				List<Component> roots = new List<Component>();
				var comps = (root as uNode.uNodeBase).NestedClass.GetComponents<Component>();
				foreach(var c in comps) {
					if(c is uNode.uNodeRoot || c is uNode.uNodeData || !(c is MonoBehaviour)) {
						roots.Add(c);
					}
				}
				foreach(var r in roots) {
					if(r is uNode.uNodeRoot) {
						var nodeRoot = r as uNode.uNodeRoot;
						if(nodeRoot.RootObject != null) {
							var children = nodeRoot.RootObject.GetComponentsInChildren<Transform>(true);
							transforms.AddRange(children);
						}
					}
				}
				var nestedTypes = Serialize((root as uNode.uNodeBase).NestedClass.gameObject, roots, transforms);
				graph.nestedTypes = nestedTypes;
			}
			{
				graph.serializedGraph = graphRoot;
				graph.variables = root.Variables.ToArray();
			}
			return graph;
		}

		/// <summary>
		/// Serialize graph (uNode) and its nodes.
		/// </summary>
		/// <param name="graphObject"></param>
		/// <returns></returns>
		public static SerializedData SerializeGraph(GameObject graphObject) {
			List<Component> roots = new List<Component>();
			var comps = graphObject.GetComponents<Component>();
			foreach(var c in comps) {
				if(c is uNode.uNodeRoot || c is uNode.uNodeData || !(c is MonoBehaviour)) {
					roots.Add(c);
				}
			}
			List<Transform> transforms = new List<Transform>();
			foreach(var r in roots) {
				if(r is uNode.uNodeRoot) {
					var root = r as uNode.uNodeRoot;
					if(root.RootObject != null) {
						var children = root.RootObject.GetComponentsInChildren<Transform>(true);
						transforms.AddRange(children);
					}
				}
			}
			return Serialize(graphObject, roots, transforms);
		}

		/// <summary>
		/// Serialize root including its children.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static SerializedData Serialize(GameObject root) {
			SerializedData serializedData = new SerializedData();
			var r = SerializeObject(root);
			r.owner = 0;
			serializedData.serializedObjects.Add(root.GetInstanceID(), r);
			serializedData.objects.Add(root);
			RecursiveRegisterObject(root.transform, serializedData);
			var objs = serializedData.objects;
			serializedData.objects = new List<Object>();
			foreach(var obj in objs) {
				if(obj is GameObject || !(obj is MonoBehaviour))
					continue;
				var json = Serialize(obj, serializedData);
				if(serializedData.serializedObjects.ContainsKey(obj.GetInstanceID())) {
					serializedData.serializedObjects[obj.GetInstanceID()].data = json;
				}
			}
			return serializedData;
		}

		/// <summary>
		/// Serialize root with defines children.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="children"></param>
		/// <returns></returns>
		public static SerializedData Serialize(GameObject root, IList<GameObject> children) {
			SerializedData serializedData = new SerializedData();
			var r = SerializeObject(root);
			r.owner = 0;
			serializedData.serializedObjects.Add(root.GetInstanceID(), r);
			serializedData.objects.Add(root);
			RegisterScriptObject(root, serializedData);
			if(children != null) {
				foreach(var child in children) {
					RegisterObject(child, serializedData);
				}
			}
			var objs = serializedData.objects;
			serializedData.objects = new List<Object>();
			foreach(var obj in objs) {
				if(obj is GameObject || !(obj is MonoBehaviour))
					continue;
				var json = Serialize(obj, serializedData);
				if(serializedData.serializedObjects.ContainsKey(obj.GetInstanceID())) {
					serializedData.serializedObjects[obj.GetInstanceID()].data = json;
				}
			}
			return serializedData;
		}

		/// <summary>
		/// Serialize root with defines children.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="children"></param>
		/// <returns></returns>
		public static SerializedData Serialize(GameObject root, IList<Transform> children) {
			SerializedData serializedData = new SerializedData();
			var r = SerializeObject(root);
			r.owner = 0;
			serializedData.serializedObjects.Add(root.GetInstanceID(), r);
			serializedData.objects.Add(root);
			RegisterScriptObject(root, serializedData);
			if(children != null) {
				foreach(var child in children) {
					RegisterObject(child.gameObject, serializedData);
				}
			}
			var objs = serializedData.objects;
			serializedData.objects = new List<Object>();
			foreach(var obj in objs) {
				if(obj is GameObject || !(obj is MonoBehaviour))
					continue;
				var json = Serialize(obj, serializedData);
				if(serializedData.serializedObjects.ContainsKey(obj.GetInstanceID())) {
					serializedData.serializedObjects[obj.GetInstanceID()].data = json;
				}
			}
			return serializedData;
		}

		/// <summary>
		/// Serialize MonoBehaviour.
		/// </summary>
		/// <param name="monoBehaviour"></param>
		/// <returns></returns>
		public static SerializedData Serialize(MonoBehaviour monoBehaviour) {
			return Serialize(monoBehaviour.gameObject, new Component[] { monoBehaviour }, null);
		}

		/// <summary>
		/// Serialize root with defines children.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="children"></param>
		/// <returns></returns>
		public static SerializedData Serialize(GameObject root, IList<Component> rootComponents, IList<Transform> children) {
			SerializedData serializedData = new SerializedData();
			var r = SerializeObject(root);
			r.owner = 0;
			serializedData.serializedObjects.Add(root.GetInstanceID(), r);
			serializedData.objects.Add(root);
			if(rootComponents != null) {
				foreach(var comp in rootComponents) {
					RegisterObject(comp, serializedData);
				}
			}
			if(children != null) {
				foreach(var child in children) {
					RegisterObject(child.gameObject, serializedData);
				}
			}
			var objs = serializedData.objects;
			serializedData.objects = new List<Object>();
			foreach(var obj in objs) {
				if(obj is GameObject || !(obj is MonoBehaviour))
					continue;
				var data = Serialize(obj, serializedData);
				if(serializedData.serializedObjects.ContainsKey(obj.GetInstanceID())) {
					serializedData.serializedObjects[obj.GetInstanceID()].data = data;
				}
			}
			return serializedData;
		}

		public static void RegisterObject(GameObject gameObject, SerializedData serializedData) {
			if(!serializedData.serializedObjects.ContainsKey(gameObject.GetInstanceID())) {
				serializedData.serializedObjects.Add(gameObject.GetInstanceID(), SerializeObject(gameObject));
				serializedData.objects.Add(gameObject);
			}
			RegisterScriptObject(gameObject, serializedData);
		}

		public static void RegisterObject(Component component, SerializedData serializedData) {
			if(serializedData.serializedObjects.ContainsKey(component.GetInstanceID()))
				return;
			serializedData.serializedObjects.Add(component.GetInstanceID(), SerializeObject(component));
			serializedData.objects.Add(component);
		}

		public static void RegisterScriptObject(GameObject gameObject, SerializedData serializedData) {
			MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
			foreach(var script in scripts) {
				RegisterObject(script, serializedData);
			}
		}

		public static void RecursiveRegisterObject(Transform transform, SerializedData serializedData) {
			RegisterObject(transform.gameObject, serializedData);
			foreach(Transform child in transform) {
				RecursiveRegisterObject(child, serializedData);
			}
		}

		public static SerializedObject SerializeObject(GameObject gameObject) {
			SerializedObject serializedObject = new SerializedObject();
			serializedObject.type = gameObject.GetType().FullName;
			serializedObject.name = gameObject.name;
			if(gameObject.transform.parent != null) {
				serializedObject.owner = gameObject.transform.parent.gameObject.GetInstanceID();
			}
			return serializedObject;
		}

		public static SerializedObject SerializeObject(Component component) {
			SerializedObject serializedObject = new SerializedObject();
			serializedObject.type = component.GetType().FullName;
			serializedObject.owner = component.gameObject.GetInstanceID();
			return serializedObject;
		}

		public static OdinSerializedData Serialize(object obj, SerializedData serializedData) {
            var result = SerializerUtility.SerializeValue(obj);
            return result;
        }

		public static void Deserialize(SerializedData serializedData, GameObject root = null) {
			serializedData.deserializedObjects = new Dictionary<int, Object>();
			//Creating GameObject first
			List<KeyValuePair<GameObject, SerializedObject>> gameObjects = new List<KeyValuePair<GameObject, SerializedObject>>();
			foreach(var pair in serializedData.serializedObjects) {
				System.Type type = pair.Value.type.ToType();
				if(type == typeof(GameObject)) {
					GameObject go;
					if(root != null && pair.Value.owner == 0) {
						go = root;
					} else {
						go = new GameObject(pair.Value.name);
					}
					gameObjects.Add(new KeyValuePair<GameObject, SerializedObject>(go, pair.Value));
					serializedData.deserializedObjects.Add(pair.Key, go);
				}
			}
			//Arrage GameObject
			foreach(var pair in gameObjects) {
				int id = pair.Value.owner;
				if(id != 0) {
					pair.Key.transform.SetParent((serializedData.deserializedObjects[id] as GameObject).transform);
				}
			}
			List<KeyValuePair<Component, string>> components = new List<KeyValuePair<Component, string>>();
			//Add Component
			foreach(var pair in serializedData.serializedObjects) {
				System.Type type = pair.Value.type.ToType();
				if(!type.IsSubclassOf(typeof(Component)))
					continue;
				GameObject owner = serializedData.deserializedObjects[pair.Value.owner] as GameObject;
				if(type.IsSubclassOf(typeof(MonoBehaviour))) {
					Component comp = owner.AddComponent(type);
					components.Add(new KeyValuePair<Component, string>(comp, pair.Value.name));
					serializedData.deserializedObjects.Add(pair.Key, comp);
				} else {
					Component comp = owner.GetComponent(type);
					if(comp == null) {
						//Debug.LogError("The component type of " + type.PrettyName(true) + " is not found in GameObject : " + owner.name, owner);
						//return;
						continue;
					}
					components.Add(new KeyValuePair<Component, string>(comp, pair.Value.name));
					serializedData.deserializedObjects.Add(pair.Key, comp);
				}
			}
			//Apply Data
			foreach(var pair in components) {
				Component comp = pair.Key;
				Deserialize(pair.Value, ref comp, serializedData);
			}
		}

		public static void Deserialize<T>(string json, ref T instance, SerializedData serializedData) {
			try {
                throw null;
            } catch(System.Exception ex) {
				Debug.LogException(ex);
			}
		}
	}

	//public class GameObjectData {
	//	public string name;
	//	public string tag;
	//	public bool isStatic;
	//}

	//public class TransformData {
	//	public Vector3 position;
	//	public Vector3 rotation;
	//	public Vector3 scale;
	//}
}