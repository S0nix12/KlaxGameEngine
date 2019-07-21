using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using KlaxMath;
using KlaxRenderer;
using KlaxRenderer.Debug;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using SharpDX;
using Vector3 = SharpDX.Vector3;

namespace KlaxCore.Physics.Components
{
	/// <summary>
	/// Note the cone collider is not supported currently, it has some internal offset to compensate it's center of mass, this would make it really hard to implement it in our default compound shapes
	/// If you use it expect it to be off from your render geometry, in theory the offset of the cone should be 1/4 of its height
	/// </summary>
	[KlaxComponent(Category = "Physics")]
	class CConeColliderComponent : CColliderComponent
	{
		public override void Init()
		{
			base.Init();
			Vector3 worldScale = WorldScale;
			m_scaledShape = new ConeShape(worldScale.Y * Length, Math.Abs(Math.Max(worldScale.X, worldScale.Z)) * Radius);
			m_scaledStaticCollidable = m_scaledShape.GetCollidableInstance();
		}

		public override CollisionShape GetShape()
		{
			return null;
		}

		public override CollisionShape GetShapeWithScale()
		{
			UpdateScaledShape();
			return Math.Abs(m_radius) > float.Epsilon && Math.Abs(m_length) > float.Epsilon ? m_scaledShape : null;
		}

		public override void GetStaticCollidablesWithScale(List<Collidable> outCollidables)
		{
			UpdateScaledShape();
			m_scaledStaticCollidable.WorldTransform = new RigidTransform(WorldPosition.ToBepu(), WorldRotation.ToBepu());
			outCollidables.Add(m_scaledStaticCollidable);
		}

		public override void DrawCollider()
		{
			float height = Length * Math.Abs(WorldScale.Y);
			float radius = Radius * Math.Abs(Math.Max(WorldScale.X, WorldScale.Z));
			CRenderer.Instance.ActiveScene.DebugRenderer.DrawCone(WorldPosition, height, radius, WorldRotation, Color.Green.ToColor4(), 0.0f, EDebugDrawCommandFlags.Wireframe);
		}

		protected override void InvalidateCollider()
		{
			UpdateScaledShape();
		}

		private void UpdateScaledShape()
		{
			Vector3 worldScale = WorldScale;
			m_scaledShape.Radius = Radius * Math.Abs(Math.Max(worldScale.X, worldScale.Z));
			m_scaledShape.Height = Length * Math.Abs(worldScale.Y);
			Owner.MarkStaticColliderDirty();
		}

		public override void UpdateStaticCollider()
		{
			RigidTransform shapeTransform = new RigidTransform(WorldPosition.ToBepu(), WorldRotation.ToBepu());
			m_scaledStaticCollidable.UpdateBoundingBoxForTransform(ref shapeTransform);
		}

		private EntityCollidable m_scaledStaticCollidable;
		private ConeShape m_scaledShape;

		[JsonProperty]
		private float m_radius = 0.5f;
		[KlaxProperty(Category = "Collision")]
		[JsonIgnore]
		public float Radius
		{
			get { return m_radius; }
			set { m_radius = value; InvalidateCollider(); }
		}

		[JsonProperty]
		private float m_length = 1.0f;
		[KlaxProperty(Category = "Collision")]
		[JsonIgnore]
		public float Length
		{
			get { return m_length; }
			set { m_length = value; InvalidateCollider(); }
		}
	}
}
