using System;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.Commands {
	public class AddArithmeticItem : CreateNodeCommand<MultiArithmeticNode> {
		public override string name {
			get {
				return "Add (+)";
			}
		}

		public override string category {
			get {
				return "Math";
			}
		}

		public override System.Type icon => typeof(TypeIcons.AddIcon2);

		protected override void OnNodeCreated(MultiArithmeticNode node) {
			node.operatorType = ArithmeticType.Add;
			if(filter != null) {
				var type = filter.GetActualType();
				if(type != typeof(object)) {
					node.targets[0] = new MemberData(ReflectionUtils.CreateInstance(type));
					node.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
					return;
				}
			}
			node.targets[0] = new MemberData(0f);
			node.targets[1] = new MemberData(0f);
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type.IsPrimitive && type != typeof(bool) && type != typeof(char);
		}
	}

	public class DivideArithmeticItem : CreateNodeCommand<MultiArithmeticNode> {
		public override string name {
			get {
				return "Divide (/)";
			}
		}

		public override string category {
			get {
				return "Math";
			}
		}

		public override System.Type icon => typeof(TypeIcons.DivideIcon2);

		protected override void OnNodeCreated(MultiArithmeticNode node) {
			node.operatorType = ArithmeticType.Divide;
			if(filter != null) {
				var type = filter.GetActualType();
				if(type != typeof(object)) {
					node.targets[0] = new MemberData(ReflectionUtils.CreateInstance(type));
					node.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
					return;
				}
			}
			node.targets[0] = new MemberData(1f);
			node.targets[1] = new MemberData(1f);
		}


		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type.IsPrimitive && type != typeof(bool) && type != typeof(char);
		}
	}

	public class ModuloArithmeticItem : CreateNodeCommand<MultiArithmeticNode> {
		public override string name {
			get {
				return "Modulo (%)";
			}
		}

		public override string category {
			get {
				return "Math";
			}
		}

		public override System.Type icon => typeof(TypeIcons.ModuloIcon2);

		protected override void OnNodeCreated(MultiArithmeticNode node) {
			node.operatorType = ArithmeticType.Modulo;
			if(filter != null) {
				var type = filter.GetActualType();
				if(type != typeof(object)) {
					node.targets[0] = new MemberData(ReflectionUtils.CreateInstance(type));
					node.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
					return;
				}
			}
			node.targets[0] = new MemberData(1f);
			node.targets[1] = new MemberData(1f);
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type.IsPrimitive && type != typeof(bool) && type != typeof(char);
		}
	}

	public class MultiplyArithmeticItem : CreateNodeCommand<MultiArithmeticNode> {
		public override string name {
			get {
				return "Multiply (*)";
			}
		}

		public override string category {
			get {
				return "Math";
			}
		}

		public override System.Type icon => typeof(TypeIcons.MultiplyIcon2);

		protected override void OnNodeCreated(MultiArithmeticNode node) {
			node.operatorType = ArithmeticType.Multiply;
			if(filter != null) {
				var type = filter.GetActualType();
				if(type != typeof(object)) {
					node.targets[0] = new MemberData(ReflectionUtils.CreateInstance(type));
					node.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
					return;
				}
			}
			node.targets[0] = new MemberData(1f);
			node.targets[1] = new MemberData(1f);
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type.IsPrimitive && type != typeof(bool) && type != typeof(char);
		}
	}

	public class SubtractArithmeticItem : CreateNodeCommand<MultiArithmeticNode> {
		public override string name {
			get {
				return "Subtract (-)";
			}
		}

		public override string category {
			get {
				return "Math";
			}
		}

		public override System.Type icon => typeof(TypeIcons.SubtractIcon2);

		protected override void OnNodeCreated(MultiArithmeticNode node) {
			node.operatorType = ArithmeticType.Subtract;
			if(filter != null) {
				var type = filter.GetActualType();
				if(type != typeof(object)) {
					node.targets[0] = new MemberData(ReflectionUtils.CreateInstance(type));
					node.targets[1] = new MemberData(ReflectionUtils.CreateInstance(type));
					return;
				}
			}
			node.targets[0] = new MemberData(1f);
			node.targets[1] = new MemberData(1f);
		}

		public override bool IsValid() {
			if(filter == null) {
				return true;
			}
			var type = filter.GetActualType();
			if(type == null)
				return true;
			return type == typeof(object) || type.IsPrimitive && type != typeof(bool) && type != typeof(char);
		}
	}
}