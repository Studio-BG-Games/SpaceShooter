using MaxyGames.uNode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MaxyGames {
	/// <summary>
	/// Class for Generating C# code from uNode with useful function for generating C# code more easier.
	/// </summary>
	public static partial class CG {
		#region Setup
		private static bool IsCanBeGrouped(NodeComponent node) {
			if(Nodes.IsStackOverflow(node))
				return false;
			return !Nodes.HasStateFlowOutput(node);
		}

		/// <summary>
		/// Reset the generator settings
		/// </summary>
		public static void ResetGenerator() {
			generatorData = new GData();
			graph = null;
			coNum = 0;
			InitData = new List<string>();
			actionID = 0;
			actionDataID = new Dictionary<Block, int>();
		}

		/// <summary>
		/// Generate new c#
		/// </summary>
		/// <param name="setting"></param>
		/// <returns></returns>
		public static GeneratedData Generate(GeneratorSetting setting) {
			//Wait other generation till finish before generate new script.
			if(isGenerating) {
				if(setting.isAsync) {
					//Wait until other generator is finished
					while(isGenerating) {
						uNodeThreadUtility.WaitOneFrame();
					}
				} else {
					//Update thread queue so the other generation will finish
					while(isGenerating) {
						uNodeThreadUtility.Update();
					}
				}
			}
			try {
				//Set max queue for async generation
				ThreadingUtil.SetMaxQueue(setting.maxQueue);
				//Mark is generating to be true so only one can generate the script at the same times.
				isGenerating = true;
				{//Change the global culture information, so that the parsing numeric values always uses dot.
					System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
					customCulture.NumberFormat.NumberDecimalSeparator = ".";
					System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
					if(setting.isAsync) {
						uNodeThreadUtility.Queue(() => {//This is for change culture in main thread.
							System.Globalization.CultureInfo customCulture2 = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
							customCulture2.NumberFormat.NumberDecimalSeparator = ".";
							System.Threading.Thread.CurrentThread.CurrentCulture = customCulture2;
						});
					}
				}
				//This is the current progress
				float progress = 0;
				float classCount = setting.graphs.Count != 0 ? setting.graphs.Count / setting.graphs.Count : 0;
				StringBuilder classBuilder = new StringBuilder();
				GeneratedData generatedData = new GeneratedData(classBuilder, setting);
				foreach(var classes in setting.graphs) {
					ResetGenerator();
					generatorData.setting = setting;
					if(classes == null)
						continue;
					graph = classes;
					graphSystem = ReflectionUtils.GetAttributeFrom<GraphSystemAttribute>(classes);
					classes.graphData.unityObjects.Clear();
					generatorData.state.state = State.Classes;
					//class name.
					string className = classes.Name;
					ThreadingUtil.Do(() => {
						className = classes.GraphName;
						if(string.IsNullOrEmpty(classes.Name) && className == "_" + Mathf.Abs(classes.GetInstanceID())) {
							className = classes.gameObject.name;
						}
						className = uNodeUtility.AutoCorrectName(className);
						generatorData.typeName = className;
						generatedData.classNames[uNodeUtility.GetActualObject(classes)] = className;
						//update progress bar
						setting.updateProgress?.Invoke(progress, "initializing class:" + classes.GraphName);
						//Initialize code gen for classes
						Initialize();
						if(graphSystem.supportVariable) {
							foreach(VariableData var in classes.Variables) {
								List<AData> attribute = new List<AData>();
								if(var.attributes != null && var.attributes.Length > 0) {
									foreach(var a in var.attributes) {
										attribute.Add(TryParseAttributeData(a));
									}
								}
								generatorData.AddVariable(new VData(var, attribute) { modifier = var.modifier });
							}
						}
						if(graphSystem.supportProperty) {
							generatorData.state.state = State.Property;
							foreach(var var in classes.Properties) {
								List<AData> attribute = new List<AData>();
								if(var.attributes != null && var.attributes.Length > 0) {
									foreach(var a in var.attributes) {
										attribute.Add(TryParseAttributeData(a));
									}
								}
								generatorData.properties.Add(new PData(var, attribute) { modifier = var.modifier });
							}
						}
						if(graphSystem.supportConstructor) {
							generatorData.state.state = State.Constructor;
							foreach(var var in classes.Constuctors) {
								generatorData.constructors.Add(new CData(var) { modifier = var.modifier });
							}
						}
					});
					int nodeCount = generatorData.allNode.Count != 0 ? generatorData.allNode.Count / generatorData.allNode.Count : 1;
					int fieldCount = classes.Variables.Count != 0 ? classes.Variables.Count / classes.Variables.Count : 1;
					int propCount = classes.Properties.Count != 0 ? classes.Properties.Count / classes.Properties.Count : 1;
					int ctorCount = classes.Constuctors.Count != 0 ? classes.Constuctors.Count / classes.Constuctors.Count : 1;
					float childFill = ((nodeCount + fieldCount + propCount + ctorCount) / 4F / (classCount)) / 4;
					//Generate functions
					GenerateFunctions((prog, text) => {
						float p = progress + (prog * (childFill));
						if(setting.updateProgress != null)
							setting.updateProgress(p, text);
					});
					progress += childFill;
					//Generate variables
					string variables = GenerateVariables((prog, text) => {
						float p = progress + (prog * (childFill));
						if(setting.updateProgress != null)
							setting.updateProgress(p, text);
					});
					progress += childFill;
					//Generate properties
					string properties = GenerateProperties((prog, text) => {
						float p = progress + (prog * (childFill));
						if(setting.updateProgress != null)
							setting.updateProgress(p, text);
					});
					progress += childFill;
					//Generate constructors
					string constructors = GenerateConstructors((prog, text) => {
						float p = progress + (prog * (childFill));
						if(setting.updateProgress != null)
							setting.updateProgress(p, text);
					});
					progress += childFill;
					generatorData.state.state = State.Classes;
					string genericParameters = null;
					string whereClause = null;
					if(classes is IGenericParameterSystem && graphSystem.supportGeneric) {//Implementing generic parameters
						var gData = (classes as IGenericParameterSystem).GenericParameters.Select(i => new GPData(i.name, i.typeConstraint.Get<Type>())).ToList();
						if(gData != null && gData.Count > 0) {
							genericParameters += "<";
							for(int i = 0; i < gData.Count; i++) {
								if(i != 0)
									genericParameters += ", ";
								genericParameters += gData[i].name;
							}
							genericParameters += ">";
						}
						if(gData != null && gData.Count > 0) {
							for(int i = 0; i < gData.Count; i++) {
								if(!string.IsNullOrEmpty(gData[i].type) &&
									!"object".Equals(gData[i].type) &&
									!"System.Object".Equals(gData[i].type)) {
									whereClause += " where " + gData[i].name + " : " +
										ParseType(gData[i].type);
								}
							}
						}
					}
					string interfaceName = null;
					if(classes is IInterfaceSystem) {//Implementing interfaces
						List<Type> interfaces = (classes as IInterfaceSystem).Interfaces.Where(item => item != null && item.Get<Type>() != null).Select(item => item.Get<Type>()).ToList();
						if(interfaces != null && interfaces.Count > 0) {
							for(int i = 0; i < interfaces.Count; i++) {
								if(interfaces[i] == null)
									continue;
								if(!string.IsNullOrEmpty(interfaceName)) {
									interfaceName += ", ";
								}
								interfaceName += Type(interfaces[i]);
							}
						}
					}
					if(!string.IsNullOrEmpty(classBuilder.ToString())) {
						classBuilder.AppendLine();
						classBuilder.AppendLine();
					}
					if(!string.IsNullOrEmpty(classes.summary)) {
						classBuilder.AppendLine("/// <summary>".AddLineInEnd() +
							"/// " + classes.summary.Replace("\n", "\n" + "/// ").AddLineInEnd() +
							"/// </summary>");
					}
					if(classes is IAttributeSystem) {
						foreach(var attribute in (classes as IAttributeSystem).Attributes) {
							if(attribute == null)
								continue;
							AData aData = TryParseAttributeData(attribute);
							if(aData != null) {
								string a = TryParseAttribute(aData);
								if(!string.IsNullOrEmpty(a)) {
									classBuilder.Append(a.AddLineInEnd());
								}
							}
						}
					}
					string classModifier = "public ";
					if(classes is IClassModifier) {
						classModifier = (classes as IClassModifier).GetModifier().GenerateCode();
					}
					Type InheritedType;
					if(graphSystem.inherithFrom != null) {
						InheritedType = graphSystem.inherithFrom;
					} else {
						InheritedType = classes.GetInheritType();
					}
					string classKeyword = "class ";
					if(classes is IClass && (classes as IClass).IsStruct) {
						classKeyword = "struct ";
						InheritedType = null;
					}
					{//Type Codegen
						classBuilder.Append(classModifier + classKeyword + className + genericParameters);
						if(InheritedType != null) {
							classBuilder.Append(" : ");
							classBuilder.Append(Type(InheritedType));
						}
						if(interfaceName != null) {
							if(InheritedType == null) {
								classBuilder.Append(" : ");
							} else {
								classBuilder.Append(", ");
							}
							classBuilder.Append(interfaceName);
						}
						if(whereClause != null) {
							classBuilder.Append(" : ");
							classBuilder.Append(whereClause);
						}
						classBuilder.Append(" {");
					}
					string classData = variables.AddLineInFirst() + properties.AddFirst("\n\n", !string.IsNullOrEmpty(variables)) + constructors.AddFirst("\n\n",
						!string.IsNullOrEmpty(variables) || !string.IsNullOrEmpty(properties));

					#region Get / Set Optimizations
					if(setting.runtimeOptimization && (classes is IClassComponent || classes is IClassAsset)) {
						var variableDatas = generatorData.GetVariables().Where(v => v.isInstance && (v.modifier == null || !v.modifier.isPrivate));
						{//Set Variables
							List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
							foreach(var var in variableDatas) {
								list.Add(
									new KeyValuePair<string, string>(
										Value(var.name),
										Flow(
											Set(var.name, Invoke(typeof(uNodeHelper), nameof(uNodeHelper.SetObject), var.name, "value", "@operator"))
										//GenerateSwitchStatement("@operator",
										//	cases: new[] {
										//		new KeyValuePair<string[], string>(
										//			new [] {
										//				ParseValue('+'),
										//				ParseValue('-'),
										//				ParseValue('/'),
										//				ParseValue('*'),
										//				ParseValue('%')
										//			},
										//			GenerateSetCode(var.name, GenerateInvokeCode(typeof(uNodeHelper), nameof(uNodeHelper.SetObject), var.name, "value", "@operator")).RemoveSemicolon()
										//		)
										//	},
										//	@default: GenerateSetCode(var.name, "value")
										//)
										)
									)
								);
							}
							if(list.Count > 0) {
								{
									var method = generatorData.AddMethod(
										nameof(RuntimeBehaviour.SetVariable),
										Type(typeof(void)),
										new[] {
										new MPData("Name", Type(typeof(string))),
										new MPData("value", Type(typeof(object))),
										}
									);
									method.modifier = new FunctionModifier() {
										Public = true,
										Override = true,
									};
									method.code = Flow(
										DoGenerateInvokeCode(nameof(RuntimeBehaviour.SetVariable), new[] { "Name", "value", Value('=') }).AddSemicolon()
									);
								}
								{
									var method = generatorData.AddMethod(
										nameof(RuntimeBehaviour.SetVariable),
										Type(typeof(void)),
										new[] {
										new MPData("Name", Type(typeof(string))),
										new MPData("value", Type(typeof(object))),
										new MPData("@operator", Type(typeof(char))),
										}
									);
									method.modifier = new FunctionModifier() {
										Public = true,
										Override = true,
									};
									method.code = Flow(
										Set("value", Invoke(typeof(uNodeHelper), nameof(uNodeHelper.GetActualRuntimeValue), "value")),
										Switch("Name", list, FlowInvoke("base", nameof(RuntimeBehaviour.SetVariable), "Name", "value", "@operator"))
									);
								}
							}
						}
						{//Get Variables
							List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
							foreach(var var in variableDatas) {
								list.Add(
									new KeyValuePair<string, string>(
										Value(var.name),
										Return(var.name)
									)
								);
							}
							if(list.Count > 0) {
								{
									var method = generatorData.AddMethod(
										nameof(RuntimeBehaviour.GetVariable),
										Type(typeof(object)),
										new[] {
											new MPData("Name", Type(typeof(string))),
										}
									);
									method.modifier = new FunctionModifier() {
										Public = true,
										Override = true,
									};
									method.code = Flow(
										Switch("Name", list),
										Return(Invoke("base", nameof(RuntimeBehaviour.GetVariable), "Name"))
									);
								}
							}
						}
					}
					#endregion

					string functionData = null;
					generatorData.state.state = State.Function;
					//Generating Event Functions ex: State Machine, Coroutine nodes, etc.
					GenerateEventFunction();
					if(generatorData.coroutineEvent.Count > 0) {
						generatorData.state.isStatic = false;
						generatorData.state.state = State.Function;
						foreach(var p in generatorData.coroutineEvent) {
							var pair = p;
							if(!string.IsNullOrEmpty(pair.Value.variableName) && pair.Key != null) {
								ThreadingUtil.Queue((() => {
									classData += CG.WrapWithInformation(
										DeclareVariable(
											pair.Value.variableName,
											typeof(Runtime.EventCoroutine),
											New(typeof(Runtime.EventCoroutine)),
											modifier: FieldModifier.PrivateModifier),
										pair.Key).AddLineInFirst();
									string onStopAction = pair.Value.onStop;
									string invokeCode = pair.Value.customExecution == null ?
										_coroutineEventCode.CGInvoke(null, generatorData.GetEventID(pair.Key)) :
										pair.Value.customExecution();
									string genData = null;
									if(!string.IsNullOrEmpty(onStopAction)) {
										genData = pair.Value.variableName.CGFlowInvoke(
											nameof(Runtime.EventCoroutine.Setup),
											Value(classes),
											invokeCode,
											Lambda(onStopAction)
										);
										//if(onStopAction.Contains("yield return")) {
										//	var uid = "stop_" + pair.Value.variableName;
										//	generatorData.InsertCustomUIDMethod("_StopCoroutineEvent", typeof(IEnumerable), uid, onStopAction);
										//	onStopAction = StaticInvoke("_StopCoroutineEvent", GeneratorExtensions.CGValue(uid));
										//	genData = pair.Value.variableName.CGFlowInvoke(
										//		nameof(Runtime.EventCoroutine.Setup),
										//		Value(classes),
										//		invokeCode,
										//		onStopAction
										//	);
										//} else {
										//}
									} else {
										genData = pair.Value.variableName.CGFlowInvoke(
											nameof(Runtime.EventCoroutine.Setup),
											Value(classes),
											invokeCode
										);
									}
									if(setting.debugScript && pair.Key as NodeComponent) {
										if(setting.debugPreprocessor)
											genData += "\n#if UNITY_EDITOR".AddLineInFirst();
										genData += DoGenerateInvokeCode(pair.Value + ".Debug", new string[] {
											Value(uNodeUtility.GetObjectID(classes)), Value(uNodeUtility.GetObjectID(pair.Key as NodeComponent))
										}).AddSemicolon().AddLineInFirst();
										if(setting.debugPreprocessor)
											genData += "\n#endif".AddLineInFirst();
									}
									InitData.Add(WrapWithInformation(genData, pair.Key));
								}));
							}
						}
						ThreadingUtil.WaitQueue();
						if(InitData.Count > 0) {//Insert init code into Awake functions.
							string code = "";
							foreach(string s in InitData) {
								code += "\n" + s;
							}
							var method = generatorData.AddMethod("Awake", Type(typeof(void)), new string[0]);
							method.code = code + method.code.AddLineInFirst();
						}
					}
					foreach(MData d in generatorData.methodData) {
						var data = d;
						ThreadingUtil.Queue(() => {
							generatorData.state.isStatic = data.modifier != null && data.modifier.Static;
							functionData += data.GenerateCode().AddFirst("\n\n");
						});
					}
					ThreadingUtil.WaitQueue();
					classData += functionData;
					//generate Nested Type
					if(classes is INestedClassSystem) {
						var nestedType = (classes as INestedClassSystem).NestedClass;
						if(nestedType) {
							generatorData.state.state = State.Classes;
							ThreadingUtil.Do(() => {
								GameObject targetObj = nestedType.gameObject;
								setting.updateProgress?.Invoke(progress, "Generating NestedType...");
								isGenerating = false;//This to prevent freeze
								var nestedData = Generate(new GeneratorSetting(targetObj, setting));//Start generating nested type
								classData += nestedData.FullTypeScript().AddLineInFirst().AddLineInFirst();
								//Restore to prev state
								isGenerating = true;
								generatorData.setting = setting;
							});
						}
					}
					classBuilder.Append(classData.AddTabAfterNewLine(1, false));
					classBuilder.Append("\n}");
				}
				if(setting.graphs.Count == 0) {
					ResetGenerator();
					generatorData.setting = setting;
				}
				ThreadingUtil.Do(() => {
					//Generate interfaces
					if(setting.interfaces != null) {
						foreach(var t in setting.interfaces) {
							if(string.IsNullOrEmpty(t.name))
								continue;
							string value = null;
							value += t.modifiers.GenerateCode() + "interface " + t.name + " {";
							string contents = null;
							foreach(var p in t.properties) {
								if(string.IsNullOrEmpty(p.name))
									continue;
								string localVal = Type(p.returnType) + " " + p.name + " {";
								if(p.accessor == PropertyAccessorKind.ReadOnly) {
									localVal += "get;".AddLineInFirst().AddTabAfterNewLine();
								} else if(p.accessor == PropertyAccessorKind.WriteOnly) {
									localVal += "set;".AddLineInFirst().AddTabAfterNewLine();
								} else {
									localVal += ("get;".AddLineInFirst() + "set;".AddLineInFirst()).AddTabAfterNewLine();
								}
								localVal += "\n}";
								contents += localVal.AddLineInFirst().AddFirst("\n", contents != null);
							}
							foreach(var f in t.functions) {
								if(string.IsNullOrEmpty(f.name))
									continue;
								string param = null;
								foreach(var p in f.parameters) {
									if(!string.IsNullOrEmpty(param)) {
										param += ", ";
									}
									if(p.refKind != ParameterData.RefKind.None) {
										if(p.refKind == ParameterData.RefKind.Ref) {
											param += "ref ";
										} else if(p.refKind == ParameterData.RefKind.Out) {
											param += "out ";
										}
									}
									param += Type(p.type) + " " + p.name;
								}
								string gParam = null;
								foreach(var p in f.genericParameters) {
									if(!string.IsNullOrEmpty(gParam)) {
										gParam += ", ";
									}
									gParam += p.name;
								}
								if(!string.IsNullOrEmpty(gParam)) {
									gParam = "<" + gParam + ">";
								}
								contents += (Type(f.returnType) + " " + f.name + gParam + "(" + param + ");").AddLineInFirst().AddFirst("\n", contents != null);
							}
							value += contents.AddTabAfterNewLine(false) + "\n}";
							classBuilder.Append(value.AddLineInFirst().AddLineInFirst());
						}
					}
					//Generate enums
					if(setting.enums != null) {
						foreach(var t in setting.enums) {
							if(string.IsNullOrEmpty(t.name) || t.enumeratorList.Length == 0)
								continue;
							string value = null;
							value += t.modifiers.GenerateCode() + "enum " + t.name;
							if(t.inheritFrom.isAssigned && t.inheritFrom.Get<Type>() != typeof(int)) {
								value += " : " + Type(t.inheritFrom);
							}
							value += " {";
							string EL = null;
							foreach(var e in t.enumeratorList) {
								EL += "\n" + e.name + ",";
							}
							value += EL.AddTabAfterNewLine() + "\n}";
							classBuilder.Append(value.AddLineInFirst().AddLineInFirst());
						}
					}
				});
				ThreadingUtil.Do(() => {
					//Initialize the generated data for futher use
					generatedData.errors = generatorData.errors;
					generatedData.InitOwner();
				});
				RegisterScriptHeader("#pragma warning disable");
				//Finish generating scripts
				setting.updateProgress?.Invoke(1, "finish");
				ThreadingUtil.Do(() => {
					OnSuccessGeneratingGraph?.Invoke(generatedData, setting);
				});
				//Ensure the generator data is clean.
				ResetGenerator();
				isGenerating = false;
				//Return the generated data
				return generatedData;
			}
			finally {
				isGenerating = false;
			}
		}
		#endregion

		#region Private Functions
		private static Type GetCompatibilityType(Type type) {
			if(type is RuntimeType) {
				if(type.IsCastableTo(typeof(MonoBehaviour))) {
					return typeof(RuntimeBehaviour);
				} else if(type.IsCastableTo(typeof(ScriptableObject))) {
					return typeof(BaseRuntimeAsset);
				} else if(type.IsInterface) {
					return typeof(IRuntimeClass);
				}
			}
			return type;
		}

		private static void GenerateFunctions(Action<float, string> updateProgress = null) {
			//if((runtimeUNode == null || runtimeUNode.eventNodes.Count == 0) && uNodeObject.Functions.Count == 0)
			//	return;
			float progress = 0;
			float count = 0;
			if(generatorData.allNode.Count > 0) {
				count = generatorData.allNode.Count / generatorData.allNode.Count;
			}
			generatorData.state.state = State.Function;

			#region Generate Nodes
			generatorData.state.isStatic = false;
			for(int i = 0; i < generatorData.allNode.Count; i++) {
				Node node = generatorData.allNode[i] as Node;
				if(node == null)
					continue;
				ThreadingUtil.Queue(() => {
					if(node != null) {
						setting.objectInformations[node.GetInstanceID()] = uNodeUtility.GetObjectID(node);
						Action action;
						if(generatorData.initActionForNodes.TryGetValue(node, out action)) {
							action();
						}
						//Skip if not flow node
						if(!node.IsFlowNode())
							return;
						if(node.rootObject != null) {
							var ro = node.rootObject;
							if(ro is uNodeFunction) {
								generatorData.state.isStatic = (ro as uNodeFunction).modifiers.Static;
							}
						}
						if(IsStateNode(node)) {
							isInUngrouped = true;
						}
						GenerateNode(node);
						isInUngrouped = false;
						progress += count;
						if(updateProgress != null)
							updateProgress(progress / generatorData.allNode.Count, "generating node:" + node.gameObject.name);
					}
				});
			}
			ThreadingUtil.WaitQueue();
			generatorData.state.isStatic = false;
			#endregion

			#region Generate Functions
			for(int x = 0; x < graph.Functions.Count; x++) {
				uNodeFunction function = graph.Functions[x];
				if(function == null)
					return;
				ThreadingUtil.Queue(() => {
					setting.objectInformations[function.GetInstanceID()] = uNodeUtility.GetObjectID(function);
					generatorData.state.isStatic = function.modifiers.Static;
					List<AData> attribute = new List<AData>();
					if(function.attributes != null && function.attributes.Length > 0) {
						foreach(var a in function.attributes) {
							attribute.Add(TryParseAttributeData(a));
						}
					}
					MData mData = generatorData.GetMethodData(
						function.Name,
						function.parameters.Select(i => Type(i.type)).ToList(),
						function.genericParameters.Length
					);
					if(mData == null) {
						mData = new MData(
							function.Name,
							Type(function.returnType),
							function.parameters.Select(i => new MPData(i.name, Type(i.type), i.refKind)).ToList(),
							function.genericParameters.Select(i => new GPData(i.name, i.typeConstraint.Get<Type>())).ToList()
						);
						generatorData.methodData.Add(mData);
					}
					mData.modifier = function.modifiers;
					mData.attributes = attribute;
					mData.summary = function.summary;
					mData.owner = function;
					if(function.localVariable != null) {
						string result = null;
						foreach(var vdata in function.localVariable) {
							if(IsInstanceVariable(vdata)) {
								continue;
							} else if(!vdata.resetOnEnter) {
								M_RegisterVariable(vdata).modifier.SetPrivate();
								continue;
							}
							//if(vdata.type.isAssigned && vdata.type.targetType == MemberData.TargetType.Type && vdata.value == null && vdata.type.startType != null && vdata.type.startType.IsValueType) {
							//	result += (ParseType(vdata.type) + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
							//	continue;
							//}
							if((vdata.value == null || vdata.type.startType == null) && vdata.type.targetType == MemberData.TargetType.Type) {
								var vType = Type(vdata.type);
								result += (vType + " " + GetVariableName(vdata) + $" = default({vType});").AddFirst("\n", !string.IsNullOrEmpty(result));
								continue;
							}
							if(vdata.type.targetType == MemberData.TargetType.uNodeGenericParameter) {
								string vType = Type(vdata.type);
								if(vdata.variable != null) {
									result += (vType + " " + GetVariableName(vdata) + $" = default({vType});").AddFirst("\n", !string.IsNullOrEmpty(result));
								} else {
									result += (vType + " " + GetVariableName(vdata) + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
								}
								continue;
							}
							result += (Type(vdata.type) + " " + GetVariableName(vdata) + " = " + Value(vdata.value) + ";").AddFirst("\n", !string.IsNullOrEmpty(result));
						}
						mData.code += result.AddLineInFirst();
					}
					if(function.startNode != null && (mData.modifier == null || !mData.modifier.Abstract)) {
						mData.code += GenerateNode(function.startNode).AddLineInFirst();
					}
				});
			}
			ThreadingUtil.WaitQueue();
			#endregion

			#region Generate Event Nodes
			generatorData.state.isStatic = false;
			for(int i = 0; i < generatorData.eventNodes.Count; i++) {
				BaseGraphEvent eventNode = generatorData.eventNodes[i];
				if(eventNode == null)
					continue;
				try {
					ThreadingUtil.Do(eventNode.GenerateCode);
				}
				catch(Exception ex) {
					if(setting != null && setting.isAsync) {

					} else {
						if(!generatorData.hasError)
							UnityEngine.Debug.LogError("Error generate code from event: " + eventNode.GetNodeName() + "\nFrom graph:" + graph.FullGraphName + "\nError:" + ex.ToString(), eventNode);
						generatorData.hasError = true;
						throw;
					}
				}
			}
			#endregion
		}

		private static void GenerateEventFunction() {
			List<string> CoroutineEventFunc = new List<string>();
			if(generatorData.stateNodes.Count > 0) {
				//Creating ActivateEvent for non grouped node
				for(int i = 0; i < generatorData.stateNodes.Count; i++) {
					Node node = generatorData.stateNodes.ElementAt(i) as Node;
					if(node == null || !node.IsFlowNode()) //skip on node is not flow node.
						continue;
					var evt = GetCoroutineEvent(node);
					if(evt != null && evt.customExecution == null) {
						ThreadingUtil.Queue(() => {
							isInUngrouped = true;
							string generatedCode = GenerateNode(node);
							isInUngrouped = false;
							if(string.IsNullOrEmpty(generatedCode))
								return;
							if(!setting.fullComment) {
								generatedCode = "\n" + generatedCode;
							}
							var strs = generatedCode.Split('\n');
							int yieldCount = 0;
							int lastStr = 0;
							for(int x = 0; x < strs.Length; x++) {
								if(strs[x].Contains("yield ")) {
									yieldCount++;
								}
								if(!string.IsNullOrEmpty(strs[x].Trim())) {
									lastStr = x;
								}
							}
							if(yieldCount == 0 || yieldCount == 1 && strs[lastStr].Contains("yield ")) {
								SetStateInitialization(node, CG.Routine(Lambda(generatedCode.Replace("yield ", "").AddTabAfterNewLine())));
								return;
							}
							string s = "case " + generatorData.GetEventID(node) + ": {" +
								generatedCode.AddTabAfterNewLine(1) + "\n}";
							CoroutineEventFunc.Add(s + "\nbreak;");
						});
					}
				}
				ThreadingUtil.WaitQueue();
			}
			if(generatorData.eventCoroutineData.Count > 0) {
				foreach(var pair in generatorData.eventCoroutineData) {
					string data = "case " + generatorData.GetEventID(pair.Key) + ": {" +
						(pair.Value.AddLineInFirst() + "\nbreak;").AddTabAfterNewLine(1) + "\n}";
					CoroutineEventFunc.Add(data);
				}
			}
			if(CoroutineEventFunc.Count > 0 || generatorData.coroutineEvent.Any(e => e.Value.customExecution == null)) {
				MData method = generatorData.AddMethod(_coroutineEventCode, Type(typeof(IEnumerable)), new[] { new MPData("uid", Type(typeof(int))) });
				method.code += "\nswitch(uid) {";
				foreach(string str in CoroutineEventFunc) {
					method.code += ("\n" + str).AddTabAfterNewLine(1);
				}
				foreach(var pair in generatorData.coroutineEvent) {
					if(pair.Value.customExecution != null)
						continue;
					string data = pair.Value.contents.AddFirst("\n");
					if(!string.IsNullOrEmpty(data)) {
						method.code += ("\ncase " + generatorData.GetEventID(pair.Key) + ": {" + data.AddTabAfterNewLine(1) + "\n}\nbreak;").AddTabAfterNewLine();
					}
				}
				method.code += "\n}\nyield break;";
			}
			if(generatorData.eventActions.Count > 0) {
				MData method = generatorData.AddMethod(_activateActionCode, Type(typeof(bool)), new[] { new MPData("ID", Type(typeof(int))) });
				method.code += "\nswitch(ID) {";
				string str = null;
				foreach(KeyValuePair<Block, string> value in generatorData.eventActions) {
					if(actionDataID.ContainsKey(value.Key)) {
						string data = value.Value.AddFirst("\n");
						str += "\ncase " + actionDataID[value.Key] + ": {" + data.AddTabAfterNewLine(1) + "\n}\nbreak;";
					}
				}
				method.code += str.AddTabAfterNewLine(1).Add("\n") + "}\nreturn true;";
			}
			if(generatorData.customUIDMethods.Count > 0) {
				foreach(var pair in generatorData.customUIDMethods) {
					foreach(var pair2 in pair.Value) {
						MData method = generatorData.AddMethod(pair.Key, Type(pair2.Key), new[] { new MPData("name", Type(typeof(string))) });
						method.code += "\nswitch(name) {";
						foreach(var pair3 in pair2.Value) {
							string data = pair3.Value.AddFirst("\n");
							method.code += ("\ncase \"" + pair3.Key + "\": {" + data.AddTabAfterNewLine(1) + "\n}\nbreak;").AddTabAfterNewLine();
						}
						method.code += "\n}";
						if(pair2.Key == typeof(IEnumerable) || pair2.Key == typeof(IEnumerator)) {
							method.code += "yield break;".AddLineInFirst();
						} else if(pair2.Key != typeof(void)) {
							method.code += ("return default(" + Type(pair2.Key) + ");").AddLineInFirst();
						}
					}
				}
			}
			if(generatorData.debugMemberMap.Count > 0) {
				var map = generatorData.debugMemberMap;
				if(map.Count > 0) {
					MData method = generatorData.AddMethod(_debugGetValueCode, "T", new MPData[] { new MPData("ID", Type(typeof(int))), new MPData("debugValue", "T") }, new GPData[] { new GPData("T") });
					method.code += "\nswitch(ID) {";
					string str = null;
					foreach(var value in map) {
						string data = value.Value.Value.AddFirst("\n");
						str += "\ncase " + value.Value.Key + ": {" + data.AddTabAfterNewLine(1) + "\n}";
					}
					method.code += str.AddTabAfterNewLine(1).Add("\n") + "}\nthrow null;";
				}
			}
		}

		private static string GenerateVariables(Action<float, string> updateProgress = null) {
			if(generatorData.GetVariables().Count == 0)
				return null;
			float progress = 0;
			float count = generatorData.GetVariables().Count / generatorData.GetVariables().Count;
			string result = null;
			generatorData.state.state = State.Classes;
			foreach(VData vdata in generatorData.GetVariables()) {
				if(!vdata.isInstance)
					continue;
				ThreadingUtil.Queue(() => {
					try {
						generatorData.state.isStatic = vdata.IsStatic;
						string str = vdata.GenerateCode().AddFirst("\n", !string.IsNullOrEmpty(result));
						if(includeGraphInformation && vdata.variableRef is VariableData) {
							str = WrapWithInformation(str, vdata.variableRef);
						}
						result += str;
						progress += count;
						if(updateProgress != null)
							updateProgress(progress / generatorData.GetVariables().Count, "generating variable");
					}
					catch(Exception ex) {
						if(setting != null && setting.isAsync) {
							generatorData.errors.Add(new uNodeException("Error on generating variable:" + vdata.name + "\nFrom graph:" + graph.FullGraphName, ex, graph));
							generatorData.errors.Add(ex);
							//In case async return error commentaries.
							result = "/*Error from variable: " + vdata.name + " */";
							return;
						}
						UnityEngine.Debug.LogError("Error on generating variable:" + vdata.name + "\nFrom graph:" + graph.FullGraphName, graph);
						throw;
					}
				});
			}
			ThreadingUtil.WaitQueue();
			return result;
		}

		private static string GenerateProperties(Action<float, string> updateProgress = null) {
			if(generatorData.properties.Count == 0)
				return null;
			float progress = 0;
			float count = generatorData.properties.Count / generatorData.properties.Count;
			string result = null;
			generatorData.state.state = State.Property;
			foreach(var prop in generatorData.properties) {
				if(prop == null || !prop.obj)
					continue;
				ThreadingUtil.Queue(() => {
					setting.objectInformations[prop.obj.GetInstanceID()] = uNodeUtility.GetObjectID(prop.obj);
					generatorData.state.isStatic = prop.modifier != null && prop.modifier.Static;
					string str = prop.GenerateCode().AddFirst("\n", result != null);
					if(includeGraphInformation && prop.obj != null) {
						str = WrapWithInformation(str, prop.obj);
					}
					result += str;
					progress += count;
					if(updateProgress != null)
						updateProgress(progress / generatorData.properties.Count, "generating property");
				});
			}
			ThreadingUtil.WaitQueue();
			return result;
		}

		private static string GenerateConstructors(Action<float, string> updateProgress = null) {
			if(generatorData.constructors.Count == 0)
				return null;
			float progress = 0;
			float count = generatorData.constructors.Count / generatorData.constructors.Count;
			string result = null;
			generatorData.state.isStatic = false;
			generatorData.state.state = State.Constructor;
			for(int i = 0; i < generatorData.constructors.Count; i++) {
				var ctor = generatorData.constructors[i];
				if(ctor == null || !ctor.obj)
					continue;
				ThreadingUtil.Queue(() => {
					setting.objectInformations[ctor.obj.GetInstanceID()] = uNodeUtility.GetObjectID(ctor.obj);
					string str = ctor.GenerateCode().AddFirst("\n\n", result != null);
					if(includeGraphInformation && ctor.obj != null) {
						str = WrapWithInformation(str, ctor.obj);
					}
					result += str;
					progress += count;
					if(updateProgress != null)
						updateProgress(progress / generatorData.constructors.Count, "generating constructor");
				});
			}
			ThreadingUtil.WaitQueue();
			return result;
		}
		#endregion

		#region GetCorrectName
		/// <summary>
		/// Function to get correct code for Get correct name in MemberReflection
		/// </summary>
		/// <param name="mData"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		private static string GetCorrectName(MemberData mData, MemberData[] parameters = null, ValueData initializer = null, Action<string, string> onEnterAndExit = null, bool autoConvert = false) {
			List<string> result = new List<string>();
			MemberInfo[] memberInfo = null;
			switch(mData.targetType) {
				case MemberData.TargetType.None:
				case MemberData.TargetType.Null:
				case MemberData.TargetType.Type:
				case MemberData.TargetType.uNodeGenericParameter:
					break;
				default:
					memberInfo = mData.GetMembers(false);
					break;
				case MemberData.TargetType.SelfTarget: {
					if(mData.instance == null) {
						throw new System.Exception("Variable with self target type can't have null value");
					}
					return Value(mData.instance);
				}
				case MemberData.TargetType.Values: {
					if(initializer != null && initializer.Value as ConstructorValueData != null) {
						ConstructorValueData ctor = initializer.Value as ConstructorValueData;
						if(ctor.initializer != null && ctor.initializer.Length > 0) {
							return Value(mData.Get(), ctor.initializer);
						}
					}
					return Value(mData.Get());
				}
				case MemberData.TargetType.uNodeFunction: {
					string data = mData.startName;
					string[] gType;
					string[] pType = null;
					if(mData.SerializedItems.Length > 0) {
						MemberDataUtility.GetItemName(SerializerUtility.Deserialize<MemberData.ItemData>(
									mData.SerializedItems[0]),
									mData.targetReference,
									out gType,
									out pType);
						if(gType.Length > 0) {
							data += String.Format("<{0}>", String.Join(", ", gType));
						}
					}
					data += "(";
					if(pType != null && pType.Length > 0) {
						var func = mData.GetUnityObject() as uNodeFunction;
						for(int i = 0; i < pType.Length; i++) {
							if(i != 0) {
								data += ", ";
							}
							if(func != null && func.parameters.Length > i) {
								if(func.parameters[i].isByRef) {
									if(func.parameters[i].refKind == ParameterData.RefKind.Ref) {
										data += "ref ";
									} else if(func.parameters[i].refKind == ParameterData.RefKind.Out) {
										data += "out ";
									}
								}
							}
							MemberData p = parameters[i];
							if(debugScript && setting.debugValueNode) {
								setting.debugScript = false;
								data += Value((object)p);
								setting.debugScript = true;
							} else {
								data += Value((object)p);
							}
						}
					}
					data += ")";
					return data;
				}
			}
			if(memberInfo != null && memberInfo.Length > 0) {
				int accessIndex = 0;
				if(parameters != null && parameters.Length > 1 && (parameters[0] == null || !parameters[0].isAssigned)) {
					accessIndex = 1;
				}
				string enter = null;
				string exit = null;
				for(int i=0;i< memberInfo.Length;i++) {
					var member = memberInfo[i];
					if(member == null)
						throw new Exception("Incorrect/Unassigned Target");
					string genericData = null;
					if(mData.Items == null || i >= mData.Items.Length)
						break;
					MemberData.ItemData iData = mData.Items[i];
					if(mData.Items.Length > i + 1) {
						iData = mData.Items[i + 1];
					}
					if(iData != null) {
						MemberDataUtility.GetItemName(mData.Items[i + 1],
							mData.targetReference,
							out var genericType,
							out var paramsType);
						if(genericType.Length > 0) {
							if(mData.targetType != MemberData.TargetType.uNodeGenericParameter &&
								mData.targetType != MemberData.TargetType.Type) {
								genericData += string.Format("<{0}>", string.Join(", ", genericType)).Replace('+', '.');
							} else {
								genericData += string.Format("{0}", string.Join(", ", genericType)).Replace('+', '.');
							}
						}
					}
					bool isRuntime = member is IRuntimeMember;
					if(isRuntime && !(member is IFakeMember)) {
						if(member is RuntimeField) {
							result.Add(GenerateGetRuntimeVariable(member as RuntimeField) + genericData);
						} else if(member is RuntimeProperty) {
							result.Add(GenerateGetRuntimeProperty(member as RuntimeProperty) + genericData);
						} else if(member is RuntimeMethod method) {
							ParameterInfo[] paramInfo = method.GetParameters();
							MemberData[] datas = new MemberData[paramInfo.Length];
							for(int index = 0; index < paramInfo.Length; index++) {
								datas[index] = parameters[accessIndex];
								accessIndex++;
							}
							result.Add(GenerateInvokeRuntimeMethod(member as RuntimeMethod, datas, ref enter, ref exit, autoConvert) + genericData);
						} else {
							throw new InvalidOperationException();
						}
					} else if(member is MethodInfo) {
						MethodInfo method = member as MethodInfo;
						ParameterInfo[] paramInfo = method.GetParameters();
						if(paramInfo.Length > 0) {
							if(parameters == null) {
								////Return only method name if the parameter is null.
								//result.Add(member.Name);
								//continue;
								throw new ArgumentNullException(nameof(parameters), "The method does have parameters but there's no given parameter values");
							}
							string data = null;
							List<string> dataList = new List<string>();
							for(int index = 0; index < paramInfo.Length; index++) {
								MemberData p = parameters[accessIndex];
								string pData = null;
								if(paramInfo[index].ParameterType.IsByRef) {
									if(paramInfo[index].IsOut) {
										pData += "out ";
									} else {
										pData += "ref ";
									}
								}
								if(pData != null) {
									bool correct = true;
									if(p.type != null && p.type.IsValueType) {
										MemberInfo[] MI = p.GetMembers();
										if(MI != null && MI.Length > 1 && ReflectionUtils.GetMemberType(MI[MI.Length - 2]).IsValueType) {
											string varName = GenerateVariableName("tempVar");
											var pVal = Value((object)p);
											pData += varName + "." + pVal.Remove(pVal.IndexOf(ParseStartValue(p)), ParseStartValue(p).Length + 1).CGSplitMember().Last();
											if(pVal.LastIndexOf(".") >= 0) {
												pVal = pVal.Remove(pVal.LastIndexOf("."));
											}
											enter += Type(ReflectionUtils.GetMemberType(MI[MI.Length - 2])) + " " + varName + " = " + pVal + ";\n";
											exit += pVal + " = " + varName + ";";
											correct = false;
										}
									}
									if(correct) {
										if(debugScript && setting.debugValueNode) {
											setting.debugScript = false;
											pData += Value((object)p);
											setting.debugScript = true;
										} else {
											pData += Value((object)p);
										}
									}
								} else {
									pData += Value((object)p);
								}
								dataList.Add(pData);
								accessIndex++;
							}
							for(int index = 0; index < dataList.Count; index++) {
								if(index != 0) {
									data += ", ";
								}
								data += dataList[index];
							}
							if(member.Name == "Item" || member.Name == "get_Item") {
								if(isRuntime && member is IFakeMember) {
									if(generatePureScript && !(ReflectionUtils.GetMemberType(member) is IFakeMember)) {
										//Get indexer and convert to actual type
										result.Add(Convert("[" + data + "]", ReflectionUtils.GetMemberType(member)));
									} else if(!generatePureScript) {
										//Get indexer and convert to actual type
										result.Add(Convert("[" + data + "]", ReflectionUtils.GetMemberType(member), true));
									} else {
										//Get indexer
										result.Add("[" + data + "]");
									}
								} else {
									//Get indexer
									result.Add("[" + data + "]");
								}
							} else if(member.Name.StartsWith("set_", StringComparison.Ordinal)) {
								if(member.Name.Equals("set_Item") && method.GetParameters().Length == 2) {
									//Set indexer
									result.Add("[" + dataList[0] + "] = " + dataList[1]);
								} else {
									//Set property
									result.Add(member.Name.Replace("set_", "") + " = " + data + genericData);
								}
							} else if(member.Name.StartsWith("op_", StringComparison.Ordinal)) {
								if(member.Name == "op_Addition") {
									result.Add(dataList[0] + "+" + dataList[1]);
								} else if(member.Name == "op_Subtraction") {
									result.Add(dataList[0] + "-" + dataList[1]);
								} else if(member.Name == "op_Division") {
									result.Add(dataList[0] + "/" + dataList[1]);
								} else if(member.Name == "op_Multiply") {
									result.Add(dataList[0] + "*" + dataList[1]);
								} else if(member.Name == "op_Modulus") {
									result.Add(dataList[0] + "%" + dataList[1]);
								} else if(member.Name == "op_Equality") {
									result.Add(dataList[0] + "==" + dataList[1]);
								} else if(member.Name == "op_Inequality") {
									result.Add(dataList[0] + "!=" + dataList[1]);
								} else if(member.Name == "op_LessThan") {
									result.Add(dataList[0] + "<" + dataList[1]);
								} else if(member.Name == "op_GreaterThan") {
									result.Add(dataList[0] + ">" + dataList[1]);
								} else if(member.Name == "op_LessThanOrEqual") {
									result.Add(dataList[0] + "<=" + dataList[1]);
								} else if(member.Name == "op_GreaterThanOrEqual") {
									result.Add(dataList[0] + ">=" + dataList[1]);
								} else if(member.Name == "op_BitwiseAnd") {
									result.Add(dataList[0] + "&" + dataList[1]);
								} else if(member.Name == "op_BitwiseOr") {
									result.Add(dataList[0] + "|" + dataList[1]);
								} else if(member.Name == "op_LeftShift") {
									result.Add(dataList[0] + "<<" + dataList[1]);
								} else if(member.Name == "op_RightShift") {
									result.Add(dataList[0] + ">>" + dataList[1]);
								} else if(member.Name == "op_ExclusiveOr") {
									result.Add(dataList[0] + "^" + dataList[1]);
								} else if(member.Name == "op_UnaryNegation") {
									result.Add(dataList[0] + "-" + dataList[1]);
								} else if(member.Name == "op_UnaryPlus") {
									result.Add(dataList[0] + "+" + dataList[1]);
								} else if(member.Name == "op_LogicalNot") {
									result.Add(dataList[0] + "!" + dataList[1]);
								} else if(member.Name == "op_OnesComplement") {
									result.Add(dataList[0] + "~" + dataList[1]);
								} else if(member.Name == "op_Increment") {
									result.Add(dataList[0] + "++" + dataList[1]);
								} else if(member.Name == "op_Decrement") {
									result.Add(dataList[0] + "--" + dataList[1]);
								} else {
									result.Add(member.Name + genericData + "(" + data + ")");
								}
								return string.Join(".", result.ToArray());
							} else if(member.Name.StartsWith("Get", StringComparison.Ordinal) && method.GetParameters().Length == 1 && (i > 0 && ReflectionUtils.GetMemberType(memberInfo[i - 1]).IsArray || i == 0 && mData.startType.IsArray)) {
								if(result.Count > 0) {
									result[result.Count - 1] = result[result.Count - 1] + "[" + data + "]";
								} else {
									result.Add("[" + data + "]");
								}
							} else if(member.Name.StartsWith("Set", StringComparison.Ordinal) && method.GetParameters().Length == 2 && (i > 0 && ReflectionUtils.GetMemberType(memberInfo[i - 1]).IsArray || i == 0 && mData.startType.IsArray)) {
								result.Add(member.Name.Replace("Set", "[" + dataList[0] + "]") + " = " + dataList[1]);
							} else {
								result.Add(member.Name + genericData + "(" + data + ")");
							}
						} else if(member.Name.StartsWith("get_", StringComparison.Ordinal)) {
							result.Add(member.Name.Replace("get_", "") + genericData);
						} else {
							if(i == memberInfo.Length - 1 && parameters != null && accessIndex < parameters.Length) {
								string data = null;
								for(int x = accessIndex; x < parameters.Length; x++) {
									if(x != accessIndex) {
										data += ", ";
									}
									MemberData p = parameters[x];
									data += Value((object)p);
								}
								result.Add(member.Name + genericData + "(" + data + ")");
							} else {
								result.Add(member.Name + genericData + "()");
							}
						}
					} else if(member is ConstructorInfo) {
						ConstructorInfo ctor = member as ConstructorInfo;
						ParameterInfo[] paramInfo = ctor.GetParameters();
						string ctorInit = ParseConstructorInitializer(initializer);
						if(paramInfo.Length > 0) {
							string data = null;
							List<string> dataList = new List<string>();
							for(int index = 0; index < paramInfo.Length; index++) {
								MemberData p = parameters[accessIndex];
								string pData = null;
								if(paramInfo[index].IsOut) {
									pData += "out ";
								} else if(paramInfo[index].ParameterType.IsByRef) {
									pData += "ref ";
								}
								if(debugScript && setting.debugValueNode) {
									setting.debugScript = false;
									pData += Value((object)p);
									setting.debugScript = true;
								} else {
									pData += Value((object)p);
								}
								dataList.Add(pData);
								accessIndex++;
							}
							for(int index = 0; index < dataList.Count; index++) {
								if(index != 0) {
									data += ", ";
								}
								data += dataList[index];
							}
							if(ctor.DeclaringType.IsArray) {
								if(result.Count > 0) {
									result.Add("(new " + Type(ctor.DeclaringType.GetElementType()) + "[" + data + "]" + ctorInit + ")");
								} else {
									result.Add("new " + Type(ctor.DeclaringType.GetElementType()) + "[" + data + "]" + ctorInit);
								}
							} else {
								if(result.Count > 0) {
									result.Add("(new " + Type(ctor.DeclaringType) + "(" + data + ")" + ctorInit + ")");
								} else {
									result.Add("new " + Type(ctor.DeclaringType) + "(" + data + ")" + ctorInit);
								}
							}
						} else {
							if(i == memberInfo.Length - 1 && parameters != null && accessIndex < parameters.Length) {
								string data = null;
								for(int x = accessIndex; x < parameters.Length; x++) {
									if(x != accessIndex) {
										data += ", ";
									}
									MemberData p = parameters[x];
									data += Value((object)p);
								}
								if(result.Count > 0) {
									result.Add("(new " + Type(ctor.DeclaringType) + "(" + data + ")" + ctorInit + ")");
								} else {
									result.Add("new " + Type(ctor.DeclaringType) + "(" + data + ")" + ctorInit);
								}
							} else {
								if(result.Count > 0) {
									result.Add("(new " + Type(ctor.DeclaringType) + "()" + ctorInit + ")");
								} else {
									result.Add("new " + Type(ctor.DeclaringType) + "()" + ctorInit);
								}
							}
						}
					} else {
						result.Add(member.Name + genericData);
					}
				}
				if(enter != null && onEnterAndExit != null)
					onEnterAndExit(enter, exit);
			} else if(mData.targetType == MemberData.TargetType.Constructor) {
				string ctorInit = ParseConstructorInitializer(initializer);
				result.Add("new " + Type(mData.startType) + "()" + ctorInit);
			}
			if(result.Count > 0) {
				string resultCode = string.Join(".", result.ToArray());
				if(result.Any(i => i.StartsWith("[", StringComparison.Ordinal) || i.StartsWith("(", StringComparison.Ordinal))) {
					resultCode = null;
					for(int i = 0; i < result.Count; i++) {
						resultCode += result[i];
						if(i + 1 != result.Count && !result[i + 1].StartsWith("[", StringComparison.Ordinal) && !result[i + 1].StartsWith("(", StringComparison.Ordinal)) {
							resultCode += ".";
						}
					}
				}
				string startData;
				if(IsContainOperatorCode(mData.name)) {
					throw new System.Exception("unsupported generating operator code in current context");
				}
				if(mData.targetType == MemberData.TargetType.Constructor) {
					return resultCode;
				}
				startData = ParseStartValue(mData);
				if(string.IsNullOrEmpty(startData)) {
					return resultCode;
				}
				return startData.Add(".", !resultCode.StartsWith("[", StringComparison.Ordinal) && !resultCode.StartsWith("(", StringComparison.Ordinal)) + resultCode;
			} else if(mData.isAssigned) {
				string str = mData.name;
				string[] names = mData.namePath;
				if(mData.isAssigned && mData.SerializedItems != null && mData.SerializedItems.Length > 0) {
					if(mData.SerializedItems.Length == names.Length) {
						str = null;
						if(mData.targetType == MemberData.TargetType.Constructor) {
							str += "new " + mData.type.PrettyName();
						}
						int accessIndex = 0;
						if(parameters != null && parameters.Length > 1 && (parameters[0] == null || !parameters[0].isAssigned)) {
							accessIndex = 1;
						}
						for(int i = 0; i < names.Length; i++) {
							if(i != 0 && (mData.targetType != MemberData.TargetType.Constructor)) {
								str += ".";
							}
							if(mData.targetType != MemberData.TargetType.uNodeGenericParameter &&
								mData.targetType != MemberData.TargetType.Type &&
								mData.targetType != MemberData.TargetType.Constructor) {
								str += names[i];
							}
							MemberData.ItemData iData = mData.Items[i];
							if(iData != null) {
								string[] paramsType = new string[0];
								string[] genericType = new string[0];
								MemberDataUtility.GetItemName(mData.Items[i],
									mData.targetReference,
									out genericType,
									out paramsType);
								if(genericType.Length > 0) {
									if(mData.targetType != MemberData.TargetType.uNodeGenericParameter &&
										mData.targetType != MemberData.TargetType.Type) {
										str += string.Format("<{0}>", string.Join(", ", genericType));
									} else {
										str += string.Format("{0}", string.Join(", ", genericType));
									}
								}
								if(paramsType.Length > 0 ||
									mData.targetType == MemberData.TargetType.uNodeFunction ||
									mData.targetType == MemberData.TargetType.uNodeConstructor ||
									mData.targetType == MemberData.TargetType.Constructor ||
									mData.targetType == MemberData.TargetType.Method && !mData.isDeepTarget) {
									List<string> dataList = new List<string>();
									var func = mData.GetUnityObject() as RootObject;
									for(int index = 0; index < paramsType.Length; index++) {
										MemberData p = parameters[accessIndex];
										string data = null;
										if(func != null && func.parameters.Length > index) {
											if(func.parameters[index].isByRef) {
												if(func.parameters[index].refKind == ParameterData.RefKind.Ref) {
													data += "ref ";
												} else if(func.parameters[index].refKind == ParameterData.RefKind.Out) {
													data += "out ";
												}
											}
										}
										if(debugScript && setting.debugValueNode) {
											setting.debugScript = false;
											dataList.Add(data + Value((object)p));
											setting.debugScript = true;
										} else {
											dataList.Add(data + Value((object)p));
										}
										accessIndex++;
									}
									str += string.Format("({0})", string.Join(", ", dataList.ToArray()));
								}
							}
						}
					}
				} else if(mData.isAssigned) {
					switch(mData.targetType) {
						case MemberData.TargetType.Constructor:
							return "new " + mData.type.PrettyName() + "()";
					}
				}
				string nextNames = str;
				var strs = nextNames.CGSplitMember();
				if(strs.Count > 0) {
					strs.RemoveAt(0);
				}
				nextNames = string.Join(".", strs.ToArray());
				if(nextNames.StartsWith(".", StringComparison.Ordinal)) {
					nextNames = nextNames.Remove(0, 1);
				}
				str = ParseStartValue(mData).Add(".", !string.IsNullOrEmpty(nextNames)) + nextNames;
				if(str.IndexOf("get_", StringComparison.Ordinal) >= 0) {
					str = str.Replace("get_", "");
				} else if(str.IndexOf("set_", StringComparison.Ordinal) >= 0) {
					str = str.Replace("set_", "");
				}
				//if(str.Contains("Item")) {
				//	str = str.Replace(".Item", "[]");
				//}
				return str;
			}
			return null;
		}
		#endregion

		#region Parse Type
		/// <summary>
		/// Function to get correct code for type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string Type(Type type) {
			if(type == null)
				return null;
			if(generatorData.typesMap.TryGetValue(type, out var typeResult)) {
				return typeResult;
			}
			if(type is RuntimeType) {
				var runtimeType = type as RuntimeType;
				if(runtimeType is ArrayFakeType) {
					return Type((runtimeType as ArrayFakeType).GetElementType());
				} else if(runtimeType is GenericFakeType) {
					var genericType = runtimeType as GenericFakeType;
					return Type(genericType.target);
				}
				if(!generatePureScript) {
					RegisterUsingNamespace(RuntimeType.CompanyNamespace);
					if(runtimeType is RuntimeGraphType graphType) {
						if(graphType.target is IClassComponent) {
							return Type(typeof(RuntimeComponent));
						} else if(graphType.target is IClassAsset) {
							return Type(typeof(BaseRuntimeAsset));
						}
					} else if(runtimeType is RuntimeGraphInterface) {
						return Type(typeof(IRuntimeClass));
					}
					throw new InvalidOperationException();
				} else if(setting.fullTypeName) {
					return runtimeType.FullName;
				} else if(type is IFakeMember) {
					string result = DoParseType(type);
					generatorData.typesMap.Add(type, result);
					return result;
				}
				if(setting.nameSpace != type.Namespace) {
					RegisterUsingNamespace(type.Namespace);
				}
				return runtimeType.Name;
				//return runtimeType.FullName;
			}
			string str = DoParseType(type);
			generatorData.typesMap.Add(type, str);
			return str;
		}

		private static string DoParseType(Type type) {
			if(type.IsGenericType) {
				if(type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
					return string.Format("{0}?", Type(Nullable.GetUnderlyingType(type)));
				} else {
					string typeName = type.GetGenericTypeDefinition().FullName.Replace("+", ".").Split('`')[0];
					if(!setting.fullTypeName && setting.usingNamespace.Contains(type.Namespace)) {
						string result = typeName.Remove(0, type.Namespace.Length + 1);
						string firstName = result.Split('.')[0];
						bool flag = false;
						generatorData.ValidateTypes(type.Namespace, setting.usingNamespace, t => {
							if(t.IsGenericType && t.GetGenericArguments().Length == type.GetGenericArguments().Length && t.Name.Equals(firstName)) {
								flag = true;
								return true;
							}
							return false;
						});
						if(!flag) {
							typeName = result;
						}
					}
					return string.Format("{0}<{1}>", typeName, string.Join(", ", type.GetGenericArguments().Select(a => Type(a)).ToArray()));
				}
			} else if(type.IsValueType || type == typeof(string) || type == typeof(object)) {
				if(type == typeof(string)) {
					return "string";
				} else if(type == typeof(bool)) {
					return "bool";
				} else if(type == typeof(float)) {
					return "float";
				} else if(type == typeof(int)) {
					return "int";
				} else if(type == typeof(short)) {
					return "short";
				} else if(type == typeof(long)) {
					return "long";
				} else if(type == typeof(double)) {
					return "double";
				} else if(type == typeof(decimal)) {
					return "decimal";
				} else if(type == typeof(byte)) {
					return "byte";
				} else if(type == typeof(uint)) {
					return "uint";
				} else if(type == typeof(ulong)) {
					return "ulong";
				} else if(type == typeof(ushort)) {
					return "ushort";
				} else if(type == typeof(char)) {
					return "char";
				} else if(type == typeof(sbyte)) {
					return "sbyte";
				} else if(type == typeof(void)) {
					return "void";
				} else if(type == typeof(object)) {
					return "object";
				}
			} else if(type.IsArray) {
				return Type(type.GetElementType()) + "[]";
			}
			if(string.IsNullOrEmpty(type.FullName)) {
				return type.Name;
			}
			if(setting.fullTypeName) {
				return type.FullName.Replace("+", ".");
			}
			if(setting.usingNamespace.Contains(type.Namespace)) {
				string result = type.FullName.Replace("+", ".").Remove(0, type.Namespace.Length + 1);
				string firstName = result.Split('.')[0];
				generatorData.ValidateTypes(type.Namespace, setting.usingNamespace, t => {
					if(t.Name.Equals(firstName, StringComparison.Ordinal)) {
						result = type.FullName.Replace("+", ".");
						return true;
					}
					return false;
				});
				return result;
			}
			return type.FullName.Replace("+", ".");
		}

		/// <summary>
		/// Function to get correct code for type
		/// </summary>
		/// <param name="fullTypeName"></param>
		/// <returns></returns>
		public static string ParseType(string fullTypeName) {
			if(!string.IsNullOrEmpty(fullTypeName)) {
				Type type = TypeSerializer.Deserialize(fullTypeName, false);
				if(type != null) {
					return Type(type);
				} else {
					if(fullTypeName.Contains("`")) {
						string[] data1 = fullTypeName.Split(new char[] { '`' }, 2);
						if(data1.Length == 2) {
							int deepLevel = 0;
							int step = -1;
							int gLength = 0;
							bool skip = false;
							List<char> listChar = new List<char>();
							List<string> gTypes = new List<string>();
							for(int i = 0; i < data1[1].Length; i++) {
								char c = data1[1][i];
								if(skip) {
									//Continue skip until end of block
									if(c != ']') {
										continue;
									} else {
										skip = false;
									}
								}
								if(c == '[') {
									if(deepLevel == 0 && listChar.Count > 0) {
										gLength = int.Parse(string.Join("", new string[] { new string(listChar.ToArray()) }));
									}
									deepLevel++;
									if(deepLevel >= 3) {
										listChar.Add(c);
									} else {
										listChar.Clear();
									}
								} else if(c == ']') {
									if(deepLevel == 2) {
										//UnityEngine.Debug.Log(string.Join("", listChar.ToArray()));
										var cType = ParseType(string.Join("", new string[] { new string(listChar.ToArray()) }));
										if(cType == null)
											return null;
										//UnityEngine.Debug.Log(cType);
										gTypes.Add(cType);
										//Debug.Log(string.Join("", listChar.ToArray()).Split(',')[0]);
										listChar.Clear();
									} else if(deepLevel >= 3) {
										listChar.Add(c);
									} else if(deepLevel == 1) {//An array handling
										step++;
									}
									deepLevel--;
								} else {
									if(c == ',' && deepLevel == 2) {
										skip = true;
									} else {
										listChar.Add(c);
									}
								}
							}
							if(gLength == 0) {
								return fullTypeName;
							}
							//var dType = ParseType(data1[0] + "`" + gLength.ToString());
							//if(dType == null) {//Fallback for fail deserialization
							//	return fullTypeName;
							//}
							var result = data1[0] + "<" + string.Join(", ", gTypes) + ">";
							while(step > 0) {
								result += "[]";
								step--;
							}
							return result;
						}
					}
				}
			}
			return fullTypeName;
		}

		/// <summary>
		/// Function to get correct code for type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string Type(MemberData type) {
			if(!object.ReferenceEquals(type, null)) {
				if(type.isAssigned) {
					if(type.targetType == MemberData.TargetType.Type) {
						object o = type.Get();
						if(o is Type) {
							return Type(o as Type);
						}
						if(type.SerializedItems?.Length > 0) {
							string data = null;
							string[] gType;
							string[] pType;
							MemberDataUtility.GetItemName(SerializerUtility.Deserialize<MemberData.ItemData>(
										type.SerializedItems[0]),
										type.targetReference,
										out gType,
										out pType);
							if(gType.Length > 0) {
								data += String.Format("{0}", String.Join(", ", gType));
							}
							if(data == null) {
								if(type.StartSerializedType.type != null) {
									data = Type(type.StartSerializedType.type);
								} else {
									data = ParseType(type.StartSerializedType.typeName);
								}
							}
							return data;
						} else if(type.StartSerializedType.type != null) {
							return Type(type.StartSerializedType.type);
						} else {
							return ParseType(type.StartSerializedType.typeName);
						}
					} else if(type.targetType == MemberData.TargetType.Null) {
						return "null";
					} else if(type.targetType == MemberData.TargetType.uNodeGenericParameter) {
						if(type.SerializedItems.Length > 0) {
							string data = null;
							string[] gType;
							string[] pType;
							MemberDataUtility.GetItemName(SerializerUtility.Deserialize<MemberData.ItemData>(
										type.SerializedItems[0]),
										type.targetReference,
										out gType,
										out pType);
							if(gType.Length > 0) {
								data += String.Format("{0}", String.Join(", ", gType));
							}
							return data;
						}
						return type.name;
					} else if(type.targetType == MemberData.TargetType.uNodeParameter) {
						return Type(type.type);
					} else if(type.targetType == MemberData.TargetType.NodeField) {
						return Type(type.type);
					} else if(type.targetType == MemberData.TargetType.NodeFieldElement) {
						return Type(type.type);
					} else if(type.targetType == MemberData.TargetType.NodeOutputValue) {
						return Type(type.type);
					} else if(type.targetType == MemberData.TargetType.uNodeType) {
						return Type(type.type);
					} else {
						throw new System.Exception("Unsupported target type for parse to type");
					}
				} else {
					throw new System.Exception("Unassigned variable");
				}
			}
			return null;
		}
		#endregion

		#region Parse Value
		/// <summary>
		/// Are the member can be generate.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static bool CanParseValue(MemberData member) {
			if(!object.ReferenceEquals(member, null)) {
				if(member.isAssigned) {
					if(member.isStatic) {
						return true;
					} else if(graph != null && member.GetInstance() is uNodeRoot && (member.GetInstance() as uNodeRoot) == graph) {
						return true;
					} else if(graph != null && member.GetInstance() is INode<uNodeRoot> && (member.GetInstance() as INode<uNodeRoot>).GetOwner() == graph) {
						return true;
					} else if(member.targetType == MemberData.TargetType.Constructor) {
						return true;
					} else if(member.targetType == MemberData.TargetType.Method) {
						return true;
					} else if(member.targetType == MemberData.TargetType.Field) {
						return true;
					} else if(member.targetType == MemberData.TargetType.Property) {
						return true;
					} else if(member.targetType == MemberData.TargetType.Type) {
						return true;
					} else if(member.targetType == MemberData.TargetType.Null) {
						return true;
					} else if(member.targetType == MemberData.TargetType.None) {
						return true;
					} else if(member.targetType == MemberData.TargetType.ValueNode) {
						return true;
					} else if(member.targetType == MemberData.TargetType.SelfTarget) {
						return true;
					} else if(member.targetType == MemberData.TargetType.uNodeParameter) {
						return true;
					} else if(member.targetType == MemberData.TargetType.uNodeGenericParameter) {
						return true;
					} else if(member.targetType == MemberData.TargetType.FlowNode) {
						return true;
					} else if(member.targetType == MemberData.TargetType.uNodeFunction) {
						return true;
					} else if(member.targetType == MemberData.TargetType.FlowInput) {
						return true;
					}
				}
			}
			return false;
		}

		private static string GenerateGetRuntimeInstance(object instance, RuntimeType runtimeType) {
			RegisterUsingNamespace("MaxyGames");
			if(generatePureScript) {
				if(instance == null) {
					return DoGenerateInvokeCode(
						nameof(Extensions.ToRuntimeInstance),
						new string[0],
						new string[] { runtimeType.Name }
					);
				}
				return Value(instance).CGAccess(DoGenerateInvokeCode(
					nameof(Extensions.ToRuntimeInstance),
					new string[0],
					new string[] { runtimeType.Name })
				);
			} else {
				//Type type = typeof(IRuntimeClass);
				//if(runtimeType is RuntimeGraphType graphType) {
				//	if(graphType.target is IClassComponent) {
				//		type = typeof(RuntimeComponent);
				//	} else if(graphType.target is IClassAsset) {
				//		type = typeof(BaseRuntimeAsset);
				//	}
				//} else if(runtimeType is RuntimeGraphInterface) {
				//	type = typeof(IRuntimeClass);
				//}
				if(instance == null) {
					return DoGenerateInvokeCode(
						nameof(Extensions.ToRuntimeInstance),
						new string[] { runtimeType.Name.AddFirst(_runtimeInterfaceKey, runtimeType.IsInterface).CGValue() }
					);
				}
				return Value(instance).CGAccess(
					DoGenerateInvokeCode(
						nameof(Extensions.ToRuntimeInstance),
						new string[] { runtimeType.Name.AddFirst(_runtimeInterfaceKey, runtimeType.IsInterface).CGValue() }
					)
				);
			}
		}

		private static string GenerateGetGeneratedComponent(object instance, RuntimeType runtimeType) {
			if(!runtimeType.IsSubclassOf(typeof(Component))) {
				return GenerateGetRuntimeInstance(instance, runtimeType);
			}
			if(generatePureScript) {
				if(instance == null) {
					return DoGenerateInvokeCode(
						nameof(uNodeHelper.GetGeneratedComponent),
						new string[0],
						new string[] { runtimeType.Name }
					);
				}
				return Value(instance).CGAccess(
					DoGenerateInvokeCode(
						nameof(uNodeHelper.GetGeneratedComponent),
						new string[0],
						new string[] { runtimeType.Name }
					)
				);
			} else {
				RegisterUsingNamespace("MaxyGames");
				if(instance == null) {
					return DoGenerateInvokeCode(
						nameof(uNodeHelper.GetGeneratedComponent),
						new string[] { runtimeType.Name.AddFirst(_runtimeInterfaceKey, runtimeType.IsInterface).CGValue()
					});
				}
				return Value(instance).CGFlowInvoke(
					nameof(uNodeHelper.GetGeneratedComponent),
					runtimeType.Name.AddFirst(_runtimeInterfaceKey, runtimeType.IsInterface).CGValue()
				);
			}
		}

		private static string GenerateGetRuntimeVariable(RuntimeField field) {
			if(generatePureScript && !(field.DeclaringType is IFakeMember)) {
				return field.Name;
			} else {
				return DoGenerateInvokeCode(nameof(RuntimeComponent.GetVariable), new string[] { field.Name.CGValue() }, new Type[] { field.FieldType });
			}
		}

		private static string GenerateGetRuntimeProperty(RuntimeProperty property) {
			if(generatePureScript && !(property.DeclaringType is IFakeMember)) {
				return property.Name;
			} else {
				return DoGenerateInvokeCode(nameof(RuntimeComponent.GetProperty), new string[] { property.Name.CGValue() }, new Type[] { property.PropertyType });
			}
		}

		private static string GenerateInvokeRuntimeMethod(RuntimeMethod method, MemberData[] parameters, ref string enter, ref string exit, bool autoConvert = false) {
			var paramInfo = method.GetParameters();
			string data = string.Empty;
			if(paramInfo.Length > 0) {
				List<string> dataList = new List<string>();
				for(int index = 0; index < paramInfo.Length; index++) {
					MemberData p = parameters[index];
					string pData = null;
					if(paramInfo[index].IsOut) {
						pData += "out ";
					} else if(paramInfo[index].ParameterType.IsByRef) {
						pData += "ref ";
					}
					if(pData != null) {
						bool correct = true;
						if(p.type != null && p.type.IsValueType) {
							MemberInfo[] MI = p.GetMembers();
							if(MI != null && MI.Length > 1 && ReflectionUtils.GetMemberType(MI[MI.Length - 2]).IsValueType) {
								string varName = GenerateVariableName("tempVar");
								var pVal = Value((object)p);
								pData += varName + "." + pVal.Remove(pVal.IndexOf(ParseStartValue(p)), ParseStartValue(p).Length + 1).CGSplitMember().Last();
								if(pVal.LastIndexOf(".") >= 0) {
									pVal = pVal.Remove(pVal.LastIndexOf("."));
								}
								enter += Type(ReflectionUtils.GetMemberType(MI[MI.Length - 2])) + " " + varName + " = " + pVal + ";\n";
								exit += pVal + " = " + varName + ";";
								correct = false;
							}
						}
						if(correct) {
							if(debugScript && setting.debugValueNode) {
								setting.debugScript = false;
								pData += Value((object)p);
								setting.debugScript = true;
							} else {
								pData += Value((object)p);
							}
						}
					} else {
						pData += Value((object)p);
					}
					dataList.Add(pData);
				}
				for(int index = 0; index < dataList.Count; index++) {
					if(index != 0) {
						data += ", ";
					}
					data += dataList[index];
				}
			}
			if(generatePureScript) {
				return method.Name + "(" + data + ")";
			} else {
				RegisterUsingNamespace("MaxyGames");
				if(paramInfo.Length == 0) {
					var result = DoGenerateInvokeCode(
						nameof(RuntimeComponent.InvokeFunction),
						new string[] {
							method.Name.CGValue(),
							"null"
						});
					if(autoConvert) {
						result = result.CGConvert(method.ReturnType, true);
					}
					return result;
				} else {
					string paramValues = MakeArray(typeof(object), data);
					var paramTypes = paramInfo.Select(p => p.ParameterType).ToArray();
					var result = DoGenerateInvokeCode(
						nameof(RuntimeComponent.InvokeFunction),
						new string[] {
							method.Name.CGValue(),
							paramTypes.CGValue(),
							paramValues });
					if(autoConvert) {
						result = result.CGConvert(method.ReturnType, true);
					}
					return result;
				}
			}
		}

		/// <summary>
		/// Return full name of the member.
		/// Note: this function is still unfinished.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static string Nameof(MemberData member) {
			string name = ParseStartValue(member);
			var path = member.namePath;
			for(int i = 1; i < path.Length; i++) {
				name += "." + path[i];
			}
			return name;
		}

		/// <summary>
		/// Return start name of member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		private static string ParseStartValue(MemberData member) {
			if(!object.ReferenceEquals(member, null)) {
				if(member.isAssigned) {
					if(member.isStatic) {
						var type = member.startType;
						if(type is RuntimeGraphType graphType && graphType.IsSingleton) {
							if(generatePureScript) {
								return typeof(uNodeSingleton).CGInvoke(nameof(uNodeSingleton.GetInstance), new Type[] { type }, null);
							} else {
								return typeof(uNodeSingleton).CGInvoke(nameof(uNodeSingleton.GetGraphInstance), graphType.FullName.CGValue());
							}
						}
						return Type(type);
					} else if(member.targetType == MemberData.TargetType.uNodeParameter) {
						return member.startName;
					} else if(member.targetType == MemberData.TargetType.uNodeGenericParameter) {
						return "typeof(" + member.startName + ")";
					} else if(member.targetType == MemberData.TargetType.uNodeVariable) {
						VariableData ESV = member.GetVariable();
						if(ESV != null) {
							return GetVariableName(ESV);
						}
					} else if(member.targetType == MemberData.TargetType.uNodeProperty) {
						uNodeRoot UNR = member.startTarget as uNodeRoot;
						if(UNR != null) {
							return UNR.GetPropertyData(member.startName).Name;
						}
					} else if(member.targetType == MemberData.TargetType.uNodeLocalVariable) {
						RootObject RO = member.startTarget as RootObject;
						if(RO != null) {
							if(isInUngrouped) {
								VariableData v = RO.GetLocalVariableData(member.startName);
								return RegisterVariable(v);
							}
							return GetVariableName(RO.GetLocalVariableData(member.startName));
						}
					} else if(member.targetType == MemberData.TargetType.NodeField) {
						member.GetMembers();
						if(member.startTarget != null && member.fieldInfo != null) {
							return GetOutputName(member.startTarget, member.fieldInfo);
						}
					} else if(member.targetType == MemberData.TargetType.NodeFieldElement) {
						member.GetMembers();
						if(member.startTarget != null && member.fieldInfo != null) {
							return GetOutputName(member.startTarget, member.fieldInfo, int.Parse(member.startName.Split('#')[1]));
						}
					} else if(member.targetType == MemberData.TargetType.ValueNode) {
						if(debugScript && setting.debugValueNode) {
							Type targetType = member.startType;
							if(!generatorData.debugMemberMap.ContainsKey(member)) {
								int randomNum = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
								while(generatorData.debugMemberMap.Any((x) => x.Value.Key == randomNum)) {
									randomNum = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
								}
								generatorData.debugMemberMap.Add(member, new KeyValuePair<int, string>(randomNum,
									Debug(member, "debugValue").AddLineInFirst() +
									("return debugValue;").AddLineInFirst()
								));
							}
							return _debugGetValueCode + "(" +
								generatorData.debugMemberMap[member].Key + ", " +
								GenerateNode(member.GetTargetNode(), true) + ")";
						}
						return GenerateNode(member.GetTargetNode(), true);
					} else if(member.targetType == MemberData.TargetType.NodeOutputValue) {
						if(member.startTarget is IExtendedOutput node) {
							var result = node.GenerateOutputCode(member.startName);
							if(includeGraphInformation) {
								return WrapWithInformation(result, node);
							}
							return result;
						} else {
							throw new Exception("Null or invalid target instance");
						}
					} else if(member.GetInstance() is uNodeRoot root && root == graph) {
						if(member.startType is RuntimeType runtimeType) {
							var runtimeInstance = ReflectionUtils.GetActualTypeFromInstance(member.instance, true);
							if(runtimeType is RuntimeGraphType) {
								if(runtimeInstance != runtimeType) {
									if(runtimeInstance is FakeGraphType && runtimeType.GetHashCode() == runtimeInstance.GetHashCode()) {
										if(generatePureScript) {
											return Convert("this", runtimeType);
										} else {
											return "this";
										}
									}
									return "this".CGAccess(GenerateGetGeneratedComponent(null, runtimeType));
								}
							} else if(runtimeType is RuntimeGraphInterface) {
								if(runtimeType != runtimeInstance) {
									if(runtimeInstance is FakeGraphInterface && runtimeType.GetHashCode() == runtimeInstance.GetHashCode()) {
										if(generatePureScript) {
											return Convert("this", runtimeType);
										}
									} else if(!runtimeInstance.IsCastableTo(runtimeType)) {
										return "this".CGAccess(GenerateGetGeneratedComponent(null, runtimeType));
									}
									// if(runtimeInstance == typeof(GameObject) || runtimeInstance.IsCastableTo(typeof(Component))) {
									// } else {
									// 	throw new Exception($"Cannot convert type from: '{runtimeType.FullName}' to '{runtimeInstance.FullName}'");
									// }
								}
							} else {
								throw new Exception($"Unsupported RuntimeType: {runtimeType.FullName}");
							}
						}
						switch(member.targetType) {
							case MemberData.TargetType.Constructor:
							case MemberData.TargetType.Event:
							case MemberData.TargetType.Field:
							case MemberData.TargetType.Method:
							case MemberData.TargetType.Property:
								return "base";
							default:
								return "this";
						}
					} else if(member.IsTargetingVariable) {
						object instance = member.GetInstance();
						if(instance is IVariableSystem) {
							VariableData varData = (instance as IVariableSystem).GetVariableData(member.startName);
							if(varData != null) {
								return GetVariableName(varData);
							}
						} else if(instance is ILocalVariableSystem) {
							VariableData varData = (instance as ILocalVariableSystem).GetLocalVariableData(member.startName);
							if(varData != null) {
								return GetVariableName(varData);
							}
						}
					} else if(member.instance != null) {
						string result = Value(member.instance);
						if(member.startType is RuntimeType runtimeType) {
							if(runtimeType is RuntimeGraphType) {
								var runtimeInstance = ReflectionUtils.GetActualTypeFromInstance(member.instance, true);
								if(runtimeType != runtimeInstance) {
									if(runtimeInstance is FakeGraphType && runtimeType.GetHashCode() == runtimeInstance.GetHashCode()) {
										if(generatePureScript) {
											return Convert(result, runtimeType);
										} else {
											return result;
										}
									}
									return result.CGAccess(GenerateGetGeneratedComponent(null, runtimeType));
								}
							} else if(runtimeType is RuntimeGraphInterface) {
								var runtimeInstance = ReflectionUtils.GetActualTypeFromInstance(member.instance, true);
								if(runtimeType != runtimeInstance) {
									if(runtimeInstance is FakeGraphInterface && runtimeType.GetHashCode() == runtimeInstance.GetHashCode()) {
										if(generatePureScript) {
											return Convert("this", runtimeType);
										}
									} else if(!runtimeInstance.IsCastableTo(runtimeType)) {
										return result.CGAccess(GenerateGetGeneratedComponent(null, runtimeType));
									}
									// if(runtimeInstance == typeof(GameObject) || runtimeInstance.IsCastableTo(typeof(Component))) {
									// } else {
									// 	throw new Exception($"Cannot convert type from: '{runtimeType.FullName}' to '{runtimeInstance.FullName}'");
									// }
								}
							} else if(runtimeType is ArrayFakeType) {
								return result;
							} else if(runtimeType is GenericFakeType) {
								return result;
							} else {
								throw new Exception($"Unsupported RuntimeType: {runtimeType.FullName}");
							}
						}
						if(result == "this") {
							switch(member.targetType) {
								case MemberData.TargetType.Constructor:
								case MemberData.TargetType.Event:
								case MemberData.TargetType.Field:
								case MemberData.TargetType.Method:
								case MemberData.TargetType.Property:
									return "base";
							}
						}
						return result;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Function to generate correct code for ValueGetter
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="storeValue"></param>
		/// <returns></returns>
		public static string TryParseValue(MultipurposeMember variable, MemberData storeValue = null, Action<string, string> onEnterAndExit = null, bool autoConvert = false) {
			string resultCode = GetCorrectName(variable.target, variable.parameters, variable.initializer, onEnterAndExit, autoConvert);
			if(!string.IsNullOrEmpty(resultCode)) {
				if(storeValue != null && storeValue.isAssigned && variable.target.type != typeof(void) && CanParseValue(variable.target)) {
					if(resultCode.Contains(" = ")) {
						if(IsContainOperatorCode(variable.target.name)) {
							return GetCorrectName(storeValue) + " = (" + resultCode + ")";
						}
						return GetCorrectName(storeValue) + " = (" + resultCode + ")";
					}
					if(IsContainOperatorCode(variable.target.name)) {
						return GetCorrectName(storeValue) + " = " + resultCode;
					}
					return GetCorrectName(storeValue) + " = " + resultCode;
				}
				if(IsContainOperatorCode(variable.target.name)) {
					throw new System.Exception("unsupported generating operator code in the current context");
				}
				if((variable.target.targetType == MemberData.TargetType.uNodeGenericParameter ||
					variable.target.targetType == MemberData.TargetType.Type) && !resultCode.StartsWith("typeof(", StringComparison.Ordinal)) {
					resultCode = "typeof(" + resultCode + ")";
				}
				return resultCode;
			} else if(storeValue != null && CanParseValue(storeValue)) {
				return Set(storeValue, variable.target).RemoveLast();
			} else {
				return Value((object)variable.target);
			}
		}

		/// <summary>
		/// Function to generate correct code for MemberReflection
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public static string Value(MemberData member, MemberData[] parameters = null, bool setVariable = false, bool autoConvert = false) {
			if(!object.ReferenceEquals(member, null)) {
				if(member.isTargeted) {
					if(member.targetType == MemberData.TargetType.None || member.targetType == MemberData.TargetType.Type) {
						object o = member.Get();
						if(o is Type) {
							return "typeof(" + Type(o as Type) + ")";
						}
					} else if(member.targetType == MemberData.TargetType.Null) {
						return "null";
					} else if(member.targetType == MemberData.TargetType.uNodeGenericParameter) {
						return "typeof(" + member.name + ")";
					} else if(member.targetType == MemberData.TargetType.FlowNode || member.targetType == MemberData.TargetType.FlowInput) {
						throw new Exception("Flow target type need to generated from GenerateFlowCode()");
					} else if(member.targetType == MemberData.TargetType.Constructor) {
						return "new " + Type(member.type) + "()";
					} else if(member.targetType == MemberData.TargetType.Values) {
						return Value(member.Get());
					} else if(member.targetType == MemberData.TargetType.uNodeFunction) {
						string data = member.startName;
						string[] gType;
						string[] pType;
						MemberDataUtility.GetItemName(SerializerUtility.Deserialize<MemberData.ItemData>(
									member.SerializedItems[0]),
									member.targetReference,
									out gType,
									out pType);
						if(gType.Length > 0) {
							data += String.Format("<{0}>", String.Join(", ", gType));
						}
						data += "()";
					}
					if(member.isStatic) {
						return GetCorrectName(member, parameters, autoConvert: autoConvert);
						//string result = CSharpGenerator.ParseType(variable.startTypeName);
						//string[] str = GetCorrectName(variable).Split(new char[] { '.' });
						//for(int i = 0; i < str.Length; i++) {
						//	if(i == 0)
						//		continue;
						//	result += "." + str[i];
						//}
						//return result;
					} else if(member.targetType == MemberData.TargetType.ValueNode) {
						if(setVariable) {
							var tNode = member.GetTargetNode() as MultipurposeNode;
							if(tNode != null) {
								return Value(tNode.target.target, tNode.target.parameters, setVariable: setVariable);
							}
							if(debugScript && setting.debugValueNode) {
								setting.debugScript = false;
								var result = GetCorrectName(member, parameters, autoConvert: autoConvert);
								setting.debugScript = true;
								return result;
							}
						}
						return GetCorrectName(member, parameters, autoConvert: autoConvert);
					} else if(member.IsTargetingVariable) {
						if(!member.isDeepTarget) {
							return ParseStartValue(member);
						}
						VariableData variable = member.GetVariable();
						if(variable != null) {
							return GetCorrectName(member, parameters, autoConvert: autoConvert);
						}
						throw new Exception("Variable not found: " + member.startName);
					} else if(member.GetInstance() is UnityEngine.Object) {
						if(member.targetType == MemberData.TargetType.uNodeVariable ||
							member.targetType == MemberData.TargetType.uNodeProperty ||
							member.targetType == MemberData.TargetType.uNodeParameter ||
							member.targetType == MemberData.TargetType.uNodeLocalVariable ||
							member.targetType == MemberData.TargetType.uNodeGenericParameter ||
							member.targetType == MemberData.TargetType.uNodeGroupVariable ||
							member.targetType == MemberData.TargetType.Property ||
							member.targetType == MemberData.TargetType.uNodeFunction ||
							member.targetType == MemberData.TargetType.Field ||
							member.targetType == MemberData.TargetType.Constructor ||
							member.targetType == MemberData.TargetType.Method ||
							member.targetType == MemberData.TargetType.Event) {
							return GetCorrectName(member, parameters, autoConvert: autoConvert);
						}
						if(graph is uNodeRuntime ||
							graph is uNodeClass && typeof(UnityEngine.Object).IsAssignableFrom(graph.GetInheritType())) {
							UnityEngine.Object obj = member.GetInstance() as UnityEngine.Object;
							if(obj is Transform && graph.transform == obj) {
								return GetCorrectName(member, parameters, autoConvert: autoConvert);
							} else if(obj is GameObject && graph.gameObject == obj) {
								return GetCorrectName(member, parameters, autoConvert: autoConvert);
							}
						}
						if(graph == member.GetInstance() as UnityEngine.Object) {
							return Value(member.GetInstance(), autoConvert: autoConvert);
						}
						return GetCorrectName(member, parameters, autoConvert: autoConvert);
					} else if(member.instance == null) {
						return "null";
					} else {
						return GetCorrectName(member, parameters, autoConvert: autoConvert);
					}
					throw new Exception("Unsupported target reference: " + member.GetInstance().GetType());
				} else {
					throw new Exception("The value is un-assigned");
				}
			} else {
				throw new ArgumentNullException(nameof(member));
			}
		}

		/// <summary>
		/// Function to generate code for any object
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="initializer"></param>
		/// <returns></returns>
		public static string Value(object obj, ParameterValueData[] initializer = null, bool autoConvert = false, bool setVariable = false) {
			if(object.ReferenceEquals(obj, null))
				return "null";
			if(obj is Type) {
				return "typeof(" + Type(obj as Type) + ")";
			} else if(obj is MemberData) {
				return Value(obj as MemberData, setVariable: setVariable, autoConvert: autoConvert);
			} else if(obj is MultipurposeMember) {
				//return TryParseValue(obj as MemberInvoke);
				string header = null;
				string footer = null;
				var rezult = TryParseValue(obj as MultipurposeMember, null, (x, y) => {
					if(!string.IsNullOrEmpty(x)) {
						header += x.AddLineInEnd();
					}
					if(!string.IsNullOrEmpty(y)) {
						footer += y.AddLineInFirst();
					}
				}, autoConvert);
				return header + rezult + footer;
			} else if(obj is UnityEngine.Object) {
				UnityEngine.Object o = obj as UnityEngine.Object;
				if(o == graph) {
					return "this";
				}
				if(o != null) {
					if(generatorData.state.isStatic || generatorData.state.state == State.Classes) {
						return "null";
					}
					Type inherithType = graph.GetInheritType();
					if(graph is uNodeRuntime || graph is uNodeClass && (inherithType.IsCastableTo(typeof(GameObject)) || inherithType.IsCastableTo(typeof(Component)))) {
						if(o is GameObject) {
							GameObject g = o as GameObject;
							if(g == graph.gameObject) {
								return "this.gameObject";
							}
						} else if(o is Transform) {
							Transform g = o as Transform;
							if(g == graph.transform) {
								return "this.transform";
							}
						} else if(o is Component) {
							Component c = o as Component;
							if(c.gameObject == graph.gameObject) {
								return "this.GetComponent<" + Type(c.GetType()) + ">()";
							}
						}
					}
					if(!generatorData.unityVariableMap.ContainsKey(o)) {
						Type objType = o.GetType();
						if(o is uNodeAssetInstance asset) {
							if(generatePureScript) {
								objType = ReflectionUtils.GetRuntimeType(o);
							} else {
								objType = typeof(BaseRuntimeAsset);
							}
						}
						//else if(o is uNodeSpawner comp) {

						//}
						string varName = RegisterVariable(
							new VariableData("objectVariable", objType) { modifier = new FieldModifier() { Public = true } });
						generatorData.unityVariableMap.Add(o, varName);
						graph.graphData.unityObjects.Add(new GraphData.ObjectData() {
							name = varName,
							value = o,
						});
					}
					return generatorData.unityVariableMap[o];
				}
				return "null";
			} else if(obj is LayerMask) {
				return Value(((LayerMask)obj).value);
			} else if(obj is ObjectValueData) {
				return Value((obj as ObjectValueData).value);
			} else if(obj is ParameterValueData) {
				return Value((obj as ParameterValueData).value);
			} else if(obj is ConstructorValueData) {
				var val = obj as ConstructorValueData;
				Type t = val.type;
				if(t != null) {
					string pVal = null;
					if(val.parameters != null) {
						for(int i = 0; i < val.parameters.Length; i++) {
							string p = Value(val.parameters[i]);
							if(!string.IsNullOrEmpty(pVal)) {
								pVal += ", ";
							}
							pVal += p;
						}
					}
					string data = "new " + Type(t) + "(" + pVal + ")";
					if(val.initializer != null && val.initializer.Length > 0) {
						data += " { ";
						bool isFirst = true;
						if(t.HasImplementInterface(typeof(ICollection<>))) {
							foreach(var param in initializer) {
								if(!isFirst) {
									data += ", ";
								}
								data += Value(param.value);
								isFirst = false;
							}
						} else {
							foreach(var param in initializer) {
								if(!isFirst) {
									data += ", ";
								}
								data += param.name + " = " + Value(param.value);
								isFirst = false;
							}
						}
						data += " }";
					}
					return data;
				}
				return "null";
			} else if(obj is BaseValueData) {
				throw new System.Exception("Unsupported Value Data:" + obj.GetType());
			} else if(obj is VariableData) {
				return GetVariableName(obj as VariableData);
			} else if(obj is StringWrapper) {
				return (obj as StringWrapper).value;
			}
			Type type = obj.GetType();
			if(type.IsValueType || type == typeof(string)) {
				if(obj is string) {
					return "\"" + StringHelper.StringLiteral(obj.ToString()) + "\"";
				} else if(obj is float) {
					return obj.ToString().Replace(',', '.') + "F";
				} else if(obj is int) {
					return obj.ToString();
				} else if(obj is uint) {
					return obj.ToString() + "U";
				} else if(obj is short) {
					return "(" + Type(typeof(short)) + ")" + obj.ToString();
				} else if(obj is ushort) {
					return "(" + Type(typeof(ushort)) + ")" + obj.ToString();
				} else if(obj is long) {
					return obj.ToString() + "L";
				} else if(obj is ulong) {
					return obj.ToString() + "UL";
				} else if(obj is byte) {
					return "(" + Type(typeof(byte)) + ")" + obj.ToString();
				} else if(obj is sbyte) {
					return "(" + Type(typeof(sbyte)) + ")" + obj.ToString();
				} else if(obj is double) {
					return obj.ToString().Replace(',', '.') + "D";
				} else if(obj is decimal) {
					return obj.ToString().Replace(',', '.') + "M";
				} else if(obj is bool) {
					return obj.ToString().ToLower();
				} else if(obj is char) {
					return "'" + obj.ToString() + "'";
				} else if(obj is Enum) {
					return Type(obj.GetType()) + "." + obj.ToString();
				} else if(obj is Vector2) {
					var val = (Vector2)obj;
					if(initializer == null || initializer.Length == 0) {
						if(val == Vector2.zero) {
							return Type(typeof(Vector2)) + ".zero";
						}
						if(val == Vector2.up) {
							return Type(typeof(Vector2)) + ".up";
						}
						if(val == Vector2.down) {
							return Type(typeof(Vector2)) + ".down";
						}
						if(val == Vector2.left) {
							return Type(typeof(Vector2)) + ".left";
						}
						if(val == Vector2.right) {
							return Type(typeof(Vector2)) + ".right";
						}
						if(val == Vector2.one) {
							return Type(typeof(Vector2)) + ".one";
						}
						return "new " + Type(typeof(Vector2)) + "(" + val.x + "f, " + val.y + "f)";
					}
				} else if(obj is Vector3) {
					if(initializer == null || initializer.Length == 0) {
						var val = (Vector3)obj;
						if(val == Vector3.zero) {
							return Type(typeof(Vector3)) + ".zero";
						} else if(val == Vector3.up) {
							return Type(typeof(Vector3)) + ".up";
						} else if(val == Vector3.down) {
							return Type(typeof(Vector3)) + ".down";
						} else if(val == Vector3.left) {
							return Type(typeof(Vector3)) + ".left";
						} else if(val == Vector3.right) {
							return Type(typeof(Vector3)) + ".right";
						} else if(val == Vector3.one) {
							return Type(typeof(Vector3)) + ".one";
						} else if(val == Vector3.forward) {
							return Type(typeof(Vector3)) + ".forward";
						} else if(val == Vector3.back) {
							return Type(typeof(Vector3)) + ".back";
						}
						return "new " + Type(typeof(Vector3)) + "(" + val.x + "f, " + val.y + "f, " + val.z + "f)";
					}
				} else if(obj is Vector4) {
					if(initializer == null || initializer.Length == 0) {
						var val = (Vector4)obj;
						if(val == Vector4.zero) {
							return Type(typeof(Vector4)) + ".zero";
						} else if(val == Vector4.one) {
							return Type(typeof(Vector4)) + ".one";
						}
					}
				} else if(obj is Color) {
					if(initializer == null || initializer.Length == 0) {
						var val = (Color)obj;
						if(val == Color.white) {
							return Type(typeof(Color)) + ".white";
						} else if(val == Color.black) {
							return Type(typeof(Color)) + ".black";
						} else if(val == Color.blue) {
							return Type(typeof(Color)) + ".blue";
						} else if(val == Color.clear) {
							return Type(typeof(Color)) + ".clear";
						} else if(val == Color.cyan) {
							return Type(typeof(Color)) + ".cyan";
						} else if(val == Color.gray) {
							return Type(typeof(Color)) + ".gray";
						} else if(val == Color.green) {
							return Type(typeof(Color)) + ".green";
						} else if(val == Color.magenta) {
							return Type(typeof(Color)) + ".magenta";
						} else if(val == Color.red) {
							return Type(typeof(Color)) + ".red";
						} else if(val == Color.yellow) {
							return Type(typeof(Color)) + ".yellow";
						}
					}
				} else if(obj is Rect) {
					if(initializer == null || initializer.Length == 0) {
						var val = (Rect)obj;
						if(val == Rect.zero) {
							return Type(typeof(Rect)) + ".zero";
						} else {
							return New(typeof(Rect), Value(val.x), Value(val.y), Value(val.width), Value(val.height));
						}
					}
				}
			} else if(type.IsGenericType) {
				string elementObject = "";
				if(obj is IDictionary) {
					IDictionary dic = obj as IDictionary;
					if(dic != null && dic.Count > 0) {
						elementObject = " { ";
						int index = 0;
						foreach(DictionaryEntry o in dic) {
							if(index != 0) {
								elementObject += ", ";
							}
							elementObject += "{ " + Value(o.Key) + ", " + Value(o.Value) + " }";
							index++;
						}
						elementObject += " }";
					}
				} else if(obj is ICollection) {
					ICollection col = obj as ICollection;
					if(col != null && col.Count > 0) {
						elementObject = " { ";
						int index = 0;
						foreach(object o in col) {
							if(index != 0) {
								elementObject += ", ";
							}
							if(o is DictionaryEntry) {
								elementObject += "{ " + Value(((DictionaryEntry)o).Key) + ", " + Value(((DictionaryEntry)o).Value) + " }";
							} else {
								elementObject += Value(o);
							}
							index++;
						}
						if(initializer != null && initializer.Length > 0) {
							foreach(var param in initializer) {
								if(index != 0) {
									elementObject += ", ";
								}
								elementObject += Value(param.value);
								index++;
							}
						}
						elementObject += " }";
					}
				} else {
					IEnumerable val = obj as IEnumerable;
					if(val != null) {
						elementObject = " { ";
						int index = 0;
						foreach(object o in val) {
							if(index != 0) {
								elementObject += ", ";
							}
							if(o is DictionaryEntry) {
								elementObject += "{ " + Value(((DictionaryEntry)o).Key) + ", " + Value(((DictionaryEntry)o).Value) + " }";
							} else {
								elementObject += Value(o);
							}
							index++;
						}
						if(initializer != null && initializer.Length > 0) {
							foreach(var param in initializer) {
								if(index != 0) {
									elementObject += ", ";
								}
								elementObject += Value(param.value);
								index++;
							}
						}
						elementObject += " }";
						if(index == 0) {
							return "new " + Type(type) + "()";
						}
					}
				}
				return "new " + Type(type) + "()" + elementObject;
			} else if(type.IsArray) {
				string elementObject = "[0]";
				Array array = obj as Array;
				if(array != null && array.Length > 0) {
					int index = 0;
					elementObject = "[" + //array.Length + 
						"] {";
					foreach(object o in array) {
						if(index != 0) {
							elementObject += ",";
						}
						elementObject += " " + Value(o);
						index++;
					}
					if(initializer != null && initializer.Length > 0) {
						foreach(var param in initializer) {
							if(index != 0) {
								elementObject += ", ";
							}
							elementObject += Value(param.value);
							index++;
						}
					}
					elementObject += " }";
				}
				return "new " + Type(type.GetElementType()) + elementObject;
			} else if(obj is IEnumerable) {
				string elementObject = "";
				IEnumerable val = obj as IEnumerable;
				if(val != null) {
					elementObject = " { ";
					int index = 0;
					foreach(object o in val) {
						if(index != 0) {
							elementObject += ", ";
						}
						elementObject += Value(o);
						index++;
					}
					if(initializer != null && initializer.Length > 0) {
						foreach(var param in initializer) {
							if(index != 0) {
								elementObject += ", ";
							}
							elementObject += Value(param.value);
							index++;
						}
					}
					elementObject += " }";
					if(index == 0) {
						return "new " + Type(type) + "()";
					}
				}
				return "new " + Type(type) + "()" + elementObject;
			}
			if(ReflectionUtils.IsNullOrDefault(obj, type)) {
				if(!type.IsValueType && obj == null) {
					return "null";
				} else {
					string data = "new " + Type(type) + "()";
					if(initializer != null && initializer.Length > 0) {
						data += " { ";
						bool isFirst = true;
						foreach(var param in initializer) {
							if(!isFirst) {
								data += ", ";
							}
							data += param.name + " = " + Value(param.value);
							isFirst = false;

						}
						data += " }";
					}
					return data;
				}
			} else if(obj is AnimationCurve) {
				var val = (AnimationCurve)obj;
				string data = "new " + Type(type) + "(";
				if(val.keys.Length > 0) {
					for(int i = 0; i < val.keys.Length; i++) {
						var key = val.keys[i];
						if(i != 0) {
							data += ", ";
						}
						data += Value(key);
					}
				}
				data = data + ")";
				if(initializer != null && initializer.Length > 0) {
					data += " { ";
					bool isFirst = true;
					foreach(var param in initializer) {
						if(!isFirst) {
							data += ", ";
						}
						data += param.name + " = " + Value(param.value);
						isFirst = false;

					}
					data += " }";
				}
				return data;
			} else if(type.IsValueType) {
				object clone = ReflectionUtils.CreateInstance(type);
				Dictionary<string, object> objMap = new Dictionary<string, object>();
				FieldInfo[] fields = ReflectionUtils.GetFields(obj);
				foreach(FieldInfo field in fields) {
					object fieldObj = field.GetValueOptimized(obj);
					if(field.FieldType.IsValueType) {
						object cloneObj = field.GetValueOptimized(clone);
						if(fieldObj.Equals(cloneObj))
							continue;
					}
					objMap.Add(field.Name, fieldObj);
				}
				PropertyInfo[] properties = ReflectionUtils.GetProperties(obj);
				foreach(PropertyInfo property in properties) {
					if(property.GetIndexParameters().Any()) {
						continue;
					}
					if(property.CanRead && property.CanWrite) {
						try {
							object fieldObj = property.GetValueOptimized(obj);
							if(property.PropertyType.IsValueType) {
								object cloneObj = property.GetValueOptimized(clone);
								if(fieldObj.Equals(cloneObj))
									continue;
							}
							objMap.Add(property.Name, fieldObj);
						}
						catch { }
					}
				}
				string data = "new " + Type(type) + "()";
				if(objMap.Count > 0) {
					data += " { ";
					bool isFirst = true;
					if(initializer != null && initializer.Length > 0) {
						foreach(var param in initializer) {
							if(!isFirst) {
								data += ", ";
							}
							data += param.name + " = " + Value(param.value);
							if(objMap.ContainsKey(param.name)) {
								objMap.Remove(param.name);
							}
							isFirst = false;

						}
					}
					foreach(KeyValuePair<string, object> pair in objMap) {
						if(!isFirst) {
							data += ", ";
						}
						data += pair.Key + " = " + Value(pair.Value);
						isFirst = false;
					}
					data += " }";
				}
				return data;
			} else {
				ConstructorInfo[] ctor = type.GetConstructors();
				foreach(ConstructorInfo info in ctor) {
					if(info.GetParameters().Length == 0) {
						object clone = ReflectionUtils.CreateInstance(type);
						Dictionary<string, object> objMap = new Dictionary<string, object>();
						FieldInfo[] fields = ReflectionUtils.GetFields(obj);
						foreach(FieldInfo field in fields) {
							object fieldObj = field.GetValueOptimized(obj);
							if(field.FieldType.IsValueType) {
								object cloneObj = field.GetValueOptimized(clone);
								if(fieldObj.Equals(cloneObj))
									continue;
							}
							objMap.Add(field.Name, fieldObj);
						}
						PropertyInfo[] properties = ReflectionUtils.GetProperties(obj);
						foreach(PropertyInfo property in properties) {
							if(property.CanRead && property.CanWrite) {
								object fieldObj = property.GetValueOptimized(obj);
								if(property.PropertyType.IsValueType) {
									object cloneObj = property.GetValueOptimized(clone);
									if(fieldObj.Equals(cloneObj))
										continue;
								}
								objMap.Add(property.Name, fieldObj);
							}
						}
						string data = "new " + Type(type) + "()";
						if(objMap.Count > 0) {
							data += " { ";
							bool isFirst = true;
							if(initializer != null && initializer.Length > 0) {
								foreach(var param in initializer) {
									if(!isFirst) {
										data += ", ";
									}
									data += param.name + " = " + Value(param.value);
									if(objMap.ContainsKey(param.name)) {
										objMap.Remove(param.name);
									}
									isFirst = false;

								}
							}
							foreach(KeyValuePair<string, object> pair in objMap) {
								if(!isFirst) {
									data += ", ";
								}
								data += pair.Key + " = " + Value(pair.Value);
								isFirst = false;
							}
							data += " }";
						}
						return data;
					}
				}
			}
			return obj.ToString();
		}

		/// <summary>
		/// Parse Constructor initializer.
		/// </summary>
		/// <param name="initializer"></param>
		/// <returns></returns>
		private static string ParseConstructorInitializer(ValueData initializer) {
			string ctorInit = null;
			if(initializer != null && initializer.Value as ConstructorValueData != null) {
				ConstructorValueData ctor = initializer.Value as ConstructorValueData;
				if(ctor.initializer != null && ctor.initializer.Length > 0) {
					ctorInit += " { ";
					bool isFirst = true;
					if(ctor.type.HasImplementInterface(typeof(ICollection<>))) {
						foreach(var param in ctor.initializer) {
							if(!isFirst) {
								ctorInit += ", ";
							}
							ctorInit += Value(param.value);
							isFirst = false;

						}
					} else {
						foreach(var param in ctor.initializer) {
							if(!isFirst) {
								ctorInit += ", ";
							}
							ctorInit += param.name + " = " + Value(param.value);
							isFirst = false;

						}
					}
					ctorInit += " }";
				}
			}
			return ctorInit;
		}

		/// <summary>
		/// Function for generate code for attribute data.
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		private static string TryParseAttribute(AData attribute) {
			if(attribute == null)
				return null;
			string parameters = null;
			if(attribute.attributeParameters != null) {
				foreach(string str in attribute.attributeParameters) {
					if(string.IsNullOrEmpty(str))
						continue;
					if(!string.IsNullOrEmpty(parameters)) {
						parameters += ", ";
					}
					parameters += str;
				}
			}
			string namedParameters = null;
			if(attribute.namedParameters != null && attribute.namedParameters.Count > 0) {
				foreach(var pain in attribute.namedParameters) {
					if(string.IsNullOrEmpty(pain.Value))
						continue;
					if(!string.IsNullOrEmpty(namedParameters)) {
						namedParameters += ", ";
					}
					namedParameters += pain.Key + " = " + pain.Value;
				}
			}
			string attName = Type(attribute.attributeType);
			if(attName.EndsWith("Attribute")) {
				attName = attName.RemoveLast(9);
			}
			string result;
			if(string.IsNullOrEmpty(parameters)) {
				if(string.IsNullOrEmpty(namedParameters)) {
					result = "[" + attName + "]";
				} else {
					result = "[" + attName + "(" + namedParameters + ")]";
				}
			} else {
				result = "[" + attName + "(" + parameters + namedParameters.AddFirst(", ", !string.IsNullOrEmpty(parameters)) + ")]";
			}
			return result;
		}

		/// <summary>
		/// Function for Convert AttributeData to AData
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		private static AData TryParseAttributeData(AttributeData attribute) {
			if(attribute != null && attribute.type != null) {
				AData data = new AData();
				if(attribute.value != null && attribute.value.type != null) {
					data.attributeType = attribute.value.type;
					if(attribute.value.Value != null && attribute.value.Value is ConstructorValueData) {
						ConstructorValueData ctor = attribute.value.Value as ConstructorValueData;
						Type t = ctor.type;
						if(t != null) {
							if(ctor.parameters != null) {
								data.attributeParameters = new string[ctor.parameters.Length];
								for(int i = 0; i < ctor.parameters.Length; i++) {
									data.attributeParameters[i] = Value(ctor.parameters[i]);
								}
							}
							data.attributeType = t;
							if(ctor.initializer != null && ctor.initializer.Length > 0) {
								if(data.namedParameters == null) {
									data.namedParameters = new Dictionary<string, string>();
								}
								foreach(var param in ctor.initializer) {
									data.namedParameters.Add(param.name, Value(param.value));
								}
							}
						}
					} else {
						data.attributeType = attribute.type.Get<Type>();
					}
				} else {
					data.attributeType = attribute.type.Get<Type>();
				}
				return data;
			}
			return null;
		}
		#endregion

		#region Variable Functions
		/// <summary>
		/// Get variable data of variable.
		/// </summary>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static VData GetVariable(VariableData variable) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef == variable) {
					return vdata;
				}
			}
			throw new System.Exception("no variable data found");
		}

		#region AddVariable
		/// <summary>
		/// Register new using namespaces
		/// </summary>
		/// <param name="nameSpace"></param>
		/// <returns></returns>
		public static bool RegisterUsingNamespace(string nameSpace) {
			return setting.usingNamespace.Add(nameSpace);
		}

		/// <summary>
		/// Register new script header like define symbol, pragma symbol or script copyright
		/// </summary>
		/// <param name="contents"></param>
		/// <returns></returns>
		public static bool RegisterScriptHeader(string contents) {
			return setting.scriptHeaders.Add(contents);
		}

		/// <summary>
		/// Register a new instance variable that's declared within the class
		/// </summary>
		/// <param name="from"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static string RegisterInstanceVariable(object from, string fieldName) {
			return RegisterInstanceVariable(from, from.GetType().GetFieldCached(fieldName));
		}

		/// <summary>
		/// Register a new instance variable that's declared within the class
		/// </summary>
		/// <param name="from"></param>
		/// <param name="fieldName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string RegisterInstanceVariable(object from, string fieldName, Type type) {
			return RegisterInstanceVariable(from, from.GetType().GetFieldCached(fieldName), type);
		}

		/// <summary>
		/// Register a new instance variable that's declared within the class
		/// </summary>
		/// <param name="from"></param>
		/// <param name="fieldName"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string RegisterInstanceVariable(object from, string fieldName, int index, Type type) {
			return RegisterInstanceVariable(from, from.GetType().GetFieldCached(fieldName), index, type);
		}

		private static string RegisterInstanceVariable(object from, FieldInfo field) {
			return RegisterInstanceVariable(from, field, field.FieldType);
		}

		private static string RegisterInstanceVariable(object from, FieldInfo field, Type type) {
			if(from == null) {
				throw new ArgumentNullException(nameof(from));
			}
			if(field == null) {
				throw new ArgumentNullException(nameof(field));
			}
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef as object[] != null && (vdata.variableRef as object[])[0] == from && (vdata.variableRef as object[])[1] as FieldInfo == field) {
					vdata.isInstance = true;
					if(generatorData.state.isStatic) {
						vdata.modifier.Static = true;
					}
					return vdata.name;
				}
			}
			if(type == null) {
				throw new ArgumentNullException(nameof(type));
			}
			string name = GetOutputName(from, field);
			generatorData.AddVariable(new VData(from, field, type, name) {
				isInstance = true,
				modifier = new FieldModifier() {
					Private = true,
					Public = false,
					Static = generatorData.state.isStatic,
				}
			});
			return name;
		}

		private static string RegisterInstanceVariable(object from, FieldInfo field, int index, Type type) {
			if(from == null) {
				throw new ArgumentNullException(nameof(from));
			}
			if(field == null) {
				throw new ArgumentNullException(nameof(field));
			}
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef as object[] != null && (vdata.variableRef as object[])[0] == from &&
					(vdata.variableRef as object[])[1] as FieldInfo == field && (vdata.variableRef as object[])[2] is int &&
						(int)(vdata.variableRef as object[])[2] == index) {
					vdata.isInstance = true;
					if(generatorData.state.isStatic) {
						vdata.modifier.Static = true;
					}
					return vdata.name;
				}
			}
			if(type == null) {
				throw new ArgumentNullException(nameof(type));
			}
			string name = GetOutputName(from, field, index);
			generatorData.AddVariable(new VData(from, field, index, type, name) {
				isInstance = true,
				modifier = new FieldModifier() {
					Private = true,
					Public = false,
					Static = generatorData.state.isStatic,
				}
			});
			return name;
		}

		public static string RegisterVariable(VariableData variable, bool isInstance = true, bool autoCorrection = true) {
			return M_RegisterVariable(variable, isInstance, autoCorrection).name;
		}

		private static VData M_RegisterVariable(VariableData variable, bool isInstance = true, bool autoCorrection = true) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.variableRef == variable) {
					if(isInstance)
						vdata.isInstance = true;
					return vdata;
				}
			}
			var result = new VData(variable, false) {
				name = autoCorrection ? GenerateVariableName(variable.Name) : variable.Name,
				isInstance = isInstance,
				modifier = variable.modifier,
				variableRef = variable
			};
			generatorData.AddVariable(result);
			return result;
		}

		public static void RegisterVariableAlias(string variableName, VariableData variable, object owner) {
			generatorData.AddVariableAlias(variableName, variable, owner);
		}

		public static VariableData GetVariableAlias(string variableName, object owner) {
			return generatorData.GetVariableAlias(variableName, owner);
		}

		/// <summary>
		/// Register a new local variable that's not auto declared within the class.
		/// Note: you need to declare the variable manually.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string RegisterLocalVariable(string name, Type type, object value) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.name == name) {
					vdata.isInstance = false;
					return name;
				}
			}
			generatorData.AddVariable(
				new VData(
					new VariableData(name, type, value) {
						modifier = new FieldModifier() {
							Public = false,
							Private = true,
						}
					}
				) {
					name = name,
					isInstance = false
				});
			return name;
		}

		/// <summary>
		/// Register a new private variable that's declared within the class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string RegisterPrivateVariable(string name, Type type, object value = null) {
			foreach(VData vdata in generatorData.GetVariables()) {
				if(vdata.name == name) {
					vdata.isInstance = true;
					if(generatorData.state.isStatic) {
						vdata.modifier.Static = true;
					}
					return name;
				}
			}
			generatorData.AddVariable(
				new VData(
					new VariableData(name, type, value) {
						modifier = new FieldModifier() {
							Public = false,
							Private = true,
							Static = generatorData.state.isStatic,
						}
					}
				) {
					name = name,
					isInstance = true
				});
			;
			return name;
		}

		/// <summary>
		/// Register node to the generators.
		/// Note: call only from RegisterPort
		/// </summary>
		/// <param name="nodeComponent"></param>
		public static void RegisterNode(NodeComponent nodeComponent) {
			if(!generatorData.allNode.Contains(nodeComponent)) {
				generatorData.allNode.Add(nodeComponent);
			}
		}

		/// <summary>
		/// Register pre node generation process
		/// Note: call only from RegisterPort
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="action"></param>
		public static void RegisterNodeSetup(NodeComponent owner, Action action) {
			Action act;
			generatorData.initActionForNodes.TryGetValue(owner, out act);
			act += action;
			generatorData.initActionForNodes[owner] = act;
		}
		#endregion

		#region Declare Variables
		/// <summary>
		/// Generate variable declaration for variable.
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="alwaysHaveValue"></param>
		/// <param name="ignoreModifier"></param>
		/// <returns></returns>
		public static string DeclareVariable(VariableData variable, bool alwaysHaveValue = true, bool ignoreModifier = false) {
			string varName = GetVariableName(variable);
			if(!string.IsNullOrEmpty(varName)) {
				if(ignoreModifier) {
					return DeclareVariable(varName, variable.type, variable.variable, true, null, alwaysHaveValue);
				}
				return DeclareVariable(varName, variable.type, variable.variable, true, variable.modifier, alwaysHaveValue);
			}
			return null;
		}

		/// <summary>
		/// Generate variable declaration for variable.
		/// </summary>
		/// <param name="variable"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string DeclareVariable(VariableData variable, object value, bool parseValue = true) {
			string varName = GetVariableName(variable);
			if(!string.IsNullOrEmpty(varName)) {
				return DeclareVariable(varName, variable.type, value, parseValue);
			}
			return null;
		}

		/// <summary>
		/// Generate variable declaration for variableName.
		/// </summary>
		/// <param name="variableName"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <param name="alwaysHaveValue"></param>
		/// <param name="modifier"></param>
		/// <returns></returns>
		public static string DeclareVariable(string variableName,
			Type type,
			object value = null,
			bool parseValue = true,
			FieldModifier modifier = null,
			bool alwaysHaveValue = true) {
			string M = null;
			if(modifier != null) {
				M = modifier.GenerateCode();
			}
			if(!alwaysHaveValue) {
				if(object.ReferenceEquals(value, null) && type.IsValueType) {
					return M + Type(type) + " " + variableName + ";";
				}
				return M + Type(type) + " " + variableName + " = " + (parseValue ? Value(value) : value != null ? value.ToString() : "null") + ";";
			}
			if(object.ReferenceEquals(value, null) && ReflectionUtils.CanCreateInstance(type)) {
				value = ReflectionUtils.CreateInstance(type);
			}
			return M + Type(type) + " " + variableName + " = " + (parseValue ? Value(value) : value != null ? value.ToString() : "null") + ";";
		}

		/// <summary>
		/// Generate variable declaration for variableName.
		/// </summary>
		/// <param name="variableName"></param>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <param name="alwaysHaveValue"></param>
		/// <param name="modifier"></param>
		/// <returns></returns>
		public static string DeclareVariable(string variableName,
			MemberData type,
			object value = null,
			bool parseValue = true,
			FieldModifier modifier = null,
			bool alwaysHaveValue = true) {
			string M = null;
			if(modifier != null) {
				M = modifier.GenerateCode();
			}
			var t = type.Get<Type>();
			if(!alwaysHaveValue) {
				if(object.ReferenceEquals(value, null) && t.IsValueType) {
					return M + Type(type) + " " + variableName + ";";
				}
				return M + Type(type) + " " + variableName + " = " + (parseValue ? Value(value) : value != null ? value.ToString() : "null") + ";";
			}
			if(object.ReferenceEquals(value, null) && ReflectionUtils.CanCreateInstance(t)) {
				value = ReflectionUtils.CreateInstance(t);
			}
			return M + Type(type) + " " + variableName + " = " + (parseValue ? Value(value) : value != null ? value.ToString() : "null") + ";";
		}

		public static string DeclareVariable(string variableName,
			Type type,
			string value,
			FieldModifier modifier = null) {
			string M = null;
			if(modifier != null) {
				M = modifier.GenerateCode();
			}
			if(string.IsNullOrEmpty(value)) {
				return M + Type(type) + " " + variableName + ";";
			}
			return M + Type(type) + " " + variableName + " = " + value + ";";
		}

		public static string DeclareVariable(string variableName,
			MemberData type,
			string value,
			FieldModifier modifier = null) {
			string M = null;
			if(modifier != null) {
				M = modifier.GenerateCode();
			}
			if(string.IsNullOrEmpty(value)) {
				return M + Type(type) + " " + variableName + ";";
			}
			return M + Type(type) + " " + variableName + " = " + value + ";";
		}
		#endregion

		#endregion

		#region InsertMethod
		public static void InsertCodeToFunction(string functionName, Type returnType, string code, int priority = 0) {
			var mData = generatorData.GetMethodData(functionName);
			if(mData == null) {
				mData = generatorData.AddMethod(functionName, Type(returnType), new string[0]);
			}
			mData.AddCode(code, priority);
		}

		public static void InsertCodeToFunction(string functionName, Type returnType, Type[] parameterTypes, string code, int priority = 0) {
			var mData = generatorData.GetMethodData(functionName, parameterTypes.Select((item) => Type(item)).ToArray());
			if(mData == null) {
				mData = generatorData.AddMethod(functionName, Type(returnType), parameterTypes.Select((item) => Type(item)).ToArray());
			}
			mData.AddCode(code, priority);
		}
		#endregion
	}
}