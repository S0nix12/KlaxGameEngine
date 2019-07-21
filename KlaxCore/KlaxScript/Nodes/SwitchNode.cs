using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	public class CSwitchNode : CNode
	{
		public CSwitchNode()
		{
			Name = "Switch";
			CanAddOutputPins = true;

			CExecutionPin execute = new CExecutionPin()
			{
				Name = "In",
				TargetNode = null
			};
			InExecutionPins.Add(execute);

			CExecutionPin defaultOutExec = new CExecutionPin()
			{
				Name = "Default",
				TargetNode = null
			};
			OutExecutionPins.Add(defaultOutExec);

			CInputPin typePin = new CInputPin()
			{
				Name = "Type",
				bIsLiteralOnly = true,
				Type = typeof(CKlaxScriptTypeInfo),
				Literal = typeof(string)
			};
			InputPins.Add(typePin);

			CInputPin objectPin = new CInputPin()
			{
				Name = "Object",
				Type = typeof(object)
			};
			InputPins.Add(objectPin);
		}

		public override void OnInputLiteralChanged(CNodeChangeContext context, CInputPin pin)
		{
			if (pin == InputPins[0])
			{
				CKlaxScriptTypeInfo newType = pin.Literal as CKlaxScriptTypeInfo;
				if (newType != null)
				{
					OutExecutionPins.RemoveRange(1, OutExecutionPins.Count - 1);
					context.Actions.Add(new CSwitchNodeTypeChangeAction(newType.Type));

					ChangePinType(context, InputPins[1], newType.Type);
					ChangeNodeName(context, $"Switch ({newType.Name})");
					InputPins[1].Literal = newType.Type.GetDefaultValue();
				}
			}
		}

		public override void OnAddOutputPinButtonClicked(CNodeChangeContext context)
		{
			if (InputPins[0].Literal is CKlaxScriptTypeInfo currentType)
			{
				CSwitchExecutionPin pin = new CSwitchExecutionPin
				{
					Name = "",
					Type = currentType.Type,
					Value = currentType.Type.GetDefaultValue()
				};
				AddExecutionPin(context, pin, OutExecutionPins.Count, false);
			}
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			object instance = inParameters[1] ?? InputPins[1].Literal;
			if (instance != null)
			{
				for (int i = 1, count = OutExecutionPins.Count; i < count; i++)
				{
					CSwitchExecutionPin pin = (CSwitchExecutionPin)OutExecutionPins[i];
					if (instance.Equals(pin.Value))
					{
						return OutExecutionPins[i];
					}
				}
			}

			return OutExecutionPins[0];
		}
	}
}
