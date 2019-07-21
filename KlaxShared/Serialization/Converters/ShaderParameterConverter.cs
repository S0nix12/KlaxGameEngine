using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KlaxShared.Definitions.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX;

namespace KlaxShared.Serialization.Converters
{
	public class CShaderParameterConverter : JsonConverter<SShaderParameter>
	{
		static CShaderParameterConverter()
		{
			m_defaultSettings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			m_defaultSettings.NullValueHandling = NullValueHandling.Ignore;
			m_defaultSettings.TypeNameHandling = TypeNameHandling.All;
			m_defaultSettings.Formatting = Formatting.Indented;
		}

		public override void WriteJson(JsonWriter writer, SShaderParameter value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("parameterType");
			writer.WriteValue((int)value.parameterType);

			writer.WritePropertyName("parameterData");
			switch (value.parameterType)
			{
				case EShaderParameterType.Scalar:
					WriteScalar(writer, (float)value.parameterData);
					break;
				case EShaderParameterType.Vector:
					WriteVector(writer, (Vector3)value.parameterData);
					break;
				case EShaderParameterType.Color:
					WriteColor(writer, (Vector4)value.parameterData);
					break;
				case EShaderParameterType.Matrix:
					WriteMatrix(writer, (Matrix)value.parameterData);
					break;
				case EShaderParameterType.Texture:
					JsonSerializer.Create(m_defaultSettings).Serialize(writer, value.parameterData, typeof(object));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			writer.WriteEndObject();
		}

		private void WriteScalar(JsonWriter writer, float data)
		{
			writer.WriteValue(data);			
		}

		private void WriteVector(JsonWriter writer, Vector3 vector)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(vector.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(vector.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(vector.Z);
			writer.WriteEndObject();
		}

		private void WriteColor(JsonWriter writer, Vector4 color)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(color.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(color.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(color.Z);
			writer.WritePropertyName("W");
			writer.WriteValue(color.W);
			writer.WriteEndObject();
		}

		private void WriteMatrix(JsonWriter writer, in Matrix matrix)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("MatrixValues");
			writer.WriteStartArray();
			float[] matrixArray = matrix.ToArray();
			for (int i = 0; i < matrixArray.Length; i++)
			{
				writer.WriteValue(matrixArray[i]);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		private void WriteTexture(JsonWriter writer, object texture)
		{
			PropertyInfo assetGuidInfo = texture.GetType().GetProperty("AssetGuid");
			if (assetGuidInfo == null)
			{
				return;
			}
			Guid assetGuid = (Guid)assetGuidInfo.GetValue(texture);
			writer.WriteStartObject();
			writer.WritePropertyName("AssetGuid");
			writer.WriteValue(assetGuid);
			writer.WriteEndObject();
		}

		public override SShaderParameter ReadJson(JsonReader reader, Type objectType, SShaderParameter existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			SShaderParameter outParameter = new SShaderParameter();
			JObject jObject = JObject.Load(reader);
			outParameter.parameterType = (EShaderParameterType)jObject.Value<int>("parameterType");

			switch (outParameter.parameterType)
			{
				case EShaderParameterType.Scalar:
					outParameter.parameterData = ReadScalar(jObject);
					break;
				case EShaderParameterType.Vector:
					outParameter.parameterData = ReadVector(jObject);
					break;
				case EShaderParameterType.Color:
					outParameter.parameterData = ReadColor(jObject);
					break;
				case EShaderParameterType.Matrix:
					outParameter.parameterData = ReadMatrix(jObject);
					break;
				case EShaderParameterType.Texture:
					outParameter.parameterData = ReadTexture(jObject);
					break;
			}

			return outParameter;
		}

		private float ReadScalar(JObject jObject)
		{
			return jObject.Value<float>("parameterData");
		}

		private Vector3 ReadVector(JObject jObject)
		{
			JObject vectorObject = jObject.Value<JObject>("parameterData");
			return new Vector3(vectorObject.Value<float>("X"), vectorObject.Value<float>("Y"), vectorObject.Value<float>("Z"));
		}

		private Vector4 ReadColor(JObject jObject)
		{
			JObject colorObject = jObject.Value<JObject>("parameterData");
			return new Vector4(colorObject.Value<float>("X"), colorObject.Value<float>("Y"), colorObject.Value<float>("Z"), colorObject.Value<float>("W"));
		}

		private Matrix ReadMatrix(JObject jObject)
		{
			JObject matrixObject = jObject.Value<JObject>("parameterData");
			JArray jArray = matrixObject["MatrixValues"] as JArray;
			float[] matrixValues = jArray.Select(jv => (float)jv).ToArray();
			return new Matrix(matrixValues);
		}

		private object ReadTexture(JObject jObject)
		{
			JObject textureObject = jObject.Value<JObject>("parameterData");
			return textureObject == null ? null : JsonSerializer.Create(m_defaultSettings).Deserialize(textureObject.CreateReader());
		}

		private static JsonSerializerSettings m_defaultSettings = new JsonSerializerSettings();
	}
}
