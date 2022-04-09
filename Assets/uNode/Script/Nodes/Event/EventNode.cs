using System;
using UnityEngine;
using System.Collections.Generic;

namespace MaxyGames.uNode {
	/// <summary>
	/// A event node to call another node.
	/// </summary>
	[AddComponentMenu("")]
	public class EventNode : BaseGraphEvent, IExtendedOutput {
		/// <summary>
		/// The name of this node.
		/// </summary>
		public string Name;

		public enum EventType {
			Custom,
			Awake,
			Start,
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

		[HideInInspector]
		public MemberData[] targetObjects = new MemberData[0];

		[Hide("eventType", EventType.Awake)]
		[Hide("eventType", EventType.Custom)]
		[Hide("eventType", EventType.Start)]
		[Hide("eventType", EventType.Update)]
		[Hide("eventType", EventType.FixedUpdate)]
		[Hide("eventType", EventType.LateUpdate)]
		[Hide("eventType", EventType.OnAnimatorMove)]
		[Hide("eventType", EventType.OnApplicationQuit)]
		[Hide("eventType", EventType.OnBecameInvisible)]
		[Hide("eventType", EventType.OnBecameVisible)]
		[Hide("eventType", EventType.OnDestroy)]
		[Hide("eventType", EventType.OnDisable)]
		[Hide("eventType", EventType.OnEnable)]
		[Hide("eventType", EventType.OnGUI)]
		[Hide("eventType", EventType.OnMouseDown)]
		[Hide("eventType", EventType.OnMouseDrag)]
		[Hide("eventType", EventType.OnMouseEnter)]
		[Hide("eventType", EventType.OnMouseExit)]
		[Hide("eventType", EventType.OnMouseOver)]
		[Hide("eventType", EventType.OnMouseUp)]
		[Hide("eventType", EventType.OnMouseUpAsButton)]
		[Hide("eventType", EventType.OnPreCull)]
		[Hide("eventType", EventType.OnPreRender)]
		[Hide("eventType", EventType.OnRenderObject)]
		[Hide("eventType", EventType.OnTransformChildrenChanged)]
		[Hide("eventType", EventType.OnTransformParentChanged)]
		[Hide("eventType", EventType.OnWillRenderObject)]
		[Filter(SetMember =true)]
		public MemberData storeValue = new MemberData();

		private object eventValue;

		public override string GetNodeName() {
			if(eventType == EventType.Custom) {
				return Name;
			}
			return eventType.ToString();
		}

		public override void OnRuntimeInitialize() {
			base.OnRuntimeInitialize();
			if(targetObjects.Length == 0 || eventType == EventType.Awake || eventType == EventType.Custom || eventType == EventType.Start || eventType == EventType.OnEnable || eventType == EventType.OnDisable) {
				switch(eventType) {
					case EventType.Start:
						runtimeUNode.onStart += Trigger;
						break;
					case EventType.Custom:
						(runtimeUNode as uNodeRuntime).customMethod.Add(Name, this);
						break;
					case EventType.Awake:
						runtimeUNode.onAwake += Trigger;
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
							if(storeValue.isAssigned) {
								storeValue.Set(obj);
							}
							eventValue = obj;
							Trigger();
						});
						break;
					case EventType.OnAnimatorMove:
						UEvent.Register(UEventID.Update, owner, Trigger);
						break;
					case EventType.OnApplicationFocus:
						UEvent.Register(UEventID.OnApplicationFocus, owner, (bool obj) => {
							if(storeValue.isAssigned) {
								storeValue.Set(obj);
							}
							eventValue = obj;
							Trigger();
						});
						break;
					case EventType.OnApplicationPause:
						UEvent.Register(UEventID.OnApplicationPause, owner, (bool obj) => {
							if(storeValue.isAssigned) {
								storeValue.Set(obj);
							}
							eventValue = obj;
							Trigger();
						});
						break;
					case EventType.OnApplicationQuit:
						UEvent.Register(UEventID.Update, owner, Trigger);
						break;
					case EventType.OnBecameInvisible:
						UEvent.Register(UEventID.Update, owner, Trigger);
						break;
					case EventType.OnBecameVisible:
						UEvent.Register(UEventID.Update, owner, Trigger);
						break;
					case EventType.OnCollisionEnter:
						UEvent.Register(UEventID.OnCollisionEnter, owner, (Collision col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnCollisionEnter2D:
						UEvent.Register(UEventID.OnCollisionEnter2D, owner, (Collision2D col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnCollisionExit:
						UEvent.Register(UEventID.OnCollisionExit, owner, (Collision col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnCollisionExit2D:
						UEvent.Register(UEventID.OnCollisionExit2D, owner, (Collision2D col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnCollisionStay:
						UEvent.Register(UEventID.OnCollisionStay, owner, (Collision col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnCollisionStay2D:
						UEvent.Register(UEventID.OnCollisionStay2D, owner, (Collision2D col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
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
							if(storeValue.isAssigned) {
								storeValue.Set(gameObject);
							}
							eventValue = gameObject;
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
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnTriggerEnter2D:
						UEvent.Register(UEventID.OnTriggerEnter2D, owner, (Collider2D col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnTriggerExit:
						UEvent.Register(UEventID.OnTriggerExit, owner, (Collider col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnTriggerExit2D:
						UEvent.Register(UEventID.OnTriggerExit2D, owner, (Collider2D col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnTriggerStay:
						UEvent.Register(UEventID.OnTriggerStay, owner, (Collider col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnTriggerStay2D:
						UEvent.Register(UEventID.OnTriggerStay2D, owner, (Collider2D col) => {
							if(storeValue.isAssigned) {
								storeValue.Set(col);
							}
							eventValue = col;
							Trigger();
						});
						break;
					case EventType.OnWillRenderObject:
						UEvent.Register(UEventID.OnWillRenderObject, owner, Trigger);
						break;
				}
			} else {
				switch(eventType) {
					case EventType.Update:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.Update, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.FixedUpdate:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.FixedUpdate, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.LateUpdate:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.LateUpdate, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnAnimatorIK:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (int obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnAnimatorMove:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnAnimatorMove, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnApplicationFocus:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (bool obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnApplicationPause:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (bool obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnApplicationQuit:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationQuit, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnBecameInvisible:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnBecameInvisible, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnBecameVisible:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnBecameVisible, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnCollisionEnter:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collision obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnCollisionEnter2D:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collision2D obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnCollisionExit:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collision obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnCollisionExit2D:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collision2D obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnCollisionStay:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collision obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnCollisionStay2D:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collision2D obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnDestroy:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnDestroy, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnGUI:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnGUI, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseDown:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseDown, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseDrag:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseDrag, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseEnter:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseEnter, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseExit:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseExit, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseOver:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseOver, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseUp:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseUp, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnMouseUpAsButton:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnMouseUpAsButton, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnPostRender:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnPostRender, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnPreCull:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnPreCull, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnPreRender:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnPreRender, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnRenderObject:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnRenderObject, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnTransformChildrenChanged:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnTransformChildrenChanged, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnTransformParentChanged:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnTransformParentChanged, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
					case EventType.OnTriggerEnter:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collider obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnTriggerEnter2D:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collider2D obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnTriggerExit:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collider obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnTriggerExit2D:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collider2D obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnTriggerStay:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collider obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnTriggerStay2D:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnApplicationFocus, targetObjects[i].Get<GameObject>(), (Collider2D obj) => {
									if(storeValue.isAssigned) {
										storeValue.Set(obj);
									}
									eventValue = obj;
									Trigger();
								});
							}
						}
						break;
					case EventType.OnWillRenderObject:
						for(int i = 0; i < targetObjects.Length; i++) {
							if(targetObjects[i].isAssigned) {
								UEvent.Register(UEventID.OnWillRenderObject, targetObjects[i].Get<GameObject>(), Trigger);
							}
						}
						break;
				}
			}
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
			return base.GetNodeIcon();
		}

		#region Ports
		int IExtendedOutput.OutputCount {
			get {
				switch(eventType) {
					case EventType.OnAnimatorIK:
					case EventType.OnApplicationFocus:
					case EventType.OnApplicationPause:
					case EventType.OnCollisionEnter:
					case EventType.OnCollisionEnter2D:
					case EventType.OnCollisionExit:
					case EventType.OnCollisionExit2D:
					case EventType.OnCollisionStay:
					case EventType.OnCollisionStay2D:
					case EventType.OnParticleCollision:
					case EventType.OnTriggerEnter:
					case EventType.OnTriggerEnter2D:
					case EventType.OnTriggerExit:
					case EventType.OnTriggerExit2D:
					case EventType.OnTriggerStay:
					case EventType.OnTriggerStay2D:
						return 1;
				}
				return 0;
			}
		}

		object IExtendedOutput.GetOutputValue(string name) {
			return eventValue;
		}

		string IExtendedOutput.GetOutputName(int index) {
			return "obj";
		}

		Type IExtendedOutput.GetOutputType(string name) {
			switch(eventType) {
				case EventType.OnAnimatorIK:
					return typeof(int);
				case EventType.OnApplicationFocus:
					return typeof(bool);
				case EventType.OnApplicationPause:
					return typeof(bool);
				case EventType.OnCollisionEnter:
					return typeof(Collision);
				case EventType.OnCollisionEnter2D:
					return typeof(Collision2D);
				case EventType.OnCollisionExit:
					return typeof(Collision);
				case EventType.OnCollisionExit2D:
					return typeof(Collision2D);
				case EventType.OnCollisionStay:
					return typeof(Collision);
				case EventType.OnCollisionStay2D:
					return typeof(Collision2D);
				case EventType.OnParticleCollision:
					return typeof(GameObject);
				case EventType.OnTriggerEnter:
					return typeof(Collider);
				case EventType.OnTriggerEnter2D:
					return typeof(Collider2D);
				case EventType.OnTriggerExit:
					return typeof(Collider);
				case EventType.OnTriggerExit2D:
					return typeof(Collider2D);
				case EventType.OnTriggerStay:
					return typeof(Collider);
				case EventType.OnTriggerStay2D:
					return typeof(Collider2D);
			}
			return typeof(object);
		}

		string IExtendedOutput.GenerateOutputCode(string name) {
			string paramName = null;
			switch(eventType) {
				case EventType.OnAnimatorIK: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(int)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(int).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnApplicationFocus: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(bool)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(bool).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnApplicationPause: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(bool)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(bool).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnCollisionEnter: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collision)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collision).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnCollisionEnter2D: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collision2D)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collision2D).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnCollisionExit: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collision)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collision).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnCollisionExit2D: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collision2D)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collision2D).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnCollisionStay: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collision)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collision).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnCollisionStay2D: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collision2D)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collision2D).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnParticleCollision: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(GameObject)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(GameObject).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnTriggerEnter: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collider)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collider).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnTriggerEnter2D: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collider2D)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collider2D).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnTriggerExit: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collider)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collider).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnTriggerExit2D: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collider2D)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collider2D).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnTriggerStay: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collider)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collider).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				case EventType.OnTriggerStay2D: {
					if(!CG.HasUserObject(this)) {
						paramName = CG.RegisterVariable(new VariableData("obj", typeof(Collider2D)) { modifier = FieldModifier.PrivateModifier });
						CG.RegisterUserObject(paramName, this);
						var mData = CG.generatorData.GetMethodData(eventType.ToString());
						if(mData == null) {
							mData = CG.generatorData.AddMethod(eventType.ToString(), typeof(void).CGType(), typeof(Collider2D).CGType());
						}
						mData.AddCode(CG.Set(paramName, mData.parameters[0].name));
					} else {
						paramName = CG.GetUserObject<string>(this);
					}
					break;
				}
				default:
					throw new NotImplementedException(eventType.ToString());
			}
			return paramName;
		}
		#endregion;

		#region Generation
		public override void GenerateCode() {
			List<string> parameterType = new List<string>();
			if(eventType == EventType.OnCollisionEnter ||
				eventType == EventType.OnCollisionExit ||
				eventType == EventType.OnCollisionStay) {
				parameterType.Add(CG.Type(typeof(Collision)));
			} else if(eventType == EventType.OnTriggerEnter ||
				eventType == EventType.OnTriggerExit ||
				eventType == EventType.OnTriggerStay) {
				parameterType.Add(CG.Type(typeof(Collider)));
			} else if(eventType == EventType.OnCollisionEnter2D ||
				eventType == EventType.OnCollisionExit2D ||
				eventType == EventType.OnCollisionStay2D) {
				parameterType.Add(CG.Type(typeof(Collision2D)));
			} else if(eventType == EventType.OnTriggerEnter2D ||
				 eventType == EventType.OnTriggerExit2D ||
				 eventType == EventType.OnTriggerStay2D) {
				parameterType.Add(CG.Type(typeof(Collider2D)));
			} else if(eventType == EventType.OnApplicationPause ||
				eventType == EventType.OnApplicationFocus) {
				parameterType.Add(CG.Type(typeof(bool)));
			} else if(eventType == EventType.OnAnimatorIK) {
				parameterType.Add(CG.Type(typeof(int)));
			}
			if(targetObjects != null && targetObjects.Length > 0) {//Generate event code for multiple target objects.
				foreach(var e in targetObjects) {
					if(e == null || !e.isAssigned)
						continue;
					string contents = GenerateFlows();
					switch(eventType) {
						case EventType.OnCollisionEnter: {
							string paramName = CG.GenerateVariableName("col", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnCollisionEnter".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collision) },
										new string[] { paramName },
										contents
									)),
								new string[0]);
							break;
						}
						case EventType.OnCollisionExit: {
							string paramName = CG.GenerateVariableName("col", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnCollisionExit".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collision) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnCollisionStay: {
							string paramName = CG.GenerateVariableName("col", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnCollisionStay".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collision) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnCollisionEnter2D: {
							string paramName = CG.GenerateVariableName("col", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnCollisionEnter2D".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collision2D) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnCollisionExit2D: {
							string paramName = CG.GenerateVariableName("col", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnCollisionExit2D".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collision2D) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnCollisionStay2D: {
							string paramName = CG.GenerateVariableName("col", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnCollisionStay2D".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collision2D) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTriggerEnter: {
							string paramName = CG.GenerateVariableName("other", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTriggerEnter".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collider) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTriggerExit: {
							string paramName = CG.GenerateVariableName("other", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTriggerExit".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collider) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTriggerStay: {
							string paramName = CG.GenerateVariableName("other", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTriggerStay".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collider) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTriggerEnter2D: {
							string paramName = CG.GenerateVariableName("other", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTriggerEnter2D".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collider2D) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTriggerExit2D: {
							string paramName = CG.GenerateVariableName("other", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTriggerExit2D".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collider2D) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTriggerStay2D: {
							string paramName = CG.GenerateVariableName("other", this);
							if(storeValue.isAssigned) {
								contents = contents.Insert(0,
									CG.Set(
										storeValue,
										CG.WrapString(paramName),
										storeValue.type
									).AddLineInFirst());
							}
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTriggerStay2D".CGValue(),
									CG.Lambda(
										new Type[] { typeof(Collider2D) },
										new string[] { paramName },
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnBecameInvisible: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnBecameInvisible".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnBecameVisible: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnBecameVisible".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnDestroy: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnDestroy".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseDown: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseDown".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseDrag: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseDrag".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseEnter: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseEnter".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseExit: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseExit".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseOver: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseOver".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseUp: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseUp".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnMouseUpAsButton: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnMouseUpAsButton".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTransformChildrenChanged: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTransformChildrenChanged".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						case EventType.OnTransformParentChanged: {
							CG.generatorData.InsertMethodCode(
								"Awake",
								CG.Type(typeof(void)),
								CG.FlowInvoke(
									typeof(UEvent),
									nameof(UEvent.Register),
									CG.Value(e),
									"OnTransformParentChanged".CGValue(),
									CG.Lambda(
										null,
										null,
										contents
									)).AddLineInFirst(),
								new string[0]);
							break;
						}
						default:
							throw new NotImplementedException(eventType.ToString());
					}
				}
			} else {//Self target : Add functions.
				string fName = eventType == EventType.Custom ? CG.generatorData.GetMethodName(this) : eventType.ToString();
				var mData = CG.generatorData.GetMethodData(fName, parameterType);
				if(mData == null) {
					var func = CG.graph.GetFunction(fName);
					Type funcType = typeof(void);
					if(func != null) {
						funcType = func.ReturnType();
					}
					mData = CG.generatorData.AddMethod(fName, CG.Type(funcType), parameterType);
					if(eventType == EventType.Custom) {
						mData.modifier = new FunctionModifier() { Public = true };
					}
				}
				string initData = null;
				if(eventType == EventType.OnCollisionEnter ||
					eventType == EventType.OnCollisionExit ||
					eventType == EventType.OnCollisionStay) {
					parameterType.Add(CG.Type(typeof(Collision)));
					if(storeValue.isAssigned) {
						initData = CG.Set(storeValue, mData.parameters[0].name);
					}
				} else if(eventType == EventType.OnTriggerEnter ||
					eventType == EventType.OnTriggerExit ||
					eventType == EventType.OnTriggerStay) {
					parameterType.Add(CG.Type(typeof(Collider)));
					if(storeValue.isAssigned) {
						initData = CG.Set(storeValue, mData.parameters[0].name);
					}
				} else if(eventType == EventType.OnCollisionEnter2D ||
					eventType == EventType.OnCollisionExit2D ||
					eventType == EventType.OnCollisionStay2D) {
					parameterType.Add(CG.Type(typeof(Collision2D)));
					if(storeValue.isAssigned) {
						initData = CG.Set(storeValue, mData.parameters[0].name);
					}
				} else if(eventType == EventType.OnTriggerEnter2D ||
					 eventType == EventType.OnTriggerExit2D ||
					eventType == EventType.OnTriggerStay2D) {
					parameterType.Add(CG.Type(typeof(Collider2D)));
					if(storeValue.isAssigned) {
						initData = CG.Set(storeValue, mData.parameters[0].name);
					}
				} else if(eventType == EventType.OnApplicationPause ||
					eventType == EventType.OnApplicationFocus) {
					parameterType.Add(CG.Type(typeof(bool)));
					if(storeValue.isAssigned) {
						initData = CG.Set(storeValue, mData.parameters[0].name);
					}
				} else if(eventType == EventType.OnAnimatorIK) {
					parameterType.Add(CG.Type(typeof(int)));
					if(storeValue.isAssigned) {
						initData = CG.Set(storeValue, mData.parameters[0].name);
					}
				}
				if(!string.IsNullOrEmpty(initData)) {
				}
				mData.AddCodeForEvent(CG.Flow(initData, GenerateFlows()));
			}
		}
		#endregion
	}
}