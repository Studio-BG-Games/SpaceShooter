using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class UIFillBarController : MonoBehaviour
    {

        [SerializeField]
        protected Image barFill;


        public void SetFillAmount(float fillAmount)
        {
            barFill.fillAmount = fillAmount;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}