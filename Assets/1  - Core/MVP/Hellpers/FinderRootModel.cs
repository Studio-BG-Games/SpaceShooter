using CorePresenter;
using CorePresenter.UniversalPart;
using ModelCore;
using UltEvents;
using UnityEngine;

namespace PartsPresenters.FinderRootModel
{
    public abstract class FinderRootModel : MonoBehaviour
    {
        protected RootModel TryFind(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<P_AccsesModel>(out var r))
                if (r.Model != null)
                    return r.Model;
            return null;
        }
        
    }
}