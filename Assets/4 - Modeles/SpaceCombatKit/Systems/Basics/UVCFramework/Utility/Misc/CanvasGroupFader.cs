using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Fade a canvas group according to an animation curve.
    /// </summary>
    public class CanvasGroupFader : MonoBehaviour
    {
        [SerializeField]
        protected bool loop;

        [SerializeField]
        protected bool startOnEnable;

        [SerializeField]
        protected AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [SerializeField]
        protected float animationLength = 3;

        protected float animationStartTime;
        protected bool animating;

        [SerializeField]
        protected float startAlpha = 0;

        [SerializeField]
        protected CanvasGroup canvasGroup;


        // Called when this component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }


        protected virtual void Awake()
        {
            canvasGroup.alpha = startAlpha;
        }


        protected virtual void OnEnable()
        {
            if (startOnEnable)
            {
                StartAnimation();
            }
        }


        /// <summary>
        /// Start animating the canvas group.
        /// </summary>
        public virtual void StartAnimation()
        {
            animating = true;
            animationStartTime = Time.time;
        }


        // Called every frame
        protected virtual void Update()
        {
            if (animating)
            {
                float amount = (Time.time - animationStartTime) / animationLength;

                // If finished, finish animating
                if (amount >= 1)
                {
                    canvasGroup.alpha = alphaCurve.Evaluate(1);
                    animating = false;

                    // If looping, start again
                    if (loop) StartAnimation();
                }
                // If still animating, update the alpha
                else
                {
                    float curveAmount = alphaCurve.Evaluate(amount);
                    canvasGroup.alpha = curveAmount;
                }
            }
        }
    }
}

