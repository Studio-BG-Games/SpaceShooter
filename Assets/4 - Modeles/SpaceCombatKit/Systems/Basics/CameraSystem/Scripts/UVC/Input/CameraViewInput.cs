using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.CameraSystem
{
    [System.Serializable]
    public class CameraViewInput
    {
        public CameraView view;
        public CustomInput input;

        public CameraViewInput(CameraView view, CustomInput input)
        {
            this.view = view;
            this.input = input;
        }
    }
}
