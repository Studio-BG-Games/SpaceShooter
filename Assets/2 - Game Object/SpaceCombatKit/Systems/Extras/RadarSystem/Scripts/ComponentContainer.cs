using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat.Radar
{

    /// <summary>
    /// Empty non-generic base class is necessary to be able to show a generic class in the inspector.
    /// </summary>
    [System.Serializable]
    public class ComponentContainerParent { }

    /// <summary>
    /// Container for efficiently getting and using components.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    [System.Serializable]
    public class ComponentContainer<T> : ComponentContainerParent
    {

        public T prefab;

        [HideInInspector]
        public int lastUsedIndex = -1;

        // Stores a component in the container
        protected class CachedComponent
        {
            public T component;
            public GameObject gameObject;

            public CachedComponent(GameObject gameObject)
            {
                this.gameObject = gameObject;
                component = gameObject.GetComponent<T>();
            }
        }

        protected List<CachedComponent> cachedComponents = new List<CachedComponent>();

        public ComponentContainer (T prefab)
        {
            this.prefab = prefab;
            cachedComponents = new List<CachedComponent>();
        }

        /// <summary>
        /// Begin using the container.
        /// </summary>
        public void Begin()
        {
            if (cachedComponents == null) cachedComponents = new List<CachedComponent>();
            lastUsedIndex = -1;
        }

        /// <summary>
        /// Finish using the container.
        /// </summary>
        public void Finish()
        {
            // Disable all the unused components
            for (int i = 0; i < cachedComponents.Count; ++i)
            {
                if (i > lastUsedIndex)
                {
                    cachedComponents[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Get next available component.
        /// </summary>
        /// <param name="parent">The parent for the component.</param>
        /// <returns>The component.</returns>
        public T GetNextAvailable(Transform parent = null)
        {

            lastUsedIndex += 1;

            if (cachedComponents.Count <= lastUsedIndex)
            {
                int diff = (lastUsedIndex + 1) - cachedComponents.Count;
                for (int i = 0; i < diff; ++i)
                {
                    CreateNew(parent);
                }
            }

            if (!cachedComponents[lastUsedIndex].gameObject.activeSelf) cachedComponents[lastUsedIndex].gameObject.SetActive(true);

            return cachedComponents[lastUsedIndex].component;
        }

        protected virtual T CreateNew(Transform parent)
        {
            return default(T);
        }
    }
}
