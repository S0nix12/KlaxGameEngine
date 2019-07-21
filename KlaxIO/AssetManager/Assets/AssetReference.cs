using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KlaxIO.AssetManager.Assets
{
	public class CAssetReference<T> where T : CAsset, new()
	{
		public static implicit operator T(CAssetReference<T> reference)
		{
			return reference?.GetAsset();
		}

		public static implicit operator CAssetReference<T>(T asset)
		{
			return new CAssetReference<T>(asset);
		}

		public static explicit operator CAssetReference<T>(string guideAsString)
		{
			return new CAssetReference<T>(new Guid(guideAsString));
		}

		public static bool operator ==(CAssetReference<T> a, CAssetReference<T> b)
		{
			return a?.AssetGuid == b?.AssetGuid;
		}
		public static bool operator !=(CAssetReference<T> a, CAssetReference<T> b)
		{
			return a?.AssetGuid != b?.AssetGuid;
		}

		public override bool Equals(object obj)
		{
			CAssetReference<T> other = obj as CAssetReference<T>;
			if (other == null)
			{
				return false;
			}

			return other.AssetGuid.Equals(AssetGuid);
		}

		public override int GetHashCode()
		{
			return AssetGuid.GetHashCode();
		}

		public override string ToString()
		{
			return AssetGuid.ToString();
		}

		public CAssetReference(T asset)
		{
			System.Diagnostics.Debug.Assert(asset != null);
			AssetGuid = asset.Guid;
			m_assetReference = asset;
		}

		[JsonConstructor]
		public CAssetReference(Guid assetGuid)
		{
			AssetGuid = assetGuid;
		}

		public T GetAsset()
		{
			if (m_assetReference == null)
			{
				m_assetReference = CAssetRegistry.Instance.GetAsset<T>(AssetGuid);
			}

			return m_assetReference;
		}

		public Guid AssetGuid { get; }
		
		private T m_assetReference;
	}
}
