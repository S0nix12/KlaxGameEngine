using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Graphics.Texture
{
	class CDepthTexture : CTexture
	{
		public void InitEmpty(Device device, in Texture2DDescription description)
		{
			m_texture = new Texture2D(device, description);

			ShaderResourceViewDescription srvDescription = new ShaderResourceViewDescription()
			{
				Format = Format.R32_Float,
				Dimension = ShaderResourceViewDimension.Texture2D
			};

			srvDescription.Texture2D.MostDetailedMip = 0;
			srvDescription.Texture2D.MipLevels = description.MipLevels;

			m_textureView = new ShaderResourceView(device, m_texture, srvDescription);

			DepthStencilViewDescription viewDescription = new DepthStencilViewDescription()
			{
				Dimension = DepthStencilViewDimension.Texture2D,
				Flags = DepthStencilViewFlags.None,
				Format = Format.D32_Float,				
			};
			
			viewDescription.Texture2D.MipSlice = 0;

			m_depthStencilView = new DepthStencilView(device, m_texture, viewDescription);
		}

		public DepthStencilView GetRenderTarget()
		{
			return m_depthStencilView;
		}

		public override void Dispose()
		{
			base.Dispose();
			m_depthStencilView.Dispose();
		}

		private DepthStencilView m_depthStencilView = null;
	}
}
