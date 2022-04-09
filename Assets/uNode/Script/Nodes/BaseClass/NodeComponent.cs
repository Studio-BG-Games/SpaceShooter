using System.Collections;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// NodeComponent is a base class for Node.
	/// </summary>
	public abstract class NodeComponent : uNodeComponent, INode<uNodeRoot> {
		[HideInInspector]
		public string comment;

		[HideInInspector, System.NonSerialized]
		private uNodeComponent m_parentComponent;

		/// <summary>
		/// The Owner of this node
		/// </summary>
		[Hide]
		public uNodeRoot owner;
		[Hide]
		public Rect editorRect = Rect.zero;
		[Hide]
		public bool nodeExpanded = true;

		private IGraphWithUnityEvent _runtime_uNode;
		/// <summary>
		/// Target runtime uNode.
		/// </summary>
		public IGraphWithUnityEvent runtimeUNode {
			get {
				if(_runtime_uNode as Object != owner) {
					_runtime_uNode = owner as IGraphWithUnityEvent;
				}
				return _runtime_uNode;
			}
		}

		/// <summary>
		/// Are this node is coroutine
		/// </summary>
		/// <returns></returns>
		public virtual bool IsSelfCoroutine() {
			return false;
		}

		/// <summary>
		/// Are this node or the flow on this node is coroutine
		/// </summary>
		/// <returns></returns>
		public virtual bool IsCoroutine() {
			return IsSelfCoroutine();
		}

		/// <summary>
		/// Called on first time uNode is initialized
		/// </summary>
		public virtual void OnRuntimeInitialize() {

		}

		/// <summary>
		/// Called on first time when initializing Code Generator
		/// </summary>
		public virtual void OnGeneratorInitialize() {

		}

		#region Editor
		/// <summary>
		/// Get the node icon.
		/// </summary>
		/// <returns></returns>
		public virtual System.Type GetNodeIcon() {
			return typeof(UnityEngine.Object);
		}

		/// <summary>
		/// Function to check error on editor.
		/// </summary>
		public virtual void CheckError() {
			
		}

		/// <summary>
		/// Register editor error.
		/// </summary>
		/// <param name="message"></param>
		public void RegisterEditorError(string message) {
			uNodeUtility.RegisterEditorError(this, message);
		}

		/// <summary>
		/// Get node name
		/// </summary>
		/// <returns></returns>
		public virtual string GetNodeName() {
			return gameObject.name;
		}

		/// <summary>
		/// Get node name with rich text
		/// </summary>
		/// <returns></returns>
		public virtual string GetRichName() => GetNodeName();
		#endregion

		/// <summary>
		/// Start a Coroutine.
		/// </summary>
		/// <param name="routine"></param>
		/// <returns></returns>
		public new Coroutine StartCoroutine(IEnumerator routine) {
			return owner.StartCoroutine(routine, this);
		}

		[System.Obsolete("Use StartCoroutine(IEnumerator) instead", true)]
		public new Coroutine StartCoroutine(string methodName) {
			throw new System.Exception("Use StartCoroutine(IEnumerator) instead");
		}

		[System.Obsolete("Use StartCoroutine(IEnumerator) instead", true)]
		public new Coroutine StartCoroutine(string methodName, object value) {
			throw new System.Exception("Use StartCoroutine(IEnumerator) instead");
		}

		/// <summary>
		/// Stop the coroutine.
		/// </summary>
		/// <param name="routine"></param>
		public new void StopCoroutine(Coroutine routine) {
			owner.StopCoroutine(routine);
		}

		/// <summary>
		/// Stop the coroutine.
		/// </summary>
		/// <param name="routine"></param>
		public new void StopCoroutine(IEnumerator routine) {
			owner.StopCoroutine(routine);
		}

		[System.Obsolete("Use StopCoroutine(IEnumerator) instead", true)]
		public new void StopCoroutine(string methodName) {
			throw new System.Exception("Use StopCoroutine(IEnumerator) instead");
		}

		/// <summary>
		/// Stops all coroutines running on this behaviour
		/// </summary>
		public new void StopAllCoroutines() {
			owner.StopAllCoroutines(this);
		}

		/// <summary>
		/// Get the owner of this node.
		/// </summary>
		/// <returns></returns>
		public uNodeRoot GetOwner() {
			return owner;
		}

		INodeRoot INode.GetNodeOwner() {
			return GetOwner();
		}

		
		/// <summary>
		/// Get parent uNode component.
		/// </summary>
		public uNodeComponent parentComponent {
			get {
				if(m_parentComponent != null) {
					return m_parentComponent;
				}
				var tr = transform;
				if(tr != null && tr.parent != null) {
					return tr.parent.GetComponent<uNodeComponent>();
				}
				return null;
			}
			set {
				m_parentComponent = value; //Manually set parent component.
			}
		}

		/// <summary>
		/// Get parent root, null if this node controlled by state machine.
		/// </summary>
		public RootObject rootObject {
			get {
				if(this != null) {
					Transform p = transform.parent;
					while(p != null) {
						RootObject r = p.GetComponent<RootObject>();
						if(r != null) {
							return r;
						}
						p = p.transform.parent;
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Are this node is in root
		/// </summary>
		public bool IsInRoot {
			get {
				if(!uNodeUtility.isPlaying || runtimeUNode == null) {
					return parentComponent == null;
				}
				if(owner.RootObject) {
					return owner.RootObject.transform == transform.parent;
				}
				return false;
			}
		}
	}
}