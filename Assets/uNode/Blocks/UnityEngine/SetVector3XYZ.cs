using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "Vector3.SetVector3XYZ")]
	public class SetVector3XYZ : Action {
		[Filter(typeof(Vector3))]
		public MemberData vector3Variable;
		[ObjectType(typeof(float))]
		public MemberData setX;
		[ObjectType(typeof(float))]
		public MemberData setY;
		[ObjectType(typeof(float))]
		public MemberData setZ;

		protected override void OnExecute() {
			if(vector3Variable.isAssigned) {
				Vector3 result = (Vector3)vector3Variable.Get();
				if(setX.isAssigned) {
					result.x = setX.Get<float>();
				}
				if(setY.isAssigned) {
					result.y = setY.Get<float>();
				}
				if(setZ.isAssigned) {
					result.z = setZ.Get<float>();
				}
				vector3Variable.Set(result);
			}
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			if(vector3Variable.isAssigned) {
				string name = CG.Value((object)vector3Variable);
				if(setX.isAssigned) {
					string code = "new " + CG.Type(typeof(Vector3)) + "(" + CG.Value((object)setX) + ", " + name + ".y, " + name + ".z)";
					data += CG.Set(name, code);
				}
				if(setY.isAssigned) {
					string code = "new " + CG.Type(typeof(Vector3)) + "(" + name + ".x, " + CG.Value((object)setY) + ", " + name + ".z)";
					data += CG.Set(name, code);
				}
				if(setZ.isAssigned) {
					string code = "new " + CG.Type(typeof(Vector3)) + "(" + name + ".x, " + name + ".y, " + CG.Value((object)setZ) + ")";
					data += CG.Set(name, code);
				}
			}
			return data;
		}
	}
}