using UnityEngine;
using System.Collections;
using UnityEditor;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Controls a gimbal (an apparatus that can rotate vertically and horizontally) such as a turret or a camera gimbal.
    /// </summary>
    public class GimbalController : MonoBehaviour 
	{

        [Header("General")]

        // Whether the gimbal is currently disabled.
        [SerializeField]
        protected bool disabled = false;
        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }

		[SerializeField]
		protected bool visualizeParameters = true;

        [Header("Gimbal Parts")]

		[SerializeField]
        protected Transform horizontalPivot;
        public Transform HorizontalPivot 
		{ 
			get { return horizontalPivot; }
            set { horizontalPivot = value; }
		}

		[SerializeField]
        protected Transform verticalPivot;
        public Transform VerticalPivot 
		{ 
			get { return verticalPivot; }
			set { verticalPivot = value; }
		}

        [SerializeField]
        protected Transform gimbalChild;

		[Header("Horizontal Constraints")]

        // Whether the horizontal pivot rotation is constrained
		[SerializeField]
        protected bool noConstraintsHorizontal = true;
        public bool NoConstraintsHorizontal 
		{
            get { return noConstraintsHorizontal; }
            set { noConstraintsHorizontal = value; }
        }

		// The horizontal arc angle
		[SerializeField]
		protected float minHorizontalPivotAngle = -180;

		[SerializeField]
		protected float maxHorizontalPivotAngle = 180;

		[SerializeField]
		protected float horizontalIdleAngle = 0;

		[Header("Vertical Constraints")]

        // The minimum angle for the vertical pivot
		[SerializeField]
        protected float minVerticalPivotAngle = -90f;	

        // The maximum angle for the vertical pivot
		[SerializeField]
        protected float maxVerticalPivotAngle = 90f;

		[SerializeField]
		protected float verticalIdleAngle = 0;

		[Header("Gimbal Motors")]

        // PID values 

		[SerializeField]
        protected float proportionalCoefficient = 0.5f;
		
		[SerializeField]
        protected float integralCoefficient = 0;

		[SerializeField]
        protected float derivativeCoefficient = 0.1f;

		[SerializeField]
        protected float maxHorizontalAngularVelocity = 3f;
		
		[SerializeField]
        protected float maxVerticalAngularVelocity = 3f;
	
        // Cached PID values

		protected float proportionalValue_HorizontalPivot = 0;
        protected float integralValue_HorizontalPivot = 0;
        protected float derivativeValue_HorizontalPivot = 0;

        protected float proportionalValue_VerticalPivot = 0;
        protected float integralValue_VerticalPivot = 0;
        protected float derivativeValue_VerticalPivot = 0;


        private void OnValidate()
        {
			// Clamp horizontal pivot parameters
			maxHorizontalPivotAngle = Mathf.Clamp(maxHorizontalPivotAngle, minHorizontalPivotAngle, minHorizontalPivotAngle + 360);		
			horizontalIdleAngle = Mathf.Clamp(horizontalIdleAngle, minHorizontalPivotAngle, maxHorizontalPivotAngle);
			if (noConstraintsHorizontal)
            {
				minHorizontalPivotAngle = -180;
				maxHorizontalPivotAngle = 180;
            }

			// Clamp vertical pivot parameters
			minVerticalPivotAngle = Mathf.Clamp(minVerticalPivotAngle, -90, 90);
			maxVerticalPivotAngle = Mathf.Max(minVerticalPivotAngle, maxVerticalPivotAngle);
			verticalIdleAngle = Mathf.Clamp(verticalIdleAngle, minVerticalPivotAngle, maxVerticalPivotAngle);
			
		}

        /// <summary>
        /// Convert an angle of any magnitude to a -180 - 180 angle.
        /// </summary>
        /// <param name="angle">The original value.</param>
        /// <returns>The result.</returns>
        protected float WrapTo180Split(float angle)
		{
	
			if (angle < -180f)
			{
				float amountOver = Mathf.Abs(angle) - 180f;
				float tmp = Mathf.Floor(amountOver/360f) + 1;
				return angle + tmp * 360f;
			} 
			else if (angle > 180f) 
			{
				float amountOver = Mathf.Abs(angle) - 180f;
				float tmp = Mathf.Floor(amountOver/360f) + 1;
				return angle - tmp * 360f;
			} 
			else 
			{
				return angle;
			}
		}


		public virtual void Idle(bool snapTo = false)
        {
			Vector3 idleVec = transform.forward;

			Quaternion horizontalPivotRotation = horizontalPivot != null ? Quaternion.AngleAxis(horizontalIdleAngle, horizontalPivot.up) : Quaternion.identity;
			idleVec = horizontalPivotRotation * idleVec;

			Quaternion verticalPivotRotation = verticalPivot != null ? Quaternion.AngleAxis(verticalIdleAngle, horizontalPivotRotation * -transform.right) : Quaternion.identity;
			idleVec = verticalPivotRotation * idleVec;

			Vector3 pos = verticalPivot != null ? verticalPivot.position : (horizontalPivot != null ? horizontalPivot.position : transform.position);

			float ang;
			TrackPosition(pos + idleVec, out ang, snapTo);
		}
      

        /// <summary>
        /// Directly set the gimbal rotation.
        /// </summary>
        /// <param name="horizontalPivotLocalRotation">The new rotation for the horizontal pivot.</param>
        /// <param name="verticalPivotLocalRotation">The new rotation for the vertical pivot.</param>
        public virtual void SetGimbalRotation(Quaternion horizontalPivotLocalRotation, Quaternion verticalPivotLocalRotation)
		{
			horizontalPivot.localRotation = horizontalPivotLocalRotation;
			verticalPivot.localRotation = verticalPivotLocalRotation;
		}


		/// <summary>
        /// Track a position in world 3D space.
        /// </summary>
        /// <param name="target">The world space target position.</param>
        /// <param name="angleToTarget">The variable to be updated with the angle to the target.</param>
        /// <param name="snapToTarget">Whether to snap to the target or use the gimbal motors to smoothly move there.</param>
		public virtual void TrackPosition(Vector3 target, out float angleToTarget, bool snapToTarget)
		{

			// For aim assist
			angleToTarget = 180;
			
			if (disabled) return;

			// ****************************** Rotate Horizontally to Target ******************************************

			// Get the local target position wrt the horizontally rotating body
			Vector3 targetLocalPos = transform.InverseTransformPoint(target);
			
			// Get the angle from the base to the target on the local horizontal plane
			float toTargetAngle_HorizontalPlane = Vector3.Angle(Vector3.forward, new Vector3(targetLocalPos.x, 0f, targetLocalPos.z));

			// Correct the sign 
			if (targetLocalPos.x < 0)
				toTargetAngle_HorizontalPlane *= -1;

			// Get the desired angle for the horizontal gimbal on the horizontal plane (wrt the gimbal parent forward vector)
			float desiredLocalHorizontalPivotAngle = toTargetAngle_HorizontalPlane;

			// Take constraints into account
			if (!noConstraintsHorizontal)
			{
				desiredLocalHorizontalPivotAngle = Mathf.Clamp(toTargetAngle_HorizontalPlane, minHorizontalPivotAngle, maxHorizontalPivotAngle);
			}
			
			if (snapToTarget)
			{
				horizontalPivot.localRotation = Quaternion.Euler(0f, desiredLocalHorizontalPivotAngle, 0f);
			}
			else
			{
				
				// Get the current angle of the horizontal gimbal on the horizontal plane (wrt the gimbal parent forward vector)
				float currentHorizontalPivotAngle = WrapTo180Split(horizontalPivot.localRotation.eulerAngles.y);
	
				// Get the angle from current to desired for horizontal gimbal
				float horizontalPivotAngle = desiredLocalHorizontalPivotAngle - currentHorizontalPivotAngle;


				// If the horizontal constraints allow it, allow the horizontal gimbal to cross the 180/-180 threshold
				if (Mathf.Abs(horizontalPivotAngle) > 180 && (minHorizontalPivotAngle <= -180 || maxHorizontalPivotAngle >= 180))
				{
					horizontalPivotAngle = Mathf.Sign(horizontalPivotAngle) * -1 * (360 - Mathf.Abs(horizontalPivotAngle));
				}
				
				// Get the PID values
				proportionalValue_HorizontalPivot = horizontalPivotAngle * proportionalCoefficient;
				derivativeValue_HorizontalPivot = -horizontalPivotAngle * derivativeCoefficient;
				integralValue_HorizontalPivot += horizontalPivotAngle * integralCoefficient;
	
				// Calculate and constrain the rotation speed
				float rotationSpeedHorizontal = proportionalValue_HorizontalPivot + integralValue_HorizontalPivot + derivativeValue_HorizontalPivot;
				
				rotationSpeedHorizontal = Mathf.Clamp(rotationSpeedHorizontal, -maxHorizontalAngularVelocity, maxHorizontalAngularVelocity);
                
                // Rotate the horizontal gimbal
                horizontalPivot.Rotate(new Vector3(0f, rotationSpeedHorizontal, 0f));
			}



            // ****************************** Rotate Vertically to Target ******************************************

            Vector3 offset = Vector3.Scale (new Vector3(1/transform.localScale.x, 1/transform.localScale.y, 1/transform.localScale.z), 
                                                        transform.InverseTransformDirection(verticalPivot.position - transform.position));

            Vector3 targetLocalPosV = targetLocalPos - offset;
            
            angleToTarget = Vector3.Angle(verticalPivot.forward, target - verticalPivot.position);

			// Get the angle from the local target position vector to the local horizontal plane
			float desiredLocalVerticalPivotAngle = Vector3.Angle(targetLocalPosV, new Vector3(targetLocalPosV.x, 0f, targetLocalPosV.z));

			// Correct the sign
			if (targetLocalPosV.y > 0)
				desiredLocalVerticalPivotAngle *= -1;

			// Constrain the desired vertical pivot angle
			desiredLocalVerticalPivotAngle = Mathf.Clamp(desiredLocalVerticalPivotAngle, -maxVerticalPivotAngle, -minVerticalPivotAngle);

			//Debug.Log("desired: " + desiredLocalVerticalPivotAngle);
		
			if (snapToTarget)
			{
				verticalPivot.localRotation = Quaternion.Euler(desiredLocalVerticalPivotAngle, 0f, 0f);
			}
			else
			{
				// Get the current angle of the vertically pivoting body
				float currentVerticalPivotAngle = WrapTo180Split(verticalPivot.localRotation.eulerAngles.x);
				float verticalPivotAngle = desiredLocalVerticalPivotAngle - currentVerticalPivotAngle;

				
				//Debug.Log("diff: " + verticalPivotAngle);

				// Get the PID values
				proportionalValue_VerticalPivot = verticalPivotAngle * proportionalCoefficient;
				derivativeValue_VerticalPivot = -verticalPivotAngle * derivativeCoefficient;
				integralValue_VerticalPivot += verticalPivotAngle * integralCoefficient;
	
				// Calculate and constrain the rotation speed
				float rotationSpeedVertical = proportionalValue_VerticalPivot + integralValue_VerticalPivot + derivativeValue_VerticalPivot;
				rotationSpeedVertical = Mathf.Clamp(rotationSpeedVertical, -maxVerticalAngularVelocity, maxVerticalAngularVelocity);
				
				// Rotate the vertical gimbal
				verticalPivot.Rotate(new Vector3(rotationSpeedVertical, 0f, 0f));
			}
		}


		/// <summary>
        /// Rotate the gimbal incrementally around each of the axes. 
        /// </summary>
        /// <param name="rotationAmounts"></param>
		public virtual void Rotate(float horizontalRotation, float verticalRotation)
		{

			if (disabled) return;

			// Get the current angle of the horizontal gimbal on the horizontal plane (wrt the gimbal parent forward vector)
			float currentHorizontalPivotAngle = WrapTo180Split(horizontalPivot.localRotation.eulerAngles.y);

			// Add the rotation
			float desiredHorizontalPivotAngle = currentHorizontalPivotAngle + horizontalRotation;
			
			// Constrain the angle
			if (!noConstraintsHorizontal) desiredHorizontalPivotAngle = Mathf.Clamp(desiredHorizontalPivotAngle, minHorizontalPivotAngle, maxHorizontalPivotAngle);
		
			// Set the horizontal pivot
			horizontalPivot.localRotation = Quaternion.Euler(0f, desiredHorizontalPivotAngle, 0f);


			// Get the current angle of the vertical gimbal on the vertical plane 
			float currentVerticalPivotAngle = WrapTo180Split(verticalPivot.localRotation.eulerAngles.x);
			
			// Add the rotation
			float desiredVerticalPivotAngle = currentVerticalPivotAngle + verticalRotation;
			
			// Constrain the angle
			desiredVerticalPivotAngle = Mathf.Clamp(desiredVerticalPivotAngle, minVerticalPivotAngle, maxVerticalPivotAngle);
		
			// Set the horizontal pivot
			verticalPivot.localRotation = Quaternion.Euler(desiredVerticalPivotAngle, 0f, 0f);

		}

        /// <summary>
        /// Reset the gimbal rotatuion.
        /// </summary>
        public virtual void ResetGimbal(bool snapToCenter)
        {
            if (snapToCenter)
            {
                horizontalPivot.localRotation = Quaternion.identity;
                verticalPivot.localRotation = Quaternion.identity;
            }
            else
            {
                float angleToTarget;
                // Target straight ahead.
                TrackPosition(transform.TransformPoint(Vector3.forward * 10) + (verticalPivot.transform.position - transform.position), out angleToTarget, false);
            }
        }

        public virtual void SetGimbalChildLocalPosition(Vector3 localPosition)
        {
            if (gimbalChild != null)
            {
                gimbalChild.localPosition = localPosition;
            }
        }


		private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR

			if (!visualizeParameters) return;

			Color c = UnityEditor.Handles.color;
			UnityEditor.Handles.color = new Color(0.1f, 1f, 0f, 0.15f);

			// Vertical arc
			if (verticalPivot != null)
            {
				// The vertical arc should be at the center of the horizontal pivot range
				Quaternion angle = horizontalPivot == null ? Quaternion.identity : Quaternion.AngleAxis((minHorizontalPivotAngle + maxHorizontalPivotAngle) / 2, horizontalPivot.up);

				Vector3 startVec = angle * Quaternion.AngleAxis(minVerticalPivotAngle, -transform.right) * transform.forward;
				Vector3 rightVec = angle * -transform.right;

				UnityEditor.Handles.DrawSolidArc(verticalPivot.position, rightVec, startVec, maxVerticalPivotAngle - minVerticalPivotAngle, 30);
			}

			// Horizontal arc
			if (horizontalPivot != null)
            {
				Vector3 startVecH = Quaternion.AngleAxis(minHorizontalPivotAngle, horizontalPivot.up) * transform.forward;
				UnityEditor.Handles.DrawSolidArc(verticalPivot != null ? verticalPivot.position : horizontalPivot.position, horizontalPivot.up, startVecH, maxHorizontalPivotAngle - minHorizontalPivotAngle, 30);
			}

			// Idle direction
			Vector3 idleVec = transform.forward;

			Quaternion horizontalPivotRotation = horizontalPivot != null ? Quaternion.AngleAxis(horizontalIdleAngle, horizontalPivot.up) : Quaternion.identity;
			idleVec = horizontalPivotRotation * idleVec;

			Quaternion verticalPivotRotation = verticalPivot != null ? Quaternion.AngleAxis(verticalIdleAngle, horizontalPivotRotation * -transform.right) : Quaternion.identity;
			idleVec = verticalPivotRotation * idleVec;

			Vector3 pos = verticalPivot != null ? verticalPivot.position : (horizontalPivot != null ? horizontalPivot.position : transform.position);

			Handles.color = Color.white;
			Gizmos.DrawLine(pos, pos + idleVec * 40);


			UnityEditor.Handles.color = c;
#endif
		}
	}
}
