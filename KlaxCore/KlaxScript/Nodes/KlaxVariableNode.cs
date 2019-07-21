using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	public abstract class CKlaxVariableNode : CNode
	{
		protected abstract void OnSourceVariableChanged();

		internal CKlaxVariable m_sourceVariable;
		[JsonIgnore]
		public CKlaxVariable SourceVariable
		{
			get { return m_sourceVariable; }
			set
			{
				m_sourceVariable = value;
				OnSourceVariableChanged();
			}
		}

		/// <summary>
		/// Index of the source variable in the KlaxVariable list, only valid during serialization
		/// </summary>
		[JsonProperty]
		internal int m_sourceVariableIndex;

		/// <summary>
		/// Alternative to index serialization, identifier to resolve variable reference
		/// </summary>
		[JsonProperty]
		internal Guid m_sourceVariableGuid;

		[JsonProperty]
		internal bool m_bIsLocalVariable;
	}

	public class CGetKlaxVariableNode : CKlaxVariableNode
	{
		public CGetKlaxVariableNode()
		{
			IsImplicit = true;
		}

		public CGetKlaxVariableNode(CKlaxVariable sourceVariable)
		{
			IsImplicit = true;
			SourceVariable = sourceVariable;			
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			outReturn.Add(SourceVariable.Value);
			return null;
		}

		protected override void OnSourceVariableChanged()
		{
			OutputPins.Clear();

			Name = "Get " + SourceVariable.Name;

			COutputPin output = new COutputPin(SourceVariable.Name, SourceVariable.Type);
			OutputPins.Add(output);
		}
	}

	public class CSetKlaxVariableNode : CKlaxVariableNode
	{
		public CSetKlaxVariableNode()
		{
			IsImplicit = true;

			CExecutionPin inPin = new CExecutionPin("In");
			InExecutionPins.Add(inPin);

			CExecutionPin outPin = new CExecutionPin("Out");
			OutExecutionPins.Add(outPin);
		}

		public CSetKlaxVariableNode(CKlaxVariable sourceVariable)
		{
			IsImplicit = true;
			SourceVariable = sourceVariable;

			CExecutionPin inPin = new CExecutionPin("In");
			InExecutionPins.Add(inPin);

			CExecutionPin outPin = new CExecutionPin("Out");
			OutExecutionPins.Add(outPin);
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			object newValue = InputPins[0].SourceNode != null ? inParameters[0] : InputPins[0].Literal;
			SourceVariable.Value = newValue;
			outReturn.Add(newValue);

			return OutExecutionPins[0];
		}

		protected override void OnSourceVariableChanged()
		{
			OutputPins.Clear();
			InputPins.Clear();

			Name = "Set " + SourceVariable.Name;

			CInputPin input = new CInputPin(SourceVariable.Name, SourceVariable.Type);
			InputPins.Add(input);

			COutputPin output = new COutputPin("NewValue", SourceVariable.Type);
			OutputPins.Add(output);
		}
	}
}
