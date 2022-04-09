namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseExit", "OnMouseExit")]
	public class OnMouseExit : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseExit, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseExit, owner, Execute);
		}

		void Execute() {
			Finish();
		}
		

		public override string GenerateOnEnterCode() {
			if(GetTargetNode() == null)
				return null;
			if(!CG.HasInitialized(this)) {
				CG.SetInitialized(this);
				CG.InsertCodeToFunction(
					"OnMouseExit",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
