using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxRenderer.Graphics
{
    interface IShader
    {
        bool Init(SharpDX.Direct3D11.Device device);
        void Shutdown();
        void Render(SharpDX.Direct3D11.DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix);
    }
}
