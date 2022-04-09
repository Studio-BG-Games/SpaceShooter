using UnityEngine;

namespace MaxyGames.Events {
	//[EventMenu("UnityEngine/Debug/DrawLine", "DebugDrawLine")]
	public class DebugDrawLine : Action {
		[Tooltip("Point in world space where the line should start.")]
		[ObjectType(typeof(Vector3))]
		public MemberData start = MemberData.empty;
		[Tooltip("Point in world space where the line should end.")]
		[ObjectType(typeof(Vector3))]
		public MemberData end = MemberData.empty;
		[Tooltip("Color of the line.")]
		[ObjectType(typeof(Color))]
		public MemberData color = MemberData.empty;
		[Tooltip("How long the line should be visible for.")]
		[ObjectType(typeof(float))]
		public MemberData duration = MemberData.empty;

		protected override void OnExecute() {
			Debug.DrawLine(start.Get<Vector3>(), end.Get<Vector3>(), color.Get<Color>(), duration.Get<float>());
		}

		public override string GenerateCode(Object obj) {
			return CG.FlowInvoke(typeof(Debug), "DrawLine", start.CGValue(), end.CGValue(), color.CGValue(), duration.CGValue());
		}

		public override string GetDescription() {
			return "Draws a line between specified start and end points.";
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(start, owner, Name + " - start");
			uNode.uNodeUtility.CheckError(end, owner, Name + " - end");
			uNode.uNodeUtility.CheckError(color, owner, Name + " - color");
			uNode.uNodeUtility.CheckError(duration, owner, Name + " - duration");
		}
	}
}