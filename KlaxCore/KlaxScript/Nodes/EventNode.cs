using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Nodes
{
	/// <summary>
	/// Receive event nodes are special nodes that serve as the start point of an event graph. The execute function of these nodes should be called with the event parameters of the event that raised the execution of the graph
	/// </summary>
	class CReceiveEventNode : CNode
	{
		public CReceiveEventNode()
		{			
			CExecutionPin invokedPin = new CExecutionPin()
			{
				Name = "Out",
				TargetNode = null,
				TargetNodeIndex = -1,
			};
			OutExecutionPins.Add(invokedPin);

			AllowCopy = false;
			AllowDelete = false;
		}

		public CReceiveEventNode(CKlaxScriptEventInfo targetKlaxEvent)
		{
			CExecutionPin invokedPin = new CExecutionPin()
			{
				Name = "Out",
				TargetNode = null,
				TargetNodeIndex = -1,
			};
			OutExecutionPins.Add(invokedPin);
			TargetKlaxEvent = targetKlaxEvent;

			AllowCopy = false;
			AllowDelete = false;
		}

		public override CExecutionPin Execute(CNodeExecutionContext context, List<object> inParameters, List<object> outReturn)
		{
			// Validate that the parameters are correct
			if (inParameters.Count != OutputPins.Count)
			{
				throw new Exception("Incoming parameters of event node do not match outgoing");
			}

			for (var i = 0; i < inParameters.Count; i++)
			{
				object inParameter = inParameters[i];
				if (inParameter.GetType() != OutputPins[i].Type)
				{
					throw new Exception("Incoming parameters of event node do not match outgoing");
				}
				outReturn.Add(inParameter);
			}

			return OutExecutionPins[0];
		}

		private void OnTargetEventChanged()
		{
			if (TargetKlaxEvent == null)
			{
				throw new NullReferenceException("Target event cannot be null");
			}

			Name = TargetKlaxEvent.displayName ?? TargetKlaxEvent.klaxEventInfo.Name;

			Type eventType = TargetKlaxEvent.klaxEventInfo.FieldType;
			for (var i = 0; i < eventType.GenericTypeArguments.Length; i++)
			{				
				Type genericTypeArgument = eventType.GenericTypeArguments[i];
				string displayName = genericTypeArgument.Name;
				switch (i)
				{
					case 0:
						displayName = TargetKlaxEvent.ParameterName1 ?? displayName;
						break;
					case 1:
						displayName = TargetKlaxEvent.ParameterName2 ?? displayName;
						break;
					case 2:
						displayName = TargetKlaxEvent.ParameterName3 ?? displayName;
						break;
					case 3:
						displayName = TargetKlaxEvent.ParameterName4 ?? displayName;
						break;
					case 4:
						displayName = TargetKlaxEvent.ParameterName5 ?? displayName;
						break;
					case 5:
						displayName = TargetKlaxEvent.ParameterName6 ?? displayName;
						break;
					case 6:
						displayName = TargetKlaxEvent.ParameterName7 ?? displayName;
						break;
					case 7:
						displayName = TargetKlaxEvent.ParameterName8 ?? displayName;
						break;
					case 8:
						displayName = TargetKlaxEvent.ParameterName9 ?? displayName;
						break;
					case 9:
						displayName = TargetKlaxEvent.ParameterName10 ?? displayName;
						break;
				}

				COutputPin output = new COutputPin()
				{
					Name = displayName,
					Type = genericTypeArgument,
				};

				OutputPins.Add(output);
			}
		}

		[JsonProperty]
		private CKlaxScriptEventInfo m_targetKlaxEvent;
		[JsonIgnore]
		public CKlaxScriptEventInfo TargetKlaxEvent
		{
			get { return m_targetKlaxEvent; }
			set
			{
				m_targetKlaxEvent = value;
				OnTargetEventChanged();	
			}
		}
	}
}
