using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    public class SliderPoint : MonoBehaviour
    {
        [InfoBox("Point должен иметь центрирование")]
        public RectTransform Point;
        public RectTransform Panel;

        public float minNormal=-1;
        public float maxNormal=1;

        public MoveAxis Axis;

        
        // normal = minNormal + (X-Xmin)/(Xmax - Xmin) * (maxNormal-minNormal)
        // X = (Normal-minNormal) / (maxNormal-minNormal) * (Xmax - Xmin) + xMin
        [Button]
        public void SetPoint(float v)
        {
            var positionValue = GetAxisPosition(Mathf.Clamp(v, minNormal, maxNormal));
            if (Axis == MoveAxis.Horizontal)
                Point.anchoredPosition = new Vector2(positionValue, 0);
            else
                Point.anchoredPosition = new Vector2(0, positionValue);
        }

        public float GetAxisPosition(float normal)
        {
            float max = 0;
            float min = 0;
            if (Axis == MoveAxis.Horizontal)
            {
                max = Panel.sizeDelta.x / 2;
                min = Panel.sizeDelta.x / 2 * -1;
            }
            else
            {
                max = Panel.sizeDelta.y / 2;
                min = Panel.sizeDelta.y / 2 * -1;
            }

            return (normal - minNormal) / (maxNormal - minNormal) * (max - min) + min;
        }
        

        private void OnValidate()
        {
            if (maxNormal < minNormal) maxNormal = minNormal;
        }
        
        public enum MoveAxis
        {
            Horizontal, Vertical
        }
    }
}