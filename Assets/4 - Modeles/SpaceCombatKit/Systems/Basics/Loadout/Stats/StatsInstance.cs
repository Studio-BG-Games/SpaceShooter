using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class StatsInstance : MonoBehaviour
    {
        public UVCText labelText;

        public UVCText valueText;

        public UIFillBarController fillBar;

        public void Set(string label, string value, float barValue)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;

            valueText.gameObject.SetActive(true);
            valueText.text = value;

            fillBar.gameObject.SetActive(true);
            fillBar.SetFillAmount(barValue);
        }

        public void Set(string label, string value)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;

            valueText.gameObject.SetActive(true);
            valueText.text = value;

            fillBar.gameObject.SetActive(false);
        }

        public void Set(string label, float barValue)
        {
            labelText.gameObject.SetActive(true);
            labelText.text = label;

            valueText.gameObject.SetActive(false);

            fillBar.gameObject.SetActive(true);
            fillBar.SetFillAmount(barValue);
        }
    }
}
