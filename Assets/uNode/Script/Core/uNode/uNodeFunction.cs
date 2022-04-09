using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MaxyGames.uNode {
	/// <summary>
	/// A class for handling function for uNode.
	/// </summary>
	[AddComponentMenu("")]
	public class uNodeFunction : RootObject, IAttributeSystem, IGenericParameterSystem, IFunction, ISummary {
		/// <summary>
		/// The summary of this Function.
		/// </summary>
		[Tooltip("The summary of this Function.")]
		[TextArea]
		public string summary;
		/// <summary>
		/// The modifier of this function.
		/// </summary>
		[Tooltip("The modifier of this function.")]
		public FunctionModifier modifiers = new FunctionModifier();
		/// <summary>
		/// The return type of this function.
		/// </summary>
		[Tooltip("The return type of this function.")]
		[Filter(OnlyGetType = true, UnityReference = false, VoidType = true), HideInInspector]
		public MemberData returnType = MemberData.CreateFromType(typeof(void));

		/// <summary>
		/// The list of attribute on this function.
		/// </summary>
		[HideInInspector]
		public AttributeData[] attributes = new AttributeData[0];
		/// <summary>
		/// The list of generic parameter on this function.
		/// </summary>
		[HideInInspector]
		public GenericParameterData[] genericParameters = new GenericParameterData[0];

		public IList<AttributeData> Attributes {
			get {
				return attributes;
			}
			set {
				if(value is AttributeData[]) {
					attributes = value as AttributeData[];
					return;
				}
				attributes = value.ToArray();
			}
		}

		public IList<GenericParameterData> GenericParameters {
			get {
				return genericParameters;
			}
			set {
				if(value is GenericParameterData[]) {
					genericParameters = value as GenericParameterData[];
					return;
				}
				genericParameters = value.ToArray();
			}
		}

		public override bool CanHaveCoroutine() {
			if(returnType != null && returnType.isAssigned) {
				System.Type rType = returnType.Get() as System.Type;
				return rType == typeof(IEnumerable) || rType == typeof(IEnumerator) || rType.IsSubclassOfRawGeneric(typeof(IEnumerator<>)) || rType.IsSubclassOfRawGeneric(typeof(IEnumerable<>));
			}
			return false;
		}

		/// <summary>
		/// Invoke this function.
		/// </summary>
		/// <returns></returns>
		public object Invoke() {
			return Invoke(null, null);
		}

		/// <summary>
		/// Invoke this function.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public object Invoke(object[] parameter) {
			return Invoke(parameter, null);
		}

		/// <summary>
		/// Invoke this function.
		/// </summary>
		/// <param name="parameter"></param>
		/// <param name="genericType"></param>
		/// <returns></returns>
		public object Invoke(object[] parameter, System.Type[] genericType) {
			if(startNode == null) {
				if(returnType.Get<System.Type>() != typeof(void)) {
					throw new System.Exception("Unassigned start node in function:" + this.Name);
				}
				return null;
			}
			if(parameter != null) {
				for(int i = 0; i < parameter.Length; i++) {
					parameters[i].value = parameter[i];
				}
				//Debug.Log($"Invoking function: {Name} with parameters:{string.Join(", ", parameter)}");
			}
			if(genericParameters.Length != 0 && genericType != null &&
				genericParameters.Length != genericType.Length)
				throw new System.Exception("Invalid Generic Length");
			if(genericType != null) {
				for(int i = 0; i < genericType.Length; i++) {
					genericParameters[i].value = genericType[i];
				}
			}
			InitLocalVariable();//Init local variable to initial value
			System.Type rType = null;
			if(returnType != null && returnType.isAssigned) {
				rType = returnType.Get() as System.Type;
				if(rType != null && rType != typeof(void) && (rType.IsCastableTo(typeof(IEnumerable)) || rType.IsCastableTo(typeof(IEnumerator)))) {
					return WaitUntilFinish(startNode);
				}
			}
			startNode.Activate();
			if(rType != null && rType != typeof(void)) {
				JumpStatement js = startNode.GetJumpState();
				if(js == null || js.jumpType != JumpStatementType.Return) {
					throw new System.Exception("No return value in function:" + Name);
				} else if(js.from is NodeReturn) {
					return (js.from as NodeReturn).GetReturnValue();
				}
			}
			return null;
		}

		private IEnumerator WaitUntilFinish(Node node) {
			node.Activate();
			yield return node.WaitUntilFinish();
		}

		/// <summary>
		/// Get the generic parameter by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public GenericParameterData GetGenericParameter(string name) {
			for(int i = 0; i < genericParameters.Length; i++) {
				if(genericParameters[i].name == name) {
					return genericParameters[i];
				}
			}
			return null;
			//throw new System.Exception(name + " GenericParameter not found");
		}

		/// <summary>
		/// Get the return type of this function.
		/// </summary>
		/// <returns></returns>
		public override System.Type ReturnType() {
			if(returnType != null && returnType.isAssigned) {
				return returnType.Get() as System.Type;
			}
			return typeof(void);
		}

		public string GetSummary() {
			return summary;
		}
	}
}