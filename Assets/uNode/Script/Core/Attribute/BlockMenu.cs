using System;

namespace MaxyGames.Events {
	/// <summary>
	/// Used for show menu item for block (action or validation)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class BlockMenuAttribute : Attribute {
		/// <summary>
		/// The category for menu.
		/// </summary>
		public string category;
		/// <summary>
		/// The name of this block.
		/// </summary>
		public string name;
		/// <summary>
		/// Are the block only can be executed on coroutine?
		/// </summary>
		public bool isCoroutine;

		/// <summary>
		/// Hide the menu for block system.
		/// </summary>
		public bool hideOnBlock = false;

		public Type type;
		
		public BlockMenuAttribute(string category, string name, bool isCoroutine = false) {
			this.category = category;
			this.name = name;
			this.isCoroutine = isCoroutine;
		}
	}
}
