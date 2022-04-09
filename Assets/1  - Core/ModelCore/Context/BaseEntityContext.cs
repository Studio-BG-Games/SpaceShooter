using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModelCore
{
    public abstract class BaseEntityContext : MonoBehaviour
    {
        public LabelObjectGo GetObjectWithLabelObject(Object ID) => GetComponentsInChildren<LabelObjectGo>().FirstOrDefault(x => x.IsAlias(ID));
    }
}