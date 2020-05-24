using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxRenderer.Graphics;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Debug
{
	[StructLayout(LayoutKind.Sequential)]
	struct ShaderBufferType
	{
		public Matrix viewProjectionMatrix;
	}

	class CDebugObjectShader : CShader
	{
		public CDebugObjectShader()
		{
			VSFileName = "Resources/Shaders/DebugObjectVS.hlsl";
			PSFileName = "Resources/Shaders/DebugObjectPS.hlsl";

			InputElements = new []
			{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
				new InputElement("INSTANCE_COLOR", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
				new InputElement("INSTANCE_TRANSFORM", 0, Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1),
				new InputElement("INSTANCE_TRANSFORM", 1, Format.R32G32B32A32_Float, 32, 1, InputClassification.PerInstanceData, 1),
				new InputElement("INSTANCE_TRANSFORM", 2, Format.R32G32B32A32_Float, 48, 1, InputClassification.PerInstanceData, 1),
				new InputElement("INSTANCE_TRANSFORM", 3, Format.R32G32B32A32_Float, 64, 1, InputClassification.PerInstanceData, 1)
			};
		}

		protected override void InitShaderBuffers(Device device)
		{
			m_matrixBuffer = new Buffer(device, Utilities.SizeOf<ShaderBufferType>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
		}

		protected override void InitShaderParameterTargets(Device device)
		{
			CShaderBufferDeclaration bufferDeclaration = new CShaderBufferDeclaration();
			bufferDeclaration.targetBuffer = m_matrixBuffer;
			bufferDeclaration.targetStage = EShaderTargetStage.Vertex;
			bufferDeclaration.targetSlot = 0;

			bufferDeclaration.AddParameterTarget(new SHashedName("viewProjectionMatrix"), EShaderParameterType.Matrix);

			AddShaderBufferDeclaration(in bufferDeclaration);
		}

		public override void Dispose()
		{
			base.Dispose();
			m_matrixBuffer.Dispose();
		}

		private Buffer m_matrixBuffer;
	}
}
