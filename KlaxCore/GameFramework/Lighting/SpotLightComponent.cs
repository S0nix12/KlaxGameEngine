using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Lights;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.GameFramework.Lighting
{
	[KlaxComponent(Category = "Lighting")]
	class CSpotLightComponent : CPointLightComponent
	{
		public CSpotLightComponent()
		{
			LightType = ELightType.Spot;
		}

		protected override void CreateRenderLight()
		{
			CSpotLight spotLight = new CSpotLight();
			spotLight.LinearAttenuation = LinearAttenuation;
			spotLight.ConstantAttenuation = ConstantAttenuation;
			spotLight.QuadraticAttenuation = QuadraticAttenuation;
			spotLight.Range = Range;
			spotLight.SpotAngle = SpotAngle;
			spotLight.LightColor = LightColor;
			spotLight.Enabled = Enabled;
			spotLight.Transform.Parent = m_transform;
			spotLight.IsCastingShadows = CastShadow;
			m_renderLight = spotLight;
		}

		protected override void UpdateRenderData(float deltaTime)
		{
			CSpotLight spotLight = (CSpotLight) m_renderLight;
			spotLight.LinearAttenuation = LinearAttenuation;
			spotLight.ConstantAttenuation = ConstantAttenuation;
			spotLight.QuadraticAttenuation = QuadraticAttenuation;
			spotLight.Range = Range;
			spotLight.SpotAngle = SpotAngle;
			spotLight.LightColor = LightColor;
			spotLight.Enabled = Enabled;
			spotLight.IsCastingShadows = CastShadow;
		}

		[JsonProperty]
		private float m_spotAngle;
		[JsonIgnore]
		[KlaxProperty(Category = "Light")]
		public float SpotAngle
		{
			get { return m_spotAngle; }
			set { m_spotAngle = value; MarkRenderStateDirty(); }
		}
	}
}
