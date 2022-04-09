namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnApplicationFocus", "OnApplicationFocus")]
	public class OnApplicationFocus : TransitionEvent {
		[Filter(typeof(bool), SetMember =true)]
		public MemberData storeValue = new MemberData();

		public override void OnEnter() {
			UEvent.Register<bool>(UEventID.OnApplicationFocus, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister<bool>(UEventID.OnApplicationFocus, owner, Execute);
		}

		void Execute(bool val) {
			if(storeValue.isAssigned) {
				storeValue.Set(val);
			}
			Finish();
		}

		public override string GenerateOnEnterCode() {
			if(GetTargetNode() == null)
				return null;
			if(!CG.HasInitialized(this)) {
				CG.SetInitialized(this);
				var mData = CG.generatorData.GetMethodData("OnApplicationFocus");
				if(mData == null) {
					mData = CG.generatorData.AddMethod(
						"OnApplicationFocus",
						CG.Type(typeof(void)),
						CG.Type(typeof(bool)));
				}
				string set = null;
				if(storeValue.isAssigned) {
					set = CG.Set(CG.Value((object)storeValue), mData.parameters[0].name).AddLineInEnd();
				}
				mData.AddCode(
					CG.Condition(
						"if",
						CG.CompareNodeState(node, null),
						set + CG.FlowFinish(this)
					)
				);
			}
			return null;
		}
	}
}
