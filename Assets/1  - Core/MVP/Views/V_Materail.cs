using DIContainer;
using ManagerResourcess;
using ModelCore;
using ModelCore.Universal.AliasValue;
using Sirenix.Utilities;
using UnityEngine;

namespace MVP.Views
{
    
    public class V_Materail : ViewRootBase
    {
        [DI] private PackOfResources _pack;
        [SerializeField] private MeshRenderer[] _meshes = new MeshRenderer[0];

        public override void View(RootModel engine)
        {
            var packName = engine.GetIdT<AliasString>("S_Pack");
            var idMaterial = engine.GetIdT<AliasString>("S_Id");

            var pack = (_pack.Get(packName.Value) as MaterialPack);
            if (pack == null)
            {
                Debug.LogWarning($"Нету пака с материалми по имени {packName.Value}");
                return;
            }

            var materail = pack.Get(idMaterial.Value);
            _meshes.ForEach(x => x.material = materail);
        }
        
        protected override string GetInfo()
        {
            return $"S_PackName - имя контейнера с материалами\nS_IdMat - id материала в контейнера";
        }
    }
}