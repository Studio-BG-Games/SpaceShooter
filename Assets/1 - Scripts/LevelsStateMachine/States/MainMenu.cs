using DIContainer;
using Infrastructure;
using Plugins.GameStateMachines.Interfaces;
using UnityEditor.SearchService;

namespace Plugins.GameStateMachines.States
{
    public class MainMenu : IEnterState
    {
        public void Enter()
        {
            SceneLoader.Load("Menu");
        }

        public void Exit()
        {
            
        }
    }

    public class GameScene : IPayLoadedState<DataGameScene>
    {
        public void Enter(DataGameScene dataScene)
        {
            DiBox.MainBox.RegisterSingle(dataScene);
            SceneLoader.Load("Game");
        }

        public void Exit()
        {
            DiBox.MainBox.RemoveSingel<DataGameScene>();
        }
    }
    
    public class TutorScene : IEnterState
    {
        public void Enter()
        {
            SceneLoader.Load("Tutor"); 
        }

        public void Exit()
        {
        }
    }


    public class DataGameScene
    {
    }
}