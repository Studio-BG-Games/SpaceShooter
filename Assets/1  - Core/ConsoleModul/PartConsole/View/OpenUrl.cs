using UnityEngine;

namespace ConsoleModul.PartConsole.View
{
    public class OpenUrl : MonoBehaviour
    {
        public void Open(string url) => Application.OpenURL(url);
    }
}