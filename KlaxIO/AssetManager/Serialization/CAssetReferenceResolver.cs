using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using Newtonsoft.Json.Serialization;

namespace KlaxIO.AssetManager.Serialization
{
	/// <summary>
	/// This reference resolver will use the asset registry to convert asset references to guids and visa versa
	/// For other types it falls back to the given default resolver
	/// </summary>
	class CAssetReferenceResolver : IReferenceResolver
	{
		public CAssetReferenceResolver(IReferenceResolver defaultResolver, CAssetRegistry assetRegistry)
		{
			m_defaultResolver = defaultResolver;
			m_assetRegistry = assetRegistry;
		}

		public object ResolveReference(object context, string reference)
		{
			if (reference.StartsWith("A:"))
			{
				Guid assetGuid = new Guid(reference.Substring(2));
				//return m_assetRegistry.GetAsset<CAsset>(assetGuid);
			}

			return m_defaultResolver.ResolveReference(context, reference);
		}

		public string GetReference(object context, object value)
		{
			if (value is CAsset asset)
			{
				return "A:" + asset.Guid;
			}

			return m_defaultResolver.GetReference(context, value);
		}

		public bool IsReferenced(object context, object value)
		{
			if (value is CAsset)
			{
				return true;
			}

			return m_defaultResolver.IsReferenced(context, value);
		}

		public void AddReference(object context, string reference, object value)
		{
			if (!(value is CAsset))
			{
				m_defaultResolver.AddReference(context, reference, value);
			}
		}

		private readonly IReferenceResolver m_defaultResolver;
		private readonly CAssetRegistry m_assetRegistry;
	}
}