using System;
using KlaxCore.GameFramework;
using KlaxRenderer;
using KlaxShared;
using SharpDX;
using ImGuiNET;
using KlaxCore.Core.View;
using KlaxCore.GameFramework.Camera;
using KlaxCore.GameFramework.Console;
using KlaxIO.AssetManager.Assets;
using KlaxIO.AssetManager.Loaders;
using KlaxMath;
using KlaxRenderer.Scene;
using System.Collections.Generic;
using System.Diagnostics;
using BEPUphysics.Materials;
using KlaxCore.GameFramework.Editor;
using KlaxCore.GameFramework.Lighting;
using KlaxCore.Physics;
using KlaxCore.Physics.Components;
using KlaxCore.Utility;
using Quaternion = SharpDX.Quaternion;
using Vector3 = SharpDX.Vector3;
using KlaxCore.GameFramework.Assets;
using KlaxRenderer.Debug;

namespace KlaxCore.Core
{
	public class CEntitySpawnParameters
	{
		public Vector3 position = Vector3.One;
		public Quaternion rotation = Quaternion.Identity;
		public Vector3 scale = Vector3.One;
		public object userData = null;
	}

	public class CWorld
	{
		public void Init(CInitializer initializer)
		{
			UpdateScheduler = new CUpdateScheduler();

			CreateLevel();
			GameConsole.Init();
			PhysicsWorld.Init(this);


			#region Sponza
			CModelAsset sponzaAsset = CImportManager.Instance.MeshImporter.LoadModelAsync("TestResources/SponzaAtrium/sponza.obj");
			m_sponzaEntity = SpawnEntity<CEntity>();
			CModelComponent modelComponent = m_sponzaEntity.AddComponent<CModelComponent>(true, true);
			m_sponzaEntity.SetWorldPosition(new Vector3(0, -5, -5));
			m_sponzaEntity.SetWorldRotation(Quaternion.RotationAxis(Axis.Up, MathUtil.DegreesToRadians(90)));
			m_sponzaEntity.SetWorldScale(new Vector3(0.03f));
			CStaticModelColliderComponent staticModelCollider = m_sponzaEntity.AddComponent<CStaticModelColliderComponent>(true, true);
			staticModelCollider.ModelAsset = sponzaAsset;
			modelComponent.Model = sponzaAsset;
			m_sponzaEntity.IsPhysicsStatic = true;
			m_sponzaEntity.IsPhysicsEnabled = true;
			#endregion

			CMeshAsset cubeAsset = CImportManager.Instance.MeshImporter.LoadMeshAsync("EngineResources/DefaultMeshes/DefaultCube.obj");

			CEntity floorEntity = SpawnEntity<CEntity>();
			floorEntity.AddComponent<CSceneComponent>(true, true);
			CMeshComponent floorMesh = floorEntity.AddComponent<CMeshComponent>(true, true);
			floorMesh.Mesh = cubeAsset;
			floorMesh.LocalScale = new Vector3(500, 1, 500);
			CBoxColliderComponent floorCollider = floorEntity.AddComponent<CBoxColliderComponent>(true, true);
			floorCollider.Height = 1;
			floorCollider.Width = 500;
			floorCollider.Length = 500;
			floorEntity.SetLocalPosition(new Vector3(0, -15, 0));
			floorEntity.IsPhysicsStatic = true;
			floorEntity.IsPhysicsEnabled = true;
			floorEntity.PhysicalStatic.Material = new Material(1, 0.8f, 1f);			

			#region LightSetup
			m_lightEntity = SpawnEntity<CEntity>();
			m_lightEntity.AddComponent<CSceneComponent>(true, true);

			CDirectionalLightComponent directionalLight = m_lightEntity.AddComponent<CDirectionalLightComponent>(true, true);
			directionalLight.LocalRotation = MathUtilities.CreateLookAtQuaternion(Vector3.Normalize(new Vector3(0.2f, -0.5f, 0.2f)), Axis.Up);
			directionalLight.LightColor = Color4.White * 0.8f;

			CSpotLightComponent spotLight = m_lightEntity.AddComponent<CSpotLightComponent>(true, true);
			spotLight.ConstantAttenuation = 0.01f;
			spotLight.LinearAttenuation = 0.3f;
			spotLight.QuadraticAttenuation = 0.0f;
			spotLight.Enabled = true;
			spotLight.SpotAngle = MathUtil.DegreesToRadians(30.0f);
			spotLight.LightColor = new Color4(0.1f, 0.8f, 0.1f, 1.0f);
			spotLight.Range = 100.0f;
			Quaternion deltaRotation = Quaternion.RotationAxis(Axis.Right, MathUtil.DegreesToRadians(10));
			spotLight.LocalRotation = deltaRotation;
			spotLight.LocalPosition = new Vector3(0, 1, -4);

			CPointLightComponent pointLight1 = m_lightEntity.AddComponent<CPointLightComponent>(true, true);
			pointLight1.ConstantAttenuation = 1;
			pointLight1.LinearAttenuation = 0.2f;
			pointLight1.Enabled = true;
			pointLight1.Range = 100.0f;
			pointLight1.LightColor = new Color4(0.8f, 0.1f, 0.1f, 1.0f);
			pointLight1.LocalPosition = new Vector3(0, 0, 3.0f);

			CPointLightComponent pointLight2 = m_lightEntity.AddComponent<CPointLightComponent>(true, true);
			pointLight2.ConstantAttenuation = 1;
			pointLight2.LinearAttenuation = 0.4f;
			pointLight2.Enabled = true;
			pointLight2.Range = 100.0f;
			pointLight2.LightColor = new Color4(0.1f, 0.1f, 0.8f, 1.0f);
			pointLight2.LocalPosition = new Vector3(0, -3, -8.0f);

			CAmbientLightComponent ambientLight = m_lightEntity.AddComponent<CAmbientLightComponent>(true, true);
			ambientLight.LightColor = Color4.White * 0.15f;
			#endregion
		}

		internal void TriggerHierarchyChanged(CSceneComponent child, CSceneComponent oldParent, CSceneComponent newParent)
		{
			OnHierarchyChanged?.Invoke(child, oldParent, newParent);
		}

		public void StartPlayMode()
		{
			IsPlaying = true;
			
			if (LoadedLevel != null)
			{
				LoadedLevel.IsPlaying = true;
			}
		}

		public void StopPlayMode()
		{
			IsPlaying = false;

			if (LoadedLevel != null)
			{
				LoadedLevel.IsPlaying = false;
			}
		}


		public void Shutdown()
		{

		}

		public void Update(float deltaSeconds)
		{
			if (IsPlaying)
			{
				UpdateScheduler.Update(deltaSeconds, EUpdatePriority.ResourceLoading, EUpdatePriority.PrePhysics);
				PhysicsWorld.Update(deltaSeconds);
				UpdateScheduler.Update(deltaSeconds, EUpdatePriority.PostPhysics, EUpdatePriority.Latest);
			}
			else
			{
				UpdateScheduler.Update(deltaSeconds, EUpdatePriority.Editor, EUpdatePriority.ResourceLoading);
			}

			GameConsole.Draw(ViewManager.ScreenWidth, ViewManager.ScreenHeight);
			ViewManager.GetViewInfo(out SSceneViewInfo viewInfo);

			#region MenuBar
			m_performanceCounter.UpdateFrametime(deltaSeconds);
			
			ImGui.BeginMainMenuBar();
			ImGui.Text("FPS: " + (1 / m_performanceCounter.SmoothedFrametime).ToString("n2"));
			ImGui.Text(string.Format("CameraPosition X: {0:0.00} Y: {1:0.00} Z: {2:0.00}", viewInfo.ViewLocation.X, viewInfo.ViewLocation.Y, viewInfo.ViewLocation.Z));
			ImGui.EndMainMenuBar();
			#endregion
			CRenderer.Instance.ActiveScene.UpdateViewInfo(in viewInfo);

			for (int i = 0; i < m_pendingDestroyEntities.Count; i++)
			{
				InternalDestroyEntity(m_pendingDestroyEntities[i]);
			}
			m_pendingDestroyEntities.Clear();
		}

		public void MarkEntityForDestruction(CEntity entity)
		{
			m_pendingDestroyEntities.Add(entity);
		}

		public T CreateObject<T>(object userData) where T : CWorldObject, new()
		{
			T newObject = new T();
			newObject.Init(this, userData);
			return newObject;
		}

		public T SpawnEntity<T>(CEntitySpawnParameters parameters = null) where T : CEntity, new()
		{
			if (LoadedLevel != null)
			{
				if (parameters == null)
				{
					parameters = new CEntitySpawnParameters();
				}

				T newEntity = new T();
				newEntity.Init(this, parameters.userData);

				newEntity.Id = ++m_entityIdCounter;
				newEntity.Name = typeof(T).Name.Substring(1) + m_entityIdCounter.ToString();

				m_entityIdMap.Add(m_entityIdCounter, newEntity);

				LoadedLevel.AddEntity(newEntity);

				newEntity.SetWorldPosition(parameters.position);
				newEntity.SetWorldRotation(parameters.rotation);
				newEntity.SetWorldScale(parameters.scale);

				OnEntitySpawned?.Invoke(newEntity);

				return newEntity;
			}

			return default;
		}

		internal void InitWithWorld(CEntity loadedEntity, bool bAddToLevel = true)
		{
			System.Diagnostics.Debug.Assert(loadedEntity != null);
			if (LoadedLevel != null)
			{
				loadedEntity.Init(this, null);
				loadedEntity.Id = ++m_entityIdCounter;
				m_entityIdMap.Add(loadedEntity.Id, loadedEntity);
				if (bAddToLevel)
				{
					LoadedLevel.AddEntity(loadedEntity);
					OnEntitySpawned?.Invoke(loadedEntity);
				}
			}
		}

		public void DestroyEntity(CEntity entity, bool bRemoveFromLevel = true)
		{
			if (entity != null && LoadedLevel != null)
			{
				entity.Shutdown();
				if (bRemoveFromLevel)
				{
					LoadedLevel.RemoveEntity(entity);
					OnEntityDestroyed?.Invoke(entity);
				}

				m_entityIdMap.Remove(entity.Id);
			}
		}

		public CEntity GetEntityById(int id)
		{
			CEntity entity = null;
			if (m_entityIdMap.TryGetValue(id, out entity))
			{
				return entity;
			}

			return null;
		}

		public void CreateLevel()
		{
			if (LoadedLevel != null)
			{
				LoadedLevel.Shutdown();
			}

			LoadedLevel = new CLevel();
			LoadedLevel.Init(this, null);

			CEntitySpawnParameters spawnParams = new CEntitySpawnParameters();
			spawnParams.position = new Vector3(0, 0, -5);
			SpawnEntity<CDebugCameraEntity>(spawnParams);

			OnLevelChanged?.Invoke(null, LoadedLevel);
		}

		public void ChangeLevel(CAssetReference<CLevelAsset> levelAsset, CLevel newLevel)
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			LoadedLevel.Shutdown();
			timer.Stop();
			LogUtility.Log("Level Shutdown took {0} ms", timer.Elapsed.TotalMilliseconds);
			timer.Restart();
			LoadedLevel = newLevel;
			LoadedLevel.Init(this, null);
			timer.Stop();
			LogUtility.Log("Level Init took {0} ms", timer.Elapsed.TotalMilliseconds);
			OnLevelChanged?.Invoke(levelAsset, LoadedLevel);
		}

		private void InternalDestroyEntity(CEntity entity)
		{
			void RemoveEntityFromLevel(CEntity target)
			{
				m_entityIdMap.Remove(target.Id);
				LoadedLevel.RemoveEntity(target);
				target.Shutdown();

				var children = target.Children;
				for (int i = 0, count = children.Count; i < count; i++)
				{
					RemoveEntityFromLevel(children[i]);
				}
				OnEntityDestroyed?.Invoke(entity);
			}

			RemoveEntityFromLevel(entity);
		}

		/// <summary>
		/// Used for already destroyed entities that are referenced from outside. (Used for editor undo feature)
		/// </summary>
		/// <param name="entity">The dead entity that needs to be rehooked into this world</param>
		public void ReviveEntity(CEntity entity)
		{
			if (entity.IsAlive)
				return;

			if (m_entityIdMap.ContainsKey(entity.Id))
				return;

			m_entityIdMap.Add(entity.Id, entity);
			LoadedLevel.AddEntity(entity);
			entity.Init(this, null);

			OnEntityRevived?.Invoke(entity);
		}

		public event Action<CEntity> OnEntitySpawned;
		public event Action<CEntity> OnEntityDestroyed;
		public event Action<CEntity> OnEntityRevived;
		public event Action<CSceneComponent, CSceneComponent, CSceneComponent> OnHierarchyChanged;
		public event Action<CAssetReference<CLevelAsset>, CLevel> OnLevelChanged;

		public CLevel LoadedLevel { get; private set; }
		public CUpdateScheduler UpdateScheduler { get; private set; }
		public CViewManager ViewManager { get; private set; } = new CViewManager();
		public GameConsole GameConsole { get; private set; } = new GameConsole();
		public CPhysicsWorld PhysicsWorld { get; private set; } = new CPhysicsWorld();
		public bool IsPlaying { get; private set; } = false;

		private CEntity m_sponzaEntity;
		private CEntity m_cubeEntity;
		private bool m_bShowObjectAxis;

		private Dictionary<int, CEntity> m_entityIdMap = new Dictionary<int, CEntity>();
		private List<CEntity> m_pendingDestroyEntities = new List<CEntity>(16);
		private int m_entityIdCounter;

		private CEntity m_lightEntity;

		private readonly CPerformanceCounter m_performanceCounter = new CPerformanceCounter();
	}
}
