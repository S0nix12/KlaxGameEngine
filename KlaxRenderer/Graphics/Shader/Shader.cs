using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using KlaxShared;
using SharpDX.D3DCompiler;
using KlaxShared.Definitions.Graphics;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace KlaxRenderer.Graphics
{
	abstract class CShader : IShader
	{
		public bool Init(Device device)
		{
			lock (m_shaderMutex)
			{
				Init(device, VSFileName, PSFileName, GSFileName, VSEntryPoint, PSEntryPoint, GSEntryPoint);
				return true;
			}
		}

		public bool Init(Device device, string vertexShaderFilename, string pixelShaderFilename, string geomShaderFilename, string vertexShaderEntry, string pixelShaderEntry, string geomShaderEntry)
		{
			if (m_bIsInitialized)
			{
				return true;
			}
			m_bIsInitialized = true;

			// TODO henning replace with proper shader loading and offline compile
			if (!string.IsNullOrEmpty(vertexShaderFilename))
			{
				using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(vertexShaderFilename, vertexShaderEntry, "vs_5_0", ShaderFlags.Debug))
				{
					if (vertexShaderByteCode.HasErrors)
					{
						throw new Exception(vertexShaderByteCode.Message);
					}
					m_vertexShader = new VertexShader(device, vertexShaderByteCode);
					m_vertexShaderSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
				}
			}

			if (!string.IsNullOrEmpty(pixelShaderFilename))
			{
				using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(pixelShaderFilename, pixelShaderEntry, "ps_5_0", ShaderFlags.Debug))
				{
					if (pixelShaderByteCode.HasErrors)
					{
						throw new Exception(pixelShaderByteCode.Message);
					}
					m_pixelShader = new PixelShader(device, pixelShaderByteCode);
				}
			}

			if (!string.IsNullOrEmpty(geomShaderFilename))
			{
				using (var geomShaderByteCode = ShaderBytecode.CompileFromFile(geomShaderFilename, geomShaderEntry, "gs_5_0", ShaderFlags.Debug))
				{
					if (geomShaderByteCode.HasErrors)
					{
						throw new Exception(geomShaderByteCode.Message);
					}
					m_geometryShader = new GeometryShader(device, geomShaderByteCode);
				}
			}

			// TODO henning read from ShaderReflection
			InitShaderBuffers(device);
			InitShaderParameterTargets(device);
			InitInputLayout(device, m_vertexShaderSignature);
			return true;
		}

		public bool Init(Device device, byte[] vsCode, byte[] psCode)
		{
			if (m_bIsInitialized)
			{
				return true;
			}
			m_bIsInitialized = true;

			m_vertexShader = new VertexShader(device, vsCode);
			m_vertexShaderSignature = ShaderSignature.GetInputSignature(vsCode);

			m_pixelShader = new PixelShader(device, psCode);

			// TODO henning read from ShaderReflection
			InitShaderBuffers(device);
			InitShaderParameterTargets(device);
			InitInputLayout(device, m_vertexShaderSignature);
			return true;
		}
		public void SetActive(DeviceContext deviceContext)
		{
			if (!m_shaderParamsSet)
			{
				// Log critical warning shader rendered without parameters set
			}

			deviceContext.VertexShader.Set(m_vertexShader);
			deviceContext.PixelShader.Set(m_pixelShader);
			deviceContext.GeometryShader.Set(m_geometryShader);			
			deviceContext.InputAssembler.InputLayout = m_inputLayout;

			m_shaderParamsSet = false;
		}

		public virtual void Dispose()
		{
			m_vertexShader?.Dispose();
			m_pixelShader?.Dispose();
			m_geometryShader?.Dispose();
			m_inputLayout.Dispose();
		}

		protected abstract void InitShaderBuffers(Device device);
		protected abstract void InitShaderParameterTargets(Device device);

		protected void InitInputLayout(Device device, ShaderSignature signature)
		{
			m_inputLayout = new InputLayout(device, signature, InputElements);
		}

		/// <summary>
		/// Add a new shader parameter this shader uses, all shader parameters must be added in order they are set in their target buffer
		/// </summary>
		/// <param name="bufferDeclaration"></param>
		protected void AddShaderBufferDeclaration(in CShaderBufferDeclaration bufferDeclaration)
		{
			m_bufferDeclarations.Add(bufferDeclaration);
		}

		protected void AddShaderTextureTarget(in SShaderTextureTarget textureTarget)
		{
			m_textureTargets.Add(textureTarget);
		}

		public void GetShaderParameterTargets(List<SShaderParameterTarget> outParameterTargets, List<SShaderTextureTarget> outTextureTargets)
		{
			foreach (var bufferDeclaration in m_bufferDeclarations)
			{
				outParameterTargets.AddRange(bufferDeclaration.parameterTargets);
			}

			foreach (var textureTarget in m_textureTargets)
			{
				outTextureTargets.Add(textureTarget);
			}
		}

		/// <summary>
		/// Update shader constant buffers and set texture resources, should be used before the shader is drawn.
		/// Should receive the parameter list from the material
		/// </summary>
		/// <param name="deviceContext"></param>
		/// <param name="parameters"></param>
		public void SetShaderParameters(DeviceContext deviceContext, Dictionary<SHashedName, SShaderParameter> parameters)
		{
			foreach (CShaderBufferDeclaration bufferDeclaration in m_bufferDeclarations)
			{
				DataStream currentStream;
				deviceContext.MapSubresource(bufferDeclaration.targetBuffer, MapMode.WriteDiscard, MapFlags.None, out currentStream);
				foreach (var parameterTarget in bufferDeclaration.parameterTargets)
				{
					if (parameterTarget.parameterType == EShaderParameterType.Texture) continue;

					if (parameters.ContainsKey(parameterTarget.parameterName))
					{
						WriteNonTextureParameterToBuffer(parameters[parameterTarget.parameterName], parameterTarget.offset, currentStream);
					}
					else
					{
						WriteDefaultNonTextureParameterToBuffer(parameterTarget, currentStream);
					}
				}
				CloseAndSetCurrentBuffer(deviceContext, bufferDeclaration.targetBuffer, bufferDeclaration.targetSlot, bufferDeclaration.targetStage);
			}

			foreach (SShaderTextureTarget textureTarget in m_textureTargets)
			{
				if (parameters.TryGetValue(textureTarget.parameterName, out SShaderParameter textureParameter) && textureParameter.parameterData != null)
				{
					SetTextureResource(deviceContext, textureTarget, textureParameter);
				}
				else
				{
					SetDefaultTextureResource(deviceContext, textureTarget);
				}
			}

			m_shaderParamsSet = true;
		}

		private static void SetTextureResource(DeviceContext deviceContext, SShaderTextureTarget textureTarget, SShaderParameter shaderParam)
		{
			CTextureSampler textureSampler = (CTextureSampler)shaderParam.parameterData;
			if (textureSampler.IsLoaded)
			{
				switch (textureTarget.targetStage)
				{
					case EShaderTargetStage.Vertex:
						deviceContext.VertexShader.SetShaderResource(textureTarget.targetSlot, textureSampler.Texture.GetTexture());
						deviceContext.VertexShader.SetSampler(textureTarget.targetSlot, textureSampler.SamplerState);
						break;
					case EShaderTargetStage.Pixel:
						deviceContext.PixelShader.SetShaderResource(textureTarget.targetSlot, textureSampler.Texture.GetTexture());
						deviceContext.PixelShader.SetSampler(textureTarget.targetSlot, textureSampler.SamplerState);
						break;
					case EShaderTargetStage.Geometry:
						deviceContext.GeometryShader.SetShaderResource(textureTarget.targetSlot, textureSampler.Texture.GetTexture());
						deviceContext.GeometryShader.SetSampler(textureTarget.targetSlot, textureSampler.SamplerState);
						break;
					case EShaderTargetStage.Compute:
						deviceContext.ComputeShader.SetShaderResource(textureTarget.targetSlot, textureSampler.Texture.GetTexture());
						deviceContext.ComputeShader.SetSampler(textureTarget.targetSlot, textureSampler.SamplerState);
						break;
					case EShaderTargetStage.Domain:
						deviceContext.DomainShader.SetShaderResource(textureTarget.targetSlot, textureSampler.Texture.GetTexture());
						deviceContext.DomainShader.SetSampler(textureTarget.targetSlot, textureSampler.SamplerState);
						break;
					case EShaderTargetStage.Hull:
						deviceContext.HullShader.SetShaderResource(textureTarget.targetSlot, textureSampler.Texture.GetTexture());
						deviceContext.HullShader.SetSampler(textureTarget.targetSlot, textureSampler.SamplerState);
						break;
				}
			}
			else
			{
				SetDefaultTextureResource(deviceContext, textureTarget);
			}
		}
		private static void SetDefaultTextureResource(DeviceContext deviceContext, SShaderTextureTarget parameterTarget)
		{
			CTextureSampler textureSampler = CRenderer.Instance.ResourceManager.BlackTexture;
			switch (parameterTarget.targetStage)
			{
				case EShaderTargetStage.Vertex:
					deviceContext.VertexShader.SetShaderResource(parameterTarget.targetSlot, textureSampler.Texture.GetTexture());
					deviceContext.VertexShader.SetSampler(parameterTarget.targetSlot, textureSampler.SamplerState);
					break;
				case EShaderTargetStage.Pixel:
					deviceContext.PixelShader.SetShaderResource(parameterTarget.targetSlot, textureSampler.Texture.GetTexture());
					deviceContext.PixelShader.SetSampler(parameterTarget.targetSlot, textureSampler.SamplerState);
					break;
				case EShaderTargetStage.Geometry:
					deviceContext.GeometryShader.SetShaderResource(parameterTarget.targetSlot, textureSampler.Texture.GetTexture());
					deviceContext.GeometryShader.SetSampler(parameterTarget.targetSlot, textureSampler.SamplerState);
					break;
				case EShaderTargetStage.Compute:
					deviceContext.ComputeShader.SetShaderResource(parameterTarget.targetSlot, textureSampler.Texture.GetTexture());
					deviceContext.ComputeShader.SetSampler(parameterTarget.targetSlot, textureSampler.SamplerState);
					break;
				case EShaderTargetStage.Domain:
					deviceContext.DomainShader.SetShaderResource(parameterTarget.targetSlot, textureSampler.Texture.GetTexture());
					deviceContext.DomainShader.SetSampler(parameterTarget.targetSlot, textureSampler.SamplerState);
					break;
				case EShaderTargetStage.Hull:
					deviceContext.HullShader.SetShaderResource(parameterTarget.targetSlot, textureSampler.Texture.GetTexture());
					deviceContext.HullShader.SetSampler(parameterTarget.targetSlot, textureSampler.SamplerState);
					break;
			}
		}

		private void WriteNonTextureParameterToBuffer(SShaderParameter shaderParam, int offset, DataStream stream)
		{
			stream.Position = offset;
			// Write to the data stream to update our buffer
			switch (shaderParam.parameterType)
			{
				case EShaderParameterType.Scalar:
					stream.Write((float)shaderParam.parameterData);
					break;
				case EShaderParameterType.Vector:
					stream.Write((Vector3)shaderParam.parameterData);
					break;
				case EShaderParameterType.Color:
					stream.Write((Vector4)shaderParam.parameterData);
					break;
				case EShaderParameterType.Matrix:
					stream.Write(Matrix.Transpose((Matrix)shaderParam.parameterData));
					break;
				default:
					throw new ArgumentException("ShaderParameterType not handled type was: " + shaderParam.parameterType);
			}
		}
		private void WriteDefaultNonTextureParameterToBuffer(SShaderParameterTarget parameterTarget, DataStream stream)
		{
			stream.Position = parameterTarget.offset;
			switch (parameterTarget.parameterType)
			{
				case EShaderParameterType.Scalar:
					stream.Write(0.0f);
					break;
				case EShaderParameterType.Vector:
					stream.Write(Vector3.Zero);
					break;
				case EShaderParameterType.Color:
					stream.Write(Vector4.Zero);
					break;
				case EShaderParameterType.Matrix:
					stream.Write(Matrix.Identity);
					break;
			}
		}

		private void OpenTargetBuffer(DeviceContext deviceContext, Buffer buffer, int targetSlot, EShaderTargetStage targetStage, out DataStream outStream)
		{
			// Open the target buffer
			deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out outStream);
		}
		private void CloseAndSetCurrentBuffer(DeviceContext deviceContext, Buffer buffer, int targetSlot, EShaderTargetStage targetStage)
		{
			// Close the previous buffer
			deviceContext.UnmapSubresource(buffer, 0);

			switch (targetStage)
			{
				case EShaderTargetStage.Vertex:
					deviceContext.VertexShader.SetConstantBuffer(targetSlot, buffer);
					break;
				case EShaderTargetStage.Pixel:
					deviceContext.PixelShader.SetConstantBuffer(targetSlot, buffer);
					break;
				case EShaderTargetStage.Geometry:
					deviceContext.GeometryShader.SetConstantBuffer(targetSlot, buffer);
					break;
				case EShaderTargetStage.Compute:
					deviceContext.ComputeShader.SetConstantBuffer(targetSlot, buffer);
					break;
				case EShaderTargetStage.Domain:
					deviceContext.DomainShader.SetConstantBuffer(targetSlot, buffer);
					break;
				case EShaderTargetStage.Hull:
					deviceContext.HullShader.SetConstantBuffer(targetSlot, buffer);
					break;
			}
		}

		/// <summary>
		/// Filename of the associated vertex shader, only needed when the shader is not initialized with precompiled byte-code, if empty no vertix shader is loaded
		/// </summary>
		public string VSFileName
		{ get; set; } = null;
		/// <summary>
		/// Filename of the associated pixel shader, only needed when the shader is not initialized with precompiled byte-code, if empty no pixel shader is loaded
		/// </summary>
		public string PSFileName
		{ get; set; } = null;
		/// <summary>
		/// Filename of the associated geometry shader, only needed when the shader is not initialized with precompiled byte-code, if empty no geom shader is loaded
		/// </summary>
		public string GSFileName
		{ get; set; } = null;
		/// <summary>
		/// Name of the vertex shader entry point
		/// </summary>
		public string VSEntryPoint
		{ get; set; } = "Vertex";
		/// <summary>
		/// Name of the pixel shader entry point
		/// </summary>
		public string PSEntryPoint
		{ get; set; } = "Pixel";
		/// <summary>
		/// Name of the geometry shader entry point
		/// </summary>
		public string GSEntryPoint
		{ get; set; } = "Geometry";

		public InputElement[] InputElements { get; protected set; }

		// Protected Fields
		protected VertexShader m_vertexShader;
		protected PixelShader m_pixelShader;
		protected GeometryShader m_geometryShader;

		// Private Fields
		private List<CShaderBufferDeclaration> m_bufferDeclarations = new List<CShaderBufferDeclaration>();
		private List<SShaderTextureTarget> m_textureTargets = new List<SShaderTextureTarget>();
		private ShaderSignature m_vertexShaderSignature;
		private InputLayout m_inputLayout;

		private bool m_shaderParamsSet;
		private bool m_bIsInitialized;
		private object m_shaderMutex = new object();
	}
}
