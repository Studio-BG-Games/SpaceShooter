using Newtonsoft.Json;
using UnityEngine;

namespace DefaultNamespace
{
    public static class ExtensionCore 
    {
        public static T DeepCloneJson<T>(this T target)
        {
            var setting = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(target, setting));
        }
    }
}