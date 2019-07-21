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

		public Vector4 LightColor { get; set; }
	}
}
