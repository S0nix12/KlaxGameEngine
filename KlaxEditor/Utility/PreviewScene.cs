using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using KlaxCore.Core;
using KlaxIO.AssetManager;
using KlaxIO.AssetManager.Assets;
using KlaxIO.Input;
using KlaxMath;
using KlaxRenderer;
using KlaxRenderer.Graphics;
using KlaxRenderer.Lights;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene;
using SharpDX;

namespace KlaxEditor.Utility
{
	public class PreviewScene
	{
		public PreviewScene()
		{
			m_cameraTransform.Parent = m_orbitTransform;
		}

		public void EditorThread_CreateScene(IRenderSurface renderSurface, EInputClass cameraInputClass)
		{
			IsCreated = true;
			OnEngineThread(() =>
			{
				Input.RegisterListener(EngineThread_OnInputEvent, cameraInputClass);
				// Create a new renderer scene with our viewport
				SSceneViewInfo viewInfo = new SSceneViewInfo();
				viewInfo.Fov = FieldOfView;
				viewInfo.ScreenFar = ScreenFar;
				viewInfo.ScreenNear = ScreenNear;
				viewInfo.FitProjectionToScene = true;
				viewInfo.ViewMatrix = Matrix.Invert(m_cameraTransform.WorldMatrix);
				viewInfo.ViewLocation = m_cameraTransform.Position;
				viewInfo.CreateBoundingFrustum();

				m_renderScene = CRenderer.Instance.CreateRenderScene(renderSurface);
				if (m_renderScene != null)
				{
					m_renderScene.OnDoFrame += EngineThread_Update;
					m_renderScene.UpdateViewInfo(in viewInfo);
					m_ambientLight = new CAmbientLight();
					m_ambientLight.LightColor = Vector4.One * 0.2f;
					m_directionalLight = new CDirectionalLight();
					m_directionalLight.LightDirection = new Vector3(0.3f, -0.7f, 0.0f);
					m_directionalLight.LightColor = Vector4.One * 0.8f;
					m_renderScene.LightManager.AddLight(m_ambientLight);
					m_renderScene.LightManager.AddLight(m_directionalLight);					
				}
			});
		}

		private void EngineThread_OnInputEvent(ReadOnlyCollection<SInputButtonEvent> buttonevents, string textinput)
		{
			Vector2 mousePos = Input.GetAbsoluteMousePosition();
			foreach (SInputButtonEvent buttonEvent in buttonevents)
			{
				if (buttonEvent.buttonEvent == EButtonEvent.Released)
				{
					switch (buttonEvent.button)
					{
						case EInputButton.MouseLeftButton:
							m_bIsLeftMouseDown = false;
							break;
						case EInputButton.MouseRightButton:
							m_bIsRightMouseDown = false;
							break;
					}
				}
				else
				{
					switch (buttonEvent.button)
					{
						case EInputButton.MouseLeftButton:
							m_bIsLeftMouseDown = IsInScreenBounds(mousePos);
							break;
						case EInputButton.MouseRightButton:
							m_bIsRightMouseDown = IsInScreenBounds(mousePos);
							break;
					}
				}
			}
		}

		public void EditorThread_SetPreviewMeshMaterial(CMaterial material)
		{
			OnEngineThread(() =>
			{
				m_previewMesh?.SetMaterialOverride(material);
			});
		}

		public void EngineThread_SetPreviewMeshMaterial(CMaterial material)
		{
			m_previewMesh?.SetMaterialOverride(material);
		}

		private void EngineThread_Update(float deltaTime)
		{
			m_renderScene.DebugRenderer.DrawArrow(Vector3.Zero, Axis.Forward, 1.0f, Color.Blue.ToColor4(), 0.0f);
			m_renderScene.DebugRenderer.DrawArrow(Vector3.Zero, Axis.Up, 1.0f, Color.Green.ToColor4(), 0.0f);
			m_renderScene.DebugRenderer.DrawArrow(Vector3.Zero, Axis.Right, 1.0f, Color.Red.ToColor4(), 0.0f);
			bool bUpdateViewInfo = false;
			if (m_bIsRightMouseDown)
			{
				float forwardDelta = Input.GetNativeAxisValue(EInputAxis.MouseX);
				Vector3 positionDelta = Axis.Forward * forwardDelta * ZoomSpeed;
				m_cameraTransform.Position += positionDelta;
				bUpdateViewInfo = true;
			}

			if (m_bIsLeftMouseDown)
			{
				float upAxisRotationAngle = Input.GetNativeAxisValue(EInputAxis.MouseX) * OrbitSpeed;
				float rightAxisRotationAngle = Input.GetNativeAxisValue(EInputAxis.MouseY) * OrbitSpeed;

				Quaternion xRotation = Quaternion.RotationAxis(KlaxMath.Axis.Up, upAxisRotationAngle);
				m_orbitTransform.RotateLocal(xRotation);
				Quaternion yRotation = Quaternion.RotationAxis(m_orbitTransform.Right, rightAxisRotationAngle);
				m_orbitTransform.RotateLocal(yRotation);
				bUpdateViewInfo = true;
			}

			if (bUpdateViewInfo)
			{
				SSceneViewInfo viewInfo = new SSceneViewInfo();
				viewInfo.Fov = FieldOfView;
				viewInfo.ScreenFar = ScreenFar;
				viewInfo.ScreenNear = ScreenNear;
				viewInfo.FitProjectionToScene = true;
				viewInfo.ViewMatrix = Matrix.Invert(m_cameraTransform.WorldMatrix);
				viewInfo.ViewLocation = m_cameraTransform.WorldPosition;
				viewInfo.CreateBoundingFrustum();
				m_renderScene.UpdateViewInfo(in viewInfo);
			}
			
			if (ShowMeshSelection)
			{
				CWindowRenderer window = m_renderScene.SceneRenderer;
				ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, window.Height - 40));
				ImGui.SetNextWindowSize(new System.Numerics.Vector2(window.Width, 40));
				ImGui.SetNextWindowBgAlpha(0);
				ImGui.Begin("MeshSelection", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar);
				if (ImGui.Button("Cu"))
				{
					EngineThread_SetPreviewMesh(EngineBaseContentLoader.DefaultCube, true, false);
					m_lastSelectedDefaultMesh = EngineBaseContentLoader.DefaultCube;
				}
				ImGui.SameLine();
				if (ImGui.Button("Sp"))
				{
					EngineThread_SetPreviewMesh(EngineBaseContentLoader.DefaultSphere, true, false);
					m_lastSelectedDefaultMesh = EngineBaseContentLoader.DefaultSphere;
				}
				ImGui.SameLine();
				if (ImGui.Button("Cy"))
				{
					EngineThread_SetPreviewMesh(EngineBaseContentLoader.DefaultCylinder, true, false);
					m_lastSelectedDefaultMesh = EngineBaseContentLoader.DefaultCylinder;
				}
				ImGui.End();
			}
		}

		public void OnEngineThread(Action action)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.EndFrame, action);
		}

		public void EditorThread_SetPreviewMesh(CMeshAsset meshAsset, bool bKeepMaterial, bool bZoomToFit = true)
		{
			OnEngineThread(() => EngineThread_SetPreviewMesh(meshAsset, bKeepMaterial, bZoomToFit));
		}

		public void EngineThread_SetPreviewMesh(CMeshAsset meshAsset, bool bKeepMaterial, bool bZoomToFit)
		{
			m_currentMeshAsset = meshAsset;
			CMaterial nodeMaterial = m_previewMesh?.GetOverrideMaterial();
			if (m_previewMesh != null)
			{
				m_renderScene?.UnregisterRenderNode(m_previewMesh);
			}
			m_previewMesh = new CMeshRenderNode(null, meshAsset, null, m_previewMeshTransform);
			if (bKeepMaterial)
			{
				m_previewMesh.SetMaterialOverride(nodeMaterial);
			}
			m_renderScene?.RegisterRenderNode(m_previewMesh);

			if (bZoomToFit)
			{
				EngineThread_ZoomCameraToFitAsset();
			}
		}

		public void EngineThread_SetLastDefaultMesh(bool bKeepMaterial, bool bZoomToFit = true)
		{
			if (m_lastSelectedDefaultMesh == null)
			{
				EngineThread_SetPreviewMesh(EngineBaseContentLoader.DefaultSphere, bKeepMaterial, bZoomToFit);
			}
			else
			{
				EngineThread_SetPreviewMesh(m_lastSelectedDefaultMesh, bKeepMaterial, bZoomToFit);
			}
		}

		public void EngineThread_ZoomCameraToFitAsset()
		{
			if (m_currentMeshAsset != null)
			{
				// Zoom camera
				float distance = (Vector3.Distance(m_currentMeshAsset.AABBMin, m_currentMeshAsset.AABBMax) / 2f) / MathUtilities.Tanf(FieldOfView / 2);
				m_cameraTransform.Position = Axis.Forward * -distance;

				SSceneViewInfo viewInfo = new SSceneViewInfo();
				viewInfo.Fov = FieldOfView;
				viewInfo.ScreenFar = ScreenFar;
				viewInfo.ScreenNear = ScreenNear;
				viewInfo.FitProjectionToScene = true;
				viewInfo.ViewMatrix = Matrix.Invert(m_cameraTransform.WorldMatrix);
				viewInfo.ViewLocation = m_cameraTransform.WorldPosition;
				viewInfo.CreateBoundingFrustum();
				m_renderScene?.UpdateViewInfo(in viewInfo);
			}
		}

		private bool IsInScreenBounds(Vector2 position)
		{
			CWindowRenderer sceneRenderer = m_renderScene.SceneRenderer;
			int top = sceneRenderer.Top;
			int left = sceneRenderer.Left;
			int bottom = sceneRenderer.Top + sceneRenderer.Height;
			int right = sceneRenderer.Left + sceneRenderer.Width;

			return (position.X >= left && position.X <= right && position.Y >= top && position.Y <= bottom);
		}

		public bool ShowMeshSelection { get; set; } = false;
		public float FieldOfView { get; set; } = MathUtil.PiOverFour;
		public float ScreenNear { get; set; } = 0.2f;
		public float ScreenFar { get; set; } = 10000.0f;
		public float ZoomSpeed { get; set; } = 0.03f;
		public float OrbitSpeed { get; set; } = 0.005f;
		public bool IsCreated { get; private set; }

		private CDirectionalLight m_directionalLight;
		private CAmbientLight m_ambientLight;

		private bool m_bIsLeftMouseDown;
		private bool m_bIsRightMouseDown;
		private CMeshAsset m_lastSelectedDefaultMesh;
		private CMeshAsset m_currentMeshAsset;

		private CRenderScene m_renderScene;
		private readonly Transform m_orbitTransform = new Transform();
		private readonly Transform m_cameraTransform = new Transform(new Vector3(0, 0, -3));
		private readonly Transform m_previewMeshTransform = new Transform();
		private CMeshRenderNode m_previewMesh;
	}
}
