using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Editors {
	public class GraphData {
		public Rect canvasArea;
		public Rect canvasRect;
		public Rect backgroundRect;

		public GraphEditorData editorData;
		public Rect borderRect;

		public bool isDim;
		public bool isDisableEdit;

		public HashSet<NodeComponent> dimmedNodes = new HashSet<NodeComponent>();
	}
}