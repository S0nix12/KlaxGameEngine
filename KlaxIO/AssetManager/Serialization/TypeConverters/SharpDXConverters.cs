using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX;

namespace KlaxIO.AssetManager.Serialization.TypeConverters
{
	public class CVector2Converter : JsonConverter<Vector2>
	{
		public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
		{
			if (value.IsZero)
			{
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WriteEndObject();
		}

		public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Vector2(jobject.Value<float>("X"), jobject.Value<float>("Y"));
		}
	}
	public class CVector3Converter : JsonConverter<Vector3>
	{
		public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
		{
			if (value.IsZero)
			{
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(value.Z);
			writer.WriteEndObject();
		}

		public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Vector3(jobject.Value<float>("X"), jobject.Value<float>("Y"), jobject.Value<float>("Z"));
		}
	}
	
	public class CVector4Converter : JsonConverter<Vector4>
	{
		public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
		{
			if (value.IsZero)
			{
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(value.Z);
			writer.WritePropertyName("W");
			writer.WriteValue(value.W);
			writer.WriteEndObject();
		}

		public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Vector4(jobject.Value<float>("X"), jobject.Value<float>("Y"), jobject.Value<float>("Z"), jobject.Value<float>("W"));
		}
	}

	public class CColorConverter : JsonConverter<Color>
	{
		public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
		{
			if (value == default)
			{
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("R");
			writer.WriteValue(value.R);
			writer.WritePropertyName("G");
			writer.WriteValue(value.G);
			writer.WritePropertyName("B");
			writer.WriteValue(value.B);
			writer.WritePropertyName("A");
			writer.WriteValue(value.A);
			writer.WriteEndObject();
		}

		public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Color(jobject.Value<float>("R"), jobject.Value<float>("G"), jobject.Value<float>("B"), jobject.Value<float>("A"));
		}
	}

	public class CColor4Converter : JsonConverter<Color4>
	{
		public override void WriteJson(JsonWriter writer, Color4 value, JsonSerializer serializer)
		{
			if (value == default)
			{
				return;
			}
			
			writer.WriteStartObject();
			writer.WritePropertyName("Red");
			writer.WriteValue(value.Red);
			writer.WritePropertyName("Green");
			writer.WriteValue(value.Green);
			writer.WritePropertyName("Blue");
			writer.WriteValue(value.Blue);
			writer.WritePropertyName("Alpha");
			writer.WriteValue(value.Alpha);
			writer.WriteEndObject();
		}

		public override Color4 ReadJson(JsonReader reader, Type objectType, Color4 existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Color4(jobject.Value<float>("Red"), jobject.Value<float>("Green"), jobject.Value<float>("Blue"), jobject.Value<float>("Alpha"));
		}
	}

	public class CQuaternionConverter : JsonConverter<Quaternion>
	{
		public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
		{
			if (value == default)
			{
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("X");
			writer.WriteValue(value.X);
			writer.WritePropertyName("Y");
			writer.WriteValue(value.Y);
			writer.WritePropertyName("Z");
			writer.WriteValue(value.Z);
			writer.WritePropertyName("W");
			writer.WriteValue(value.W);
			writer.WriteEndObject();
		}

		public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			return new Quaternion(jobject.Value<float>("X"), jobject.Value<float>("Y"), jobject.Value<float>("Z"), jobject.Value<float>("W"));
		}
	}

	public class CMatrixConverter : JsonConverter<Matrix>
	{
		public override void WriteJson(JsonWriter writer, Matrix value, JsonSerializer serializer)
		{
			if (value == default)
			{
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("MatrixValues");
			writer.WriteStartArray();
			float[] matrixArray = value.ToArray();
			for (int i = 0; i < matrixArray.Length; i++)
			{
				writer.WriteValue(matrixArray[i]);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		public override Matrix ReadJson(JsonReader reader, Type objectType, Matrix existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			JArray jArray = jObject["MatrixValues"] as JArray;
			float[] matrixValues = jArray.Select(jv => (float) jv).ToArray();
			return new Matrix(matrixValues);
		}
	}
}
