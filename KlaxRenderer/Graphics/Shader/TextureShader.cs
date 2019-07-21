using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics
{
    class CTextureShader : CShader
    {
		public CTextureShader() : base()
		{
			VSFileName = "Resources/Shaders/TextureShader.hlsl";
			PSFileName = "Resources/Shaders/TextureShader.hlsl";

			InputElements = new[]
			{
				new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 0)
			};
		}
        public override void Dispose()
        {
			base.Dispose();
            m_matrixBuffer.Dispose();
			m_colorBuffer.Dispose();
        }

		protected override void InitShaderBuffers(Device device)
		{
			BufferDescription bufferDescription = new BufferDescription(MatrixBufferType.SizeInBytes(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_matrixBuffer = new SharpDX.Direct3D11.Buffer(device, bufferDescription);

			BufferDescription colorBufferDesc = new BufferDescription(Vector4.SizeInBytes, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_colorBuffer = new SharpDX.Direct3D11.Buffer(device, colorBufferDesc);
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

			CShaderBufferDeclaration colorBufferDeclaration = new CShaderBufferDeclaration();
			colorBufferDeclaration.targetBuffer = m_colorBuffer;
			colorBufferDeclaration.targetSlot = 1;
			colorBufferDeclaration.targetStage = EShaderTargetStage.Pixel;
			colorBufferDeclaration.AddParameterTarget(new SHashedName("tintColor"), EShaderParameterType.Color);

			AddShaderBufferDeclaration(colorBufferDeclaration);

			SShaderTextureTarget diffuseTextureTarget;
			diffuseTextureTarget.parameterName = new SHashedName("diffuseTexture");
			diffuseTextureTarget.targetSlot = 0;
			diffuseTextureTarget.targetStage = EShaderTargetStage.Pixel;
			AddShaderTextureTarget(in diffuseTextureTarget);
		}

		private SharpDX.Direct3D11.Buffer m_matrixBuffer;
		private SharpDX.Direct3D11.Buffer m_colorBuffer;
    }
}
