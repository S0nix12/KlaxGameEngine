using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KlaxCore.GameFramework.Assets.Serializer
{
	public class CRawJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Data");
			writer.WriteRawValue(value.ToString());
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return jobject["Data"].ToString();
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(string).IsAssignableFrom(objectType);
		}
	}
}
