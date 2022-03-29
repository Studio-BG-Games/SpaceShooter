using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    [System.Serializable]
    public class OnButtonSelectedEventHandler : UnityEvent<int> { }

    public class ButtonsListManager : MonoBehaviour
    {
        [SerializeField]
        protected ButtonController buttonPrefab;

        [SerializeField]
        protected Transform buttonsParent;

        [SerializeField]
        protected AudioSource buttonAudio;

        protected List<ButtonController> buttonControllers = new List<ButtonController>();

        public OnButtonSelectedEventHandler onButtonSelected;


        public void SetNumButtons(int numButtons)
        {
            int diff = numButtons - buttonControllers.Count;
            if (diff > 0)
            {
                for (int i = 0; i < diff; ++i)
                {
                    ButtonController buttonController = Instantiate(buttonPrefab, buttonsParent) as ButtonController;
                    buttonController.transform.SetParent(buttonsParent);
                    buttonController.transform.localPosition = Vector3.zero;
                    buttonController.transform.localRotation = Quaternion.identity;
                    buttonController.transform.localScale = new Vector3(1f, 1f, 1f);

                    buttonControllers.Add(buttonController);

                    buttonController.SetIndex(buttonControllers.Count - 1);

                    // Add events to the button
                    int index = buttonControllers.Count - 1;
                    buttonController.Button.onClick.AddListener(delegate { SelectButton(index); });

                    if (buttonAudio != null)
                    {
                        buttonController.Button.onClick.AddListener(buttonAudio.Play);
                    }

                }
            }
            else
            {
                for (int i = 0; i < diff; ++i)
                {
                    int nextIndex = numButtons + i;
                    buttonControllers[nextIndex].gameObject.SetActive(false);
                }
            }

            // Activate the buttons
            for (int i = 0; i < numButtons; ++i)
            {
                buttonControllers[i].gameObject.SetActive(true);
            }
        }

        public void SetVisibleButtons(List<int> visibleIndexes)
        {

            for (int i = 0; i < buttonControllers.Count; ++i)
            {
                buttonControllers[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < visibleIndexes.Count; ++i)
            {
                buttonControllers[visibleIndexes[i]].gameObject.SetActive(true);
            }
        }

        public void SetButtonSelected(int index)
        {
            for (int i = 0; i < buttonControllers.Count; ++i)
            {
                if (i == index)
                {
                    buttonControllers[i].SetSelected();
                }
                else
                {
                    buttonControllers[i].SetUnselected();
                }
            }
            
        }

        public void SelectButton(int index)
        {
            onButtonSelected.Invoke(index);
        }
    }
}

