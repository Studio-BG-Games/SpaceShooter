using UnityEngine;
using MaxyGames.Events;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames {
	/// <summary>
	/// this class for manage and execute the blocks
	/// </summary>
	[System.Serializable]
	public class EventData {
		[SerializeField]
		private List<EventActionData> eventList = new List<EventActionData>();

		public List<EventActionData> blocks => eventList;

		public enum EventType {
			Condition,
			Action,
		}
		/// <summary>
		/// True indicate validation using Level Validation.
		/// </summary>
		public bool useLevelValidation;

		[System.NonSerialized]
		public int editIndex;//for editor

		public void AddBlock(Block eventAction, EventActionData.EventType eventType = EventActionData.EventType.Event) {
			eventList.Add(new EventActionData(eventAction, eventType));
		}

		public void AddBlockRange(IEnumerable<EventActionData> events) {
			if(events == null) return;
			eventList.AddRange(events);
		}

		public void InsertBlock(int index, Block eventAction, EventActionData.EventType eventType = EventActionData.EventType.Event) {
			eventList.Insert(index, new EventActionData(eventAction, eventType));
		}

		public void RemoveBlock(EventActionData @event) {
			eventList.Remove(@event);
		}

		public void RemoveBlock(int index) {
			eventList.RemoveAt(index);
		}

		#region Editors
		/// <summary>
		/// Function to check error on editor.
		/// </summary>
		/// <param name="owner">The owner of this event.</param>
		public virtual void CheckError(Object owner) {
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i] != null && eventList[i].block != null) {
					eventList[i].block.CheckError(owner);
				}
			}
		}

		/// <summary>
		/// Check whether the event has a coroutine action or not.
		/// </summary>
		/// <returns></returns>
		public bool HasCoroutineAction() {
			if(eventList == null || eventList.Count == 0) {
				//editIndex = -1;
				return false;
			}
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i].block != null) {
					Action action = eventList[i].block as Action;
					if(action != null && action.IsCoroutine()) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Generate code for event action or condition.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public string GenerateCode(uNode.Node node, EventType type) {
			if(eventList.Count > 0) {
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				int index = 0;
				int lastVLevel = 0;
				bool lastIsOR = false;
				foreach(EventActionData data in eventList) {
					try {
						string s = data.GenerateCode(node, type);
						int VLevel = data.levelValidation;
						if(!string.IsNullOrEmpty(s)) {
							if(type == EventType.Action) {
								builder.Append(s.AddLineInFirst());
							} else {
								if(index != 0) {
									if(!lastIsOR) {
										builder.Append(" && ");
									}
									if(useLevelValidation) {
										while(lastVLevel < VLevel) {
											builder.Append("(");
											VLevel--;
										}
									}
								}
								builder.Append(s);
							}
						} else if(type == EventType.Condition) {
							if(data.eventType == EventActionData.EventType.Event) {
								if(index != 0) {
									if(!lastIsOR) {
										builder.Append(" && ");
									}
									if(useLevelValidation) {
										while(lastVLevel < VLevel) {
											builder.Append("(");
											VLevel--;
										}
									}
								}
								builder.Append("true");
							} else {
								lastIsOR = true;
								builder.Append(" || ");
								lastVLevel = data.levelValidation;
								index++;
								continue;
							}
						}
						VLevel = data.levelValidation;
						if(type == EventType.Condition && useLevelValidation) {
							if(index + 1 >= eventList.Count) {
								lastVLevel = VLevel;
								VLevel = 0;
							} else {
								lastVLevel = VLevel;
								VLevel = eventList[index + 1].levelValidation;
							}
							while(lastVLevel > VLevel) {
								builder.Append(")");
								lastVLevel--;
							}
						}
						lastIsOR = false;
						lastVLevel = data.levelValidation;
						index++;
					}
					catch {
						Debug.LogError("Error in event:" + data.displayName + " |in index :" + index);
						throw;
					}
				}
				if(type == EventType.Condition && string.IsNullOrEmpty(builder.ToString())) {
					builder.Append("true");
				}
				return builder.ToString();
			}
			if(type == EventType.Condition) {
				return "true";
			}
			return null;
		}

		/// <summary>
		/// Generate coroutine action code.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="runInParallel"></param>
		/// <returns></returns>
		public string GenerateCoroutineCode(uNode.Node node, bool runInParallel) {
			if(eventList.Count > 0) {
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				int index = 0;
				foreach(EventActionData action in eventList) {
					try {
						if(action.isCoroutine && runInParallel) {
							if(CG.generatorData.coroutineEvent.ContainsKey(action.block)) {
								builder.Append(CG.RunEvent(action.block).AddLineInFirst());
							} else {
								string data = action.GenerateCode(node, EventType.Action);
								if(!string.IsNullOrEmpty(data)) {
									CG.SetStateAction(action.block, data);
									if(!CG.generatorData.portableActionInNode.Contains(node)) {
										CG.generatorData.portableActionInNode.Add(node);
									}
									builder.Append(CG.RunEvent(action.block).AddLineInFirst());
								}
							}
						} else {
							string s = action.GenerateCode(node, EventType.Action);
							if(!string.IsNullOrEmpty(s)) {
								builder.Append(s.AddLineInFirst());
							}
						}
						index++;
					}
					catch {
						Debug.LogError("Error in event:" + action.displayName + " |in index :" + index);
						throw;
					}
				}
				if(runInParallel) {
					foreach(EventActionData action in eventList) {
						if(action.isCoroutine) {
							builder.Append(CG.WaitEvent(action.block, false).AddLineInFirst());
						}
					}
				}
				return builder.ToString();
			}
			return null;
		}

		public string GenerateStopCode(Object obj) {
			string result = null;
			foreach(EventActionData data in eventList) {
				result += data.GenerateStopCode(obj).AddLineInFirst();
			}
			return result;
		}

		/// <summary>
		/// Function for generating condition code
		/// </summary>
		/// <param name="node"></param>
		/// <param name="contents">The content inside condition</param>
		/// <returns></returns>
		public string GenerateConditionCode(uNode.Node node, string contents) {
			if(eventList.Count > 0) {
				string str = GenerateCode(node, EventType.Condition);
				if(!string.IsNullOrEmpty(str)) {
					str = str.Insert(0, "if(");
					if(!string.IsNullOrEmpty(contents)) {
						contents = ("\n" + contents).AddTabAfterNewLine(1) + "\n";
					}
					str += ") {" + contents + "}";
					return str;
				}
			} else {
				//Debug.Log("No event.", node);
			}
			if(!string.IsNullOrEmpty(contents)) {
				contents = ("\n" + contents).AddTabAfterNewLine(1) + "\n";
			}
			return "if(true) {" + contents + "}";
		}

		/// <summary>
		/// Function for generating condition code
		/// </summary>
		/// <param name="node"></param>
		/// <param name="contents"></param>
		/// <param name="elseContents"></param>
		/// <returns></returns>
		public string GenerateConditionCode(uNode.Node node, string contents, string elseContents) {
			var result = GenerateConditionCode(node, contents);
			if(string.IsNullOrEmpty(result)) {
				return result;
			}
			return result.Add(" else {").AddLineInEnd() + elseContents.AddTabAfterNewLine().AddLineInEnd().Add("}");
		}
		#endregion

		#region Action
		/// <summary>
		/// Execute action.
		/// </summary>
		public void Execute(UnityEngine.Object instance) {
			//if(editIndex == -1) return;
			if(eventList == null || eventList.Count == 0) {
				//editIndex = -1;
				return;
			}
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i].block != null && eventList[i].eventType == EventActionData.EventType.Event) {
					try {
						eventList[i].block.Execute(instance);
					}
					catch {
						Debug.LogError("Error in action : " + eventList[i].displayName + " in index : " + i);
						throw;
					}
				}
			}
		}

		private ActionCoroutine actionCoroutine;
		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public Coroutine StartAction(MonoBehaviour owner) {
			actionCoroutine = new ActionCoroutine(owner, ExecuteCoroutine(owner));
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public Coroutine StartAction(MonoBehaviour owner, System.Action onFinished) {
			actionCoroutine = new ActionCoroutine(owner, ExecuteCoroutine(owner, onFinished));
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="startCoroutine"></param>
		/// <param name="stopCoroutine"></param>
		/// <returns></returns>
		public Coroutine StartAction(
			UnityEngine.Object instance, 
			System.Func<IEnumerator, Coroutine> startCoroutine, 
			System.Action<Coroutine> stopCoroutine) {
			actionCoroutine = new ActionCoroutine(ExecuteCoroutine(instance), startCoroutine, stopCoroutine);
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="startCoroutine"></param>
		/// <param name="stopCoroutine"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public Coroutine StartAction(
			UnityEngine.Object instance,
			System.Func<IEnumerator, Coroutine> startCoroutine, 
			System.Action<Coroutine> stopCoroutine, 
			System.Action onFinished) {
			actionCoroutine = new ActionCoroutine(ExecuteCoroutine(instance, onFinished), startCoroutine, stopCoroutine);
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public Coroutine StartActionInParallel(MonoBehaviour owner) {
			actionCoroutine = new ActionCoroutine(owner, ExecuteCoroutineInParallel(owner, owner.StartCoroutine));
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="onFinished"></param>
		/// <returns></returns>
		public Coroutine StartActionInParallel(MonoBehaviour owner, System.Action onFinished) {
			actionCoroutine = new ActionCoroutine(owner, ExecuteCoroutineInParallel(owner, owner.StartCoroutine, onFinished));
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="startCoroutine"></param>
		/// <param name="stopCoroutine"></param>
		/// <returns></returns>
		public Coroutine StartActionInParallel(
			UnityEngine.Object instance, 
			System.Func<IEnumerator, Coroutine> startCoroutine, 
			System.Action<Coroutine> stopCoroutine) {
			actionCoroutine = new ActionCoroutine(ExecuteCoroutineInParallel(instance, startCoroutine), startCoroutine, stopCoroutine);
			return actionCoroutine.Run();
		}


		/// <summary>
		/// Start execute an action.
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		public Coroutine StartActionInParallel(
			UnityEngine.Object instance, 
			System.Func<IEnumerator, Coroutine> startCoroutine, 
			System.Action<Coroutine> stopCoroutine, System.Action onFinished) {
			actionCoroutine = new ActionCoroutine(ExecuteCoroutineInParallel(instance, startCoroutine, onFinished), startCoroutine, stopCoroutine);
			return actionCoroutine.Run();
		}

		/// <summary>
		/// Stop running coroutine action.
		/// </summary>
		public void StopAction() {
			if(actionCoroutine != null) {
				actionCoroutine.Stop();
				actionCoroutine = null;
			}
		}

		private IEnumerable ExecuteCoroutine(UnityEngine.Object instance) {
			if(eventList == null || eventList.Count == 0) {
				yield break;
			}
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i].block != null && eventList[i].eventType == EventActionData.EventType.Event) {
					IEnumerator iterator = eventList[i].block.ExecuteCoroutine(instance);
					while(iterator.MoveNext()) {
						yield return iterator.Current;
					}
				}
			}
		}

		private IEnumerable ExecuteCoroutine(UnityEngine.Object instance, System.Action onFinished) {
			if(eventList == null || eventList.Count == 0) {
				yield break;
			}
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i].block != null && eventList[i].eventType == EventActionData.EventType.Event) {
					IEnumerator iterator = eventList[i].block.ExecuteCoroutine(instance);
					while(iterator.MoveNext()) {
						yield return iterator.Current;
					}
				}
			}
			if(onFinished != null) {
				onFinished();
			}
		}

		private IEnumerable ExecuteCoroutineInParallel(UnityEngine.Object instance, System.Func<IEnumerator, Coroutine> startCoroutine) {
			if(eventList == null || eventList.Count == 0) {
				yield break;
			}
			List<Coroutine> coroutines = new List<Coroutine>();
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i].block != null && eventList[i].eventType == EventActionData.EventType.Event) {
					startCoroutine(eventList[i].block.ExecuteCoroutine(instance));
				}
			}
			for(int i = 0; i < coroutines.Count; i++) {
				yield return coroutines[i];
			}
		}

		private IEnumerable ExecuteCoroutineInParallel(UnityEngine.Object instance, System.Func<IEnumerator, Coroutine> startCoroutine, System.Action onFinished) {
			if(eventList == null || eventList.Count == 0) {
				yield break;
			}
			List<Coroutine> coroutines = new List<Coroutine>();
			for(int i = 0; i < eventList.Count; i++) {
				if(eventList[i].block != null && eventList[i].eventType == EventActionData.EventType.Event) {
					startCoroutine(eventList[i].block.ExecuteCoroutine(instance));
				}
			}
			for(int i = 0; i < coroutines.Count; i++) {
				yield return coroutines[i];
			}
			if(onFinished != null) {
				onFinished();
			}
		}
		#endregion

		#region Validation
		/// <summary>
		/// Validate validation.
		/// </summary>
		/// <returns></returns>
		public bool Validate(UnityEngine.Object instance) {
			//if(editIndex == -1) return true;
			if(eventList == null || eventList.Count == 0) {
				//editIndex = -1;
				return true;
			}
			bool hasFail = false;//to check that validation has fail.
			if(!useLevelValidation) {
				for(int i = 0; i < eventList.Count; i++) {
					EventActionData Event = eventList[i];
					if(!hasFail && Event.eventType == EventActionData.EventType.Event) {
#if UNITY_EDITOR
						try {
							if(!Event.block.Validate(instance)) {//do validate
								hasFail = true;
							}
						}
						catch {
							Debug.LogError("Error in validation : " + eventList[i].displayName + " in index : " + i);
							throw;
						}
#else
						if(!Event.block.Validate(instance)) {//do validate
							hasFail = true;
						}
#endif
					} else if(Event.eventType == EventActionData.EventType.Or) {
						if(hasFail && i + 1 == eventList.Count) {//if or event is in the end of array then stop it.
							break;
						} else if(hasFail) {//if prev validate has fail, execute next validate
							hasFail = false;
						} else {//if prev validate does't have fail then stop this.
							break;
						}
					}
				}
			} else {
				byte lastLevel = 0;
				for(int i = 0; i < eventList.Count; i++) {
					EventActionData Event = eventList[i];
					if(Event.levelValidation == 0) {
						if(Event.eventType == EventActionData.EventType.Event) {
#if UNITY_EDITOR
							try {
								if(!hasFail && !Event.block.Validate(instance)) {//do validate
									hasFail = true;
								}
							}
							catch {
								Debug.LogError("Error in validation : " + eventList[i].displayName + " in index : " + i);
								throw;
							}
#else
							if(!hasFail && !Event.block.Validate(instance)) {//do validate
								hasFail = true;
							}
#endif
						} else {//Or Statement
							if(hasFail) {//if or event is in the end of array then stop it.
								if(i + 1 == eventList.Count)
									return false;
								//if prev validate has fail, execute next validate
								hasFail = false;
							} else {//if prev validate does't have fail then stop this.
								break;
							}
						}
					} else {
						if(i == 0) {
							lastLevel = Event.levelValidation;
						} else if(lastLevel != Event.levelValidation && hasFail) {
							continue;
						}
						if(Event.eventType == EventActionData.EventType.Event) {
#if UNITY_EDITOR
							try {
								if(!hasFail && !Event.block.Validate(instance)) {//do validate
									hasFail = true;
								}
							}
							catch {
								Debug.LogError("Error in validation : " + eventList[i].displayName + " in index : " + i);
								throw;
							}
#else
							if(!hasFail && !Event.block.Validate(instance)) {//do validate
								hasFail = true;
							}
#endif
						} else {//Or Statement
							if(lastLevel == Event.levelValidation && !hasFail) {
								continue;
							}
							if(hasFail) {//if or event is in the end of array then stop it.
								if(i + 1 == eventList.Count)
									return false;
								//if prev validate has fail, execute next validate
								hasFail = false;
							} else {//if prev validate does't have fail then execute next validate.
								if(i + 1 == eventList.Count)
									break;
								continue;
							}
						}
					}
					lastLevel = Event.levelValidation;
				}
			}
			return !hasFail;
		}
		#endregion
		#region Constructor
		/// <summary>
		/// Create new EventData
		/// </summary>
		public EventData() { }

		/// <summary>
		/// Create new EventData with one action
		/// </summary>
		/// <param name="block"></param>
		public EventData(Block block) {
			eventList.Add(new EventActionData(block, EventActionData.EventType.Event));
		}

		/// <summary>
		/// Create new EventData with one event
		/// </summary>
		/// <param name="block"></param>
		/// <param name="type"></param>
		public EventData(Block block, EventActionData.EventType type) {
			eventList.Add(new EventActionData(block, type));
		}

		/// <summary>
		/// Create new EventData with multiple action
		/// </summary>
		/// <param name="block"></param>
		public EventData(params Block[] block) {
			foreach(Block a in block) {
				eventList.Add(new EventActionData(a));
			}
		}

		/// <summary>
		/// Create new EventData with multiple event
		/// </summary>
		/// <param name="eventAction"></param>
		public EventData(params EventActionData[] eventAction) {
			eventList.AddRange(eventAction);
		}
		#endregion
	}

	/// <summary>
	/// This is base class for all event
	/// </summary>
	public abstract class Block {
		protected UnityEngine.Object instance { get; private set; }
		//[HideInInspector]
		//[Tooltip("The name of this action.")]
		//public string Name;

		#region Action Method
		/// <summary>
		/// Called on executing action.
		/// </summary>
		protected virtual void OnExecute() {

		}

		public void Execute(UnityEngine.Object instance) {
			this.instance = instance;
			OnExecute();
		}

		/// <summary>
		/// Called on executing coroutine action.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ExecuteCoroutine() {
			OnExecute();
			yield break;
		}

		/// <summary>
		/// Execute an coroutine action, also handles action state (running or finished).
		/// </summary>
		/// <returns></returns>
		public IEnumerator ExecuteCoroutine(UnityEngine.Object instance) {
			this.instance = instance;
			state = false;
			var iterator = ExecuteCoroutine();
			while(iterator.MoveNext()) {
				yield return iterator.Current;
			}
			state = true;
		}

		/// <summary>
		/// Called on running action is stopped manually.
		/// </summary>
		protected virtual void OnStop() {

		}

		/// <summary>
		/// Stop running action.
		/// </summary>
		public void Stop(UnityEngine.Object instance) {
			this.instance = instance;
			if(state == false) {
				OnStop();
			}
		}

		/// <summary>
		/// The event state.
		/// null mean action is never run in coroutine.
		/// true mean the action is finished.
		/// false mean the action is still running.
		/// </summary>
		[HideInInspector]
		public bool? state = null;
		#endregion

		#region Validate Method
		protected virtual bool OnValidate() {
			return true;
		}

		public bool Validate(UnityEngine.Object instance) {
			this.instance = instance;
			return OnValidate();
		}
		#endregion

		#region Editor
		/// <summary>
		/// The name of block.
		/// </summary>
		public virtual string Name {
			get {
				//if(!string.IsNullOrEmpty(Name)) {
				//	return Name;
				//}
				System.Type type = this.GetType();
				if(type.IsDefined(typeof(BlockMenuAttribute), true)) {
					BlockMenuAttribute eventMenu = type.GetCustomAttributes(typeof(BlockMenuAttribute), true)[0] as BlockMenuAttribute;
					if(eventMenu != null && !string.IsNullOrEmpty(eventMenu.name)) {
						return eventMenu.name;
					}
				}
				return type.Name;
			}
		}

		/// <summary>
		/// Get rich name for the block
		/// </summary>
		/// <returns></returns>
		public virtual string GetRichName() => Name;

		/// <summary>
		/// The tooltip for the event.
		/// </summary>
		public virtual string ToolTip {
			get {
				return Name + "\n";
			}
		}

		/// <summary>
		/// Get the description for the event.
		/// </summary>
		/// <returns></returns>
		public virtual string GetDescription() {
			return null;
		}

		/// <summary>
		/// Function to check error on editor.
		/// </summary>
		/// <param name="owner"></param>
		public virtual void CheckError(Object owner) {

		}

		/// <summary>
		/// Override this function to implement code generation.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string GenerateCode(Object obj) {
			return null;
		}

		/// <summary>
		/// Override this function to implement code generation for coroutine action.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string GenerateCoroutineCode(Object obj) {
			return GenerateCode(obj);
		}

		/// <summary>
		/// Override this function to implement code generation for OnStop event.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string GenerateStopCode(Object obj) {
			return null;
		}

		/// <summary>
		/// Override this function to implement custom code generation for validation.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string GenerateConditionCode(Object obj) {
			if(this is Action && (this as Action).IsCoroutine()) {
				if(CG.generatorData.eventActions.ContainsKey(this)) {
					return CG.RunEvent(this);
				} else {
					string data = GenerateCode(obj);
					if(!string.IsNullOrEmpty(data)) {
						CG.generatorData.eventActions.Add(this, data);
						CG.generatorData.portableActionInNode.Add(obj as uNode.Node);
						return CG.RunEvent(this);
					}
				}
			} else {
				if(CG.generatorData.eventActions.ContainsKey(this)) {
					return CG.GetInvokeActionCode(this);
				} else {
					string data = GenerateCode(obj);
					if(!string.IsNullOrEmpty(data)) {
						CG.generatorData.eventActions.Add(this, data);
						CG.generatorData.portableActionInNode.Add(obj as uNode.Node);
						return CG.GetInvokeActionCode(this);
					}
				}
			}
			throw new System.NotImplementedException(this.GetType().FullName);
		}
		#endregion
	}

	/// <summary>
	/// This is class for all Missing event
	/// </summary>
	[System.Serializable]
	public class MissingEvent : Block {
		[System.NonSerialized, HideInInspector]
		public bool hasShowLog;
	}

	[System.Serializable]
	public class EventActionData : ISerializationCallbackReceiver {
		public enum EventType {
			Event,
			Or,
		}
		public EventType eventType = EventType.Event;
		public byte levelValidation;
		[HideInInspector]
		public bool expanded;

		/// <summary>
		/// The event object.
		/// </summary>
		[System.NonSerialized]
		public Block block;

		public static EventActionData OrEvent => new EventActionData(null, EventType.Or);

		/// <summary>
		/// Stop running action.
		/// </summary>
		public void StopAction(UnityEngine.Object instance) {
			if(block != null) {
				block.Stop(instance);
			}
		}

		public string GenerateCode(uNode.Node node, EventData.EventType type = EventData.EventType.Action) {
			if(eventType == EventType.Or)
				return null;
			return CG.GenerateEventCode(node, block, type);
		}

		public string GenerateStopCode(Object obj) {
			if(block != null) {
				return block.GenerateStopCode(obj);
			}
			return null;
		}

		public static implicit operator EventActionData(Block action) {
			return new EventActionData(action);
		}

		#region Editor
		/// <summary>
		/// The editor name of this event.
		/// </summary>
		public string displayName {
			get {
				if(eventType == EventType.Event && block != null) {
					return block.Name;
				}
				return "";
			}
		}

		/// <summary>
		/// Get rich name for the block.
		/// </summary>
		/// <returns></returns>
		public string GetRichName() {
			if(eventType == EventType.Event) {
				if(block != null) {
					return block.GetRichName();
				} else {
					return "[Block is Null]";
				}
			} else {
				return "OR";
			}
		}

		/// <summary>
		/// The tooltip of this event.
		/// </summary>
		public string toolTip {
			get {
				if(eventType == EventType.Event && block != null) {
					return block.ToolTip;
				}
				return "";
			}
		}

		/// <summary>
		/// Are this event is coroutine?
		/// </summary>
		public bool isCoroutine {
			get {
				return block != null && block is Action && (block as Action).IsCoroutine();
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Make or Event
		/// </summary>
		public EventActionData() {
			this.eventType = EventType.Or;
			this.block = null;
		}

		/// <summary>
		/// Make Event
		/// </summary>
		/// <param name="vEvent"></param>
		/// <param name="eventType"></param>
		public EventActionData(Block vEvent, EventType eventType = EventType.Event) {
			this.eventType = eventType;
			this.block = vEvent;
		}

		/// <summary>
		/// Make new Event
		/// </summary>
		/// <param name="data"></param>
		public EventActionData(EventActionData data) {
			this.levelValidation = data.levelValidation;
			if(data.eventType == EventType.Event) {
				this.eventType = data.eventType;
				string json = JsonUtility.ToJson(data.block);
				Block sData = JsonUtility.FromJson(json, data.block.GetType()) as Block;
				if(sData != null) {
					BeforeSerialize(sData);
					AfterSerialize();
				}
			} else {
				eventType = EventType.Or;
			}
		}

		#endregion

		#region Serialization Data
		[SerializeField]
		string typeName;
		[SerializeField]
		byte[] odinSerializedData;
		[SerializeField]
		List<Object> references = new List<Object>();

		public void OnBeforeSerialize() {
			#if UNITY_EDITOR
			Event e = Event.current;
			if(e != null && e.type != UnityEngine.EventType.Used &&
				(e.type == UnityEngine.EventType.Repaint ||
				e.type == UnityEngine.EventType.MouseDrag ||
				e.type == UnityEngine.EventType.Layout ||
				e.type == UnityEngine.EventType.ScrollWheel)) {
				return;
			}
			#endif
			BeforeSerialize(block);
		}

		public void OnAfterDeserialize() {
			AfterSerialize();
		}

		void BeforeSerialize(Block action) {
			if(action is MissingEvent) {
				return;
			}
			if(action != null) {
				System.Type type = action.GetType();
				typeName = type.FullName;
				odinSerializedData = SerializerUtility.Serialize(action, out references);
			}
		}

		void AfterSerialize() {
			try {
				System.Type type = null;
				if(!string.IsNullOrEmpty(typeName)) {
					type = TypeSerializer.Deserialize(typeName, false);
				}
				if(type != null) {
					block = SerializerUtility.Deserialize(odinSerializedData, references, type) as Block;
				} else if(eventType != EventType.Or) {
					block = new MissingEvent();
				}
			}
			catch(System.Exception ex) {
				Debug.LogException(ex);
			}
		}

		public string TypeName {
			get {
				return typeName;
			}
			set {
				typeName = value;
			}
		}
		#endregion
	}

	public class ActionCoroutine {
		private MonoBehaviour owner;
		private IEnumerable target;
		private IEnumerator iterator;
		private System.Func<IEnumerator, Coroutine> startCoroutine;
		private System.Action<Coroutine> stopCoroutine;
		private System.Action onStop;
		public Coroutine coroutine { get; private set; }

		public ActionCoroutine(MonoBehaviour owner, IEnumerable target) {
			this.target = target;
			this.owner = owner;
		}

		public ActionCoroutine(MonoBehaviour owner, IEnumerable target, System.Action onStop) {
			this.target = target;
			this.owner = owner;
			this.onStop = onStop;
		}

		public ActionCoroutine(IEnumerable target, System.Func<IEnumerator, Coroutine> startCoroutine, System.Action<Coroutine> stopCoroutine) {
			this.target = target;
			this.startCoroutine = startCoroutine;
			this.stopCoroutine = stopCoroutine;
		}

		public ActionCoroutine(IEnumerable target, System.Func<IEnumerator, Coroutine> startCoroutine, System.Action<Coroutine> stopCoroutine, System.Action onStop) {
			this.target = target;
			this.startCoroutine = startCoroutine;
			this.stopCoroutine = stopCoroutine;
			this.onStop = onStop;
		}

		private IEnumerator RunCoroutine() {
			while(iterator.MoveNext()) {
				yield return iterator.Current;
			}
			if(onStop != null) {
				onStop();
			}
		}

		public Coroutine Run() {
			if(coroutine == null) {
				iterator = target.GetEnumerator();
				if(owner != null) {
					coroutine = owner.StartCoroutine(RunCoroutine());
				} else {
					coroutine = startCoroutine(RunCoroutine());
				}
			}
			return coroutine;
		}

		public void Stop() {
			if(coroutine != null) {
				if(owner != null) {
					owner.StopCoroutine(coroutine);
				} else {
					stopCoroutine(coroutine);
				}
				coroutine = null;
				if(onStop != null) {
					onStop();
				}
			}
		}
	}
}