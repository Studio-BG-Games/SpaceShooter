using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dreamteck.Forever
{
    [AddComponentMenu("Dreamteck/Forever/Remote Level")]
    public class RemoteLevel : MonoBehaviour
    {
        [HideInInspector] public SegmentSequenceCollection sequenceCollection = new SegmentSequenceCollection();
        [SerializeField] [HideInInspector] private bool _usePooling = false;
        [SerializeField] [HideInInspector] 
        [Tooltip("If checked, the level will unload all unique registered assets when destroyed.")] 
        private bool _unloadAssetsOnDestroy = false;
        [SerializeField] [HideInInspector] private PreserveObjectsData _preventObjectsFromUnloading;

        [SerializeField] [HideInInspector] private UniqueAssetCollection _assetCollection = new UniqueAssetCollection();
        public bool usePooling
        {
            get { return _usePooling; }
        }

        public bool unloadAssetsOnDestroy
        {
            get { return _unloadAssetsOnDestroy; }
        }

        public UniqueAssetCollection assetCollection
        {
            get
            {
                return _assetCollection;
            }
        }

        public PreserveObjectsData preventObjectsFromUnloading
        {
            get; set;
        }

        private bool ContainsAsset(UniqueAssetCollection.AssetGUIDPair asset)
        {
            return _assetCollection.ContainsAsset(asset);
        }

        private void OnDestroy()
        {
            if(_usePooling && _unloadAssetsOnDestroy)
            {
                List<ForeverLevel> loaded = LevelGenerator.instance.loadedLevels;
                for (int i = loaded.Count-1; i >= 0 ; i--)
                {
                    if (loaded[i].associatedRemoteLevel == null)
                    {
                        loaded.RemoveAt(i);
                    }
                }

                int loadedIndex = 0;
                for (int i = 0; i < loaded.Count; i++)
                {
                    if(LevelGenerator.instance.loadedLevels[i].associatedRemoteLevel == this)
                    {
                        loadedIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < _assetCollection.assets.Length; i++)
                {
                    for (int j = loadedIndex + 1; j < loaded.Count; j++)
                    {
                        if (!loaded[j].associatedRemoteLevel.ContainsAsset(_assetCollection.assets[i]))
                        {
                            Resources.UnloadAsset(_assetCollection.assets[i].asset);
                        }
                    }
                }
            }
        }

       

#if UNITY_EDITOR
        public void EditorCacheAssets()
        {
            _assetCollection.ClearExtractedAssets();
            if (!_usePooling || !_unloadAssetsOnDestroy) return;
            _assetCollection.ClearExtractedAssets();
            foreach (Transform child in transform)
            {
                if (child == transform) continue;
                _assetCollection.ExtractUniqueAssets(child, _preventObjectsFromUnloading);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
