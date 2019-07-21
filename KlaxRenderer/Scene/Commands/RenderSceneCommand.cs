using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Scene.Commands
{
	public interface IRenderSceneCommand
	{
		bool TryExecute(Device device, DeviceContext deviceContext, CRenderScene scene);
	}
}
