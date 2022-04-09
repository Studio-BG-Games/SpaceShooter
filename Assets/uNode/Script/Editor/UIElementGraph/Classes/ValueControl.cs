using System;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	public abstract class ValueControl : ImmediateModeElement {
		public readonly ControlConfig config;

		public ValueControl(ControlConfig config, bool autoLayout = false) {
			if(config != null && config.filter == null && config.type != null) {
				config.filter = new FilterAttribute(config.type);
			}
			this.config = config;
			EnableInClassList("Layout", autoLayout);
		}

		protected override void ImmediateRepaint() {

		}
	}

	[Serializable]
	public class ControlConfig {
		public UNodeView owner;
		public object value;
		public Type type;
		public FilterAttribute filter;
		public Action<object> onValueChanged { private get; set; }

		public void OnValueChanged(object value) {
			//if(value is MemberData member && member.targetType == MemberData.TargetType.uNodeType) {
			//	var instanced = owner.targetNode.owner;
			//	var type = member.startType;
			//	if(type is RuntimeGraphType runtimeType && GraphUtility.IsTempGraphObject(instanced.gameObject)) {
			//		var prefab = GraphUtility.GetOriginalObject(instanced.gameObject);
			//		var graphs = prefab.GetComponents<uNodeRoot>();
			//		for(int i=0;i<graphs.Length;i++) {
			//			if(runtimeType.target == graphs[i]) {
			//				var temp = GraphUtility.GetTempGraphObject(graphs[i]);
			//				if(temp != null) {
			//					member.CopyFrom(new MemberData(ReflectionUtils.GetRuntimeType(temp));
			//				}
			//			}
			//		}
			//	}
			//}
			onValueChanged?.Invoke(value);
			this.value = value;
			uNodeEditor.GUIChanged(owner.targetNode);
		}
	}
}