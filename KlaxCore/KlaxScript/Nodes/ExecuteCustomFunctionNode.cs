using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.GameFramework;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	public class CExecuteCustomFunctionNode : CNode
	{
		[JsonConstructor]
		private CExecuteCustomFunctionNode() { }

		public CExecuteCustomFunctionNode(CCustomFunctionGraph functionGraph)
		{
			TargetFunctionGuid = functionGraph.Guid;

			Name = functionGraph.Name;

			CExecutionPin inPin = new CExecutionPin("In");
			InExecutionPins.Add(inPin);

			CExecutionPin execPin = new CExecutionPin("Next");
			OutExecutionPins.Add(execPin);

			foreach (var inputParameter in functionGraph.InputParameters)
			{
				CInputPin input = new CInputPin(inputParameter.Name, inputParameter.Type);
				InputPins.Add(input);
			}

			foreach (var returnParameter in functionGraph.OutputParameters)
			{
				COutputPin output = new COutputPin(returnParameter.Name, returnParameter.Type);
				OutputPins.Add(output);
			}
		}

		public void ResolveFunctionReference(CKlaxScriptObject scriptObject)
		{
			foreach (var functionGraph in scriptObject.FunctionGraphs)
			{
				if (functionGraph.Guid == TargetFunctionGuid)
				{
					m_targetFunction = functionGraph;
					m_targetFunction.FunctionNodesRebuilt += OnFunctionRebuilt;
					return;
				}
			}
		}

		private void OnFunctionRebuilt()
		{
			// Input parameter rebuild
			if (m_targetFunction.InputParameters.Count > InputPins.Count)
			{
				for (int i = 0; i < m_targetFunction.InputParameters.Count; i++)
				{
					CKlaxVariable inputParameter = m_targetFunction.InputParameters[i];
					if (InputPins.Count > i)
					{
						InputPins[i].Type = inputParameter.Type;
						InputPins[i].Name = inputParameter.Name;
					}
					else
					{
						InputPins.Add(new CInputPin(inputParameter.Name, inputParameter.Type));
					}
				}
			}
			else
			{
				for (int i = InputPins.Count - 1; i >= 0; i--)
				{
					if (m_targetFunction.InputParameters.Count > i)
					{
						CKlaxVariable inputParameter = m_targetFunction.InputParameters[i];
						InputPins[i].Type = inputParameter.Type;
						InputPins[i].Name = inputParameter.Name;
					}
					else
					{
						InputPins.RemoveAt(i);
					}
				}
			}

			// Output parameter rebuild
			if (m_targetFunction.OutputParameters.Count > OutputPins.Count)
			{
				for (int i = 0; i < m_targetFunction.OutputParameters.Count; i++)
				{
					CKlaxVariable outputParameter = m_targetFunction.OutputParameters[i];
					if (OutputPins.Count > i)
					{
						OutputPins[i].Type = outputParameter.Type;
						OutputPins[i].Name = outputParameter.Name;
					}
					else
					{
						OutputPins.Add(new COutputPin(outputParameter.Name, outputParameter.Type));
					}
				}

			}
			else
			{
				for (int i = OutputPins.Count - 1; i >= 0; i--)
				{
					if (m_targetFunction.OutputParameters.Count > i)
					{
						CKlaxVariable outputParameter = m_targetFunction.OutputParameters[i];
						OutputPins[i].Type = outputParameter.Type;
						OutputPins[i].Name = outputParameter.Name;
					}
					else
					{
						OutputPins.RemoveAt(i);
					}
				}
			}
			RaiseNodeRebuildEvent();
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			if (m_targetFunction == null)
			{
				LogUtility.Log("Function {0} not executed as the target function was null, make sure to resolve the function reference before", Name);
				return OutExecutionPins[0];
			}

			for (int i = 0; i < InputPins.Count; i++)
			{
				if (InputPins[i].SourceNode == null)
				{
					inParameters[i] = InputPins[i].Literal;
				}
			}				

			m_targetFunction.Execute(inParameters, outReturn);
			return OutExecutionPins[0];
		}


		[JsonProperty]
		public Guid TargetFunctionGuid { get; private set; }

		private CCustomFunctionGraph m_targetFunction;
	}
}
