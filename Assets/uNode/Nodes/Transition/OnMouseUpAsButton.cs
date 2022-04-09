namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseUpAsButton", "OnMouseUpAsButton")]
	public class OnMouseUpAsButton : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseUpAsButton, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseUpAsButton, owner, Execute);
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
					"OnMouseUpAsButton",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
