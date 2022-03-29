using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class PowerManagementMenuControls : MonoBehaviour
    {

        [Header("Settings")]

        [SerializeField]
        protected PowerManagementMenuController powerManagementMenuController;

        [SerializeField]
        protected float powerBallMoveSpeed = 1;

        [SerializeField]
        protected CustomInput powerBallMoveHorizontalInput;

        [SerializeField]
        protected CustomInput powerBallMoveVerticalInput;


        protected void Update()
        {
            if (powerManagementMenuController.MenuActivated)
            {
                // Move power ball horizontally
                powerManagementMenuController.MovePowerBallHorizontally(powerBallMoveHorizontalInput.FloatValue() * powerBallMoveSpeed * Time.unscaledDeltaTime);

                // Move power ball vertically
                powerManagementMenuController.MovePowerBallVertically(powerBallMoveVerticalInput.FloatValue() * powerBallMoveSpeed * Time.unscaledDeltaTime);
            }
        }
    }
}

