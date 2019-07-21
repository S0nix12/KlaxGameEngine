using KlaxCore.GameFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.KlaxScript.Nodes
{
	class CSelfReferenceNode : CNode
	{
		public CSelfReferenceNode()
		{
			IsImplicit = true;
			Name = "Self Reference";

			OutputPins.Add(new COutputPin("Self", typeof(CEntity)));
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			if (context.graph.ScriptableObject.Outer is CEntity entity)
			{
				outReturn.Add(entity);
			}
			else
			{
				outReturn.Add(null);
			}

			return null;
		}
	}
}
