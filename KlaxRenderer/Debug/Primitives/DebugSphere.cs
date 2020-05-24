using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugSphere : CDebugDrawPrimitive
	{
		private const string MODEL_FILE = "Resources/DebugPrimitives/DebugSphere.obj";

		protected override void CreateBuffer(Device device)
		{
			CreateBufferFromFile(device, MODEL_FILE);
		}
	}
}
