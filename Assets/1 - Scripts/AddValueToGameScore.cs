using ModelCore;
using UnityEngine;

namespace MC.Models
{
    public class AddValueToGameScore : MonoBehaviour
    {
        [Min(0)] [SerializeField] private int Value;
        private ScoreGame _score;

        private void Start() => _score = EntityAgregator.Instance.Select(x => x.Has<ScoreGame>()).Select<ScoreGame>();

        public void Add() => _score.ChangeScoreAt(Value);
        
        public void Add(int value) => _score.ChangeScoreAt(value);
    }
}