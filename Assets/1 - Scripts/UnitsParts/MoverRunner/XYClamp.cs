using Dreamteck.Forever;
using UnityEngine;

namespace Services
{
    public class XYClamp : PartUnit
    {
        public Runner Runner;
        public Vector2 Min;
        public Vector2 Max;

        public void Clamp()
        {
            var oofset = Runner.motion.offset;
            oofset = new Vector2(Mathf.Clamp(oofset.x, Min.x, Max.x), Mathf.Clamp(oofset.y, Min.y, Max.y));
            Runner.motion.offset = oofset;
        }
    }
}