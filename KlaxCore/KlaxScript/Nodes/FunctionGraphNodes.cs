using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	class CFunctionGraphEntryNode : CNode
	{
		[JsonConstructor]
		private CFunctionGraphEntryNode()
		{
			AllowDelete = false;
			AllowCopy = false;

			Name = "Entry";
		}

		public CFunctionGraphEntryNode(List<CKlaxVariable> inputParameters)
		{
			AllowDelete = false;
			AllowCopy = false;

			Name = "Entry";

			CExecutionPin execPin = new CExecutionPin("Next");
			OutExecutionPins.Add(execPin);

			foreach (var inputParameter in inputParameters)
			{
				COutputPin output = new COutputPin(inputParameter.Name, inputParameter.Type);
				OutputPins.Add(output);
			}
		}

		public void RebuildNode(List<CKlaxVariable> inputParameters)
		{
			// Adjust the nodes output pins to the new input parameters
			// Note the editor has to take care of adjusting pin connections 
			if (inputParameters.Count >= OutputPins.Count)
			{
				for (int i = 0; i < inputParameters.Count; i++)
				{
					CKlaxVariable inputParameter = inputParameters[i];
					if (OutputPins.Count > i)
					{
						COutputPin pin = OutputPins[i];
						pin.Name = inputParameter.Name;
						pin.Type = inputParameter.Type;
					}
					else
					{
						OutputPins.Add(new COutputPin(inputParameter.Name, inputParameter.Type));
					}
				}
			}
			else
			{
				for (int i = OutputPins.Count - 1; i >= 0; i--)
				{
					if (inputParameters.Count > i)
					{
						CKlaxVariable inputParameter = inputParameters[i];
						COutputPin pin = OutputPins[i];
						pin.Name = inputParameter.Name;
						pin.Type = inputParameter.Type;
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
			if (inParameters.Count != OutputPins.Count)
			{
				throw new Exception($"Function entry parameters had a different size than expected. Size was {inParameters.Count} expected was {OutputPins.Count}");
			}

			foreach (object inParameter in inParameters)
			{
				outReturn.Add(inParameter);
			}

			return OutExecutionPins[0];
		}
	}

	class CFunctionGraphReturnNode : CNode
	{
		[JsonConstructor]
		private CFunctionGraphReturnNode()
		{
			AllowDelete = false;
			AllowCopy = false;
		}

		public CFunctionGraphReturnNode(List<CKlaxVariable> returnParameters)
		{
			AllowDelete = false;
			AllowCopy = false;

			Name = "Return";

			CExecutionPin inPin = new CExecutionPin("Return");
			InExecutionPins.Add(inPin);

			foreach (var returnParameter in returnParameters)
			{
				CInputPin input = new CInputPin(returnParameter.Name, returnParameter.Type);
				InputPins.Add(input);
			}
		}

		public void RebuildNode(List<CKlaxVariable> returnParameters)
		{
			// Adjust the nodes input pins to the new return parameters
			// Note the editor has to take care of adjusting pin connections 
			if (returnParameters.Count >= InputPins.Count)
			{
				for (int i = 0; i < returnParameters.Count; i++)
				{
					CKlaxVariable returnParameter = returnParameters[i];
					if (InputPins.Count > i)
					{
						CInputPin pin = InputPins[i];
						pin.Name = returnParameter.Name;
						pin.Type = returnParameter.Type;
					}
					else
					{
						InputPins.Add(new CInputPin(returnParameter.Name, returnParameter.Type));
					}
				}
			}
			else
			{
				for (int i = InputPins.Count - 1; i >= 0; i--)
				{
					if (returnParameters.Count > i)
					{
						CKlaxVariable returnParameter = returnParameters[i];
						CInputPin pin = InputPins[i];
						pin.Name = returnParameter.Name;
						pin.Type = returnParameter.Type;
					}
					else
					{
						InputPins.RemoveAt(i);
					}
				}
			}

			RaiseNodeRebuildEvent();
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			if (inParameters.Count != InputPins.Count)
			{
				throw new Exception($"Function return parameters had a different size than expected. Size was {inParameters.Count} expected was {InputPins.Count}");
			}

			foreach (object inParameter in inParameters)
			{
				outReturn.Add(inParameter);
			}

			return null;
		}
	}
}
