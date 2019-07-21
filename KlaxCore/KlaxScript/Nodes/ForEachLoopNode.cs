using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;

namespace KlaxCore.KlaxScript.Nodes
{
	class CForEachLoopNode : CNode
	{
		public CForEachLoopNode()
		{
			Name = "Foreach Loop";

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

			InputPins.Add(new CInputPin("List", typeof(IList)));

			OutputPins.Add(new COutputPin("Index", typeof(int)));
			OutputPins.Add(new COutputPin("Element", typeof(object)));
		}

		public override void OnInputConnectionChanged(CNodeChangeContext context, CInputPin pin, COutputPin otherpin)
		{
			if (otherpin == null)
			{
				ChangePinType(context, OutputPins[1], typeof(object));
			}
			else
			{
				Type type = otherpin.Type;
				if (type.IsGenericType)
				{
					if (type.GetGenericTypeDefinition() == typeof(List<>))
					{
						Type outputType = type.GenericTypeArguments[0];
						if (outputType != OutputPins[1].Type)
						{
							ChangePinType(context, OutputPins[1], outputType);
						}
					}
				}
			}
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			if (context.calledPin == null)
			{
				//Advance loop
				m_currentIndex++;
				if (m_currentIndex >= m_currentList.Count)
				{
					//Loop finished
					m_currentIndex = -1;
					m_currentList = null;
					outReturn.Add(-1);
					outReturn.Add(OutputPins[1].Type.GetDefaultValue());
					return OutExecutionPins[1];
				}
				else
				{
					//Loop continues
					context.graph.AddReturnPointNode(this);
					outReturn.Add(m_currentIndex);
					outReturn.Add(m_currentList[m_currentIndex]);
					return OutExecutionPins[0];
				}
			}
			else
			{
				if (context.calledPin == InExecutionPins[0])
				{
					//Start new loop
					m_currentIndex = 0;
					m_currentList = inParameters[0] != null ? (IList)inParameters[0] : (IList)InputPins[0].Literal;

					context.graph.AddReturnPointNode(this);

					outReturn.Add(m_currentIndex);
					outReturn.Add(m_currentList[m_currentIndex]);
					return OutExecutionPins[0];
				}
				else if (m_currentIndex != -1)
				{
					//Break loop
					m_currentIndex = -1;
					m_currentList = null;

					context.graph.RemoveReturnPointNode(this);

					outReturn.Add(-1);
					outReturn.Add(OutputPins[1].Type.GetDefaultValue());
					return OutExecutionPins[1];
				}
			}

			return null;
		}

		private int m_currentIndex;
		private IList m_currentList;
	}
}
