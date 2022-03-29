using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
	public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			// Options for which axes to use
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}

		public int MovementRange = 100;
		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

		bool m_UseX; // Toggle for using the x axis
		bool m_UseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

        [SerializeField]
        protected CanvasScaler canvasScaler;

        protected RectTransform rectTransform;


		void OnEnable()
		{
			CreateVirtualAxes();
		}

      
		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = transform.parent.position - value;
			delta.y = -delta.y;
			if (m_UseX)
			{
                delta.x /= MovementRange * canvasScaler.transform.localScale.x;
				m_HorizontalVirtualAxis.Update(-delta.x);
            }

			if (m_UseY)
			{
                delta.y /= MovementRange * canvasScaler.transform.localScale.y;
                m_VerticalVirtualAxis.Update(delta.y);
			}
		}

		void CreateVirtualAxes()
		{
			// set axes to use
			m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
			m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

			// create new axes based on axes to use
			if (m_UseX)
			{
				m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
			}
			if (m_UseY)
			{
				m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
			}
		}

        
		public void OnDrag(PointerEventData data)
		{
			Vector3 newPos = Vector3.zero;

			if (m_UseX)
			{
				int delta = (int)(data.position.x - transform.parent.position.x);
				newPos.x = delta;
			}

			if (m_UseY)
			{
				int delta = (int)(data.position.y - transform.parent.position.y);
				newPos.y = delta;
			}

            transform.position = transform.parent.position + Mathf.Min(newPos.magnitude, MovementRange * canvasScaler.transform.localScale.y) * newPos.normalized;
;			UpdateVirtualAxes(transform.position);
		}


		public void OnPointerUp(PointerEventData data)
		{
			transform.position = transform.parent.position;
			UpdateVirtualAxes(transform.parent.position);
		}


		public void OnPointerDown(PointerEventData data) { }

		void OnDisable()
		{
			// remove the joysticks from the cross platform input
			if (m_UseX)
			{
				m_HorizontalVirtualAxis.Remove();
			}
			if (m_UseY)
			{
				m_VerticalVirtualAxis.Remove();
			}
		}
	}
}