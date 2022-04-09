using UnityEngine;
using UnityEditor;
using MaxyGames.Events;
using MaxyGames.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.Callbacks;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using MaxyGames.OdinSerializer.Editor;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	/// <summary>
	/// Provides function to initialize useful function.
	/// </summary>
	[InitializeOnLoad]
	public class uNodeEditorInitializer {
		static Texture uNodeIcon;
		static Texture2D _backgroundIcon;
		static Texture2D backgroundIcon {
			get {
				if(_backgroundIcon == null) {
					if(EditorGUIUtility.isProSkin) {
						_backgroundIcon = uNodeEditorUtility.MakeTexture(1, 1, new Color(0.2196079f, 0.2196079f, 0.2196079f));
					} else {
						_backgroundIcon = uNodeEditorUtility.MakeTexture(1, 1, new Color(0.7607844f, 0.7607844f, 0.7607844f));
					}
				}
				return _backgroundIcon;
			}
		}
		static List<int> markedObjects = new List<int>();
		static HashSet<string> assetGUIDs = new HashSet<string>();
		static Dictionary<string, UnityEngine.Object> markedAssets = new Dictionary<string, UnityEngine.Object>();

		static uNodeEditorInitializer() {
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyItem;
			EditorApplication.projectWindowItemOnGUI += ProjectItem;
			Selection.selectionChanged += OnSelectionChanged;
			SceneView.duringSceneGui += OnSceneGUI;
			EditorApplication.update += Update;
			// Setup();

			#region Bind Init
			uNodeUtility.getActualObject = (obj) => {
				if(obj == null)
					return null;
				if(uNodeEditorUtility.IsPrefabInstance(obj)) {
					return PrefabUtility.GetCorrespondingObjectFromSource(obj);
				} else if(uNodeEditorUtility.IsPrefab(obj)) {
					return obj;
				} else {
					return uNodeEditorUtility.GetObjectSource(obj, null, obj.GetType()) ?? obj;
				}
			};
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			//UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += EditorBinding.onSceneChanged;
			var typePatcher = "MaxyGames.uNode.Editors.TypePatcher".ToType(false);
			if(typePatcher != null) {
				var method = typePatcher.GetMethod("Patch", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				EditorBinding.patchType = (System.Action<System.Type, System.Type>)System.Delegate.CreateDelegate(typeof(System.Action<System.Type, System.Type>), method);
			}
			if(EditorApplication.isPlayingOrWillChangePlaymode) {
				uNodeUtility.isPlaying = true;
#if UNODE_COMPILE_ON_PLAY
				uNodeThreadUtility.Queue(() => {
					GenerationUtility.CompileProjectGraphsAnonymous();
				});
#endif
			} else {
				//Set is playing to false
				uNodeUtility.isPlaying = false;
			}
			#endregion
		}

		private static void OnPlayModeChanged(PlayModeStateChange state) {
			switch(state) {
				case PlayModeStateChange.ExitingEditMode:
					//case PlayModeStateChange.ExitingPlayMode:
					//Make sure we save all temporary graph on exit play mode or edit mode.
					GraphUtility.SaveAllGraph(false);
					GenerationUtility.SaveData();
					if(uNodeEditor.window != null) {
						uNodeEditor.window.SaveEditorData();
					}
					break;
				case PlayModeStateChange.EnteredEditMode:
					//If user is saving graph in play mode
					if(EditorPrefs.GetBool("unode_graph_saved_in_playmode", true)) {
						//Ensure the saved graph in play mode keep the changes.
						GraphUtility.DestroyTempGraph();
						EditorPrefs.SetBool("unode_graph_saved_in_playmode", false);
					}
					//Set is playing to false
					uNodeUtility.isPlaying = false;
					ReflectionUtils.ClearInvalidRuntimeType();
#if UNITY_2019_3_OR_NEWER
					//If play mode options is enable and domain reload is disable
					if(EditorSettings.enterPlayModeOptionsEnabled && EditorSettings.enterPlayModeOptions.HasFlags(EnterPlayModeOptions.DisableDomainReload)) {
						//then clear graph cache
						UGraphView.ClearCache();
						uNodeThreadUtility.ExecuteAfter(5, () => {
							UGraphView.ClearCache();
							uNodeEditor.window?.Refresh(true);
						});
						return;
					}
#endif
					uNodeThreadUtility.ExecuteAfter(5, () => {
						UGraphView.ClearCache();
					});
					if(uNodeEditor.window != null) {
						EditorApplication.delayCall += () => uNodeEditor.window.Refresh(true);
					}
					//EditorBinding.restorePatch?.Invoke();
					break;
				case PlayModeStateChange.EnteredPlayMode:
					//This will prevent destroying temp graphs in play mode
					GraphUtility.PreventDestroyOnPlayMode();
					break;
				case PlayModeStateChange.ExitingPlayMode:
					//Update the assembly
					ReflectionUtils.UpdateAssemblies();
					//Clean compiled runtime assembly so the runtime type is cannot be loaded again
					ReflectionUtils.CleanRuntimeAssembly();
					break;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		static void RuntimeSetup() {
#if UNITY_2019_3_OR_NEWER
			//If play mode options is enable and domain reload is disable
			if(EditorSettings.enterPlayModeOptionsEnabled && EditorSettings.enterPlayModeOptions.HasFlags(EnterPlayModeOptions.DisableDomainReload)) {
				//then enable is playing and clean graph cache
				uNodeUtility.isPlaying = true;
				UGraphView.ClearCache();
				//Clean Type Cache so it get fresh types.
				TypeSerializer.CleanCache();
				//Clean compiled runtime assembly so the runtime type is cannot be loaded again.
				ReflectionUtils.CleanRuntimeAssembly();
#if UNODE_COMPILE_ON_PLAY
				if(uNodePreference.preferenceData.generatorData.compilationMethod == CompilationMethod.Unity) {
					//Do compile graphs project in temporary folder and load it when using auto compile on play
					GenerationUtility.CompileProjectGraphsAnonymous();
				}
#endif
			}
#endif
			//Load a Runtime Graph Assembly.
			if(uNodePreference.preferenceData.generatorData.compilationMethod == CompilationMethod.Roslyn) {
				GenerationUtility.LoadRuntimeAssembly();
#if UNODE_COMPILE_ON_PLAY
				Debug.LogWarning("Warning: you're using Compile On Play & Roslyn compilation, this is not supported.\nThe auto compile on play will be ignored.");
#endif
			}
		}

		[InitializeOnLoadMethod]
		static void Setup() {
			uNodeIcon = uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.UNodeIcon));
			UpdateMarkedObject();
			uNodeUtility.hideRootObject = () => uNodePreference.GetPreference().hideChildObject;

			#region uNodeUtils Init
			uNodeUtility.isInEditor = true;
			if(uNodeUtility.guiChanged == null) {
				uNodeUtility.guiChanged += uNodeEditor.GUIChanged;
			}
			if(uNodeUtility.RegisterCompleteObjectUndo == null) {
				uNodeUtility.RegisterCompleteObjectUndo = uNodeEditorUtility.RegisterUndo;
			}
			if(uNodeUtility.richTextColor == null) {
				uNodeUtility.richTextColor = () => {
					if(uNodePreference.editorTheme != null) {
						return uNodePreference.editorTheme.textSettings;
					} else {
						return new EditorTextSetting();
					}
				};
			}
			if(uNodeUtility.getColorForType == null) {
				uNodeUtility.getColorForType = (t) => {
					return uNodePreference.GetColorForType(t);
				};
			}
			if(uNodeUtility.getObjectID == null) {
				uNodeUtility.getObjectID = delegate (UnityEngine.Object obj) {
					if(obj == null)
						return 0;
					if(uNodeEditorUtility.IsPrefab(obj)) {
						return (int)Unsupported.GetLocalIdentifierInFileForPersistentObject(obj);
					} else if(uNodeEditorUtility.IsPrefabInstance(obj)) {
						var o = PrefabUtility.GetCorrespondingObjectFromSource(obj);
						if(o == null)
							return obj.GetInstanceID();
						return (int)Unsupported.GetLocalIdentifierInFileForPersistentObject(o);
					}
					var result = uNodeEditorUtility.GetObjectSource(obj, null, obj.GetType());
					if(result == null || !EditorUtility.IsPersistent(result)) {
						return obj.GetInstanceID();
					}
					return (int)Unsupported.GetLocalIdentifierInFileForPersistentObject(result);
				};
			}
			if(uNodeUtility.debugObject == null) {
				uNodeUtility.debugObject = delegate () {
					if(uNodeEditor.window != null) {
						return uNodeEditor.window.debugObject;
					}
					return null;
				};
			}
			if(GraphDebug.hasBreakpoint == null) {
				GraphDebug.hasBreakpoint = delegate (int id) {
					return nodeDebugData.Contains(id);
				};
			}
			if(GraphDebug.addBreakpoint == null) {
				GraphDebug.addBreakpoint = delegate (int id) {
					if(!nodeDebugData.Contains(id)) {
						nodeDebugData.Add(id);
						SaveDebugData();
					}
				};
			}
			if(GraphDebug.removeBreakpoint == null) {
				GraphDebug.removeBreakpoint = delegate (int id) {
					if(nodeDebugData.Contains(id)) {
						nodeDebugData.Remove(id);
						SaveDebugData();
					}
				};
			}
			#endregion

			#region uNodeDEBUG Init
			if(uNodeDEBUG.InvokeEvent == null) {
				uNodeDEBUG.InvokeEvent = delegate (EventCoroutine coroutine, int objectUID, int nodeUID) {
					if(!GraphDebug.useDebug || !Application.isPlaying)
						return;
					Dictionary<object, GraphDebug.DebugData> debugMap = null;
					if(GraphDebug.debugData.ContainsKey(objectUID)) {
						debugMap = GraphDebug.debugData[objectUID];
					} else {
						debugMap = new Dictionary<object, GraphDebug.DebugData>();
						GraphDebug.debugData.Add(objectUID, debugMap);
					}
					object obj = coroutine.owner;
					GraphDebug.DebugData data = null;
					if(debugMap.ContainsKey(obj)) {
						data = debugMap[obj];
					} else {
						data = new GraphDebug.DebugData();
						debugMap.Add(obj, data);
					}
					GraphDebug.DebugData.NodeDebug nodeDebug = null;
					if(data.nodeDebug.ContainsKey(nodeUID)) {
						nodeDebug = data.nodeDebug[nodeUID];
					} else {
						nodeDebug = new GraphDebug.DebugData.NodeDebug();
						data.nodeDebug[nodeUID] = nodeDebug;
					}
					nodeDebug.customCondition = delegate () {
						if(string.IsNullOrEmpty(coroutine.state) || !coroutine.IsFinished) {
							return StateType.Running;
						} else {
							return coroutine.IsSuccess ? StateType.Success : StateType.Failure;
						}
					};
					nodeDebug.calledTime = Time.unscaledTime;
				};
			}
			if(uNodeDEBUG.InvokeEventNode == null) {
				uNodeDEBUG.InvokeEventNode = GraphDebug.FlowNode;
			}
			if(uNodeDEBUG.InvokeFlowNode == null) {
				uNodeDEBUG.InvokeFlowNode = GraphDebug.FlowTransition;
			}
			if(uNodeDEBUG.InvokeTransition == null) {
				uNodeDEBUG.InvokeTransition = GraphDebug.Transition;
			}
			if(uNodeDEBUG.invokeValueNode == null) {
				uNodeDEBUG.invokeValueNode = (a, b, c, d) => {
					GraphDebug.ValueNode(a, b, c, (int)(d as object[])[0], (d as object[])[1], (bool)(d as object[])[2]);
				};
			}
			#endregion

			#region EventDataDrawer Init
			if(EventDataDrawer.customMenu == null) {
				var menu = new List<CustomEventMenu>();
				menu.Add(new CustomEventMenu() {
					isSeparator = true,
				});
				menu.Add(new CustomEventMenu() {
					menuName = "EqualityComparer",
					isValidationMenu = true,
					filter = new FilterAttribute() { HideTypes = new List<System.Type>() { typeof(void) } },
					onClickItem = delegate (MemberData m) {
						EqualityCompare v = new EqualityCompare() { target = new MultipurposeMember() { target = m } };
						return new EventActionData(v, EventActionData.EventType.Event);
					},
				});
				//menu.Add(new CustomEventMenu() {
				//	menuName = "ConditionValidation",
				//	isValidationMenu = true,
				//	filter = new FilterAttribute(typeof(bool)) { MaxMethodParam = int.MaxValue },
				//	onClickItem = delegate (MemberData m) {
				//		MethodValidation v = new MethodValidation() { target = new MultipurposeMember() { target = m } };
				//		MemberDataUtility.UpdateMultipurposeMember(v.target);
				//		return new EventActionData(v, EventActionData.EventType.Event);
				//	},
				//});
				menu.Add(new CustomEventMenu() {
					menuName = "Compare Object",
					isValidationMenu = true,
					filter = new FilterAttribute() { HideTypes = new List<System.Type>() { typeof(void) } },
					onClickItem = delegate (MemberData m) {
						ObjectCompare v = new ObjectCompare();
						v.targetA = new MultipurposeMember() { target = m };
						MemberDataUtility.UpdateMultipurposeMember(v.targetA);
						return new EventActionData(v, EventActionData.EventType.Event);
					},
				});
				menu.Add(new CustomEventMenu() {
					isSeparator = true,
					isValidationMenu = true
				});
				menu.Add(new CustomEventMenu() {
					menuName = "Invoke or GetValue",
					filter = new FilterAttribute() { MaxMethodParam = int.MaxValue, VoidType = true },
					onClickItem = delegate (MemberData m) {
						GetValue v = new GetValue() { target = new MultipurposeMember() { target = m } };
						MemberDataUtility.UpdateMultipurposeMember(v.target);
						return new EventActionData(v, EventActionData.EventType.Event);
					},
				});
				menu.Add(new CustomEventMenu() {
					menuName = "SetValue",
					filter = new FilterAttribute() { SetMember = true },
					onClickItem = delegate (MemberData m) {
						SetValue v = new SetValue() { target = m };
						return new EventActionData(v, EventActionData.EventType.Event);
					},
				});
				EventDataDrawer.customMenu = menu;
			}
			if(EventDataDrawer.dragAndDropCapturer == null) {
				var drag = new List<DragAndDropCapturer>();
				drag.Add(new DragAndDropCapturer() {
					validation = (x) => {
						if(DragAndDrop.GetGenericData("uNode") != null || DragAndDrop.visualMode == DragAndDropVisualMode.None && DragAndDrop.objectReferences.Length == 1) {
							return true;
						}
						return false;
					},
					onDragPerformed = (ed, z) => {
						var MPos = uNodeGUIUtility.GUIToScreenPoint(Event.current.mousePosition);
						if(DragAndDrop.GetGenericData("uNode") != null) {
							var generic = DragAndDrop.GetGenericData("uNode");
							var UT = DragAndDrop.GetGenericData("uNode-Target") as UnityEngine.Object;
							if(generic is uNodeFunction) {
								var function = generic as uNodeFunction;
								EventData ED = ed;
								UnityEngine.Object UO = z;
								GetValue val = new GetValue();
								val.target.target = MemberData.CreateFromValue(function);
								MemberDataUtility.UpdateMultipurposeMember(val.target);
								if(UO)
									uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
								ED.blocks.Add(new EventActionData(val));
							} else if(generic is uNodeProperty) {
								var property = generic as uNodeProperty;
								GenericMenu menu = new GenericMenu();
								if(property.CanGetValue()) {
									menu.AddItem(new GUIContent("Get"), false, (y) => {
										EventData ED = (y as object[])[0] as EventData;
										UnityEngine.Object UO = (y as object[])[1] as UnityEngine.Object;
										GetValue val = new GetValue();
										val.target.target = MemberData.CreateFromValue(property, UT as IPropertySystem);
										MemberDataUtility.UpdateMultipurposeMember(val.target);
										if(UO)
											uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
										ED.blocks.Add(new EventActionData(val));
									}, new object[] { ed, z });
								}
								if(property.CanSetValue()) {
									menu.AddItem(new GUIContent("Set"), false, (y) => {
										EventData ED = (y as object[])[0] as EventData;
										UnityEngine.Object UO = (y as object[])[1] as UnityEngine.Object;
										SetValue val = new SetValue();
										var mData = MemberData.CreateFromValue(property, UT as IPropertySystem);
										val.target = mData;
										if(mData.type != null) {
											val.value = MemberData.CreateValueFromType(mData.type);
										}
										if(UO)
											uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
										ED.blocks.Add(new EventActionData(val));
									}, new object[] { ed, z });
								}
								menu.ShowAsContext();
							} else if(generic is VariableData) {
								var varData = generic as VariableData;
								GenericMenu menu = new GenericMenu();
								menu.AddItem(new GUIContent("Get"), false, (y) => {
									EventData ED = (y as object[])[0] as EventData;
									UnityEngine.Object UO = (y as object[])[1] as UnityEngine.Object;
									GetValue val = new GetValue();
									val.target.target = MemberData.CreateFromValue(varData, UT);
									MemberDataUtility.UpdateMultipurposeMember(val.target);
									if(UO)
										uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
									ED.blocks.Add(new EventActionData(val));
								}, new object[] { ed, z });
								menu.AddItem(new GUIContent("Set"), false, (y) => {
									EventData ED = (y as object[])[0] as EventData;
									UnityEngine.Object UO = (y as object[])[1] as UnityEngine.Object;
									var mData = MemberData.CreateFromValue(varData, UT);
									SetValue val = new SetValue();
									val.target = mData;
									if(mData.type != null) {
										val.value = MemberData.CreateValueFromType(mData.type);
									}
									if(UO)
										uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
									ED.blocks.Add(new EventActionData(val));
								}, new object[] { ed, z });
								menu.ShowAsContext();
							}
						} else {
							GenericMenu menu = new GenericMenu();
							var unityObject = DragAndDrop.objectReferences[0];
							System.Action<Object, string> action = (dOBJ, startName) => {
								menu.AddItem(new GUIContent(startName + "Get"), false, (y) => {
									EventData ED = (y as object[])[0] as EventData;
									UnityEngine.Object UO = (y as object[])[1] as UnityEngine.Object;
									FilterAttribute filter = new FilterAttribute();
									filter.MaxMethodParam = int.MaxValue;
									filter.VoidType = true;
									filter.Public = true;
									filter.Instance = true;
									filter.Static = false;
									filter.DisplayDefaultStaticType = false;
									filter.InvalidTargetType = MemberData.TargetType.Null | MemberData.TargetType.Values;
									var customItems = ItemSelector.MakeCustomItems(dOBJ.GetType(), filter);
									if(customItems != null) {
										ItemSelector w = ItemSelector.ShowWindow(dOBJ, new MemberData(dOBJ, MemberData.TargetType.SelfTarget), filter, delegate (MemberData value) {
											value.instance = dOBJ;
											GetValue val = new GetValue();
											val.target.target = value;
											MemberDataUtility.UpdateMultipurposeMember(val.target);
											if(UO)
												uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
											ED.blocks.Add(new EventActionData(val));
										}, customItems).ChangePosition(MPos);
										w.displayDefaultItem = false;
									}
								}, new object[] { ed, z });
								menu.AddItem(new GUIContent(startName + "Set"), false, (y) => {
									EventData ED = (y as object[])[0] as EventData;
									UnityEngine.Object UO = (y as object[])[1] as UnityEngine.Object;
									FilterAttribute filter = new FilterAttribute();
									filter.Public = true;
									filter.Instance = true;
									filter.Static = false;
									filter.SetMember = true;
									filter.DisplayDefaultStaticType = false;
									filter.InvalidTargetType = MemberData.TargetType.Null | MemberData.TargetType.Values;
									var customItems = ItemSelector.MakeCustomItems(dOBJ.GetType(), filter);
									if(customItems != null) {
										ItemSelector w = ItemSelector.ShowWindow(dOBJ, new MemberData(dOBJ, MemberData.TargetType.SelfTarget), filter, delegate (MemberData value) {
											value.instance = dOBJ;
											SetValue val = new SetValue();
											val.target = value;
											if(value.type != null) {
												val.value = MemberData.CreateValueFromType(value.type);
											}
											if(UO)
												uNodeEditorUtility.RegisterUndo(UO, "Add new Event");
											ED.blocks.Add(new EventActionData(val));
										}, customItems).ChangePosition(MPos);
										w.displayDefaultItem = false;
									}
								}, new object[] { ed, z });
							};
							action(unityObject, "");
							menu.AddSeparator("");
							if(unityObject is GameObject) {
								Component[] components = (unityObject as GameObject).GetComponents<Component>();
								foreach(var c in components) {
									action(c, c.GetType().Name + "/");
								}
							} else if(unityObject is Component) {
								action((unityObject as Component).gameObject, "GameObject/");
								Component[] components = (unityObject as GameObject).GetComponents<Component>();
								foreach(var c in components) {
									action(c, c.GetType().Name + "/");
								}
							}
							menu.ShowAsContext();
						}
					}
				});
				EventDataDrawer.dragAndDropCapturer = drag;
			}
			#endregion

			#region Completions
			if(CompletionEvaluator.completionToNode == null) {
				CompletionEvaluator.completionToNode = (CompletionInfo completion, GraphEditorData editorData, Vector2 graphPosition) => {
					NodeComponent result = null;
					if(completion.isKeyword) {
						switch(completion.keywordKind) {
							case KeywordKind.As:
								NodeEditorUtility.AddNewNode<Nodes.ASNode>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.Break:
								NodeEditorUtility.AddNewNode<NodeJumpStatement>(editorData,
									graphPosition,
									(node) => {
										node.statementType = JumpStatementType.Break;
										result = node;
									});
								break;
							case KeywordKind.Continue:
								NodeEditorUtility.AddNewNode<NodeJumpStatement>(editorData,
									graphPosition,
									(node) => {
										node.statementType = JumpStatementType.Continue;
										result = node;
									});
								break;
							case KeywordKind.Default:
								NodeEditorUtility.AddNewNode<Nodes.DefaultNode>(editorData,
									graphPosition,
									(node) => {
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.type = member;
											}
										}
										result = node;
									});
								break;
							case KeywordKind.For:
								NodeEditorUtility.AddNewNode<Nodes.ForNumberLoop>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.Foreach:
								NodeEditorUtility.AddNewNode<Nodes.ForeachLoop>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.If:
								NodeEditorUtility.AddNewNode<Nodes.NodeIf>(editorData,
									graphPosition,
									(node) => {
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.condition = member;
											}
										}
										result = node;
									});
								break;
							case KeywordKind.Is:
								NodeEditorUtility.AddNewNode<Nodes.ISNode>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.Lock:
								NodeEditorUtility.AddNewNode<Nodes.NodeLock>(editorData,
									graphPosition,
									(node) => {
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.target = member;
											}
										}
										result = node;
									});
								break;
							case KeywordKind.Return:
								NodeEditorUtility.AddNewNode<NodeReturn>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.Switch:
								NodeEditorUtility.AddNewNode<Nodes.NodeSwitch>(editorData,
									graphPosition,
									(node) => {
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.target = member;
											}
										}
										result = node;
									});
								break;
							case KeywordKind.Throw:
								NodeEditorUtility.AddNewNode<Nodes.NodeThrow>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.Try:
								NodeEditorUtility.AddNewNode<Nodes.NodeTry>(editorData,
									graphPosition,
									(node) => {
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.Try = member;
											}
										}
										result = node;
									});
								break;
							case KeywordKind.Using:
								NodeEditorUtility.AddNewNode<Nodes.NodeUsing>(editorData,
									graphPosition,
									(node) => {
										result = node;
									});
								break;
							case KeywordKind.While:
								NodeEditorUtility.AddNewNode<Nodes.WhileLoop>(editorData,
									graphPosition,
									(node) => {
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.condition = member;
											}
										}
										result = node;
									});
								break;
						}
					} else if(completion.isSymbol) {
						switch(completion.name) {
							case "+":
							case "-":
							case "*":
							case "/":
							case "%":
								NodeEditorUtility.AddNewNode<Nodes.MultiArithmeticNode>(editorData,
									graphPosition,
									(node) => {
										if(completion.genericCompletions != null && completion.genericCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.genericCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targets[0] = member;
											}
										}
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targets[1] = member;
											}
										}
										if(completion.name == "+") {
											node.operatorType = ArithmeticType.Add;
										} else if(completion.name == "-") {
											node.operatorType = ArithmeticType.Subtract;
										} else if(completion.name == "*") {
											node.operatorType = ArithmeticType.Multiply;
										} else if(completion.name == "/") {
											node.operatorType = ArithmeticType.Divide;
										} else if(completion.name == "%") {
											node.operatorType = ArithmeticType.Modulo;
										}
										result = node;
									});
								break;
							case "==":
							case "!=":
							case ">":
							case ">=":
							case "<":
							case "<=":
								NodeEditorUtility.AddNewNode<Nodes.ComparisonNode>(editorData,
									graphPosition,
									(node) => {
										if(completion.genericCompletions != null && completion.genericCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.genericCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targetA = member;
											}
										}
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targetB = member;
											}
										}
										if(completion.name == "==") {
											node.operatorType = ComparisonType.Equal;
										} else if(completion.name == "!=") {
											node.operatorType = ComparisonType.NotEqual;
										} else if(completion.name == ">") {
											node.operatorType = ComparisonType.GreaterThan;
										} else if(completion.name == ">=") {
											node.operatorType = ComparisonType.GreaterThanOrEqual;
										} else if(completion.name == "<") {
											node.operatorType = ComparisonType.LessThan;
										} else if(completion.name == "<=") {
											node.operatorType = ComparisonType.LessThanOrEqual;
										}
										result = node;
									});
								break;
							case "++":
							case "--":
								NodeEditorUtility.AddNewNode<Nodes.IncrementDecrementNode>(editorData,
									graphPosition,
									(node) => {
										node.isDecrement = completion.name == "--";
										if(completion.genericCompletions != null && completion.genericCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.genericCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.target = member;
											}
											node.isPrefix = false;
										} else if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.target = member;
											}
											node.isPrefix = true;
										}
										result = node;
									});
								break;
							case "=":
							case "+=":
							case "-=":
							case "/=":
							case "*=":
							case "%=":
								NodeEditorUtility.AddNewNode<NodeSetValue>(editorData,
									graphPosition,
									(node) => {
										if(completion.genericCompletions != null && completion.genericCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.genericCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.target = member;
											}
										}
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.value = member;
											}
										}
										if(completion.name == "=") {
											node.setType = SetType.Change;
										} else if(completion.name == "+=") {
											node.setType = SetType.Add;
										} else if(completion.name == "-=") {
											node.setType = SetType.Subtract;
										} else if(completion.name == "/=") {
											node.setType = SetType.Divide;
										} else if(completion.name == "*=") {
											node.setType = SetType.Multiply;
										} else if(completion.name == "%=") {
											node.setType = SetType.Modulo;
										}
										result = node;
									});
								break;
							case "||":
								NodeEditorUtility.AddNewNode<Nodes.MultiORNode>(editorData,
									graphPosition,
									(node) => {
										if(completion.genericCompletions != null && completion.genericCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.genericCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targets[0] = member;
											}
										}
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targets[1] = member;
											}
										}
										result = node;
									});
								break;
							case "&&":
								NodeEditorUtility.AddNewNode<Nodes.MultiANDNode>(editorData,
									graphPosition,
									(node) => {
										if(completion.genericCompletions != null && completion.genericCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.genericCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targets[0] = member;
											}
										}
										if(completion.parameterCompletions != null && completion.parameterCompletions.Count > 0) {
											var member = CompletionEvaluator.CompletionsToMemberData(
												completion.parameterCompletions,
												editorData,
												graphPosition);
											if(member != null) {
												node.targets[1] = member;
											}
										}
										result = node;
									});
								break;
							default:
								throw new System.Exception("Unsupported symbol:" + completion.name);
						}
					}
					return result;
				};
			}
			#endregion

			EditorReflectionUtility.GetNamespaces();
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
			EditorApplication.quitting += OnUnityQuitting;

			uNodeThreadUtility.ExecuteAfter(5, () => {
				XmlDoc.LoadDocInBackground();
			});

			Update();
		}

		private static void OnBeforeAssemblyReload() {
			GenerationUtility.SaveData();
			uNodeEditor.OnCompiling();
		}

		//[InitializeOnLoad]
		//class DomainReloadCallback : ScriptableObject {
		//	static DomainReloadCallback() {
		//		uNodeThreadUtility.Queue(() => {
		//			s_Instance = CreateInstance<DomainReloadCallback>();
		//		});
		//	}

		//	private static DomainReloadCallback s_Instance;
		//	private void OnDisable() {
		//		OnBeforeAssemblyReload();
		//	}
		//}

		private static void OnUnityQuitting() {
			GenerationUtility.SaveData();
		}

		static void OnAfterAssemblyReload() {
			uNodeThreadUtility.ExecuteAfter(1, OnSelectionChanged);
		}

		static MethodInfo GetInspectors;
		static PropertyInfo GetTracker;
		static System.Action resetInspector;

		static void OnSelectionChanged() {
			if(GetInspectors == null) {
				var inspector = "UnityEditor.InspectorWindow".ToType(false);
				if(inspector != null) {
					GetInspectors = inspector.GetMethod("GetInspectors", MemberData.flags);
					GetTracker = inspector.GetProperty("tracker", MemberData.flags);
				}
			}
			if(GetInspectors != null) {
				if(resetInspector != null) {
					resetInspector();
					resetInspector = null;
				}
				IList inspectors = GetInspectors.Invoke(null, null) as IList;
				if(inspectors != null) {
					foreach(var obj in inspectors) {
						EditorWindow inspector = obj as EditorWindow;
						ActiveEditorTracker tracker = GetTracker.GetValue(inspector) as ActiveEditorTracker;
						if(tracker != null && tracker.activeEditors.Length == 0)
							continue;
						if(tracker.activeEditors[0].targets.Length != 1)
							continue;
						GameObject go = tracker.activeEditors[0].target as GameObject;
						if(go == null && tracker.activeEditors[0].GetType().FullName == "UnityEditor.PrefabImporterEditor")
							go = tracker.activeEditors[1].target as GameObject;
						if(go == null)
							continue;
						var graph = go.GetComponent<uNodeComponentSystem>();
						if(graph != null && !(graph is IRuntimeGraph)) {
							var vs = inspector.rootVisualElement.Q("unity-content-container");
							if(vs != null && vs.childCount >= 2) {
								int index = 0;
								foreach(var child in vs.ElementAt(0).Children()) {
									if(index >= tracker.activeEditors.Length)
										break;
									var comp = tracker.activeEditors[index].target as uNodeComponentSystem;
									index++;
									if(comp != null)
										continue;
									child.SetDisplay(DisplayStyle.None);
								}
								var inspectorAddComponent = vs.ElementAt(1);
								inspectorAddComponent.SetDisplay(DisplayStyle.None);
								resetInspector += () => inspectorAddComponent.SetDisplay(DisplayStyle.Flex);
							}
						}
					}
				}
			}
		}

		static int refreshTime;

		static void Update() {
			#region Startup
			if(WelcomeWindow.IsShowOnStartup && EditorApplication.timeSinceStartup < 30) {
				WelcomeWindow.ShowWindow();
			}
			#endregion
			uNodeUtility.preferredDisplay = uNodePreference.GetPreference().displayKind;
			if(System.DateTime.Now.Second > refreshTime || refreshTime > 60 && refreshTime - 60 >= System.DateTime.Now.Second) {
				UpdateMarkedObject();
				refreshTime = System.DateTime.Now.Second + 4;
			}
			uNodeUtility.isPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
			uNodeThreadUtility.Update();
			//uNodeUtils.frameCount++;
		}

		#region Project & Hierarchy
		static void UpdateMarkedObject() {
			if(uNodeIcon != null) {
				markedAssets.Clear();
				foreach(var guid in assetGUIDs) {
					var path = AssetDatabase.GUIDToAssetPath(guid);
					var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
					if(asset is GameObject) {
						var go = asset as GameObject;
						var comp = go.GetComponent<uNodeRoot>();
						if(comp != null) {
							markedAssets.Add(guid, comp);
						}
					} else if(asset is ICustomIcon) {
						markedAssets.Add(guid, asset);
					}
				}
				var scene = EditorSceneManager.GetActiveScene();
				if(scene != null) {
					var objects = scene.GetRootGameObjects();
					foreach(var obj in objects) {
						FindObjectToMark(obj.transform);
					}
				}
			}
		}

		static void FindObjectToMark(Transform transform) {
			if(transform.GetComponent<IRuntimeClass>() != null) {
				markedObjects.Add(transform.gameObject.GetInstanceID());
			}
			foreach(Transform t in transform) {
				FindObjectToMark(t);
			}
		}

		static uNodeRoot draggedUNODE;
		static void HierarchyItem(int instanceID, Rect selectionRect) {
			//Show uNode Icon
			if(uNodeIcon != null) {
				Rect r = new Rect(selectionRect);
				r.x += r.width - 4;
				//r.x -= 5;
				r.width = 18;

				if(markedObjects.Contains(instanceID)) {
					GUI.Label(r, uNodeIcon);
				}
			}
			HandleDragAndDropEvents();
			//Drag & Drop
			if(Event.current.type == EventType.DragPerform) {
				if(DragAndDrop.objectReferences?.Length == 1) {
					var obj = DragAndDrop.objectReferences[0];
					if(obj is GameObject && uNodeEditorUtility.IsPrefab(obj)) {
						var comp = (obj as GameObject).GetComponent<uNodeRoot>();
						if(comp is uNodeRuntime) {
							//if(EditorUtility.DisplayDialog("", "Do you want to Instantiate the Prefab or Spawn the graph?", "Prefab", "Graph")) {
							//	comp = null;
							//	PrefabUtility.InstantiatePrefab(comp);
							//	Event.current.Use();
							//}
							return;
						}
						if(comp != null && (comp is IClassComponent || comp is IGraphWithUnityEvent)) {
							draggedUNODE = comp;
							DragAndDrop.AcceptDrag();
							Event.current.Use();
							EditorApplication.delayCall += () => {
								if(draggedUNODE != null) {
									var gameObject = new GameObject(draggedUNODE.gameObject.name);
									var spawner = gameObject.AddComponent<uNodeSpawner>();
									spawner.target = draggedUNODE;
									Selection.objects = new Object[] { gameObject };
									draggedUNODE = null;
								}
							};
						}
					}
				}
			}
			if(draggedUNODE != null) {
				if(selectionRect.Contains(Event.current.mousePosition)) {
					var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
					if(gameObject != null) {
						var spawner = gameObject.AddComponent<uNodeSpawner>();
						spawner.target = draggedUNODE;
						Selection.objects = new Object[] { gameObject };
						draggedUNODE = null;
					}
				}
			}
		}

		private static void HandleDragAndDropEvents() {
			if(Event.current.type == EventType.DragUpdated) {
				if(DragAndDrop.objectReferences?.Length == 1) {
					var obj = DragAndDrop.objectReferences[0];
					if(obj is GameObject && uNodeEditorUtility.IsPrefab(obj)) {
						var comp = (obj as GameObject).GetComponent<uNodeRoot>();
						if(comp != null && !(comp is IIndependentGraph)) {
							Event.current.type = EventType.MouseDrag;
							DragAndDrop.PrepareStartDrag();
							DragAndDrop.objectReferences = new Object[0];
							DragAndDrop.StartDrag("Drag uNode");
							Event.current.Use();
						}
					}
				}
			}
		}

		private static void OnSceneGUI(SceneView obj) {
			HandleDragAndDropEvents();
		}

		private static void ProjectItem(string guid, Rect rect) {
			HandleDragAndDropEvents();
			if(uNodeIcon == null)
				return;
			if(!markedAssets.TryGetValue(guid, out var obj)) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
				if(asset is GameObject) {
					var go = asset as GameObject;
					obj = go.GetComponent<uNodeRoot>();
				} else if(asset is ICustomIcon) {
					obj = asset;
				}
				markedAssets[guid] = obj;
			}
			if(obj != null) {
				var isSmall = IsIconSmall(ref rect);
				if(obj is Component) {
					DrawCustomIcon(rect, backgroundIcon, isSmall);
				}
				if(obj is ICustomIcon customIcon) {
					if(customIcon.GetIcon() != null) {
						DrawCustomIcon(rect, customIcon.GetIcon(), isSmall);
					} else if(obj is uNodeInterface) {
						DrawCustomIcon(rect, uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.InterfaceIcon)), isSmall);
					} else {
						DrawCustomIcon(rect, uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.RuntimeTypeIcon)), isSmall);
					}
				} else if(obj is uNodeRuntime) {
					DrawCustomIcon(rect, uNodeIcon, isSmall);
				} else if(obj is IClass) {
					if((obj as IClass).IsStruct) {
						DrawCustomIcon(rect, uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.StructureIcon)), isSmall);
					} else {
						DrawCustomIcon(rect, uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.ClassIcon)), isSmall);
					}
				} else {
					DrawCustomIcon(rect, uNodeIcon, isSmall);
				}
			}
		}

		private static void DrawCustomIcon(Rect rect, Texture texture, bool isSmall) {
			const float LARGE_ICON_SIZE = 128f;
			if(rect.width > LARGE_ICON_SIZE) {
				// center the icon if it is zoomed
				var offset = (rect.width - LARGE_ICON_SIZE) / 2f;
				rect = new Rect(rect.x + offset, rect.y + offset, LARGE_ICON_SIZE, LARGE_ICON_SIZE);
			} else {
				if(isSmall && !IsTreeView(rect))
					rect = new Rect(rect.x + 3, rect.y, rect.width, rect.height);
			}
			GUI.DrawTexture(rect, texture);
		}
		private static bool IsTreeView(Rect rect) {
			return (rect.x - 16) % 14 == 0;
		}

		private static bool IsIconSmall(ref Rect rect) {
			var isSmall = rect.width > rect.height;

			if(isSmall)
				rect.width = rect.height;
			else
				rect.height = rect.width;

			return isSmall;
		}
		#endregion

		private static List<int> _nodeDebugData;
		private static List<int> nodeDebugData {
			get {
				if(_nodeDebugData == null) {
					_nodeDebugData = uNodeEditorUtility.LoadEditorData<List<int>>("BreakpointsMap");
					if(_nodeDebugData == null) {
						_nodeDebugData = new List<int>();
					}
				}
				return _nodeDebugData;
			}
		}

		private static void SaveDebugData() {
			uNodeEditorUtility.SaveEditorData(_nodeDebugData, "BreakpointsMap");
		}

		#region AOT Scans
		public static bool AOTScan(out List<Type> serializedTypes) {
			return AOTScan(out serializedTypes, true, true, true, true, null);
		}

		public static bool AOTScan(out List<Type> serializedTypes, bool scanBuildScenes = true, bool scanAllAssetBundles = true, bool scanPreloadedAssets = true, bool scanResources = true, List<string> resourcesToScan = null) {
			using(AOTSupportScanner aOTSupportScanner = new AOTSupportScanner()) {
				aOTSupportScanner.BeginScan();
				if(scanBuildScenes && !aOTSupportScanner.ScanBuildScenes(includeSceneDependencies: true, showProgressBar: true)) {
					Debug.Log("Project scan canceled while scanning scenes and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if(scanResources && !aOTSupportScanner.ScanAllResources(includeResourceDependencies: true, showProgressBar: true, resourcesToScan)) {
					Debug.Log("Project scan canceled while scanning resources and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if(scanAllAssetBundles && !aOTSupportScanner.ScanAllAssetBundles(showProgressBar: true)) {
					Debug.Log("Project scan canceled while scanning asset bundles and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if(scanPreloadedAssets && !aOTSupportScanner.ScanPreloadedAssets(showProgressBar: true)) {
					Debug.Log("Project scan canceled while scanning preloaded assets and their dependencies.");
					serializedTypes = null;
					return false;
				}
				aOTSupportScanner.GetType().GetField("allowRegisteringScannedTypes", MemberData.flags).SetValueOptimized(aOTSupportScanner, true);
				ScanAOTOnGraphs();
				OnPreprocessBuild();
				serializedTypes = aOTSupportScanner.EndScan();
				for(int i = 0; i < serializedTypes.Count; i++) {
					if(EditorReflectionUtility.IsInEditorAssembly(serializedTypes[i])) {
						serializedTypes.RemoveAt(i);
						i--;
					}
				}
			}
			return true;
		}

		private static void ScanAOTOnGraphs() {
			List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
			objects.AddRange(GraphUtility.FindGraphPrefabs());
			objects.AddRange(uNodeEditorUtility.FindAssetsByType<uNodeInterface>());
			HashSet<Type> serializedTypes = new HashSet<Type>();
			Action<object> analyzer = (param) => {
				AnalizerUtility.AnalizeObject(param, (fieldObj) => {
					if(fieldObj is MemberData member && member.isTargeted) {
						object mVal;
						if(member.targetType.IsTargetingValue()) {
							mVal = member.Get();
						} else {
							mVal = member.instance;
						}
						if(mVal != null && !(mVal is Object) && !(mVal is MemberData) && !serializedTypes.Contains(mVal.GetType())) {
							serializedTypes.Add(mVal.GetType());
							SerializerUtility.Serialize(mVal);
						}
					}
					return false;
				});
			};
			foreach(var obj in objects) {
				if(obj is GameObject) {
					var scripts = (obj as GameObject).GetComponentsInChildren<MonoBehaviour>(true);
					foreach(var script in scripts) {
						if(script is ISerializationCallbackReceiver serialization) {
							serialization.OnBeforeSerialize();
						}
						if(script is IVariableSystem VS && VS.Variables != null) {
							foreach(var var in VS.Variables) {
								var.Serialize();
							}
						}
						if(script is ILocalVariableSystem IVS && IVS.LocalVariables != null) {
							foreach(var var in IVS.LocalVariables) {
								var.Serialize();
							}
						}
						analyzer(script);
					}
				} else if(obj is ISerializationCallbackReceiver) {
					(obj as ISerializationCallbackReceiver).OnBeforeSerialize();
				} else {
					analyzer(obj);
				}
			}
			SerializerUtility.Serialize(new MemberData());
		}
		#endregion

		#region Build Processor
		private static bool isEditorOpen;
		private static bool hasRunPreBuild;

		public static void OnPreprocessBuild() {
			if(hasRunPreBuild)
				return;
			hasRunPreBuild = true;
			if(uNodePreference.preferenceData.generatorData.autoGenerateOnBuild) {
				GenerationUtility.CompileProjectGraphs();
				while(uNodeThreadUtility.IsNeedUpdate()) {
					uNodeThreadUtility.Update();
				}
				if(uNodeEditor.window != null) {
					uNodeEditor.window.Close();
					isEditorOpen = true;
				}
				GraphUtility.SaveAllGraph();
			}
		}

		public static void OnPostprocessBuild() {
			if(isEditorOpen) {
				uNodeThreadUtility.ExecuteAfter(5, () => {
					uNodeEditor.ShowWindow();
				});
				isEditorOpen = false;
			}
			hasRunPreBuild = false;
		}
		#endregion
	}

	static class uNodeAssetHandler {
		[OnOpenAsset(int.MinValue)]
		public static bool OpenEditor(int instanceID, int line) {
			Object obj = EditorUtility.InstanceIDToObject(instanceID);
			if(obj is GameObject) {
				GameObject go = obj as GameObject;
				uNodeRoot root = go.GetComponent<uNodeRoot>();
				if(root != null && !(root is uNodeRuntime)) {
					uNodeEditor.Open(root);
					return true; //comment this to allow editing prefab.
				} else {
					uNodeData data = go.GetComponent<uNodeData>();
					if(data != null) {
						uNodeEditor.Open(data);
						return true; //comment this to allow editing prefab.
					}
				}
			}
			return false;
		}
	}

	class uNodeAssetModificationPreprocessor : UnityEditor.AssetModificationProcessor {
		static HashSet<string> removePaths = new HashSet<string>();
		static bool containsUNODE = false;
		private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options) {
			if(path.EndsWith(".prefab")) {
				var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
				if(obj != null && obj is GameObject) {
					if(GraphUtility.HasTempGraphObject(obj as GameObject)) {
						GraphUtility.DestroyTempGraphObject(obj as GameObject);
					}
				}
			} else if(path.StartsWith(uNodeEditorUtility.GetUNodePath() + "/", StringComparison.Ordinal) || path == uNodeEditorUtility.GetUNodePath()) {
				if(GraphUtility.GetTempManager() != null) {
					containsUNODE = true;
				}
			}
			removePaths.Add(path);
			uNodeThreadUtility.ExecuteOnce(() => {
				if(containsUNODE) {
					containsUNODE = false;
					//Close the uNode Editor window
					uNodeEditor.window?.Close();
					//Save all graphs and remove all root graphs.
					GraphUtility.SaveAllGraph();
					//Save all dirty assets
					AssetDatabase.SaveAssets();
				}
				uNodeThreadUtility.Queue(() => {
					EditorUtility.DisplayProgressBar("Deleting Files", "", 0);
					foreach(var p in removePaths) {
						if(Directory.Exists(p)) {
							ForceDeleteDirectory(p);
						} else if(File.Exists(p)) {
							if(AssetDatabase.IsMainAssetAtPathLoaded(p)) {
								var asset = AssetDatabase.LoadMainAssetAtPath(p);
								if(!(asset is GameObject || asset is Component)) {
									Resources.UnloadAsset(asset);
								}
							}
							new FileStream(p, FileMode.Open).Dispose();
							File.Delete(p);
						} else {
							continue;
						}
						var metaPath = p + ".meta";
						if(File.Exists(metaPath)) {
							File.Delete(metaPath);
						}
					}
					EditorUtility.ClearProgressBar();
					removePaths.Clear();
					uNodeThreadUtility.Queue(() => {
						AssetDatabase.Refresh();
					});
				});
			}, "[UNODE_DELETE]");
			return AssetDeleteResult.DidDelete;
		}

		static void ForceDeleteDirectory(string path) {
			var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };
			foreach(var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories)) {
				info.Attributes = FileAttributes.Normal;
				if(info is FileInfo fi) {
					var p = fi.FullName.Remove(0, Application.dataPath.Length + 1).Replace('\\', '/');
					if(!p.EndsWith(".meta") && AssetDatabase.IsMainAssetAtPathLoaded(p)) {
						Resources.UnloadAsset(AssetDatabase.LoadMainAssetAtPath(p));
					}
					fi.Create().Dispose();
					fi.Delete();
				}
			}
			directory.Delete(true);
		}

		//private static void OnWillCreateAsset(string assetName) {
		//	if(assetName.EndsWith(".prefab") || assetName.EndsWith(".prefab.meta")) {
		//		CachingUtility.MarkDirtyGraph();
		//	}
		//}
	}

	class uNodeAssetPostprocessor : AssetPostprocessor {
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach(var path in importedAssets) {
				if(path.EndsWith(".prefab")) {
					var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
					if(obj != null) {
						if(obj is GameObject go && go.GetComponent<uNodeComponentSystem>() != null) {
							CachingUtility.MarkDirtyGraph();
							break;
						}
					}
				}
			}
			if(movedAssets.Length == movedFromAssetPaths.Length) {
				bool flag = false;
				for(int i = 0; i < movedAssets.Length; i++) {
					var path = movedAssets[i];
					if(path.EndsWith(".prefab")) {
						var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
						if(obj != null) {
							if(obj is GameObject go && go.GetComponent<uNodeComponentSystem>() != null) {
								if(Path.GetFileName(movedFromAssetPaths[i]) != Path.GetFileName(path)) {//If user rename an asset
									var name = go.name;
									foreach(var comp in go.GetComponents<uNodeRoot>()) {
										comp.graphData.fileName = name;
									}
									if(GraphUtility.HasTempGraphObject(go)) {
										var tempGraph = GraphUtility.GetTempGraphObject(go);
										foreach(var comp in tempGraph.GetComponents<uNodeRoot>()) {
											comp.graphData.fileName = name;
										}
										uNodeGUIUtility.GUIChanged(tempGraph);
									}
									EditorUtility.SetDirty(go);
									PrefabUtility.SavePrefabAsset(go);
								}
								uNodeGUIUtility.GUIChanged(go);
							}
						}
					}
				}
				if(flag) {
					UGraphView.ClearCache();
					uNodeEditor.window?.Refresh();
					CachingUtility.MarkDirtyGraph();
				}
			}
			//Alternative way of manage deleted assets
			//foreach(var path in deletedAssets) {
			//	if(path.EndsWith(".prefab")) {
			//		var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
			//		if(obj != null && obj is GameObject) {
			//			if(GraphUtility.HasTempGraphObject(obj as GameObject)) {
			//				GraphUtility.DestroyTempGraphObject(obj as GameObject);
			//			}
			//		}
			//	} else if(path.StartsWith(uNodeEditorUtility.GetUNodePath() + "/", StringComparison.Ordinal) || path == uNodeEditorUtility.GetUNodePath()) {
			//		if(GraphUtility.GetTempManager() != null) {
			//			//Close the uNode Editor window
			//			uNodeEditor.window?.Close();
			//			//Save all graphs and remove all root graphs.
			//			GraphUtility.SaveAllGraph();
			//			//Save all dirty assets
			//			AssetDatabase.SaveAssets();
			//			break;
			//		}
			//	}
			//}
		}
	}
}