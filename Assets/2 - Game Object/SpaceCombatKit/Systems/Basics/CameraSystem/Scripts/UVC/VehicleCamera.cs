using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using VSX.CameraSystem;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Unity Event for running functions when the vehicle camera's vehicle target is set.
    /// </summary>
    [System.Serializable]
    public class OnCameraTargetVehicleChangedEventHandler : UnityEvent<Vehicle> { }

    /// <summary>
    /// This class represents a vehicle camera, a camera which follows a vehicle and shows different views.
    /// </summary>
    public class VehicleCamera : CameraEntity
    {

        [Header("Vehicle Camera")]

        [SerializeField]
        protected bool linkToGameAgentManager = true;

        [Tooltip("The vehicle to follow when the scene starts.")]
        [SerializeField]
        protected Vehicle startingTargetVehicle;

        // Reference to the current target vehicle
        protected Vehicle targetVehicle;
        public Vehicle TargetVehicle { get { return targetVehicle; } }

        public OnCameraTargetVehicleChangedEventHandler onTargetVehicleChanged;


        protected override void Awake()
        {
            base.Awake();

            if (linkToGameAgentManager)
            {
                if (GameAgentManager.Instance != null)
                {
                    GameAgentManager.Instance.onFocusedVehicleChanged.AddListener(SetVehicle);
                }
            }
        }

        // Called at the start
        protected override void Start()
        {
            base.Start();

            // Start targeting the starting target vehicle
            if (startingTargetVehicle != null)
            {
                SetVehicle(startingTargetVehicle);
            }
        }


        /// <summary>
        /// Called to set a new target vehicle for the Vehicle Camera.
        /// </summary>
        /// <param name="newVehicle">The new target vehicle.</param>
        public virtual void SetVehicle(Vehicle newVehicle)
        {
            if (newVehicle == targetVehicle) return;

            CameraTarget newTarget = newVehicle != null ? newVehicle.GetComponent<CameraTarget>() : null;
            SetCameraTarget(newTarget);

        }

        public override void SetCameraTarget(CameraTarget target)
        {

            if (targetVehicle != null)
            {
                targetVehicle.onDestroyed.RemoveListener(OnVehicleDestroyed);
            }

            base.SetCameraTarget(target);
            targetVehicle = cameraTarget == null ? null : target.GetComponent<Vehicle>();

            if (targetVehicle != null)
            {
                targetVehicle.onDestroyed.AddListener(OnVehicleDestroyed);
            }

            onTargetVehicleChanged.Invoke(targetVehicle);
        }

        protected void OnVehicleDestroyed()
        {
            transform.SetParent(null);
        }
    }
}
