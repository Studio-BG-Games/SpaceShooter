using System;

namespace MaxyGames.uNode {
	/// <summary>
	/// Used to show menu item for Node
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Class)]
	public class NodeMenu : Attribute {
		/// <summary>
		/// The menu name
		/// </summary>
		public string name;
		/// <summary>
		/// The category of menu
		/// </summary>
		public string category;
		/// <summary>
		/// The tooltip of menu
		/// </summary>
		public string tooltip;

		public Type type;
		public Type returnType = typeof(object);

		/// <summary>
		/// If true, this node will hide on non coroutine graph.
		/// </summary>
		public bool IsCoroutine { get; set; }
		/// <summary>
		/// If true, hide menu in flow.
		/// </summary>
		public bool HideOnFlow { get; set; }
		/// <summary>
		/// If true, hide menu in group.
		/// </summary>
		public bool HideOnGroup { get; set; }
		/// <summary>
		/// If true, hide menu in StateMachine.
		/// </summary>
		public bool HideOnStateMachine { get; set; }
		/// <summary>
		/// The order index to sort the menu, default is 0.
		/// </summary>
		public int order { get; set; }

		public NodeMenu(string category, string name) {
			this.category = category;
			this.name = name;
		}

		public NodeMenu(string category, string name, Type returnType) {
			this.category = category;
			this.name = name;
			this.returnType = returnType;
		}
	}

	[System.AttributeUsage(AttributeTargets.Class)]
	public class EventMenuAttribute : Attribute {
		/// <summary>
		/// The menu name
		/// </summary>
		public string name;
		/// <summary>
		/// The category of menu
		/// </summary>
		public string category;
		/// <summary>
		/// The order index to sort the menu, default is 0.
		/// </summary>
		public int order { get; set; }

		public Type type;

		public EventMenuAttribute(string category, string name) {
			this.category = category;
			this.name = name;
		}
	}
}
