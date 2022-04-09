namespace MaxyGames.Events {
	/// <summary>
	/// Base class for all coroutine action.
	/// </summary>
	public abstract class CoroutineAction : Action {
		protected override void OnExecute() {
			throw new System.Exception("Calling OnExecute() on CoroutineAction is prohibited.");
		}

		public override bool IsCoroutine() {
			return true;
		}
	}
}