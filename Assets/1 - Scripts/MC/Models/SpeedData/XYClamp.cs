using Dreamteck.Forever;
using UnityEngine;

namespace Services
{
    public class XYClamp : MonoBehaviour
    {
        public Vector2 Min;
        public Vector2 Max;

        public void Clamp(Runner runner)
        {
            var oofset = runner.motion.offset;
            oofset = new Vector2(Mathf.Clamp(oofset.x, Min.x, Max.x), Mathf.Clamp(oofset.y, Min.y, Max.y));
            runner.motion.offset = oofset;
        }

        public Vector2 GetNormal(Runner runner) 
            => new Vector2(NormalValue(runner.motion.offset.x, Min.x, Max.x, -1, 1), NormalValue(runner.motion.offset.y, Min.y, Max.y, -1, 1));

        private float NormalValue(float value, float minNormal, float maxNormal, float minValue, float maxValue)
        {
            // normal formula: [-1;1]normalValue = -1 + (X-Xmin) / (Xmax-XMin) * (1-(-1))
            return minNormal + (value - minValue) / (maxValue - minValue) * (maxNormal - minNormal);
        }
    }
}