using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Instantiates a prefab and respawns a set time after it is deactivated.
    /// </summary>
    public class Respawner : MonoBehaviour
    {
        [SerializeField]
        protected float respawnTime = 5;

        [SerializeField]
        protected GameObject prefab;
        protected GameObject createdGameObject;

        protected bool respawning = false;

        protected float respawnWaitStartTime = 0;


        private void Awake()
        {
            createdGameObject = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
            createdGameObject.transform.SetParent(transform);
        }

        // Called every frame
        private void Update()
        {
            if (!createdGameObject.activeSelf)
            {
                // Wait for respawn
                if (!respawning)
                {
                    respawning = true;
                    respawnWaitStartTime = Time.time;
                }
                else
                {
                    // Respawn
                    if (Time.time - respawnWaitStartTime > respawnTime)
                    {
                        createdGameObject.SetActive(true);
                        respawning = false;
                    }
                }
            }
        }
    }
}