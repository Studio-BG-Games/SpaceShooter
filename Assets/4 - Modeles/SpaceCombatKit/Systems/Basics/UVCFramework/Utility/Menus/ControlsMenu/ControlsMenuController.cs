using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Manages the controls menu.
    /// </summary>
    public class ControlsMenuController : SimpleMenuManager
    {

        [Header("Controls")]

        [SerializeField]
        protected bool specifyVisibleGroups = false;

        [SerializeField]
        protected List<string> visibleGroups = new List<string>();

        [Tooltip("A prefab for a single item in the controls menu.")]
        [SerializeField]
        protected ControlsMenuItem controlsMenuItemPrefab;

        [Tooltip("A prefab for a group item in the controls menu.")]
        [SerializeField]
        protected ControlsMenuGroupItem controlsMenuGroupItemPrefab;

        [Tooltip("The parent for the controls menu items.")]
        [SerializeField]
        protected RectTransform controlsMenuItemsParent;


        // Use this for initialization
        protected virtual void Start()
        {

            List<CustomInput> customInputs = new List<CustomInput>();

            Object[] allMonoBehaviours = GameObject.FindObjectsOfType<Object>();
            for (int i = 0; i < allMonoBehaviours.Length; ++i)
            {
                FieldInfo[] fieldInfos = allMonoBehaviours[i].GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int j = 0; j < fieldInfos.Length; ++j)
                {
                    if (fieldInfos[j].FieldType == typeof(CustomInput))
                    {
                        customInputs.Add(fieldInfos[j].GetValue(allMonoBehaviours[i]) as CustomInput);
                    }
                }
            }

            // Get all the groups
            List<string> groups = new List<string>();
            foreach (CustomInput customInput in customInputs)
            {
                if (!groups.Contains(customInput.group))
                {
                    groups.Add(customInput.group);
                }
            }
            
            foreach (string group in groups)
            {
                if (specifyVisibleGroups && visibleGroups.IndexOf(group) == -1)
                {
                    continue;
                }

                // Add a group
                ControlsMenuGroupItem groupItemController = (ControlsMenuGroupItem)Instantiate(controlsMenuGroupItemPrefab, controlsMenuItemsParent);
                groupItemController.Set(group);

                foreach (CustomInput customInput in customInputs)
                {
                    if (customInput.group == group)
                    {
                        ControlsMenuItem itemController = (ControlsMenuItem)Instantiate(controlsMenuItemPrefab, controlsMenuItemsParent);
                        itemController.Set(customInput.action, customInput.GetInputAsString());
                    } 
                }
            }
        }
    }
}