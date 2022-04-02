using UnityEngine;

namespace RandomModule
{
    public class ColorRandom : BaseRandom<Color>
    {
        //rgba
        public override Color Generate(Color min, Color max) => new Color(Random.Range(min.r, max.r), Random.Range(min.g, max.g), Random.Range(min.b, max.b));
    }
}