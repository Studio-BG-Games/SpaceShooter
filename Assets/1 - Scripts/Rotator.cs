using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    public class Rotator : MonoBehaviour
    {
        public Transform Target;
        [ShowInInspector] public Vector3 Diraction { get; set; }
        [ShowInInspector] public float Speed { get; set; }

        private void Update() => Target.Rotate(Diraction*Speed);
    }
}