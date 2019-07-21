using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxCore.KlaxScript.Serialization
{
	class CUseScriptSerializerConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is CGraph graph)
			{
				CScriptSerializer.PrepareNodesForSerialization(graph.m_nodes);
			}
			else if(value is IList<CNode> nodeList)
			{
				CScriptSerializer.PrepareNodesForSerialization(nodeList);
			}
			CScriptSerializer.Instance.SerializeToWriter(value, writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			object outObject = CScriptSerializer.Instance.DeserializeFromReader(reader, objectType);
			if (outObject is CGraph graph)
			{
				CScriptSerializer.ResolveNodeReferences(graph.m_nodes);
			}
			else if(outObject is IList<CNode> nodeList)
			{
				CScriptSerializer.ResolveNodeReferences(nodeList);
			}

			return outObject;
		}

		public override bool CanConvert(Type objectType)
		{
			return true;
		}
	}
}
