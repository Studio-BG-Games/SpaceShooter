using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine/Transform", "Translate")]
	public class TransfromTranslate : Action {
		[ObjectType(typeof(Transform))]
		public MemberData transform = MemberData.empty;
		[ObjectType(typeof(Vector3))]
		public MemberData translation = new MemberData(Vector3.forward);
		[ObjectType(typeof(float))]
		public MemberData speed = new MemberData(1f);

		protected override void OnExecute() {
			if(transform != null) {
				transform.Get<Transform>().Translate(translation.Get<Vector3>() * speed.Get<float>() * Time.deltaTime);
			}
		}

		public override string GenerateCode(Object obj) {
			if(transform.isAssigned && translation.isAssigned) {
				return CG.FlowInvoke(transform, "Translate",
					CG.Value((object)translation) + " * " +
					CG.Value((object)speed) + " * " +
					CG.Type(typeof(Time)) + ".deltaTime");
			}
			throw new System.Exception("transform or translation is unassigned");
		}
	}
}