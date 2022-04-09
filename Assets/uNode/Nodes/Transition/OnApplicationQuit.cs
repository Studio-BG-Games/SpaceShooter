namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnApplicationQuit", "OnApplicationQuit")]
	public class OnApplicationQuit : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnApplicationQuit, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnApplicationQuit, owner, Execute);
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
					"OnApplicationQuit",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
