namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnMouseDrag", "OnMouseDrag")]
	public class OnMouseDrag : TransitionEvent {

		public override void OnEnter() {
			UEvent.Register(UEventID.OnMouseDrag, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister(UEventID.OnMouseDrag, owner, Execute);
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
					"OnMouseDrag",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node, null), CG.FlowFinish(this)));
			}
			return null;
		}
	}
}
