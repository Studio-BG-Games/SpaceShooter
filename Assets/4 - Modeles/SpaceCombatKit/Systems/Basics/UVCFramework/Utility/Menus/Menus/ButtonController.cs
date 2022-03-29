using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{
    public class ButtonController : MonoBehaviour
    {

        [SerializeField]
        protected Button button;
        public Button Button { get { return button; } }

        [SerializeField]
        protected UVCText buttonText;

        [SerializeField]
        protected Image iconImage;

        [SerializeField]
        protected Image buttonImage;

        [SerializeField]
        protected Color selectedColor = Color.white;

        [SerializeField]
        protected Color unselectedColor = Color.white;

        [SerializeField]
        protected bool setButtonSpriteOnSelect = true;

        [SerializeField]
        protected Sprite selectedSprite;

        [SerializeField]
        protected Sprite unselectedSprite;

        [SerializeField]
        protected int buttonIndex;


        public void SetIndex(int index)
        {
            buttonIndex = index;
        }

        public void SetText(string text)
        {
            buttonText.text = text;
        }

        public void SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }

        public void SetSelected()
        {
            buttonImage.color = selectedColor;

            if (setButtonSpriteOnSelect) buttonImage.sprite = selectedSprite;
        }

        public void SetUnselected()
        {
            buttonImage.color = unselectedColor;

            if (setButtonSpriteOnSelect) buttonImage.sprite = unselectedSprite;
        }
    }
}