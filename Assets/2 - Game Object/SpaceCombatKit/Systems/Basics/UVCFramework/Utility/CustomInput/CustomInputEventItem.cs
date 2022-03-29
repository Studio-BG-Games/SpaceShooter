using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Unity event for running functions when an input down event is detected.
    /// </summary>
    [System.Serializable]
    public class OnInputDownEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when an input up event is detected.
    /// </summary>
    [System.Serializable]
    public class OnInputUpEventHandler : UnityEvent { }

    /// <summary>
    /// Unity event for running functions when an input axis is non-zero.
    /// </summary>
    [System.Serializable]
    public class OnInputAxisEventHandler : UnityEvent<float> { }

    /// <summary>
    /// An input item that listens for input events and triggers events when they happen.
    /// </summary>
    [System.Serializable]
    public class CustomInputEventItem
    {
        // Input settings
        public CustomInput customInput;

        // Input down event
        public OnInputDownEventHandler onInputDown;

        // Input down event
        public OnInputUpEventHandler onInputUp;

        // Non-zero input axis event
        public OnInputAxisEventHandler onInputAxis;


        public virtual void ProcessEvents()
        {
            switch (customInput.inputType)
            {
                
                case CustomInputType.Axis:

                    onInputAxis.Invoke(customInput.FloatValue());

                    break;

                default:

                    if (customInput.Down())
                    {
                        onInputDown.Invoke();
                    }
                    if (customInput.Up())
                    {
                        onInputUp.Invoke();
                    }
                    break;

            }
        }

    }
}

