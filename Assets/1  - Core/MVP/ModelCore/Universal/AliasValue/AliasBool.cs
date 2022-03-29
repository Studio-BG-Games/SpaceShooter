using System;

namespace ModelCore.Universal.AliasValue
{
    public class AliasBool : BaseAliasValue<bool>{
        
        public AliasBool(string alias, bool value) : base(alias, value) {}

        protected override string PrefixValue() => "B";
    }
}