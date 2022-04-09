using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Editors {
    /// <summary>
    /// This is base class for convert a graph into other graph type
    /// </summary>
	public abstract class GraphConverter {
		public virtual int order => 0;
		public abstract bool IsValid(uNodeRoot graph);
		public abstract string GetMenuName(uNodeRoot graph);
		public abstract uNodeRoot Convert(uNodeRoot graph);

		protected void ValidateGraph(
			uNodeRoot graph,
			bool supportAttribute = true, 
			bool supportGeneric = true, 
			bool supportNativeType = true, 
			bool supportRuntimeType = true,
			bool supportModifier = true,
			bool supportConstructor = true) {
			if (!supportAttribute) {
				if (graph is IAttributeSystem AS && AS.Attributes?.Count > 0) {
					Debug.LogWarning("The target graph contains 'Attributes' which is not supported, the converted graph will not include it.");
					AS.Attributes = new AttributeData[0];
				}
			}
			if (!supportGeneric) {
				if (graph is IGenericParameterSystem GPS && GPS.GenericParameters?.Count > 0) {
					Debug.LogWarning("The target graph contains 'GenericParameter' which is not supported, the converted graph will not include it.");
					GPS.GenericParameters = new GenericParameterData[0];
				}
			}
			if(!supportModifier) {
				if(graph is IClassModifier clsModifier) {
					var modifier = clsModifier.GetModifier();
					if(modifier.Static || modifier.Abstract) {
						Debug.LogWarning("The target graph contains unsupported class modifier, the converted graph will ignore it.");
						modifier.Static = false;
						modifier.Abstract = false;
					}
				}
			}
			if (!supportModifier && graph is IVariableSystem variableSystem) {
				var variables = variableSystem.Variables;
				if(variables != null) {
					foreach(var v in variables) {
						if (!supportModifier) {
							if (v.modifier.Static || v.modifier.ReadOnly || v.modifier.Const) {
								Debug.LogWarning("The target graph contains unsupported variable modifier, the converted graph will ignore it.");
								v.modifier.Static = false;
								v.modifier.ReadOnly = false;
								v.modifier.Const = false;
							}
						}
					}
				}
			}
			if((!supportAttribute || !supportGeneric || !supportModifier) && graph is IFunctionSystem functionSystem) {
				var functions = functionSystem.Functions;
				if(functions != null) {
					foreach(var f in functions) {
						if(!supportAttribute) {
							if(f.Attributes?.Count > 0) {
								Debug.LogWarning("The target graph contains function 'Attributes' which is not supported, the converted graph will not include it.");
								f.Attributes = new AttributeData[0];
							}
						}
						if(!supportGeneric) {
							if(f.GenericParameters?.Count > 0) {
								Debug.LogWarning("The target graph contains function generic parameter which is not supported, the converted graph will not include it.");
								f.GenericParameters = new GenericParameterData[0];
							}
						}
						if (!supportModifier) {
							if (f.modifiers.Abstract || f.modifiers.Async || f.modifiers.Extern || f.modifiers.New || f.modifiers.Override || f.modifiers.Partial || f.modifiers.Static || f.modifiers.Unsafe || f.modifiers.Virtual) {
								Debug.LogWarning("The target graph contains unsupported function modifier, the converted graph will ignore it.");
								f.modifiers = new FunctionModifier() { 
									Public = f.modifiers.isPublic, 
									Private = !f.modifiers.isPublic 
								};
							}
						}
					}
				}
			}
			if((!supportAttribute || !supportModifier) && graph is IPropertySystem propertySystem) {
				var properties = propertySystem.Properties;
				if(properties != null) {
					foreach(var f in properties) {
						if(!supportAttribute) {
							if(f.Attributes?.Count > 0) {
								Debug.LogWarning("The target graph contains property 'Attributes' which is not supported, the converted graph will not include it.");
								f.Attributes = new AttributeData[0];
							}
						}
						if (!supportModifier) {
							if (f.modifier.Abstract || f.modifier.Static || f.modifier.Virtual) {
								Debug.LogWarning("The target graph contains unsupported property modifier, the converted graph will ignore it.");
								f.modifier = new PropertyModifier() { 
									Public = f.modifier.isPublic, 
									Private = !f.modifier.isPublic 
								};
							}
						}
					}
				}
			}
			if(!supportConstructor) {
				if(graph is IConstructorSystem CS && CS.Constuctors?.Count > 0) {
					Debug.LogWarning("The target graph contains constructor which is not supported, the converted graph will not include it.");
					for (int i = 0; i < CS.Constuctors.Count;i++) {
						if(CS.Constuctors[i] != null) {
							Object.DestroyImmediate(CS.Constuctors[i].gameObject);
						}
					}
				}
			}
		}
	}
    
	public class ClassGraphConverter : GraphConverter {
		public override uNodeRoot Convert(uNodeRoot graph) {
            var cls = Object.Instantiate(graph);
            var gameObject = new GameObject("Converted Graph");
            var result = gameObject.AddComponent<uNodeClass>();
            result.Name = cls.Name;
			result.Variables.Clear();
			foreach(var v in cls.Variables) {
                result.Variables.Add(new VariableData(v));
            }
			result.inheritFrom = new MemberData(cls.GetInheritType());
			result.RootObject = cls.RootObject;
			if (result.RootObject != null) {
				result.RootObject.transform.SetParent(gameObject.transform);
				AnalizerUtility.RetargetNodeOwner(cls, result, result.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
			}
			Object.DestroyImmediate(cls.gameObject);
			result.Refresh();
			return result;
		}

		public override string GetMenuName(uNodeRoot graph) {
            return "Convert to C# Class";
		}

		public override bool IsValid(uNodeRoot graph) {
			if(graph is uNodeClassComponent || graph is uNodeClassAsset || graph is uNodeRuntime) {
				return true;
			}
            return false;
		}
	}

    public class RuntimeGraphConverter : GraphConverter {
		public override uNodeRoot Convert(uNodeRoot graph) {
			var cls = Object.Instantiate(graph);
            var gameObject = new GameObject("Converted Graph");
            var result = gameObject.AddComponent<uNodeRuntime>();
            result.Name = cls.Name;
			result.Variables.Clear();
			foreach(var v in cls.Variables) {
                result.Variables.Add(new VariableData(v));
            }
			result.RootObject = cls.RootObject;
			if (result.RootObject != null) {
				result.RootObject.transform.SetParent(gameObject.transform);
				AnalizerUtility.RetargetNodeOwner(cls, result, result.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
			}
			Object.DestroyImmediate(cls.gameObject);
			result.Refresh();
			ValidateGraph(result, supportAttribute: false, supportGeneric: false, supportModifier: false, supportNativeType: false, supportConstructor:false);
			return result;
		}

		public override string GetMenuName(uNodeRoot graph) {
            return "Convert to uNode Runtime";
		}

		public override bool IsValid(uNodeRoot graph) {
			if(graph is uNodeClass cls && cls.GetInheritType() == typeof(MonoBehaviour)) {
				return true;
			} else if(graph is uNodeClassComponent) {
				return true;
			}
            return false;
		}
	}

	public class ClassComponentGraphConverter : GraphConverter {
		public override uNodeRoot Convert(uNodeRoot graph) {
            var cls = Object.Instantiate(graph);
            var gameObject = new GameObject("Converted Graph");
            var result = gameObject.AddComponent<uNodeClassComponent>();
            result.Name = cls.Name;
			result.Variables.Clear();
			foreach(var v in cls.Variables) {
                result.Variables.Add(new VariableData(v));
            }
            result.RootObject = cls.RootObject;
			if (result.RootObject != null) {
				result.RootObject.transform.SetParent(gameObject.transform);
				AnalizerUtility.RetargetNodeOwner(cls, result, result.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
			}
			Object.DestroyImmediate(cls.gameObject);
			result.Refresh();
			ValidateGraph(result, supportAttribute: false, supportGeneric: false, supportModifier: false, supportNativeType: false, supportConstructor:false);
			return result;
		}

		public override string GetMenuName(uNodeRoot graph) {
            return "Convert to Class Component";
		}

		public override bool IsValid(uNodeRoot graph) {
			if(graph is uNodeClass) {
                var root = graph as uNodeClass;
                if(root.GetInheritType() == typeof(MonoBehaviour)) {
					return true;
				}
			} else if(graph is uNodeRuntime) {
				return true;
			} else if(graph is uNodeComponentSingleton) {
				return true;
			}
			return false;
		}
	}

	public class SingletonGraphConverter : GraphConverter {
		public override uNodeRoot Convert(uNodeRoot graph) {
			var cls = Object.Instantiate(graph);
			var gameObject = new GameObject("Converted Graph");
			var result = gameObject.AddComponent<uNodeComponentSingleton>();
			result.Name = cls.Name;
			result.Variables.Clear();
			foreach(var v in cls.Variables) {
				result.Variables.Add(new VariableData(v));
			}
			result.RootObject = cls.RootObject;
			if(result.RootObject != null) {
				result.RootObject.transform.SetParent(gameObject.transform);
				AnalizerUtility.RetargetNodeOwner(cls, result, result.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
			}
			Object.DestroyImmediate(cls.gameObject);
			result.Refresh();
			ValidateGraph(result, supportAttribute: false, supportGeneric: false, supportModifier: false, supportNativeType: false, supportConstructor: false);
			return result;
		}

		public override string GetMenuName(uNodeRoot graph) {
			return "Convert to Component Singleton";
		}

		public override bool IsValid(uNodeRoot graph) {
			if(graph is uNodeClass) {
				var root = graph as uNodeClass;
				if(root.GetInheritType() == typeof(MonoBehaviour)) {
					return true;
				}
			} else if(graph is uNodeRuntime) {
				return true;
			} else if(graph is uNodeClassComponent) {
				return true;
			}
			return false;
		}
	}

	public class ClassAssetGraphConverter : GraphConverter {
		public override uNodeRoot Convert(uNodeRoot graph) {
            var cls = Object.Instantiate(graph);
            var gameObject = new GameObject("Converted Graph");
            var result = gameObject.AddComponent<uNodeClassAsset>();
            result.Name = cls.Name;
			result.Variables.Clear();
			foreach(var v in cls.Variables) {
                result.Variables.Add(new VariableData(v));
            }
            result.RootObject = cls.RootObject;
			if (result.RootObject != null) {
				result.RootObject.transform.SetParent(gameObject.transform);
				AnalizerUtility.RetargetNodeOwner(cls, result, result.RootObject.GetComponentsInChildren<MonoBehaviour>(true));
			}
			Object.DestroyImmediate(cls.gameObject);
			result.Refresh();
			ValidateGraph(result, supportAttribute: false, supportGeneric: false, supportModifier: false, supportNativeType: false, supportConstructor:false);
            return result;
		}

		public override string GetMenuName(uNodeRoot graph) {
            return "Convert to Class Asset";
		}

		public override bool IsValid(uNodeRoot graph) {
			if(graph is uNodeClass) {
                var root = graph as uNodeClass;
                if(root.GetInheritType() == typeof(ScriptableObject)) {
					return true;
				}
			}
            return false;
		}
	}
}