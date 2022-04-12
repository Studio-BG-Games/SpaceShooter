using System;
using System.Collections.Generic;
using DIContainer;
using Infrastructure;
using ModelCore;
using Services;
using Services.PauseManagers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MC.Models
{
    public class ScoreGame : MonoBehaviour
    {
        public event Action ScoreUpdated;
        
        public Multiplier MultiFactor => _multiplier;        
        public LabelGoSo LabelPlayer;
        public int Score => _score;
        public float NormalPastTime=>_pastTime/_timeToAddScoreByTime;

        [SerializeField][Min(0)] private int _score;
        [Min(0)][SerializeField] private int _scroeToAddByTime;
        [Min(0.1f)][SerializeField] private int _timeToAddScoreByTime;
        [SerializeField] private Multiplier _multiplier;

        private float _pastTime = 0;
        
        private ResolveSingle<PauseGame> _pause = new ResolveSingle<PauseGame>();
        private Entity _player;
        private string _idCor;

        public void ChangeScoreAt(int addValue)
        {
            _score += _multiplier.Multi(addValue);
            ScoreUpdated?.Invoke();
        }
        
        private void Start()
        {
            _player = EntityAgregator.Instance.Select(x => x.Label.IsAlias(LabelPlayer));
            if (_player == null)
            {
                _idCor = CorutineGame.Instance.Wait(0.5f, ()=>Start());
            }
            else
            {
                _player.SelectAll<Health>().ForEach(x => x.ChangedOldNew += OnDamageHealth);
            }
        }

        private void OnDestroy()
        {
            if(_idCor!=null) CorutineGame.Instance.StopWait(_idCor);
        }

        private void OnDamageHealth(int prev, int now)
        {
            if (now >= prev) return;
            _pastTime = 0;
            _multiplier.Zero();
            ScoreUpdated?.Invoke();
        }

        private void Update()
        {
            if(_pause.Depence.IsPause.Value) return;
            
            _multiplier.Update(Time.deltaTime);

            _pastTime += Time.deltaTime;
            _pastTime = Mathf.Clamp(_pastTime, 0, _timeToAddScoreByTime);
            ScoreUpdated?.Invoke();
            if (_pastTime >= _timeToAddScoreByTime)
            {
                ChangeScoreAt(_scroeToAddByTime);
                _pastTime = 0;
            }
            
        }

        [System.Serializable]
        public class Multiplier
        {
            public event Action Updated;
            public Factor Current => Factors[indexCurrent];
            
            public List<Factor> Factors;
            private int indexCurrent=0;
            
            public int Multi(int scoreToMul) => Current.Multy(scoreToMul);

            public void Update(float deltaTime)
            {
                Current.Updated(deltaTime);
                if (Current.IsReady() && indexCurrent<Factors.Count-1)
                {
                    indexCurrent++;
                    Current.Zero();
                }
                Updated?.Invoke();
            }

            public void Zero()
            {
                indexCurrent = 0;
                Factors.ForEach(x => x.Zero());
            }

            [System.Serializable]
            public class Factor
            {
                [Min(1)]public int MultiAtFactor;
                [Min(0.1f)]public float TimeToNextFactor;
                private float _pastTimeToNextFactor;
                public float NormalTimeProgress => _pastTimeToNextFactor / TimeToNextFactor;

                public bool IsReady() => _pastTimeToNextFactor >= TimeToNextFactor;

                public void Updated(float deltatTime) => _pastTimeToNextFactor+=deltatTime;

                public int Multy(int score) => score * MultiAtFactor;

                public void Zero() => _pastTimeToNextFactor = 0;
            }
        }
    }
}