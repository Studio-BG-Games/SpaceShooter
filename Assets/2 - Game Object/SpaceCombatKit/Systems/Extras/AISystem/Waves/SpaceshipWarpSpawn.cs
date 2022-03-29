using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class SpaceshipWarpSpawn : PilotedVehicleSpawn
    {

        [Header("Warp Animation")]

        [SerializeField]
        protected Transform warpStart;

        [SerializeField]
        protected Transform warpEnd;

        [SerializeField]
        protected float animationTime = 1;

        [SerializeField]
        protected AnimationCurve warpPositionAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header ("Warp Finish")]

        [SerializeField]
        protected Vector3 exitingWarpThrottleInputs = new Vector3(0, 0, 0.5f);

        [SerializeField]
        protected bool applyVelocityOnFinish = true;

        [SerializeField]
        protected bool enablePhysicsOnRelease = true;

        [SerializeField]
        protected bool enableGameAgentControlsOnRelease = true;

        [Header("Warp Animation Events")]

        public UnityEvent onAnimationStarted;
        public UnityEvent onAnimationFinished;

        protected Vector3 startPosition;
        protected Quaternion startRotation;

        protected Vector3 previousPosition;

        protected Coroutine animationCoroutine;

        protected bool finished = false;
        public bool Finished { get { return finished; } }


        public override void Create()
        {
            base.Create();

            spawnedVehicle.transform.position = warpStart.position;
            spawnedVehicle.transform.rotation = warpStart.rotation;
            spawnedVehicle.CachedRigidbody.isKinematic = true;
        }

        public override void Spawn()
        {
            base.Spawn();

            for (int i = 0; i < spawnedPilot.VehicleInputs.Count; ++i)
            {
                spawnedPilot.VehicleInputs[i].DisableInput();
            }

            animationCoroutine = StartCoroutine(AnimationCoroutine());

        }

        IEnumerator AnimationCoroutine()
        {

            startPosition = warpStart.position;
            startRotation = warpStart.rotation;

            spawnedVehicle.transform.position = startPosition;
            spawnedVehicle.transform.rotation = startRotation;

            spawnedVehicle.gameObject.SetActive(true);

            yield return null; // Wait for ship's Awake to run

            spawnedVehicle.CachedRigidbody.isKinematic = true;

            onAnimationStarted.Invoke();

            previousPosition = spawnedVehicle.transform.position;
            float startTime = Time.time;

            while (true)
            {
                float time = Time.time - startTime;
                float amount = Mathf.Max(time / animationTime, 0);

                if (amount > 1)
                {
                    FinishAnimation(amount);
                    break;
                }
                else
                {
                    SetAnimationPosition(amount);
                    previousPosition = spawnedVehicle.CachedRigidbody.position;
                }

                yield return new WaitForFixedUpdate();
            }

            spawned = true;
        }

        void FinishAnimation(float finishAmount)
        {
            if (enablePhysicsOnRelease)
            {
                // Prepare vehicle for gameplay
                spawnedVehicle.CachedRigidbody.isKinematic = false;

                spawnedVehicle.GetComponent<VehicleEngines3D>().SetMovementInputs(exitingWarpThrottleInputs);
                if (applyVelocityOnFinish)
                {
                    SetAnimationPosition(finishAmount);
                    spawnedVehicle.CachedRigidbody.velocity = (spawnedVehicle.CachedRigidbody.position - previousPosition) / Time.fixedDeltaTime;
                }
                else
                {
                    spawnedVehicle.CachedRigidbody.velocity = Vector3.zero;
                    SetAnimationPosition(1);
                }
            }
            else
            {
                SetAnimationPosition(warpPositionAnimationCurve.Evaluate(1));
            }

            if (enableGameAgentControlsOnRelease)
            {
                for (int i = 0; i < spawnedPilot.VehicleInputs.Count; ++i)
                {
                    spawnedPilot.VehicleInputs[i].EnableInput();
                }
            }

            finished = true;

            onAnimationFinished.Invoke();

        }

        void SetAnimationPosition(float position)
        {
            spawnedVehicle.transform.position = (position * warpEnd.position + (1 - position) * startPosition);
        }

        public override void FinishSpawn()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            FinishAnimation(1);
        }

        public override void ResetSpawn()
        {
            base.ResetSpawn();

            spawnedVehicle.transform.position = warpStart.position;
            spawnedVehicle.transform.rotation = warpStart.rotation;
            spawnedVehicle.CachedRigidbody.isKinematic = true;
        }
    }
}
