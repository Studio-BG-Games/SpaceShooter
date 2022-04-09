using System;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	public class uNodeConstuctor : RootObject {
		public ConstructorModifier modifier = new ConstructorModifier();
		/// <summary>
		/// The summary of this Constructor.
		/// </summary>
		[TextArea]
		public string summary;

		public override bool CanHaveCoroutine() {
			return false;
		}

		public override Type ReturnType() {
			return typeof(void);
		}
	}
}