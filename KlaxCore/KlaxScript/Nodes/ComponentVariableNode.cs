using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.GameFramework;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	class CComponentVariableNode : CNode
	{
		public CComponentVariableNode()
		{
			IsImplicit = true;
		}

		public CComponentVariableNode(CEntityComponent targetComponent)
		{
			IsImplicit = true;

			Name = "Get " + targetComponent.Name;
			ComponentTargetGuid = targetComponent.ComponentGuid;

			COutputPin output = new COutputPin(targetComponent.Name, targetComponent.GetType());
			OutputPins.Add(output);
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			if (context.graph.ScriptableObject.Outer is CEntity entity)
			{
				CEntityComponent outComponent = entity.GetComponentByGuid(ComponentTargetGuid);
#if DEBUG
				if (outComponent == null || outComponent.GetType() != OutputPins[0].Type)
				{
					LogUtility.Log("Receive component node failed. The given component did not exist or was of a different type than expected");
					outReturn.Add(null);
					return null;
				}		
#endif
				outReturn.Add(outComponent);
			}
			else
			{
				outReturn.Add(null);
			}

			return null;
		}

		[JsonProperty]
		public Guid ComponentTargetGuid { get; private set; }
	}
}
