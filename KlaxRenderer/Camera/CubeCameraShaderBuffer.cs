using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxRenderer.Graphics;
using KlaxRenderer.Lights;
using KlaxRenderer.Scene;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace KlaxRenderer.Camera
{
	class CCubeCameraShaderBuffer : IDisposable
	{
		public const int TargetSlot = 11;

#pragma warning disable 0169, 0649
		[StructLayout(LayoutKind.Sequential)]
		private struct BufferType
		{
			public Matrix projectionMatrix;
			public Matrix viewMatrixX;
			public Matrix viewMatrixNX;
			public Matrix viewMatrixY;
			public Matrix viewMatrixNY;
			public Matrix viewMatrixZ;
			public Matrix viewMatrixNZ;

			public Vector3 cubeCenterPos;
			public float cubeFarPlane;
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
			dataStream.Write(Matrix.Transpose(viewInfo.ProjectionMatrix));

			Vector3[] forwardArray = { Axis.Right, -Axis.Right, Axis.Up, -Axis.Up, Axis.Forward, -Axis.Forward, };
			Vector3[] upArray = { Axis.Up, Axis.Up, -Axis.Forward, Axis.Forward, Axis.Up, Axis.Up };

			for (int i = 0; i < 6; i++)
			{

				Vector3 worldPos = viewInfo.ViewLocation;
				Matrix viewMatrix = Matrix.LookAtLH(worldPos, worldPos + forwardArray[i], upArray[i]);
				dataStream.Write(Matrix.Transpose(viewMatrix));
			}

			dataStream.Write(viewInfo.ViewLocation);
			dataStream.Write(viewInfo.ScreenFar);
			deviceContext.UnmapSubresource(m_cameraBuffer, 0);
		}

		private Buffer m_cameraBuffer;
	}
}
