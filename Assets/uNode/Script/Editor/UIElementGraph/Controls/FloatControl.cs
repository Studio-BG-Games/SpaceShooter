﻿using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	[ControlField(typeof(float))]
	public class FloatControl : ValueControl {
		public FloatControl(ControlConfig config, bool autoLayout = false) : base(config, autoLayout) {
			Init();
		}

		void Init() {
			FloatField field = new FloatField() {
				value = config.value != null ? (float)config.value : new float(),
			};
			field.RegisterValueChangedCallback((e) => {
				config.OnValueChanged(e.newValue);
				MarkDirtyRepaint();
			});
			Add(field);
		}
	}
}