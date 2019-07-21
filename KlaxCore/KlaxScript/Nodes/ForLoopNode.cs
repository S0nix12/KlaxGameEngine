using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.KlaxScript.Nodes
{
	class CForLoopNode : CNode
	{
		public CForLoopNode()
		{
			CExecutionPin execute = new CExecutionPin()
			{
				Name = "In",
				TargetNode = null
			};
			InExecutionPins.Add(execute);

			CExecutionPin breakExec = new CExecutionPin()
			{
				Name = "Break",
				TargetNode = null
			};
			InExecutionPins.Add(breakExec);

			CExecutionPin loop = new CExecutionPin()
			{
				Name = "Loop",
				TargetNode = null
			};

			OutExecutionPins.Add(loop);
			CExecutionPin done = new CExecutionPin()
			{
				Name = "Done",
				TargetNode = null
			};
			OutExecutionPins.Add(done);

			InputPins.Add(new CInputPin("First Index", typeof(int)));
			InputPins.Add(new CInputPin("Last Index", typeof(int)));

			OutputPins.Add(new COutputPin("Index", typeof(int)));
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			if (context.calledPin == null)
			{
				//Advance loop
				m_currentIndex++;
				if (m_currentIndex > m_lastIndex)
				{
					//Loop finished
					m_currentIndex = -1;
					m_lastIndex = -1;
					outReturn.Add(-1);
					return OutExecutionPins[1];
				}
				else
				{
					//Loop continues
					context.graph.AddReturnPointNode(this);
					outReturn.Add(m_currentIndex);
					return OutExecutionPins[0];
				}
			}
			else
			{
				if (context.calledPin == InExecutionPins[0])
				{
					//Start new loop
					m_currentIndex = inParameters[0] != null ? (int)inParameters[0] : (int)InputPins[0].Literal;
					m_lastIndex = inParameters[1] != null ? (int)inParameters[1] : (int)InputPins[1].Literal;

					context.graph.AddReturnPointNode(this);

					outReturn.Add(m_currentIndex);
					return OutExecutionPins[0];
				}
				else if (m_currentIndex != -1)
				{
					//Break loop
					m_currentIndex = -1;
					m_lastIndex = -1;

					context.graph.RemoveReturnPointNode(this);

					outReturn.Add(-1);
					return OutExecutionPins[1];
				}
			}

			return null;
		}

		private int m_currentIndex;
		private int m_lastIndex;
	}
}
