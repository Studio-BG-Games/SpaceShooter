using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VSX.UniversalVehicleCombat
{
    public class BoxSpawnBlocker : MassObjectSpawnBlocker
    {

        public float length = 200;
        public float height = 200;
        public float width = 200;

        public override bool IsBlocked(Vector3 position)
        {
            Vector3 unrotatedLocalPos = position - transform.position;

            if (Mathf.Abs(unrotatedLocalPos.x) > width / 2f) return false;
            if (Mathf.Abs(unrotatedLocalPos.y) > height / 2f) return false;
            if (Mathf.Abs(unrotatedLocalPos.z) > length / 2f) return false;

            return true;
        }


        protected override void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(width, height, length));
        }
    }
}

