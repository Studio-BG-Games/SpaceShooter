using UltEvents;
using UnityEngine;

namespace DefaultNamespace
{
    public class ChanceEvent : MonoBehaviour
    {
        [Range(0, 1f)] public float _chance;

        public UltEvent True;
        public UltEvent False;

        public void Invoke()
        {
            if(Random.Range(0,1f)<_chance) True.Invoke();
            else False.Invoke();
        }
    }
}