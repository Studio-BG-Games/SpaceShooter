namespace MaxyGames.Events {
	/// <summary>
	/// Base class for all action.
	/// This sub class type will auto add in event menu on action event.
	/// </summary>
	public abstract class Action : Block {
		protected override bool OnValidate() {
			OnExecute();
			return true;
		}

		/// <summary>
		/// Check whether action is coroutine or not.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsCoroutine() {
			return false;
		}
	}
}