using System;
using Dreamteck.Forever;
using MoreMountains.Tools;
using UnityEngine;

namespace DefaultNamespace
{
    public class PointPlayerCamera : MonoBehaviour
    {
        private PointPlayerCamera _instance;
        private Camera _camera;

        public Runner RunnerOfPlayer;
        [Min(0.1f)] public float SpeedMove = 5;

        private void Awake()
        {
            if (_instance) Debug.LogError("Есть несколько точек для камеры, это неправильно!", this);
            else _instance = this;

            _camera = Camera.main;
        }

        private void OnEnable()
        {
            _camera.transform.position = GetPosition();
            _camera.transform.rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            _camera.transform.rotation = transform.rotation;
            _camera.transform.position = Vector3.MoveTowards(_camera.transform.position, GetPosition(), Time.deltaTime * SpeedMove);
        }

        private Vector3 GetPosition()
        {
            //(transform.up * RunnerOfPlayer.motion.offset.y);
            var oofsert = (transform.right * RunnerOfPlayer.motion.offset.x);
            var newPos = transform.position - oofsert;
            newPos.x = 0;
            return newPos;
        }

        private void OnDestroy()
        {
            if(_instance==this) _instance = null;
        }
    }
}