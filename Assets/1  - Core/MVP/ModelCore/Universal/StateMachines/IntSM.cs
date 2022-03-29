namespace ModelCore.Universal.StateMachines
{
    public class IntSM : StateMachine<int>
    {

        protected override State<int> Create(int aliasState) => new IntState(aliasState);

        public IntSM(string alias) : base(alias) { }
    }
}