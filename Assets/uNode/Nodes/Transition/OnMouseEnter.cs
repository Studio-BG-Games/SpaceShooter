namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseEnter", "OnMouseEnter")]
	public class OnMouseEnter : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseEnter, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseEnter, owner, Execute);
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
					"OnMouseEnter",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
