using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class provides a fast lookup for converting float distances to strings, because the .ToString() method
    /// can hit performance hard if there are many targets.
    /// </summary>
	public class HUDDistanceLookup : MonoBehaviour
    {
	
		[Header("General")]
	
		[SerializeField]
		private int maxRangeInMetres = 10000;
		
		[SerializeField]
		private string outOfRangeLabel = "OutOfRange";
		private static string s_OutOfRangeLabel = "OutOfRange";
	
		[Header("Kilometres")]
	
		// the resolution of the kilometre information (0.001 to 1)
		[SerializeField]
		private float kilometreResolution = 0.1f;
		private static float s_KilometreResolution = 0.1f;
		static int s_NumEntriesPerKilometre;
	
		[SerializeField]
		private int thresholdKM = 200;
		private static int s_ThresholdKM = 200;
	
		private static List<string> s_DistanceLookupKilometres = new List<string>();
		private static List<string> s_DistanceLookupMetres = new List<string>();

        public static HUDDistanceLookup Instance;


		
		// Called only on the editor when script is loaded or values are changed
		void OnValidate()
		{	
			// Clamp the kilometre resolution from 1 metre to 1 kilometre
			kilometreResolution = Mathf.Clamp(kilometreResolution, 0.001f, 1f);

            // Make sure the resolution gives a whole number
            kilometreResolution = 1f / (Mathf.RoundToInt(1f / kilometreResolution));
		}
	

		void Awake()
		{
	
            // Assign singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

			// Clear the lookup lists
			s_DistanceLookupKilometres.Clear();

			s_DistanceLookupMetres.Clear();

			int numKilometres = maxRangeInMetres / 1000;

			s_NumEntriesPerKilometre = Mathf.RoundToInt(1 / kilometreResolution);
			
			// Fill the kilometre list
			for (int i = 0; i < numKilometres; ++i){
				for (int j = 0; j < s_NumEntriesPerKilometre; ++j)
				{
					float decimalPart = j * kilometreResolution;
					float num = i + decimalPart;
					string result = num.ToString("F1") + " KM";
					s_DistanceLookupKilometres.Add (result);
				}
			}
	
			// Fill the metre list for higher resolution distance info at close proximity
			for (int i = 0; i < thresholdKM; ++i){
				string result = i.ToString() + " M";
				s_DistanceLookupMetres.Add(result);
			}
	
			// Update static variables
			s_KilometreResolution = kilometreResolution;
			s_OutOfRangeLabel = outOfRangeLabel;
			s_ThresholdKM = thresholdKM;
	
		}


        /// <summary>
        /// Called by a widget that needs to convert a float distance to a string
        /// </summary>
        /// <param name="distanceInMetres"></param>
        /// <returns>The distance value as a string, either in kilometres or metres depending on the threshold.</returns>
        public string Lookup(float distanceInMetres)
		{
	
			// If less than threshold, return the distance in metre format
			if (distanceInMetres < s_ThresholdKM)
			{
				int val = (int)distanceInMetres;
				return s_DistanceLookupMetres.Count > val ? s_DistanceLookupMetres[(int)distanceInMetres] : s_OutOfRangeLabel;
			}
			else
			{
	
				int wholeIndex = (int)(distanceInMetres / 1000f);
				int fractionIndex = (int)(((distanceInMetres / 1000f) - wholeIndex) / s_KilometreResolution);
					
				int index = wholeIndex * s_NumEntriesPerKilometre + fractionIndex;

				if (index < s_DistanceLookupKilometres.Count)
				{
					return (s_DistanceLookupKilometres[index]);
				}
				else
				{
					return s_OutOfRangeLabel;
				}
			}
			
		}
	}
}