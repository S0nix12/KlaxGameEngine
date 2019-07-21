using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using KlaxRenderer.Graphics.Shader;
using KlaxShared;
using KlaxShared.Utilities;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Graphics.ResourceManagement
{
	internal class CResourceManager : IDisposable
	{
		public CResourceManager(Device device)
		{
			m_graphicsDevice = device;
			m_shaderRegistry = new CShaderRegistry(device);
		}

		public void InitDefaultResources(DeviceContext deviceContext)
		{
			DefaultShader = RequestShaderResource(new SHashedName("SimpleLitShader"));

			DefaultTexture = new CTextureSampler(m_graphicsDevice, deviceContext, "Resources/Textures/DefaultTexture.tga");
			FallbackTexture = new CTextureSampler(m_graphicsDevice, deviceContext, "Resources/Textures/MissingTexture.tga");
			WhiteTexture = new CTextureSampler(m_graphicsDevice, deviceContext, "Resources/Textures/DefaultWhite.tga");
			BlackTexture = new CTextureSampler(m_graphicsDevice, deviceContext, "Resources/Textures/DefaultBlack.tga");

			DefaultMaterial = CMaterial.CreateDefaultMaterial();
			DefaultMaterial.ShaderResource = DefaultShader;
			DefaultMaterial.SetTextureParameter(new SHashedName("DiffuseTexture"), WhiteTexture);
			DefaultMaterial.FinishLoading();

			DefaultTextureMaterial = CMaterial.CreateDefaultMaterial();
			DefaultTextureMaterial.ShaderResource = DefaultShader;
			DefaultTextureMaterial.SetTextureParameter(new SHashedName("DiffuseTexture"), DefaultTexture);
			DefaultTextureMaterial.FinishLoading();

			MissingTextureMaterial = CMaterial.CreateDefaultMaterial();
			MissingTextureMaterial.ShaderResource = DefaultShader;
			MissingTextureMaterial.SetTextureParameter(new SHashedName("DiffuseTexture"), FallbackTexture);
			MissingTextureMaterial.FinishLoading();
		}

		public void Dispose()
		{
			DefaultShader.Dispose();
			DefaultTexture.Dispose();
			FallbackTexture.Dispose();
			WhiteTexture.Dispose();
			BlackTexture.Dispose();
			DefaultMaterial.Dispose();
			DefaultTextureMaterial.Dispose();
			MissingTextureMaterial.Dispose();
			m_shaderRegistry.Dispose();

			foreach (CResource waitingResource in m_waitingResources)
			{
				waitingResource.Dispose();
			}

			m_waitingResources.Clear();

			foreach (var registeredResource in m_registeredResources)
			{
				registeredResource.Value.Dispose();
			}

			m_registeredResources.Clear();
		}

		public void UpdateResources(DeviceContext deviceContext)
		{
			for (int i = m_waitingAssets.Count - 1; i >= 0; --i)
			{
				CAsset asset = m_waitingAssets[i];
				if (asset.IsLoaded)
				{
					CResource resource = m_registeredResources[asset.Guid];
					System.Diagnostics.Debug.Assert(resource != null);
					Task.Run(() => CreateResource(resource, asset));

					ContainerUtilities.RemoveSwapAt(m_waitingAssets, i);
					break;
				}
			}

			lock (m_resourceMutex)
			{
				for (int i = 0; i < m_waitingResources.Count; ++i)
				{
					m_waitingResources[i].InitWithContext(m_graphicsDevice, deviceContext);
				}
				m_waitingResources.Clear();
			}
		}

		/// <summary>
		/// Gets a resource of a given type created from the given asset. If the resource is not registered yet it will be created async returning a not loaded resource
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		public T RequestResourceFromAsset<T>(CAsset asset) where T : CResource, new()
		{
			System.Diagnostics.Debug.Assert(asset != null, "Tried to get a resource from a invalid asset");
			T newResource;
			lock (m_resourceMutex)
			{
				m_registeredResources.TryGetValue(asset.Guid, out CResource rawResource);
				T resource = rawResource as T;
				if (resource != null)
				{
					return resource;
				}

				newResource = new T();
				newResource.Guid = asset.Guid;
				System.Diagnostics.Debug.Assert(newResource.IsAssetCorrectType(asset));

				m_registeredResources.Add(asset.Guid, newResource);
			}

			if (asset.IsLoaded)
			{
				Task.Run(() => CreateResource(newResource, asset));
			}
			else
			{
				m_waitingAssets.Add(asset);
			}

			return newResource;
		}

		public CShaderResource RequestShaderResource(SHashedName shaderName)
		{
			CShaderResource resource = new CShaderResource();
			resource.Shader = RequestShader(shaderName);
			resource.FinishLoading();
			return resource;
		}

		public CShader RequestShader(SHashedName shaderName)
		{
			return m_shaderRegistry.RequestShader(shaderName);
		}

		private void CreateResource(CResource target, CAsset source)
		{
			System.Diagnostics.Debug.Assert(source.IsLoaded);
			target.InitFromAsset(m_graphicsDevice, source);

			if (target.NeedsContext())
			{
				lock (m_resourceMutex)
				{
					m_waitingResources.Add(target);
				}
			}
			else
			{
				target.FinishLoading();
			}
		}

		public CResource RequestResource(Guid resourceGuid)
		{
			return m_registeredResources[resourceGuid];
		}
		
		private readonly List<CAsset> m_waitingAssets = new List<CAsset>();

		private readonly List<CResource> m_waitingResources = new List<CResource>();
		private readonly object m_resourceMutex = new object();

		private readonly Dictionary<Guid, CResource> m_registeredResources = new Dictionary<Guid, CResource>();
		private readonly Device m_graphicsDevice;
		private readonly CShaderRegistry m_shaderRegistry;

		public CTextureSampler DefaultTexture { get; private set; }
		public CTextureSampler FallbackTexture { get; private set; }
		public CTextureSampler WhiteTexture { get; private set; }
		public CTextureSampler BlackTexture { get; private set; }
		public CMaterial DefaultMaterial { get; private set; }
		public CMaterial DefaultTextureMaterial { get; private set; }
		public CMaterial MissingTextureMaterial { get; private set; }
		public CShaderResource DefaultShader { get; private set; }
	}
}
