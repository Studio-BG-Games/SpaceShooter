using UnityEngine;

namespace RandomModule
{
    public class FloatRandom : BaseRandom<float>
    {
        public override float Generate(float min, float max) => Random.Range(min, max);
    }
}