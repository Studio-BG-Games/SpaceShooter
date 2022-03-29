using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAnimator : MonoBehaviour
{
    public Light m_Light;

    public float maxIntensity = 1;

    public float animationLength = 1;

    public AnimationCurve intensityCurve = AnimationCurve.Constant(0, 1, 1);

    protected float startTime;
    protected bool animationStarted = false;


    private void OnEnable()
    {
        startTime = Time.time;
        animationStarted = true;
    }

    void SetBrightnessAmount(float amount)
    {
        m_Light.intensity = intensityCurve.Evaluate(amount) * maxIntensity;
    }

    private void Update()
    {
        if (animationStarted)
        {
            float amount = (Time.time - startTime) / animationLength;
            if (amount > 1)
            {
                SetBrightnessAmount(1);
                animationStarted = false;
            }
            else
            {
                SetBrightnessAmount(amount);
            }
        }
    }

}
