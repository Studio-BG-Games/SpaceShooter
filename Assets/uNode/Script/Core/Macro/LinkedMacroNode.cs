using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class LinkedMacroNode : CustomNode, IMacro, IVariableSystem, IRefreshable {
		public uNodeMacro macroAsset;

		[HideInInspector]
		public List<VariableData> variables = new List<VariableData>();

		[HideInInspector]
		public List<MacroPortNode> inputFlows = new List<MacroPortNode>();
		[HideInInspector]
		public List<MacroPortNode> inputValues = new List<MacroPortNode>();
		[HideInInspector]
		public List<MacroPortNode> outputFlows = new List<MacroPortNode>();
		[HideInInspector]
		public List<MacroPortNode> outputValues = new List<MacroPortNode>();

		[Hide]
		public GameObject pinObject;

		public List<VariableData> Variables {
			get {
				return variables;
			}
		}

		public VariableData GetVariableData(string name) {
			if(CG.isGenerating) {
				return uNodeUtility.GetVariableData(name, variables) ?? uNodeUtility.GetVariableData(name, m_generatedVariable);
			}
			return uNodeUtility.GetVariableData(name, variables);
		}

		public void SetVariableValue(string name, object value) {
			GetVariableData(name).Set(value);
		}

		public object GetVariableValue(string name) {
			return GetVariableData(name).Get();
		}

		List<MacroPortNode> IMacro.InputFlows => inputFlows;
		List<MacroPortNode> IMacro.InputValues => inputValues;
		List<MacroPortNode> IMacro.OutputFlows => outputFlows;
		List<MacroPortNode> IMacro.OutputValues => outputValues;

		[System.NonSerialized]
		public uNodeMacro runtimeMacro;
		private IMacroPort m_port;
		private List<VariableData> m_generatedVariable;
		public void InitMacroPort(IMacroPort port) {
			if(port == null)
				return;
			if(m_port == null) {
				m_port = port;
			}
			if(m_port == port) {// Make sure to init macro only one times.
				Refresh();
				if(CG.isGenerating) {
					foreach(VariableData variable in variables) {
						variable.modifier.SetPrivate();
						CG.RegisterVariable(variable);
					}
					m_generatedVariable = new List<VariableData>();
					//Ensure to register default variable from macro graph
					var macroVariable = macroAsset.Variables;
					foreach(var variable in macroVariable) {
						if(!variables.Any(v => v.Name == variable.Name)) {
							var tmpVar = new VariableData(variable);
							tmpVar.modifier.SetPrivate();
							CG.RegisterVariable(tmpVar);
							m_generatedVariable.Add(tmpVar);
						}
					}
				}
				if(macroAsset != null) {
					if(CG.isGenerating || Application.isPlaying) {
						if(runtimeMacro != null) {
							DestroyImmediate(runtimeMacro.gameObject);
						}
						runtimeMacro = uNodeUtility.TempManagement.CreateTempObject(macroAsset);
						var behaviors = runtimeMacro.RootObject.GetComponentsInChildren<MonoBehaviour>(true);
						AnalizerUtility.RetargetNodeOwner(runtimeMacro, owner, behaviors, (obj) => {
							MemberData member = obj as MemberData;
							if(member != null && member.targetType == MemberData.TargetType.uNodeVariable && GetVariableData(member.startName) != null) {
								member.RefactorUnityObject(new Object[] { runtimeMacro }, new Object[] { this });
							}
						});
						Dictionary<int, MacroPortNode> portMap = new Dictionary<int, MacroPortNode>();
						foreach(var p in inputFlows) {
							if(p != null && p.linkedPort != null) {
								portMap[p.linkedPort.port.transform.GetSiblingIndex()] = p;
							}
						}
						foreach(var p in inputValues) {
							if(p != null && p.linkedPort != null) {
								portMap[p.linkedPort.port.transform.GetSiblingIndex()] = p;
							}
						}
						foreach(var p in outputFlows) {
							if(p != null && p.linkedPort != null) {
								portMap[p.linkedPort.port.transform.GetSiblingIndex()] = p;
							}
						}
						foreach(var p in outputValues) {
							if(p != null && p.linkedPort != null) {
								portMap[p.linkedPort.port.transform.GetSiblingIndex()] = p;
							}
						}
						foreach(Transform t in runtimeMacro.RootObject.transform) {
							var comp = t.GetComponent<Node>();
							if(comp != null) {
								comp.parentComponent = this;
								var p = comp as MacroPortNode;
								MacroPortNode originP;
								if(portMap.TryGetValue(t.GetSiblingIndex(), out originP)) {
									switch(originP.kind) {
										case PortKind.FlowInput:
										case PortKind.ValueOutput:
											originP.target = new MemberData(p.target);
											break;
										case PortKind.FlowOutput:
										case PortKind.ValueInput:
											p.target = new MemberData(originP.target);
											break;
									}
								}
							}
						}
						runtimeMacro.SetLinkedOwner(owner); //Make sure all get/set property will be liked to actual owner.
						if(CG.isGenerating) {
							const string key = "[MACROS-MAP]";
							var map = CG.GetUserObject<Dictionary<uNodeMacro, bool>>(key);
							if(map == null) {
								map = new Dictionary<uNodeMacro, bool>();
								CG.RegisterUserObject(map, key);
							}
							var macro = uNodeUtility.FindParentComponent<uNodeMacro>(transform);
							bool isInState;
							if(macro != null) {
								map.TryGetValue(macro, out isInState);
							} else {
								isInState = CG.IsInStateGraph(this);
								map[runtimeMacro] = isInState;
							}
							foreach(var b in behaviors) {
								if(b is NodeComponent nodeComponent) {
									nodeComponent.OnGeneratorInitialize();
									if(isInState) {
										CG.RegisterAsStateNode(nodeComponent);
									} else {
										CG.RegisterAsRegularNode(nodeComponent);
									}
								}
							}
						} else {
							foreach(var b in behaviors) {
								if(b is NodeComponent nodeComponent) {
									nodeComponent.OnGeneratorInitialize();
								}
							}
						}
					}
				}
			}
		}

		public void Refresh() {
			if(macroAsset != null) {
				for(int i = 0; i < variables.Count; i++) {
					bool flag = false;
					foreach(var mVar in macroAsset.Variables) {
						if(mVar.Name.Equals(variables[i].Name)) {
							flag = true;
						}
					}
					if(!flag) {
						variables.RemoveAt(i);
						i--;
					}
				}
				for(int i = 0; i < inputFlows.Count; i++) {
					var p = inputFlows[i];
					if(p != null && p.linkedPort != null && p.linkedPort.owner != macroAsset) {
						DestroyImmediate(p);
					}
				}
				for(int i = 0; i < inputValues.Count; i++) {
					var p = inputValues[i];
					if(p != null && p.linkedPort != null && p.linkedPort.owner != macroAsset) {
						DestroyImmediate(p);
					}
				}
				for(int i = 0; i < outputFlows.Count; i++) {
					var p = outputFlows[i];
					if(p != null && p.linkedPort != null && p.linkedPort.owner != macroAsset) {
						DestroyImmediate(p);
					}
				}
				for(int i = 0; i < outputValues.Count; i++) {
					var p = outputValues[i];
					if(p != null && p.linkedPort != null && p.linkedPort.owner != macroAsset) {
						DestroyImmediate(p);
					}
				}
				inputFlows.Clear();
				inputValues.Clear();
				outputFlows.Clear();
				outputValues.Clear();
				if(pinObject != null) {
					var macros = pinObject.GetComponents<MacroPortNode>();
					var map = new Dictionary<MacroPortNode, MacroPortNode>();
					foreach(var m in macros) {
						if(m.linkedPort == null || m.linkedPort.port == null)
							continue;
						if(map.ContainsKey(m.linkedPort.port)) {
							//Removes duplicated port
							DestroyImmediate(m);
							continue;
						}
						map.Add(m.linkedPort.port, m);
					}
					foreach(var p in macroAsset.inputFlows) {
						if(p == null)
							continue;
						MacroPortNode pin;
						if(!map.TryGetValue(p, out pin)) {
							pin = pinObject.AddComponent<MacroPortNode>();
						}
						pin.type = new MemberData(p.type);
						pin.kind = p.kind;
						pin.Name = p.GetName();
						if(string.IsNullOrEmpty(pin.Name)) {
							pin.Name = " ";
						}
						pin.linkedPort = new MacroPortNode.LinkedPort() {
							guid = macroAsset.GetPortGuid(p),
							owner = macroAsset,
						};
						pin.owner = owner;
						inputFlows.Add(pin);
					}
					foreach(var p in macroAsset.inputValues) {
						if(p == null)
							continue;
						MacroPortNode pin;
						if(!map.TryGetValue(p, out pin)) {
							pin = pinObject.AddComponent<MacroPortNode>();
						}
						pin.type = new MemberData(p.type);
						pin.kind = p.kind;
						pin.Name = p.GetName();
						if(string.IsNullOrEmpty(pin.Name)) {
							pin.Name = " ";
						}
						pin.linkedPort = new MacroPortNode.LinkedPort() {
							guid = macroAsset.GetPortGuid(p),
							owner = macroAsset,
						};
						pin.owner = owner;
						inputValues.Add(pin);
					}
					foreach(var p in macroAsset.outputFlows) {
						if(p == null)
							continue;
						MacroPortNode pin;
						if(!map.TryGetValue(p, out pin)) {
							pin = pinObject.AddComponent<MacroPortNode>();
						}
						pin.type = new MemberData(p.type);
						pin.kind = p.kind;
						pin.Name = p.GetName();
						if(string.IsNullOrEmpty(pin.Name)) {
							pin.Name = " ";
						}
						pin.linkedPort = new MacroPortNode.LinkedPort() {
							guid = macroAsset.GetPortGuid(p),
							owner = macroAsset,
						};
						pin.owner = owner;
						outputFlows.Add(pin);
					}
					foreach(var p in macroAsset.outputValues) {
						if(p == null)
							continue;
						MacroPortNode pin;
						if(!map.TryGetValue(p, out pin)) {
							pin = pinObject.AddComponent<MacroPortNode>();
						}
						pin.type = new MemberData(p.type);
						pin.kind = p.kind;
						pin.Name = p.GetName();
						if(string.IsNullOrEmpty(pin.Name)) {
							pin.Name = " ";
						}
						pin.linkedPort = new MacroPortNode.LinkedPort() {
							guid = macroAsset.GetPortGuid(p),
							owner = macroAsset,
						};
						pin.owner = owner;
						outputValues.Add(pin);
					}
				}
			}
		}

		public override System.Type GetNodeIcon() {
			return null;
		}

		public override string GetNodeName() {
			if(macroAsset != null) {
				return macroAsset.DisplayName;
			}
			return gameObject.name;
		}

		public override bool IsSelfCoroutine() {
			if(macroAsset != null) {
				return macroAsset.HasCoroutineNode;
			}
			return false;
		}
	}
}