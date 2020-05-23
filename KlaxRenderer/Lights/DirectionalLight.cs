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

		public bool IsCastingShadow() { return IsCastingShadows; }
		public bool NeedsShadowMapInit() { return !m_isShadowMapIntialized; }

		public bool IsShadowMapCube()
		{
			return false;
		}

		public Vector4 LightColor { get; set; }
		public Vector3 LightDirection { get; set; }
		public bool Enabled { get; set; }		
		public bool IsCastingShadows { get; set; } = true;
		public int ShadowMapRegister { get; set; }

		protected bool m_isShadowMapIntialized = false;
	}
}
