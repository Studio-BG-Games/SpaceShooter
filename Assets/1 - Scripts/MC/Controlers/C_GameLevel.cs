using Infrastructure;
using MC.Models;
using ModelCore;
using Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MC.Controlers
{
    public class C_GameLevel : MonoBehaviour
    {
        public C_GeneratorLevel GeneratorLevel;
        public C_PlayerShipUI UiController;
        public C_GameSceneScreenControl GameScreen;

        [Min(0)]public float DelayFirstShowGameUI = 0.4f;
        public string MenuSceneName;
        private GameFlowModel _gameFlow;
        private OnOffInput _inputModel;

        private void Start()
        {
            GeneratorLevel.GenerateLevel(OnLevelGenerate);
            _gameFlow= EntityAgregator.Instance.Select(x => x.Has<GameFlowModel>()).Select<GameFlowModel>();
            _inputModel = EntityAgregator.Instance.Select(x => x.Has<OnOffInput>()).Select<OnOffInput>();
        }

        private void OnLevelGenerate()
        {
            C_FadeScreen.Instance.Load.Model.Status = false;
            CorutineGame.Instance.WaitFrame(2, () => UiController.Init(GeneratorLevel.SpawnPlayer()));
            CorutineGame.Instance.Wait(DelayFirstShowGameUI, () => GameScreen.GameHud.Model.Status = true);

            _gameFlow.PlayerDead.Evented += () =>
            {
                GameScreen.GameHud.Model.Status = false;
                _inputModel.IsOn = false;
                GameScreen.LoseScreen.Model.Status = true;
            };

            _gameFlow.GameWin.Evented += () =>
            {
                GameScreen.GameHud.Model.Status = false;
                _inputModel.IsOn = false;
                GameScreen.WinScreen.Model.Status = true;
            };

            _gameFlow.ExitMenu.Evented += () =>
            {
                C_FadeScreen.Instance.Load.Model.Status = true;
                GameScreen.LoseScreen.Model.Status = false;
                GameScreen.WinScreen.Model.Status = false;
                CorutineGame.Instance.Wait(2, () => SceneLoader.Load(MenuSceneName));
            };

            _gameFlow.RestartGame.Evented += () =>
            {
                C_FadeScreen.Instance.Load.Model.Status = true;
                GameScreen.LoseScreen.Model.Status = false;
                CorutineGame.Instance.Wait(2, () => SceneLoader.Restart());
            };
        }
    }
}