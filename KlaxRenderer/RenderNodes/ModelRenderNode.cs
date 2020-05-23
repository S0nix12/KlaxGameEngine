using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxMath.Geometry;
using KlaxRenderer.Graphics;
using KlaxRenderer.Graphics.Shader;
using SharpDX;
using SharpDX.Direct3D11;

namespace KlaxRenderer.RenderNodes
{
	public class CModelRenderNode : CRenderNode
	{
		private CModelRenderNode(object outer) : base(outer)
		{}

		public CModelRenderNode(object outer, CModel model) : base(outer)
		{
			m_model = model;
			m_bounds.center = m_model.AABoxCenter;
			m_bounds.extent = (m_model.AABoxMax - m_model.AABoxMin) / 2;
			IsFullyLoaded = true;
		}

		public override void Dispose()
		{
			m_model.Dispose();
		}

		public override void Draw(DeviceContext deviceContext)
		{
			m_model.Render(deviceContext);
		}

		internal override void DrawWithShader(DeviceContext deviceContext, CShaderResource shaderResource)
		{
			m_model.RenderWithShader(deviceContext, shaderResource);
		}

		public override bool TryCreateResources()
		{
			throw new NotImplementedException();
		}

		public override bool Intersects(Ray ray, out STriangle hitTriangle, out float hitDistance)
		{
			hitTriangle = new STriangle();
			hitDistance = -1.0f;
			return false;
		}

		public override ContainmentType FrustumTest(in BoundingFrustum frustum)
		{
			return ContainmentType.Contains;
		}

		public override ContainmentType BoundingBoxTest(in BoundingBox boundingBox)
		{
			return ContainmentType.Contains;
		}

		private CModel m_model;		
	}
}