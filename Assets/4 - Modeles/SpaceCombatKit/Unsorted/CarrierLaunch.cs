using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;
using UnityEngine.Events;
using VSX.Effects;

public class CarrierLaunch : MonoBehaviour
{

    [SerializeField]
    protected Vehicle startingLaunchVehicle;

    [SerializeField]
    protected bool launchOnStart;

    [SerializeField]
    protected List<Collider> carrierColliders = new List<Collider>();


    [Header("Launch Parameters")]

    [SerializeField]
    protected Transform launchStart;

    [SerializeField]
    protected Transform launchEnd;

    [SerializeField]
    protected float launchTime = 3;


    [Header("Animation Curves")]

    [SerializeField]
    protected AnimationCurve launchCurve;

    [SerializeField]
    protected AnimationCurve throttleCurve;

    [SerializeField]
    protected AnimationCurve rumbleCurve;


    [Header("Game States")]

    [SerializeField]
    protected GameState launchingGameState;

    [SerializeField]
    protected GameState launchingFinishedGameState;

    public AudioSource launchAudio;
    public float launchAudioDelay = 2;


    [Header("Events")]

    public UnityEvent onLaunched;

    
    private void Awake()
    {
        // Disable the carrier colliders at the start
        for(int i = 0; i < carrierColliders.Count; ++i)
        {
            carrierColliders[i].enabled = false;
        }
    }

    private void Start()
    {
        if (launchOnStart && startingLaunchVehicle != null)
        {
            Launch(startingLaunchVehicle);
        }
    }

    /// <summary>
    /// Launch a vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle to launch.</param>
    public void Launch(Vehicle vehicle)
    {
        StartCoroutine(LaunchCoroutine(vehicle));   
    }

    // Launch coroutine
    protected IEnumerator LaunchCoroutine(Vehicle vehicle)
    {

        GameStateManager.Instance.EnterGameState(launchingGameState);
        VehicleEngines3D engines = vehicle.GetComponent<VehicleEngines3D>();

        // Prepare the vehicle
        vehicle.transform.position = launchStart.position;
        vehicle.transform.LookAt(launchEnd, Vector3.up);
        engines.SetMovementInputs(new Vector3(0, 0, 0));
        vehicle.CachedRigidbody.isKinematic = true;

        // Prepare data
        Vector3 previousPosition = vehicle.transform.position;
        float startTime = Time.time;

        launchAudio.PlayDelayed(launchAudioDelay);

        // Animate
        while (true)
        {

            float timeAmount = (Time.time - startTime) / launchTime;

            // Move the vehicle
            float launchAmount = launchCurve.Evaluate(timeAmount);
            vehicle.CachedRigidbody.MovePosition(launchAmount * launchEnd.position + (1 - launchAmount) * launchStart.position);

            // Update the throttle
            engines.SetMovementInputs(new Vector3(0, 0, throttleCurve.Evaluate(timeAmount)));

            // Add a rumble
            RumbleManager.Instance.AddSingleFrameRumble(rumbleCurve.Evaluate(timeAmount));

            // Check if finished animation
            if (timeAmount > 1)
            {
                // Enter new game state
                GameStateManager.Instance.EnterGameState(launchingFinishedGameState);

                // Prepare vehicle for gameplay
                vehicle.CachedRigidbody.isKinematic = false;
                vehicle.CachedRigidbody.velocity = vehicle.transform.forward * ((vehicle.CachedRigidbody.position - previousPosition).magnitude / Time.fixedDeltaTime);
                
                engines.SetMovementInputs(new Vector3(0, 0, 1));

                // Enable carrier colliders
                for (int i = 0; i < carrierColliders.Count; ++i)
                {
                    carrierColliders[i].enabled = true;
                }

                // Call the launched event
                onLaunched.Invoke();

                break;
            }

            // Record the previous position to calculate the speed next animation update
            previousPosition = vehicle.CachedRigidbody.position;

            yield return new WaitForFixedUpdate();
        }
    }
}
