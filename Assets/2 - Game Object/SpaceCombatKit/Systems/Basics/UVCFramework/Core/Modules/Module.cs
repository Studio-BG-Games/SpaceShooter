using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Unity event for running functions when the owner's root gameobject is set.
    /// </summary>
    [System.Serializable]
    public class OnSetRootTransformEventHandler : UnityEvent <Transform> { };


    /// <summary>
    /// This class represents a module that can be loaded or unloaded on a vehicle's module mount.
    /// </summary>
    public class Module : MonoBehaviour
	{

        [Header("General")]

        [SerializeField]
        protected string label = "Module";
		public string Label { get { return label; } }

        [TextArea]
        [SerializeField]
        protected string description = "Module.";
        public string Description { get { return description; } }

        [SerializeField]
        protected string m_ID;
        public string ID { get { return m_ID; } }

        [SerializeField]
        protected List<Sprite> sprites = new List<Sprite>();
        public List <Sprite> Sprites { get { return sprites; } }

        [SerializeField]
        protected ModuleType moduleType;
		public ModuleType ModuleType { get { return moduleType; } }

        protected GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

        [Header("Mount Events")]

        public UnityEvent onMounted;

        public UnityEvent onUnmounted;

        public UnityEvent onActivated;

        public UnityEvent onDeactivated;

        protected bool isActivated = true;
        public bool IsActivated { get { return isActivated; } }

        // Module owner root gameobject set event
        public OnSetRootTransformEventHandler onSetRootTransform;
        protected List<IRootTransformUser> rootTransformUsers;


        [Header("Ownership Events")]

        public UnityEvent onOwnedByPlayer;

        public UnityEvent onOwnedByAI;

        public UnityEvent onNoOwner;


        protected void Reset()
        {
            m_ID = UnityEngine.Random.Range(0, 1000000).ToString();
        }

        protected void Awake()
        {
            rootTransformUsers = new List<IRootTransformUser>(transform.GetComponentsInChildren<IRootTransformUser>());
        }

        public virtual void SetOwner(GameAgent gameAgent)
        {
            if (gameAgent == null)
            {
                onNoOwner.Invoke();
            }
            else
            {
                if (gameAgent.IsPlayer)
                {
                    onOwnedByPlayer.Invoke();
                }
                else
                {
                    onOwnedByAI.Invoke();
                }
            }
        }

        /// <summary>
        /// Called when this module is mounted at a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount this module is to be mounted at.</param>
		public virtual void Mount(ModuleMount moduleMount)
        {
            onMounted.Invoke();
        }

        /// <summary>
        /// Called when this module is unmounted from a module mount.
        /// </summary>
		public virtual void Unmount()
        {
            onUnmounted.Invoke();
        }

        /// <summary>
        /// Pass the module owner's root gameobject to relevant components via event.
        /// </summary>
        /// <param name="ownerRootGameObject">The owner's root gameobject.</param>
        public virtual void SetRootTransform(Transform rootTransform)
        {
            onSetRootTransform.Invoke(rootTransform);
            for(int i = 0; i < rootTransformUsers.Count; ++i)
            {
                rootTransformUsers[i].RootTransform = rootTransform;
            }
        }

        public virtual void SetActivated(bool activate)
        {
            if (activate && !isActivated)
            {
                onActivated.Invoke();
            }
            else if (!activate && isActivated)
            {
                onDeactivated.Invoke();
            }

            isActivated = activate;
        }
    }
}
