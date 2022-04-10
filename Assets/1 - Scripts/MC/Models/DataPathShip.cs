using UnityEngine;

namespace MC.Models
{
    public class DataPathShip : MonoBehaviour
    {
        [SerializeField] private Vector2 _offsetInNomal;

        public Vector2 OffsetInNomal
        {
            get => _offsetInNomal;
            set => _offsetInNomal = value;
        }
        [SerializeField] private double _progress;
        
        public double Progress
        {
            get => _progress;
            set
            {
                if(value<0 || value > 1) Debug.LogWarning("Ты патаешься поставить прогресс в ненормальном состоянии");
                if (value < 0) _progress = 0;
                else if (value > 0) _progress = 0;
                else _progress = value;
            }
        }
    }
}