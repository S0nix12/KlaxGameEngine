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

namespace KlaxCore.Physics.Components
{
	[KlaxComponent(Category = "Physics")]
	class CSphereColliderComponent : CColliderComponent
	{
		public override void Init()
		{
			base.Init();
			float scale = WorldScale.MaxComponent();
			m_scaledShape = new SphereShape(Radius * Math.Abs(scale));
			m_scaledStaticCollidable = m_scaledShape.GetCollidableInstance();
		}

		public override CollisionShape GetShape()
		{
			return null;
		}

		public override CollisionShape GetShapeWithScale()
		{
			UpdateScaledShape();
			return Math.Abs(m_radius) > float.Epsilon ? m_scaledShape : null;
		}

		public override void GetStaticCollidablesWithScale(List<Collidable> outCollidables)
		{
			UpdateScaledShape();
			m_scaledStaticCollidable.WorldTransform = new RigidTransform(WorldPosition.ToBepu(), WorldRotation.ToBepu());
			outCollidables.Add(m_scaledStaticCollidable);
		}

		public override void DrawCollider()
		{
			float radiusScale = WorldScale.MaxComponent();
			CRenderer.Instance.ActiveScene.DebugRenderer.DrawSphere(WorldPosition, radiusScale * Radius, Color.Green.ToColor4(), 0.0f, EDebugDrawCommandFlags.Wireframe);
		}

		protected override void InvalidateCollider()
		{
			UpdateScaledShape();
		}

		private void UpdateScaledShape()
		{
			m_scaledShape.Radius = Math.Abs(WorldScale.MaxComponent()) * Radius;
			Owner.MarkStaticColliderDirty();
		}

		public override void UpdateStaticCollider()
		{
			RigidTransform shapeTransform = new RigidTransform(WorldPosition.ToBepu(), WorldRotation.ToBepu());
			m_scaledStaticCollidable.UpdateBoundingBoxForTransform(ref shapeTransform);
		}

		private EntityCollidable m_scaledStaticCollidable;
		private SphereShape m_scaledShape;

		[JsonProperty]
		private float m_radius = 0.5f;
		[KlaxProperty(Category = "Collision")]
		[JsonIgnore]
		public float Radius
		{
			get { return m_radius; }
			set { m_radius = value; InvalidateCollider(); }
		}
	}
}
