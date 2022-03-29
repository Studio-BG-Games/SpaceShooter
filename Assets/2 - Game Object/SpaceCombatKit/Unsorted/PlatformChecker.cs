using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    public class PlatformChecker : MonoBehaviour
    {
        [SerializeField]
        protected bool includeEditor = true;

        public bool IsMobilePlatform
        {
            get
            {
#if UNITY_EDITOR
                if (includeEditor)
                {

                    if ((UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android) ||
                                                        (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
#else

                return Application.isMobilePlatform;
#endif
            }
        }

        public bool IsPCPlatform
        {
            get
            {
#if UNITY_EDITOR
                if (includeEditor)
                {
                    if (UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString().Contains("Standalone") ||
                            UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString().Contains("WebGL"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
#else
                return (Application.platform.ToString().Contains("Windows") ||
                        Application.platform.ToString().Contains("OSX") ||
                        Application.platform.ToString().Contains("Linux") ||
                        Application.platform.ToString().Contains("WebGL"));
#endif
            }
        }

        [SerializeField]
        protected List<GameObject> mobileActivationObjects = new List<GameObject>();

        public UnityEvent onEnabledMobilePlatform;

        public UnityEvent onEnabledPCPlatform;


        private void OnEnable()
        {
            for (int i = 0; i < mobileActivationObjects.Count; ++i)
            {
                mobileActivationObjects[i].SetActive(IsMobilePlatform);

            }

            if (IsMobilePlatform) onEnabledMobilePlatform.Invoke();
            if (IsPCPlatform) onEnabledPCPlatform.Invoke();
        }
    }
}
