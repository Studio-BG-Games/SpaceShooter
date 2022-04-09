namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseOver", "OnMouseOver")]
	public class OnMouseOver : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseOver, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseOver, owner, Execute);
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
					"OnMouseOver",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
