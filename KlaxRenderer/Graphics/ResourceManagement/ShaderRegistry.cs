using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KlaxRenderer.Graphics.Shader;
using KlaxShared;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics.ResourceManagement
{
	class CShaderRegistry : IDisposable
	{
		internal CShaderRegistry(Device device)
		{
			// Register Shader classes
			m_shaderClassMap.Add(new SHashedName("colorShader"), typeof(CColorShader));
			m_shaderClassMap.Add(new SHashedName("simpleLitShader"), typeof(CSimpleLitShader));
			m_shaderClassMap.Add(new SHashedName("textureShader"), typeof(CTextureShader));
			m_shaderClassMap.Add(new SHashedName("uiShader"), typeof(CUIShader));
			m_shaderClassMap.Add(new SHashedName("depthShader"), typeof(CDepthShader));
			m_shaderClassMap.Add(new SHashedName("depthCubeShader"), typeof(CDepthCubeShader));

			// Preload basic shaders
			CColorShader colorShader = new CColorShader();
			CSimpleLitShader simpleLitShader = new CSimpleLitShader();
			CTextureShader textureShader = new CTextureShader();
			CUIShader uiShader = new CUIShader();
			CDepthShader depthShader = new CDepthShader();			
			CDepthCubeShader depthCubeShader = new CDepthCubeShader();

			Parallel.Invoke(() => colorShader.Init(device), () => simpleLitShader.Init(device), () => textureShader.Init(device), () => uiShader.Init(device), () => depthShader.Init(device), () => depthCubeShader.Init(device));

			m_shaders.Add(new SHashedName("colorShader"), colorShader);
			m_shaders.Add(new SHashedName("simpleLitShader"), simpleLitShader);
			m_shaders.Add(new SHashedName("textureShader"), textureShader);
			m_shaders.Add(new SHashedName("uiShader"), uiShader);
			m_shaders.Add(new SHashedName("depthShader"), depthShader);
			m_shaders.Add(new SHashedName("depthCubeShader"), depthCubeShader);

			m_device = device;
		}

		public CShader RequestShader(SHashedName shaderName)
		{
			m_shaders.TryGetValue(shaderName, out CShader outShader);
			if (outShader != null)
			{
				return outShader;
			}

			m_shaderClassMap.TryGetValue(shaderName, out Type shaderClass);
			if (shaderClass != null)
			{
				CShader shader = (CShader) Activator.CreateInstance(shaderClass);
				m_shaders.Add(shaderName, shader);
				Task.Run(() => shader.Init(m_device));
			}

			throw new ArgumentException("Shader with the name " + shaderName + " was not found");
		}

		public void Dispose()
		{
			foreach (var shader in m_shaders)
			{
				shader.Value.Dispose();
			}
		}

		private readonly Device m_device;
		private readonly Dictionary<SHashedName, Type> m_shaderClassMap = new Dictionary<SHashedName, Type>();
		private readonly Dictionary<SHashedName, CShader> m_shaders = new Dictionary<SHashedName, CShader>();
	}
}
