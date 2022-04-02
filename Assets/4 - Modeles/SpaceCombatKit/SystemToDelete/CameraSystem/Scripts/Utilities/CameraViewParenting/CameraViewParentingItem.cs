using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    [System.Serializable]
    public class CameraViewParentingItem
    {
        public Transform m_Transform;

        public List<CameraView> cameraViews = new List<CameraView>();

        public CameraViewParentType parentType;

        public Transform parentTransform;

        public bool setLocalPosition = true;
        public Vector3 localPosition = Vector3.zero;

        public bool setLocalRotation = true;
        public Vector3 localRotationEulerAngles = Vector3.zero;

        public bool setLocalScale = false;
        public Vector3 localScale = new Vector3(1, 1, 1);
    }

}
