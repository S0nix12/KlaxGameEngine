using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxMath.Geometry;
using KlaxRenderer.Camera;
using KlaxRenderer.Debug;
using KlaxRenderer.Graphics;
using KlaxRenderer.Graphics.Shader;
using KlaxRenderer.Lights;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene.Commands;
using KlaxShared;
using KlaxShared.Definitions.Graphics;
using KlaxShared.Utilities;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Scene
{
	public class CRenderScene : IDisposable
	{
		public delegate void FrameCallback(float frameTime);
		internal event FrameCallback OnDoFrame;

		public void InitScene(Device device, DeviceContext deviceContext, IRenderSurface renderSurface)
		{
			SceneRenderer.Init(renderSurface, device);
			m_cameraBuffer.Init(device);
			m_cubeCameraBuffer.Init(device);

			LightManager.Init(device, deviceContext);
		}

		public void UpdateViewInfo(in SSceneViewInfo viewInfo)
		{
			m_viewInfo = viewInfo;
		}

		public void UpdateScene(float deltaTime)
		{
			OnDoFrame?.Invoke(deltaTime);
			DebugRenderer.Update(deltaTime);
		}

		public void RenderScene(Device device, DeviceContext deviceContext)
		{
			// Execute scene commands
			for (int i = m_sceneCommands.Count - 1; i >= 0; --i)
			{
				if (m_sceneCommands[i].TryExecute(device, deviceContext, this))
				{
					ContainerUtilities.RemoveSwapAt(m_sceneCommands, i);
				}
			}

			// Try creating resources for pending render nodes
			for (int i = m_pendingRenderNodes.Count - 1; i >= 0; --i)
			{
				CRenderNode pendingNode = m_pendingRenderNodes[i];
				if (pendingNode.TryCreateResources())
				{
					ContainerUtilities.RemoveSwapAt(m_pendingRenderNodes, i);
					m_activeRenderNodes.Add(pendingNode);
				}
			}

			if (m_viewInfo.FitProjectionToScene)
			{
				float screenAspect = (float)SceneRenderer.Width / (float)SceneRenderer.Height;
				m_viewInfo.ProjectionMatrix = Matrix.PerspectiveFovLH(m_viewInfo.Fov, screenAspect, m_viewInfo.ScreenNear, m_viewInfo.ScreenFar);
			}

			UserDefinedAnnotation annotation = deviceContext.QueryInterface<UserDefinedAnnotation>();


			annotation.BeginEvent("ShadowMapPass");
			LightManager.InitShadowMapsIfNeeded(device);
			LightManager.GenerateShadowMaps(device, deviceContext, this);
			annotation.EndEvent();

			annotation.BeginEvent("LightUpdatePass");
			LightManager.BindBuffer(deviceContext);
			LightManager.Update(deviceContext);
			LightManager.UpdatePerObjectLights(deviceContext, null);
			annotation.EndEvent();

			annotation.BeginEvent("SceneColorPass"); 

			m_cameraBuffer.BindBuffer(deviceContext, EShaderTargetStage.Vertex);
			m_cameraBuffer.UpdateBuffer(deviceContext, in m_viewInfo);

			BoundingFrustum cameraFrustum = m_viewInfo.CameraFrustum;

			for (int i = 0; i < m_activeRenderNodes.Count; i++)
			{
				if (m_activeRenderNodes[i].FrustumTest(in cameraFrustum) != ContainmentType.Disjoint)
				{
					m_activeRenderNodes[i].Draw(deviceContext);
				}
			}

			annotation.EndEvent();
			annotation.BeginEvent("SceneDebugPass");
			DebugRenderer.Draw(deviceContext, in m_viewInfo);
			annotation.EndEvent();
			annotation.Dispose();
		}

		public void RenderSceneDepth(Device device, DeviceContext deviceContext, in SSceneViewInfo viewInfo)
		{
			m_cameraBuffer.BindBuffer(deviceContext, EShaderTargetStage.Vertex);
			m_cameraBuffer.BindBuffer(deviceContext, EShaderTargetStage.Pixel);
			m_cameraBuffer.UpdateBuffer(deviceContext, in viewInfo);
			CShaderResource depthShader = CRenderer.Instance.ResourceManager.DepthShader;

			BoundingFrustum cameraFrustum = viewInfo.CameraFrustum;
			for (int i = 0; i < m_activeRenderNodes.Count; i++)
			{
				if (m_activeRenderNodes[i].FrustumTest(in cameraFrustum) != ContainmentType.Disjoint)
				{
					m_activeRenderNodes[i].DrawWithShader(deviceContext, depthShader);
				}
			}
		}

		public void RenderSceneDepthCube(DeviceContext deviceContext, in SSceneViewInfo viewInfo)
		{
			BoundingBox boundingBox = new BoundingBox();
			boundingBox.Minimum = new Vector3(-viewInfo.ScreenFar);
			boundingBox.Minimum += viewInfo.ViewLocation;
			boundingBox.Maximum = new Vector3(viewInfo.ScreenFar);
			boundingBox.Maximum += viewInfo.ViewLocation;

			m_cubeCameraBuffer.UpdateBuffer(deviceContext, in viewInfo);
			m_cubeCameraBuffer.BindBuffer(deviceContext, EShaderTargetStage.Geometry);
			m_cubeCameraBuffer.BindBuffer(deviceContext, EShaderTargetStage.Pixel);

			CShaderResource depthShader = CRenderer.Instance.ResourceManager.DepthCubeShader;

			for (int i = 0; i < m_activeRenderNodes.Count; i++)
			{
				if (m_activeRenderNodes[i].BoundingBoxTest(in boundingBox) != ContainmentType.Disjoint)
				{
					m_activeRenderNodes[i].DrawWithShader(deviceContext, depthShader);
				}
			}
		}

		public void AddCommand(IRenderSceneCommand command)
		{
			m_sceneCommands.Add(command);
		}

		public void RegisterRenderNode(CRenderNode node)
		{
			if (node.IsFullyLoaded)
			{
				m_activeRenderNodes.Add(node);
			}
			else
			{
				m_pendingRenderNodes.Add(node);
			}
		}

		public void UnregisterRenderNode(CRenderNode node)
		{
			if (!m_pendingRenderNodes.Remove(node))
			{
				m_activeRenderNodes.Remove(node);
			}
		}

		public bool RayIntersection(Ray ray, out CRenderNode hitNode, out STriangle hitTriangle, out float hitDistance, bool bMarkMesh = true)
		{
			List<(CRenderNode, STriangle, float)> nodeHits = new List<(CRenderNode, STriangle, float)>();
			foreach (var renderNode in m_activeRenderNodes)
			{
				if (renderNode.Intersects(ray, out STriangle triangle, out float distance))
				{
					nodeHits.Add((renderNode, triangle, distance));
				}
			}

			if (nodeHits.Count <= 0)
			{
				hitNode = null;
				hitTriangle = new STriangle();
				hitDistance = -1.0f;
				return false;
			}


			nodeHits.Sort((a, b) => a.Item3.CompareTo(b.Item3));
			var hit = nodeHits.First();
			Vector3 hitPoint = ray.Position + ray.Direction * hit.Item3;
			DebugRenderer.DrawPoint(hitPoint, 1, Color.Azure.ToColor4(), 5.0f);

			if (bMarkMesh && hit.Item1 is CMeshRenderNode meshNode)
			{
				meshNode.CreateUniqueMaterial();
				meshNode.m_overrideMaterial?.SetColorParameter(new SHashedName("tintColor"), new Vector4(1, 0, 0, 1));
			}

			hitNode = hit.Item1;
			hitTriangle = hit.Item2;
			hitDistance = hit.Item3;

			return true;
		}

		public void Dispose()
		{
			LightManager.Dispose();
			m_cameraBuffer.Dispose();
			DebugRenderer.Dispose();
			foreach (CRenderNode renderNode in m_activeRenderNodes)
			{
				renderNode.Dispose();
			}
			m_activeRenderNodes.Clear();

			foreach (CRenderNode renderNode in m_pendingRenderNodes)
			{
				renderNode.Dispose();
			}
			m_pendingRenderNodes.Clear();
			SceneRenderer.Dispose();
		}

		internal CWindowRenderer SceneRenderer { get; private set; } = new CWindowRenderer();
		public CSceneLightManager LightManager { get; private set; } = new CSceneLightManager();
		public CDebugRenderer DebugRenderer { get; private set; } = new CDebugRenderer();

		private readonly CCameraShaderBuffer m_cameraBuffer = new CCameraShaderBuffer();
		private readonly CCubeCameraShaderBuffer m_cubeCameraBuffer = new CCubeCameraShaderBuffer();
		private readonly List<CRenderNode> m_activeRenderNodes = new List<CRenderNode>();
		private readonly List<CRenderNode> m_pendingRenderNodes = new List<CRenderNode>();
		private readonly List<IRenderSceneCommand> m_sceneCommands = new List<IRenderSceneCommand>();
		private SSceneViewInfo m_viewInfo;
	}
}
