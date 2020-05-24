using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugCone : CDebugDrawPrimitive
	{
		private const string MODEL_FILE = "Resources/DebugPrimitives/DebugCone.obj";
		protected override void CreateBuffer(Device device)
		{
			CreateBufferFromFile(device, MODEL_FILE);
		}
	}
}
