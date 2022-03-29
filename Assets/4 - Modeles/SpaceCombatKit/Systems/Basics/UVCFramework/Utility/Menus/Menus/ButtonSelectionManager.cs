using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class ButtonSelectionManager : MonoBehaviour
    {

        protected Button[] buttons;

        [SerializeField]
        protected Sprite selectedSprite;

        [SerializeField]
        protected Sprite unselectedSprite;

        [SerializeField]
        protected int startingHighlightedIndex = -1;

        protected void Awake()
        {
            buttons = transform.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; ++i)
            {
                int index = i;
                buttons[i].onClick.AddListener(delegate { OnButtonClicked(index); });
            }

            if (startingHighlightedIndex >= 0)
            {
                OnButtonClicked(startingHighlightedIndex);
            }
        }

        void OnButtonClicked(int index)
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (i == index)
                {
                    buttons[i].image.sprite = selectedSprite;
                }
                else
                {
                    buttons[i].image.sprite = unselectedSprite;
                }
            }
        }
    }
}