namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("Condition", "Condition")]
	public class ConditionTransition : TransitionEvent {
		[EventType(EventData.EventType.Condition)]
		public EventData condition = new EventData();

		public override void OnUpdate() {
			if(condition.Validate(owner)) {
				Finish();
			}
		}

		public override string GenerateOnUpdateCode() {
			if(GetTargetNode() == null)
				return null;
			return condition.GenerateConditionCode(this.node, CG.FlowFinish(this));
		}
	}
}
