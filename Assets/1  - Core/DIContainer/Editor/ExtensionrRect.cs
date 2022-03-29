using UnityEngine;

namespace DIContainer.Editor
{
    public static class ExtensionrRect
    {
        public static Rect[] Roww(this Rect rect, float[] wights)
        {
            Rect[] result = new Rect[wights.Length];

            float sum = 0;
            for (int i = 0; i < wights.Length; i++) sum += wights[i] < 0 ? (wights[i] = 1) : wights[i];

            for (int i = 0; i < wights.Length; i++)
            {
                float ratio = wights[i] / sum;
                var newRect = new Rect();
                newRect.height = rect.height;
                newRect.width = rect.width * ratio;
                newRect.y = rect.y;
                float lastX = 0;
                if (i > 0)
                    lastX = result[i - 1].xMax;
                newRect.x = lastX;
                result[i] = newRect;
            }

            return result;
        }
    }
}