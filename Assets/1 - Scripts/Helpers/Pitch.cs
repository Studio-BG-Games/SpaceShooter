using UnityEngine;

namespace DefaultNamespace
{
    public class Pitch : MonoBehaviour
    {
        public float PitchForce; // Rotate X. X negative to up; X positive to down; (WTF!?!?!)
        public float RollForce; // Rotate Z. Z negative rotate to right; Z positive rotate to left;

        [Min(0)]public float Speed;
        public Transform Target;

        public void Make(Vector2 input)
        {
            var targetRotate = Quaternion.Euler(new Vector3(input.y * -1 * PitchForce, 0, input.x * -1 * RollForce));
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotate, Time.deltaTime * Speed);
        }

    }
}