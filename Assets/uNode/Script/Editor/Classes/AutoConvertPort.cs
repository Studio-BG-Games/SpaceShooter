using UnityEngine;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// Base class for auto convert port.
	/// </summary>
	public abstract class AutoConvertPort {
		public FilterAttribute filter;

		public bool force;

		public System.Func<MemberData> getConnection;

		public System.Type leftType;
		public System.Type rightType;

		public uNodeRoot graph;
		public Transform parent;
		public Vector2 position;

		public virtual int order { get { return 0; } }

		public abstract bool IsValid();
		public abstract bool CreateNode(System.Action<Node> action);

		protected MemberData GetConnection() {
			return getConnection?.Invoke();
		}
	}
}