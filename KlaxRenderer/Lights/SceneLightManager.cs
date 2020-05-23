using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Graphics;
using KlaxRenderer.Scene;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace KlaxRenderer.Lights
{
	public class CSceneLightManager : IDisposable
	{
		private const int CubeShadowMapBaseSlot = 20;
		private const int MaxCubeShadowMaps = 6;
		private const int ShadowMapBaseSlot = 26;
		private const int MaxShadowMaps = 6;

		public void Init(Device device, DeviceContext deviceContext)
		{
			m_sharedLightBuffer.Init(device);
			m_perObjectLightBuffer.Init(device);
			m_shadowMapSamplers.InitSamplers(device);
		}

		public void BindBuffer(DeviceContext deviceContext)
		{
			m_sharedLightBuffer.BindBuffer(deviceContext, EShaderTargetStage.Vertex);
			m_sharedLightBuffer.BindBuffer(deviceContext, EShaderTargetStage.Pixel);
			m_perObjectLightBuffer.BindBuffer(deviceContext, EShaderTargetStage.Vertex);
			m_perObjectLightBuffer.BindBuffer(deviceContext, EShaderTargetStage.Pixel);
		}

		public void GenerateShadowMaps(Device device, DeviceContext deviceContext, CRenderScene renderScene)
		{
			// Store current render targets. Accourding to the function description we need to release these temporary interfaces 
			DepthStencilView depthStencilRestore;
			RenderTargetView[] renderTargetRestore = deviceContext.OutputMerger.GetRenderTargets(1, out depthStencilRestore);
			RawViewportF[] viewPortsRestore = deviceContext.Rasterizer.GetViewports<RawViewportF>();
			DepthStencilState depthStenctilStateRestore = deviceContext.OutputMerger.DepthStencilState;
			RasterizerState rasterizerRestore = deviceContext.Rasterizer.State;

			int shadowMapSlotOffset = 0;
			int cubeShadowMapSlotOffset = 0;

			// Unbind texture resources
			for (int i = CubeShadowMapBaseSlot; i < CubeShadowMapBaseSlot + MaxCubeShadowMaps; i++)
			{
				deviceContext.PixelShader.SetShaderResource(i, null);
			}

			for (int i = ShadowMapBaseSlot; i < ShadowMapBaseSlot + MaxShadowMaps; i++)
			{
				deviceContext.PixelShader.SetShaderResource(i, null);
			}

			// Generate shadow maps for all active lights
			foreach (CDirectionalLight directionalLight in m_directionalLights)
			{
				if (directionalLight.IsCastingShadow() && !directionalLight.NeedsShadowMapInit())
				{
					// Init Shadow Map
				}
			}
			
			foreach (CPositionalLight positionalLight in m_positionalLights)
			{
				if (positionalLight.IsCastingShadow() && !positionalLight.NeedsShadowMapInit())
				{
					if (positionalLight.IsShadowMapCube())
					{
						if (cubeShadowMapSlotOffset < MaxCubeShadowMaps)
						{
							positionalLight.GenerateShadowMaps(device, deviceContext, renderScene);
							positionalLight.ShadowMapRegister = CubeShadowMapBaseSlot + cubeShadowMapSlotOffset;
							deviceContext.PixelShader.SetShaderResource(CubeShadowMapBaseSlot + cubeShadowMapSlotOffset, positionalLight.GetShadowMapView());
							cubeShadowMapSlotOffset++;
						}
					}
					else
					{
						if (shadowMapSlotOffset < MaxShadowMaps)
						{
							positionalLight.GenerateShadowMaps(device, deviceContext, renderScene);
							positionalLight.ShadowMapRegister = ShadowMapBaseSlot + shadowMapSlotOffset;
							deviceContext.PixelShader.SetShaderResource(ShadowMapBaseSlot + shadowMapSlotOffset, positionalLight.GetShadowMapView());
							shadowMapSlotOffset++;
						}
					}
				}
			}
			m_shadowMapSamplers.SetSamplers(deviceContext);

			// Restore previous render targets
			deviceContext.OutputMerger.SetRenderTargets(depthStencilRestore, renderTargetRestore);
			deviceContext.Rasterizer.SetViewports(viewPortsRestore);
			deviceContext.Rasterizer.State = rasterizerRestore;
			deviceContext.OutputMerger.DepthStencilState = depthStenctilStateRestore;

			//Dispose temporary interfaces
			depthStencilRestore?.Dispose();
			depthStenctilStateRestore?.Dispose();
			rasterizerRestore?.Dispose();

			foreach (RenderTargetView renderTargetView in renderTargetRestore)
			{
				renderTargetView?.Dispose();
			}
		}

		public void InitShadowMapsIfNeeded(Device device)
		{
			foreach (CDirectionalLight directionalLight in m_directionalLights)
			{
				if (directionalLight.IsCastingShadow() && directionalLight.NeedsShadowMapInit())
				{
					// Init Shadow Map
				}
			}

			foreach (CPositionalLight positionalLight in m_positionalLights)
			{
				if (positionalLight.IsCastingShadow() && positionalLight.NeedsShadowMapInit())
				{
					positionalLight.InitializeShadowMaps(device);
				}
			}
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

			foreach (CPositionalLight positionalLight in m_positionalLights)
			{
				positionalLight.Dispose();				
			}
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
				positionalLight.Dispose();
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
					m_positionalLights[index].Dispose();
					m_positionalLights.RemoveAt(index);
				}
			}
		}
		
		private readonly List<CDirectionalLight> m_directionalLights = new List<CDirectionalLight>();
		private readonly List<CPositionalLight> m_positionalLights = new List<CPositionalLight>();
		private readonly List<CAmbientLight> m_ambientLights = new List<CAmbientLight>();
		private readonly SharedLightBuffer m_sharedLightBuffer = new SharedLightBuffer();
		private readonly PerObjectLightBuffer m_perObjectLightBuffer = new PerObjectLightBuffer();
		private readonly CShadowMapSamplers m_shadowMapSamplers = new CShadowMapSamplers();
	}
}
