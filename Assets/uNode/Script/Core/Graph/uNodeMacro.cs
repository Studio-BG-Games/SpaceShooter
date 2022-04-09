using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	[GraphSystem("Macro", supportAttribute = false, supportConstructor = false, supportGeneric = false, supportModifier = false, supportProperty = false, allowCreateInScene = false, allowPreviewScript = false, allowAutoCompile = false, allowCompileToScript = false)]
	public class uNodeMacro : uNodeRoot, IMacroGraph {
		public string category = "Macro";
		[HideInInspector]
		public List<VariableData> variables = new List<VariableData>();
		[HideInInspector]
		public List<string> usingNamespaces = new List<string>() { "UnityEngine", "System.Collections.Generic" };

		[HideInInspector]
		public List<MacroPortNode> inputFlows = new List<MacroPortNode>();
		[HideInInspector]
		public List<MacroPortNode> inputValues = new List<MacroPortNode>();
		[HideInInspector]
		public List<MacroPortNode> outputFlows = new List<MacroPortNode>();
		[HideInInspector]
		public List<MacroPortNode> outputValues = new List<MacroPortNode>();

		[SerializeField, HideInInspector]
		private bool hasCoroutineNodes;

		public override string Namespace => "MaxyGames.Generated";
		List<string> IIndependentGraph.UsingNamespaces { get => usingNamespaces; set => usingNamespaces = value; }
		
		public override string GraphName {
			get {
				if(string.IsNullOrEmpty(Name)) {
					return gameObject.name;
				}
				return Name;
			}
		}
		
		public override List<VariableData> Variables {
			get {
				if(linkedOwner != null) {
					return linkedOwner.Variables;
				}
				return variables;
			}
		}
		public override IList<uNodeProperty> Properties {
			get {
				if(linkedOwner != null) {
					return linkedOwner.Properties;
				}
				return new uNodeProperty[0];
			}
		}
		public override IList<uNodeFunction> Functions {
			get {
				if(linkedOwner != null) {
					return linkedOwner.Functions;
				}
				return new uNodeFunction[0];
			}
		}
		public override IList<uNodeConstuctor> Constuctors {
			get {
				if(linkedOwner != null) {
					return linkedOwner.Constuctors;
				}
				return new uNodeConstuctor[0];
			}
		}

		/// <summary>
		/// Return true if the macro containing coroutine node.
		/// </summary>
		public bool HasCoroutineNode {
			get {
				return hasCoroutineNodes;
			}
		}

		#region Port Guid
		[Serializable]
		public class MacroPortID {
			public string guid;
			public MacroPortNode port;
		}
		[HideInInspector]
		public List<MacroPortID> portGuids = new List<MacroPortID>();

		public MacroPortNode GetPortByGuid(string guid) {
			foreach(var p in portGuids) {
				if(p.guid.Equals(guid)) {
					return p.port;
				}
			}
			return null;
		}

		public string GetPortGuid(MacroPortNode port) {
			foreach(var p in portGuids) {
				if(p.port == port) {
					return p.guid;
				}
			}
			return "";
		}
		#endregion

		private uNodeRoot linkedOwner;
		public void SetLinkedOwner(uNodeRoot owner) {
			linkedOwner = owner;
		}

		//public void Initialize() {
		//	if(nodes != null) {
		//		for(int i=0;i< nodes.Length;i++) {
		//			nodes[i].RegisterPort();
		//		}
		//	}
		//}

		public override void Refresh() {
			base.Refresh();
			inputFlows.RemoveAll(m => m == null);
			inputValues.RemoveAll(m => m == null);
			outputFlows.RemoveAll(m => m == null);
			outputValues.RemoveAll(m => m == null);

			if(RootObject != null) {
				if(nodes != null) {
					hasCoroutineNodes = false;
					for(int i = 0; i < nodes.Length; i++) {
						if(nodes[i].IsSelfCoroutine()) {
							hasCoroutineNodes = true;
							break;
						}
					}
				}
				foreach(Transform t in RootObject.transform) {
					var node = t.GetComponent<Node>();
					if(node is MacroPortNode) {
						MacroPortNode macro = node as MacroPortNode;
						{//Guid Creations.
							bool flag = true;
							foreach(var p in portGuids) {
								if(macro == p.port) {
									flag = false;
									break;
								}
							}
							if(flag) {
								portGuids.Add(new MacroPortID() { guid = Guid.NewGuid().ToString(), port = macro });
							}
						}
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
		}

		public override uNodeFunction GetFunction(string name, params Type[] parameters) {
			if(linkedOwner != null) {
				return linkedOwner.GetFunction(name, parameters);
			}
			return null;
		}

		public override uNodeFunction GetFunction(string name, int genericParameterLength, params Type[] parameters) {
			if(linkedOwner != null) {
				return linkedOwner.GetFunction(name, genericParameterLength, parameters);
			}
			return null;
		}

		public override Type GetInheritType() {
			return typeof(object);
		}

		public override uNodeProperty GetPropertyData(string name) {
			if(linkedOwner != null) {
				return linkedOwner.GetPropertyData(name);
			}
			return null;
		}
		
		public override VariableData GetVariableData(string name) {
			if(linkedOwner != null) {
				return linkedOwner.GetVariableData(name);
			}
			return uNodeUtility.GetVariableData(name, variables);
		}
	}
}