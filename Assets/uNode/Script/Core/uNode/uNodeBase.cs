using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Base class for all node which implement graphs system that is used for generating pure c#.
	/// </summary>
	public abstract class uNodeBase : uNodeRoot, IClassSystem {
		#region Fields
		/// <summary>
		/// The prefab which contains uNodeData to be used to generate nested type.
		/// </summary>
		[Hide, SerializeField]
		protected uNodeData nestedTypes;
		/// <summary>
		/// The list of variable in this uNode.
		/// </summary>
		[HideInInspector, SerializeField]
		protected List<VariableData> variable = new List<VariableData>();
		/// <summary>
		/// List of method in this uNode.
		/// </summary>
		[HideInInspector, SerializeField]
		protected uNodeFunction[] functions = new uNodeFunction[0];
		/// <summary>
		/// List of property in this uNode.
		/// </summary>
		[HideInInspector, SerializeField]
		protected uNodeProperty[] properties = new uNodeProperty[0];
		/// <summary>
		/// List of constructor in this uNode.
		/// </summary>
		[HideInInspector, SerializeField]
		protected uNodeConstuctor[] constructors = new uNodeConstuctor[0];
		/// <summary>
		/// List of attribute in this uNode.
		/// </summary>
		[HideInInspector, SerializeField]
		protected AttributeData[] attributes = new AttributeData[0];
		/// <summary>
		/// The list of generic parameter in this uNode.
		/// </summary>
		[HideInInspector, SerializeField]
		protected GenericParameterData[] genericParameters = new GenericParameterData[0];
		#endregion

		#region Properties
		public override List<VariableData> Variables {
			get {
				return variable;
			}
		}

		public override IList<uNodeProperty> Properties {
			get {
				return properties;
			}
		}

		public override IList<uNodeFunction> Functions {
			get {
				return functions;
			}
		}

		public override IList<uNodeConstuctor> Constuctors {
			get {
				return constructors;
			}
		}

		public IList<AttributeData> Attributes {
			get {
				return attributes;
			}
			set {
				if(value is AttributeData[]) {
					attributes = value as AttributeData[];
				} else {
					attributes = value.ToArray();
				}
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

		public uNodeData NestedClass {
			get {
				return nestedTypes;
			}
			set {
				nestedTypes = value;
			}
		}

		public virtual bool IsStruct {
			get {
				return false;
			}
		}
		#endregion
		
		/// <summary>
		/// Get the generic parameter.
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
		}

		#region Editor
		public override void Refresh() {
			base.Refresh();
			if(RootObject == null)
				return;
			functions = GetFunctions();
			properties = GetProperties();
			constructors = GetConstuctors();
		}
		#endregion
	}
}