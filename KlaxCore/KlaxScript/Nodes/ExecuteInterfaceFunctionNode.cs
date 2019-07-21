using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.GameFramework;
using KlaxCore.KlaxScript.Interfaces;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	public class CExecuteInterfaceFunctionNode : CNode
	{
		[JsonConstructor]
		private CExecuteInterfaceFunctionNode() { }

		public CExecuteInterfaceFunctionNode(CKlaxScriptInterfaceFunction targetFunction)
		{
			TargetFunctionGuid = targetFunction.Guid;

			Name = targetFunction.Name;

			CExecutionPin inPin = new CExecutionPin("In");
			InExecutionPins.Add(inPin);

			CExecutionPin execPin = new CExecutionPin("Next");
			OutExecutionPins.Add(execPin);

			CInputPin targetInput = new CInputPin("Target", typeof(CEntity));
			InputPins.Add(targetInput);

			foreach (var inputParameter in targetFunction.InputParameters)
			{
				CInputPin input = new CInputPin(inputParameter.Name, inputParameter.Type);
				InputPins.Add(input);
			}

			foreach (var returnParameter in targetFunction.OutputParameters)
			{
				COutputPin output = new COutputPin(returnParameter.Name, returnParameter.Type);
				OutputPins.Add(output);
			}
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			for (int i = 0; i < InputPins.Count; i++)
			{
				if (InputPins[i].SourceNode == null)
				{
					inParameters[i] = InputPins[i].Literal;
				}
			}

			if (inParameters[0] == null)
			{
				LogUtility.Log("Interface function not executed target was null");
				return OutExecutionPins[0];
			}

			CEntity targetEntity = inParameters[0] as CEntity;
			if (targetEntity == null)
			{
				LogUtility.Log("Interface function not executed target is not an entity");
				return OutExecutionPins[0];
			}

			CInterfaceFunctionGraph targetGraph = null;
			foreach (var interfaceGraph in targetEntity.KlaxScriptObject.InterfaceGraphs)
			{
				if (interfaceGraph.InterfaceFunctionGuid == TargetFunctionGuid)
				{
					targetGraph = interfaceGraph;
					break;
				}
			}

			if (targetGraph == null)
			{
				LogUtility.Log("Interface function not executed target does not implemented the interface function. Target was {0}", targetEntity.Name);
				return OutExecutionPins[0];
			}

			List<object> functionParameters = new List<object>(inParameters.Count - 1);
			for (int i = 1; i < inParameters.Count; i++)
			{
				functionParameters.Add(inParameters[i]);
			}

			targetGraph.Execute(functionParameters, outReturn);
			return OutExecutionPins[0];
		}

		[JsonProperty]
		public Guid TargetFunctionGuid { get; private set; }
	}
}
