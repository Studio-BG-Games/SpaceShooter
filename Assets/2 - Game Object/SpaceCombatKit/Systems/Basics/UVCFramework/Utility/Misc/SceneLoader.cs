using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Load a scene using a name or a build index.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {

        [SerializeField]
        protected string sceneName;

        /// <summary>
        /// Load a scene with the name set in the inspector.
        /// </summary>
        public void LoadScene()
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Load a scene with a specified name.
        /// </summary>
        /// <param name="sceneName">The name of the scene.</param>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Load a scene with a specified build index.
        /// </summary>
        /// <param name="sceneBuildIndex">The build index of the scene to load.</param>
        public void LoadScene(int sceneBuildIndex)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }


        /// <summary>
        /// Reload the current scene.
        /// </summary>
        public void ReloadActiveScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitApplication()
        {
            Application.Quit();
        }
    }

}
