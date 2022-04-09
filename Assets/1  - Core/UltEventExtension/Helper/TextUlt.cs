using System;
using UnityEngine;

namespace CorePresenter.UltEventExtension.Helper
{
    [AddComponentMenu("MV*/Event mediator/Helper-String")]
    public class TextUlt : MonoBehaviour
    {
        public string Convert(object o) => o.ToString();
        public string Convert(int x) => x.ToString();
        public string Convert(float x) => x.ToString();
        public string Convert(bool x) => x.ToString();

        public string Format(string str, object o) => String.Format(str, o);
        public string Format(string str, object o, object o1) => String.Format(str, o, o1);
        public string Format(string str, object o, object o1, object o2) => String.Format(str, o, o1, o2);
        public string Format(string str, object o, object o1, object o2, object o3) => String.Format(str, o, o1, o2, o3);
    }
}