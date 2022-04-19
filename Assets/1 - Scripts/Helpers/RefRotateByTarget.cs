using System;
using UnityEngine;

namespace Helpers
{
    public class RefRotateByTarget : MonoBehaviour
    {
        public Transform Reference;
        public Transform Target;

        private void Update()
        {
            if(Reference && Target)Target.rotation = Reference.rotation;
        }
    }
}