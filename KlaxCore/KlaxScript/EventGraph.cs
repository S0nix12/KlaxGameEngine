using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.GameFramework;
using KlaxCore.KlaxScript.Nodes;
using KlaxCore.KlaxScript.Serialization;
using KlaxCore.Physics;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	/// <summary>
	/// A graph that is executed on invocation of an event
	/// </summary>
	public class CEventGraph : CGraph
	{
		public CEventGraph() { }

		public CEventGraph(CKlaxScriptEventInfo klaxEventInfo)
		{
			Name = klaxEventInfo.displayName;
			TargetEvent = klaxEventInfo;
		}

		public CEventGraph(CKlaxScriptEventInfo klaxEventInfo, Guid componentGuid, string componentName)
		{
			Name = componentName + "_" + klaxEventInfo.displayName;
			TargetEvent = klaxEventInfo;
			TargetComponentGuid = componentGuid;
		}

		public CEventGraph(CKlaxScriptEventInfo targetEvent, object eventSource)
		{
			TargetEvent = targetEvent;
			Subscribe(targetEvent.klaxEventInfo, eventSource);
		}

		public override void Execute()
		{
			throw new NotImplementedException("An event graph cannot be executed directly only by invoking the event it is subscribed to");
		}

		protected void Execute(object[] eventArgs)
		{
			if (m_nodes.Count <= 0)
			{
				return;
			}

			if (!(m_nodes[0] is CReceiveEventNode eventTargetNode))
			{
				throw new Exception("The first node of an event graph must always be a ReceiveEventNode");
			}

			PrepareStack();

			m_inParameterBuffer.Clear();
			m_outParameterBuffer.Clear();

			if (eventArgs != null)
			{
				foreach (object arg in eventArgs)
				{
					m_inParameterBuffer.Add(arg);
				}
			}

			CGraph oldContextGraph = s_nodeExecutionContext.graph;
			CExecutionPin oldCalledPin = s_nodeExecutionContext.calledPin;

			s_nodeExecutionContext.graph = this;
			s_nodeExecutionContext.calledPin = null;

			CExecutionPin nextExecPin = eventTargetNode.Execute(s_nodeExecutionContext, m_inParameterBuffer, m_outParameterBuffer);
			m_inParameterBuffer.Clear();
			if (nextExecPin != null && nextExecPin.TargetNode != null)
			{
				AddOutParametersToStack(eventTargetNode.OutParameterStackIndex, eventTargetNode.GetOutParameterCount());
				m_outParameterBuffer.Clear();
				Execute(nextExecPin.TargetNode, nextExecPin);
			}

			s_nodeExecutionContext.graph = oldContextGraph;
			s_nodeExecutionContext.calledPin = oldCalledPin;
		}

		/// <summary>
		/// Unsubscribes this graph if it is currently subscribed to an event
		/// </summary>
		public void Unsubscribe()
		{
			if (m_bIsSubscribed)
			{
				CKlaxScriptEventBase klaxEvent = (CKlaxScriptEventBase) TargetEvent.klaxEventInfo.GetValue(m_eventSource);
				klaxEvent.Unsubscribe(Execute);				
				m_bIsSubscribed = false;
			}
		}

		/// <summary>
		/// Subscribe to the given event on the given target object
		/// </summary>
		/// <param name="targetEvent"></param>
		/// <param name="target"></param>
		public void Subscribe(FieldInfo targetKlaxEvent, object target)
		{
			// Unsubscribe in case we are subscribed to another event currently
			Unsubscribe();

			if (TargetComponentGuid != Guid.Empty && target is CEntity baseEntity)
			{
				CEntityComponent targetComponent = baseEntity.GetComponentByGuid(TargetComponentGuid);
				if (targetComponent == null)
				{
					throw new Exception("Event graphs tried to bind to a component event of a component that does not exist on the parent entity");
				}

				target = targetComponent;
			}

			//Validate that the event handler has the correct form
			CKlaxScriptEventBase klaxEvent = (CKlaxScriptEventBase) TargetEvent.klaxEventInfo.GetValue(target);
			if (klaxEvent == null)
			{
				throw new NullReferenceException("The given event has an invalid handler");
			}

			klaxEvent.Subscribe(Execute);
			m_eventSource = target;
			m_bIsSubscribed = true;
		}

		/// <summary>
		/// Subscribe the current target event on the given target object
		/// </summary>
		/// <param name="target"></param>
		public void Subscribe(object target)
		{
			if (TargetEvent == null)
			{
				throw new Exception("Target event was not set");
			}
			Subscribe(TargetEvent.klaxEventInfo, target);
		}

		private void OnTargetEventChanged()
		{
			Unsubscribe();
			if (m_nodes.Count > 0)
			{
				CReceiveEventNode eventNode = (CReceiveEventNode) m_nodes[0];
				eventNode.TargetKlaxEvent = TargetEvent;
			}
			else
			{
				CReceiveEventNode startNode = new CReceiveEventNode(TargetEvent);
				m_nodes.Add(startNode);
			}
		}

		[JsonProperty]
		[JsonConverter(typeof(CUseScriptSerializerConverter))]
		private CKlaxScriptEventInfo m_targetEvent;
		[JsonIgnore]
		public CKlaxScriptEventInfo TargetEvent
		{
			get { return m_targetEvent; }
			set
			{
				if (m_targetEvent != value)
				{
					m_targetEvent = value;
					OnTargetEventChanged();
				}
			}
		}

		[JsonProperty]
		public Guid TargetComponentGuid { get; private set; }

		private object m_eventSource;
		private bool m_bIsSubscribed;
	}
}
