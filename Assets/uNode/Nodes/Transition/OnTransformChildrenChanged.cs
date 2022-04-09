namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnTransformChildrenChanged", "OnTransformChildrenChanged")]
	public class OnTransformChildrenChanged : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnTransformChildrenChanged, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnTransformChildrenChanged, owner, Execute);
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
					"OnTransformChildrenChanged",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
