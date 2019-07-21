using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Lights;
using KlaxShared.Attributes;

namespace KlaxCore.GameFramework.Lighting
{
	[KlaxComponent(Category = "Lighting")]
	class CAmbientLightComponent : CLightComponent
	{
		public CAmbientLightComponent()
		{
			LightType = ELightType.Ambient;
		}

		protected override void CreateRenderLight()
		{
			CAmbientLight ambLight = new CAmbientLight();
			ambLight.LightColor = LightColor;
			m_renderLight = ambLight;
		}

		protected override void UpdateRenderData(float deltaTime)
		{
			CAmbientLight ambLight = (CAmbientLight) m_renderLight;
			ambLight.LightColor = LightColor;
		}
	}
}
