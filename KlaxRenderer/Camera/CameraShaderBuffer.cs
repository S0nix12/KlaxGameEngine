using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Graphics;
using KlaxRenderer.Lights;
using KlaxRenderer.Scene;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace KlaxRenderer.Camera
{
	class CCameraShaderBuffer : IDisposable
	{
		public const int TargetSlot = 11;

#pragma warning disable 0169, 0649
		private struct BufferType
		{
			public Matrix viewProjectionMatrix;
			public Vector3 directionalLightDirection;
			private float _padding;
		}
#pragma warning restore 0169, 0649

		public void Init(Device device)
		{
			BufferDescription bufferDescription = new BufferDescription(Utilities.SizeOf<BufferType>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_cameraBuffer = new Buffer(device, bufferDescription);
		}
		public void Dispose()
		{
			m_cameraBuffer?.Dispose();
		}

		public void BindBuffer(DeviceContext deviceContext, EShaderTargetStage targetStage)
		{
			switch (targetStage)
			{
				case EShaderTargetStage.Vertex:
					deviceContext.VertexShader.SetConstantBuffer(TargetSlot, m_cameraBuffer);
					break;
				case EShaderTargetStage.Pixel:
					deviceContext.PixelShader.SetConstantBuffer(TargetSlot, m_cameraBuffer);
					break;
				case EShaderTargetStage.Geometry:
					deviceContext.GeometryShader.SetConstantBuffer(TargetSlot, m_cameraBuffer);
					break;
				case EShaderTargetStage.Compute:
					deviceContext.ComputeShader.SetConstantBuffer(TargetSlot, m_cameraBuffer);
					break;
				case EShaderTargetStage.Domain:
					deviceContext.DomainShader.SetConstantBuffer(TargetSlot, m_cameraBuffer);
					break;
				case EShaderTargetStage.Hull:
					deviceContext.HullShader.SetConstantBuffer(TargetSlot, m_cameraBuffer);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(targetStage), targetStage, null);
			}
		}

		public void UpdateBuffer(DeviceContext deviceContext, in SSceneViewInfo viewInfo)
		{
			deviceContext.MapSubresource(m_cameraBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
			dataStream.Write(Matrix.Transpose(viewInfo.ViewMatrix * viewInfo.ProjectionMatrix));
			dataStream.Write(viewInfo.ViewLocation);
			deviceContext.UnmapSubresource(m_cameraBuffer, 0);
		}

		private Buffer m_cameraBuffer;
	}
}
