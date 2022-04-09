using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames {
	public static class ComponentUtility {
		/// <summary>
		/// Get component of type T only in children of transfrom.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="transfrom"></param>
		/// <returns></returns>
		public static T GetComponentInChild<T>(Transform transfrom) where T : Component {
			foreach(Transform t in transfrom) {
				T comp = t.GetComponent<T>();
				if(comp != null) {
					return comp;
				}
			}
			return null;
		}

		/// <summary>
		/// Get components of type T only in children of transfrom.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="transfrom"></param>
		/// <returns></returns>
		public static List<T> GetComponentsInChild<T>(Transform transfrom) where T : Component {
			List<T> list = new List<T>();
			foreach(Transform t in transfrom) {
				T[] comp = t.GetComponents<T>();
				list.AddRange(comp);
			}
			return list;
		}
	}
}