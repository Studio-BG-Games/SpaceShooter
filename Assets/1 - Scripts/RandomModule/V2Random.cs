using UnityEngine;

namespace RandomModule
{
    public class V2Random : BaseRandom<Vector2>
    {
        public override Vector2 Generate(Vector2 min, Vector2 max) => new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
    }
}