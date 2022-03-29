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
    }
}