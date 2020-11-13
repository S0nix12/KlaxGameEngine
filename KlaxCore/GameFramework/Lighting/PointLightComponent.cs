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
	class CPointLightComponent : CLightComponent
	{
		public CPointLightComponent()
		{
			LightType = ELightType.Point;
		}

		protected override void CreateRenderLight()
		{
			CPointLight pointLight = new CPointLight();
			pointLight.LinearAttenuation = m_linearAttenuation;
			pointLight.ConstantAttenuation = m_constantAttenuation;
			pointLight.QuadraticAttenuation = m_quadraticAttenuation;
			pointLight.Range = m_range;
			pointLight.LightColor = LightColor;
			pointLight.Enabled = Enabled;
			pointLight.IsCastingShadows = CastShadow;
			pointLight.Transform.Parent = m_transform;
			m_renderLight = pointLight;
		}

		protected override void UpdateRenderData(float deltaTime)
		{
			CPointLight pointLight = (CPointLight) m_renderLight;
			pointLight.LinearAttenuation = m_linearAttenuation;
			pointLight.ConstantAttenuation = m_constantAttenuation;
			pointLight.QuadraticAttenuation = m_quadraticAttenuation;
			pointLight.Range = m_range;
			pointLight.LightColor = LightColor;
			pointLight.Enabled = Enabled;
			pointLight.IsCastingShadows = CastShadow;
		}

		[JsonProperty]
		private float m_range;
		[KlaxProperty(Category = "Light")]
		[JsonIgnore]
		public float Range
		{
			get { return m_range;}
			set { m_range = value; MarkRenderStateDirty(); }
		}

		[JsonProperty]
		private float m_constantAttenuation;
		[KlaxProperty(Category = "Light")]
		[JsonIgnore]
        public float ConstantAttenuation
		{
			get { return m_constantAttenuation;}
			set { m_constantAttenuation = value; MarkRenderStateDirty(); }
		}

		[JsonProperty]
		private float m_linearAttenuation;
		[KlaxProperty(Category = "Light")]
		[JsonIgnore]
        public float LinearAttenuation
		{
			get { return m_linearAttenuation;}
			set { m_linearAttenuation = value; MarkRenderStateDirty(); }
		}

		[JsonProperty]
		public float m_quadraticAttenuation;
		[KlaxProperty(Category = "Light")]
		[JsonIgnore]
        public float QuadraticAttenuation
		{
			get { return m_quadraticAttenuation; }
			set { m_quadraticAttenuation = value; MarkRenderStateDirty(); }
		}

		[JsonProperty]
		private bool m_bCastShadow = true;
		[KlaxProperty(Category = "Light")]
		[JsonIgnore]
		public bool CastShadow
		{
			get { return m_bCastShadow; }
			set { m_bCastShadow = value; MarkRenderStateDirty(); }
		}
	}
}
