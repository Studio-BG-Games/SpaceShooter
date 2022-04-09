using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine/Transform", "SetLocalRotation")]
	public class SetLocalRotation : Action {
		[ObjectType(typeof(Transform))]
		public MemberData transform;
		public bool SetX = true;
		[ObjectType(typeof(float)), Hide(nameof(SetX), false)]
		public MemberData XValue;
		public bool SetY = true;
		[ObjectType(typeof(float)), Hide(nameof(SetY), false)]
		public MemberData YValue;
		public bool SetZ = true;
		[ObjectType(typeof(float)), Hide(nameof(SetZ), false)]
		public MemberData ZValue;

		protected override void OnExecute() {
			if(transform != null) {
				Transform tr = transform.Get<Transform>();
				var vector = tr.localEulerAngles;
				if(SetX) {
					vector.x = XValue.Get<float>();
				}
				if(SetY) {
					vector.y = YValue.Get<float>();
				}
				if(SetZ) {
					vector.z = ZValue.Get<float>();
				}
				tr.localEulerAngles = vector;
			}
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			if(transform.isAssigned) {
				string name = CG.Value((object)transform) + ".localEulerAngles";
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
					} else if(SetZ){
						code += CG.Value((object)XValue) + ", " + name + ".y, " + CG.Value((object)YValue) + ", " + name + ".z)";
					} else {
						code += CG.Value((object)XValue) + ", " + name + ".y, " + name + ".z)";
					}
				} else if(SetY) {
					code += name + ".x, " + CG.Value((object)YValue) + ", " + name + ".z)";
				} else if(SetZ) {
					code += name + ".x, " + name + ".y, " + CG.Value((object)ZValue) + ")";
				} else {
					return null;
				}
				data += CG.Set(name, code);
			}
			return data;
		}
	}
}