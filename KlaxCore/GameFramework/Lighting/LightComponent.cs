using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer;
using KlaxRenderer.Lights;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.GameFramework.Lighting
{
	[KlaxComponent(HideInEditor = true, Category = "Lighting")]
	public abstract class CLightComponent : CSceneComponent
	{
		protected void MarkRenderStateDirty()
		{
			m_bIsRenderStateDirty = true;
			if (m_preRenderUpdateScope == null || !m_preRenderUpdateScope.IsConnected())
			{
				m_preRenderUpdateScope = World.UpdateScheduler.ConnectOneTimeUpdate(UpdateRenderData, EUpdatePriority.ResourceLoading);
			}
		}

		public override void Init()
		{
			base.Init();
			CreateRenderLight();
			if (m_renderLight != null)
			{
				CRenderer.Instance.ActiveScene.LightManager.AddLight(m_renderLight);
			}
		}

		protected abstract void CreateRenderLight();

		protected abstract void UpdateRenderData(float deltaTime);

		public override void Shutdown()
		{
			base.Shutdown();
			if (m_renderLight != null)
			{
				CRenderer.Instance.ActiveScene.LightManager.RemoveLight(m_renderLight);
				m_renderLight = null;
			}
		}

		public ELightType LightType { get; protected set; }

		[JsonProperty]
		private Color4 m_lightColor;
       [KlaxProperty(Category = "Light")]
		[JsonIgnore]
        public Color4 LightColor
		{
			get { return m_lightColor; }
			set { m_lightColor = value; MarkRenderStateDirty(); }
		}

		[JsonProperty]
		private bool m_bEnabled = true;
		[KlaxProperty(Category = "Light")]
		[JsonIgnore]
        public bool Enabled
		{
			get { return m_bEnabled; }
			set { m_bEnabled = value; MarkRenderStateDirty(); }
		}

		protected ILight m_renderLight;
		protected bool m_bIsRenderStateDirty;
		private CUpdateScope m_preRenderUpdateScope;
	}
}
