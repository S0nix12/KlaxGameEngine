using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KlaxCore.KlaxScript.Serialization
{
	class CTypeJsonConverter : JsonConverter<Type>
	{
		public override void WriteJson(JsonWriter writer, Type value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("ObjectType");
			writer.WriteValue(value.AssemblyQualifiedName);
			writer.WriteEndObject();
		}

		public override Type ReadJson(JsonReader reader, Type objectType, Type existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string typeName = jObject.Value<string>("ObjectType");
			return Type.GetType(typeName, true);
		}
	}
	class CKlaxTypeJsonConverter : JsonConverter<CKlaxScriptTypeInfo>
	{
		public override void WriteJson(JsonWriter writer, CKlaxScriptTypeInfo value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("ObjectType");
			writer.WriteValue(value.Type.AssemblyQualifiedName);
			writer.WriteEndObject();
		}

		public override CKlaxScriptTypeInfo ReadJson(JsonReader reader, Type objectType, CKlaxScriptTypeInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string typeName = jObject.Value<string>("ObjectType");
			Type reflectionType = Type.GetType(typeName, true);
			CKlaxScriptRegistry.Instance.TryGetTypeInfo(reflectionType, out CKlaxScriptTypeInfo outTypeInfo);
			return outTypeInfo;
		}
	}

	class CFieldInfoConverter : JsonConverter<FieldInfo>
	{
		public override void WriteJson(JsonWriter writer, FieldInfo value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("FieldParentType");
			writer.WriteValue(value.DeclaringType.AssemblyQualifiedName);
			writer.WritePropertyName("FieldName");
			writer.WriteValue(value.Name);
			writer.WriteEndObject();
		}

		public override FieldInfo ReadJson(JsonReader reader, Type objectType, FieldInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string declaringTypeName = jObject.Value<string>("FieldParentType");
			Type declaringType = Type.GetType(declaringTypeName, true);
			string fieldName = jObject.Value<string>("FieldName");

			return declaringType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
		}
	}

	class CPropertyInfoConverter : JsonConverter<PropertyInfo>
	{
		public override void WriteJson(JsonWriter writer, PropertyInfo value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("PropertyParentType");
			writer.WriteValue(value.DeclaringType.AssemblyQualifiedName);
			writer.WritePropertyName("PropertyName");
			writer.WriteValue(value.Name);
			writer.WriteEndObject();
		}

		public override PropertyInfo ReadJson(JsonReader reader, Type objectType, PropertyInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string declaringTypeName = jObject.Value<string>("PropertyParentType");
			Type declaringType = Type.GetType(declaringTypeName, true);
			string propertyName = jObject.Value<string>("PropertyName");

			return declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
		}
	}

	class CMethodInfoConverter : JsonConverter<MethodInfo>
	{
		public override void WriteJson(JsonWriter writer, MethodInfo value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("MethodParentType");
			writer.WriteValue(value.DeclaringType.AssemblyQualifiedName);
			writer.WritePropertyName("MethodName");
			writer.WriteValue(value.Name);
			writer.WriteEndObject();
		}

		public override MethodInfo ReadJson(JsonReader reader, Type objectType, MethodInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string declaringTypeName = jObject.Value<string>("MethodParentType");
			Type declaringType = Type.GetType(declaringTypeName, true);
			string methodName = jObject.Value<string>("MethodName");

			return declaringType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
		}
	}

	class CEventInfoConverter : JsonConverter<EventInfo>
	{
		public override void WriteJson(JsonWriter writer, EventInfo value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("EventParentType");
			writer.WriteValue(value.DeclaringType.AssemblyQualifiedName);
			writer.WritePropertyName("EventName");
			writer.WriteValue(value.Name);
			writer.WriteEndObject();
		}

		public override EventInfo ReadJson(JsonReader reader, Type objectType, EventInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			string declaringTypeName = jObject.Value<string>("EventParentType");
			Type declaringType = Type.GetType(declaringTypeName, true);
			string eventName = jObject.Value<string>("EventName");

			return declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
		}
	}
}
