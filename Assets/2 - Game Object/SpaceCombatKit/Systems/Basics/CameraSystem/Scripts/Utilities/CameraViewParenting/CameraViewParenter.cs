using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{   
    public class CameraViewParenter : MonoBehaviour, ICameraEntityUser
    {
        [Tooltip("The camera entity on which camera view changes will trigger the parenting behaviour on this component.")]
        [SerializeField]
        protected CameraEntity cameraEntity;

        [Tooltip("The view (if any) that will be applied on Awake.")]
        [SerializeField]
        protected CameraView initialCameraView;

        [Tooltip("The list of camera view parenting items.")]
        public List<CameraViewParentingItem> cameraViewParentingItems = new List<CameraViewParentingItem>();


        protected virtual void Awake()
        {

            if (initialCameraView != null)
            {
                OnCameraViewChanged(initialCameraView);
            }

            if (cameraEntity != null)
            {
                SetCameraEntity(cameraEntity);
            }
        }

        public virtual void SetCameraEntity(CameraEntity newCameraEntity)
        {
            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.RemoveListener(OnCameraViewTargetChanged);
            }

            cameraEntity = newCameraEntity;

            if (cameraEntity != null)
            {
                cameraEntity.onCameraViewTargetChanged.AddListener(OnCameraViewTargetChanged);
            }
        }


        protected virtual void OnCameraViewTargetChanged(CameraViewTarget cameraViewTarget)
        {
            OnCameraViewChanged(cameraViewTarget.CameraView);
        }


        // Called when the camera view changes
        protected virtual void OnCameraViewChanged(CameraView cameraView)
        {
            for (int i = 0; i < cameraViewParentingItems.Count; ++i)
            {
                for (int j = 0; j < cameraViewParentingItems[i].cameraViews.Count; ++j)
                {
                    if (cameraViewParentingItems[i].cameraViews[j] == cameraView)
                    {
                        switch (cameraViewParentingItems[i].parentType)
                        {
                            case CameraViewParentType.Transform:

                                cameraViewParentingItems[i].m_Transform.SetParent(cameraViewParentingItems[i].parentTransform);
                                break;

                            case CameraViewParentType.Camera:

                                if (cameraEntity != null) cameraViewParentingItems[i].m_Transform.SetParent(cameraEntity.MainCamera.transform);
                                break;

                            case CameraViewParentType.None:

                                cameraViewParentingItems[i].m_Transform.SetParent(null);
                                break;

                        }

                        // Position
                        if (cameraViewParentingItems[i].setLocalPosition)
                        {
                            cameraViewParentingItems[i].m_Transform.localPosition = cameraViewParentingItems[i].localPosition;
                        }

                        // Rotation
                        if (cameraViewParentingItems[i].setLocalRotation)
                        {
                            cameraViewParentingItems[i].m_Transform.transform.localRotation = Quaternion.Euler(cameraViewParentingItems[i].localRotationEulerAngles);
                        }

                        //Scale
                        if (cameraViewParentingItems[i].setLocalScale)
                        {
                            cameraViewParentingItems[i].m_Transform.localScale = cameraViewParentingItems[i].localScale;
                        }
                    }
                }
            }
        }

    }
}
