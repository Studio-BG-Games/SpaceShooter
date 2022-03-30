using System.IO;
using DIContainer;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace ModelCore
{
    public static class LoaderRoot
    {
        public static string LoadJson(string path, string name)
        {
            if (File.Exists(GetPathFile(path, name)))
                return File.ReadAllText(GetPathFile(path, name));
            Debug.Log($"Файл по пути {GetPathFile(path, name)} не существует");
            return "";
        }

        public static RootModel LoadModel(string path, string name, bool initAuto = true) => ModelFromJson(LoadJson(path, name), initAuto);

        public static void SaveJson(string content, string nameFile, string path)
        {
            if (!Directory.Exists(GetPathDir(path))) Directory.CreateDirectory(GetPathDir(path));
            var filePath = GetPathFile(path, nameFile);
            File.WriteAllText(filePath, content);
        }

        public static void SaveModel(RootModel model, string nameFile, string path) => SaveJson(model.Save(), nameFile, path);

        [CanBeNull] public static RootModel ModelFromJson(string jsonFile, bool initAuto = true)
        {
            var result = JsonConvert.DeserializeObject<RootModel>(jsonFile, RootModel.Factory.SettingJson());
            if(initAuto && result != null) result.Init();
            return result;
        }

        private static string GetPathDir(string path) => Application.dataPath + "/Save/" + path;

        private static string GetPathFile(string path, string name) => GetPathDir(path) + "/" + name + ".json";
    }
}