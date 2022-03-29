using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{
    public class VehicleDeathCameraController : CameraOrbiter
    {

        protected Vehicle targetVehicle;


        protected override void Start()
        {
            base.Start();
            m_CameraEntity.onCameraTargetChanged.AddListener(OnCameraTargetChanged);
        }

        /// <summary>
        /// Called when the vehicle camera's camera target changes.
        /// </summary>
        /// <param name="newTarget">The new camera target.</param>
        protected virtual void OnCameraTargetChanged(CameraTarget newTarget)
        {
            if (targetVehicle != null)
            {
                targetVehicle.onDestroyed.RemoveListener(OnVehicleDestroyed);
            }

            if (newTarget != null)
            {
                targetVehicle = newTarget.GetComponent<Vehicle>();
                if (targetVehicle != null)
                {
                    targetVehicle.onDestroyed.AddListener(OnVehicleDestroyed);
                }
            }
            else
            {
                targetVehicle = null;
            }
        }

        protected virtual void OnVehicleDestroyed()
        {
            Orbit(targetVehicle.transform.position);
        }
    }

}
