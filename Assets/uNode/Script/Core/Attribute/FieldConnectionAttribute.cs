using System;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Attribute for show field connection in graph editor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public abstract class FieldConnectionAttribute : Attribute {
		/// <summary>
		/// The label of the connection
		/// </summary>
		public GUIContent label;
		/// <summary>
		/// Hide connection if node is flow node.
		/// </summary>
		public bool hideOnFlowNode;
		/// <summary>
		/// Hide connection if node is not flow node.
		/// </summary>
		public bool hideOnNotFlowNode;

		public FieldConnectionAttribute() {
			label = GUIContent.none;
		}

		public FieldConnectionAttribute(string label) {
			if(label == null) {
				this.label = GUIContent.none;
			} else {
				this.label = new GUIContent(label, string.Empty);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class FlowOutAttribute : FieldConnectionAttribute {
		public bool finishedFlow;
		public bool localFunction;
		public bool displayFlowInHierarchy = true;

		public FlowOutAttribute() : base() { }

		public FlowOutAttribute(string label) : base(label) { }

		public FlowOutAttribute(string label, bool finishedFlow) : base(label) {
			this.finishedFlow = finishedFlow;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class ValueInAttribute : FieldConnectionAttribute {
		public ValueInAttribute() : base() { }

		public ValueInAttribute(string label) : base(label) { }
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class ValueOutAttribute : Attribute {
		/// <summary>
		/// The field label
		/// </summary>
		public GUIContent label = GUIContent.none;
		public Type type;
		public bool isInstance;

		public ValueOutAttribute(Type type = null) {
			this.type = type;
		}

		public ValueOutAttribute(string label, Type type = null) {
			this.label = new GUIContent(label, string.Empty);
			this.type = type;
		}
	}
}