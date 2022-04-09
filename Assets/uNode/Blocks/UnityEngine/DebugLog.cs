using UnityEngine;

namespace MaxyGames.Events {
	//[EventMenu("UnityEngine/Debug/DebugLog", "DebugLog")]
	public class DebugLog : Action {
		public enum MassageType {
			Info,
			Warning,
			Error
		}
		[Tooltip("The type of massage")]
		public MassageType massageType = MassageType.Info;
		public MemberData message = new MemberData("");

		protected override void OnExecute() {
			object obj = message.Get();
			switch(massageType) {
				case MassageType.Info:
					Debug.Log(obj);
					break;
				case MassageType.Warning:
					Debug.LogWarning(obj);
					break;
				case MassageType.Error:
					Debug.LogError(obj);
					break;
			}
		}

		protected override bool OnValidate() {
			OnExecute();
			return true;
		}

		public override string GenerateCode(Object obj) {
			string DebugName = CG.Type(typeof(Debug));
			switch(massageType) {
				case MassageType.Info:
					return DebugName + ".Log(" + CG.Value((object)message) + ");";
				case MassageType.Warning:
					return DebugName + ".LogWarning(" + CG.Value((object)message) + ");";
				case MassageType.Error:
					return DebugName + ".LogError(" + CG.Value((object)message) + ");";
			}
			return null;
		}

		public override string Name {
			get {
				switch(massageType) {
					case MassageType.Info:
						return "Debug Log <i>" + uNode.uNodeUtility.GetDisplayName(message) + "</i>";
					case MassageType.Warning:
						return "Debug LogWarning <i>" + uNode.uNodeUtility.GetDisplayName(message) + "</i>";
					case MassageType.Error:
						return "Debug LogError <i>" + uNode.uNodeUtility.GetDisplayName(message) + "</i>";
				}
				return base.Name;
			}
		}

		public override string GetDescription() {
			return "Logs message to the Unity Console";
		}

		public override void CheckError(Object owner) {
			base.CheckError(owner);
			uNode.uNodeUtility.CheckError(message, owner, Name + " - message");
		}
	}
}