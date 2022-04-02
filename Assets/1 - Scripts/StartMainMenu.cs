using DIContainer;
using Plugins.GameStateMachines;
using Plugins.GameStateMachines.States;
using UnityEngine;

namespace DefaultNamespace
{
    public class StartMainMenu : MonoBehaviour
    {
        [DI] private AppStateMachine _appStateMachine;

        public void Enter()
        {
            _appStateMachine.Enter<MainMenu>();
        }
    }
}