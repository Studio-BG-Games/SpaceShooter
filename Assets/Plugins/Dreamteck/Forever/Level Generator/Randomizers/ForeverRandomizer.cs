namespace Dreamteck.Forever
{
    using UnityEngine;

    public abstract class ForeverRandomizer : ScriptableObject
    {
        public virtual void Initialize()
        {

        }

        public abstract float Next01();

        public abstract int NextInt(int min, int max);

        public abstract float NextFloat(float min, float max);
    }
}