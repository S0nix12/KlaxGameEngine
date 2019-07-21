using KlaxShared.Attributes;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using KlaxIO.AssetManager.Assets;
using KlaxRenderer.Graphics.ResourceManagement;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Graphics
{
	public class CTextureSampler : CResource
	{
		[CVar(8)]
		static int TextureAnisotropyLevel
		{ get; set; } = 8;

		[CVar(Filter.Anisotropic)]
		static Filter TextureFilter
		{ get; set; } = Filter.Anisotropic;

		[CVar(0.0f)]
		static float TextureMipLodBias
		{ get; set; } = 0.0f;

		[CVar()]
		static Color4 TextureBorderColor
		{ get; set; } = Color4.Black;

		public CTextureSampler()
		{}

		public CTextureSampler(Device device, DeviceContext deviceContext, string filePath)
		{
			Texture = new CTexture();
			Texture.InitFromFile(device, deviceContext, filePath);
			InitDefaultSampler(device);
			FinishLoading(); // This is a synchronous operation so we are finished loading directly
		}

		public CTextureSampler(Device device, DeviceContext deviceContext, string filePath, in SamplerStateDescription samplerDescription)
		{
			Texture = new CTexture();
			Texture.InitFromFile(device, deviceContext, filePath);
			SamplerState = new SamplerState(device, samplerDescription);
			FinishLoading(); // This is a synchronous operation so we are finished loading directly
		}

		public CTextureSampler(Device device, DeviceContext deviceContext, CTexture texture, in SamplerStateDescription samplerDescription)
		{
			Texture = texture;
			SamplerState = new SamplerState(device, samplerDescription);
			FinishLoading();
		}

		public void Init(CTexture texture, SamplerState samplerState)
		{
			Texture = texture;
			SamplerState = samplerState;
		}

		private void InitDefaultSampler(Device device)
		{
			SamplerStateDescription samplerStateDescription = new SamplerStateDescription
			{
				Filter = TextureFilter,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				MipLodBias = TextureMipLodBias,
				MaximumAnisotropy = TextureAnisotropyLevel,
				ComparisonFunction = Comparison.Always,
				BorderColor = TextureBorderColor,
				MinimumLod = 0,
				MaximumLod = 32
			};


			SamplerState = new SamplerState(device, samplerStateDescription);
		}
		internal override void InitFromAsset(Device device, CAsset asset)
		{
			System.Diagnostics.Debug.Assert(asset.IsLoaded);
			CTextureAsset textureAsset = (CTextureAsset)asset;

			//todo henning implement offline mip level generation to make texture loading more efficient
			Texture = new CTexture();
			m_bCreateMips = true;
			m_sourceAsset = textureAsset;
			InitDefaultSampler(device);
		}

		internal override void InitWithContext(Device device, DeviceContext deviceContext)
		{
			//todo henning implement offline mip level generation to make texture loading more efficient
			Texture2DDescription desc;
			desc.Width = m_sourceAsset.ImageSurface.Width;
			desc.Height = m_sourceAsset.ImageSurface.Height;
			desc.ArraySize = 1;
			desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
			desc.Usage = ResourceUsage.Default;
			desc.CpuAccessFlags = CpuAccessFlags.None;
			desc.Format = Format.B8G8R8A8_UNorm;
			desc.MipLevels = 0;
			desc.OptionFlags = ResourceOptionFlags.GenerateMipMaps;
			desc.SampleDescription.Count = 1;
			desc.SampleDescription.Quality = 0;
			//m_sourceAsset.ImageSurface.FlipVertically();
			Texture.InitFromData(device, deviceContext, m_sourceAsset.ImageSurface.DataPtr, m_sourceAsset.ImageSurface.Pitch, in desc, true);
			FinishLoading();
		}
		internal override bool NeedsContext()
		{
			return m_bCreateMips;
		}

		internal override bool IsAssetCorrectType(CAsset asset)
		{
			return asset is CTextureAsset;
		}

		public override void Dispose()
		{
			SamplerState?.Dispose();
			Texture?.Dispose();
		}

		public CTexture Texture
		{ get; set; } = new CTexture();

		public SamplerState SamplerState
		{ get; protected set; }

		protected Device m_targetDevice;
		protected CTextureAsset m_sourceAsset;
		private bool m_bCreateMips;
	}
}
