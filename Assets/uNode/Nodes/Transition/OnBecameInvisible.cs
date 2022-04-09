namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnBecameInvisible", "OnBecameInvisible")]
	public class OnBecameInvisible : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnBecameInvisible, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnBecameInvisible, owner, Execute);
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
					"OnBecameInvisible",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
