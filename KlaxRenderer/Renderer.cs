using KlaxRenderer.Camera;
using KlaxRenderer.Graphics;
using KlaxRenderer.Tests;
using SharpDX.Windows;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer
{
    public class CRenderer : IDisposable
    {
        public CRenderer()
        { }

        public void Init(string windowName, int width, int height)
        {
            m_d3dRenderer.Init(windowName, width, height);
            Init();
        }

        public void Init(RenderForm window)
        {
            m_d3dRenderer.Init(window);
            Init();
        }

        private void Init()
        {
            m_testMesh.Init(m_d3dRenderer.D3DDevie);
            m_testMesh2.Init(m_d3dRenderer.D3DDevie);
        }

        public void Run()
        {
            m_testMesh2.Transform.Position = new Vector3(0.0f, -1.0f, 0.0f);
            Quaternion deltaRotation = Quaternion.RotationAxis(KlaxMath.Axis.Right, MathUtil.DegreesToRadians(70));
            m_testMesh2.Transform.Rotation = deltaRotation * m_testMesh2.Transform.Rotation;
            //m_testMesh2.Transform.Rotation = Quaternion.RotationLookAtLH(new Vector3(0.0f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f));
            m_testMesh2.Transform.Scale = new Vector3(3.0f, 3.0f, 3.0f);
            Camera.Transform.Position = new Vector3(0.0f, 0.0f, -5.0f);
            Camera.UpdateViewParams();
            m_d3dRenderer.Run(DrawCallback);
        }

        private void DrawCallback(SharpDX.Direct3D11.DeviceContext deviceContext)
        {
            Camera.UpdateViewParams();
            m_testMesh2.Draw(deviceContext, Camera);
            m_testMesh.Draw(deviceContext, Camera);

            Quaternion deltaRotation = Quaternion.RotationAxis(KlaxMath.Axis.Up, 0.05f);
            Quaternion deltaRotation2 = Quaternion.RotationAxis(KlaxMath.Axis.Up, 0.02f);
            m_testMesh.Transform.Rotation = deltaRotation * m_testMesh.Transform.Rotation;
            m_testMesh2.Transform.Rotation = deltaRotation2 * m_testMesh2.Transform.Rotation;
        }

        public void Dispose()
        {
            m_d3dRenderer.Dispose();
        }

        public ICamera Camera
        { get; set; } = new CBaseCamera();

        CD3DRenderer m_d3dRenderer = new CD3DRenderer();
        CMesh m_testMesh = new CTestTriangle();
        CMesh m_testMesh2 = new CTestTriangle();
    }
}
