using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes;
using KlaxCore.GameFramework;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.Physics.Components
{
	[KlaxComponent(HideInEditor = true, Category = "Physics")]
	public abstract class CColliderComponent : CSceneComponent
	{
		public override void Init()
		{
			base.Init();
			m_transform.OnScaleChanged += OnScaleChanged;
			Owner.OnPhysicsStateChanged += OnOwnerPhysicsChanged;
			Owner.OnPhysicsStaticStateChanged += OnOwnerPhysicsStaticChanged;		
			OnOwnerPhysicsChanged(Owner.IsPhysicsEnabled);
		}

		public override void Shutdown()
		{
			base.Shutdown();
			m_transform.OnScaleChanged -= OnScaleChanged;
			m_transform.OnPositionChanged -= OnPositionChanged;
			m_transform.OnRotationChanged -= OnRotationChanged;
			Owner.OnPhysicsStateChanged -= OnOwnerPhysicsChanged;
			Owner.OnPhysicsStaticStateChanged -= OnOwnerPhysicsStaticChanged;
		}

		public abstract CollisionShape GetShape();
		[KlaxFunction(Category = "Physics", Tooltip = "Get the collision shape this component represents")]
		public abstract CollisionShape GetShapeWithScale();

		/// <summary>
		/// Get all collidables which define this collider component
		/// </summary>
		/// <param name="outCollidables">Collidables will be added to the list, the list is not cleared before adding</param>
		public abstract void GetStaticCollidablesWithScale(List<Collidable> outCollidables);
		public abstract void DrawCollider();

		protected abstract void InvalidateCollider();

		protected virtual void OnScaleChanged(Vector3 scale)
		{
			InvalidateCollider();
		}

		protected virtual void OnPositionChanged(Vector3 position)
		{
			InvalidateCollider();
		}

		protected virtual void OnRotationChanged(Quaternion rotation)
		{
			InvalidateCollider();
		}

		public virtual void UpdateStaticCollider()
		{ }

		protected virtual void OnOwnerPhysicsChanged(bool bEnabled)
		{
			if (Owner.IsPhysicsStatic)
			{
				if (bEnabled)
				{
					m_transform.OnPositionChanged += OnPositionChanged;
					m_transform.OnRotationChanged += OnRotationChanged;
				}
				else
				{
					m_transform.OnPositionChanged -= OnPositionChanged;
					m_transform.OnRotationChanged -= OnRotationChanged;
				}
			}
		}

		protected virtual void OnOwnerPhysicsStaticChanged(bool bIsStatic)
		{
			if (Owner.IsPhysicsEnabled)
			{
				if (bIsStatic)
				{
					m_transform.OnPositionChanged += OnPositionChanged;
					m_transform.OnRotationChanged += OnRotationChanged;
				}
				else
				{
					m_transform.OnPositionChanged -= OnPositionChanged;
					m_transform.OnRotationChanged -= OnRotationChanged;
				}
			}
		}
	}
}
