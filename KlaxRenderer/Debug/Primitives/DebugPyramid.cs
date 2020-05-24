using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugPyramid : CDebugDrawPrimitive
	{
		private const string MODEL_FILE = "Resources/DebugPrimitives/DebugPyramid.obj";
		protected override void CreateBuffer(Device device)
		{
			CreateBufferFromFile(device, MODEL_FILE);
		}
	}
}
