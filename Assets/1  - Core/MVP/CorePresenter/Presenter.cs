using System;
using System.Linq;
using ModelCore;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace CorePresenter
{
    public abstract class Presenter : MonoBehaviour
    {
        [InfoBox("$GetModelTree")]
        [InfoBox("Example Alias: IsPlay, Speed.\nNot: B_IsPlay, F_Speed", "isFirst")]
        [InfoBox("Example Alias: B_IsPlay, Root_M1/Root_M2/F_Health\nNot: IsPlay, Speed, M1/M2/Health", "isSecond")]
        [HideIf("isHideAll")]
        [EnumToggleButtons, ShowInInspector, SerializeField]protected Method method = Method.SecondQue;
        
        [HideIf("isHideAll")][MultiLineProperty(3)]
        [ShowInInspector][SerializeField]protected string PathToModel = "enter path";

        public string PathForFind()
        {
            if (method == Method.FirstQue) return $"*Type*_{PathToModel}";
            else return PathToModel;
        }

        private bool isFirst => method == Method.FirstQue;
        private bool isSecond => method == Method.SecondQue;

        protected virtual bool isHideAll => false;

        public abstract void Init(RootModel rootModel);

        protected T GetModel<T>(RootModel rootModel, Func<T, bool> predict) where T : Model
        {
            if (method == Method.FirstQue) return rootModel.Select<T>(predict);
            else return rootModel[PathToModel] as T;
        }

        private string GetModelTree()
        {
            string result = "";
            var elements = PathToModel.Split('/');
            for (var i = 0; i < elements.Length; i++)
            {
                if (i > 0)
                {
                    result += string.Concat(Enumerable.Repeat("  ", i+1)) + "*" + elements[i] + "\n";
                }
                else
                {
                    result = "*Main*\n  *"+elements[i] + "\n";
                }
            }

            return result;
        }
        
        public enum Method { FirstQue, SecondQue }
    }
}