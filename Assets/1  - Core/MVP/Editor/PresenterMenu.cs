using System.IO;
using System.Linq;
using ModelCore;
using UnityEditor;
using UnityEngine;

namespace CorePresenter.Editor
{
    public class PresenterMenu
    {
        [MenuItem("MVP/Auto document")]
        public static void DocumentModel()
        {
            var types = typeof(RootModel).Assembly.GetTypes().
                Where(x => x.FullName.Split('.')[0] == typeof(RootModel).FullName.Split('.')[0])
                .ToArray();
            var str = AutoDocumentaion.DocumentationTypes(types);
            EditorGUIUtility.systemCopyBuffer = str;
            File.WriteAllText("Assets/Doc Model.txt", str);
            AssetDatabase.Refresh();
            Debug.Log("Документация скопирована\n"+str);
        }
    }
}