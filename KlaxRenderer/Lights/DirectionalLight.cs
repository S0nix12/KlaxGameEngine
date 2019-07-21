using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxRenderer.Lights
{
	public class CDirectionalLight : ILight
	{
		public ELightType GetLightType()
		{
			return ELightType.Directional;
		}

		public Vector4 LightColor { get; set; }
		public Vector3 LightDirection { get; set; }
		public bool Enabled { get; set; }		
	}
}
