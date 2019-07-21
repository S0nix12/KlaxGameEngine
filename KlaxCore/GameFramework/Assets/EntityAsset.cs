using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.GameFramework.Assets.Serializer;
using KlaxIO.AssetManager.Assets;
using KlaxShared.Definitions;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.GameFramework.Assets
{
	public class CEntityAsset<T> : CAsset where T : CEntity
	{
		public const string FILE_EXTENSION = ".klaxentity";
		public const string TYPE_NAME = "Entity";
		public static readonly Color4 TYPE_COLOR = new Color4(197f /255f, 128f / 255f, 7f / 255f, 1f);
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

		public static CEntityAsset<T> CreateFromEntity(T entity, string assetPath)
		{
			CEntityAsset<T> outAsset = new CEntityAsset<T> {Name = entity.Name, m_entityJson = CEntitySerializer.Instance.Serialize(entity)};
			if (CAssetRegistry.Instance.RequestRegisterAsset(outAsset, assetPath, out CEntityAsset<T> outExistingAsset, true))
			{
				outExistingAsset.WaitUntilLoaded();
				outAsset.CopyFrom(outExistingAsset);
				outAsset.m_entityJson = outExistingAsset.m_entityJson;
			}
			else
			{
				outAsset.IsLoaded = true;
			}

			return outAsset;
		}

		public CEntityAsset(T entity, string name)
		{
			Name = name;
			m_entityJson = CEntitySerializer.Instance.Serialize(entity);
			if (CAssetRegistry.Instance.RequestRegisterAsset(this, "Core/Entities/", out CEntityAsset<T> outExistingAsset))
			{
				outExistingAsset.WaitUntilLoaded();
				base.CopyFrom(outExistingAsset);
				m_entityJson = outExistingAsset.m_entityJson;
			}
			else
			{
				IsLoaded = true;
			}
		}

		[JsonConstructor]
		public CEntityAsset()
		{

		}

		public T GetEntity()
		{
			return CEntitySerializer.Instance.DeserializeObject<T>((string)m_entityJson);
		}

		public void SetEntity(CEntity entity)
		{
			m_entityJson = CEntitySerializer.Instance.Serialize(entity);
		}

		public override void CopyFrom(CAsset source)
		{
			base.CopyFrom(source);
			CEntityAsset<T> sourceAsset = (CEntityAsset<T>) source;
			m_entityJson = sourceAsset.m_entityJson;
		}

		public override void Dispose()
		{
			
		}

		[JsonProperty]
		[JsonConverter(typeof(CRawJsonConverter))]
		private string m_entityJson;
	}
}
