using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure
{
    public static class SceneLoader
    {
        public static void Load(string name, Action onLoaded = null) =>
            CorutineGame.Instance.StartCoroutine(LoadScene(name, onLoaded));
        
        private static IEnumerator LoadScene(string name, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == name)
            {
                onLoaded?.Invoke();
                yield break;
            }
            
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(name);

            while (!waitNextScene.isDone)
                yield return null;
            
            onLoaded?.Invoke();
        }
    }
}
