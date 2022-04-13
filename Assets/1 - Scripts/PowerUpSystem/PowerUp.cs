using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PowerUpSystem
{
    [System.Serializable]
    public class PowerUp
    {
        public event Action StateUpdated;
        
        public float LifeTime => _lifeTime;
        public float LifeProgressInNormal => _pastLiveTime / LifeTime;
        public bool IsEnd => LifeProgressInNormal == 1;
        public PowerUpType TypePowerUp => _typePowerUp;

        [SerializeField] private PowerUpType _typePowerUp;
        [Min(0)] [SerializeField] private float _lifeTime;

        [SerializeField][ReadOnly] private float _pastLiveTime;

        public void Life(float deltaTime)
        {
            _pastLiveTime += deltaTime;
            _pastLiveTime = Mathf.Clamp(_pastLiveTime, 0, LifeTime);
            StateUpdated?.Invoke();
        }
    }
}