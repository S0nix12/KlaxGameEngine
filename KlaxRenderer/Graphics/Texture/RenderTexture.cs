using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics.Texture
{
	class CRenderTexture : CTexture
	{
		public void InitEmpty(Device device, in Texture2DDescription description)
		{
			m_texture = new Texture2D(device, description);

			ShaderResourceViewDescription srvDescription = new ShaderResourceViewDescription()
			{
				Format = description.Format,
				Dimension = ShaderResourceViewDimension.Texture2D
			};

			srvDescription.Texture2D.MostDetailedMip = 0;
			srvDescription.Texture2D.MipLevels = description.MipLevels;

			m_textureView = new ShaderResourceView(device, m_texture, srvDescription);

			RenderTargetViewDescription renderTargetDesc = new RenderTargetViewDescription()
			{
				Dimension = RenderTargetViewDimension.Texture2D,
				Format = description.Format				
			};

			renderTargetDesc.Texture2D.MipSlice = 0;
			m_renderTargetView = new RenderTargetView(device, m_texture, renderTargetDesc);
		}

		public RenderTargetView GetRenderTarget()
		{
			return m_renderTargetView;
		}

		public override void Dispose()
		{
			base.Dispose();
			m_renderTargetView.Dispose();			
		}

		private RenderTargetView m_renderTargetView;
	}
}
