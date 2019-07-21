using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using KlaxRenderer.Graphics.ResourceManagement;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics.Shader
{
	class CShaderResource : CResource
	{
		internal override void InitFromAsset(Device device, CAsset asset)
		{
			CShaderAsset shaderAsset = new CShaderAsset();
			System.Diagnostics.Debug.Assert(shaderAsset != null && shaderAsset.IsLoaded);
			Shader = CRenderer.Instance.ResourceManager.RequestShader(shaderAsset.ShaderName);
		}

		internal override void InitWithContext(Device device, DeviceContext deviceContext)
		{}

		internal override bool IsAssetCorrectType(CAsset asset)
		{
			return asset is CShaderAsset;
		}

		public override void Dispose()
		{
			Shader.Dispose();
		}

		public CShader Shader { get; internal set; }
	}
}
