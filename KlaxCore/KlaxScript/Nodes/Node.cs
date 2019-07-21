using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.GameFramework;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.KlaxScript
{
	public abstract class CPin
	{
		/// <summary>
		/// Name of the pin, only used in the editor for display purpose
		/// </summary>
		public string Name;
	}

	public class CInputPin : CPin
	{
		public CInputPin()
		{
			SourceNode = null;
			SourceParameterIndex = -1;
			SourceNodeIndex = -1;
			StackIndex = -1;
		}

		public CInputPin(string name, Type type)
		{
			Name = name;
			Type = type;
			Literal = Type.IsValueType ? Activator.CreateInstance(Type) : null;
			SourceNode = null;
			SourceParameterIndex = -1;
			SourceNodeIndex = -1;
			StackIndex = -1;
		}

		/// <summary>
		/// Value Type this pin expects
		/// </summary>
		public Type Type;
		/// <summary>
		/// Default value to use if this pin is not connected
		/// </summary>
		public object Literal;
		/// <summary>
		/// Node this pin is connected with, if null the pin is not connected
		/// </summary>
		[JsonIgnore]
		public CNode SourceNode;
		/// <summary>
		/// Only used during serialization, index of the source node in the serialized nodes array
		/// </summary>
		public int SourceNodeIndex;
		/// <summary>
		/// Index of the output pin on the source node if this pin is connected
		/// </summary>
		public int SourceParameterIndex;
		/// <summary>
		/// If true this input cannot be connected to another value and can only be set by an input literal
		/// </summary>
		public bool bIsLiteralOnly;

		/// <summary>
		/// Only valid after the nodes parent graph was compiled and the pin is connected, index into the stack array to locate the pins value
		/// </summary>
		[JsonIgnore]
		public int StackIndex;
	}

	public class COutputPin : CPin
	{
		public COutputPin() { }
		public COutputPin(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		/// <summary>
		/// Value Type that this pin outputs
		/// </summary>
		public Type Type;
	}

	public class CExecutionPin : CPin
	{
		public CExecutionPin()
		{
			TargetNodeIndex = -1;
			TargetPinIndex = -1;
		}

		public CExecutionPin(string name)
		{
			Name = name;
			TargetNode = null;
			TargetNodeIndex = -1;
			TargetPin = null;
			TargetPinIndex = -1;
		}

		/// <summary>
		/// Node this pin targets, if null this pin is not connected
		/// </summary>
		[JsonIgnore]
		public CNode TargetNode;

		/// <summary>
		/// Only valid during serialization, index of the target node in the serialized nodes array
		/// </summary>
		public int TargetNodeIndex;

		/// <summary>
		/// Execution pin this pin targets, if null this pin is not connected
		/// </summary>
		[JsonIgnore]
		public CExecutionPin TargetPin;

		/// <summary>
		/// Only valid during serialization, index of the execution pin on the targeted node
		/// </summary>
		public int TargetPinIndex;
	}

	public class CSwitchExecutionPin : CExecutionPin
	{
		public CSwitchExecutionPin() { }
		public CSwitchExecutionPin(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		/// <summary>
		/// The value of type {Type} that should be displayed
		/// </summary>
		public object Value;
		/// <summary>
		/// The type this execution pin should display
		/// </summary>
		public Type Type;
	}

	public abstract class CNode
	{
		public event Action NodeRebuilt;
		protected void RaiseNodeRebuildEvent()
		{
			NodeRebuilt?.Invoke();
		}

		/// <summary>
		/// Execute this nodes behavior
		/// </summary>
		/// <param name="context">Graph this node is executed on</param>
		/// <param name="inParameters">Parameters with which to execute</param>
		/// <param name="outReturn">Parameters this node returns</param>
		/// <returns>Node to execute next</returns>
		public abstract CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn);

		// Editor functions

		/// <summary>
		/// Notifies the node that the literal value of one of its pins has been altered
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="pin">The pin whose literal value has been changed</param>
		/// <returns>If true the node will be redrawn in the node graph</returns>
		public virtual void OnInputLiteralChanged(CNodeChangeContext context, CInputPin pin) { }

		/// <summary>
		/// Notifies the node that the output of another pin has been connected to one of its input pins
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="pin">The pin on this node that got a new connection</param>
		/// <param name="otherpin">The pin this node has been connected to</param>
		/// <returns></returns>
		public virtual void OnInputConnectionChanged(CNodeChangeContext context, CInputPin pin, COutputPin otherpin) { }

		/// <summary>
		/// Notifies the node that its 'Add Input' button has been clicked
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		public virtual void OnAddInputPinButtonClicked(CNodeChangeContext context) { }

		/// <summary>
		/// Notifies the node that its 'Add Output' button has been clicked
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		public virtual void OnAddOutputPinButtonClicked(CNodeChangeContext context) { }

		/// <summary>
		/// Changes the type of a pin. Should only be called upon editor callback.
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="pin"></param>
		/// <param name="newType"></param>
		protected void ChangePinType(CNodeChangeContext context, CPin pin, Type newType)
		{
			if (pin is CInputPin inputPin)
			{
				inputPin.Type = newType;
				context.Actions.Add(new CPinTypeChangeAction(pin, newType));
			}
			else if (pin is COutputPin outputPin)
			{
				outputPin.Type = newType;
				context.Actions.Add(new CPinTypeChangeAction(pin, newType));
			}
		}

		/// <summary>
		/// Changes the name of a pin. Should only be called upon editor callback.
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="pin">The pin whose name should be changed</param>
		/// <param name="newName">The new name of the pin</param>
		protected void ChangePinName(CNodeChangeContext context, CPin pin, string newName)
		{
			pin.Name = newName;
			context.Actions.Add(new CPinNameChangeAction(pin, newName));
		}

		/// <summary>
		/// Changes the name of this node. Should only be called upon editor callback.
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="newName">The new name of the node</param>
		protected void ChangeNodeName(CNodeChangeContext context, string newName)
		{
			Name = newName;
			context.Actions.Add(new CNodeNameChangeAction(newName));
		}

		/// <summary>
		/// Adds a new execution pin. This should only be called upon editor callback.
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="newPin">The new execution pin that should be added</param>
		/// <param name="index">The index at which the execution pin should be inserted</param>
		/// <param name="bIsIn">Whether the pin acts as in or output</param>
		protected void AddExecutionPin(CNodeChangeContext context, CExecutionPin newPin, int index, bool bIsIn)
		{
			if (bIsIn)
			{
				InExecutionPins.Add(newPin);
			}
			else
			{
				OutExecutionPins.Add(newPin);
			}

			context.Actions.Add(new CAddPinChangeAction(newPin, index, bIsIn));
		}

		/// <summary>
		/// Adds a new value output pin. This should only be called upon editor callback.
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="newPin">The new output pin that should be added</param>
		/// <param name="index">The index at which the output pin should be inserted</param>
		protected void AddOutputPin(CNodeChangeContext context, COutputPin newPin, int index)
		{
			OutputPins.Add(newPin);
			context.Actions.Add(new CAddPinChangeAction(newPin, index, false));
		}

		/// <summary>
		/// Adds a new value input pin. This should only be called upon editor callback.
		/// </summary>
		/// <param name="context">Context which contains all editor node actions that need to be executed after this call</param>
		/// <param name="newPin">The new input pin that should be added</param>
		/// <param name="index">The index at which the input pin should be inserted</param>
		protected void AddInputPin(CNodeChangeContext context, CInputPin newPin, int index)
		{
			InputPins.Add(newPin);
			context.Actions.Add(new CAddPinChangeAction(newPin, index, true));
		}
		// ~Editor functions

		protected virtual void OnImplicitChanged() { }

		public int GetInParameterCount()
		{
			return InputPins.Count;
		}

		public int GetOutParameterCount()
		{
			return OutputPins.Count;
		}

		public int GetInExecutionCount()
		{
			return InExecutionPins.Count;
		}

		public int GetOutExecutionCount()
		{
			return OutExecutionPins.Count;
		}

		/// <summary>
		/// Used by the editor and serialization to save the nodes position in the graph canvas
		/// </summary>
		[JsonProperty]
		internal double NodePosX { get; set; }
		/// <summary>
		/// Used by the editor and serialization to save the nodes position in the graph canvas
		/// </summary>
		[JsonProperty]
		internal double NodePosY { get; set; }

		[JsonProperty]
		private bool m_bIsImplict;
		/// <summary>
		/// True if this node is implicitly executed, implicit nodes do not have execution pins and are executed when their output is needed
		/// </summary>		
		internal bool IsImplicit
		{
			get { return m_bIsImplict; }
			set { m_bIsImplict = value; OnImplicitChanged(); }
		}
		/// <summary>
		/// Name of the node used in the editor
		/// </summary>
		[JsonProperty]
		internal string Name { get; set; }
		/// <summary>
		/// List of all execution pins that are used to execute this node
		/// </summary>
		[JsonProperty]
		internal List<CExecutionPin> InExecutionPins { get; set; } = new List<CExecutionPin>();
		/// <summary>
		/// List of all execution pins that are used the point to the next node that should be executed
		/// </summary>
		[JsonProperty]
		internal List<CExecutionPin> OutExecutionPins { get; set; } = new List<CExecutionPin>();
		/// <summary>
		/// List of input pins that describe the input the node uses to execute
		/// </summary>
		[JsonProperty]
		internal List<CInputPin> InputPins { get; set; } = new List<CInputPin>();
		/// <summary>
		/// List of output pins that describe the values the node returns after execution
		/// </summary>
		[JsonProperty]
		internal List<COutputPin> OutputPins { get; set; } = new List<COutputPin>();

		/// <summary>
		/// Only valid after the parent graph was compiled, first index into the stack to save the output values of this node
		/// </summary>
		internal int OutParameterStackIndex { get; set; }

		/// <summary>
		/// True if this node is allowed to be copied
		/// </summary>
		public bool AllowCopy { get; protected set; } = true;
		/// <summary>
		/// True if this node is allowed to be deleted from a graph
		/// </summary>
		public bool AllowDelete { get; protected set; } = true;
		/// <summary>
		/// 
		/// </summary>
		public bool CanAddInputPins { get; protected set; } = false;
		/// <summary>
		/// 
		/// </summary>
		public bool CanAddOutputPins { get; protected set; } = false;
	}
}
