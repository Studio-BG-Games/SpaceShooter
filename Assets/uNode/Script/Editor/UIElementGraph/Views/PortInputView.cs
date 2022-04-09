using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public class PortInputView : GraphElement {
		public Color edgeColor {
			get {
				if(data != null && data.port != null) {
					return data.port.portColor;
				}
				return Color.white;
			}
		}

		public PortData data { get; private set; }

		VisualElement m_Control;
		VisualElement m_Container;
		VisualElement m_Dot;
		EdgeControl m_EdgeControl;

		public PortInputView(PortData data) {
			this.AddStyleSheet("uNodeStyles/NativePortStyle");
			this.AddStyleSheet(UIElementUtility.Theme.portStyle);
			pickingMode = PickingMode.Ignore;
			ClearClassList();
			this.data = data;

			m_EdgeControl = new EdgeControl {
				from = new Vector2(412f - 21f, 11.5f),
				to = new Vector2(412f, 11.5f),
				edgeWidth = 2,
				pickingMode = PickingMode.Ignore,
				visible = false,
			};
			Add(m_EdgeControl);

			m_Container = new VisualElement { name = "container", visible = false };
			m_Container.SetOpacity(false);
			{
				if(this.data != null) {
					m_Control = this.data.InstantiateControl();
					if(m_Control != null) {
						m_Control.AddToClassList("port-control");
						m_Container.Add(m_Control);
					}
				}

				m_Dot = new VisualElement { name = "dot" };
				m_Dot.style.backgroundColor = edgeColor;
				var slotElement = new VisualElement { name = "slot" };
				{
					slotElement.Add(m_Dot);
				}
				var slotContainer = new VisualElement() { name = "slotContainer" };
				{
					slotContainer.Add(slotElement);
				}
				m_Container.Add(slotContainer);
			}
			Add(m_Container);

			this.ScheduleAction(DoUpdate, 500);
			data?.owner?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
		}

		private void OnGeometryChanged(GeometryChangedEvent evt) {
			// Check if dimension has changed
			if(evt.oldRect.size == evt.newRect.size)
				return;
			UpdatePosition();
		}

		public void UpdatePosition() {
			if(data == null || parent == null)
				return;
			this.SetPosition(new Vector2(0, data.port.ChangeCoordinatesTo(parent, Vector2.zero).y));
		}

		private bool hasUpdatePosition = false;

		void DoUpdate() {
			if(data.owner.IsFaded())
				return;
			if(!hasUpdatePosition) {
				hasUpdatePosition = true;
				UpdatePosition();
			}
			m_Container.visible = m_EdgeControl.visible = data != null && !data.owner.inputPorts.Any((p) => p.portData != null && p.portData.portID == data.portID && p.connected);
			m_Container.SetOpacity(m_Container.visible);
			m_Container.SetDisplay(m_Container.visible);
			var color = edgeColor;
			if(!m_EdgeControl.visible) {
				var edges = data?.port?.GetEdges();
				if(edges != null && edges.Count > 0) {
					if(edges.Any(e => e != null && e.isProxy)) {
						m_EdgeControl.visible = false;
					}
				}
			} else {
				m_EdgeControl.inputColor = color;
				m_EdgeControl.outputColor = color;
				m_Dot.style.backgroundColor = color;
			}
			if(UIElementUtility.Theme.coloredPortBorder) {
				m_Container.style.SetBorderColor(color);
			}
		}

		protected override void OnCustomStyleResolved(ICustomStyle style) {
			base.OnCustomStyleResolved(style);
			if(m_Container.visible) {
				m_EdgeControl.UpdateLayout();
			}
		}

		public float GetPortWidth() {
			return m_Container.visible ? m_Container.layout.width : 0;
		}

		public bool IsVisible() {
			return m_Container.IsVisible();
		}
	}
}