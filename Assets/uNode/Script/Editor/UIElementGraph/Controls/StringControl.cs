using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	[ControlField(typeof(string))]
	public class StringControl : ValueControl {
		public TextField field;

		public StringControl(ControlConfig config, bool autoLayout = false) : base(config, autoLayout) {
			Init();
		}

		void Init() {
			TextField field = new TextField() {
				value = config.value != null ? (string)config.value : "",
			};
			field.RegisterValueChangedCallback((e) => {
				config.OnValueChanged(e.newValue);
				MarkDirtyRepaint();
			});
			Add(field);
			this.field = field;
		}
	}
}