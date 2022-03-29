using System.IO;
using DIContainer;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ModelCore
{
    public static class LoaderRoot
    {
        public static string LoadJson(string path)
        {
            if (File.Exists(GetPath(path)))
                return File.ReadAllText(GetPath(path));
            Debug.Log($"Файл по пути {GetPath(path)} не существует");
            return "";
        }

        public static RootModel LoadModel(string path) => ModelFromJson(LoadJson(path));

        public static void SaveJson(string content, string nameFile, string path)
        {
            if (!Directory.Exists(GetPath(path))) Directory.CreateDirectory(GetPath(path));
            var filePath = GetPath(path) + "/" + nameFile + ".json";
            if (!File.Exists(filePath)) File.Create(filePath);
            File.WriteAllText(filePath, content);
        }

        public static void SaveModel(RootModel model, string nameFile, string path) => SaveJson(model.Save(), nameFile, path);

        public static RootModel ModelFromJson(string jsonFile, bool initAuto = true)
        {
            var result = JsonConvert.DeserializeObject<RootModel>(jsonFile, RootModel.Factory.SettingJson());
            if (result == null) result = RootModel.Empty;
            if(initAuto) result.Init();
            return result;
        }

        private static string GetPath(string path) => Application.dataPath + "/Save/" + path;
    }
}