using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.KlaxScript.Interfaces;
using KlaxCore.KlaxScript.Nodes;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	public class CCustomFunctionGraph : CFunctionGraph
	{
		public event Action FunctionNodesRebuilt;

		public CCustomFunctionGraph()
		{
			CFunctionGraphEntryNode entryNode = new CFunctionGraphEntryNode(InputParameters);
			m_nodes.Add(entryNode);

			CFunctionGraphReturnNode returnNode = new CFunctionGraphReturnNode(OutputParameters);
			returnNode.NodePosX = 400;
			m_nodes.Add(returnNode);

			NumInputValues = InputParameters.Count;
			NumOutputValues = OutputParameters.Count;
		}

		public void RebuildFunctionNodes()
		{
			if (m_nodes.Count < 2)
			{
				throw new Exception("A function graph always needs at least 2 nodes, Entry and Return");
			}
			CFunctionGraphEntryNode entryNode = m_nodes[0] as CFunctionGraphEntryNode;
			if (entryNode == null)
			{
				throw new Exception("First node in the function graph was not a entry node");
			}
			entryNode.RebuildNode(InputParameters);

			CFunctionGraphReturnNode returnNode = m_nodes[1] as CFunctionGraphReturnNode;
			if (returnNode == null)
			{
				throw new Exception("Second node in the function graph was not a return node");
			}
			returnNode.RebuildNode(OutputParameters);

			NumInputValues = InputParameters.Count;
			NumOutputValues = OutputParameters.Count;

			FunctionNodesRebuilt?.Invoke();
		}

		[JsonProperty]
		public List<CKlaxVariable> InputParameters { get; private set; } = new List<CKlaxVariable>();

		[JsonProperty]
		public List<CKlaxVariable> OutputParameters { get; private set; } = new List<CKlaxVariable>();

		[JsonProperty]
		public Guid Guid { get; private set; } = Guid.NewGuid();
	}
}
