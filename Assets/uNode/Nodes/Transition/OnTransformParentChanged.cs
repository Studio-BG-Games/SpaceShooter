namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnTransformParentChanged", "OnTransformParentChanged")]
	public class OnTransformParentChanged : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnTransformParentChanged, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnTransformParentChanged, owner, Execute);
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
					"OnTransformParentChanged",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
