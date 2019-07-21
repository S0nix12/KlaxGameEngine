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
	public class CInterfaceFunctionGraph : CFunctionGraph
	{
		[JsonConstructor]
		private CInterfaceFunctionGraph() { }

		public CInterfaceFunctionGraph(CKlaxScriptInterfaceFunction interfaceFunction)
		{
			InterfaceFunctionGuid = interfaceFunction.Guid;

			CFunctionGraphEntryNode entryNode = new CFunctionGraphEntryNode(interfaceFunction.InputParameters);
			m_nodes.Add(entryNode);

			CFunctionGraphReturnNode returnNode = new CFunctionGraphReturnNode(interfaceFunction.OutputParameters);
			returnNode.NodePosX = 400;
			m_nodes.Add(returnNode);

			NumInputValues = interfaceFunction.InputParameters.Count;
			NumOutputValues = interfaceFunction.OutputParameters.Count;
		}

		[JsonProperty]
		public Guid InterfaceFunctionGuid { get; private set; }
	}
}
