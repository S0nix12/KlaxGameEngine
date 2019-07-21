using System;
using System.Collections.Generic;
using KlaxCore.KlaxScript.Nodes;
using KlaxCore.KlaxScript.Serialization;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript
{
	public class CGraph
	{
		protected static readonly CNodeExecutionContext s_nodeExecutionContext = new CNodeExecutionContext();

		public void Compile()
		{
			int stackIndex = m_localVariables.Count;
			// todo henning local variable getters should reuse stack places
			foreach (CNode node in m_nodes)
			{
				node.OutParameterStackIndex = stackIndex;
				stackIndex += node.GetOutParameterCount();
			}

			m_stackSize = stackIndex;

			foreach (CNode node in m_nodes)
			{
				List<CInputPin> parameterConnections = node.InputPins;
				foreach (CInputPin inParameter in parameterConnections)
				{
					if (inParameter.SourceNode != null)
					{
						inParameter.StackIndex = inParameter.SourceNode.OutParameterStackIndex + inParameter.SourceParameterIndex;
					}
					else
					{
						inParameter.StackIndex = -1;
					}
				}
			}
		}

		public virtual void Execute()
		{
			if (m_nodes.Count <= 0)
			{
				return;
			}

			PrepareStack();
			Execute(m_nodes[0], null);
		}

		protected void PrepareStack()
		{
			m_stack.Clear();
			for (int i = 0; i < m_stackSize; i++)
			{
				m_stack.Add(null);
			}

			for (int i = 0; i < m_localVariables.Count; i++)
			{
				m_stack[i] = m_localVariables[i].Value;
			}

		}

		/// <summary>
		/// Executes the graph, returns the last node executed
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="firstPin"></param>
		/// <returns></returns>
		protected CNode Execute(CNode startNode, CExecutionPin firstPin)
		{
			s_nodeExecutionContext.graph = this;

			CExecutionPin nextExecutionPin = firstPin;
			CNode nextNode = startNode;
			while (nextNode != null)
			{
				m_inParameterBuffer.Clear();
				m_outParameterBuffer.Clear();

				CNode nodeToExecute = nextNode;
				s_nodeExecutionContext.calledPin = null;

				// We first execute all implicit nodes 
				for (int i = 0; i < nodeToExecute.GetInParameterCount(); i++)
				{
					CInputPin inputPin = nodeToExecute.InputPins[i];
					if (inputPin.SourceNode != null && inputPin.SourceNode.IsImplicit)
					{
						ExecuteImplicitNode(inputPin.SourceNode);
					}
				}

				// Afterwards the parameters are added to the buffer
				for (int i = 0; i < nodeToExecute.GetInParameterCount(); i++)
				{
					int stackIndex = nodeToExecute.InputPins[i].StackIndex;
					if (stackIndex >= 0)
					{
						m_inParameterBuffer.Add(m_stack[stackIndex]);
					}
					else
					{
						m_inParameterBuffer.Add(null);
					}
				}

				s_nodeExecutionContext.calledPin = nextExecutionPin != null ? nextExecutionPin.TargetPin : null;
				nextExecutionPin = nodeToExecute.Execute(s_nodeExecutionContext, m_inParameterBuffer, m_outParameterBuffer);

				if (nextExecutionPin == null || nextExecutionPin.TargetNode == null)
				{
					nextExecutionPin = null;

					int count = m_returnPointNodes.Count;
					if (count > 0)
					{
						nextNode = m_returnPointNodes[count - 1];
						m_returnPointNodes.RemoveAt(count - 1);
						continue;
					}
					else
					{
						return nextNode;
					}
				}

				nextNode = nextExecutionPin.TargetNode;
				AddOutParametersToStack(nodeToExecute.OutParameterStackIndex, nodeToExecute.GetOutParameterCount());
			}

			return null;
		}

		private void ExecuteImplicitNode(CNode implicitNode)
		{
			m_inParameterBuffer.Clear();
			m_outParameterBuffer.Clear();
			// We first execute all implicit nodes 
			for (int i = 0; i < implicitNode.GetInParameterCount(); i++)
			{
				CInputPin inputPin = implicitNode.InputPins[i];
				if (inputPin.SourceNode != null && inputPin.SourceNode.IsImplicit)
				{
					ExecuteImplicitNode(inputPin.SourceNode);
				}
			}

			// Afterwards the parameters are added to the buffer
			for (int i = 0; i < implicitNode.GetInParameterCount(); i++)
			{
				int stackIndex = implicitNode.InputPins[i].StackIndex;
				if (stackIndex >= 0)
				{
					m_inParameterBuffer.Add(m_stack[stackIndex]);
				}
				else
				{
					m_inParameterBuffer.Add(null);
				}
			}

			// After all input parameters are resolved we execute the node and add it's output parameters to the stack			
			implicitNode.Execute(s_nodeExecutionContext, m_inParameterBuffer, m_outParameterBuffer);

			AddOutParametersToStack(implicitNode.OutParameterStackIndex, implicitNode.GetOutParameterCount());
			m_inParameterBuffer.Clear();
			m_outParameterBuffer.Clear();
		}

		protected void AddOutParametersToStack(int startIndex, int parameterCount)
		{
			for (int i = 0; i < parameterCount; i++)
			{
				m_stack[i + startIndex] = m_outParameterBuffer[i];
			}
		}

		public void AddReturnPointNode(CNode node)
		{
			m_returnPointNodes.Add(node);
		}

		public void RemoveReturnPointNode(CNode node)
		{
			m_returnPointNodes.Remove(node);
		}

		[JsonIgnore]
		public List<CKlaxVariable> LocalVariables
		{
			get { return m_localVariables; }
		}

		internal void PrepareNodesForSerialization(CKlaxScriptObject outerScriptObject)
		{
			CScriptSerializer.PrepareNodesForSerialization(m_nodes);
			Dictionary<CKlaxVariable, int> objectVariableToIndex = new Dictionary<CKlaxVariable, int>();
			for (var i = 0; i < outerScriptObject.KlaxVariables.Count; i++)
			{
				objectVariableToIndex.Add(outerScriptObject.KlaxVariables[i], i);
			}

			Dictionary<CKlaxVariable, int> localVariableToIndex = new Dictionary<CKlaxVariable, int>();
			for (int i = 0; i < m_localVariables.Count; i++)
			{
				localVariableToIndex.Add(m_localVariables[i], i);
			}

			for (int i = 0; i < m_nodes.Count; i++)
			{
				if (m_nodes[i] is CKlaxVariableNode variableNode)
				{
					if (variableNode.SourceVariable == null)
					{
						throw new NullReferenceException("Tried to serialize a KlaxVariable node without a valid reference to an existing variable");
					}

					if (objectVariableToIndex.TryGetValue(variableNode.SourceVariable, out int index))
					{
						variableNode.m_sourceVariableIndex = index;
						variableNode.m_bIsLocalVariable = false;
						continue;
					}

					if (localVariableToIndex.TryGetValue(variableNode.SourceVariable, out int localIndex))
					{
						variableNode.m_sourceVariableIndex = localIndex;
						variableNode.m_bIsLocalVariable = true;
						continue;
					}

					throw new Exception("KlaxVariable Node Referenced a variable that does not exist in its context");
				}
			}
		}

		internal void ResolveNodesAfterSerialization(CKlaxScriptObject outerScriptObject)
		{
			ScriptableObject = outerScriptObject;
			for (int i = 0; i < m_nodes.Count; i++)
			{
				if (m_nodes[i] is CKlaxVariableNode variableNode)
				{
					int varIndex = variableNode.m_sourceVariableIndex;
					variableNode.m_sourceVariable = variableNode.m_bIsLocalVariable ? m_localVariables[varIndex] : outerScriptObject.KlaxVariables[varIndex];
				}
				else if (m_nodes[i] is CExecuteCustomFunctionNode functionNode)
				{
					functionNode.ResolveFunctionReference(outerScriptObject);
				}
			}
			CScriptSerializer.ResolveNodeReferences(m_nodes);
		}

		internal void PrepareNodesForCopy(IList<CNode> nodes)
		{
			CScriptSerializer.PrepareNodesForSerialization(nodes);
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i] is CKlaxVariableNode variableNode)
				{
					if (variableNode.SourceVariable == null)
					{
						throw new NullReferenceException("Tried to serialize a KlaxVariable node without a valid reference to an existing variable");
					}

					variableNode.m_sourceVariableGuid = variableNode.SourceVariable.Guid;
				}
			}
		}

		internal void ResolveNodesForPaste(IList<CNode> nodes)
		{
			List<CNode> invalidNodes = new List<CNode>();
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				if (nodes[i] is CKlaxVariableNode variableNode)
				{
					bool bFoundVariable = false;
					foreach (CKlaxVariable localVariable in m_localVariables)
					{
						if (localVariable.Guid == variableNode.m_sourceVariableGuid)
						{
							variableNode.SourceVariable = localVariable;
							bFoundVariable = true;
							break;
						}
					}

					if (!bFoundVariable)
					{
						foreach (CKlaxVariable objectVariable in ScriptableObject.KlaxVariables)
						{
							if (objectVariable.Guid == variableNode.m_sourceVariableGuid)
							{
								bFoundVariable = true;
								variableNode.SourceVariable = objectVariable;
								break;
							}
						}
					}

					if (!bFoundVariable)
					{
						nodes.RemoveAt(i);
					}
				}
				else if (nodes[i] is CExecuteCustomFunctionNode functionNode)
				{
					functionNode.ResolveFunctionReference(ScriptableObject);
				}
			}
			CScriptSerializer.ResolveNodeReferences(nodes);
		}

		[JsonProperty]
		[JsonConverter(typeof(CUseScriptSerializerConverter))]
		internal List<CKlaxVariable> m_localVariables = new List<CKlaxVariable>();
		[JsonProperty]
		[JsonConverter(typeof(CUseScriptSerializerConverter))]
		internal List<CNode> m_nodes = new List<CNode>();

		private List<CNode> m_returnPointNodes = new List<CNode>(4);

		public string Name { get; set; } = "Graph";
		public CKlaxScriptObject ScriptableObject { get; set; }

		protected int m_stackSize = 100;
		protected List<object> m_stack = new List<object>(500);
		protected List<object> m_inParameterBuffer = new List<object>(10);
		protected List<object> m_outParameterBuffer = new List<object>(4);
	}
}
