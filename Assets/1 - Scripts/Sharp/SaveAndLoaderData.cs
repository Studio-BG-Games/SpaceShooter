using System.IO;
using ModelCore;
using ModelCore.Universal;
using Sirenix.Utilities;

namespace Sharp
{
    [Info("Этот скрипт позволяет сохрнять и загружать определнный рут по соседству с собою. Загрузка происходит с копированием значений полей из save в оригинал. Если у оригинал нет каких либо полей, они добавяться")]
    public class SaveAndLoaderData : CsEn
    {
        public override string IdModel => $"{Prefics}SaveLoad({TargetRoot})";

        public string TargetRoot;
        public string RelitivePath;
        public string NameFile;

        public override void InitByModel()
        {
            var path = Path.Combine(RelitivePath, $"{NameFile}");
            var loadetRoot = LoaderRoot.LoadModel(RelitivePath, NameFile, false);
            var targetRoot = Root["Root_" + TargetRoot] as RootModel;
            
            if (targetRoot == null)
            {
                Root.Logger.LogError($"{IdModel} не может получить целевой объект Root_{TargetRoot}");
                return;
            }

            if (loadetRoot == null)
            {
                Root.Logger.LogWarning($"{IdModel} не удалось загрузить сохранения по пути, будет создано автоматически: {path}");
                LoaderRoot.SaveModel(targetRoot, NameFile, RelitivePath);
            }
            else
            {
                loadetRoot.Slaves.ForEach(x =>
                {
                    if (targetRoot[x.IdModel] != null) targetRoot[x.IdModel].CopyFrom(x);
                    else targetRoot.AddModel(x.Clone());
                });
            }
            
        }

        public override string Valid()
        {
            //if (!Root.CanRename(this, IdModel)) return "Не могу поменять имя, так как оно уже есть";
            //else if (Root[IdModel] == null)
            
            Rename(IdModel);

            if ((Root["Root_" + TargetRoot] as RootModel) != null)
            {
                return "";
            }
            else return $"Не могу найти root по id: Root_{TargetRoot}";
        }
    }
}