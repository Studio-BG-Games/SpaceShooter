using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat.Radar;

namespace VSX.UniversalVehicleCombat
{
    public class HUDCursor : MonoBehaviour, IHUDCameraUser
    {
        [Tooltip("The camera used by the HUDCursor when setting the viewport position.")]
        [SerializeField]
        protected Camera m_Camera;
        public virtual Camera HUDCamera
        {
            set
            {
                m_Camera = value;
            }
        }

        [Tooltip("The canvas that the HUDCursor is part of.")]
        [SerializeField]
        protected Canvas canvas;
        protected RectTransform canvasRectTransform;

        [Tooltip("The Rect Transform of the cursor object.")]
        [SerializeField]
        protected RectTransform cursorRectTransform;

        [Tooltip("The Rect Transform of the line that goes from the screen center to the cursor.")]
        [SerializeField]
        protected RectTransform lineRectTransform;

        [Tooltip("The distance from the camera to maintain when operating on a world space canvas.")]
        [SerializeField]
        protected float worldSpaceDistanceFromCamera = 0.5f;

        public Vector3 ViewportPosition
        {
            get
            {
                if (m_Camera == null)
                {
                    return new Vector3(0, 0, 1);
                }

                bool worldSpace = (canvas == null) || (canvas.renderMode == RenderMode.WorldSpace);
                if (worldSpace)
                {
                    return (m_Camera.WorldToViewportPoint(cursorRectTransform.position));
                }
                else
                {
                    Vector3 canvasPos = cursorRectTransform.anchoredPosition + (0.5f * canvasRectTransform.sizeDelta);
                    return (new Vector3(canvasPos.x / canvasRectTransform.sizeDelta.x, canvasPos.y / canvasRectTransform.sizeDelta.y, 1));
                }
            }
        }

        public Vector3 AimDirection
        {
            get
            {
                if (m_Camera == null)
                {
                    return -cursorRectTransform.forward;
                }

                bool worldSpace = (canvas == null) || (canvas.renderMode == RenderMode.WorldSpace);
                if (worldSpace)
                {
                    return (cursorRectTransform.position - m_Camera.transform.position).normalized;
                }
                else
                {
                    return (m_Camera.ViewportToWorldPoint(ViewportPosition) - m_Camera.transform.position).normalized;
                }
            }
        }


        private void Awake()
        {
            if (canvas != null)
            {
                canvasRectTransform = canvas.GetComponent<RectTransform>();
            }
        }


        /// <summary>
        /// Put the cursor at the center of the screen.
        /// </summary>
        public virtual void CenterCursor()
        {
            SetViewportPosition(new Vector3(0.5f, 0.5f, worldSpaceDistanceFromCamera));
        }


        /// <summary>
        /// Set the viewport position of the cursor.
        /// </summary>
        /// <param name="viewportPosition"></param>
        public virtual void SetViewportPosition(Vector3 viewportPosition)
        {

            bool worldSpace = (canvas == null) || (canvas.renderMode == RenderMode.WorldSpace);

            // World space
            if (worldSpace && m_Camera != null)
            {

                viewportPosition.z = worldSpaceDistanceFromCamera;

                // Set the cursor position
                if (cursorRectTransform != null)
                {
                    cursorRectTransform.position = m_Camera.ViewportToWorldPoint(viewportPosition);
                    cursorRectTransform.position = m_Camera.transform.position + (cursorRectTransform.position - m_Camera.transform.position).normalized * worldSpaceDistanceFromCamera;
                    cursorRectTransform.LookAt(m_Camera.transform, m_Camera.transform.up);
                }

                // Set the line position
                if (lineRectTransform != null)
                {
                    Vector3 centerPos = m_Camera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, worldSpaceDistanceFromCamera));
                    centerPos = m_Camera.transform.position + (centerPos - m_Camera.transform.position).normalized * worldSpaceDistanceFromCamera;


                    lineRectTransform.position = 0.5f * centerPos + 0.5f * cursorRectTransform.position;
                    lineRectTransform.LookAt(m_Camera.transform,
                                                Vector3.Cross(m_Camera.transform.forward, (cursorRectTransform.position - lineRectTransform.position).normalized));

                    lineRectTransform.sizeDelta = new Vector2((cursorRectTransform.localPosition - lineRectTransform.localPosition).magnitude * 2 * (1 / lineRectTransform.localScale.x),
                                                                lineRectTransform.sizeDelta.y);
                }
            }
            // Screen/camera space
            else
            {
                // Set the cursor position
                if (cursorRectTransform != null)
                {
                    cursorRectTransform.anchoredPosition = Vector3.Scale(viewportPosition - new Vector3(0.5f, 0.5f, 0), canvasRectTransform.sizeDelta);
                }

                // Set the line position
                if (lineRectTransform != null)
                {
                    lineRectTransform.anchoredPosition = 0.5f * cursorRectTransform.anchoredPosition;

                    if (cursorRectTransform.anchoredPosition.magnitude > 0.001f) // Otherwise will get look rotation viewing vector is zero warning
                    {
                        lineRectTransform.localRotation = Quaternion.LookRotation(lineRectTransform.anchoredPosition, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
                        lineRectTransform.sizeDelta = new Vector2((cursorRectTransform.position - lineRectTransform.position).magnitude * 2 * (1 / lineRectTransform.localScale.x),
                                                                    lineRectTransform.sizeDelta.y);
                    }
                    else
                    {
                        lineRectTransform.sizeDelta = new Vector2(0, lineRectTransform.sizeDelta.y);
                    }
                }
            }
        }
    }
}