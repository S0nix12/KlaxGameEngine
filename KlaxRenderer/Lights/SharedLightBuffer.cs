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
	class SharedLightBuffer : IDisposable
	{
		public const int TargetSlot = 13;

#pragma warning disable 0169, 0649
		private struct BufferType
		{
			public Vector4 ambientColor;
			public Vector4 directionalLightColor;
			public Vector3 directionalLightDirection;
			private float _padding;
		}
#pragma warning restore 0169, 0649

		public void Init(Device device)
		{
			BufferDescription bufferDescription = new BufferDescription(Utilities.SizeOf<BufferType>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_lightBuffer = new Buffer(device, bufferDescription);
		}
		public void Dispose()
		{
			m_lightBuffer?.Dispose();
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

		public void UpdateBuffer(DeviceContext deviceContext, in Vector4 ambientColor, CDirectionalLight directionalLight)
		{
			deviceContext.MapSubresource(m_lightBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
			dataStream.Write(ambientColor);

			if (directionalLight != null)
			{
				dataStream.Write(directionalLight.LightColor);
				dataStream.Write(directionalLight.LightDirection);
			}
			deviceContext.UnmapSubresource(m_lightBuffer, 0);			
		}
		
		private Buffer m_lightBuffer;
	}
}
