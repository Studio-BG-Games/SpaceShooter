namespace Dreamteck.Forever
{
    using UnityEngine;
    using System.Collections.Generic;

    [AddComponentMenu("Dreamteck/Forever/Builders/Active Random Children")]
    public class ActiveRandomChildren : Builder, ISerializationCallbackReceiver
    {
        [SerializeField] private ForeverRandomizer _randomizer = null;
        [SerializeField] [HideInInspector] private bool _hasRandomizer = false;
        [Range(0f, 1f)]
        public float minPercent = 0f;
        [Range(0f, 1f)]
        public float maxPercent = 1f;
        private float percent = 0f;


        protected override void Awake()
        {
            base.Awake();
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        public override void OnUnpack()
        {
            foreach (Transform child in transform) child.gameObject.SetActive(true);
        }
#endif

        protected override void Build()
        {
            base.Build();
            if (_hasRandomizer)
            {
                _randomizer.Initialize();
            }
            Transform trs = transform;
            percent = Mathf.Lerp(minPercent, maxPercent, GetRandom());
            if (trs.childCount == 0)  return;
            List<int> available = new List<int>();
            for (int i = 0; i < trs.childCount; i++) available.Add(i);
            int activeCount = Mathf.RoundToInt(trs.childCount * percent);
            for (int i = 0; i < activeCount; i++)
            {
                int rand = GetRandomInt(available.Count);
                trs.GetChild(available[rand]).gameObject.SetActive(true);
                available.RemoveAt(rand);
            }
        }

        private float GetRandom()
        {
            return _hasRandomizer ? _randomizer.Next01() : Random.Range(0f, 1f);
        }

        private int GetRandomInt(int max)
        {
            max--;
            if(max < 0)
            {
                max = 0;
            }
            return Mathf.RoundToInt(GetRandom() * max);
        }

        public void OnBeforeSerialize()
        {
            _hasRandomizer = _randomizer != null;
        }

        public void OnAfterDeserialize()
        {

        }
    }
}
