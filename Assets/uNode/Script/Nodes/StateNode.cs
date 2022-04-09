using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	//[NodeMenu("Flow", "State", IsCoroutine = true, order = 1, HideOnFlow = true)]
	[AddComponentMenu("")]
	public class StateNode : Node, ISuperNode {
		[SerializeField, Hide]
		private GameObject transitionEventObject;

		public GameObject TransitionEventObject {
			get {
				return transitionEventObject;
			}
			set {
				this.transitionEventObject = value;
			}
		}

		public TransitionEvent[] GetTransitions() {
			if(transitionEventObject != null) {
				return transitionEventObject.GetComponents<TransitionEvent>();
			}
			return new TransitionEvent[0];
		}

		[System.NonSerialized]
		private TransitionEvent[] _TransitionEvents;
		public TransitionEvent[] TransitionEvents {
			get {
				if(_TransitionEvents == null) {
					_TransitionEvents = new TransitionEvent[0];
				}
				if(_TransitionEvents.Length == 0 && transitionEventObject != null) {
					_TransitionEvents = transitionEventObject.GetComponents<TransitionEvent>();
				}
				return _TransitionEvents;
			}
		}

		public IList<NodeComponent> nestedFlowNodes {
			get {
				List<NodeComponent> nodes = new List<NodeComponent>();
				foreach(Transform t in transform) {
					var comp = t.GetComponent<StateEventNode>();
					if(comp) {
						nodes.Add(comp);
					}
				}
				return nodes;
			}
		}

		public event System.Action onEnter;
		public event System.Action onExit;

		public override void OnExecute() {
			if(onEnter != null) {
				onEnter();
			}
			if(transitionEventObject != null && jumpState == null) {
				for(int i = 0; i < TransitionEvents.Length; i++) {
					TransitionEvent transition = TransitionEvents[i];
					if(transition) {
						transition.OnEnter();
					}
				}
			}
			owner.StartCoroutine(OnUpdate(), this);
		}

		private System.Collections.IEnumerator OnUpdate() {
			while(state == StateType.Running) {
				if(transitionEventObject != null && jumpState == null) {
					for(int i = 0; i < TransitionEvents.Length; i++) {
						TransitionEvent transition = TransitionEvents[i];
						if(transition) {
							transition.OnUpdate();
							if(state != StateType.Running) {
								yield break;
							}
						}
					}
				}
				yield return null;
			}
		}

		public override void Finish() {
			if(onExit != null) {
				onExit();
			}
			if(transitionEventObject != null) {
				for(int i = 0; i < TransitionEvents.Length; i++) {
					TransitionEvent transition = TransitionEvents[i];
					if(transition) {
						transition.OnExit();
					}
				}
			}
			if(state == StateType.Running) {
				state = StateType.Success;
			}
			base.Finish();
		}

		public override void Stop() {
			//Check if this node is still running
			if(state == StateType.Running) {
				if(onExit != null) {
					onExit();
				}
				if(transitionEventObject != null) {
					for(int i = 0; i < TransitionEvents.Length; i++) {
						TransitionEvent transition = TransitionEvents[i];
						if(transition) {
							transition.OnExit();
						}
					}
				}
			}
			base.Stop();
		}

		public override bool IsSelfCoroutine() {
			return true;
		}

		public override void OnGeneratorInitialize() {
			//Register this node as state node.
			CG.RegisterAsStateNode(this);
			var transitions = GetTransitions();
			for(int i = 0; i < transitions.Length; i++) {
				TransitionEvent transition = transitions[i];
				transition.OnGeneratorInitialize();
			}
			CG.SetStateInitialization(this, () => CG.GenerateNode(this));
		}

		public override string GenerateCode() {
			string onEnter = null;
			string onUpdate = null;
			string onExit = null;
			var transitions = GetTransitions();
			for(int i = 0; i < transitions.Length; i++) {
				TransitionEvent transition = transitions[i];
				if(transition) {
					onEnter += transition.GenerateOnEnterCode().Add("\n", !string.IsNullOrEmpty(onEnter));
					onUpdate += transition.GenerateOnUpdateCode().AddLineInFirst();
					onExit += transition.GenerateOnExitCode().AddLineInFirst();
				}
			}
			foreach(Transform t in transform) {
				var comp = t.GetComponent<StateEventNode>();
				if(comp) {
					if(comp.eventType == StateEventNode.EventType.OnEnter) {
						onEnter += CG.GenerateNode(comp).AddLineInFirst().Replace("yield ", "");
					} else if(comp.eventType == StateEventNode.EventType.OnExit) {
						onExit += CG.GenerateNode(comp).AddLineInFirst();
					} else {
						CG.SetStateInitialization(comp, CG.Routine(CG.Lambda(CG.GenerateNode(comp).Replace("yield ", ""))));
					}
				}
			}
			//onEnter += CG.Condition("while", CG.CompareNodeState(this, null), onUpdate.AddLineInEnd() + CG.GetYieldReturn(null).AddLineInFirst()).AddLineInFirst();
			CG.SetStateStopAction(this, onExit);
			return CG.Routine(
				CG.Routine(CG.Lambda(onEnter)),
				CG.Invoke(typeof(Runtime.Routine), nameof(Runtime.Routine.WaitWhile), CG.Lambda(onUpdate.AddLineInEnd() + CG.Return(CG.CompareNodeState(this, null))))
			);
		}

		public override string GetNodeName() {
			return gameObject.name;
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.StateIcon);
		}

		public bool AcceptCoroutine() {
			return true;
		}
	}
}