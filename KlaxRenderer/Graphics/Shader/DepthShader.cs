using KlaxShared.Definitions.Graphics;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics.Shader
{
	class CDepthShader : CShader
	{
		public CDepthShader() : base()
		{
			VSFileName = "Resources/Shaders/DepthShader.hlsl";
			PSFileName = "Resources/Shaders/DepthShader.hlsl";

			InputElements = new[]
			{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0)
			};
		}

		public override void Dispose()
		{
			base.Dispose();
			m_matrixBuffer.Dispose();
		}

		protected override void InitShaderBuffers(Device device)
		{
			BufferDescription bufferDescription = new BufferDescription(MatrixBufferType.SizeInBytes(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_matrixBuffer = new SharpDX.Direct3D11.Buffer(device, bufferDescription);
		}

		protected override void InitShaderParameterTargets(Device device)
		{
			CShaderBufferDeclaration matrixBufferDeclaration = new CShaderBufferDeclaration();
			matrixBufferDeclaration.targetBuffer = m_matrixBuffer;
			matrixBufferDeclaration.targetSlot = 0;
			matrixBufferDeclaration.targetStage = EShaderTargetStage.Vertex;

			matrixBufferDeclaration.AddParameterTarget(SShaderParameterNames.WorldMatrixParameterName, EShaderParameterType.Matrix);
			matrixBufferDeclaration.AddParameterTarget(SShaderParameterNames.InvTransWorldMatrixParName, EShaderParameterType.Matrix);

			AddShaderBufferDeclaration(in matrixBufferDeclaration);
		}

		private SharpDX.Direct3D11.Buffer m_matrixBuffer;
	}
}
