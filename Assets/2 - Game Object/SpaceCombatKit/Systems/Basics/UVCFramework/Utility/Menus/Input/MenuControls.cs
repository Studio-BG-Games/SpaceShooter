using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    
    [System.Serializable]
    public class MenuOpenInput
    {
        public SimpleMenuManager menu;
        public CustomInput input;
    }

    public class MenuControls : GeneralInput
    {
        [Header("Menu Input")]

        [Tooltip("Input settings for opening different menus.")]
        [SerializeField]
        protected List<MenuOpenInput> menuOpenInputs = new List<MenuOpenInput>();

        protected override void InputUpdate()
        {
            for(int i = 0; i < menuOpenInputs.Count; ++i)
            {
                if (menuOpenInputs[i].input.Down())
                {
                    menuOpenInputs[i].menu.OpenMenu();
                }
            }
        }
    }
}

