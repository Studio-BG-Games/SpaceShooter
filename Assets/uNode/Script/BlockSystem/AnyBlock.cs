namespace MaxyGames.Events {
	/// <summary>
	/// Base class for any event.
	/// This sub class type will auto add in the event menu.
	/// </summary>
	public abstract class AnyBlock : Block {
		protected override bool OnValidate() {
			OnExecute();
			return true;
		}
	}
}