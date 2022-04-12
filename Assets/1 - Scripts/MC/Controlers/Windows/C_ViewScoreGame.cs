using System;
using MC.Models;
using ModelCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Models
{
    public class C_ViewScoreGame : MonoBehaviour
    {
        public TextMeshProUGUI FactorText;
        public TextMeshProUGUI ScoreText;
        public Image ProgressFactor;
        public Image ProgressSurvive;
        
        private ScoreGame _scoreGame;


        private void Start()
        {
            _scoreGame = EntityAgregator.Instance.Select(x => x.Has<ScoreGame>()).Select<ScoreGame>();
            InitScoreGame();
        }

        private void InitScoreGame()
        {
            _scoreGame.ScoreUpdated += OnScoreUpdated;
            _scoreGame.MultiFactor.Updated += OnUpdatedMultiFactor;
            OnUpdatedMultiFactor();
            OnScoreUpdated();
        }

        private void OnUpdatedMultiFactor()
        {
            FactorText.text = _scoreGame.MultiFactor.Current.MultiAtFactor.ToString() + "x";
            ProgressFactor.fillAmount = _scoreGame.MultiFactor.Current.NormalTimeProgress;
        }

        private void OnScoreUpdated()
        {
            ScoreText.text = _scoreGame.Score.ToString();
            ProgressSurvive.fillAmount = _scoreGame.NormalPastTime;
        }
    }
}