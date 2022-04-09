using UnityEngine;
using MaxyGames.uNode;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Vector3.GetVector3XYZ")]
	public class GetVector3XYZ : Action {
		[Filter(typeof(Vector3))]
		public MemberData vector3Variable;
		[Filter(typeof(float), SetMember = true)]
		public MemberData storeX;
		[Filter(typeof(float), SetMember = true)]
		public MemberData storeY;
		[Filter(typeof(float), SetMember = true)]
		public MemberData storeZ;

		protected override void OnExecute() {
			if(vector3Variable.isAssigned) {
				Vector3 vec = (Vector3)vector3Variable.Get();
				if(storeX.isAssigned) {
					storeX.Set(vec.x);
				}
				if(storeY.isAssigned) {
					storeY.Set(vec.y);
				}
				if(storeZ.isAssigned) {
					storeZ.Set(vec.z);
				}
			}
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			if(vector3Variable.isAssigned) {
				if(storeX.isAssigned) {
					data += CG.Set(storeX.CGValue(), vector3Variable.CGAccess("x"));
				}
				if(storeY.isAssigned) {
					data += CG.Set(storeY.CGValue(), vector3Variable.CGAccess("y"));
				}
				if(storeZ.isAssigned) {
					data += CG.Set(storeZ.CGValue(), vector3Variable.CGAccess("z"));
				}
			}
			return data;
		}
	}
}