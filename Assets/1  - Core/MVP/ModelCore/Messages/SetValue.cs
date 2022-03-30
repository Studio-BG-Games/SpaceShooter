using ModelCore.Universal;
using ModelCore.Universal.AliasValue;
using UnityEngine;

namespace ModelCore.Messages
{
    public abstract class SetValue<T> : Message
    {
        public T Value;
    }

    [CustomPath("Setter Values")] public class SetBool : SetValue<bool> { }
    
    [CustomPath("Setter Values")] public class Setfloat : SetValue<float> { }
    
    [CustomPath("Setter Values")] public class SetInt : SetValue<int> { }
    
    [CustomPath("Setter Values")] public class SetString : SetValue<string> { }
    
    [CustomPath("Setter Values")] public class SetV2 : SetValue<Vector2> { }
    
    [CustomPath("Setter Values")] public class SetV3 : SetValue<Vector3> { }

    public interface ISetValueToChildRoot
    {
        void TrySet(RootModel model);
    }
    
    public abstract class SetValueToChildRoot<T> : Message, ISetValueToChildRoot
    {
        public T Value;
        public string Path;
        
        public void TrySet(RootModel model)
        {
            var aliasValue = model[Path] as BaseAliasValue<T>;
            if(aliasValue==null) return;
            aliasValue.Value = Value;
        }
    }
    
    [CustomPath("Setter Throw Root")] public class SetBooToChildRoot : SetValueToChildRoot<bool> { }
    
    [CustomPath("Setter Throw Root")] public class SetfloatToChildRoot : SetValueToChildRoot<float> { }
    
    [CustomPath("Setter Throw Root")] public class SetIntToChildRoot : SetValueToChildRoot<int> { }
    
    [CustomPath("Setter Throw Root")] public class SetStringToChildRoot : SetValueToChildRoot<string> { }
    
    [CustomPath("Setter Throw Root")] public class SetV2ToChildRoot : SetValueToChildRoot<Vector2> { }
    
    [CustomPath("Setter Throw Root")] public class SetV3ToChildRoot : SetValueToChildRoot<Vector3> { }
}