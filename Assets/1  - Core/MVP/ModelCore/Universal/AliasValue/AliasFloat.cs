using System;

namespace ModelCore.Universal.AliasValue
{
    public delegate void Changed<T>(T oldV, T newV);
    
    public class AliasFloat : BaseAliasValue<float> 
    {
        public AliasFloat(string alias, float value) : base(alias, value) {}

        protected override string PrefixValue() => "F";
    }
}