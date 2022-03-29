using System.Runtime.InteropServices;
using ModelCore;
using ModelCore.Universal;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace CorePresenter.UniversalPart
{
    [AddComponentMenu(RootPresenter.PathToPresenters+"P_FuncToJs")]
    public class P_FuncToJs : Presenter
    {
        [InfoBox("Как писать путь функцит: Obj1.Obj2.NameFunc")] 
        [InfoBox("$HasFunc")] 
        public string PathToFunc;
        
        public string HasFunc()
        {
            if (Application.isPlaying)
            {
                if (Model == null) return "Нет движка со скриптов по пути " + PathToModel;
                else return Model.HasFunc(PathToFunc) ? "Есть функция" : "Нет Функции";
            }
            else return "игра не запущена, ничего не могу сказать";
        }

        [Button]
        private void Swap()
        {
            var temp = PathToModel;
            PathToModel = PathToFunc;
            PathToFunc = temp;
        }

        protected JsEn Model;
        public override void Init(RootModel rootModel)
        {
            Model = GetModel<JsEn>(rootModel, x => x.IdModel == PathToModel);
            if(Model == null) Debug.LogWarning($"P_FuncJs не может получить скрипт по пути, {PathToModel}", this);
        }

        public void Invoke(){ Model?.Invoke(PathToFunc);}
        public void Invoke(object o){ Model?.InvokeParam(PathToFunc, o);}
        public void Invoke(object o, object o1){ Model?.InvokeParam(PathToFunc, o, o1);}
        public void Invoke(object o, object o1, object o2){ Model?.InvokeParam(PathToFunc, o, o1, o2);}
        public void Invoke(object o, object o1, object o2, object o3){ Model?.InvokeParam(PathToFunc, o, o1, o2, o3);}

        private static string BuildStr(string[] strs)
        {
            string result = "";
            strs.ForEach(x => result += x + "\n");
            return result;
        }
    }
}