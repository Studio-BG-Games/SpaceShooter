using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Class for generating C# interfaces
	/// </summary>
	[System.Serializable]
	public class InterfaceData {
		/// <summary>
		/// The name of interface.
		/// </summary>
		public string name;
		/// <summary>
		/// The summary of interface.
		/// </summary>
		[TextArea]
		public string summary;
		public InterfaceModifier modifiers = new InterfaceModifier();
		//[HideInInspector]
		//public AttributeData[] attributes;
		//[HideInInspector]
		//public GenericParameterData[] genericParameters;
		public InterfaceFunction[] functions = new InterfaceFunction[0];
		public InterfaceProperty[] properties = new InterfaceProperty[0];
		//public InterfaceIndexer[] indexers = new InterfaceIndexer[0];
	}

	[System.Serializable]
	public class InterfaceModifier : AccessModifier {

	}

	[System.Serializable]
	public class InterfaceFunction : ISummary {
		/// <summary>
		/// The name of this method.
		/// </summary>
		public string name;
		/// <summary>
		/// The summary of this Function.
		/// </summary>
		[TextArea]
		public string summary;
		/// <summary>
		/// The return type of this method.
		/// </summary>
		[Filter(OnlyGetType =true)]
		public MemberData returnType = new MemberData(typeof(void), MemberData.TargetType.Type);

		[HideInInspector]
		public ParameterData[] parameters = new ParameterData[0];
		[HideInInspector]
		public GenericParameterData[] genericParameters = new GenericParameterData[0];

		public string GetSummary() {
			return summary;
		}

		/// <summary>
		/// The return type of an interface
		/// </summary>
		/// <returns></returns>
		public System.Type ReturnType() {
			if(returnType.isAssigned) {
				return returnType.startType;
			}
			return typeof(void);
		}
	}

	[System.Serializable]
	public class InterfaceProperty : ISummary {
		/// <summary>
		/// The name of this property.
		/// </summary>
		public string name;
		/// <summary>
		/// The summary of this property.
		/// </summary>
		[TextArea]
		public string summary;
		/// <summary>
		/// The return type of this property.
		/// </summary>
		[Filter(OnlyGetType = true)]
		public MemberData returnType = new MemberData(typeof(int), MemberData.TargetType.Type);
		public PropertyAccessorKind accessor;

		public string GetSummary() {
			return summary;
		}

		/// <summary>
		/// The return type of an interface
		/// </summary>
		/// <returns></returns>
		public System.Type ReturnType() {
			if(returnType.isAssigned) {
				return returnType.startType;
			}
			return typeof(void);
		}

		public bool CanGetValue() {
			return accessor == PropertyAccessorKind.ReadOnly || accessor == PropertyAccessorKind.ReadWrite;
		}

		public bool CanSetValue() {
			return accessor == PropertyAccessorKind.WriteOnly || accessor == PropertyAccessorKind.ReadWrite;
		}
	}

	//[System.Serializable]
	//public class InterfaceIndexer {
	//	public PropertyAccessor accessor;
	//}

	//public interface ITerst {
	//	string name {
	//		get;
	//	}
	//	string this[int index] {
	//		get;
	//	}

	//	void Execute();

	//	void Execute<A, B>();
	//}
}