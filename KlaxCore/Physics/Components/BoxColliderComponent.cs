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
	[KlaxComponent(Category = "Physics")]
	class CBoxColliderComponent : CColliderComponent
	{
		public override void Init()
		{
			base.Init();
			Vector3 worldScale = WorldScale;
			m_scaledShape = new BoxShape(m_width * worldScale.X, m_height * worldScale.Y, m_length * worldScale.Z);
			m_scaledStaticCollidable = m_scaledShape.GetCollidableInstance();
			Owner.MarkPhysicsDirty();
		}

		public override CollisionShape GetShape()
		{
			return null;
		}

		public override CollisionShape GetShapeWithScale()
		{
			UpdateScaledShape();
			return m_scaledShape;
		}

		public override void GetStaticCollidablesWithScale(List<Collidable> outCollidables)
		{
			UpdateScaledShape();
			m_scaledStaticCollidable.WorldTransform = new RigidTransform(WorldPosition.ToBepu(), WorldRotation.ToBepu());
			outCollidables.Add(m_scaledStaticCollidable);
		}

		public override void DrawCollider()
		{
			Vector3 boxExtent = new Vector3(Width * WorldScale.X, Height * WorldScale.Y, Length * WorldScale.Z);
			CRenderer.Instance.ActiveScene.DebugRenderer.DrawBox(WorldPosition, WorldRotation, boxExtent, Color.Green.ToColor4(), 0.0f, EDebugDrawCommandFlags.Wireframe);
		}

		protected override void InvalidateCollider()
		{
			UpdateScaledShape();
		}

		private void UpdateScaledShape()
		{
			Vector3 worldScale = WorldScale;
			m_scaledShape.Width = Width * Math.Abs(worldScale.X);
			m_scaledShape.Height = Height * Math.Abs(worldScale.Y);
			m_scaledShape.Length = Length * Math.Abs(worldScale.Z);
			Owner.MarkStaticColliderDirty();
		}

		public override void UpdateStaticCollider()
		{
			RigidTransform shapeTransform = new RigidTransform(WorldPosition.ToBepu(), WorldRotation.ToBepu());
			m_scaledStaticCollidable.UpdateBoundingBoxForTransform(ref shapeTransform);
		}

		private EntityCollidable m_scaledStaticCollidable;
		private BoxShape m_scaledShape;

		[JsonProperty]
		private float m_width = 1;
		[JsonIgnore]
		[KlaxProperty(Category = "Collision")]
		public float Width
		{
			get { return m_width; }
			set { m_width = value; InvalidateCollider(); }
		}

		[JsonProperty]
		private float m_height = 1;
		[JsonIgnore]
		[KlaxProperty(Category = "Collision")]
		public float Height
		{
			get { return m_height; }
			set { m_height = value; InvalidateCollider(); }
		}

		[JsonProperty]
		private float m_length = 1;
		[JsonIgnore]
		[KlaxProperty(Category = "Collision")]
		public float Length
		{
			get { return m_length; }
			set { m_length = value; InvalidateCollider(); }
		}
	}
}
