using UnityEngine;

namespace RandomModule
{
    public class V3Random : BaseRandom<Vector3>
    {
        public override Vector3 Generate(Vector3 min, Vector3 max) => new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }
}