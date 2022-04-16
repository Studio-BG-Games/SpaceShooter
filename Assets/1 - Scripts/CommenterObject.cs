using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace
{
    public class CommenterObject : MonoBehaviour
    {
        [HideLabel, Multiline(40)] public string Value;
    }
}