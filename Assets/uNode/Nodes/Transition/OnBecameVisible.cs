namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnBecameVisible", "OnBecameVisible")]
	public class OnBecameVisible : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnBecameVisible, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnBecameVisible, owner, Execute);
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
					"OnBecameVisible",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
