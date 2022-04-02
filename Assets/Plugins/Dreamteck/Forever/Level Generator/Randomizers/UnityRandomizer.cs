namespace Dreamteck.Forever
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Forever/Randomizers/Unity Randomizer")]
    public class UnityRandomizer : ForeverRandomizer
    {

        public override float Next01()
        {
            return Random.Range(0f, 1f);
        }

        public override int NextInt(int min, int max)
        {
            return Random.Range(min, max);
        }

        public override float NextFloat(float min, float max)
        {
            return Random.Range(min, max);
        }
    }
}