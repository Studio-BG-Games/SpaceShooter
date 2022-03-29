using UnityEngine;

namespace ModelCore.Universal.AliasValue
{
    public class AliasVector2 : BaseAliasValue<Vector2>
    {
        public AliasVector2(string alias, Vector2 value) : base(alias, value)
        {
        }

        public float X
        {
            get => Value.x;
            set => Value = new Vector2(value, Y);
        }
        
        public float Y
        {
            get => Value.y;
            set => Value = new Vector2(X,  value);
        }

        protected override string PrefixValue() => "V2";
    }
}