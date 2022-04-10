using UnityEngine;

namespace DefaultNamespace
{
    public class TimeGame : MonoBehaviour
    {
        public void Play() => Time.timeScale = 1;

        public void SetCustomTime(float timeScale) => Time.timeScale = timeScale;
        
        public void Stop() => Time.timeScale = 0;
    }
}