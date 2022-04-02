using DIContainer;
using Plugins.GameStateMachines;
using Plugins.GameStateMachines.States;
using UnityEngine;

namespace DefaultNamespace
{
    public class StartGame : MonoBehaviour
    {
        [DI] private AppStateMachine _appStateMachine;

        public void EnterToGameScene()
        {
            _appStateMachine.Enter<GameScene, DataGameScene>(new DataGameScene());
        }
        
        public void EnterToTutotrScene()
        {
            _appStateMachine.Enter<TutorScene>();
        }
    }
}