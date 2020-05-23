using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Lights
{
	class CShadowMapSamplers : IDisposable
	{
		private const int ShadowMapSamplerBaseSlot = 10;

		public void InitSamplers(Device device)
		{
			SamplerStateDescription shadowMapSamplerDesc = new SamplerStateDescription
			{
				Filter = Filter.MinMagLinearMipPoint,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
				MipLodBias = 0.0f,
				MaximumAnisotropy = 1,
				ComparisonFunction = Comparison.Always,
				BorderColor = SharpDX.Color4.Black,
				MinimumLod = 0,
				MaximumLod = 0
			};

			m_shadowMapSampler = new SamplerState(device, shadowMapSamplerDesc);

			SamplerStateDescription shadowMapCompSamplerDesc = new SamplerStateDescription
			{
				Filter = Filter.ComparisonMinMagMipLinear,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
				MipLodBias = 0.0f,
				MaximumAnisotropy = 1,
				ComparisonFunction = Comparison.Less,
				BorderColor = SharpDX.Color4.Black,
				MinimumLod = 0,
				MaximumLod = 0
			};

			m_shadowMapComparisionSampler = new SamplerState(device, shadowMapCompSamplerDesc);
		}

		public void SetSamplers(DeviceContext deviceContext)
		{
			deviceContext.PixelShader.SetSampler(ShadowMapSamplerBaseSlot, m_shadowMapSampler);
			deviceContext.PixelShader.SetSampler(ShadowMapSamplerBaseSlot + 1, m_shadowMapComparisionSampler);
		}

		public void Dispose()
		{
			m_shadowMapSampler?.Dispose();
			m_shadowMapComparisionSampler?.Dispose();
		}
		
		private SamplerState m_shadowMapSampler;
		private SamplerState m_shadowMapComparisionSampler;
	}
}
