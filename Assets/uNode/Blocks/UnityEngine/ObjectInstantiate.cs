using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Object.Instantiate", hideOnBlock =true)]
	public class ObjectInstantiate : Action {
		[ObjectType(typeof(Object))]
		public MemberData targetObject;
		public bool useTransform = true;
		[Hide("useTransform", false)]
		[ObjectType(typeof(Transform))]
		public MemberData transform;
		[Hide("useTransform", true)]
		[ObjectType(typeof(Vector3))]
		public MemberData position;
		[Hide("useTransform", true)]
		[ObjectType(typeof(Vector3))]
		public MemberData rotation;
		[ObjectType(typeof(Transform))]
		public MemberData parent;
		[Filter(SetMember = true)]
		[ObjectType("targetObject")]
		public MemberData storeResult;

		protected override void OnExecute() {
			if(targetObject.isAssigned) {
				Vector3 pos;
				Vector3 rot;
				if(useTransform) {
					pos = transform.Get<Transform>().position;
					rot = transform.Get<Transform>().eulerAngles;
				} else {
					pos = position.Get<Vector3>();
					rot = rotation.Get<Vector3>();
				}
				Object obj = Object.Instantiate(targetObject.Get<Object>(), pos, Quaternion.Euler(rot), parent.Get<Transform>());
				if(storeResult.isAssigned) {
					storeResult.Set(obj);
				}
			}
		}

		public override string GenerateCode(Object obj) {
			if(targetObject.isAssigned) {
				string data = CG.Value((object)storeResult);
				string pos = null;
				string rot = null;
				if(useTransform) {
					pos = CG.Value((object)transform) + ".position";
					rot = CG.Value((object)transform) + ".eulerAngles";
				} else {
					pos = CG.Value((object)position);
					rot = CG.Value((object)rotation);
				}
				rot = CG.FlowInvoke(typeof(Quaternion), "Euler", rot).Replace(";", "");
				if(string.IsNullOrEmpty(data)) {
					return CG.FlowInvoke(typeof(Object), "Instantiate", CG.Value((object)targetObject), pos, rot, CG.Value((object)parent));
				} else {
					return data + " = " + CG.FlowInvoke(typeof(Object), "Instantiate", CG.Value((object)targetObject), pos, rot, CG.Value((object)parent)).Replace(";", "") + " as " + CG.Type(storeResult.type) + ";";
				}
			}
			return null;
		}
	}
}