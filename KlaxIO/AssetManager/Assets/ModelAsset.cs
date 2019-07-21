using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxIO.AssetManager.Assets
{
	public struct SMeshChild
	{
		public CAssetReference<CMeshAsset> meshAsset;
		public Matrix relativeTransform;
	}

	public class CModelAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxmodel";
		public const string TYPE_NAME = "Model";
		public static readonly Color4 TYPE_COLOR = new Color(42, 96, 191).ToColor4();
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

		public override void Dispose()
		{

		}

		public override void CopyFrom(CAsset source)
		{
			base.CopyFrom(source);
			CModelAsset modelSource = (CModelAsset) source;
			MeshChildren = modelSource.MeshChildren;
		}

		public List<SMeshChild> MeshChildren { get; internal set; } = new List<SMeshChild>();
	}
}
