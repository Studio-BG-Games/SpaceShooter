using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class AISpaceshipBehaviour : AIVehicleBehaviour
    {

        [SerializeField]
        protected ShipPIDController shipPIDController;

        [SerializeField]
        protected Vector3 maxRotationAngles = new Vector3(360, 360, 360);

    }
}