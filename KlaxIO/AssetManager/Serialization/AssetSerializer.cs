using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Serialization.TypeConverters;
using Newtonsoft.Json;

namespace KlaxIO.AssetManager.Serialization
{
	class CAssetSerializer : CJsonSerializer
	{
		protected CAssetSerializer()
		{
			m_serializerSettings = new JsonSerializerSettings();
			m_serializerSettings.Formatting = Formatting.Indented;
			m_serializerSettings.Converters.Add(new CVector2Converter());
			m_serializerSettings.Converters.Add(new CVector3Converter());
			//m_serializerSettings.Converters.Add(new CVector4Converter());
			m_serializerSettings.Converters.Add(new CColorConverter());
			m_serializerSettings.Converters.Add(new CColor4Converter());
			m_serializerSettings.Converters.Add(new CQuaternionConverter());
			m_serializerSettings.Converters.Add(new CMatrixConverter());
			m_serializerSettings.Converters.Add(new CHashedNameConverter());
			m_serializerSettings.NullValueHandling = NullValueHandling.Ignore;
			m_serializerSettings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			m_serializerSettings.TypeNameHandling = TypeNameHandling.Auto;
		}

		public static CAssetSerializer Instance { get; } = new CAssetSerializer();
	}
}
