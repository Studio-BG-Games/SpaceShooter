using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	public interface IElementResizable {
		void OnStartResize();
		void OnResized();
	}

	public interface IDragableElement {
		void StartDrag();
	}

	public interface IDragManager {
		List<VisualElement> draggableElements { get; }
	}
}