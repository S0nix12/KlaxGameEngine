using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxRenderer.Camera;
using KlaxShared;
using SharpDX;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics
{
    class CTexturedMesh : CMesh
    {
        [StructLayout(LayoutKind.Sequential)]
        protected new struct VertexType
        {
            public Vector3 position;
            public Vector2 texCoord;
        };

		public CTexturedMesh()
		{
			m_sizePerVertex = Utilities.SizeOf<VertexType>();
			Material.ShaderResource = CRenderer.Instance.ResourceManager.RequestShaderResource(new SHashedName("textureShader"));
		}

		public override void Init(Device device)
		{
			// Load resource
			// Create Buffers
			InitializeBuffers(device);
		}

		public void InitTexture(Device device, DeviceContext deviceContext, string fileName)
		{
			CTextureSampler textureSampler = new CTextureSampler(device, deviceContext, fileName);
			Material.SetTextureParameter(new SHashedName("DiffuseTexture"), textureSampler);
			Material.SetColorParameter(new SHashedName("tintColor"), new Vector4(1, 1, 1, 1));
		}
	}
}
