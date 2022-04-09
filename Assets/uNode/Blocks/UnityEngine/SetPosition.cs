using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine/Transform", "SetPosition")]
	public class SetPosition : Action {
		[ObjectType(typeof(Transform))]
		public MemberData transform = MemberData.empty;
		public bool SetX = true;
		[ObjectType(typeof(float))]
		public MemberData XValue = new MemberData(0f);
		public bool SetY = true;
		[ObjectType(typeof(float))]
		public MemberData YValue = new MemberData(0f);
		public bool SetZ = true;
		[ObjectType(typeof(float))]
		public MemberData ZValue = new MemberData(0f);

		protected override void OnExecute() {
			if(transform != null) {
				Transform tr = transform.Get<Transform>();
				var vector = tr.position;
				if(SetX) {
					vector.x = XValue.Get<float>();
				}
				if(SetY) {
					vector.y = YValue.Get<float>();
				}
				if(SetZ) {
					vector.z = ZValue.Get<float>();
				}
			}
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			if(transform.isAssigned) {
				string name = CG.Value((object)transform) + ".position";
				string code = "new " + CG.Type(typeof(Vector3)) + "(";
				if(SetX) {
					if(SetY) {
						if(SetZ) {
							code += CG.Value((object)XValue) + ", " +
								CG.Value((object)YValue) + ", " +
								CG.Value((object)ZValue) + ")";
						} else {
							code += CG.Value((object)XValue) + ", " + CG.Value((object)YValue) + ", " + name + ".z)";
						}
					} else if(SetZ) {
						code += CG.Value((object)XValue) + ", " + name + ".y, " + CG.Value((object)ZValue) + ")";
					} else {
						code += CG.Value((object)XValue) + ", " + name + ".y, " + name + ".z)";
					}
				} else if(SetY) {
					code += name + ".x, " + CG.Value((object)YValue) + ", " + name + ".z)";
				} else if(SetZ) {
					code += name + ".x, " + name + ".y, " + CG.Value((object)ZValue) + ")";
				}
				data += CG.Set(name, code);
			}
			return data;
		}
	}
}