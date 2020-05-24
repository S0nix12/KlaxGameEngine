using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugHemisphere : CDebugDrawPrimitive
	{
		private const string MODEL_FILE = "Resources/DebugPrimitives/DebugHemisphere.obj";
		protected override void CreateBuffer(Device device)
		{
			CreateBufferFromFile(device, MODEL_FILE);
		}
	}
}
