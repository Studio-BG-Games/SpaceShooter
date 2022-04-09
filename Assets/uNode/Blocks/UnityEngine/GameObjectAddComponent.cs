using UnityEngine;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "GameObject.AddComponent")]
	public class GameObjectAddComponent : Action {
		[ObjectType(typeof(GameObject))]
		public MemberData gameObject;
		[Filter(typeof(Component), OnlyGetType = true)]
		public MemberData componentType;
		[Filter(SetMember=true)]
		[ObjectType("componentType")]
		public MemberData storeComponent;

		protected override void OnExecute() {
			if(gameObject.isAssigned && componentType.isAssigned) {
				if(storeComponent.isAssigned) {
					storeComponent.Set(gameObject.Get<GameObject>().AddComponent(componentType.Get() as System.Type));
					return;
				}
				gameObject.Get<GameObject>().AddComponent(componentType.Get() as System.Type);
			}
		}

		public override string GenerateCode(Object obj) {
			if(gameObject.isAssigned && componentType.isAssigned) {
				string data = CG.Invoke(gameObject, "AddComponent", componentType.CGValue());
				if(string.IsNullOrEmpty(data)) return null;
				if(storeComponent.isAssigned) {
					return CG.Value((object)storeComponent) + " = " +
						data + " as " +
						CG.Type(componentType.type).AddSemicolon();
				}
				return data;
			}
			return null;
		}

		public override string GetDescription() {
			return "Adds a component class of type componentType to the game object.";
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(gameObject, owner, Name + " - gameObject");
			uNode.uNodeUtility.CheckError(componentType, owner, Name + " - componentType");
		}
	}
}