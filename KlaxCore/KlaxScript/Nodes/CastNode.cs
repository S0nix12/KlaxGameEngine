using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace KlaxCore.KlaxScript.Nodes
{
	public abstract class CBaseCastNode : CNode
	{
		public CBaseCastNode()
		{
			Name = "Cast";

			CInputPin targetTypeInput = new CInputPin
			{
				Name = "Target Type",
				Type = typeof(CKlaxScriptTypeInfo),
				Literal = null,
				SourceNode = null,
				SourceParameterIndex = -1,
				StackIndex = -1
			};
			InputPins.Add(targetTypeInput);

			CInputPin targetObjectInput = new CInputPin
			{
				Name = "Target",
				Type = typeof(object),
				Literal = null,
				SourceNode = null,
				SourceParameterIndex = -1,
				StackIndex = -1
			};
			InputPins.Add(targetObjectInput);

			COutputPin convertedObjectOutput = new COutputPin
			{
				Name = "Result",
				Type = typeof(object)
			};
			OutputPins.Add(convertedObjectOutput);
		}

		public override void OnInputLiteralChanged(CNodeChangeContext context, CInputPin pin)
		{
			if (pin == InputPins[0])
			{
				CKlaxScriptTypeInfo targetType = ((CKlaxScriptTypeInfo)pin.Literal);
				ChangePinType(context, OutputPins[0], targetType == null ? typeof(object) : targetType.Type);
				
				if (targetType != null)
				{
					ChangeNodeName(context, "Cast to " + targetType.Name);
				}
			}
		}
	}

	public class CExplicitCastNode : CBaseCastNode
	{
		public CExplicitCastNode()
		{
			CExecutionPin execute = new CExecutionPin()
			{
				Name = "In",
				TargetNode = null
			};
			InExecutionPins.Add(execute);

			CExecutionPin success = new CExecutionPin()
			{
				Name = "Succeeded",
				TargetNode = null
			};

			CExecutionPin failed = new CExecutionPin()
			{
				Name = "Failed",
				TargetNode = null
			};

			OutExecutionPins.Add(success);
			OutExecutionPins.Add(failed);
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			Type targetType = InputPins[0].SourceNode == null ? ((CKlaxScriptTypeInfo)InputPins[0].Literal).Type : ((CKlaxScriptTypeInfo)inParameters[0]).Type;
			object targetObject = inParameters[1];

			if (targetObject == null)
			{
				outReturn.Add(null);
				return OutExecutionPins[1];
			}

			if (targetType.IsInstanceOfType(targetObject))
			{
				outReturn.Add(targetObject);
				//outReturn.Add(Convert.ChangeType(targetObject, targetType));
				return OutExecutionPins[0];
			}
			else
			{
				outReturn.Add(null);
				return OutExecutionPins[1];
			}
		}
	}

	public class CImplicitCastNode : CBaseCastNode
	{
		public CImplicitCastNode()
		{
			IsImplicit = true;

			COutputPin successOutput = new COutputPin
			{
				Name = "Success",
				Type = typeof(bool)
			};
			OutputPins.Add(successOutput);

		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			Type targetType = InputPins[0].SourceNode == null ? ((CKlaxScriptTypeInfo)InputPins[0].Literal).Type : ((CKlaxScriptTypeInfo)inParameters[0]).Type;
			object targetObject = inParameters[1];

			if (targetObject == null)
			{
				outReturn.Add(null);
				outReturn.Add(false);
				return null;
			}

			if (targetType.IsInstanceOfType(targetObject))
			{
				outReturn.Add(targetObject);
				//outReturn.Add(Convert.ChangeType(targetObject, targetType));
				outReturn.Add(true);
				return null;
			}
			else
			{
				outReturn.Add(null);
				outReturn.Add(false);
				return null;
			}
		}		
	}
}
