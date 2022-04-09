namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("Custom", "Custom")]
	public class CustomTransition : TransitionEvent {
		public void Execute() {
			Finish();
		}

		public override string GenerateOnEnterCode() {
			if(GetTargetNode() == null)
				return null;
			string contents = CG.If(CG.CompareNodeState(node, null), CG.FlowFinish(this));
			CG.generatorData.InsertCustomUIDMethod("_ActivateTransition", typeof(void), Name + node.GetInstanceID().ToString(), contents);
			return null;
		}
	}
}
