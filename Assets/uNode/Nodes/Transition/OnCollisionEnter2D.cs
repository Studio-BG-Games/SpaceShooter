namespace MaxyGames.uNode.Transition {
	[UnityEngine.AddComponentMenu("")]
	[TransitionMenu("OnCollisionEnter2D", "OnCollisionEnter2D")]
	public class OnCollisionEnter2D : TransitionEvent {
		[Filter(typeof(UnityEngine.Collision2D), SetMember = true)]
		public MemberData storeCollision = new MemberData();

		public override void OnEnter() {
			UEvent.Register<UnityEngine.Collision2D>(UEventID.OnCollisionEnter2D, owner, Execute);
		}

		public override void OnExit() {
			UEvent.Unregister<UnityEngine.Collision2D>(UEventID.OnCollisionEnter2D, owner, Execute);
		}


		void Execute(UnityEngine.Collision2D collision) {
			if(storeCollision.isAssigned) {
				storeCollision.Set(collision);
			}
			Finish();
		}

		public override string GenerateOnEnterCode() {
			if(GetTargetNode() == null)
				return null;
			if(!CG.HasInitialized(this)) {
				CG.SetInitialized(this);
				var mData = CG.generatorData.GetMethodData("OnCollisionEnter2D");
				if(mData == null) {
					mData = CG.generatorData.AddMethod(
						"OnCollisionEnter2D",
						CG.Type(typeof(void)),
						CG.Type(typeof(UnityEngine.Collision2D)));
				}
				string set = null;
				if(storeCollision.isAssigned) {
					set = CG.Set(CG.Value((object)storeCollision), mData.parameters[0].name).AddLineInEnd();
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
