using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine/Transform", "SmoothMove")]
	public class SmoothMove : Action {
		[ObjectType(typeof(Transform))]
		public MemberData transform = MemberData.empty;
		[ObjectType(typeof(Vector3))]
		public MemberData destination = MemberData.empty;
		[ObjectType(typeof(float))]
		public MemberData speed = new MemberData(0.1f);

		protected override void OnExecute() {
			if(transform != null) {
				Transform tr = transform.Get<Transform>();
				tr.position = Vector3.Lerp(tr.position, destination.Get<Vector3>(), speed.Get<float>() * Time.deltaTime);
			}
		}

		public override string GenerateCode(Object obj) {
			if(transform.isAssigned && destination.isAssigned) {
				uNode.VariableData variable = CG.GetOrRegisterUserObject(new uNode.VariableData("tr", transform.type), this);
				string left = CG.GetVariableName(variable) + ".position";
				string right = CG.Invoke(typeof(Vector3), "Lerp", left, CG.Value(destination), 
					CG.Type(typeof(Time))+ ".deltaTime * " + CG.Value(speed));
				return CG.DeclareVariable(variable, transform).AddLineInEnd() +  CG.Set(left, right);
			}
			throw new System.Exception("transform or destination is unassigned");
		}
	}
}