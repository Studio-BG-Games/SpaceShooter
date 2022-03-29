using ModelCore;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace PartsPresenters.FinderRootModel
{
    [RequireComponent(typeof(Collider2D))]
    public class FinderRootModelTrigger : FinderRootModel
    {
        [HideLabel][TabGroup("Enter")]public SearchGroup Enter;
        [HideLabel][TabGroup("Stay")]public SearchGroup Stay;
        [HideLabel][TabGroup("Exit")]public SearchGroup Exit;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Enter.IsOn) Enter.Invoke(TryFind(other.gameObject));
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if(Stay.IsOn) Stay.Invoke(TryFind(other.gameObject));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(Exit.IsOn) Exit.Invoke(TryFind(other.gameObject));
        }

        [System.Serializable]
        public class SearchGroup
        {
            public bool IsOn;
            public UltEvent<RootModel> Finded;

            public void Invoke(RootModel root)
            {
                if(root != null) Finded.Invoke(root);
            }
        }
    }
}