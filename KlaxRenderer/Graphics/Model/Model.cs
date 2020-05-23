using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxRenderer.Camera;
using SharpDX;
using SharpDX.Direct3D11;
using KlaxRenderer.Lights;
using KlaxShared;
using KlaxRenderer.Graphics.Shader;

namespace KlaxRenderer.Graphics
{
    /// <summary>
    /// A Model represents a single assets usable by the engine user
    /// It can hold multiply meshes each with its own Material
    /// A multi mesh model is normally created if the user imports a 3d asset with multiple meshes in it and not using the option to split the content
    /// </summary>
    public class CModel : IDisposable
    {
        public void Init(Device device)
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.Init(device);
			}
		}
        public void Render(DeviceContext deviceContext)
        {
            foreach(CMesh mesh in m_meshes)
			{
				mesh.Render(deviceContext);
			}
		}

		internal void RenderWithShader(DeviceContext deviceContext, CShaderResource shaderResource)
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.RenderWithShader(deviceContext, shaderResource);
			}
		}

		public void SetScalarParameter(SHashedName parameterName, float scalar)
	    {
		    foreach (CMesh mesh in m_meshes)
		    {
			    mesh.Material.SetScalarParameter(parameterName, scalar);
		    }
	    }

	    public void SetVectorParameter(SHashedName parameterName, in Vector3 vector)
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.Material.SetVectorParameter(parameterName, in vector);
			}
		}

	    public void SetColorParameter(SHashedName parameterName, in Vector4 color)
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.Material.SetColorParameter(parameterName, in color);
			}
		}

	    public void SetMatrixParameter(SHashedName parameterName, in Matrix matrix)
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.Material.SetMatrixParameter(parameterName, in matrix);
			}
		}

	    public void SetTextureParameter(SHashedName parameterName, CTextureSampler texture)
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.Material.SetTextureParameter(parameterName, texture);
			}
		}

		public void Dispose()
		{
			foreach (CMesh mesh in m_meshes)
			{
				mesh.Dispose();
			}
		}

        public void AddMesh(ref CMesh mesh)
        {
            m_meshes.Add(mesh);
			mesh.SetParent(Transform);
        }

        public void SetAABox(in Vector3 min, in Vector3 max)
        {
            AABoxMin = min;
            AABoxMax = max;
            AABoxCenter = (min + max) * 0.5f;
        }

        public Vector3 AABoxMin
        { get; protected set; }
        public Vector3 AABoxMax
        { get; protected set; }
        public Vector3 AABoxCenter
        { get; protected set; }

        public Transform Transform
        { get; protected set; } = new Transform();

        List<CMesh> m_meshes = new List<CMesh>();
    }
}
