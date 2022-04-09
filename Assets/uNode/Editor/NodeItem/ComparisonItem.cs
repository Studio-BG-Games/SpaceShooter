using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.Commands {
	public class EqualComparisonItem : CreateNodeCommand<ComparisonNode> {
		public override string name {
			get {
				return "Equal (==)";
			}
		}

		public override string category {
			get {
				return "Compare";
			}
		}

		public override System.Type icon => typeof(TypeIcons.Equal);

		protected override void OnNodeCreated(ComparisonNode node) {
			node.operatorType = ComparisonType.Equal;
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type == typeof(bool);
		}
	}

	public class NotEqualComparisonItem : CreateNodeCommand<ComparisonNode> {
		public override string name {
			get {
				return "NotEqual (!=)";
			}
		}

		public override string category {
			get {
				return "Compare";
			}
		}

		public override System.Type icon => typeof(TypeIcons.NotEqual);

		protected override void OnNodeCreated(ComparisonNode node) {
			node.operatorType = ComparisonType.NotEqual;
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type == typeof(bool);
		}
	}

	public class GreaterThanComparisonItem : CreateNodeCommand<ComparisonNode> {
		public override string name {
			get {
				return "GreaterThan (>)";
			}
		}

		public override string category {
			get {
				return "Compare";
			}
		}

		public override System.Type icon => typeof(TypeIcons.GreaterThan);

		protected override void OnNodeCreated(ComparisonNode node) {
			node.operatorType = ComparisonType.GreaterThan;
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type == typeof(bool);
		}
	}

	public class GreaterThanOrEqualComparisonItem : CreateNodeCommand<ComparisonNode> {
		public override string name {
			get {
				return "GreaterThanOrEqual (>=)";
			}
		}

		public override string category {
			get {
				return "Compare";
			}
		}

		public override System.Type icon => typeof(TypeIcons.GreaterThanOrEqual);

		protected override void OnNodeCreated(ComparisonNode node) {
			node.operatorType = ComparisonType.GreaterThanOrEqual;
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type == typeof(bool);
		}
	}

	public class LessThanComparisonItem : CreateNodeCommand<ComparisonNode> {
		public override string name {
			get {
				return "LessThan (<)";
			}
		}

		public override string category {
			get {
				return "Compare";
			}
		}

		public override System.Type icon => typeof(TypeIcons.LessThan);

		protected override void OnNodeCreated(ComparisonNode node) {
			node.operatorType = ComparisonType.LessThan;
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type == typeof(bool);
		}
	}

	public class LessThanOrEqualComparisonItem : CreateNodeCommand<ComparisonNode> {
		public override string name {
			get {
				return "LessThanOrEqual (<=)";
			}
		}

		public override string category {
			get {
				return "Compare";
			}
		}

		public override System.Type icon => typeof(TypeIcons.LessThanOrEqual);

		protected override void OnNodeCreated(ComparisonNode node) {
			node.operatorType = ComparisonType.LessThanOrEqual;
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type == typeof(bool);
		}
	}
}