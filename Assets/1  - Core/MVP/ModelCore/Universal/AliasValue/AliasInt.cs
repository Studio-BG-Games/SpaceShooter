using System;

namespace ModelCore.Universal.AliasValue
{
    public class AliasInt : BaseAliasValue<int>
    {
        protected override string PrefixValue() => "I";

        public AliasInt(string alias, int value) : base(alias, value) {}
    }
}