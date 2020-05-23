using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxRenderer.Lights
{
	public class CAmbientLight : ILight
	{
		public ELightType GetLightType()
		{
			return ELightType.Ambient;
		}

		public bool IsCastingShadow()
		{
			return false;
		}

		public bool NeedsShadowMapInit()
		{
			return false;
		}

		public bool IsShadowMapCube()
		{
			return false;
		}

		public Vector4 LightColor { get; set; }
		public int ShadowMapRegister { get; set; } = 0;
	}
}
