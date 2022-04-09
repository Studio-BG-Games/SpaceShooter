using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "LoadScene")]
	public class SceneLoadScene : Action {
		public bool useIndex;
		[Hide("useIndex", true)]
		[Filter(typeof(string), InvalidTargetType = MemberData.TargetType.Null)]
		public MemberData sceneName = new MemberData("");
		[Hide("useIndex", false)]
		[ObjectType(typeof(int))]
		public MemberData sceneBuildIndex = new MemberData(0);
		public UnityEngine.SceneManagement.LoadSceneMode loadMode = UnityEngine.SceneManagement.LoadSceneMode.Single;

		protected override void OnExecute() {
			if(!useIndex) {
				UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName.Get<string>(), loadMode);
			} else {
				UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex.Get<int>(), loadMode);
			}
		}

		public override string GenerateCode(Object obj) {
			string scene;
			if(!useIndex) {
				scene = CG.Value((object)sceneName);
			} else {
				scene = CG.Value((object)sceneBuildIndex);
			}
			return CG.FlowInvoke(typeof(UnityEngine.SceneManagement.SceneManager), "LoadScene", scene, CG.Value(loadMode));
		}

		public override string GetDescription() {
			return "Loads the Scene by its name or index in Build Settings.";
		}
	}
}