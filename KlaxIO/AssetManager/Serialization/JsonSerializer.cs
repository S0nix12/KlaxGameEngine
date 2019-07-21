using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using KlaxIO.AssetManager.Serialization.TypeConverters;
using Newtonsoft.Json;

namespace KlaxIO.AssetManager.Serialization
{
	public abstract class CJsonSerializer
	{
		protected CJsonSerializer()
		{
		}

		public virtual string Serialize(object value)
		{
			return JsonConvert.SerializeObject(value, typeof(object), m_serializerSettings);			
		}

		public virtual void SerializeToStream(object value, Stream stream)
		{
			using (StreamWriter streamWriter = new StreamWriter(stream))
			{				
				JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter);
				JsonSerializer serializer = JsonSerializer.Create(m_serializerSettings);				
				serializer.Serialize(jsonWriter, value, typeof(object));
			}	
		}

		public virtual void SerializeToWriter(object value, JsonWriter writer)
		{
			JsonSerializer serializer = JsonSerializer.Create(m_serializerSettings);
			serializer.Serialize(writer, value, typeof(object));
		}

		public virtual object Deserialize(string json)
		{
			return JsonConvert.DeserializeObject(json, m_serializerSettings);
		}

		public virtual object DeserializeFromStream(Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				JsonReader jsonReader = new JsonTextReader(streamReader);
				JsonSerializer serializer = JsonSerializer.Create(m_serializerSettings);
				return serializer.Deserialize(jsonReader);
			}
		}
		public virtual object DeserializeFromReader(JsonReader reader, Type objectType)
		{
			JsonSerializer serializer = JsonSerializer.Create(m_serializerSettings);
			return serializer.Deserialize(reader, objectType);
		}

		public virtual T DeserializeFromReader<T>(JsonReader reader)
		{
			JsonSerializer serializer = JsonSerializer.Create(m_serializerSettings);
			return serializer.Deserialize<T>(reader);
		}

		public virtual T DeserializeFromStream<T>(Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				JsonReader jsonReader = new JsonTextReader(streamReader);
				JsonSerializer serializer = JsonSerializer.Create(m_serializerSettings);
				return serializer.Deserialize<T>(jsonReader);
			}
		}

		public virtual T DeserializeObject<T>(string json)
		{			
			return JsonConvert.DeserializeObject<T>(json, m_serializerSettings);
		}

		protected JsonSerializerSettings m_serializerSettings;
	}
}
