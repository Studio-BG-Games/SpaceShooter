namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseDown", "OnMouseDown")]
	public class OnMouseDown : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseDown, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseDown, owner, Execute);
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
					"OnMouseDown",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
