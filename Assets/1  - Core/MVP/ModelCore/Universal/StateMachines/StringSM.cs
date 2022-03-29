namespace ModelCore.Universal.StateMachines
{
    public class StringSM : StateMachine<string>
    {
        protected override State<string> Create(string aliasState) => new StringState(aliasState);

        public StringSM(string alias) : base(alias)
        {
        }
    }
}