using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	[ControlField(typeof(Enum))]
	public class EnumControl : ValueControl {
		public EnumControl(ControlConfig config, bool autoLayout = false) : base(config, autoLayout) {
			Init();
		}

		void Init() {
			EnumField field = new EnumField(config.value as Enum ?? ReflectionUtils.CreateInstance(config.type) as Enum);
			field.RegisterValueChangedCallback((e) => {
				config.OnValueChanged(e.newValue);
				MarkDirtyRepaint();
			});
			Add(field);
		}
	}
}