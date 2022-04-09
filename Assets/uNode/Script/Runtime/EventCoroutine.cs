using System.Collections;
using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames.Runtime {
	/// <summary>
	/// EventCoroutine is a class to Start coroutine with return data and some function that useful for making event based coroutine.
	/// </summary>
	public class EventCoroutine : CustomYieldInstruction {
		#region Classes
		internal class Processor {
			public IEventIterator target;
			public bool isStopped;

			private object current;
			private bool isNotEnd;
			private bool canMoveNext = true;

			public Processor(IEventIterator target) {
				this.target = target;
			}

			public void Reset() {
				target.Reset();
				current = null;
				canMoveNext = true;
				isStopped = false;
			}

			public void Stop() {
				isStopped = true;
			}

			public bool Process(ref int state) {
				if(isStopped)
					return false;
				if(canMoveNext) {
					isNotEnd = target.MoveNext();
					this.current = target.Current;
				}
				var current = this.current;
				if(current == null) {
					return isNotEnd;
				} else if(current is bool) {
					bool r = (bool)current;
					state = r ? 1 : 2;
					return false;
				} else if(current is string) {
					string r = current as string;
					if(r == "Success") {
						state = 1;
						//Mark process finish
						return false;
					} else if(r == "Failure") {
						state = 2;
						//Mark process finish
						return false;
					}
				} else if(current is CustomYieldInstruction) {
					if((current as CustomYieldInstruction).keepWaiting) {
						canMoveNext = false;
						//Wait for next process.
						return true;
					} else {
						//Immediately process to next
						this.current = null;
						canMoveNext = isNotEnd;
						return Process(ref state);
					}
				} else if(current is WaitSecond) {
					if(((WaitSecond)current).IsTimeExceed()) {
						this.current = null;
						canMoveNext = isNotEnd;
						return Process(ref state);
					} else {
						//Wait for next process.
						return true;
					}
				} else if(current is WaitForSeconds) {
					this.current = new WaitSecond((float)current.GetType().GetFieldCached("m_Seconds").GetValueOptimized(current), false);
					canMoveNext = false;
					return true;
				} else if(current is WaitForEndOfFrame || current is WaitForFixedUpdate) {
					//Wait for next process.
					canMoveNext = isNotEnd;
					return true;
				}
				return isNotEnd;
			}

			struct WaitSecond {
				public float time;
				public bool unscaled;

				public WaitSecond(float time, bool unscaled) {
					if(unscaled) {
						this.time = Time.unscaledTime + time;
					} else {
						this.time = Time.time + time;
					}
					this.unscaled = unscaled;
				}

				public bool IsTimeExceed() {
					if(unscaled) {
						return Time.unscaledTime - time >= 0;
					} else {
						return Time.time - time >= 0;
					}
				}
			}
		}
		#endregion

		private int rawState;

		/// <summary>
		/// The owner of coroutine to start running coroutine
		/// </summary>
		public MonoBehaviour owner;
		private Processor target;
		private Processor onStop;
		private bool hasRun, hasStop;
		private bool run;

		/// <summary>
		/// Indicate state of Coroutine, 
		/// "Success" indicate state is success, 
		/// "Failure" indicate state is failure, 
		/// otherwise indicate state is running or never running
		/// </summary>
		public string state {
			get {
				switch(rawState) {
					case 1:
						return "Success";
					case 2:
						return "Failure";
					default:
						return null;
				}
			}
		}

		/// <summary>
		/// Indicate coroutine is finished running when its has running before
		/// </summary>
		public bool IsFinished {
			get {
				return hasRun && rawState != 0;
			}
		}

		/// <summary>
		/// Indicate coroutine is finished running or never running
		/// </summary>
		public bool IsFinishedOrNeverRun {
			get {
				return rawState != 0;
			}
		}

		/// <summary>
		/// True if the state is "Success"
		/// </summary>
		public bool IsSuccess => rawState == 1;
		/// <summary>
		/// Try if the state is "Failure"
		/// </summary>
		public bool IsFailure => rawState == 2;
		/// <summary>
		/// True if the state is "Running"
		/// </summary>
		public bool IsRunning => rawState == 0 && run;

		public override bool keepWaiting => !IsFinishedOrNeverRun;

		/// <summary>
		/// Create a new event.
		/// </summary>
		/// <returns></returns>
		public static EventCoroutine New() {
			return new EventCoroutine();
		}

		#region Create
		/// <summary>
		/// Create a new event
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static EventCoroutine Create(IEventIterator target) {
			return new EventCoroutine().Setup(target);
		}

		/// <summary>
		/// Create a new event
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static EventCoroutine Create(MonoBehaviour owner, IEventIterator target) {
			return new EventCoroutine().Setup(owner, target);
		}

		/// <summary>
		/// Create a new event
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static EventCoroutine Create(MonoBehaviour owner, params IEventIterator[] targets) {
			return new EventCoroutine().Setup(owner, Routine.New(targets));
		}

		/// <summary>
		/// Create a new event
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="targets"></param>
		/// <returns></returns>
		public static EventCoroutine Create(MonoBehaviour owner, params EventCoroutine[] targets) {
			return new EventCoroutine().Setup(owner, Routine.New(targets));
		}

		/// <summary>
		/// Create a new event
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static EventCoroutine Create(MonoBehaviour owner, System.Action target) {
			return new EventCoroutine().Setup(owner, Routine.New(target));
		}

		/// <summary>
		/// Create a new event
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="owner"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static EventCoroutine Create<T>(MonoBehaviour owner, System.Func<T> target) {
			return new EventCoroutine().Setup(owner, Routine.New(target));
		}
		#endregion

		#region Initializers
		/// <summary>
		/// Initialize Event Coroutine without owner.
		/// </summary>
		/// <param name="target"></param>
		public EventCoroutine Setup(IEventIterator target) {
			this.target = new Processor(target);
			this.owner = RuntimeSMHelper.Instance;
			return this;
		}

		/// <summary>
		/// Initialize Event Coroutine.
		/// </summary>
		/// <param name="owner">The coroutine owner</param>
		/// <param name="target">The coroutine function target</param>
		public EventCoroutine Setup(MonoBehaviour owner, IEventIterator target) {
			this.target = new Processor(target);
			this.owner = owner ?? RuntimeSMHelper.Instance;
			return this;
		}

		/// <summary>
		/// Initialize Event Coroutine
		/// </summary>
		/// <param name="owner">The coroutine owner</param>
		/// <param name="target">The coroutine function target</param>
		/// <param name="onStop"></param>
		public EventCoroutine Setup(MonoBehaviour owner, IEventIterator target, IEventIterator onStop) {
			this.target = new Processor(target);
			this.owner = owner ?? RuntimeSMHelper.Instance;
			this.onStop = new Processor(onStop);
			return this;
		}

		/// <summary>
		/// Initialize Event Coroutine
		/// </summary>
		/// <param name="owner">The coroutine owner</param>
		/// <param name="target">The coroutine function target</param>
		/// <param name="onStop"></param>
		public EventCoroutine Setup(MonoBehaviour owner, IEventIterator target, System.Action onStop) {
			this.target = new Processor(target);
			this.owner = owner ?? RuntimeSMHelper.Instance;
			this.onStop = new Processor(Routine.New(onStop));
			return this;
		}


		/// <summary>
		/// Initialize Event Coroutine without owner.
		/// </summary>
		/// <param name="target"></param>
		public EventCoroutine Setup(IEnumerable target) {
			this.target = new Processor(Routine.New(target));
			this.owner = RuntimeSMHelper.Instance;
			return this;
		}

		/// <summary>
		/// Initialize Event Coroutine.
		/// </summary>
		/// <param name="owner">The coroutine owner</param>
		/// <param name="target">The coroutine function target</param>
		public EventCoroutine Setup(MonoBehaviour owner, IEnumerable target) {
			this.target = new Processor(Routine.New(target));
			this.owner = owner ?? RuntimeSMHelper.Instance;
			return this;
		}

		/// <summary>
		/// Initialize Event Coroutine
		/// </summary>
		/// <param name="owner">The coroutine owner</param>
		/// <param name="target">The coroutine function target</param>
		/// <param name="onStop"></param>
		public EventCoroutine Setup(MonoBehaviour owner, IEnumerable target, System.Action onStop) {
			this.target = new Processor(Routine.New(target));
			this.owner = owner ?? RuntimeSMHelper.Instance;
			this.onStop = new Processor(Routine.New(onStop));
			return this;
		}

		/// <summary>
		/// Initialize Event Coroutine
		/// </summary>
		/// <param name="owner">The coroutine owner</param>
		/// <param name="target">The coroutine function target</param>
		/// <param name="onStop"></param>
		public EventCoroutine Setup(MonoBehaviour owner, IEnumerable target, IEnumerable onStop) {
			this.target = new Processor(Routine.New(target));
			this.owner = owner ?? RuntimeSMHelper.Instance;
			this.onStop = new Processor(Routine.New(onStop));
			return this;
		}

		public EventCoroutine OnStop(IEventIterator target) {
			this.onStop = new Processor(target);
			return this;
		}

		public EventCoroutine OnStop(System.Action target) {
			this.onStop = new Processor(Routine.New(target));
			return this;
		}

		public EventCoroutine OnStop(IEnumerable target) {
			this.onStop = new Processor(Routine.New(target));
			return this;
		}

		public EventCoroutine OnStop(EventCoroutine target) {
			this.onStop = new Processor(Routine.New(target));
			return this;
		}

		public EventCoroutine OnStop<T>(System.Func<T> target) {
			this.onStop = new Processor(Routine.New(target));
			return this;
		}
		#endregion

		/// <summary>
		/// Run the coroutine if not running
		/// </summary>
		/// <returns></returns>
		public EventCoroutine Run() {
			if(!run) {
				if(hasRun) {
					target.Reset();
				}
				rawState = 0;
				run = true;
				hasRun = true;
#if UNITY_EDITOR
				if(debug) {
					uNodeDEBUG.InvokeEvent(this, eventUID, debugUID);
				}
#endif
				Update();
				if(rawState == 0)
					UEvent.Register(UEventID.Update, owner, Update);
			}
			return this;
		}

		void Update() {
			if(run) {
				if(!target.Process(ref rawState)) {
					bool s = rawState != 2;
					rawState = 0;
#if UNITY_EDITOR
					if(debug) {
						uNodeDEBUG.InvokeEvent(this, eventUID, debugUID);
					}
#endif
					Stop(s);
				}
			}
		}

		void UpdateStop() {
			int state = 0;
			if(!onStop.Process(ref state)) {
				run = false;
				hasStop = true;
				UEvent.Unregister(UEventID.Update, owner, UpdateStop);
			}
		}

		/// <summary>
		/// Stop Running Coroutine.
		/// </summary>
		public void Stop(bool state = false) {
			if(rawState == 0) {
				rawState = state ? 1 : 2;
				run = false;
				if(onStop != null) {
					if(hasStop)
						onStop.Reset();
					UpdateStop();
					if(rawState == 0)
						UEvent.Register(UEventID.Update, owner, UpdateStop);
				}
			}
		}

		#region Debug
#if UNITY_EDITOR
		private bool debug;
		private int debugUID;
		private int eventUID;
#endif

		/// <summary>
		/// Call this to implement debugging in editor.
		/// </summary>
		/// <param name="nodeUID"></param>
		public void Debug(int eventSystemUID, int nodeUID) {
#if UNITY_EDITOR
			debug = true;
			eventUID = eventSystemUID;
			debugUID = nodeUID;
#endif
		}
		#endregion
	}
}