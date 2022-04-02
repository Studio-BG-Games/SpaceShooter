using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat.Radar
{ 
    /// <summary>
    /// Unity event for running functions when a target box is updated.
    /// </summary>
    [System.Serializable]
    public class OnTargetBoxUpdatedEventHandler : UnityEvent<Trackable> { } 

    /// <summary>
    /// Base class for controlling a single target box on the HUD.
    /// </summary>
    public class HUDTargetBox : MonoBehaviour
    {

        [Header("References")]

        [SerializeField]
        protected RectTransform rectTransform;
        public RectTransform RectTransform { get { return rectTransform; } }

        [SerializeField]
        protected HUDTargetBox_LocksController locksController;

        [SerializeField]
        protected HUDTargetBox_LeadTargetBoxesController leadTargetBoxesController;

        [SerializeField]
        protected UVCText distanceText;

        [SerializeField]
        protected UIColorManager targetBoxColorManager;

        [Header("UI Objects")]

        [SerializeField]
        protected List<GameObject> onScreenUIObjects = new List<GameObject>();

        [SerializeField]
        protected List<GameObject> offScreenUIObjects = new List<GameObject>();

        [SerializeField]
        protected List<GameObject> selectedUIObjects = new List<GameObject>();

        [SerializeField]
        protected List<GameObject> unselectedUIObjects = new List<GameObject>();

        [Header("Settings")]

        [SerializeField]
        protected bool resizeToTarget = true;

        [SerializeField]
        protected Vector2 minSize = new Vector2(30, 30);

        [SerializeField]
        protected Vector2 sizeMargin = new Vector2(15, 15);
        
        [Header("Events")]

        // Target box updated event
        public OnTargetBoxUpdatedEventHandler onTargetBoxUpdated;



        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
           
            // Update the linked texts when the target box is updated
            LinkedUIText[] linkedTexts = GetComponentsInChildren<LinkedUIText>();
            foreach(LinkedUIText linkedText in linkedTexts)
            {
                onTargetBoxUpdated.AddListener(linkedText.Set);
            }

            // Update the linked bars every time the target box is updated 
            LinkedUIBar[] bars = GetComponentsInChildren<LinkedUIBar>();
            
            foreach (LinkedUIBar bar in bars)
            {
                onTargetBoxUpdated.AddListener(bar.Set);
            }
        }

        /// <summary>
        /// Set whether the target is selected.
        /// </summary>
        /// <param name="isSelected">Whether the target is selected.</param>
        public virtual void SetIsSelectedTarget(bool isSelected)
        {
            for (int i = 0; i < selectedUIObjects.Count; ++i)
            {
                selectedUIObjects[i].SetActive(isSelected);
            }

            for (int i = 0; i < unselectedUIObjects.Count; ++i)
            {
                unselectedUIObjects[i].SetActive(!isSelected);
            }
        }

        /// <summary>
        /// Set the color for the target box.
        /// </summary>
        /// <param name="newColor">The new color.</param>
        public virtual void SetColor(Color newColor)
        {
            if (targetBoxColorManager != null) targetBoxColorManager.SetColor(newColor);
        }



        /// <summary>
        /// Set the size of the target box.
        /// </summary>
        /// <param name="size">The size of the target box.</param>
        public virtual void SetSize(Vector2 size)
        {
            
            if (resizeToTarget)
            {
                // Update the size of the target box
                rectTransform.sizeDelta = new Vector2(Mathf.Max(size.x, minSize.x),
                                                        Mathf.Max(size.y, minSize.y)) + sizeMargin;
            }
        }


        /// <summary>
        /// Set whether the target box is in view or off screen.
        /// </summary>
        /// <param name="isInView">Whether the target box is in view.</param>
        public virtual void SetIsInView(bool isInView)
        {
            // Update activation of on and off screen UI elements
            for (int i = 0; i < onScreenUIObjects.Count; ++i)
            {
                onScreenUIObjects[i].SetActive(isInView);
            }
            for (int i = 0; i < offScreenUIObjects.Count; ++i)
            {
                offScreenUIObjects[i].SetActive(!isInView);
            }
        }

        /// <summary>
        /// Set the distance to the target on the target box.
        /// </summary>
        /// <param name="distance">The distance to the target.</param>
        public virtual void SetDistance(float distance)
        {
            if (distanceText != null)
            {
                if (HUDDistanceLookup.Instance != null)
                {
                    distanceText.text = HUDDistanceLookup.Instance.Lookup(distance);
                }
                else
                {
                    distanceText.text = ((int)distance).ToString() + " M";
                }
            }
        }


        /// <summary>
        /// Get a lead target box.
        /// </summary>
        /// <returns>The lead target box controller.</returns>
        public virtual HUDTargetBox_LeadTargetBoxController GetLeadTargetBox()
        {
            if (leadTargetBoxesController != null)
            {
                return leadTargetBoxesController.GetLeadTargetBox();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add a lock to the target box.
        /// </summary>
        /// <param name="targetLocker">The lock information source.</param>
        public virtual void AddLock(TargetLocker targetLocker)
        {
            if (locksController != null)
            {
                locksController.AddLock(targetLocker);
            }
        }


        /// <summary>
        /// Called to set the trackable that this target box represents.
        /// </summary>
        /// <param name="trackable">The trackable that this target box represents.</param>
        public virtual void UpdateTargetBox(Trackable trackable)
        {
            // Call the event
            onTargetBoxUpdated.Invoke(trackable);   
        }
    }
}
