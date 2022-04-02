using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dreamteck.Forever
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class UniqueAssetCollection
    {
        [SerializeField] private AssetGUIDPair[] _uniqueAssets = new AssetGUIDPair[0];

        private List<Object> _uniqueAssetsList = new List<Object>();

        public AssetGUIDPair[] assets
        {
            get
            {
                return _uniqueAssets;
            }
        }

        public bool ContainsAsset(AssetGUIDPair asset)
        {
            int search = BinarySearchAssets(asset.guid, asset.searchID);
            while (search < _uniqueAssets.Length && _uniqueAssets[search].searchID == asset.searchID)
            {
                if (_uniqueAssets[search].guid == asset.guid)
                {
                    return true;
                }
                search++;
            }
            return false;
        }

        private int BinarySearchAssets(string guid, int key)
        {
            int minNum = 0;
            int maxNum = _uniqueAssets.Length - 1;
            int index = 0;
            while (minNum <= maxNum)
            {
                int mid = (minNum + maxNum) / 2;
                if (key == _uniqueAssets[mid].searchID)
                {
                    index = ++mid;
                    break;
                }
                else if (key < _uniqueAssets[mid].searchID)
                {
                    maxNum = mid - 1;
                }
                else
                {
                    minNum = mid + 1;
                }
            }

            //Make sure we are returning the first occurance of this key
            while (index > 0 && _uniqueAssets[index - 1].searchID == key)
            {
                index--;
            }
            return index;
        }

#if UNITY_EDITOR
        public void ExtractUniqueAssets(Object searchRoot, PreserveObjectsData excludeObjects = null)
        {
            //Get unique assets from the given root
            List<AssetGUIDPair> extracted = GetUniqueAssets(searchRoot);
            //Exclude objects from unloading
            if (excludeObjects != null)
            {
                for (int i = 0; i < extracted.Count; i++)
                {
                    if (excludeObjects.ContainsAsset(extracted[i]))
                    {
                        extracted.RemoveAt(i);
                        i--;
                    }
                }
            }
            _uniqueAssets = extracted.ToArray();
        }
        public void ClearExtractedAssets()
        {
            _uniqueAssetsList.Clear();
            _uniqueAssets = new AssetGUIDPair[0];
        }

        private List<AssetGUIDPair> GetUniqueAssets(Object searchRoot)
        {
            List<Object> searchObjects = new List<Object>();
            if (searchRoot is Transform)
            {
                List<Transform> children = new List<Transform>();
                SceneUtility.GetChildrenRecursively((Transform)searchRoot, ref children);
                for (int i = 0; i < children.Count; i++)
                {
                    searchObjects.Add(children[i]);
                }
            } else
            {
                searchObjects.Add(searchRoot);
            }

            Object[] dependencies = UnityEditor.EditorUtility.CollectDependencies(searchObjects.ToArray());
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i] is Mesh || dependencies[i] is AudioClip || dependencies[i] is Material || dependencies[i] is Texture)
                {
                    AddAssetIfUnique(dependencies[i], _uniqueAssetsList);
                }
            }

            List<AssetGUIDPair> list = new List<AssetGUIDPair>();
            for (int i = 0; i < _uniqueAssetsList.Count; i++)
            {
                string guid;
                long file;
                if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_uniqueAssetsList[i], out guid, out file))
                {
                    list.Add(new AssetGUIDPair(_uniqueAssetsList[i], guid));
                }
                else
                {
                    Debug.LogError("Could not find GUID for " + _uniqueAssetsList[i].name + ". Asset will not be unloaded during runtime.");
                }
            }

            return list.OrderBy(asset => asset.searchID).ToList();
        }

        private void AddAssetIfUnique(Object asset, List<Object> list)
        {
            if (asset != null)
            {
                if (!list.Contains(asset) && UnityEditor.AssetDatabase.Contains(asset))
                {
                    list.Add(asset);
                }
            }
        }
#endif


        /// <summary>
        /// A helper class used by RemoteLevel to help identify and sort unique assets for unloading
        /// </summary>
        [System.Serializable]
        public class AssetGUIDPair
        {
            [SerializeField] private Object _asset = null;
            [SerializeField] private string _guid = "";
            [SerializeField] private int _searchID = 0;

            public Object asset
            {
                get { return _asset; }
            }

            public string guid
            {
                get { return _guid; }
            }

            public int searchID
            {
                get { return _searchID; }
            }

            public AssetGUIDPair(Object asset, string guid)
            {
                _asset = asset;
                _guid = guid;
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(guid.ToCharArray());
                for (int i = 0; i < bytes.Length; i++)
                {
                    _searchID += bytes[i];
                }
            }
        }
    }
}
