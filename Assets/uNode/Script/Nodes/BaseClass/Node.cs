using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Base class for all node.
	/// </summary>
	public abstract class Node : NodeComponent {
		#region Parameters
		/// <summary>
		/// Get/Set the current state
		/// </summary>
		[System.NonSerialized]
		protected StateType state;
		/// <summary>
		/// Are this event is finished
		/// </summary>
		[System.NonSerialized]
		protected bool finished;
		/// <summary>
		/// Are this event has called
		/// </summary>
		[System.NonSerialized]
		protected bool hasCalled;
		public StateType currentState {
			get {
				if(!finished && hasCalled) {
					return StateType.Running;
				}
				return state;
			}
		}
		#endregion

		#region Functions
		/// <summary>
		/// Called when this node is called
		/// </summary>
		public virtual void OnExecute() { }

		/// <summary>
		/// Call This to activate event
		/// </summary>
		public virtual void Activate() {
			if(hasCalled && !IsFinished())
				return;
			jumpState = null;
			if(!hasCalled)
				hasCalled = true;
			finished = false;
			state = StateType.Running;
			if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
				GraphDebug.FlowNode(owner, owner.GetInstanceID(), this.GetInstanceID(), state);
			}
			try {
				OnExecute();
			}
			catch(System.Exception ex) {
				//Ensure the state stop on error.
				if(state == StateType.Running)
					state = StateType.Failure;
				//finished = true;
				Finish();
				if(ex is uNodeException) {
					throw;
				}
				throw uNodeDebug.LogException(ex, this);
			}
		}

		#region Value Functions
		/// <summary>
		/// Get value of the node
		/// </summary>
		/// <returns></returns>
		public virtual object GetValue() {
			throw uNodeDebug.LogException(new System.NotImplementedException(), this);
		}

		/// <summary>
		/// The type of value will return.
		/// </summary>
		/// <returns></returns>
		public virtual System.Type ReturnType() {
			return typeof(void);
		}

		/// <summary>
		/// Are this node have value that can be get
		/// </summary>
		/// <returns></returns>
		public virtual bool CanGetValue() {
			return false;
		}

		/// <summary>
		/// Are this node have value that can be set
		/// </summary>
		/// <returns></returns>
		public virtual bool CanSetValue() {
			return false;
		}

		/// <summary>
		/// Set value of the node
		/// </summary>
		/// <param name="value"></param>
		public virtual void SetValue(object value) {
			throw uNodeDebug.LogException(new System.NotImplementedException(), this);
		}
		#endregion
		#region Flow Functions
		/// <summary>
		/// Are this node is flow node?
		/// </summary>
		/// <returns></returns>
		public virtual bool IsFlowNode() {
			return true;
		}

		/// <summary>
		/// Execute flow node and handle jump state and coroutine nodes.
		/// Note: this will call Finish() in end.
		/// </summary>
		/// <param name="flowNodes"></param>
		protected void ExecuteFlow(params MemberData[] flowNodes) {
			ExecuteFlow(flowNodes, delegate () {
				Finish();
			});
		}

		/// <summary>
		/// Execute flow node and handle jump state and coroutine nodes.
		/// </summary>
		/// <param name="onFinish"></param>
		/// <param name="flowNodes"></param>
		protected void ExecuteFlow(System.Action onFinish, params MemberData[] flowNodes) {
			ExecuteFlow(flowNodes, onFinish);
		}

		/// <summary>
		/// Execute flow node and handle jump state and coroutine nodes.
		/// </summary>
		/// <param name="flowNodes"></param>
		/// <param name="onFinish"></param>
		/// <param name="handleCoroutine"></param>
		protected void ExecuteFlow(IList<MemberData> flowNodes, System.Action onFinish, bool handleCoroutine = true) {
			for(int i = 0; i < flowNodes.Count; i++) {
				MemberData flow = flowNodes[i];
				if(flow == null || !flow.isAssigned)
					continue;
				Node n;
				if(!flow.ActivateFlowNode(out n)) {//Activate flow node and check if the node is not finished.
					if(handleCoroutine) {//Wait when handleCoroutine is true
						owner.StartCoroutine(ExecuteFlowCoroutine(flowNodes, i, onFinish), this);
						return;
					}
				}
				if(n != null) {
					jumpState = n.GetJumpState();
					if(jumpState != null) {
						break;
					}
				}
			}
			if(onFinish != null)
				onFinish();
		}

		/// <summary>
		/// Used for execute coroutine flow nodes.
		/// </summary>
		/// <param name="flowNodes"></param>
		/// <param name="startIndex"></param>
		/// <param name="onFinish"></param>
		/// <returns></returns>
		private IEnumerator ExecuteFlowCoroutine(IList<MemberData> flowNodes, int startIndex, System.Action onFinish) {
			yield return flowNodes[startIndex].WaitFlowNode();
			for(int i = startIndex + 1; i < flowNodes.Count; i++) {
				MemberData flow = flowNodes[i];
				if(flow == null || !flowNodes[i].isAssigned)
					continue;
				Node n;
				WaitUntil w;
				if(!flowNodes[i].ActivateFlowNode(out n, out w)) {
					yield return w;
				}
				if(n == null)//Skip on executing flow input pin.
					continue;
				jumpState = n.GetJumpState();
				if(jumpState != null) {
					break;
				}
			}
			if(onFinish != null)
				onFinish();
		}

		/// <summary>
		/// Execute flow node and finish this node.
		/// </summary>
		/// <param name="flowNode"></param>
		protected void Finish(MemberData flowNode) {
			if(flowNode != null && flowNode.isAssigned) {
				Node n;
				WaitUntil w;
				if(!flowNode.ActivateFlowNode(out n, out w)) {
					WaitAndExecute(w, delegate () {
						if(n != null) {
							jumpState = n.GetJumpState();
						}
						Finish();
					});
					return;
				}
				if(n != null) {
					jumpState = n.GetJumpState();
				}
			}
			Finish();
		}

		/// <summary>
		/// Execute flow node and finish this node.
		/// </summary>
		/// <param name="flowNodes"></param>
		protected void Finish(IList<MemberData> flowNodes, bool handleCoroutine = true) {
			ExecuteFlow(flowNodes,
			delegate () {
				Finish();
			}, handleCoroutine);
		}

		/// <summary>
		/// Execute flow node and finish this node.
		/// </summary>
		/// <param name="flowNodes"></param>
		protected void Finish(params MemberData[] flowNodes) {
			ExecuteFlow(flowNodes,
			delegate () {
				Finish();
			});
		}
		#endregion

		/// <summary>
		/// Return true of flow member is coroutine.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		protected bool HasCoroutineInFlow(MemberData member) {
			if(member != null && member.isAssigned) {
				var node = member.GetTargetNode();
				if(node != null) {
					return node.IsCoroutine();
				}
			}
			return false;
		}

		/// <summary>
		/// Return true if any flow member is coroutine.
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		protected bool HasCoroutineInFlow(params MemberData[] members) {
			if(members != null) {
				for(int i = 0; i < members.Length; i++) {
					if(HasCoroutineInFlow(members[i])) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Return true if any flow member is coroutine. 
		/// </summary>
		/// <param name="members"></param>
		/// <returns></returns>
		protected bool HasCoroutineInFlow(IList<MemberData> members) {
			if(members != null) {
				for(int i = 0; i < members.Count; i++) {
					if(HasCoroutineInFlow(members[i])) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Wait and Call action.
		/// </summary>
		/// <param name="waitObject"></param>
		/// <param name="action"></param>
		protected void WaitAndExecute(object waitObject, System.Action action) {
			owner.StartCoroutine(WaitAndExecuteCoroutine(waitObject, action), this);
		}

		private IEnumerator WaitAndExecuteCoroutine(object waitObject, System.Action action) {
			yield return waitObject;
			if(action != null)
				action();
		}

		/// <summary>
		/// Activate Node and Find Jump State Condition.
		/// </summary>
		/// <returns></returns>
		public virtual JumpStatement ActivateAndFindJumpState() {
			Activate();
			return GetJumpState();
		}

		[System.NonSerialized]
		protected JumpStatement jumpState;

		/// <summary>
		/// Get jump state condition from this node.
		/// </summary>
		/// <returns></returns>
		public JumpStatement GetJumpState() {
			return jumpState;
		}

		/// <summary>
		/// Call this function to finish the node.
		/// </summary>
		public virtual void Finish() {
			finished = true;
			if(state == StateType.Running) {
				state = StateType.Success;
			}
			if(Application.isEditor) {
				GraphDebug.FlowNode(owner, owner.GetInstanceID(), this.GetInstanceID(), state);
			}
		}

		/// <summary>
		/// Are this event is finished.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsFinished() {
			return finished && state != StateType.Running;
		}

		/// <summary>
		/// Wait this event until finished.
		/// </summary>
		/// <returns></returns>
		public WaitUntil WaitUntilFinish() {
			return new WaitUntil(() => IsFinished());
		}
		#endregion

		#region Code Generator
		/// <summary>
		/// Override this function to add C# script generation on the flow node.
		/// If this function is not override the node will not have script generation support
		/// and will throw an exception when you want generating a script.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string GenerateCode() {
			throw new System.NotImplementedException(this.GetType().FullName);
		}

		/// <summary>
		/// Override this function to add C# script generation on the value node.
		/// If this function is not override the node will not have script generation support
		/// and will throw an exception when you want generating a script.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual string GenerateValueCode() {
			throw new System.NotImplementedException(this.GetType().FullName);
		}
		#endregion

		#region Others
		/// <summary>
		/// Forced terminate the node and its transition, the finish state will failure and no transition will be call
		/// </summary>
		public virtual void Stop() {
			//Check if this node is still running
			if(state == StateType.Running) {
				//Mark this node to finish
				finished = true;
				//Change state to failure.
				state = StateType.Failure;
				owner.StopAllCoroutines(this);
				if(uNodeUtility.isInEditor && GraphDebug.useDebug) {
					GraphDebug.FlowNode(owner, owner.GetInstanceID(), this.GetInstanceID(), state);
				}
			}
		}
		#endregion

		#region Editor
		/// <summary>
		/// Function to check error on editor.
		/// </summary>
		public override void CheckError() {
			if(IsSelfCoroutine()) {//Check this node if is coroutine.
				var pComp = parentComponent;
				if(pComp != null) {
					if(pComp as RootObject) {
						if(!(pComp as RootObject).CanHaveCoroutine()) {
							RegisterEditorError("The current graph doesn't allow coroutine nodes.");
						}
					} else if(pComp is ISuperNode) {
						if(!(pComp as ISuperNode).AcceptCoroutine()) {
							RegisterEditorError("The current graph doesn't allow coroutine nodes.");
						}
					}
				}
			}
		}

		/// <summary>
		/// Get the event name for show in title of node.
		/// </summary>
		/// <returns></returns>
		public override string GetNodeName() {
			if(GetType().IsDefined(typeof(NodeMenu), true)) {
				return (GetType().GetCustomAttributes(typeof(NodeMenu), true)[0] as NodeMenu).name;
			}
			return GetType().Name;
		}

		public override Type GetNodeIcon() {
			return ReturnType();
		}
		#endregion
	}

	/// <summary>
	/// Base class for node that is not flow or value node.
	/// </summary>
	public abstract class CustomNode : Node {
		public override bool IsFlowNode() {
			return false;
		}
	}
}