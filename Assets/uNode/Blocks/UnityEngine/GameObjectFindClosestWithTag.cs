using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "GameObject.FindClosestWithTag")]
	public class GameObjectFindClosestWithTag : Action {
		[ObjectType(typeof(Transform))]
		public MemberData from;
		[ObjectType(typeof(string))]
		public MemberData searchTag;
		[Filter(SetMember = true)]
		[ObjectType(typeof(GameObject))]
		public MemberData storeObject;
		[Filter(SetMember = true)]
		[ObjectType(typeof(float))]
		public MemberData storeDistance;

		protected override void OnExecute() {
			var gameObjects = GameObject.FindGameObjectsWithTag(searchTag.Get<string>());
			GameObject closest = null;
			var dis = Mathf.Infinity;
			foreach(var go in gameObjects) {
				var newDis = Vector3.Distance(go.transform.position, from.Get<Transform>().position);
				if(newDis < dis) {
					dis = newDis;
					closest = go;
				}
			}
			if(storeObject.isAssigned) {
				storeObject.Set(closest);
			}
			if(storeDistance.isAssigned) {
				storeDistance.Set(dis);
			}
		}

		public override string GenerateCode(Object obj) {
			string gameObjects = CG.GenerateVariableName("gameObjects", this);
			string closest = CG.GenerateVariableName("closest", this);
			string dis = CG.GenerateVariableName("dis", this);
			string result = null;
			//GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(searchTag);
			result += CG.DeclareVariable(gameObjects, typeof(GameObject[]), CG.Invoke(typeof(GameObject), "FindGameObjectsWithTag", searchTag.CGValue()), false);
			//GameObject closest = null;
			result += CG.DeclareVariable(closest, typeof(GameObject), null).AddLineInFirst();
			//float dis = Mathf.Infinity;
			result += CG.DeclareVariable(dis, typeof(float), CG.Type(typeof(Mathf)) + ".Infinity", false).AddLineInFirst();
			string go = CG.GenerateVariableName("go", this);
			string newDis = CG.GenerateVariableName("newDis", this);
			{//foreach contents
				string contents = CG.DeclareVariable(newDis, typeof(float), CG.Invoke(typeof(Vector3), "Distance", go + ".transform.position", CG.Value(from) + ".position"), false);
				{//If contents
					string ifContents = CG.Set(dis, newDis);
					ifContents += CG.Set(closest, go).AddLineInFirst();
					contents += CG.If(CG.Compare(newDis, dis, ComparisonType.LessThan), ifContents).AddLineInFirst();
				}
				result += CG.Foreach(typeof(GameObject), go, gameObjects, contents).AddLineInFirst();
			}
			{//Store result
				if(storeObject.isAssigned) {
					result += CG.Set(storeObject, CG.WrapString(closest)).AddLineInFirst();
				}
				if(storeDistance.isAssigned) {
					result += CG.Set(storeDistance, CG.WrapString(dis)).AddLineInFirst();
				}
			}
			return result;
		}

		public override string GetDescription() {
			return "Find the closest game object of tag to the from.";
		}
	}
}