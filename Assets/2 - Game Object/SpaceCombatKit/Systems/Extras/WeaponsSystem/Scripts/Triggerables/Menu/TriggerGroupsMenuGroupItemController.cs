using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    // Unity event class for running functions when a trigger group is selected
    [System.Serializable]
    public class OnTriggerGroupsMenuGroupItemSelectedEventHandler : UnityEvent<int> { }

	/// <summary>
    /// Manages a trigger group item in the trigger groups menu.
    /// </summary>
	public class TriggerGroupsMenuGroupItemController : MonoBehaviour 
	{
	
        [Header("Settings")]
        [Tooltip("The Text used to show the label for this trigger group in the menu.")]
		[SerializeField]
		protected Text groupLabelText;

        [Tooltip("The Image component used by the Button for this trigger group.")]
        [SerializeField]
        protected Image buttonImage;

        [Tooltip("The sprite shown on the button when this trigger group is not selected.")]
        [SerializeField]
        protected Sprite unselectedSprite;

        [Tooltip("The sprite shown on the button when this trigger group is selected.")]
        [SerializeField]
        protected Sprite selectedSprite;

        // The controllers for the trigger items in this group
        [HideInInspector]
		public List<TriggerGroupsMenuTriggerItemController> triggerItems = new List <TriggerGroupsMenuTriggerItemController>();

        [Header("Events")]

        public OnTriggerGroupsMenuGroupItemSelectedEventHandler onTriggerGroupSelected;

        protected int index = -1;


        // Called when this group item is destroyed in the scene
        protected virtual void OnDestroy()
        {
            for (int i = 0; i < triggerItems.Count; ++i)
            {
                Destroy(triggerItems[i].gameObject);
            }
            triggerItems.Clear();
        }

        /// <summary>
        /// Set the index for this trigger group.
        /// </summary>
        /// <param name="newGroupIndex">The new trigger group index value.</param>
        public virtual void SetGroupIndex(int newGroupIndex)
		{
			index = newGroupIndex;
			groupLabelText.text = "GRP " + index.ToString();
		}


        /// <summary>
        /// Select this trigger group in the menu.
        /// </summary>
        /// <param name="callEvent">Whether to call the event.</param>
        public virtual void Select(bool callEvent = true)
		{
			buttonImage.sprite = selectedSprite;
            if (callEvent) onTriggerGroupSelected.Invoke(index);
		}
	

		/// <summary>
        /// Event called when this trigger group is no longer focused in the trigger group menu.
        /// </summary>
		public virtual void Deselect()
		{
			buttonImage.sprite = unselectedSprite;
		}
	}
}
