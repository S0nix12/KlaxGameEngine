using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxIO.AssetManager.Assets
{
	public struct SMaterialParameterEntry
	{
		public SMaterialParameterEntry(SHashedName inName, SShaderParameter inParameter)
		{
			name = inName;
			parameter = inParameter;
		}

		public SHashedName name;
		public SShaderParameter parameter;
	}
	public class CMaterialAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxmaterial";
		public const string TYPE_NAME = "Material";
		public static readonly Color4 TYPE_COLOR = new Color(130, 0, 191).ToColor4();

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
			CMaterialAsset sourceMaterial = (CMaterialAsset) source;
			MaterialParameters = sourceMaterial.MaterialParameters;
			Shader = sourceMaterial.Shader;
		}

		public List<SMaterialParameterEntry> MaterialParameters { get; internal set; } = new List<SMaterialParameterEntry>();
		public CAssetReference<CShaderAsset> Shader { get; internal set; }
	}
}
