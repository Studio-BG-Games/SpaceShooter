using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class ExposedNode : CustomNode, IExtendedOutput {
		[Serializable]
		public class OutputData {
			public string name;
			[SerializeField]
			private SerializedType _type;

			public Type type {
				get {
					return _type?.type;
				}
				set {
					_type = new SerializedType(value);
				}
			}
		}
		[HideInInspector]
		public OutputData[] outputDatas = new OutputData[0];
		[Hide, ValueIn(""), Tooltip("The value to expose.")]
		public MemberData value;

		public override string GetNodeName() => "Exposed";

		public int OutputCount => outputDatas.Length;

		public string GenerateOutputCode(string name) {
			foreach(var d in outputDatas) {
				if(d.name == name) {
					return CG.Value((object)value).CGAccess(name);
				}
			}
			throw new Exception("No matching port: " + name);
		}

		public string GetOutputName(int index) {
			if(outputDatas.Length > index) {
				return outputDatas[index].name;
			}
			return "Missing";
		}

		public Type GetOutputType(string name) {
			foreach(var d in outputDatas) {
				if(d.name == name) {
					return d.type;
				}
			}
			throw new Exception("No matching port: " + name);
		}

		public object GetOutputValue(string name) {
			foreach(var d in outputDatas) {
				if(d.name == name) {
					var val = value.Get();
					var member = val.GetType().GetMemberCached(name);
					if(member is System.Reflection.FieldInfo field) {
						return field.GetValueOptimized(val);
					} else if(member is System.Reflection.PropertyInfo prop) {
						return prop.GetValueOptimized(val);
					}
				}
			}
			throw new Exception("No matching port: " + name);
		}

		#region Editors
		public void Refresh(bool addFields = false) {
			var type = value.type;
			var fields = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
			for(int x = 0; x < outputDatas.Length; x++) {
				bool valid = false;
				for(int y = 0; y < fields.Length; y++) {
					if(fields[y].MemberType != MemberTypes.Property && fields[y].MemberType != MemberTypes.Field)
						continue;
					if(outputDatas[x].name == fields[y].Name) {
						valid = true;
						break;
					}
				}
				if(!valid) {
					uNodeUtility.RemoveArrayAt(ref outputDatas, x);
					x--;
				}
			}
			for(int x = 0; x < fields.Length; x++) {
				var m = fields[x];
				if(m is FieldInfo field) {
					if(field.Attributes.HasFlags(FieldAttributes.InitOnly))
						continue;
				} else if(m is PropertyInfo property) {
					if(!property.CanRead || property.GetIndexParameters().Length > 0) {
						continue;
					}
				} else {
					continue;
				}
				var t = ReflectionUtils.GetMemberType(m);
				bool found = false;
				for(int y = 0; y < outputDatas.Length; y++) {
					if(m.Name == outputDatas[y].name) {
						if(t != outputDatas[y].type) {
							outputDatas[y] = new OutputData() { name = m.Name, type = t };
						}
						found = true;
						break;
					}
				}
				if(!found && addFields) {
					uNodeUtility.AddArray(ref outputDatas, new OutputData() { name = m.Name, type = t });
				}
			}
		}
		#endregion
	}
}