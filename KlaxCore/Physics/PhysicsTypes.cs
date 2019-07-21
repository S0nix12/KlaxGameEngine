using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionTests;
using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using KlaxCore.GameFramework;
using KlaxMath;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.Physics
{
	[KlaxScriptType]
	public struct SCollisionEventData
	{
		public SCollisionEventData(CEntity a, CEntity b, Entity physA, Entity physB, Contact contact)
		{
			EntityA = a;
			EntityB = b;

			Location = contact.Position.ToSharp();
			Normal = contact.Normal.ToSharp();

			VelocityA = physA?.LinearVelocity.ToSharp() ?? Vector3.Zero;
			VelocityB = physB?.LinearVelocity.ToSharp() ?? Vector3.Zero;
		}

		/// <summary>
		/// Entity that send the collision event
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public CEntity EntityA;
		/// <summary>
		/// Entity that collided with the sending entity
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public CEntity EntityB;

		/// <summary>
		/// Location of the contact that started the collision
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public Vector3 Location;
		/// <summary>
		/// Contact normal at the collision location
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public Vector3 Normal;
		/// <summary>
		/// Velocity of EntityA before the collision (Zero if static)
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public Vector3 VelocityA;
		/// <summary>
		/// Velocity of EntityB before the collision (Zero if static)
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public Vector3 VelocityB;
	}

	[KlaxScriptType]
	public struct SRaycastResult
	{
		/// <summary>
		/// Entity that was hit
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public CEntity HitEntity;
		/// <summary>
		/// Location the hit occured
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public Vector3 Location;
		/// <summary>
		/// Normal at the hit location
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public Vector3 Normal;
		/// <summary>
		/// Distance from the start of the raycast the hit occured
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public float Distance;
		/// <summary>
		/// True if the raycast stopped at this hit, false if it overlapped but continued
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public bool bIsSolidHit;
	}

	internal class SimpleCollisionRulesOwner : ICollisionRulesOwner
	{
		public SimpleCollisionRulesOwner(CollisionRules rules)
		{
			CollisionRules = rules;
		}
		public CollisionRules CollisionRules { get; set; }
	}
}
