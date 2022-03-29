using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Control script that performs vehicle camera view selection for the 
    /// </summary>
    public class CustomVehicleInput : VehicleInput
    {

        [Header("Input Event Items")]

        [SerializeField]
        protected List<CustomInputEventItem> inputItems = new List<CustomInputEventItem>();

        // Called every frame
        protected override void InputUpdate()
        {           
            // Run the input items
            for (int i = 0; i < inputItems.Count; ++i)
            {
                switch (inputItems[i].customInput.inputType)
                {
                    case CustomInputType.Key:

                        if (Input.GetKeyDown(inputItems[i].customInput.key))
                        {
                            
                            inputItems[i].onInputDown.Invoke();
                        }
                        if (Input.GetKeyUp(inputItems[i].customInput.key))
                        {
                            inputItems[i].onInputUp.Invoke();
                        }
                        break;

                    case CustomInputType.MouseButton:

                        if (Input.GetMouseButtonDown(inputItems[i].customInput.mouseButton))
                        {
                            inputItems[i].onInputDown.Invoke();
                        }
                        if (Input.GetMouseButtonUp(inputItems[i].customInput.mouseButton))
                        {
                            inputItems[i].onInputUp.Invoke();
                        }
                        break;

                    case CustomInputType.Axis:

                        inputItems[i].onInputAxis.Invoke(Input.GetAxis(inputItems[i].customInput.axis));
                        
                        break;
                }
            }
        }
    }
}

