namespace ModelCore.Universal.AliasValue
{
    public class AliasString : BaseAliasValue<string>
    {
        public AliasString(string alias, string value) : base(alias, value!=null?value : "SomeText")
        {
        }

        protected override string PrefixValue() => "S";
    }
}