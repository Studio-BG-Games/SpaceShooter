using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.SpaceCombatKit
{
    public class SCKRequiredInputAxis : ScriptableObject
    {

        public enum AxisType
        {
            KeyOrMouseButton = 0,
            MouseMovement = 1,
            JoystickAxis = 2
        };

        public string axisName = "Horizontal";
        public string negativeButton = "left";
        public string positiveButton = "right";
        public string altNegativeButton = "a";
        public string altPositiveButton = "d";

        public float gravity = 3;
        public float dead = 0.001f;
        public float sensitivity = 3;

        public bool snap = true;
        public bool invert = false;

        public AxisType axisType = AxisType.KeyOrMouseButton;
        public int axis = 0;
        public int joyNum = 0;

    }
}


