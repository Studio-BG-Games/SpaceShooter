using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class provides a simple way to enable and disable a set of UI objects, and set the first UI element selection.
    /// </summary>
    public class SimpleMenuManager : MonoBehaviour
    {

        [Header("Menu Manager")]

        [SerializeField]
        protected bool deactivateMenuOnAwake = true;

        [SerializeField]
        protected List<GameObject> UIObjects = new List<GameObject>();

        [SerializeField]
        protected bool selectFirstUIObject = true;
        public bool SelectFirstUIObject
        {
            get { return selectFirstUIObject; }
            set
            {
                selectFirstUIObject = value;

                if (menuActivated)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    waitingForHighlight = true;
                }
            }
        }

        [SerializeField]
        protected GameObject firstSelected;
        protected bool waitingForHighlight = false;

        protected bool menuActivated = false;
        public bool MenuActivated { get { return menuActivated; } }

        protected bool menuInitialized = false;

        public UnityEvent onMenuOpened;

        public UnityEvent onMenuClosed;


        protected virtual void Awake()
        {
            InitializeMenu();
            if (deactivateMenuOnAwake) DeactivateMenu();
        }


        protected virtual void InitializeMenu()
        {
            menuInitialized = true;
        }


        protected virtual IEnumerator WaitForActivation(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            OpenMenu();

        }


        public virtual void OpenMenuDelayed(float delay)
        {
            StartCoroutine(WaitForActivation(delay));
        }


        public virtual void OpenMenu()
        {
            if (menuInitialized)
            {
                ActivateMenu();
            }
        }


        protected virtual void ActivateMenu()
        {

            for (int i = 0; i < UIObjects.Count; ++i)
            {
                UIObjects[i].SetActive(true);
            }

            if (selectFirstUIObject && firstSelected != null)
            {
                // When the menu activates, flag the first item to be selected, and clear the currently selected item.
                // The new selected gameobject must be selected in OnGUI.
                EventSystem.current.SetSelectedGameObject(null);
                waitingForHighlight = true;
            }

            menuActivated = true;

            onMenuOpened.Invoke();
        }


        public virtual void CloseMenu()
        {
            if (menuActivated)
            {
                DeactivateMenu();
            }
        }


        protected virtual void DeactivateMenu()
        {
            for (int i = 0; i < UIObjects.Count; ++i)
            {
                UIObjects[i].SetActive(false);
            }

            menuActivated = false;

            waitingForHighlight = false;

            onMenuClosed.Invoke();
        }


        // Called when the UI is updated
        protected virtual void OnGUI()
        {
            // If the flag is still up, highlight the first button
            if (waitingForHighlight)
            {
                // Highlight the first button
                EventSystem.current.SetSelectedGameObject(selectFirstUIObject ? firstSelected : null);

                // Reset the flag
                waitingForHighlight = false;
            }
        }
    }
}