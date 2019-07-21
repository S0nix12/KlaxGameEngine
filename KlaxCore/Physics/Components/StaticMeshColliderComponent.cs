using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes;
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
	public class CStaticMeshColliderComponent : CColliderComponent
	{
		public override void Init()
		{
			base.Init();
			if (MeshAsset == null || MeshAsset.GetAsset() == null || !MeshAsset.GetAsset().IsLoaded)
			{
				m_physicsMesh = new InstancedMesh(PhysicsStatics.DummyInstancedMesh);
				m_physicsMesh.Sidedness = TriangleSidedness.Clockwise;
			}
			InvalidateCollider();
			m_transform.OnPositionChanged += OnPositionChanged;
			m_transform.OnRotationChanged += OnRotationChanged;
		}

		public override void Shutdown()
		{
			base.Shutdown();
			m_transform.OnPositionChanged -= OnPositionChanged;
			m_transform.OnRotationChanged -= OnRotationChanged;
			m_loadWaitUpdateScope?.Disconnect();
			m_loadWaitUpdateScope = null;
		}

		public override CollisionShape GetShape()
		{
			return m_physicsMesh.Shape;
		}

		public override CollisionShape GetShapeWithScale()
		{
			return m_physicsMesh.Shape;
		}

		public override void GetStaticCollidablesWithScale(List<Collidable> outCollidables)
		{
			UpdateStaticCollider();
			outCollidables.Add(m_physicsMesh);
		}

		public override void DrawCollider()
		{
			throw new NotImplementedException();
		}

		protected void WaitMeshLoadUpdate(float deltaTime)
		{
			if (MeshAsset != null && MeshAsset.GetAsset().IsLoaded)
			{
				SetPhysicsFromLoadedMesh();
				m_loadWaitUpdateScope.Disconnect();
				m_loadWaitUpdateScope = null;
			}
		}

		protected override void InvalidateCollider()
		{
			if (MeshAsset != null && MeshAsset.GetAsset() != null)
			{
				if (MeshAsset.GetAsset().IsLoaded)
				{
					SetPhysicsFromLoadedMesh();
				}
				else if(m_loadWaitUpdateScope == null)
				{
					m_loadWaitUpdateScope = World.UpdateScheduler.Connect(WaitMeshLoadUpdate, EUpdatePriority.Earliest);
				}
			}
			else
			{
				m_physicsMesh.Shape = PhysicsStatics.DummyInstancedMesh;

			}
		}

		protected void SetPhysicsFromLoadedMesh()
		{
			if (m_physicsMesh != null)
			{
				m_physicsMesh.Shape = m_meshAssetReference.GetAsset().GetPhysicsInstancedMesh();
				m_physicsMesh.Sidedness = TriangleSidedness.Clockwise;
			}
			else
			{
				m_physicsMesh = new InstancedMesh(m_meshAssetReference.GetAsset().GetPhysicsInstancedMesh());
			}
			m_physicsMesh.WorldTransform = new AffineTransform(WorldScale.ToBepu(), WorldRotation.ToBepu(), WorldPosition.ToBepu());
		}

		protected override void OnPositionChanged(Vector3 position)
		{
			if (m_physicsMesh != null)
			{
				Owner.MarkStaticColliderDirty();
			}
		}

		protected override void OnRotationChanged(Quaternion rotation)
		{
			if (m_physicsMesh != null)
			{
				Owner.MarkStaticColliderDirty();
			}
		}

		protected override void OnScaleChanged(Vector3 scale)
		{
			if (m_physicsMesh != null)
			{
				Owner.MarkStaticColliderDirty();
			}
		}

		public override void UpdateStaticCollider()
		{
			if (m_physicsMesh != null)
			{
				AffineTransform transform = new AffineTransform(WorldScale.ToBepu(), WorldRotation.ToBepu(), WorldPosition.ToBepu());
				m_physicsMesh.WorldTransform = transform;
			}
		}

		private InstancedMesh m_physicsMesh;
		public InstancedMesh PhysicsMesh
		{
			get { return m_physicsMesh; }
		}

		[JsonProperty]
		private CAssetReference<CMeshAsset> m_meshAssetReference;
		[JsonIgnore]
		[KlaxProperty(Category = "Collision")]
		public CAssetReference<CMeshAsset> MeshAsset
		{
			get { return m_meshAssetReference; }
			set
			{
				if (m_meshAssetReference != value)
				{
					m_meshAssetReference = value;
					InvalidateCollider();
				}
			}
		}

		private CUpdateScope m_loadWaitUpdateScope;
	}
}
