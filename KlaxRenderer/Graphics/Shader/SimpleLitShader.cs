using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Graphics
{
	class CSimpleLitShader : CShader
	{
		[StructLayout(LayoutKind.Sequential)]
		struct MaterialBufferType
		{
			public Vector4 diffuseTint;
			public Vector4 specularColor;
			public float specularPower;
			private Vector3 _padding;
		}
		public CSimpleLitShader() : base()
		{
			VSFileName = "Resources/Shaders/SimpleLitVS.hlsl";
			PSFileName = "Resources/Shaders/SimpleLitPS.hlsl";

			// Default Input Layout
			InputElements = new[]
			{
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
				new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0),
				new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
				new InputElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
				new InputElement("BITANGENT", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 0)
			};
		}

		public override void Dispose()
		{
			base.Dispose();
			m_matrixBuffer.Dispose();
			m_matDescBuffer.Dispose();
		}

		protected override void InitShaderBuffers(Device device)
		{
			BufferDescription bufferDescription = new BufferDescription(MatrixBufferType.SizeInBytes() + 16, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_matrixBuffer = new SharpDX.Direct3D11.Buffer(device, bufferDescription);

			BufferDescription colorBufferDesc = new BufferDescription(Utilities.SizeOf<MaterialBufferType>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
			m_matDescBuffer = new SharpDX.Direct3D11.Buffer(device, colorBufferDesc);
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

			CShaderBufferDeclaration matBufferDeclaration = new CShaderBufferDeclaration();
			matBufferDeclaration.targetBuffer = m_matDescBuffer;
			matBufferDeclaration.targetSlot = 1;
			matBufferDeclaration.targetStage = EShaderTargetStage.Pixel;
			matBufferDeclaration.AddParameterTarget(new SHashedName("tintColor"), EShaderParameterType.Color);
			matBufferDeclaration.AddParameterTarget(new SHashedName("specularColor"), EShaderParameterType.Color);
			matBufferDeclaration.AddParameterTarget(new SHashedName("specularPower"), EShaderParameterType.Scalar);

			AddShaderBufferDeclaration(matBufferDeclaration);

			SShaderTextureTarget diffuseTextureTarget;
			diffuseTextureTarget.parameterName = new SHashedName("diffuseTexture");
			diffuseTextureTarget.targetSlot = 0;
			diffuseTextureTarget.targetStage = EShaderTargetStage.Pixel;
			AddShaderTextureTarget(in diffuseTextureTarget);
		}

		private SharpDX.Direct3D11.Buffer m_matrixBuffer;
		private SharpDX.Direct3D11.Buffer m_matDescBuffer;
	}
}
