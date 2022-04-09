using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine/Transform", "SmoothLookAt")]
	public class SmoothLookAt : Action {
		[ObjectType(typeof(Transform))]
		public MemberData transform = MemberData.empty;
		[ObjectType(typeof(Transform))]
		public MemberData target = MemberData.empty;
		[ObjectType(typeof(float))]
		public MemberData damping = new MemberData(6f);

		protected override void OnExecute() {
			if(transform != null) {
				Transform tr = transform.Get<Transform>();
				tr.rotation = Quaternion.Slerp(tr.rotation, 
					Quaternion.LookRotation(target.Get<Transform>().position - tr.position), 
					Time.deltaTime * damping.Get<float>());
			}
		}

		public override string GenerateCode(Object obj) {
			if(transform.isAssigned && target.isAssigned) {
				uNode.VariableData variable = CG.GetOrRegisterUserObject(new uNode.VariableData("tr", transform.type), this);
				string lookRotation = CG.FlowInvoke(typeof(Quaternion), "LookRotation",
					CG.Value((object)target) + ".position - " +
					CG.GetVariableName(variable) + ".position");
				string left = CG.GetVariableName(variable) + ".rotation";
				string right = CG.Invoke(
					typeof(Quaternion), 
					"Slerp", 
					left, 
					lookRotation,  
					CG.Type(
						typeof(Time)) + ".deltaTime * " + 
						CG.Value(damping));
				return CG.DeclareVariable(variable, transform).AddLineInEnd() +  CG.Set(left, right);
			}
			throw new System.Exception("transform or target is unassigned");
		}
	}
}