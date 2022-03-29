using UnityEngine;

namespace DIContainer
{
    public class InjectDI : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var beh in FindObjectsOfType<MonoBehaviour>(true)) DiBox.MainBox.InjectSingle(beh);
        }
    }
}