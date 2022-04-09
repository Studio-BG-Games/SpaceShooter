using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "RestartScene")]
	public class SceneRestartScene : Action {
		private LoadSceneMode loadMode = LoadSceneMode.Single;

		protected override void OnExecute() {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, loadMode);
		}

		public override string GenerateCode(Object obj) {
			string scene = CG.FlowInvoke(typeof(SceneManager), "GetActiveScene") + ".buildIndex";
			return CG.FlowInvoke(typeof(SceneManager), "LoadScene", scene, CG.Value(loadMode));
		}

		public override string GetDescription() {
			return "Restart active scene.";
		}
	}
}