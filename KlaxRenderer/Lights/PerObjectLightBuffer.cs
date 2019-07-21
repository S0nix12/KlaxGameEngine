using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using KlaxShared.Definitions.Graphics;

namespace KlaxRenderer.Lights
{
	/// <summary>
	/// This class represents the constant shader buffer that is updated per object with the light data that is affecting the object
	/// The buffer is constantly bind to register 12 in the shader stage and is updated on request for the given object
	/// this means the update should be called every frame before the rendering of the object
	/// </summary>
	class PerObjectLightBuffer : IDisposable
	{
		public const int TargetSlot = 12;
		public void Init(Device device)
		{
			// Our buffer holds an array of LightData with the size of MaxNumPositionalLights plus an int for the number of lights that should be used by the shader
			int bufferSize = Utilities.SizeOf<CPositionalLight.SPositionalLightShaderData>() * CPositionalLight.MaxNumPositionalLights + 16;
			BufferDescription bufferDescription = new BufferDescription(bufferSize, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_lightBuffer = new Buffer(device, bufferDescription);
		}

		public void BindBuffer(DeviceContext deviceContext, EShaderTargetStage targetStage)
		{
			switch (targetStage)
			{
				case EShaderTargetStage.Vertex:
					deviceContext.VertexShader.SetConstantBuffer(TargetSlot, m_lightBuffer);
					break;
				case EShaderTargetStage.Pixel:
					deviceContext.PixelShader.SetConstantBuffer(TargetSlot, m_lightBuffer);
					break;
				case EShaderTargetStage.Geometry:
					deviceContext.GeometryShader.SetConstantBuffer(TargetSlot, m_lightBuffer);
					break;
				case EShaderTargetStage.Compute:
					deviceContext.ComputeShader.SetConstantBuffer(TargetSlot, m_lightBuffer);
					break;
				case EShaderTargetStage.Domain:
					deviceContext.DomainShader.SetConstantBuffer(TargetSlot, m_lightBuffer);
					break;
				case EShaderTargetStage.Hull:
					deviceContext.HullShader.SetConstantBuffer(TargetSlot, m_lightBuffer);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(targetStage), targetStage, null);
			}
		}

		public void UpdateBuffer(DeviceContext deviceContext, List<CPositionalLight> lights)
		{
			int numLights = lights.Count <= CPositionalLight.MaxNumPositionalLights ? lights.Count : CPositionalLight.MaxNumPositionalLights;
			deviceContext.MapSubresource(m_lightBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
			dataStream.Write(numLights);
			dataStream.Position = 16;

			CPositionalLight.SPositionalLightShaderData lightData = new CPositionalLight.SPositionalLightShaderData();
			for (int i = 0; i < numLights; i++)
			{
				lights[i].FillShaderData(ref lightData);
				dataStream.Write(lightData);
			}
			deviceContext.UnmapSubresource(m_lightBuffer, 0);
			dataStream.Dispose();
		}

		public void Dispose()
		{
			m_lightBuffer?.Dispose();
		}

		private Buffer m_lightBuffer;
	}
}
