using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugCylinder : CDebugDrawPrimitive
	{
		private const string MODEL_FILE = "Resources/DebugPrimitives/DebugCylinder.obj";
		protected override void CreateBuffer(Device device)
		{
			CreateBufferFromFile(device, MODEL_FILE);
		}
	}
}
