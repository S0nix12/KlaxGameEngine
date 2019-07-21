using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Graphics
{
	class CUIShader : CShader
	{
		[StructLayout(LayoutKind.Sequential)]
		struct ConstantBufferType
		{
			public Matrix mvpMatrix;
		}
		
		public CUIShader() : base()
		{
			VSFileName = "Resources/Shaders/UIShaderVS.hlsl";
			PSFileName = "Resources/Shaders/UIShaderPS.hlsl";

			InputElements = new[]
			{
				new InputElement("POSITION", 0, Format.R32G32_Float, 0),
				new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0),
				new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 0)
			};
		}

		public override void Dispose()
		{
			base.Dispose();
			m_matrixBuffer.Dispose();			
		}

		protected override void InitShaderBuffers(Device device)
		{
			m_matrixBuffer = new Buffer(device, Utilities.SizeOf<ConstantBufferType>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
		}

		protected override void InitShaderParameterTargets(Device device)
		{
			CShaderBufferDeclaration matrixBufferDeclaration = new CShaderBufferDeclaration();
			matrixBufferDeclaration.targetBuffer = m_matrixBuffer;
			matrixBufferDeclaration.targetSlot = 0;
			matrixBufferDeclaration.targetStage = EShaderTargetStage.Vertex;

			matrixBufferDeclaration.AddParameterTarget(new SHashedName("mvpMatrix"), EShaderParameterType.Matrix);
			AddShaderBufferDeclaration(matrixBufferDeclaration);

			SShaderTextureTarget textureTarget;
			textureTarget.targetSlot = 0;
			textureTarget.targetStage = EShaderTargetStage.Pixel;
			textureTarget.parameterName = new SHashedName("texture");
			AddShaderTextureTarget(in textureTarget);
		}

		private Buffer m_matrixBuffer;
	}
}
