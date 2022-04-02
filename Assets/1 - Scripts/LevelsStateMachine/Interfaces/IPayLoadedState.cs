namespace Plugins.GameStateMachines.Interfaces
{
    public interface IPayLoadedState<TPay> : IExitableState
    {
        void Enter(TPay dataScene);
    }
}