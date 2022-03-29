using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// Creates a flash by modifying the alpha of a set of materials over time.
    /// </summary>
	public class FlashController : MonoBehaviour 
	{   
        [Header("Settings")]
        
        [SerializeField]
        protected float flashPeriod = 0.1f;

        [SerializeField]
        protected AnimationCurve alphaOverLifetimeCurve = AnimationCurve.Linear(0, 1, 1, 0);

        protected float flashStartTime;
        protected float curveLocation = 0;
        protected float flashLevel = 0;
        protected bool flashStarted = false;

        [Header("VFX Elements")]

        [SerializeField]
        protected List<Renderer> vfxRenderers = new List<Renderer>();
        protected List<Material> vfxMaterials = new List<Material>();

        [SerializeField]
        protected string colorPropertyName = "_TintColor";

        [Header("Events")]

        // Called whenever a flash begins
        public UnityEvent onFlash;



        void Awake()
		{          
            // Cache the materials
			for (int i = 0; i < vfxRenderers.Count; ++i)
            {
                vfxMaterials.Add(vfxRenderers[i].material);
            }
            
            // Prevent flash at start
            flashStartTime = -flashPeriod;

            SetLevel(0);           
		}
	
		
        /// <summary>
        /// Create a flash.
        /// </summary>
		public void Flash()
		{
            flashStarted = true;
            flashStartTime = Time.time;

            onFlash.Invoke();
		}
        

        /// <summary>
        /// Set the flash level directly.
        /// </summary>
        /// <param name="newLevel">The new flash level.</param>
        public void SetLevel(float newLevel)
        {
            // Clamp from 0 to 1
            flashLevel = Mathf.Clamp(newLevel, 0, 1);
            
            // Set the flash alpha
            for (int i = 0; i < vfxRenderers.Count; ++i)
            {
                Color c = vfxMaterials[i].GetColor(colorPropertyName);
                c.a = newLevel;

                vfxMaterials[i].SetColor(colorPropertyName, c);
            }
        }

        
        // Called every frame
		void Update()
		{
            
            if (flashStarted)
            {
                // Prevent divide-by-zero errors with zero flash period
                if (Mathf.Approximately(flashPeriod, 0))
                {
                    flashStarted = false;
                    curveLocation = 1;
                }
                else
                {
                    // Calculate flash level from curve
                    curveLocation = (Time.time - flashStartTime) / flashPeriod;
                    if (curveLocation > 1)
                    {
                        curveLocation = 1;
                        flashStarted = false;
                    }
                }

                // Set flash level
                SetLevel(alphaOverLifetimeCurve.Evaluate(curveLocation));
            }
        }
	}
}
