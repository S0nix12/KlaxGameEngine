using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics
{
    public class CTexture : IDisposable
    {
        public virtual void InitFromFile(Device device, DeviceContext deviceContext, string filePath)
        {
            m_texture = TextureLoader.CreateTex2DFromFile(device, deviceContext, filePath);

            ShaderResourceViewDescription srvDescription = new ShaderResourceViewDescription
            {
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D
            };

            srvDescription.Texture2D.MostDetailedMip = 0;
            srvDescription.Texture2D.MipLevels = -1;

            m_textureView = new ShaderResourceView(device, m_texture, srvDescription);
            deviceContext.GenerateMips(m_textureView);
        }

	    public virtual void InitFromData(Device device, DeviceContext deviceContext, IntPtr dataPtr, int pitch, in Texture2DDescription description, bool bGenerateMips)
	    {
			m_texture = new Texture2D(device, description);
			deviceContext.UpdateSubresource(new DataBox(dataPtr, pitch, 0), m_texture);

			ShaderResourceViewDescription srvDescription = new ShaderResourceViewDescription()
		    {
			    Format = description.Format,
			    Dimension = ShaderResourceViewDimension.Texture2D
		    };

		    srvDescription.Texture2D.MostDetailedMip = 0;
		    srvDescription.Texture2D.MipLevels = bGenerateMips ? -1 : description.MipLevels;

			m_textureView = new ShaderResourceView(device, m_texture, srvDescription);
		    if (bGenerateMips)
		    {
			    deviceContext.GenerateMips(m_textureView);
		    }
	    }

        public ShaderResourceView GetTexture()
        {
            return m_textureView;
        }

        public virtual void Dispose()
        {
            m_textureView?.Dispose();
            m_texture?.Dispose();
		}

        protected Texture2D m_texture;
        protected ShaderResourceView m_textureView;
    }
}
