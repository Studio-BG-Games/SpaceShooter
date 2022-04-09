using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;

namespace MaxyGames.uNode {
	public interface INodeComponent { }
	
	public interface INamespaceSystem {
		IEnumerable<string> GetNamespaces();
	}

	/// <summary>
	/// Interface for extending node input flow ports and are useful for dynamic input ports.
	/// Note: still not fully implemented and only used in dummy nodes to handle errors when parsing UVS graph.
	/// </summary>
	public interface IExtendedInput {
		/// <summary>
		/// Invoke the flow input ports
		/// </summary>
		/// <param name="name"></param>
		void InvokeFlowInput(string name);

		#region Editors
		/// <summary>
		/// The number of extended inputs
		/// </summary>
		int InputCount { get; }
		/// <summary>
		/// The name of input
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		string GetInputName(int index);
		/// <summary>
		/// Use for generating the input code
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		string GenerateInputCode(string name);
		#endregion
	}

	/// <summary>
	/// Interface for extending node output value ports and are useful for dynamic output ports.
	/// </summary>
	public interface IExtendedOutput {
		/// <summary>
		/// Get the value of the output
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		object GetOutputValue(string name);

		#region Editors
		/// <summary>
		/// The number of extended outputs
		/// </summary>
		int OutputCount { get; }
		/// <summary>
		/// The name of output
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		string GetOutputName(int index);
		/// <summary>
		/// Get the output type
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Type GetOutputType(string name);
		/// <summary>
		/// Use for generating the output code
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		string GenerateOutputCode(string name);
		#endregion
	}

	/// <summary>
	/// A graph that generate c# classes or struct
	/// </summary>
	public interface IClass {
		bool IsStruct { get; }
		System.Type GetInheritType();
	}

	/// <summary>
	/// The object with custom icon
	/// </summary>
	public interface ICustomIcon {
		Texture GetIcon();
	}
	
	/// <summary>
	/// Interface for custom icon for nodes
	/// </summary>
	public interface IIcon {
		System.Type GetIcon();
	}

	/// <summary>
	/// A base member for Runtime Type, Field, Property, Parameter and Method
	/// </summary>
	public interface IRuntimeMember {
		
	}

	/// <summary>
	/// A interface for all Fake Type, Field, Property, Parameter and Method
	/// </summary>
	public interface IFakeMember {

	}

	/// <summary>
	/// A independent graph that have its own namespace and using namespace data
	/// </summary>
	public interface IIndependentGraph {
		string Namespace { get; }
		List<string> UsingNamespaces { get; set; }
	}

	/// <summary>
	/// A runtime graph that can be referenced without using instance like static class in C#
	/// </summary>
	public interface ISingletonGraph : IRuntimeComponent {
		bool IsPersistence { get; }
	}

	public interface INodeRoot {
		System.Type GetInheritType();
	}

	public interface IValue<T> : IGetValue<T>, ISetValue<T> {

	}

	public interface IGetValue<T> {
		T Get();
	}

	public interface ISetValue<T> {
		void Set(T value);
	}

	public interface IValue : IGetValue, ISetValue {

	}

	public interface IGetValue {
		object Get();
	}

	public interface ISetValue {
		void Set(object value);
	}

	public interface IParameterSystem {
		IList<ParameterData> Parameters { get; }
		ParameterData GetParameterData(string name);
		void SetParameterValue(string name, object value);
		object GetParameterValue(string name);
	}

	public interface IGenericParameterSystem {
		IList<GenericParameterData> GenericParameters { get; set; }
		GenericParameterData GetGenericParameter(string name);
	}

	public interface IAttributeSystem {
		IList<AttributeData> Attributes { get; set; }
	}

	public interface IPropertySystem {
		IList<uNodeProperty> Properties { get; }
		uNodeProperty GetPropertyData(string name);
	}

	public interface IVariableSystem {
		List<VariableData> Variables { get; }
		VariableData GetVariableData(string name);
	}

	public interface ILocalVariableSystem {
		List<VariableData> LocalVariables { get; }
		VariableData GetLocalVariableData(string name);
	}

	public interface IFunctionSystem {
		IList<uNodeFunction> Functions { get; }
		uNodeFunction GetFunction(string name, params System.Type[] parameters);
		uNodeFunction GetFunction(string name, int genericParameterLength, params System.Type[] parameters);
	}

	public interface IConstructorSystem {
		IList<uNodeConstuctor> Constuctors { get; }
	}

	public interface IFunction {
		object Invoke();
		object Invoke(object[] parameter);
		object Invoke(object[] parameter, System.Type[] genericType);
		System.Type ReturnType();
	}

	public interface IVariable : IValue {
		System.Type Type { get; }
	}

	public interface IProperty : IValue {
		bool CanGetValue();
		bool CanSetValue();
		bool AutoProperty { get; }
		System.Type ReturnType();
	}

	public interface INode {
		INodeRoot GetNodeOwner();
	}

	public interface INode<T> : INode where T : INodeRoot {
		T GetOwner();
	}

	/// <summary>
	/// Interface to implement code generation for data
	/// </summary>
	public interface IGenerate {
		string GenerateCode();
	}

	/// <summary>
	/// Interface to implement code generation for flow
	/// </summary>
	public interface IFlowGenerate {
		string GenerateCode();
	}

	/// <summary>
	/// A general interface for graph
	/// </summary>
	public interface IGraphSystem : INodeComponent, IVariableSystem, IPropertySystem, IFunctionSystem, INodeRoot, INamespaceSystem {
		IList<Node> Nodes { get; }
	}

	/// <summary>
	/// An interface to implement Macro Graph
	/// </summary>
	public interface IMacroGraph : INodeComponent, IIndependentGraph {
		bool HasCoroutineNode { get; }
	}

	/// <summary>
	/// An interface to implement State Graph
	/// </summary>
	public interface IStateGraph : INodeComponent {
		IList<BaseGraphEvent> eventNodes { get; }

		bool canCreateGraph { get; }
	}

	/// <summary>
	/// An interface for an object that's refresable ( editor only )
	/// </summary>
	public interface IRefreshable {
		void Refresh();
	}

	/// <summary>
	/// Interface to implement interface system for a class or struct graph
	/// </summary>
	public interface IInterfaceSystem {
		IList<MemberData> Interfaces { get; set; }
	}

	/// <summary>
	/// Interface to implement runtime interface system which only allowing Graph Interface
	/// </summary>
	public interface IRuntimeInterfaceSystem : IInterfaceSystem {
		
	}

	/// <summary>
	/// Interface to implement nested class graph
	/// </summary>
	public interface INestedClassSystem {
		uNodeData NestedClass { get; set; }
	}

	/// <summary>
	/// Interface to implement class system graph
	/// </summary>
	public interface IClassSystem : IClass, IAttributeSystem, IGenericParameterSystem, IConstructorSystem, INestedClassSystem {

	}

	/// <summary>
	/// An interface for Macro node
	/// </summary>
	public interface IMacro {
		void InitMacroPort(IMacroPort port);
		List<Nodes.MacroPortNode> InputFlows { get; }
		List<Nodes.MacroPortNode> InputValues { get; }
		List<Nodes.MacroPortNode> OutputFlows { get; }
		List<Nodes.MacroPortNode> OutputValues { get; }
	}

	public interface INodePort {

	}

	/// <summary>
	/// An interface for macro port
	/// </summary>
	public interface IMacroPort : INodePort {

	}

	/// <summary>
	/// An interface for flow port
	/// </summary>
	public interface IFlowPort {
		void OnExecute();
	}

	/// <summary>
	/// An interface for SuperNode / Group Node
	/// </summary>
	public interface ISuperNode {
		IList<NodeComponent> nestedFlowNodes { get; }
		bool AcceptCoroutine();
	}

	/// <summary>
	/// Interface to implement runtime function which can be Invoked by its unique name and parameters
	/// </summary>
	public interface IRuntimeFunction {
		object InvokeFunction(string Name, object[] values);
		object InvokeFunction(string Name, System.Type[] parameters, object[] values);
	}

	/// <summary>
	/// Interface to implement runtime variable which can be Set and Get a variable value by its unique name
	/// </summary>
	public interface IRuntimeVariable {
		void SetVariable(string Name, object value);
		object GetVariable(string Name);
		T GetVariable<T>(string Name);
	}

	/// <summary>
	/// Interface to implement runtime property which can be Set and Get a property value by its unique name
	/// </summary>
	public interface IRuntimeProperty {
		void SetProperty(string Name, object value);
		object GetProperty(string Name);
		T GetProperty<T>(string Name);
	}

	/// <summary>
	/// A runtime graph that is inherith from IRuntimeClass and with additional functions
	/// </summary>
	public interface IRuntimeGraph : IRuntimeClass {
		void ExecuteFunction(string Name);
	}

	/// <summary>
	/// A runtime class graph that has function, variable, and property
	/// </summary>
	public interface IRuntimeClass : IRuntimeFunction, IRuntimeVariable, IRuntimeProperty {
		void SetVariable(string Name, object value, char @operator);
		void SetProperty(string Name, object value, char @operator);
	}

	public interface IRuntimeInterface {
		IEnumerable<Type> GetInterfaces();
	}

	/// <summary>
	/// Used only for MonoBehaviour sub classes that's identified as instance of IClassComponent
	/// </summary>
	public interface IRuntimeComponent : IRuntimeClass, IRuntimeInterface {
		string uniqueIdentifier { get; }
	}

	/// <summary>
	/// Used only for ScriptableObject sub classes that's identified as instance of IClassAsset
	/// </summary>
	public interface IRuntimeAsset : IRuntimeClass, IRuntimeInterface {
		string uniqueIdentifier { get; }
	}
	
	public interface IRuntimeClassContainer {
		IRuntimeClass RuntimeClass { get; }
	}

	/// <summary>
	/// Use for identify graph that's supported to reference using RuntimeType
	/// </summary>
	public interface IClassIdentifier {
		string uniqueIdentifier { get; }
	}

	/// <summary>
	/// Used for a graph that's specifically generating a Class Component for runtime
	/// </summary>
	public interface IClassComponent : IClassIdentifier {
	}

	/// <summary>
	/// Used for a graph that's specifically generating a Class Asset for runtime
	/// </summary>
	public interface IClassAsset : IClassIdentifier {
		
	}
	
	/// <summary>
	/// Interface for implementing class modifier
	/// </summary>
	public interface IVariableModifier {
		FieldModifier GetModifier();
	}

	/// <summary>
	/// Interface for implementing class modifier
	/// </summary>
	public interface IPropertyModifier {
		PropertyModifier GetModifier();
	}

	/// <summary>
	/// Interface for implementing class modifier
	/// </summary>
	public interface IClassModifier {
		ClassModifier GetModifier();
	}

	/// <summary>
	/// Interface for describing the instance
	/// </summary>
	public interface ISummary {
		string GetSummary();
	}

	/// <summary>
	/// Used for a runtime graph that's listening to UnityEvent
	/// </summary>
	public interface IGraphWithUnityEvent {
		System.Action onAwake { get; set; }
		System.Action onStart { get; set; }
		System.Action onDestroy { get; set; }
		System.Action onDisable { get; set; }
		System.Action onEnable { get; set; }
	}

	/// <summary>
	/// An interface for implementing flow node that has one input and output
	/// </summary>
	public interface IFlowNode {
		void Execute(object graph);
	}

	/// <summary>
	/// An interface for implementing data node
	/// </summary>
	public interface IDataNode {
		object GetValue(object graph);
		Type ReturnType();
	}

	public interface IDataNode<T> : IDataNode {
		new T GetValue(object graph);
	}

	/// <summary>
	/// A class that's implement IDataNode with additional functions
	/// </summary>
	public abstract class DataNode : IDataNode {
		public abstract object GetValue(object graph);

		public T GetValue<T>(object graph) {
			var val = GetValue(graph);
			if(val == null) {
				return default;
			}
			return (T)val;
		}

		public abstract Type ReturnType();
	}

	/// <summary>
	/// A class that's implement IDataNode with additional functions
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DataNode<T> : IDataNode<T> {
		object IDataNode.GetValue(object graph) {
			return GetValue(graph);
		}

		public abstract T GetValue(object graph);

		public T1 GetValue<T1>(object graph) {
			var val = GetValue(graph) as object;
			if(val == null) {
				return default;
			}
			return (T1)val;
		}

		public virtual Type ReturnType() {
			return typeof(T);
		}
	}

	/// <summary>
	/// An interface for implementing State node which return if its a Success or Failure
	/// commands:
	/// =>yield return break; will finish the coroutine with success state.
	/// =>yield return true; and => yield return "Success"; will finish the coroutine with success state.
	/// =>yield return false; and => yield return "Failure"; will finish the coroutine with failure state.
	/// When the Execute function is finished without above command the node will finish with success state.
	/// </summary>
	public interface IStateNode {
		bool Execute(object graph);
	}

	/// <summary>
	/// An interface for implementing Coroutine State Node
	/// commands:
	/// =>yield return break; will finish the coroutine with success state and execute On Success flow.
	/// =>yield return true; and => yield return "Success"; will finish the coroutine with success state and execute On Success flow.
	/// =>yield return false; and => yield return "Failure"; will finish the coroutine with failure state and execute On Failure flow.
	/// When the Execute function is finished without above command the node will finish with success state.
	/// </summary>
	public interface IStateCoroutineNode {
		IEnumerable Execute(object graph);
	}

	/// <summary>
	/// An interface for implementing Coroutine node
	/// </summary>
	public interface ICoroutineNode {
		IEnumerable Execute(object graph);
	}

	public interface IReflectedNode { 
		
	}

	public abstract class NodePortAttribute : Attribute {
		public string name;
		public string description;

		public bool hideInNode;
		public bool editableInInspector;
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
	public class PortFlowInputAttribute : NodePortAttribute {
		
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class PortFlowOutputAttribute : NodePortAttribute {
		
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class PortDataInputAttribute : NodePortAttribute {
		
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
	public class PortDataOutputAttribute : NodePortAttribute {
		
	}
}