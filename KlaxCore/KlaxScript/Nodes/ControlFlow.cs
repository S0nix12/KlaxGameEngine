using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.KlaxScript.Nodes
{
	class CBranchNode : CNode
	{
		public CBranchNode()
		{
			Name = "Branch";

			CExecutionPin inPin = new CExecutionPin("In");
			InExecutionPins.Add(inPin);

			CExecutionPin truePin = new CExecutionPin("True");
			OutExecutionPins.Add(truePin);

			CExecutionPin falsePin = new CExecutionPin("False");
			OutExecutionPins.Add(falsePin);

			CInputPin conditionPin = new CInputPin("Condition", typeof(bool));
			InputPins.Add(conditionPin);
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			bool bCondition = InputPins[0].SourceNode == null ? (bool) InputPins[0].Literal : (bool) inParameters[0];
			return bCondition ? OutExecutionPins[0] : OutExecutionPins[1];
		}
	}
}
