using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Serialization;
using KlaxIO.AssetManager.Serialization.TypeConverters;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Serialization
{
	class CScriptSerializer : CJsonSerializer
	{
		public CScriptSerializer()
		{
			m_serializerSettings = new JsonSerializerSettings();
			m_serializerSettings.Formatting = Formatting.Indented;
			m_serializerSettings.NullValueHandling = NullValueHandling.Ignore;
			m_serializerSettings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			m_serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
			m_serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			m_serializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
			m_serializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
			m_serializerSettings.FloatParseHandling = FloatParseHandling.Single;
			m_serializerSettings.Converters.Add(new CTypeJsonConverter());
			m_serializerSettings.Converters.Add(new CFieldInfoConverter());
			m_serializerSettings.Converters.Add(new CPropertyInfoConverter());
			m_serializerSettings.Converters.Add(new CMethodInfoConverter());
			m_serializerSettings.Converters.Add(new CEventInfoConverter());
		}

		/// <summary>
		/// Prepares a list of nodes to be serialized by converting their internal references to indices external referenced nodes will not be saved
		/// </summary>
		/// <param name="nodes">List of nodes that should be serialized</param>
		public static void PrepareNodesForSerialization(IList<CNode> nodes)
		{
			Dictionary<CNode, int> nodesToIndex = new Dictionary<CNode, int>();
			for (int i = 0; i < nodes.Count; i++)
			{
				nodesToIndex.Add(nodes[i], i);
			}

			foreach (CNode node in nodes)
			{
				foreach (CExecutionPin outExecutionPin in node.OutExecutionPins)
				{
					if (outExecutionPin.TargetNode != null && nodesToIndex.TryGetValue(outExecutionPin.TargetNode, out int targetIndex))
					{
						outExecutionPin.TargetNodeIndex = targetIndex;

						List<CExecutionPin> pins = outExecutionPin.TargetNode.InExecutionPins;
						for (int i = 0; i < pins.Count; i++)
						{
							if (pins[i] == outExecutionPin.TargetPin)
							{
								outExecutionPin.TargetPinIndex = i;
								break;
							}
						}
					}
					else
					{
						outExecutionPin.TargetNodeIndex = -1;
						outExecutionPin.TargetPinIndex = -1;
					}
				}

				foreach (CInputPin inputPin in node.InputPins)
				{
					if (inputPin.SourceNode != null && nodesToIndex.TryGetValue(inputPin.SourceNode, out int targetIndex))
					{
						inputPin.SourceNodeIndex = targetIndex;
					}
					else
					{
						inputPin.SourceNodeIndex = -1;
					}
				}
			}
		}

		/// <summary>
		/// Resolves references in a list of deserialized nodes only references inside the list can be resolved
		/// </summary>
		/// <param name="deserializedNodes"></param>
		public static void ResolveNodeReferences(IList<CNode> deserializedNodes)
		{
			foreach (CNode node in deserializedNodes)
			{
				foreach (CExecutionPin outExecutionPin in node.OutExecutionPins)
				{
					if (outExecutionPin.TargetNodeIndex >= 0)
					{
						if (outExecutionPin.TargetNodeIndex >= deserializedNodes.Count)
						{
							outExecutionPin.TargetNode = null;
							outExecutionPin.TargetNodeIndex = -1;
							outExecutionPin.TargetPinIndex = -1;

							LogUtility.Log("Could not resolve connection for execution pin {0} in {1}", outExecutionPin.Name, node.Name);
							continue;
						}

						//todo henning we could do much more error checking here (check that source type is valid etc.)
						outExecutionPin.TargetNode = deserializedNodes[outExecutionPin.TargetNodeIndex];
						outExecutionPin.TargetPin = outExecutionPin.TargetNode.InExecutionPins[outExecutionPin.TargetPinIndex];
					}
				}

				foreach (CInputPin inputPin in node.InputPins)
				{
					if (inputPin.SourceNodeIndex >= 0)
					{
						if (inputPin.SourceNodeIndex >= deserializedNodes.Count)
						{
							inputPin.SourceNode = null;
							inputPin.SourceNodeIndex = -1;
							inputPin.SourceParameterIndex = -1;

							LogUtility.Log("Could not resolve connection for input pin {0} in {1}", inputPin.Name, node.Name);
							continue;
						}

						//todo henning we could do much more error checking here (check that source type is valid etc.)
						inputPin.SourceNode = deserializedNodes[inputPin.SourceNodeIndex];
					}
				}
			}
		}

		public static CScriptSerializer Instance { get; } = new CScriptSerializer();
	}
}
