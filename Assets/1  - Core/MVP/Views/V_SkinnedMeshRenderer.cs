using DIContainer;
using ManagerResourcess;
using ModelCore;
using ModelCore.Universal.AliasValue;
using UnityEngine;

namespace MVP.Views
{
    public class V_SkinnedMeshRenderer : ViewRootBase
    {
        [DI] private PackOfResources _packOfResources; 
        
        public SkinnedMeshRenderer MeshRenderer;

        public override void View(RootModel engine)
        {
            var container = engine.GetIdT<AliasString>("S_Pack");
            var idMesh = engine.GetIdT<AliasString>("S_Id");
            
            if(Log(container, $"У {engine.Alias} нет AliasString по S_Pack")) return;
            if(Log(idMesh, $"У {engine.Alias} нет AliasString по S_Id")) return;

            var pack = (_packOfResources.Get(container.Value) as PackMesh);
            if (Log(pack, $"В Pack Of Resources нет PackMesh по {container.Value}")) return;
            
            var mesh = pack.Get(idMesh.Value);
            if (Log(mesh, $"Нет Mesh по id {idMesh.Value}")) return;

            MeshRenderer.sharedMesh = mesh;
        }

        protected override string GetInfo()
        {
            return $"S_ContainerAlias - имя контейнера с мешами\nS_MeshId - id меша в контейнера";
        }
    }
}