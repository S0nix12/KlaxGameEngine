using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Lights
{
	public interface ILight
	{
		ELightType GetLightType();
		bool IsCastingShadow();
		bool IsShadowMapCube();
		bool NeedsShadowMapInit();

		int ShadowMapRegister { get; set; }
	}
}
