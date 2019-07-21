using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using KlaxShared;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using KlaxShared.Definitions.Graphics;

namespace KlaxRenderer.Graphics
{
    // TODO henning test Code replace with precaluclated WorldViewProjection Matrix
    [StructLayout(LayoutKind.Sequential)]
    struct MatrixBufferType
    {
        Matrix world;
		Matrix inverseTransposeWorld;

        public static int SizeInBytes()
        {
            return Matrix.SizeInBytes * 2;
        }
    }

	public delegate void ShaderSignatureCallback(Device device, ShaderSignature shaderSignature);
    public interface IShader : IDisposable
    {
        bool Init(Device device);
        bool Init(Device device, string vertexShaderFilename, string pixelShaderFilename, string vertexShaderEntry, string pixelShaderEntry);
        bool Init(Device device, byte[] vsCode, byte[] psCode);
        void SetActive(SharpDX.Direct3D11.DeviceContext deviceContext);
		void SetShaderParameters(DeviceContext deviceContext, Dictionary<SHashedName, SShaderParameter> parameters);

		//void SetTextureParameter(SharpDX.Direct3D11.DeviceContext deviceContext, SHashedName parameterName, CTexture texture);
		//void SetVectorParameter(SharpDX.Direct3D11.DeviceContext deviceContext, SHashedName parameterName, in Vector3 vector);
		//void SetColorParameter(SharpDX.Direct3D11.DeviceContext deviceContext, SHashedName parameterName, in Vector4 color);
		//void SetScalarParameter(SharpDX.Direct3D11.DeviceContext deviceContext, SHashedName parameterName, float scalar);
		//void SetMatrixParameter(SharpDX.Direct3D11.DeviceContext deviceContext, SHashedName parameterName, in Matrix color);
	}
}
