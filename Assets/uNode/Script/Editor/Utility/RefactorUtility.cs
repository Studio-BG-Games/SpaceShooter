using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public class RefactorUtility {
		public static void RefactorVariable(VariableData variable, string name, UnityEngine.Object owner) {
			bool isLocal = false;
			uNodeRoot graph = owner as uNodeRoot;
			if (graph == null) {
				INode<uNodeRoot> node = owner as INode<uNodeRoot>;
				if (node != null) {
					graph = node.GetOwner();
					isLocal = true;
				}
			}
			if (graph != null) {
				HashSet<GameObject> referencedGraphs = new HashSet<GameObject>();
				if (!isLocal) {
					RuntimeField field = null;
					if (graph is IIndependentGraph) {
						if (GraphUtility.IsTempGraphObject(graph.gameObject)) {
							var prefab = uNodeEditorUtility.GetGameObjectSource(graph.gameObject, null);
							if (prefab != null) {
								var oriGraph = prefab.GetComponent<uNodeRoot>();
								if (oriGraph != null) {
									field = ReflectionUtils.GetRuntimeType(oriGraph).GetField(variable.Name) as RuntimeField;
								}
							}
						} else {
							field = ReflectionUtils.GetRuntimeType(graph).GetField(variable.Name) as RuntimeField;
						}
					}
					FieldInfo nativeMember = null;
					if(graph.GeneratedTypeName.ToType(false) != null) {
						var type = graph.GeneratedTypeName.ToType(false);
						nativeMember = type.GetField(variable.Name, MemberData.flags);
					}
					var graphPrefabs = GraphUtility.FindGraphPrefabsWithComponent<uNodeRoot>();
					foreach (var prefab in graphPrefabs) {
						var gameObject = prefab;
						GameObject prefabContent = null;
						if (GraphUtility.HasTempGraphObject(prefab)) {
							gameObject = GraphUtility.GetTempGraphObject(prefab);
						} else if(uNodeEditorUtility.IsPrefab(prefab)) {
							prefabContent = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
							gameObject = prefabContent;
						}
						var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
						bool hasUndo = false;
						Func<object, bool> scriptValidation = (obj) => {
							MemberData member = obj as MemberData;
							if (member != null) {
								var members = member.GetMembers(false);
								if (members != null) {
									for (int i = 0; i < members.Length; i++) {
										var m = members[i];
										if (member.namePath.Length > i + 1) {
											if (m == field || m == nativeMember) {
												if (!hasUndo && prefabContent == null) {
													uNodeEditorUtility.RegisterFullHierarchyUndo(gameObject, "Rename Variable: " + variable.Name);
													hasUndo = true;
												}
												var path = member.namePath;
												path[i + 1] = name;
												member.name = string.Join(".", path);
												if(m == nativeMember) {
													referencedGraphs.Add(prefab);
												}
												return true;
											}
										}
									}
								}
							}
							return false;
						};
						if (field != null || nativeMember != null) {
							bool hasChanged = false;
							Array.ForEach(scripts, script => {
								bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
								if(flag) {
									hasChanged = true;
									hasUndo = false;
									uNodeGUIUtility.GUIChanged(script);
									uNodeEditorUtility.MarkDirty(script);
								}
							});
							if (hasChanged) {
								if (gameObject != prefab) {
									uNodeEditorUtility.RegisterFullHierarchyUndo(prefab, "Rename Variable: " + variable.Name);
									if (prefabContent == null) {
										//Save the temporary graph
										GraphUtility.AutoSaveGraph(gameObject);
									} else {
										//Save the prefab contents
										uNodeEditorUtility.SavePrefabAsset(gameObject, prefab);
									}
								}
								uNodeEditorUtility.MarkDirty(prefab);
							}
						}
						if(prefabContent != null) {
							PrefabUtility.UnloadPrefabContents(prefabContent);
						}
					}
				}
				string oldVarName = variable.Name;
				variable.Name = name;
				Func<object, bool> validation = delegate (object OBJ) {
					return CallbackRenameVariable(OBJ, owner, variable.Name, oldVarName);
				};
				graph.Refresh();
				Array.ForEach(graph.nodes, item => AnalizerUtility.AnalizeObject(item, validation));
				if (GraphUtility.IsTempGraphObject(graph.gameObject)) {
					var prefab = uNodeEditorUtility.GetGameObjectSource(graph.gameObject, null);
					uNodeEditorUtility.RegisterFullHierarchyUndo(prefab, "Rename Variable: " + oldVarName);
					GraphUtility.AutoSaveGraph(graph.gameObject);
				}
				uNodeEditor.ClearGraphCache();
				uNodeEditor.window?.Refresh(true);
				DoCompileReferences(graph, referencedGraphs);
			}
		}

		public static void MoveLocalVariableToVariable(string variableName, ILocalVariableSystem LVS, IVariableSystem VS) {
			var variable = LVS.GetLocalVariableData(variableName);
			if(VS.GetVariableData(variableName) != null) {
				uNodeEditorUtility.DisplayErrorMessage("Variable with same name already exist");
				return;
			}
			var references = GraphUtility.FindLocalVariableUsages(LVS, variableName);
			if(references.Count > 0) {
				Func<object, bool> validation = delegate (object OBJ) {
					if(OBJ is MemberData) {
						MemberData member = OBJ as MemberData;
						if(member.targetType == MemberData.TargetType.uNodeLocalVariable && member.startName == variableName && member.startTarget == LVS) {
							member.targetType = MemberData.TargetType.uNodeVariable;
							member.instance = VS;
							return true;
						}
					}
					return false;
				};
				foreach(var obj in references) {
					AnalizerUtility.AnalizeObject(obj, validation);
				}
			}
			LVS.LocalVariables.Remove(variable);
			VS.Variables.Add(new VariableData(variable));
		}

		public static void MoveVariableToLocalVariable(string variableName, uNodeRoot graph) {
			var variable = graph.GetVariableData(variableName);
			var references = GraphUtility.FindVariableUsages(graph, variableName);
			if(references.Count > 0) {
				ILocalVariableSystem LVS = null;
				Object rootID = null;
				List<Object> objects = new List<Object>();
				foreach(var obj in references) {
					if(obj is NodeComponent node) {
						var owner = node.GetOwner();
						if(owner == graph) {
							objects.Add(obj);
							if(rootID == null) {
								rootID = node.rootObject ?? graph as Object;
								LVS = node.rootObject;
								if(LVS == null) {
									uNodeEditorUtility.DisplayErrorMessage("Cannot move to local variable because the variable is used by STATE graph.");
									return;
								}
							} else if(rootID != (node.rootObject ?? graph as Object)) {
								uNodeEditorUtility.DisplayErrorMessage("Cannot move to local variable because the variable is used by more than one function.");
								return;
							}
						} else {
							uNodeEditorUtility.DisplayErrorMessage("Cannot move to local variable because the variable is used by other graph.");
							return;
						}
					}
				}
				Func<object, bool> validation = delegate (object OBJ) {
					if(OBJ is MemberData) {
						MemberData member = OBJ as MemberData;
						if(member.targetType == MemberData.TargetType.uNodeVariable && member.startName == variableName && member.startTarget as Object == graph) {
							member.targetType = MemberData.TargetType.uNodeLocalVariable;
							member.instance = LVS;
							return true;
						}
					}
					return false;
				};
				foreach(var obj in objects) {
					AnalizerUtility.AnalizeObject(obj, validation);
				}
				graph.Variables.Remove(variable);
				LVS.LocalVariables.Add(new VariableData(variable) { resetOnEnter = false });
			} else {
				uNodeEditorUtility.DisplayErrorMessage("Cannot move to local variable because no node using that variable.");
			}
		}

		private static void DoCompileReferences(uNodeRoot graph, ICollection<GameObject> referencedGraphs) {
			if(referencedGraphs.Count > 0) {
				uNodeThreadUtility.Queue(() => {
					GenerationUtility.MarkGraphDirty(referencedGraphs);
					var graphs = referencedGraphs.Where(g => g.GetComponent<uNodeComponentSystem>() is uNodeRoot root && !(root is IIndependentGraph) || g == null).ToList();
					if (graphs.FirstOrDefault() != null) {
						var graphNames = string.Join("\n", graphs.Select(g => AssetDatabase.GetAssetPath(g)));
						if (!(graph is IIndependentGraph)) {
							graphs.Insert(0, graph.gameObject);
						}
						if (EditorUtility.DisplayDialog("Compile graph to c#", "Some graph has referenced to the renamed member and need to be compiled" +
							"\nReferenced graphs:" +
							"\n" + graphNames +
							"\n\nDo you want to compile this graph and referenced graphs?", "Ok", "Cancel")) {
							GenerationUtility.CompileNativeGraph(graphs);
						}
					}
				});
			}
		}

		public static void RefactorProperty(uNodeProperty property, string name) {
			name = uNodeUtility.AutoCorrectName(name);
			var graph = property.owner;
			bool hasVariable = false;
			if (graph.Properties != null && graph.Properties.Count > 0) {
				foreach (var V in graph.Properties) {
					if (V.Name == name) {
						hasVariable = true;
						break;
					}
				}
			}
			if (graph.Variables != null && graph.Variables.Count > 0) {
				foreach (var V in graph.Variables) {
					if (V.Name == name) {
						hasVariable = true;
						break;
					}
				}
			}
			if(hasVariable) return;
			Undo.SetCurrentGroupName("Rename Property: " + property.Name);
			HashSet<GameObject> referencedGraphs = new HashSet<GameObject>();
			if (graph != null) {
				RuntimeProperty runtime = null;
				if (graph is IIndependentGraph) {
					if (GraphUtility.IsTempGraphObject(graph.gameObject)) {
						var prefab = uNodeEditorUtility.GetGameObjectSource(graph.gameObject, null);
						if (prefab != null) {
							var oriGraph = prefab.GetComponent<uNodeRoot>();
							if (oriGraph != null) {
								runtime = ReflectionUtils.GetRuntimeType(oriGraph).GetProperty(property.Name) as RuntimeProperty;
							}
						}
					} else {
						runtime = ReflectionUtils.GetRuntimeType(graph).GetProperty(property.Name) as RuntimeProperty;
					}
				}
				PropertyInfo nativeMember = null;
				if(graph.GeneratedTypeName.ToType(false) != null) {
					var type = graph.GeneratedTypeName.ToType(false);
					nativeMember = type.GetProperty(property.Name, MemberData.flags);
				}
				var graphPrefabs = GraphUtility.FindGraphPrefabsWithComponent<uNodeRoot>();
				foreach (var prefab in graphPrefabs) {
					var gameObject = prefab;
					GameObject prefabContent = null;
					if (GraphUtility.HasTempGraphObject(prefab)) {
						gameObject = GraphUtility.GetTempGraphObject(prefab);
					} else if(uNodeEditorUtility.IsPrefab(prefab)) {
						prefabContent = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
						gameObject = prefabContent;
					}
					var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
					bool hasUndo = false;
					Func<object, bool> scriptValidation = (obj) => {
						MemberData member = obj as MemberData;
						if (member != null && member.startType is RuntimeType) {
							var members = member.GetMembers(false);
							if (members != null) {
								for (int i = 0; i < members.Length; i++) {
									var m = members[i];
									if (member.namePath.Length > i + 1) {
										if (m == runtime || m == nativeMember) {
											if (!hasUndo && prefabContent == null) {
												uNodeEditorUtility.RegisterFullHierarchyUndo(gameObject);
												hasUndo = true;
											}
											var path = member.namePath;
											path[i + 1] = name;
											member.name = string.Join(".", path);
											if(m == nativeMember) {
												referencedGraphs.Add(prefab);
											}
											return true;
										}
									}
								}
							}
						}
						return false;
					};
					if (runtime != null || nativeMember != null) {
						bool hasChanged = false;
						Array.ForEach(scripts, script => {
							bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
							if (flag) {
								hasChanged = true;
								hasUndo = false;
								uNodeGUIUtility.GUIChanged(script);
								uNodeEditorUtility.MarkDirty(script);
							}
						});
						if (hasChanged) {
							if (gameObject != prefab) {
								uNodeEditorUtility.RegisterFullHierarchyUndo(prefab);
								if (prefabContent == null) {
									//Save the temporary graph
									GraphUtility.AutoSaveGraph(gameObject);
								} else {
									//Save the prefab contents and unload it
									uNodeEditorUtility.SavePrefabAsset(gameObject, prefab);
								}
							}
							uNodeEditorUtility.MarkDirty(prefab);
						}
					}
					if(prefabContent != null) {
						PrefabUtility.UnloadPrefabContents(prefabContent);
					}
				}
			}
			uNodeEditorUtility.RegisterFullHierarchyUndo(graph.gameObject);
			string oldVarName = property.Name;
			property.Name = name;
			graph.Refresh();
			Func<object, bool> validation = delegate (object OBJ) {
				return CallbackRenameProperty(OBJ, graph, property.name, oldVarName);
			};
			Array.ForEach(graph.nodes, item => AnalizerUtility.AnalizeObject(item, validation));
			if (GraphUtility.IsTempGraphObject(graph.gameObject)) {
				var prefab = uNodeEditorUtility.GetGameObjectSource(graph.gameObject, null);
				uNodeEditorUtility.RegisterFullHierarchyUndo(prefab);
				GraphUtility.AutoSaveGraph(graph.gameObject);
			}
			uNodeEditor.ClearGraphCache();
			uNodeEditor.window?.Refresh(true);
			DoCompileReferences(graph, referencedGraphs);
		}

		public static bool IsValidParameter(IList<Type> types, IList<ParameterData> typeMembers) {
			if(types == null || typeMembers == null || types.Count != typeMembers.Count)
				return false;
			for(int i=0;i<types.Count; i++) {
				if(typeMembers[i].Type != types[i])
					return false;
			}
			return true;
		}

		public static Action GetUpdateReferencesAction(Object targetObj) {
			Action result = null;
			if(targetObj is RootObject root) {
				var graph = root.owner;
				if(graph == null)
					return result;
				if(root is uNodeFunction function) {
					var name = function.Name;
					var returnType = function.ReturnType();
					var paramTypes = function.Parameters.Select(p => p.Type).ToArray();
					result += () => {
						if(function.Name != name || function.ReturnType() != returnType || !IsValidParameter(paramTypes, function.Parameters)) {
							RefactorFunction(function, name, returnType, paramTypes);
						}
					};
					//var scripts = graph.GetComponentsInChildren<MonoBehaviour>(true);
					//bool needUndo = true;
					//Func<object, bool> validation = (obj) => {
					//	MemberData member = obj as MemberData;
					//	if(member != null && member.targetType == MemberData.TargetType.uNodeFunction) {
					//		if(member.GetUnityObject() == root) {
					//			action += () => {
					//				if(needUndo) {
					//					Undo.RegisterFullObjectHierarchyUndo(graph, "");
					//					needUndo = false;
					//				}
					//				MemberData d = MemberData.CreateFromValue(function);
					//				member.CopyFrom(d);
					//				UGraphView.ClearCache(function.owner);
					//			};
					//		}
					//	}
					//	return false;
					//};
					//foreach(var script in scripts) {
					//	AnalizerUtility.AnalizeObject(script, validation);
					//}
				}
			}
			return result;
		}

		private static void RefactorFunction(uNodeFunction function, string name, Type returnType, Type[] paramTypes) {
			var graph = function.owner;
			Undo.SetCurrentGroupName("Refactor Function: " + function.Name);
			HashSet<GameObject> referencedGraphs = new HashSet<GameObject>();
			if(graph != null) {
				RuntimeMethod runtime = null;
				if(graph is IIndependentGraph) {
					if(GraphUtility.IsTempGraphObject(graph.gameObject)) {
						var prefab = uNodeEditorUtility.GetGameObjectSource(graph.gameObject, null);
						if(prefab != null) {
							var oriGraph = prefab.GetComponent<uNodeRoot>();
							if(oriGraph != null) {
								var methods = ReflectionUtils.GetRuntimeType(oriGraph).GetMethods();
								foreach(var m in methods) {
									if(m is RuntimeMethod && m.Name == name) {
										var parameters = m.GetParameters();
										if(parameters.Length == paramTypes.Length) {
											if(runtime == null) {
												runtime = m as RuntimeMethod;
											}
											bool isValid = true;
											for(int i = 0; i < parameters.Length; i++) {
												if(parameters[i].ParameterType != paramTypes[i]) {
													isValid = false;
													break;
												}
											}
											if(isValid) {
												runtime = m as RuntimeMethod;
											}
										}
									}
								}
							}
						}
					} else {
						var methods = ReflectionUtils.GetRuntimeType(graph).GetMethods();
						foreach(var m in methods) {
							if(m is RuntimeGraphMethod runtimeMethod && runtimeMethod.target == function) {
								runtime = runtimeMethod;
								break;
							}
						}
					}
				}
				MemberInfo nativeMember = null;
				if(graph.GeneratedTypeName.ToType(false) != null) {
					var type = graph.GeneratedTypeName.ToType(false);
					if(paramTypes.Length == 0 && function.GenericParameters.Count == 0) {
						nativeMember = type.GetMethod(name, MemberData.flags);
					}
					var members = type.GetMember(name, MemberData.flags);
					if(members != null) {
						var genericLength = function.GenericParameters.Count;
						foreach(var m in members) {
							if(m is MethodInfo method) {
								var mParam = method.GetParameters();
								var mGeneric = method.GetGenericArguments();
								if(paramTypes.Length == mParam.Length && mGeneric.Length == genericLength) {
									bool valid = true;
									for(int i = 0; i < mParam.Length; i++) {
										if(mParam[i].ParameterType != paramTypes[i]) {
											valid = false;
											break;
										}
									}
									if(valid) {
										nativeMember = method;
										break;
									}
								}
							}
						}
					}
				}
				var graphPrefabs = GraphUtility.FindGraphPrefabsWithComponent<uNodeRoot>();
				foreach(var prefab in graphPrefabs) {
					var gameObject = prefab;
					GameObject prefabContent = null;
					if(GraphUtility.HasTempGraphObject(prefab)) {
						gameObject = GraphUtility.GetTempGraphObject(prefab);
					} else if(uNodeEditorUtility.IsPrefab(prefab)) {
						prefabContent = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
						gameObject = prefabContent;
					}
					var scripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
					bool hasUndo = false;
					Func<object, bool> scriptValidation = (obj) => {
						MemberData member = obj as MemberData;
						if(member != null && member.startType is RuntimeType) {
							var members = member.GetMembers(false);
							if(members != null) {
								for(int i = 0; i < members.Length; i++) {
									var m = members[i];
									if(member.namePath.Length > i + 1) {
										if(m == runtime || m == nativeMember) {
											if(!hasUndo && prefabContent == null) {
												uNodeEditorUtility.RegisterFullHierarchyUndo(gameObject);
												hasUndo = true;
											}
											var path = member.namePath;
											path[i + 1] = function.Name;
											member.name = string.Join(".", path);
											{
												var items = member.Items;
												if(items.Length > i) {
													var mVal = MemberData.CreateFromMember(runtime);
													items[i] = mVal.Items[0];
													member.SetItems(items);
												}
											}
											if(m == nativeMember) {
												referencedGraphs.Add(prefab);
											}
											return true;
										}
									}
								}
							}
						}
						return false;
					};
					if(runtime != null) {
						bool hasChanged = false;
						Array.ForEach(scripts, script => {
							bool flag = AnalizerUtility.AnalizeObject(script, scriptValidation);
							if(flag) {
								hasChanged = true;
								hasUndo = false;
								uNodeGUIUtility.GUIChanged(script);
								uNodeEditorUtility.MarkDirty(script);
							}
						});
						if(hasChanged) {
							if(gameObject != prefab) {
								uNodeEditorUtility.RegisterFullHierarchyUndo(prefab);
								if(prefabContent == null) {
									//Save the temporary graph
									GraphUtility.AutoSaveGraph(gameObject);
								} else {
									//Save the prefab contents and unload it
									uNodeEditorUtility.SavePrefabAsset(gameObject, prefab);
								}
							}
							uNodeEditorUtility.MarkDirty(prefab);
						}
					}
					if(prefabContent != null) {
						PrefabUtility.UnloadPrefabContents(prefabContent);
					}
				}
			}
			uNodeEditorUtility.RegisterFullHierarchyUndo(graph.gameObject);
			graph.Refresh();
			Func<object, bool> validation = delegate (object OBJ) {
				return CallbackRefactorFunction(OBJ, function, name, paramTypes);
			};
			Array.ForEach(graph.nodes, item => AnalizerUtility.AnalizeObject(item, validation));
			if(GraphUtility.IsTempGraphObject(graph.gameObject)) {
				var prefab = uNodeEditorUtility.GetGameObjectSource(graph.gameObject, null);
				uNodeEditorUtility.RegisterFullHierarchyUndo(prefab);
				GraphUtility.AutoSaveGraph(graph.gameObject);
			}
			uNodeEditor.ClearGraphCache();
			uNodeEditor.window?.Refresh(true);
			DoCompileReferences(graph, referencedGraphs);
		}

		private static bool CallbackRefactorFunction(object obj, uNodeFunction func, string name, Type[] paramTypes) {
			if(obj is MemberData) {
				MemberData member = obj as MemberData;
				if(member.instance as UnityEngine.Object == func.owner && member.targetType == MemberData.TargetType.uNodeFunction) {
					int pLength = member.ParameterTypes[0] == null ? 0 : member.ParameterTypes[0].Length;
					if(member.startName.Equals(name) && pLength == paramTypes.Length) {
						bool isValid = true;
						if(pLength != 0) {
							for(int x = 0; x < paramTypes.Length; x++) {
								if(paramTypes[x] != member.ParameterTypes[0][x]) {
									isValid = false;
									break;
								}
							}
						}
						if(isValid) {
							if(!member.isDeepTarget) {
								MemberData.CreateFromValue(func).CopyTo(member);
							} else {
								var mVal = MemberData.CreateFromValue(func);
								var items = member.Items;
								items[0] = mVal.Items[0];
								member.SetItems(items);
								member.startName = func.Name;
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		public static void RefactorFunction(uNodeFunction function, string name) {
			name = uNodeUtility.AutoCorrectName(name);
			var graph = function.owner;
			var action = GetUpdateReferencesAction(function);
			function.Name = name;
			action?.Invoke();
		}

		#region RetargetNode
		public static void RetargetNode(Node source, Node destination) {
			HashSet<uNodeComponent> nodes = new HashSet<uNodeComponent>();
			foreach(Transform t in source.transform.parent) {
				var comp = t.GetComponent<uNodeComponent>();
				if(comp != null) {
					if(comp is StateNode) {
						var state = comp as StateNode;
						var transitions = state.GetTransitions();
						foreach(var tr in transitions) {
							nodes.Add(tr);
						}
					}
					nodes.Add(comp);
				}
			}
			if(source.transform.parent.parent != null) {
				foreach(Transform t in source.transform.parent.parent) {
					var comp = t.GetComponent<uNodeComponent>();
					if(comp != null) {
						if(comp is StateNode) {
							var state = comp as StateNode;
							var transitions = state.GetTransitions();
							foreach(var tr in transitions) {
								nodes.Add(tr);
							}
						}
						nodes.Add(comp);
					}
				}
			}
			Func<object, bool> validation = delegate (object o) {
				if(o is MemberData) {
					MemberData member = o as MemberData;
					if(member.targetType == MemberData.TargetType.ValueNode ||
						member.targetType == MemberData.TargetType.FlowNode ||
						member.targetType == MemberData.TargetType.NodeField ||
						member.targetType == MemberData.TargetType.NodeFieldElement ||
						member.targetType == MemberData.TargetType.FlowInput) {
						var n = member.GetTargetNode();
						if(n != null && n == source) {
							member.RefactorUnityObject(new Object[] { source }, new Object[] { destination });
							//return true;
						}
					}
				}
				return false;
			};
			foreach(var n in nodes) {
				AnalizerUtility.AnalizeObject(n, validation);
			}
		}

		public static void RetargetValueNode(Node source, Node destination) {
			List<uNodeComponent> nodes = new List<uNodeComponent>();
			foreach(Transform t in source.transform.parent) {
				var comp = t.GetComponent<uNodeComponent>();
				if(comp != null) {
					if(comp is StateNode) {
						var state = comp as StateNode;
						var transitions = state.GetTransitions();
						foreach(var tr in transitions) {
							nodes.Add(tr);
						}
					}
					nodes.Add(comp);
				}
			}
			Func<object, bool> validation = delegate (object o) {
				if(o is MemberData) {
					MemberData member = o as MemberData;
					if(member.targetType == MemberData.TargetType.ValueNode) {
						var n = member.GetTargetNode();
						if(n != null && n == source) {
							member.RefactorUnityObject(new Object[] { source }, new Object[] { destination });
							//return true;
						}
					}
				}
				return false;
			};
			foreach(var n in nodes) {
				AnalizerUtility.AnalizeObject(n, validation);
			}
		}
		#endregion

		#region Callback
		public static bool CallbackRenameVariable(object obj, UnityEngine.Object owner, string variableName, string oldVariableName) {
			if (obj is MemberData) {
				MemberData member = obj as MemberData;
				if (member.instance as UnityEngine.Object == owner && member.targetType.IsTargetingVariable()) {
					if ((owner is uNodeRoot && member.targetType == MemberData.TargetType.uNodeVariable ||
						owner is INode<uNodeRoot> && (
							member.targetType == MemberData.TargetType.uNodeGroupVariable || 
							member.targetType == MemberData.TargetType.uNodeLocalVariable)) &&
						member.startName.Equals(oldVariableName)) {
						member.startName = variableName;
						return true;
					}
				}
			}
			return false;
		}

		public static bool CallbackRenameProperty(object obj, UnityEngine.Object owner, string name, string oldName) {
			if (obj is MemberData) {
				MemberData member = obj as MemberData;
				if (member.instance as UnityEngine.Object == owner && (member.targetType == MemberData.TargetType.uNodeProperty)) {
					if (member.startName.Equals(oldName)) {
						member.startName = name;
						return true;
					}
				}
			}
			return false;
		}

		public static bool CallbackRenameLocalVariable(object obj, RootObject owner, string variableName, string newName) {
			if (obj is MemberData) {
				MemberData member = obj as MemberData;
				if (member.instance as UnityEngine.Object == owner && (member.targetType == MemberData.TargetType.uNodeLocalVariable)) {
					if (member.startName.Equals(variableName)) {
						member.startName = newName;
						return true;
					}
				}
			}
			return false;
		}
		#endregion
	}
}