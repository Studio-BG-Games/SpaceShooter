using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.CameraSystem
{
    /// <summary>
    /// Represents a type of camera view, to categorize camera view targets across different camera targets.
    /// </summary>
    [CreateAssetMenu]
    public class CameraView : ScriptableObject
    {
        [SerializeField]
        protected string m_ID = "Camera View";
        public string ID { get { return m_ID; } }
    }
}
