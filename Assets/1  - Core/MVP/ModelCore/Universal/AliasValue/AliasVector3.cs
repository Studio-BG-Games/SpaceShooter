using UnityEngine;

namespace ModelCore.Universal.AliasValue
{
    public class AliasVector3 : BaseAliasValue<Vector3>
    {
        public AliasVector3(string alias, Vector3 value) : base(alias, value)
        {
        }
        
        public float X
        {
            get => Value.x;
            set => Value = new Vector3(value, Y, Z);
        }
        
        public float Y
        {
            get => Value.y;
            set => Value = new Vector3(X,  value, Z);
        }
        
        public float Z
        {
            get => Value.z;
            set => Value  = new Vector3(X,  Y, value);
        }


        protected override string PrefixValue() => "V3";
    }
}