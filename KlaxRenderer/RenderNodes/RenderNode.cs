using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxMath.Geometry;
using KlaxRenderer.Graphics.Shader;
using SharpDX;
using SharpDX.Direct3D11;

namespace KlaxRenderer.RenderNodes
{
	/// <summary>
	/// Struct that represents a box and sphere bounds with the same center
	/// </summary>
	public struct SBoxBounds
	{
		public Vector3 center;
		public Vector3 extent;
	}

	public abstract class CRenderNode : IDisposable
	{
		protected CRenderNode(object outer)
		{
			Outer = outer;
		}

		public abstract void Dispose();

		public abstract void Draw(DeviceContext deviceContext);
		internal abstract void DrawWithShader(DeviceContext deviceContext, CShaderResource shaderResource);
		public abstract bool TryCreateResources();
		public abstract bool Intersects(Ray ray, out STriangle hitTriangle, out float hitDistance);
		public abstract ContainmentType FrustumTest(in BoundingFrustum frustum);
		public abstract ContainmentType BoundingBoxTest(in BoundingBox boundingBox);
		public bool IsFullyLoaded { get; protected set; }
		public object Outer { get; private set; }
		public SBoxBounds m_bounds;
	}
}
