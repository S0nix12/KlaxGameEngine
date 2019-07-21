using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KlaxIO.AssetManager.Serialization.TypeConverters
{
	public class CHashedNameConverter : JsonConverter<SHashedName>
	{
		public override void WriteJson(JsonWriter writer, SHashedName value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Name");
			writer.WriteValue(value.GetString());
			writer.WriteEndObject();
		}

		public override SHashedName ReadJson(JsonReader reader, Type objectType, SHashedName existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			return new SHashedName(jObject.Value<string>("Name"));
		}
	}
}
