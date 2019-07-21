using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics
{
    class CColorShader : CShader
    {
		public CColorShader() : base()
		{
			VSFileName = "Resources/Shaders/ColorShader.hlsl";
			PSFileName = "Resources/Shaders/ColorShader.hlsl";

			InputElements = new[]
			{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
				new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0)
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

			matrixBufferDeclaration.AddParameterTarget(new SHashedName("worldMatrix"), EShaderParameterType.Matrix);
			matrixBufferDeclaration.AddParameterTarget(new SHashedName("invTransposeWorldMatrix"), EShaderParameterType.Matrix);

			AddShaderBufferDeclaration(in matrixBufferDeclaration);
		}
		
        private SharpDX.Direct3D11.Buffer m_matrixBuffer;
    }
}
