using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[NodeMenu("Data", "Condition", typeof(bool))]
	[AddComponentMenu("")]
	public class ConditionNode : ValueNode {
		[EventType(EventData.EventType.Condition)]
		public EventData Condition = new EventData();

		public override System.Type ReturnType() {
			return typeof(bool);
		}

		protected override object Value() {
			return Condition.Validate(owner);
		}

		public override string GenerateValueCode() {
			return Condition.GenerateCode(this, EventData.EventType.Condition);
		}

		public override string GetNodeName() {
			return "Condition";
		}

		public override void CheckError() {
			base.CheckError();
			if(Condition != null)
				Condition.CheckError(this);
		}
	}
}