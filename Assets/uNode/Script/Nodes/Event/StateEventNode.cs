using System;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// EventNode for StateMachine
	/// </summary>
	[AddComponentMenu("")]
	public class StateEventNode : BaseStateEvent {
		public enum EventType {
			OnEnter,
			OnExit,
			Update,
			FixedUpdate,
			LateUpdate,
			OnAnimatorIK,
			OnAnimatorMove,
			OnApplicationFocus,
			OnApplicationPause,
			OnApplicationQuit,
			OnBecameInvisible,
			OnBecameVisible,
			OnCollisionEnter,
			OnCollisionEnter2D,
			OnCollisionExit,
			OnCollisionExit2D,
			OnCollisionStay,
			OnCollisionStay2D,
			OnDestroy,
			OnDisable,
			OnEnable,
			OnGUI,
			OnMouseDown,
			OnMouseDrag,
			OnMouseEnter,
			OnMouseExit,
			OnMouseOver,
			OnMouseUp,
			OnMouseUpAsButton,
			OnPostRender,
			OnPreCull,
			OnPreRender,
			OnRenderObject,
			OnTransformChildrenChanged,
			OnTransformParentChanged,
			OnTriggerEnter,
			OnTriggerEnter2D,
			OnTriggerExit,
			OnTriggerExit2D,
			OnTriggerStay,
			OnTriggerStay2D,
			OnWillRenderObject,
			OnParticleCollision,
		}
		public EventType eventType;
		[Hide]
		public MemberData storeParameter = new MemberData();

		public override void OnRuntimeInitialize() {
			base.OnRuntimeInitialize();
			switch(eventType) {
				case EventType.OnEnter:
					stateNode.onEnter += Trigger;
					break;
				case EventType.OnExit:
					stateNode.onExit += Trigger;
					break;
				case EventType.Update:
					UEvent.Register(UEventID.Update, owner, Trigger);
					break;
				case EventType.FixedUpdate:
					UEvent.Register(UEventID.FixedUpdate, owner, Trigger);
					break;
				case EventType.LateUpdate:
					UEvent.Register(UEventID.LateUpdate, owner, Trigger);
					break;
				case EventType.OnAnimatorIK:
					UEvent.Register(UEventID.OnAnimatorIK, owner, (int obj) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(obj);
						}
						Trigger();
					});
					break;
				case EventType.OnAnimatorMove:
					UEvent.Register(UEventID.Update, owner, Trigger);
					break;
				case EventType.OnApplicationFocus:
					UEvent.Register(UEventID.OnApplicationFocus, owner, (bool obj) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(obj);
						}
						Trigger();
					});
					break;
				case EventType.OnApplicationPause:
					UEvent.Register(UEventID.OnApplicationPause, owner, (bool obj) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(obj);
						}
						Trigger();
					});
					break;
				case EventType.OnApplicationQuit:
					UEvent.Register(UEventID.OnApplicationQuit, owner, Trigger);
					break;
				case EventType.OnBecameInvisible:
					UEvent.Register(UEventID.OnBecameInvisible, owner, Trigger);
					break;
				case EventType.OnBecameVisible:
					UEvent.Register(UEventID.OnBecameVisible, owner, Trigger);
					break;
				case EventType.OnCollisionEnter:
					UEvent.Register(UEventID.OnCollisionEnter, owner, (Collision col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnCollisionEnter2D:
					UEvent.Register(UEventID.OnCollisionEnter2D, owner, (Collision2D col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnCollisionExit:
					UEvent.Register(UEventID.OnCollisionExit, owner, (Collision col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnCollisionExit2D:
					UEvent.Register(UEventID.OnCollisionExit2D, owner, (Collision2D col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnCollisionStay:
					UEvent.Register(UEventID.OnCollisionStay, owner, (Collision col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnCollisionStay2D:
					UEvent.Register(UEventID.OnCollisionStay2D, owner, (Collision2D col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnDestroy:
					runtimeUNode.onDestroy += Trigger;
					break;
				case EventType.OnDisable:
					runtimeUNode.onDisable += Trigger;
					break;
				case EventType.OnEnable:
					runtimeUNode.onEnable += Trigger;
					break;
				case EventType.OnGUI:
					UEvent.Register(UEventID.OnGUI, owner, Trigger);
					break;
				case EventType.OnMouseDown:
					UEvent.Register(UEventID.OnMouseDown, owner, Trigger);
					break;
				case EventType.OnMouseDrag:
					UEvent.Register(UEventID.OnMouseDrag, owner, Trigger);
					break;
				case EventType.OnMouseEnter:
					UEvent.Register(UEventID.OnMouseEnter, owner, Trigger);
					break;
				case EventType.OnMouseExit:
					UEvent.Register(UEventID.OnMouseExit, owner, Trigger);
					break;
				case EventType.OnMouseOver:
					UEvent.Register(UEventID.OnMouseOver, owner, Trigger);
					break;
				case EventType.OnMouseUp:
					UEvent.Register(UEventID.OnMouseUp, owner, Trigger);
					break;
				case EventType.OnMouseUpAsButton:
					UEvent.Register(UEventID.OnMouseUpAsButton, owner, Trigger);
					break;
				case EventType.OnParticleCollision:
					UEvent.Register(UEventID.OnParticleCollision, owner, (GameObject gameObject) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(gameObject);
						}
						Trigger();
					});
					break;
				case EventType.OnPostRender:
					UEvent.Register(UEventID.OnPostRender, owner, Trigger);
					break;
				case EventType.OnPreCull:
					UEvent.Register(UEventID.OnPreCull, owner, Trigger);
					break;
				case EventType.OnPreRender:
					UEvent.Register(UEventID.OnPreRender, owner, Trigger);
					break;
				case EventType.OnRenderObject:
					UEvent.Register(UEventID.OnRenderObject, owner, Trigger);
					break;
				case EventType.OnTransformChildrenChanged:
					UEvent.Register(UEventID.OnTransformChildrenChanged, owner, Trigger);
					break;
				case EventType.OnTransformParentChanged:
					UEvent.Register(UEventID.OnTransformParentChanged, owner, Trigger);
					break;
				case EventType.OnTriggerEnter:
					UEvent.Register(UEventID.OnTriggerEnter, owner, (Collider col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnTriggerEnter2D:
					UEvent.Register(UEventID.OnTriggerEnter2D, owner, (Collider2D col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnTriggerExit:
					UEvent.Register(UEventID.OnTriggerExit, owner, (Collider col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnTriggerExit2D:
					UEvent.Register(UEventID.OnTriggerExit2D, owner, (Collider2D col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnTriggerStay:
					UEvent.Register(UEventID.OnTriggerStay, owner, (Collider col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnTriggerStay2D:
					UEvent.Register(UEventID.OnTriggerStay2D, owner, (Collider2D col) => {
						if(storeParameter.isAssigned) {
							storeParameter.Set(col);
						}
						Trigger();
					});
					break;
				case EventType.OnWillRenderObject:
					UEvent.Register(UEventID.OnWillRenderObject, owner, Trigger);
					break;
			}
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterAsStateNode(this);
		}

		public override Type GetNodeIcon() {
			switch(eventType) {
				case EventType.OnMouseDown:
				case EventType.OnMouseDrag:
				case EventType.OnMouseEnter:
				case EventType.OnMouseExit:
				case EventType.OnMouseOver:
				case EventType.OnMouseUp:
				case EventType.OnMouseUpAsButton:
					return typeof(TypeIcons.MouseIcon);
				case EventType.OnCollisionEnter:
				case EventType.OnCollisionExit:
				case EventType.OnCollisionStay:
				case EventType.OnTriggerEnter:
				case EventType.OnTriggerExit:
				case EventType.OnTriggerStay:
					return typeof(BoxCollider);
				case EventType.OnCollisionEnter2D:
				case EventType.OnCollisionExit2D:
				case EventType.OnCollisionStay2D:
				case EventType.OnTriggerEnter2D:
				case EventType.OnTriggerExit2D:
				case EventType.OnTriggerStay2D:
					return typeof(BoxCollider2D);
			}
			return typeof(TypeIcons.EventIcon);
		}
	}
}