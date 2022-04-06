using System;
using Dreamteck.Forever;
using MoreMountains.Tools;
using UnityEngine;

namespace DefaultNamespace
{
    public class MoverPointCamera : MonoBehaviour
    {
        public Runner RunnerOfPlayer;
        public Transform Point;

        private void OnEnable()
        {
            Point.transform.position = GetPosition();
            Point.transform.rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            Point.transform.rotation = transform.rotation;
            Point.transform.position = GetPosition();
        }

        private Vector3 GetPosition()
        {
            //(transform.up * RunnerOfPlayer.motion.offset.y);
            var oofsert = (transform.right * RunnerOfPlayer.motion.offset.x);
            var newPos = transform.position - oofsert;
            return newPos;
        }

        private void OnValidate()
        {
            if(Point == transform) Point = null;
        }

        private void OnDrawGizmos()
        {
            if(Point==null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, Point.position);
            Gizmos.DrawWireSphere(Point.position, 3);
        }
    }
}