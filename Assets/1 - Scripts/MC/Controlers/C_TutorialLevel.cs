using System.Collections.Generic;
using DefaultNamespace;
using DIContainer;
using Infrastructure;
using MC.Models;
using ModelCore;
using Models;
using Services.PauseManagers;
using Services.RecoveryManagers;
using UltEvents;
using UnityEngine;

namespace MC.Controlers
{
    public class C_TutorialLevel : MonoBehaviour
    {
        public List<TutorWindowContainer> Screens;
        
        private ResolveSingle<PauseGame> _pauseGame = new ResolveSingle<PauseGame>();
        private OnOffInput _inputModel;
        private OnOffInput InputModel => _inputModel??= EntityAgregator.Instance.Select(x => x.Has<OnOffInput>()).Select<OnOffInput>();

        private int _currentIndex = 0;

        public UltEvent Ended;

        public void StartTutor(float delay)
        {
            _currentIndex = 0;
            OpenWindowByCurrentIndex(delay);
        }

        private void ShowNextWindow()
        {
            Screens[_currentIndex].Screen.Confirned -= ShowNextWindow;
            _currentIndex++;
            InputModel.IsOn = true;
            _pauseGame.Depence.IsPause.Value = false;
            if (_currentIndex >= Screens.Count)
            {
                Ended.Invoke();
                return;
            }
            OpenWindowByCurrentIndex(Screens[_currentIndex].TimeToShow);
        }

        private void OpenWindowByCurrentIndex(float delay)
        {
            CorutineGame.Instance.Wait(delay, () =>
            {
                InputModel.IsOn = false;
                _pauseGame.Depence.IsPause.Value = true;
                Screens[_currentIndex].Screen.Show();
                Screens[_currentIndex].Screen.Confirned += ShowNextWindow;
            });
        }
        
        [System.Serializable]
        public class TutorWindowContainer
        {
            public TutorScreen Screen;
            public float TimeToShow;
        }
    }
    
}