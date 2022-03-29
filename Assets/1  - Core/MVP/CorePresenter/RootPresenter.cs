using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CorePresenter.ContextAndReciver;
using ModelCore;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UltEvents;
using Unity.Collections;
using UnityEngine;

namespace CorePresenter
{
    [AddComponentMenu("MV*/Root Presenter", 0)]
    public class RootPresenter : MonoBehaviour
    {
        public const string PathToPresenters = "MV*/Part Presenter/";
        public const string PathToView = "MV*/Part Views/";

        [FoldoutGroup("Paths")][HorizontalGroup("Paths/Hor")]
        [ShowInInspector, MultiLineProperty(20), HideLabel, PropertyOrder(-1), BoxGroup("Paths/Hor/Left")] public string Paths;
        [ShowInInspector, MultiLineProperty(20), HideLabel, PropertyOrder(-1), BoxGroup("Paths/Hor/Right")] public string ObjectPath;
        [ShowInInspector] private List<Presenter> _viewModels;

        public RootModel CurrentRootModel { get; private set; }
        public UltEvent<RootModel> Updated;

        private void Awake() => SetAll();

        public void Init(RootModel rootModel)
        {
            CurrentRootModel = rootModel;
            if(_viewModels==null) SetAll();
            _viewModels.ForEach(x => x.Init(rootModel));
        }

        [Button]
        public void ReInit() => Init(CurrentRootModel);

        [Button]
        private void SetAll()
        {
            _viewModels = GetComponentsInChildren<Presenter>().Where(x=>(x is Reciver)==false).ToList();
            var otherRoots = GetComponentsInChildren<RootPresenter>().Except(new []{this});
            otherRoots.ForEach(x => x.SetAll());
            otherRoots.ForEach(x => _viewModels = _viewModels.Except(x._viewModels).ToList());
        }

        private void LogAllPath(out string path, out string objectpath)
        {
            path = "";
            objectpath = "";
            int index = 1;
            foreach (var presenter in _viewModels)
            {
                path += $"{index}: {presenter.PathForFind()} \n";
                objectpath += $"{index}: {GetObjectName(presenter)}\n";
                index++;
            }
        }

        private string GetObjectName(Presenter presenter) => $"Object:({presenter.name})";

        [OnInspectorInit] 
        private void InpectorInit()
        {
            SetAll();
            LogAllPath(out Paths, out ObjectPath);
        }
    }
}