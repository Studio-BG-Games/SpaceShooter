namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseUp", "OnMouseUp")]
	public class OnMouseUp : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseUp, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseUp, owner, Execute);
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
					"OnMouseUp",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
