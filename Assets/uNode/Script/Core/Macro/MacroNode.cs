using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode.Nodes {
	//[NodeMenu("Flow", "Macro")]
	[AddComponentMenu("")]
	public class MacroNode : CustomNode, IMacro, ISuperNode, IVariableSystem {
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

		public List<VariableData> Variables {
			get {
				return variables;
			}
		}
		public IList<NodeComponent> nestedFlowNodes {
			get {
				Refresh();
				List<NodeComponent> macros = new List<NodeComponent>();
				foreach(var n in inputFlows) {
					macros.Add(n);
				}
				//macros.AddRange(outputFlows);
				//macros.AddRange(inputValues);
				//macros.AddRange(outputValues);
				return macros;
			}
		}

		List<MacroPortNode> IMacro.InputFlows => inputFlows;
		List<MacroPortNode> IMacro.InputValues => inputValues;
		List<MacroPortNode> IMacro.OutputFlows => outputFlows;
		List<MacroPortNode> IMacro.OutputValues => outputValues;

		private IMacroPort m_port;
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
				}
			}
		}

		public void Refresh() {
			inputFlows.RemoveAll(m => m == null);
			inputValues.RemoveAll(m => m == null);
			outputFlows.RemoveAll(m => m == null);
			outputValues.RemoveAll(m => m == null);
			foreach(Transform t in transform) {
				var node = t.GetComponent<Node>();
				if(node is MacroPortNode) {
					MacroPortNode macro = node as MacroPortNode;
					switch(macro.kind) {
						case PortKind.FlowInput:
							if(!inputFlows.Contains(macro)) {
								inputFlows.Add(macro);
							}
							outputFlows.Remove(macro);
							inputValues.Remove(macro);
							outputValues.Remove(macro);
							break;
						case PortKind.FlowOutput:
							if(!outputFlows.Contains(macro)) {
								outputFlows.Add(macro);
							}
							inputFlows.Remove(macro);
							inputValues.Remove(macro);
							outputValues.Remove(macro);
							break;
						case PortKind.ValueInput:
							if(!inputValues.Contains(macro)) {
								inputValues.Add(macro);
							}
							inputFlows.Remove(macro);
							outputFlows.Remove(macro);
							outputValues.Remove(macro);
							break;
						case PortKind.ValueOutput:
							if(!outputValues.Contains(macro)) {
								outputValues.Add(macro);
							}
							inputFlows.Remove(macro);
							outputFlows.Remove(macro);
							inputValues.Remove(macro);
							break;
					}
				}
			}
		}

		public override Type GetNodeIcon() {
			return typeof(TypeIcons.StateIcon);
		}

		public override string GetNodeName() {
			return gameObject.name;
		}

		public VariableData GetVariableData(string name) {
			return uNodeUtility.GetVariableData(name, variables);
		}

		public void SetVariableValue(string name, object value) {
			GetVariableData(name).Set(value);
		}

		public object GetVariableValue(string name) {
			return GetVariableData(name).Get();
		}

		public bool AcceptCoroutine() {
			if(parentComponent == null) {
				return true;
			} else {
				var pComp = parentComponent;
				if(pComp as RootObject) {
					return (pComp as RootObject).CanHaveCoroutine();
				} else if(pComp is StateNode) {
					return true;
				} else if(pComp is ISuperNode) {
					return (pComp as ISuperNode).AcceptCoroutine();
				}
			}
			return false;
		}

		public override bool IsCoroutine() {
			foreach(var n in inputFlows) {
				if(n != null && n.IsCoroutine()) {
					return true;
				}
			}
			return false;
		}
	}
}