using System;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugPrimitiveMap : IDisposable
	{
		public void Init(Device device)
		{
			m_debugPrimitiveMap[(int)EDebugPrimitiveType.Cube] = new CDebugCube();
			m_debugPrimitiveMap[(int)EDebugPrimitiveType.Sphere] = new CDebugSphere();
			m_debugPrimitiveMap[(int)EDebugPrimitiveType.Cylinder] = new CDebugCylinder();
			m_debugPrimitiveMap[(int)EDebugPrimitiveType.Cone] = new CDebugCone();
			m_debugPrimitiveMap[(int)EDebugPrimitiveType.Pyramid] = new CDebugPyramid();
			m_debugPrimitiveMap[(int)EDebugPrimitiveType.Hemisphere] = new CDebugHemisphere();

			foreach (CDebugDrawPrimitive primitive in m_debugPrimitiveMap)
			{
				primitive.Init(device);
			}
		}

		public void Dispose()
		{
			foreach (CDebugDrawPrimitive primitive in m_debugPrimitiveMap)
			{
				primitive.Dispose();
			}
		}

		public CDebugDrawPrimitive GetPrimitiveInstance(EDebugPrimitiveType type)
		{
			return m_debugPrimitiveMap[(int) type];
		}
		public CDebugDrawPrimitive GetPrimitiveInstance(int index)
		{
			return m_debugPrimitiveMap[index];
		}

		private readonly CDebugDrawPrimitive[] m_debugPrimitiveMap = new CDebugDrawPrimitive[(int)EDebugPrimitiveType.Count];
	}
}
