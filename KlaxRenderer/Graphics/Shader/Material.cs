using System;
using System.Collections.Generic;
using KlaxIO.AssetManager.Assets;
using KlaxRenderer.Graphics.ResourceManagement;
using KlaxRenderer.Graphics.Shader;
using KlaxShared;
using KlaxShared.Attributes;
using SharpDX;
using SharpDX.Direct3D11;
using KlaxShared.Definitions.Graphics;

namespace KlaxRenderer.Graphics
{
	[KlaxScriptType]
    public class CMaterial : CResource
    {
	    public static CMaterial CreateDefaultMaterial()
		{
			CMaterial outMaterial = new CMaterial();
			outMaterial.ShaderResource = CRenderer.Instance.ResourceManager.DefaultShader;
			outMaterial.SetColorParameter(new SHashedName("tintColor"), Vector4.One);
			outMaterial.SetColorParameter(new SHashedName("specularColor"), Vector4.One);
			outMaterial.SetScalarParameter(new SHashedName("specularPower"), 10);

			return outMaterial;
		}
		
		internal override void InitFromAsset(Device device, CAsset asset)
		{
			CMaterialAsset materialAsset = (CMaterialAsset) asset;
			System.Diagnostics.Debug.Assert(materialAsset != null && materialAsset.IsLoaded);
			
			if (materialAsset.Shader != null)
			{
				ShaderResource = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CShaderResource>(materialAsset.Shader);
			}
			else
			{
				ShaderResource = CRenderer.Instance.ResourceManager.DefaultShader;
			}

			SetColorParameter(new SHashedName("tintColor"), Vector4.One);
			SetColorParameter(new SHashedName("specularColor"), Vector4.One);
			SetScalarParameter(new SHashedName("specularPower"), 10);
			for (int i = 0; i < materialAsset.MaterialParameters.Count; i++)
			{
				var parameterDesc = materialAsset.MaterialParameters[i];
				if (parameterDesc.parameter.parameterData == null)
				{
					continue;
				}
				if (parameterDesc.parameter.parameterType == EShaderParameterType.Texture)
				{
					CAssetReference<CTextureAsset> textureReference = (CAssetReference<CTextureAsset>) parameterDesc.parameter.parameterData;
					if (textureReference.GetAsset() != null)
					{
						CTextureSampler sampler = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CTextureSampler>(textureReference.GetAsset());
						SShaderParameter activeParameter = new SShaderParameter()
						{
							parameterData = sampler,
							parameterType = EShaderParameterType.Texture
						};
						m_activeParameters[parameterDesc.name] = activeParameter;
					}
				}
				else
				{
					m_activeParameters[parameterDesc.name] = parameterDesc.parameter;
				}
			}
		}

		internal override void InitWithContext(Device device, DeviceContext deviceContext)
		{}

		internal override bool IsAssetCorrectType(CAsset asset)
		{
			return asset is CMaterialAsset;
		}

		public void Render(DeviceContext deviceContext, in Matrix worldMatrix)
        {
			SetMatrixParameter(SShaderParameterNames.WorldMatrixParameterName, in worldMatrix);
	        Matrix invTranspose = Matrix.Transpose(Matrix.Invert(worldMatrix));
			SetMatrixParameter(SShaderParameterNames.InvTransWorldMatrixParName, in invTranspose);
			ShaderResource.Shader.SetShaderParameters(deviceContext, m_activeParameters);
	        ShaderResource.Shader.SetActive(deviceContext);
        }

        public override void Dispose()
        {
			//TODO henning when we have a resource manager we should notify it that the shader is no longer in use so it can be cleaned up when no longer used
			ShaderResource.Dispose();

			foreach(var shaderParam in m_activeParameters)
			{
				IDisposable disposable = shaderParam.Value.parameterData as IDisposable;
				disposable?.Dispose();
			}
        }

		public int GetNumActiveParameters()
		{
			return m_activeParameters.Count;
		}

        /// <summary>
        /// Adds all active parameters from the given material to this material
        /// </summary>
        /// <param name="other"></param>
        public void MergeWith(CMaterial other)
        {
            foreach (var parameter in other.m_activeParameters)
            {
                m_activeParameters[parameter.Key] = parameter.Value;
            }
        }

		// TODO henning to multithread the renderer we would have to queue this
		[KlaxFunction(Category = "Rendering", Tooltip = "Set the given scalar parameter on this material, normally this should be done after the material was made unique")]
		public void SetScalarParameter(SHashedName parameterName, float scalar)
		{
			SShaderParameter parameter = new SShaderParameter
			{
				parameterType = EShaderParameterType.Scalar,
				parameterData = scalar
			};

			AddActiveParameter(parameterName, parameter);
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Set the given vector parameter on this material, normally this should be done after the material was made unique")]
		public void SetVectorParameter(SHashedName parameterName, in Vector3 vector)
		{
			SShaderParameter parameter = new SShaderParameter
			{
				parameterType = EShaderParameterType.Vector,
				parameterData = vector
			};

			AddActiveParameter(parameterName, parameter);
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Set the given color parameter on this material, normally this should be done after the material was made unique")]
		public void SetColorParameter(SHashedName parameterName, in Vector4 color)
		{
			SShaderParameter parameter = new SShaderParameter
			{
				parameterType = EShaderParameterType.Color,
				parameterData = color
			};

			AddActiveParameter(parameterName, parameter);
		}

		public void SetMatrixParameter(SHashedName parameterName, in Matrix matrix)
		{
			SShaderParameter parameter = new SShaderParameter
			{
				parameterType = EShaderParameterType.Matrix,
				parameterData = matrix
			};

			AddActiveParameter(parameterName, parameter);
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Set the given texture parameter on this material, normally this should be done after the material was made unique")]
		public void SetTextureParameter(SHashedName parameterName, CTextureSampler texture)
		{
			SShaderParameter parameter = new SShaderParameter
			{
				parameterType = EShaderParameterType.Texture,
				parameterData = texture
			};

			AddActiveParameter(parameterName, parameter);
		}

        public void SetParameters(SMaterialParameterEntry[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
			{
				SMaterialParameterEntry parameter = parameters[i];
				if (parameter.parameter.parameterType == EShaderParameterType.Texture)
				{
					CTextureAsset asset = (CTextureAsset) parameter.parameter.parameterData;
					CTextureSampler sampler = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CTextureSampler>(asset);
					m_activeParameters[parameter.name] = new SShaderParameter(EShaderParameterType.Texture, sampler);
					continue;
				}
                m_activeParameters[parameter.name] = parameter.parameter;
            }
        }

		private void AddActiveParameter(SHashedName parameterName, in SShaderParameter parameter)
		{
			m_activeParameters[parameterName] = parameter;
		}

		internal CShaderResource ShaderResource { get; set; }
		internal Dictionary<SHashedName, SShaderParameter> m_activeParameters = new Dictionary<SHashedName, SShaderParameter>();

		internal static readonly SHashedName WorldMatrixParameterName = new SHashedName("worldMatrix");
		internal static readonly SHashedName InvTransWorldMatrixParName = new SHashedName("invTransposeWorldMatrix");
	}
}
