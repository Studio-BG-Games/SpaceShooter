using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu("")]
	public class DummyNode : Node, IExtendedOutput, IExtendedInput {
		[Serializable]
		public class PortData {
			public string name;
			public bool isFlow;
			public MemberData value;
			public SerializedType type = new SerializedType();
		}

		[HideInInspector]
		public string title;
		[HideInInspector]
		public string errorMessage;
		[HideInInspector]
		public List<PortData> inputPorts = new List<PortData>();
		[HideInInspector]
		public List<PortData> outputPorts = new List<PortData>();

		[NonSerialized]
		private List<PortData> _inputValuePorts;
		public List<PortData> inputValuePorts {
			get {
				if(_inputValuePorts == null) {
					_inputValuePorts = new List<PortData>();
					foreach(var port in inputPorts) {
						if(!port.isFlow) {
							_inputValuePorts.Add(port);
						}
					}
				}
				return _inputValuePorts;
			}
		}

		[NonSerialized]
		private List<PortData> _inputFlowPorts;
		public List<PortData> inputFlowPorts {
			get {
				if(_inputFlowPorts == null) {
					_inputFlowPorts = new List<PortData>();
					foreach(var port in inputPorts) {
						if(port.isFlow) {
							_inputFlowPorts.Add(port);
						}
					}
				}
				return _inputFlowPorts;
			}
		}

		[NonSerialized]
		private List<PortData> _outputValuePorts;
		public List<PortData> outputValuePorts {
			get {
				if(_outputValuePorts == null) {
					_outputValuePorts = new List<PortData>();
					foreach(var port in outputPorts) {
						if(!port.isFlow) {
							_outputValuePorts.Add(port);
						}
					}
				}
				return _outputValuePorts;
			}
		}

		[NonSerialized]
		private List<PortData> _outputFlowPorts;
		public List<PortData> outputFlowPorts {
			get {
				if(_outputFlowPorts == null) {
					_outputFlowPorts = new List<PortData>();
					foreach(var port in outputPorts) {
						if(!port.isFlow) {
							_outputFlowPorts.Add(port);
						}
					}
				}
				return _outputFlowPorts;
			}
		}

		public override void CheckError() {
			if(!string.IsNullOrEmpty(errorMessage)) {
				RegisterEditorError(errorMessage + "\nYou need to replace into correct node in order to work.");
			}
		}

		public override string GetNodeName() {
			return title;
		}

		int IExtendedOutput.OutputCount => outputValuePorts.Count;
		int IExtendedInput.InputCount => inputFlowPorts.Count;

		string IExtendedOutput.GetOutputName(int index) {
			if(outputValuePorts.Count < index)
				return string.Empty;
			return outputValuePorts[index].name;
		}

		Type IExtendedOutput.GetOutputType(string name) {
			for(int i = 0; i < outputValuePorts.Count; i++) {
				if(name == outputValuePorts[i].name) {
					return outputValuePorts[i].type?.type ?? typeof(object);
				}
			}
			return typeof(object);
		}

		string IExtendedInput.GetInputName(int index) {
			if(inputFlowPorts.Count < index)
				return string.Empty;
			return inputFlowPorts[index].name;
		}

		#region Exceptions
		private Exception exception => new System.Exception("Please replace / remove this node to actual node.");

		object IExtendedOutput.GetOutputValue(string name) {
			throw exception;
		}

		string IExtendedOutput.GenerateOutputCode(string name) {
			throw exception;
		}

		void IExtendedInput.InvokeFlowInput(string name) {
			throw exception;
		}

		string IExtendedInput.GenerateInputCode(string name) {
			throw exception;
		}

		public override void OnExecute() {
			throw exception;
		}

		public override string GenerateCode() {
			throw exception;
		}

		public override bool IsFlowNode() {
			return false;
		}
		#endregion
	}
}