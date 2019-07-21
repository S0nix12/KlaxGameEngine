using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxCore.GameFramework.Assets.Serializer;
using KlaxIO.AssetManager.Assets;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.GameFramework.Assets
{
	public class CLevelAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxlevel";
		public const string TYPE_NAME = "Level";
		public static readonly Color4 TYPE_COLOR = new Color4(207f / 255f, 101f / 255f, 13f / 255f, 1f);

		public override string GetFileExtension()
		{
			return FILE_EXTENSION;
		}

		public override string GetTypeName()
		{
			return TYPE_NAME;
		}

		public override Color4 GetTypeColor()
		{
			return TYPE_COLOR;
		}

		[JsonConstructor]
		public CLevelAsset()
		{

		}

		/// <summary>
		/// Create a new level asset from the given level, the asset will not be registered in the asset registry
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		public CLevelAsset(CLevel level, string name)
		{
			Name = name;
			m_levelJson = CEntitySerializer.Instance.Serialize(level);
			IsLoaded = true;
		}

		/// <summary>
		/// Creates a new level asset from the given level and register it in the asset registry
		/// </summary>
		/// <param name="level">Level to save in this asset</param>
		/// <param name="name">Name of the asset</param>
		/// <param name="assetPath">Path to save the asset</param>
		/// <param name="bAlwaysCreateNewAsset">If true always create a new asset even if in the target path an asset with the same name exists</param>
		public CLevelAsset(CLevel level, string name, string assetPath, bool bAlwaysCreateNewAsset = true)
		{
			Name = name;
			m_levelJson = CEntitySerializer.Instance.Serialize(level);
			if (CAssetRegistry.Instance.RequestRegisterAsset(this, assetPath, out CLevelAsset existingAsset, bAlwaysCreateNewAsset))
			{
				existingAsset.WaitUntilLoaded();
				base.CopyFrom(existingAsset);
				m_levelJson = existingAsset.m_levelJson;
			}
			else
			{
				IsLoaded = true;
			}
		}

		public CLevel GetLevel()
		{
			return CEntitySerializer.Instance.DeserializeObject<CLevel>(m_levelJson);
		}

		public void SetLevel(CLevel level)
		{
			m_levelJson = CEntitySerializer.Instance.Serialize(level);
		}

		public override void Dispose()
		{
		}

		[JsonProperty]
		[JsonConverter(typeof(CRawJsonConverter))]
		private string m_levelJson;
	}
}
