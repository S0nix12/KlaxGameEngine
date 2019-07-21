using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using SharpDX;

namespace KlaxIO.AssetManager.Assets
{
	public class CShaderAsset : CAsset
	{
		public const string FILE_EXTENSION = ".klaxshader";
		public const string TYPE_NAME = "Shader";
		public static readonly Color4 TYPE_COLOR = new Color(7, 108, 44).ToColor4();
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
			CShaderAsset shaderSource = (CShaderAsset)source;
			ShaderName = shaderSource.ShaderName;
			ShaderBytecode = shaderSource.ShaderBytecode;
		}

		public SHashedName ShaderName { get; internal set; }
		public byte[] ShaderBytecode { get; internal set; }
	}
}
