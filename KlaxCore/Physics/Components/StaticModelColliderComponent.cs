using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes;
using BEPUphysics.DataStructures;
using BEPUutilities;
using KlaxCore.GameFramework;
using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using Quaternion = SharpDX.Quaternion;
using Vector3 = SharpDX.Vector3;

namespace KlaxCore.Physics.Components
{
	[KlaxComponent(Category = "Physics")]
	public class CStaticModelColliderComponent : CColliderComponent
	{
		protected class CStaticModelColliderEntry
		{
			public CStaticModelColliderEntry(InstancedMesh mesh, in Vector3 pos, in Vector3 scale, in Quaternion rot)
			{				
				PhysicsMesh = mesh;
				LocalPosition = pos;
				LocalScale = scale;
				LocalRotation = rot;
			}

			public void UpdateParentTransform(in Vector3 parentPos, in Quaternion parentRot, in Vector3 parentScale)
			{
				Vector3 meshPosition = Vector3.Transform(parentScale * LocalPosition, LocalRotation) + parentPos;
				Quaternion meshRotation = parentRot * LocalRotation;
				Vector3 meshScale = parentScale * LocalScale;

				AffineTransform transform = new AffineTransform(meshScale.ToBepu(), meshRotation.ToBepu(), meshPosition.ToBepu());
				PhysicsMesh.WorldTransform = transform;
			}
			
			public InstancedMesh PhysicsMesh;
			public Vector3 LocalPosition;
			public Vector3 LocalScale;
			public Quaternion LocalRotation;
		}

		public override void Init()
		{
			base.Init();
			InvalidateCollider();
			m_transform.OnPositionChanged += OnPositionChanged;
			m_transform.OnRotationChanged += OnRotationChanged;
		}

		public override void Shutdown()
		{
			base.Shutdown();
			m_transform.OnPositionChanged -= OnPositionChanged;
			m_transform.OnRotationChanged -= OnRotationChanged;

			if (m_loadWaitUpdateScope != null && m_loadWaitUpdateScope.IsConnected())
			{
				m_loadWaitUpdateScope.Disconnect();
				m_loadWaitUpdateScope = null;
			}

			m_colliderEntries = null;
		}

		public override CollisionShape GetShape()
		{
			return null;
		}

		public override CollisionShape GetShapeWithScale()
		{
			return null;
		}

		public override void GetStaticCollidablesWithScale(List<Collidable> outCollidables)
		{
			if (m_colliderEntries == null)
			{
				return;
			}

			Vector3 worldPos = WorldPosition;
			Vector3 worldScale = WorldScale;
			Quaternion worldRot = WorldRotation;
			foreach (CStaticModelColliderEntry colliderEntry in m_colliderEntries)
			{
				if (colliderEntry != null)
				{
					colliderEntry.UpdateParentTransform(in worldPos, in worldRot, in worldScale);
					outCollidables.Add(colliderEntry.PhysicsMesh);
				}
			}
		}

		public override void DrawCollider()
		{
			throw new NotImplementedException();
		}

		protected void WaitMeshLoadUpdate(float deltaTime)
		{
			if (SetPhysicsFromLoadedMesh())
			{
				IsFullyLoaded = true;
				m_loadWaitUpdateScope.Disconnect();
				m_loadWaitUpdateScope = null;
			}
		}

		protected override void InvalidateCollider()
		{			
			if (ModelAsset != null && ModelAsset.GetAsset() != null)
			{
				if (ModelAsset.GetAsset().IsLoaded)
				{
					if (!SetPhysicsFromLoadedMesh())
					{
						m_loadWaitUpdateScope = World.UpdateScheduler.Connect(WaitMeshLoadUpdate, EUpdatePriority.ResourceLoading);
					}
				}
				else if (m_loadWaitUpdateScope == null)
				{
					m_loadWaitUpdateScope = World.UpdateScheduler.Connect(WaitMeshLoadUpdate, EUpdatePriority.ResourceLoading);
				}
			}
			else
			{
				Owner.MarkPhysicsDirty();
			}
		}

		protected bool SetPhysicsFromLoadedMesh()
		{
			if (m_modelAssetReference != null && m_modelAssetReference.GetAsset().IsLoaded)
			{
				CModelAsset model = m_modelAssetReference.GetAsset();
				if (m_colliderEntries == null)
				{
					m_colliderEntries = new CStaticModelColliderEntry[model.MeshChildren.Count];
					Owner.MarkPhysicsDirty();
				}
				return AddModelMeshColliders(model);
			}

			return false;
		}

		private bool AddModelMeshColliders(CModelAsset modelAsset)
		{
			System.Diagnostics.Debug.Assert(modelAsset.IsLoaded);
			Vector3 worldPos = WorldPosition;
			Quaternion worldRot = WorldRotation;
			Vector3 worldScale = WorldScale;

			bool bFullyLoaded = true;
			for (var i = 0; i < modelAsset.MeshChildren.Count; i++)
			{
				if (m_colliderEntries[i] != null)
				{
					continue;
				}

				CMeshAsset mesh = modelAsset.MeshChildren[i].meshAsset.GetAsset();
				if (mesh.IsLoaded)
				{
					modelAsset.MeshChildren[i].relativeTransform.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
					Vector3 meshPosition = Vector3.Transform(worldScale * translation, rotation) + worldPos;
					Quaternion meshRotation = worldRot * rotation;
					Vector3 meshScale = worldScale * scale;

					AffineTransform transform = new AffineTransform(meshScale.ToBepu(), meshRotation.ToBepu(), meshPosition.ToBepu());
					InstancedMesh physicsMesh = new InstancedMesh(mesh.GetPhysicsInstancedMesh(), transform);
					physicsMesh.Sidedness = TriangleSidedness.Clockwise;
					m_colliderEntries[i] = new CStaticModelColliderEntry(physicsMesh, in translation, in scale, in rotation);					
					Owner.RecreatePhysics();
					Owner.MarkPhysicsDirty();
				}
				else
				{
					bFullyLoaded = false;
				}
			}

			return bFullyLoaded;
		}

		protected override void OnPositionChanged(SharpDX.Vector3 position)
		{
			if (m_colliderEntries != null)
			{
				Owner.MarkStaticColliderDirty();
			}
		}

		protected override void OnRotationChanged(Quaternion rotation)
		{
			if (m_colliderEntries != null)
			{
				Owner.MarkStaticColliderDirty();
			}
		}

		protected override void OnScaleChanged(Vector3 scale)
		{
			if (m_colliderEntries != null)
			{
				Owner.MarkStaticColliderDirty();
			}
		}

		public override void UpdateStaticCollider()
		{
			if (m_colliderEntries != null)
			{
				Vector3 worldPosition = WorldPosition;
				Vector3 worldScale = WorldScale;
				Quaternion worldRotation = WorldRotation;
				foreach (CStaticModelColliderEntry colliderEntry in m_colliderEntries)
				{
					colliderEntry?.UpdateParentTransform(in worldPosition, in worldRotation, in worldScale);
				}
			}
		}

		private CStaticModelColliderEntry[] m_colliderEntries;


		public bool IsFullyLoaded { get; private set; }

		[JsonProperty]
		private CAssetReference<CModelAsset> m_modelAssetReference;
		[JsonIgnore]
		[KlaxProperty(Category = "Collision")]
		public CAssetReference<CModelAsset> ModelAsset
		{
			get { return m_modelAssetReference; }
			set
			{
				if (m_modelAssetReference != value)
				{
					m_modelAssetReference = value;
					m_colliderEntries = null;
					InvalidateCollider();
				}
			}
		}

		private CUpdateScope m_loadWaitUpdateScope;
	}
}
