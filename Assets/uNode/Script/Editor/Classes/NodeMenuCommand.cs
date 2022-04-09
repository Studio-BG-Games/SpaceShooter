using UnityEngine;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// Base class for all custom node command.
	/// </summary>
	public abstract class NodeMenuCommand {
		public NodeGraph graph;
		public Vector2 mousePositionOnCanvas;

		public abstract string name { get; }
		public virtual int order { get { return 0; } }
		public abstract void OnClick(Node source, Vector2 mousePosition);
		public virtual bool IsValidNode(Node source) {
			return true;
		}
	}

	/// <summary>
	/// Base class for all custom input port items.
	/// </summary>
	public abstract class CustomInputPortItem {
		public NodeGraph graph;
		public Vector2 mousePositionOnCanvas;
		
		public virtual int order { get { return 0; } }
		public virtual bool IsValidPort(System.Type type) {
			return true;
		}

		public virtual bool IsValidPort(System.Type type, PortAccessibility accessibility) {
			return accessibility != PortAccessibility.OnlySet && IsValidPort(type);
		}

		public abstract System.Collections.Generic.IList<ItemSelector.CustomItem> GetItems(Node source, MemberData data, System.Type type);
	}

	/// <summary>
	/// Base class for all custom graph command.
	/// </summary>
	public abstract class GraphMenuCommand {
		public NodeGraph graph;
		public Vector2 mousePositionOnCanvas;

		public abstract string name { get; }
		public virtual int order { get { return 0; } }
		public abstract void OnClick(Vector2 mousePosition);
		public virtual bool IsValid() {
			return true;
		}
	}
}