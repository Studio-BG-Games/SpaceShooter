using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Class for generating C# properties
	/// </summary>
	[AddComponentMenu("")]
	public class uNodeProperty : uNodeComponent, IProperty, IPropertyModifier, IAttributeSystem, ISummary {
		/// <summary>
		/// The name of this root.
		/// </summary>
		[Hide]
		public string Name;
		[Filter(OnlyGetType = true, UnityReference = false)]
		public MemberData type = new MemberData(typeof(object), MemberData.TargetType.Type);

		[HideInInspector]
		public AttributeData[] attributes;
		public PropertyModifier modifier = new PropertyModifier();
		[Hide]
		public FunctionModifier getterModifier, setterModifier;
		/// <summary>
		/// The summary of this Property.
		/// </summary>
		[TextArea]
		public string summary;

		[Hide]
		public uNodeFunction setRoot, getRoot;
		[Hide]
		public uNodeRoot owner;

		private object autoPropertyValue;
		[System.NonSerialized]
		private bool isInitialize;

		public bool AutoProperty {
			get {
				return !getRoot && !setRoot;
			}
		}

		public IList<AttributeData> Attributes { get => attributes; set => attributes = uNodeUtility.CreateArrayFrom(value); }

		public bool CanGetValue() {
			return AutoProperty || getRoot;
		}

		public bool CanSetValue() {
			return AutoProperty || setRoot;
		}

		public object Get() {
			if(!AutoProperty) {
				if(getRoot) {
					return getRoot.Invoke();
				} else {
					throw new System.Exception("Can't get value of Property because no Getter.");
				}
			} else if(!isInitialize && autoPropertyValue == null) {
				if(ReturnType().IsValueType) {
					autoPropertyValue = System.Activator.CreateInstance(ReturnType());
				}
				isInitialize = true;
			}
			return autoPropertyValue;
		}

		public void Set(object value) {
			if(AutoProperty) {
				autoPropertyValue = value;
			} else {
				if(setRoot) {
					setRoot.Invoke(new object[] { value });
				} else {
					throw new System.Exception("Can't set value of Property because no Setter.");
				}
			}
		}

		public System.Type ReturnType() {
			if(type != null && type.isAssigned) {
				System.Type t = type.Get<System.Type>();
				if(t != null) return t;
			}
			return typeof(object);
		}

		public PropertyModifier GetModifier() {
			return modifier;
		}

		public string GetSummary() {
			return summary;
		}
	}
}