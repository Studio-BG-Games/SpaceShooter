using UnityEngine;

namespace Infrastructure
{
    public class CorutineGame : MonoBehaviour
    {
        private static CorutineGame _instnace;

        public static CorutineGame Instance => _instnace??= new GameObject("Corutine object").AddComponent<CorutineGame>();

        private void Awake()
        {
            if(_instnace!=null && _instnace!=this) Destroy(this);
            else if (_instnace == null)
            {
                DontDestroyOnLoad(this);
                _instnace = this;
            }
        }
    }
}