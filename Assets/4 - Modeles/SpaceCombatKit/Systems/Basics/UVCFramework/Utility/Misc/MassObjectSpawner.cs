using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{
	/// <summary>
	/// This class spawns a group of objects in the scene (e.g. asteroids).
	/// </summary>
	public class MassObjectSpawner : MonoBehaviour
	{

		[SerializeField]
		protected GameObject prefab;

		[Header("Density")]

		[SerializeField]
		protected int numX = 20;

		[SerializeField]
		protected int numY = 2;

		[SerializeField]
		protected int numZ = 20;

		[SerializeField]
		protected float spacingX = 300;

		[SerializeField]
		protected float spacingY = 300;

		[SerializeField]
		protected float spacingZ = 300;

		[SerializeField]
		protected float minRandomOffset = 20;

		[SerializeField]
		protected float maxRandomOffset = 20;

		[Header("Scaling")]

		[SerializeField]
		protected float minRandomScale = 1;

		[SerializeField]
		protected float maxRandomScale = 3;

		[SerializeField]
		protected float minScaleAmount = 1;

		[SerializeField]
		protected float maxScaleAmount = 2;

		[SerializeField]
		protected AnimationCurve distanceScaleCurve = AnimationCurve.Linear(0, 1, 1, 0);

		[SerializeField]
		protected float distanceCutoff = 1000;

		[SerializeField]
		protected float maxDistMargin = 0;

		[SerializeField]
		protected List<MassObjectSpawnBlocker> spawnBlockers = new List<MassObjectSpawnBlocker>();



		// Use this for initialization
		void Start()
		{
			CreateObjects();
		}


		/// <summary>
		/// Create the objects in the scene
		/// </summary>
		void CreateObjects()
		{
			for (int i = 0; i < numX; ++i)
			{
				for (int j = 0; j < numY; ++j)
				{
					for (int k = 0; k < numZ; ++k)
					{

						Vector3 spawnPos = Vector3.zero;

						// Get a random offset for the position
						Vector3 offsetVector = Random.Range(minRandomOffset, maxRandomOffset) * Random.insideUnitSphere;

						// Calculate the spawn position
						spawnPos.x = transform.position.x - ((numX - 1) * spacingX) / 2 + (i * spacingX);
						spawnPos.y = transform.position.y - ((numY - 1) * spacingY) / 2 + (j * spacingY);
						spawnPos.z = transform.position.z - ((numZ - 1) * spacingZ) / 2 + (k * spacingZ);

						spawnPos += offsetVector;

						// Spawn objects within a radius from the center, pulling in those objects that are close to the boundary
						float distFromCenter = Vector3.Distance(spawnPos, transform.position);
						if (distFromCenter > distanceCutoff)
						{
							if (distFromCenter - distanceCutoff < maxDistMargin)
							{
								spawnPos = transform.position + (spawnPos - transform.position).normalized * distanceCutoff;
							}
							else
							{
								continue;
							}
						}

						bool isBlocked = false;
						for (int l = 0; l < spawnBlockers.Count; ++l)
						{
							if (spawnBlockers[l].IsBlocked(spawnPos))
							{
								isBlocked = true;
								break;
							}
						}
						if (isBlocked) continue;

						



						// Calculate a random rotation
						Quaternion spawnRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360),
																Random.Range(0, 360));

						// Create the object
						GameObject temp = (GameObject)Instantiate(prefab, spawnPos, spawnRot, transform);

						// Random scale
						float scale = Random.Range(minRandomScale, maxRandomScale);
						float scaleAmount = distanceScaleCurve.Evaluate(distFromCenter / distanceCutoff);
						scale *= scaleAmount * maxScaleAmount + (1 - scaleAmount) * scaleAmount;

						temp.transform.localScale = new Vector3(scale, scale, scale);

					}
				}
			}
		}
	}
}
