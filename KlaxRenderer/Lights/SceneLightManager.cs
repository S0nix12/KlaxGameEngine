using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Graphics;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Lights
{
	public class CSceneLightManager : IDisposable
	{
		public void Init(Device device, DeviceContext deviceContext)
		{
			m_sharedLightBuffer.Init(device);
			m_perObjectLightBuffer.Init(device);
		}

		public void BindBuffer(DeviceContext deviceContext)
		{
			m_sharedLightBuffer.BindBuffer(deviceContext, EShaderTargetStage.Pixel);
			m_perObjectLightBuffer.BindBuffer(deviceContext, EShaderTargetStage.Pixel);
		}

		public void Update(DeviceContext deviceContext)
		{
			Vector4 ambientColor = Vector4.Zero;
			for (int i = 0; i < m_ambientLights.Count; i++)
			{
				ambientColor += m_ambientLights[i].LightColor;
			}

			ambientColor = Vector4.Clamp(ambientColor, Vector4.Zero, Vector4.One);
			m_sharedLightBuffer.UpdateBuffer(deviceContext, in ambientColor, m_directionalLights.ElementAtOrDefault(0));
		}

		public void UpdatePerObjectLights(DeviceContext deviceContext, CMesh meshToRender)
		{
			// TODO henning do something smart here to get the best lights that affect this mesh
			m_perObjectLightBuffer.UpdateBuffer(deviceContext, m_positionalLights);
		}

		public void Dispose()
		{
			m_sharedLightBuffer.Dispose();
			m_perObjectLightBuffer.Dispose();
		}

		public int AddLight(ILight newLight)
		{
			if (newLight.GetLightType() == ELightType.Directional)
			{
				CDirectionalLight newDirectionalLight = (CDirectionalLight) newLight;
				m_directionalLights.Add(newDirectionalLight);
				return m_directionalLights.Count - 1;
			}
			else if (newLight.GetLightType() == ELightType.Ambient)
			{
				CAmbientLight ambLight = (CAmbientLight) newLight;
				m_ambientLights.Add(ambLight);
				return m_ambientLights.Count - 1;
			}
			else
			{
				CPositionalLight newPositionalLight = (CPositionalLight) newLight;
				m_positionalLights.Add(newPositionalLight);
				return m_positionalLights.Count - 1;
			}
		}

		public void RemoveLight(ILight lightToRemove)
		{
			if (lightToRemove.GetLightType() == ELightType.Directional)
			{
				CDirectionalLight directionalLight = (CDirectionalLight)lightToRemove;
				m_directionalLights.Remove(directionalLight);
			}
			else if(lightToRemove.GetLightType() == ELightType.Ambient)
			{
				CAmbientLight ambLight = (CAmbientLight) lightToRemove;
				m_ambientLights.Remove(ambLight);
			}
			else
			{
				CPositionalLight positionalLight = (CPositionalLight)lightToRemove;
				m_positionalLights.Remove(positionalLight);
			}
		}

		public void RemoveLight(int index, ELightType lightType)
		{
			if (lightType == ELightType.Directional)
			{
				if (index >= 0 && index < m_directionalLights.Count)
				{
					m_directionalLights.RemoveAt(index);
				}
			}
			else if(lightType == ELightType.Ambient)
			{
				if (index >= 0 && index < m_ambientLights.Count)
				{
					m_ambientLights.RemoveAt(index);
				}
			}
			else
			{
				if (index >= 0 && index < m_positionalLights.Count)
				{
					m_positionalLights.RemoveAt(index);
				}
			}
		}
		
		private readonly List<CDirectionalLight> m_directionalLights = new List<CDirectionalLight>();
		private readonly List<CPositionalLight> m_positionalLights = new List<CPositionalLight>();
		private readonly List<CAmbientLight> m_ambientLights = new List<CAmbientLight>();
		private readonly SharedLightBuffer m_sharedLightBuffer = new SharedLightBuffer();
		private readonly PerObjectLightBuffer m_perObjectLightBuffer = new PerObjectLightBuffer();
	}
}
