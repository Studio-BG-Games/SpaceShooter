using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	[ControlField(typeof(int))]
	public class IntegerControl : ValueControl {
		public IntegerControl(ControlConfig config, bool autoLayout = false) : base(config, autoLayout) {
			Init();
		}

		void Init() {
			IntegerField field = new IntegerField() {
				value = config.value != null ? (int)config.value : 0,
			};
			field.RegisterValueChangedCallback((e) => {
				config.OnValueChanged(e.newValue);
				MarkDirtyRepaint();
			});
			Add(field);
		}
	}
}