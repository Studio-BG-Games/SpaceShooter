using UnityEngine;

namespace RandomModule
{
    public class IntRandom : BaseRandom<int>
    {
        public override int Generate(int min, int max) => Random.Range(min, max);
    }
}