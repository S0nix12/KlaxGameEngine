using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics
{
    interface IDrawable
    {
        void Init(SharpDX.Direct3D11.Device device);
        void Draw(SharpDX.Direct3D11.DeviceContext deviceContext, Camera.ICamera camera);
    }
}
