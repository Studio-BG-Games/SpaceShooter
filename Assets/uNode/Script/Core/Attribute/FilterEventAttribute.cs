using System;

namespace MaxyGames {
	[System.AttributeUsage(AttributeTargets.Field)]
	public class EventTypeAttribute : Attribute {
		public EventData.EventType eventType;
		public Type[] type;
		public bool includeSubClass = true;
		public bool supportCoroutine;

		public EventTypeAttribute(EventData.EventType eventType, params Type[] type) {
			this.eventType = eventType;
			this.type = type;
		}
	}
}