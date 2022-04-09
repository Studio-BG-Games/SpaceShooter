using UnityEngine;
using UnityEngine.AI;

namespace MaxyGames.Runtime {
	/// <summary>
	/// A Utility for generated code to be more simple as possible.
	/// </summary>
	public static class RuntimeUtility {
		public static T DebugValue<T>(object instance, T value, int objectUID, int nodeUID, int valueID, bool isSet = false) {
#if UNITY_EDITOR
			uNodeDEBUG.InvokeValueNode(instance, objectUID, nodeUID, valueID, value, isSet);
#endif
			return value;
		}

		public static void DebugFlow(object instance, int objectUID, int nodeUID, bool? state) {
#if UNITY_EDITOR
			uNodeDEBUG.InvokeEventNode(instance, objectUID, nodeUID, state);
#endif
		}

		public static bool HasLayer(LayerMask mask, int layer) {
			return mask == (mask | (1 << layer));
		}

		public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask = -1) {
			Vector3 randomDirection = Random.insideUnitSphere * distance;
			randomDirection += origin;
			NavMeshHit navHit;
			NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);
			return navHit.position;
		}

		public static Vector3 RandomNavSphere(Vector3 origin, float minDistance, float maxDistance, int layermask = -1) {
			float distance = Random.Range(minDistance, maxDistance);
			Vector3 randomDirection = Random.insideUnitSphere * distance;
			randomDirection += origin;
			NavMeshHit navHit;
			NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);
			return navHit.position;
		}
	}
}
