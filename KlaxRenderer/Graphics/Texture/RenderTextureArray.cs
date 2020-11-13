using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics.Texture
{
	class CRenderTextureArray : CTexture
	{
		public void InitEmpty(Device device, in Texture2DDescription description)
		{
			m_texture = new Texture2D(device, description);

			ShaderResourceViewDescription srvDescription = new ShaderResourceViewDescription()
			{
				Format = description.Format,
				Dimension = ShaderResourceViewDimension.Texture2DArray
			};

			srvDescription.Texture2DArray.MipLevels = description.MipLevels;
			srvDescription.Texture2DArray.MostDetailedMip = 0;
			srvDescription.Texture2DArray.FirstArraySlice = 0;
			srvDescription.Texture2DArray.ArraySize = description.ArraySize;

			m_textureView = new ShaderResourceView(device, m_texture, srvDescription);

			m_renderTargetView = new RenderTargetView[description.ArraySize];
			for (int i = 0; i < description.ArraySize; i++)
			{
				RenderTargetViewDescription renderTargetDesc = new RenderTargetViewDescription()
				{
					Dimension = RenderTargetViewDimension.Texture2DArray,
					Format = description.Format
				};

				renderTargetDesc.Texture2D.MipSlice = 0;
				renderTargetDesc.Texture2DArray.ArraySize = 1;
				renderTargetDesc.Texture2DArray.FirstArraySlice = i;
				m_renderTargetView[i] = new RenderTargetView(device, m_texture, renderTargetDesc);
			}
		}

		public RenderTargetView GetRenderTarget(int index)
		{
			if (index >= 0 && index < m_renderTargetView.Length)
			{
				return m_renderTargetView[index];
			}

			return null;
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (RenderTargetView renderTargetView in m_renderTargetView)
			{
				renderTargetView.Dispose();
			}
		}

		private RenderTargetView[] m_renderTargetView;
	}
}
