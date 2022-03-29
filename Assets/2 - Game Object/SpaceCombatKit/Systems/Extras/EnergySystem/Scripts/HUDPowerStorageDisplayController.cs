using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class HUDPowerStorageDisplayController : MonoBehaviour {

        [SerializeField]
        protected Power power;

        [SerializeField]
        protected MeshRenderer barRenderer;

        [SerializeField]
        protected Text text;

        [SerializeField]
        protected PoweredSubsystemType powerType;


        // Update is called once per frame
        void Update() {
            if (power != null)
            {
                if (barRenderer != null)
                {
                    float fraction = power.GetStorageCapacity(powerType) == 0 ? 0 : power.GetStoredPower(powerType) / power.GetStorageCapacity(powerType);
                    barRenderer.material.SetFloat("_FillAmount", fraction);
                }

                if (text != null)
                {
                    text.text = ((int)power.GetStoredPower(powerType)).ToString();
                }
            }
        }
    }
}