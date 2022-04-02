using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat.Radar
{
    /// <summary>
    /// Helper functions for the HUD target boxes.
    /// </summary>
    public static class HUDTargetBoxesFunctions
    {
        
        /// <summary>
        /// Get the viewport position of a position in world space.
        /// </summary>
        /// <param name="worldPosition">The world position.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="centered">Whether the center of the viewport is in the middle of the screen (not the bottom left). </param>
        /// <param name="clampToScreenBorder">Whether to clamp the viewport position to the screen borders.</param>
        /// <returns>The viewport position.</returns>
        public static Vector3 GetViewportPosition(Vector3 worldPosition, Camera camera, bool centered = true, bool clampToScreenBorder = true)
        {

            // Get the viewport position
            Vector3 pos = camera.transform.InverseTransformPoint(worldPosition);
            float sign = Mathf.Sign(pos.z);
            pos.z = Mathf.Abs(pos.z);
            pos = camera.transform.TransformPoint(pos);
            pos = camera.WorldToViewportPoint(pos);
            pos.z *= sign;

            // Center the position
            if (centered)
            {
                pos.x -= 0.5f;
                pos.y -= 0.5f;
            }

            return pos;

        }


        /// <summary>
        /// Get the viewport size of the trackable.
        /// </summary>
        /// <param name="trackable">The trackable.</param>
        /// <param name="extentsCornersArray">An array for storing the bound extents information.</param>
        /// <param name="camera">The HUD camera.</param>
        /// <param name="rect">The rect that represents the viewport.</param>
        /// <returns>The viewport size.</returns>
        public static Vector2 GetViewportSize(Trackable trackable, Vector3[] extentsCornersArray, Camera camera, Rect rect)
        {

            // Get the positions of all of the corners of the bounding box
            Vector3 extents = trackable.TrackingBounds.extents;

            extentsCornersArray[0] = extents;
            extentsCornersArray[1] = new Vector3(-extents.x, extents.y, extents.z);
            extentsCornersArray[2] = new Vector3(extents.x, -extents.y, extents.z);
            extentsCornersArray[3] = new Vector3(extents.x, extents.y, -extents.z);
            extentsCornersArray[4] = new Vector3(-extents.x, -extents.y, -extents.z);
            extentsCornersArray[5] = new Vector3(-extents.x, -extents.y, extents.z);
            extentsCornersArray[6] = new Vector3(-extents.x, extents.y, -extents.z);
            extentsCornersArray[7] = new Vector3(extents.x, -extents.y, -extents.z);

            // Get the screen position of all of the box corners
            for (int i = 0; i < 8; ++i)
            {
                extentsCornersArray[i] = GetViewportPosition(trackable.transform.TransformPoint(trackable.TrackingBounds.center + extentsCornersArray[i]), camera);
            }

            // Find the minimum and maximum bounding box corners in screen space
            Vector3 min = extentsCornersArray[0];
            Vector3 max = extentsCornersArray[0];
            for (int i = 1; i < 8; ++i)
            {
                min = Vector3.Min(extentsCornersArray[i], min);
                max = Vector3.Max(extentsCornersArray[i], max);
            }
            
            return (new Vector2(Mathf.Min(max.x - min.x, rect.width), Mathf.Min(max.y - min.y, rect.height)));

        }

        /// <summary>
        /// Clamps a position inside a rect and returns the angle from the center.
        /// </summary>
        /// <param name="positionInRect">The position in the rect.</param>
        /// <param name="rect">The rect.</param>
        /// <param name="angle">The angle from the center.</param>
        /// <returns></returns>
        public static Vector3 ClampToBorder(Vector3 positionInRect, Rect rect, out float angle)
        {
            // Check if position is already inside the rect
            if (rect.Contains(positionInRect) && positionInRect.z > 0)
            {
                angle = Mathf.Atan2(positionInRect.y, positionInRect.x) * Mathf.Rad2Deg;
                return positionInRect;
            }

            // Slope of the target screen position vector relative to the screen center
            float screenPosSlope = positionInRect.x != 0 ? (positionInRect.y / positionInRect.x) : 0;            // Prevent divide by zero
            float rectSlope = rect.size.x != 0 ? rect.size.y / rect.size.x : 0;

            // Get the position on the screen border
            Vector2 pos = Vector2.zero;

            if (Mathf.Abs(screenPosSlope) < rectSlope)
            {
                // If the slope is shallower than the diagonal, arrow will be on the side of the rect

                float factor = rect.max.x / (Mathf.Approximately(positionInRect.x, 0) ? 0.00001f : Mathf.Abs(positionInRect.x));
                pos = positionInRect * factor;
            }
            else
            {
                float factor = rect.max.y / (Mathf.Approximately(positionInRect.y, 0) ? 0.00001f : Mathf.Abs(positionInRect.y));
                pos = positionInRect * factor;
            }

            // z angle of arrow relative to the screen
            angle = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;

            return pos;

        }  
    }
}
