using KlaxRenderer.Lights;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.Lighting
{
	[KlaxComponent(Category = "Lighting")]
	class CDirectionalLightComponent : CLightComponent
	{
		public CDirectionalLightComponent()
		{
			LightType = ELightType.Directional;
		}

		public override void Init()
		{
			base.Init();
			m_transform.OnRotationChanged += OnRotationChanged;
		}

		protected override void CreateRenderLight()
		{
			CDirectionalLight dirLight = new CDirectionalLight();
			dirLight.Enabled = Enabled;
			dirLight.LightColor = LightColor;
			dirLight.LightDirection = m_transform.Forward;
			m_renderLight = dirLight;
		}

		protected override void UpdateRenderData(float deltaTime)
		{
			CDirectionalLight dirLight = (CDirectionalLight) m_renderLight;
			dirLight.Enabled = Enabled;
			dirLight.LightColor = LightColor;
			dirLight.LightDirection = m_transform.Forward;
		}

		public void OnRotationChanged(Quaternion newRotation)
		{
			MarkRenderStateDirty();
		}
	}
}
