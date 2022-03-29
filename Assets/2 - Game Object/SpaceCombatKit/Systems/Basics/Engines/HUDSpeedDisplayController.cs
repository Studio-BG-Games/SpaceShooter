using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class HUDSpeedDisplayController : MonoBehaviour {

        [SerializeField]
        protected VehicleEngines3D engines;

        [SerializeField]
        protected Rigidbody rBody;

        [SerializeField]
        protected MeshRenderer speedBarRenderer;

        [SerializeField]
        protected Text speedText;

        [SerializeField]
        protected Image img;


        // Update is called once per frame
        void Update() {
            if (engines != null && rBody != null)
            {
                if (speedBarRenderer != null)
                {
                    speedBarRenderer.material.SetFloat("_FillAmount", rBody.velocity.magnitude / engines.GetCurrentMaxSpeedByAxis(false).z);
                }
                if (img != null) img.fillAmount = rBody.velocity.magnitude / engines.GetCurrentMaxSpeedByAxis(false).z;
                if (speedText != null)
                {
                    speedText.text = ((int)rBody.velocity.magnitude).ToString();
                }
            }
        }
    }
}