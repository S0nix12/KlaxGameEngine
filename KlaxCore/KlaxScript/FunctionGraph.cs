using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.KlaxScript.Nodes;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	public abstract class CFunctionGraph : CGraph
	{
		public override void Execute()
		{
			throw new NotImplementedException();
		}

		public void Execute(List<object> inValues, List<object> outValues)
		{
			if (m_nodes.Count <= 0)
			{
				return;
			}

			CFunctionGraphEntryNode entryNode = m_nodes[0] as CFunctionGraphEntryNode;
			if (entryNode == null)
			{
				throw new Exception("The first node of a function graph has to be an entry node");
			}

			if (inValues.Count != NumInputValues)
			{
				throw new Exception($"Number of input values does not match the expected number on inputs. In Count was {inValues.Count} expected was {NumInputValues}");
			}

			outValues.Clear();
			outValues.Capacity = NumOutputValues;

			PrepareStack();
			m_inParameterBuffer.Clear();
			m_outParameterBuffer.Clear();

			foreach (var inValue in inValues)
			{
				m_inParameterBuffer.Add(inValue);
			}

			CGraph oldContextGraph = s_nodeExecutionContext.graph;
			CExecutionPin oldCalledPin = s_nodeExecutionContext.calledPin;

			s_nodeExecutionContext.graph = this;
			s_nodeExecutionContext.calledPin = null;

			CExecutionPin nextExecPin = entryNode.Execute(s_nodeExecutionContext, m_inParameterBuffer, m_outParameterBuffer);
			if (nextExecPin?.TargetNode != null)
			{
				AddOutParametersToStack(entryNode.OutParameterStackIndex, entryNode.GetOutParameterCount());
				CNode lastNode = Execute(nextExecPin.TargetNode, nextExecPin);
				if (lastNode is CFunctionGraphReturnNode)
				{
					// Out buffer contains the return values of the return nodes
					foreach (var outValue in m_outParameterBuffer)
					{
						outValues.Add(outValue);
					}
				}
				else
				{
					for (int i = 0; i < NumOutputValues; i++)
					{
						outValues.Add(null);
					}
				}
			}

			s_nodeExecutionContext.graph = oldContextGraph;
			s_nodeExecutionContext.calledPin = oldCalledPin;
		}

		[JsonProperty]
		public int NumInputValues { get; protected set; }
		[JsonProperty]
		public int NumOutputValues { get; protected set; }
	}
}
