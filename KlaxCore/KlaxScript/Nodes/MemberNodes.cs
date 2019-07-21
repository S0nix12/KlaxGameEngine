using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	abstract class CMemberNode : CNode
	{
		protected abstract void OnTargetFieldChanged();
		protected abstract void OnTargetPropertyChanged();

		[JsonProperty]
		private FieldInfo m_targetField;
		[JsonIgnore]
		public FieldInfo TargetField
		{
			get { return m_targetField; }
			set
			{
				m_targetField = value;
				m_targetProperty = null;
				m_bIsField = true;
				OnTargetFieldChanged();
			}
		}

		[JsonProperty]
		private PropertyInfo m_targetProperty;
		[JsonIgnore]
		public PropertyInfo TargetProperty
		{
			get { return m_targetProperty; }
			set
			{
				m_targetProperty = value;
				m_targetField = null;
				m_bIsField = false;
				OnTargetPropertyChanged();
			}
		}

		/// <summary>
		/// True if this node targets a member field otherwise it targets a property
		/// </summary>
		[JsonProperty]
		protected bool m_bIsField;
	}

	class CGetMemberNode : CMemberNode
	{
		public CGetMemberNode()
		{
			IsImplicit = true;
		}

		public CGetMemberNode(FieldInfo targetField)
		{
			IsImplicit = true;
			TargetField = targetField;
		}

		public CGetMemberNode(PropertyInfo targetProperty)
		{
			IsImplicit = true;
			TargetProperty = targetProperty;
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			for (int i = 0; i < inParameters.Count; i++)
			{
				if (inParameters[i] == null)
				{
					inParameters[i] = InputPins[i].Literal;
				}
			}

			object targetObject = inParameters[0];
			if (m_bIsField)
			{
				outReturn.Add(TargetField.GetValue(targetObject));
			}
			else
			{
				outReturn.Add(TargetProperty.GetValue(targetObject));
			}

			// Implicit nodes do not return a following node
			return null;
		}

		protected override void OnTargetFieldChanged()
		{
			if (TargetField == null)
			{
				throw new NullReferenceException("Target Field cannot be null as this would make the node invalid");
			}
			InputPins.Clear();
			OutputPins.Clear();

			CInputPin targetObjectInput = new CInputPin("Target", TargetField.DeclaringType);
			InputPins.Add(targetObjectInput);

			Name = "Get " + TargetField.Name;

			COutputPin outputPin = new COutputPin(TargetField.Name, TargetField.FieldType);
			OutputPins.Add(outputPin);
		}

		protected override void OnTargetPropertyChanged()
		{
			if (TargetProperty == null)
			{
				throw new NullReferenceException("Target Property cannot be null as this would make the node invalid");
			}
			InputPins.Clear();
			OutputPins.Clear();

			CInputPin targetObjectInput = new CInputPin("Target", TargetProperty.DeclaringType);
			InputPins.Add(targetObjectInput);

			Name = "Get " + TargetProperty.Name;

			COutputPin outputPin = new COutputPin(TargetProperty.Name, TargetProperty.PropertyType);
			OutputPins.Add(outputPin);
		}
	}

	class CSetMemberNode : CMemberNode
	{
		public CSetMemberNode()
		{
			AddExecutionPins();
		}

		public CSetMemberNode(FieldInfo targetField)
		{
			AddExecutionPins();
			TargetField = targetField;
		}

		public CSetMemberNode(PropertyInfo targetProperty)
		{
			AddExecutionPins();
			TargetProperty = targetProperty;
		}

		private void AddExecutionPins()
		{
			CExecutionPin execute = new CExecutionPin()
			{
				Name = "In",
				TargetNode = null
			};
			InExecutionPins.Add(execute);

			CExecutionPin done = new CExecutionPin()
			{
				Name = "Out",
				TargetNode = null
			};
			OutExecutionPins.Add(done);
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			for (int i = 0; i < inParameters.Count; i++)
			{
				if (inParameters[i] == null)
				{
					inParameters[i] = InputPins[i].Literal;
				}
			}

			if (m_bIsField)
			{
				TargetField.SetValue(inParameters[0], inParameters[1]);
				outReturn.Add(inParameters[1]);
			}
			else
			{
				TargetProperty.SetValue(inParameters[0], inParameters[1]);
				outReturn.Add(TargetProperty.GetValue(inParameters[0]));
			}
			return OutExecutionPins[0];
		}

		protected override void OnTargetFieldChanged()
		{
			if (TargetField == null)
			{
				throw new NullReferenceException("Target Field cannot be null as this would make the node invalid");
			}
			InputPins.Clear();
			OutputPins.Clear();

			Name = "Set " + TargetField.Name;

			CInputPin targetObjectInput = new CInputPin("Target", TargetField.DeclaringType);
			InputPins.Add(targetObjectInput);

			CInputPin setValueInput = new CInputPin("Value", TargetField.FieldType);
			InputPins.Add(setValueInput);

			COutputPin newValueOutput = new COutputPin("NewValue", TargetField.FieldType);
			OutputPins.Add(newValueOutput);
		}

		protected override void OnTargetPropertyChanged()
		{
			if (TargetProperty == null)
			{
				throw new NullReferenceException("Target Property cannot be null as this would make the node invalid");
			}
			InputPins.Clear();
			OutputPins.Clear();

			Name = "Set " + TargetProperty.Name;

			CInputPin targetObjectInput = new CInputPin("Target", TargetProperty.DeclaringType);
			InputPins.Add(targetObjectInput);

			CInputPin setValueInput = new CInputPin("Value", TargetProperty.PropertyType);
			InputPins.Add(setValueInput);

			COutputPin newValueOutput = new COutputPin("NewValue", TargetProperty.PropertyType);
			OutputPins.Add(newValueOutput);
		}
	}
}
