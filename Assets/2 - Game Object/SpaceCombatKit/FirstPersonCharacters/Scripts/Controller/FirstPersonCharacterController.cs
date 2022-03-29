using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This is a modification of the FirstPersonController script from Unity's Standard Assets.
    /// </summary>
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonCharacterController : MonoBehaviour
    {
        [Header("Rotation")]

        [SerializeField]
        protected GimbalController gimbalController;

        [SerializeField]
        protected float rotationSensitivityX;

        [SerializeField]
        protected float rotationSensitivityY;

        [SerializeField]
        protected float rotationSensitivityZ;

        [Header("Movement")]

        [SerializeField] protected float walkSpeed = 5;
        [SerializeField] protected float runSpeed = 10;
        [SerializeField] [Range(0f, 1f)] private float runstepLengthen = 0.7f;
        [SerializeField] protected float jumpSpeed = 10;
        [SerializeField] protected float stickToGroundForce = 10;
        [SerializeField] protected float gravityMultiplier = 2;
        [SerializeField] protected bool enableVerticalMovement = false;
        [SerializeField] protected float stepInterval = 5;
        protected float speed;
        protected bool isRunning;

        [Header("Sound Effects")]

        [SerializeField] protected AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] protected AudioClip jumpSound;           // the sound played when character leaves the ground.
        [SerializeField] protected AudioClip landSound;           // the sound played when character touches back on ground.

        protected bool jump;
        protected float yRotation;
        protected Vector3 input;
        protected Vector3 moveDir = Vector3.zero;
        protected CharacterController characterController;
        protected CollisionFlags collisionFlags;
        protected bool previouslyGrounded;
        protected float stepCycle;
        protected float nextStep;
        protected bool jumping;
        protected AudioSource audioSource;

        

        


        // Use this for initialization
        private void Awake()
        {

            characterController = GetComponent<CharacterController>();
            
            if (gimbalController == null)
            {
                Debug.LogWarning("Add a gimbal controller to the first person character to enable looking around and rotating.");
            }
         
            stepCycle = 0f;
            nextStep = stepCycle/2f;
            jumping = false;
            audioSource = GetComponent<AudioSource>();

            speed = walkSpeed;
 
        }


        // Update is called once per frame
        protected void Update()
        {

            if (!previouslyGrounded && characterController.isGrounded)
            {
                PlayLandingSound();
                moveDir.y = 0f;
                jumping = false;
            }
            if (!characterController.isGrounded && !jumping && previouslyGrounded)
            {
                moveDir.y = 0f;
            }

            previouslyGrounded = characterController.isGrounded;

        }

        public void Jump()
        {
            jump = true;
        }


        private void PlayLandingSound()
        {
            audioSource.clip = landSound;
            audioSource.Play();
            nextStep = stepCycle + .5f;
        }


        private void FixedUpdate()
        {

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = characterController.transform.right * input.x + characterController.transform.up * input.y + characterController.transform.forward * input.z;
            

            if (enableVerticalMovement)
            {
                moveDir.y = desiredMove.y * speed;
            }
            else
            {
                // get a normal for the surface that is being touched to move along it
                RaycastHit hitInfo;
                Physics.SphereCast(characterController.transform.position, characterController.radius, Vector3.down, out hitInfo,
                                   characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
            }

            moveDir.x = desiredMove.x * speed;

            moveDir.z = desiredMove.z * speed;


            if (!enableVerticalMovement)
            {
                if (characterController.isGrounded)
                {
                    moveDir.y = -stickToGroundForce;

                    if (jump)
                    {
                        moveDir.y = jumpSpeed;
                        PlayjumpSound();
                        jump = false;
                        jumping = true;
                    }
                }
                else
                {
                    moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
                }
            }

            collisionFlags = characterController.Move(moveDir*Time.fixedDeltaTime);
            
            ProgressStepCycle(speed);
            
        }


        private void PlayjumpSound()
        {
            audioSource.clip = jumpSound;
            audioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (characterController.velocity.sqrMagnitude > 0 && (input.x != 0 || input.y != 0))
            {
                stepCycle += (characterController.velocity.magnitude + (speed*(isRunning ? runstepLengthen : 1f))) * Time.fixedDeltaTime;
            }

            if (!(stepCycle > nextStep))
            {
                return;
            }

            nextStep = stepCycle + stepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {

            if (footstepSounds.Length == 0) return;

            if (!characterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, footstepSounds.Length);
            audioSource.clip = footstepSounds[n];
            audioSource.PlayOneShot(audioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = audioSource.clip;
        }

        public void SetMovementInputs(float horizontalMovement, float verticalMovement, float forwardMovement)
        {
            input = new Vector3(horizontalMovement, verticalMovement, forwardMovement);
            
            // normalize input if it exceeds 1 in combined length:
            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }
        }


        public void SetRotationInputs(float horizontalRotationInput, float verticalRotationInput)
        {
            if (gimbalController != null)
            {
                Debug.Log(horizontalRotationInput * rotationSensitivityY);
                gimbalController.Rotate(horizontalRotationInput * rotationSensitivityY, verticalRotationInput * rotationSensitivityX);
            }
        }

    
        public void SetRunning(bool isRunning)
        {
            this.isRunning = isRunning;
            speed = isRunning ? runSpeed : walkSpeed;
        }
        
        
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {

            Rigidbody body = hit.collider.attachedRigidbody;

            //dont move the rigidbody if the character is on top of it
            if (collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            body.AddForceAtPosition(characterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
