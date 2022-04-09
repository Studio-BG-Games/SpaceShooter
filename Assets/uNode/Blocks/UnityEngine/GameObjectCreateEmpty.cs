using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "GameObject.CreateEmpty")]
	public class GameObjectCreateEmpty : Action {
		[ObjectType(typeof(string))]
		public MemberData objectName = new MemberData("");
		public bool useTransform = true;
		[Hide("useTransform", false)]
		[ObjectType(typeof(Transform))]
		public MemberData transform;
		[Hide("useTransform", true)]
		[ObjectType(typeof(Vector3))]
		public MemberData position = new MemberData(Vector3.zero);
		[Hide("useTransform", true)]
		[ObjectType(typeof(Vector3))]
		public MemberData rotation = new MemberData(Vector3.zero);
		[Filter(typeof(GameObject), SetMember = true)]
		public MemberData storeResult;

		protected override void OnExecute() {
			GameObject go = null;
			if(objectName.isAssigned) {
				go = new GameObject(objectName.Get<string>());
			} else {
				go = new GameObject();
			}
			if(useTransform) {
				go.transform.position = transform.Get<Transform>().position;
				go.transform.eulerAngles = transform.Get<Transform>().eulerAngles;
			} else {
				go.transform.position = position.Get<Vector3>();
				go.transform.eulerAngles = rotation.Get<Vector3>();
			}
			if(storeResult.isAssigned) {
				storeResult.Set(go);
			}
		}

		public override string GenerateCode(Object obj) {
			if(useTransform && !transform.isAssigned || !useTransform && (!position.isAssigned || !rotation.isAssigned)) return null;
			string go = CG.GenerateVariableName("go", this);
			string data = null;
			data += CG.Type(typeof(GameObject)) + " " + go + " = new GameObject(" + 
				(objectName.isAssigned ? CG.Value((object)objectName) : "") + ");";
			data += go + ".transform.position = " +
				(useTransform ? CG.Value((object)transform) + ".position" : CG.Value((object)position)) + ";";
			data += go + ".transform.eulerAngles = " +
				(useTransform ? CG.Value((object)transform) + ".eulerAngles" : CG.Value((object)rotation)) + ";";
			if(storeResult.isAssigned) {
				data += CG.Set(go, storeResult);
			}
			return data;
		}

		public override string GetDescription() {
			return "Creates a new game object.";
		}
	}
}