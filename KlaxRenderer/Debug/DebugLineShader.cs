using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace KlaxRenderer.Debug
{
	class CDebugLineShader : CDebugObjectShader
	{
		public CDebugLineShader()
		{
			VSFileName = "Resources/Shaders/DebugLineVS.hlsl";
			PSFileName = "Resources/Shaders/DebugLinePS.hlsl";

			InputElements = new []
			{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
				new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0)
			};
		}
	}
}
