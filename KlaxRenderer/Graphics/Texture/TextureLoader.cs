using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using TeximpNet;
using Surface = TeximpNet.Surface;

namespace KlaxRenderer.Graphics
{
    public static class TextureLoader
    {
        // TODO henning make this more generic to setup own description
		//[HandleProcessCorruptedStateExceptions]
        public static Texture2D CreateTex2DFromFile(Device device, DeviceContext deviceContext, string filename)
        {
			try
			{
				using (Surface image = Surface.LoadFromFile(filename, true))
				{
					image.ConvertTo(ImageConversion.To32Bits);
					Texture2DDescription desc;
					desc.Width = image.Width;
					desc.Height = image.Height;
					desc.ArraySize = 1;
					desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
					desc.Usage = ResourceUsage.Default;
					desc.CpuAccessFlags = CpuAccessFlags.None;
					desc.Format = Format.B8G8R8A8_UNorm;
					desc.MipLevels = 0;
					desc.OptionFlags = ResourceOptionFlags.GenerateMipMaps;
					desc.SampleDescription.Count = 1;
					desc.SampleDescription.Quality = 0;
					
					var t2D = new Texture2D(device, desc);
					deviceContext.UpdateSubresource(new DataBox(image.DataPtr, image.Pitch, 0), t2D);
					return t2D;
				}
			}
			catch
			{
				using (Surface image = Surface.LoadFromFile("Resources/Textures/MissingTexture.tga", true))
				{
					Texture2DDescription desc;
					desc.Width = image.Width;
					desc.Height = image.Height;
					desc.ArraySize = 1;
					desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
					desc.Usage = ResourceUsage.Default;
					desc.CpuAccessFlags = CpuAccessFlags.None;
					desc.Format = Format.B8G8R8A8_UNorm;
					desc.MipLevels = 0;
					desc.OptionFlags = ResourceOptionFlags.GenerateMipMaps;
					desc.SampleDescription.Count = 1;
					desc.SampleDescription.Quality = 0;

					// Create our texture without data and only update the data later with the first mip level so we can auto generate them
					var t2D = new Texture2D(device, desc);
					deviceContext.UpdateSubresource(new DataBox(image.DataPtr, image.Pitch, 0), t2D);
					return t2D;
				}
			}
		}
    }
}
