using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KlaxCore.GameFramework.Assets.Serializer
{
	class CEntityAssetConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			dynamic dynObject = value;
			writer.WriteStartObject();
			writer.WritePropertyName("Guid");
			writer.WriteValue(dynObject.Guid.ToString());
			writer.WritePropertyName("Name");
			writer.WriteValue(dynObject.Name);

			Type type = value.GetType();
			FieldInfo fieldInfo = type.GetField("m_entityJson", BindingFlags.Instance | BindingFlags.NonPublic);
			writer.WritePropertyName("EntityData");
			writer.WriteRawValue(fieldInfo.GetValue(value).ToString());
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			dynamic outObject = Activator.CreateInstance(objectType);
			outObject.Name = jObject.Value<string>("Name");
			PropertyInfo guidInfo = objectType.GetProperty("Guid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			guidInfo.SetValue(outObject, new Guid(jObject.Value<string>("Guid")));
			FieldInfo jsonField = objectType.GetField("m_entityJson", BindingFlags.Instance | BindingFlags.NonPublic);
			jsonField.SetValue(outObject, jObject["EntityData"].ToString());
			return outObject;
		}

		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
	}
}
