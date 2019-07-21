using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxCore.Physics;
using KlaxMath;
using KlaxShared.Attributes;
using SharpDX;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class PhysicsScriptFunctionLibrary
	{
		[KlaxFunction(Category = "Physics", Tooltip = "Applies an impulse to the given entity at its current world position")]
		public static void ApplyImpulse(CEntity target, Vector3 impulse)
		{
			if (target == null)
			{
				LogUtility.Log("Could not apply impulse target was null");
				return;
			}

			if (target.PhysicalEntity == null)
			{
				LogUtility.Log("Could not apply impulse target does not have dynamic physics");
				return;
			}

			target.PhysicalEntity.ApplyImpulse(target.GetWorldPosition().ToBepu(), impulse.ToBepu());
		}

		[KlaxFunction(Category = "Physics", Tooltip = "Applies an impulse to the given entity at the given position")]
		public static void ApplyImpulseAtLocation(CEntity target, Vector3 position, Vector3 impulse)
		{
			if (target == null)
			{
				LogUtility.Log("Could not apply impulse target was null");
				return;
			}

			if (target.PhysicalEntity == null)
			{
				LogUtility.Log("Could not apply impulse target does not have dynamic physics");
				return;
			}

			target.PhysicalEntity.ApplyImpulse(position.ToBepu(), impulse.ToBepu());
		}

		[KlaxFunction(Category = "Physics", Tooltip = "Sets the velocity of the given entity")]
		public static void SetVelocity(CEntity target, Vector3 velocity)
		{
			if (target == null)
			{
				LogUtility.Log("Could not set velocity target was null");
				return;
			}

			if (target.PhysicalEntity == null)
			{
				LogUtility.Log("Could not set velocity target does not have dynamic physics");
				return;
			}

			target.PhysicalEntity.LinearVelocity = velocity.ToBepu();
		}

		[KlaxFunction(Category = "Physics", Tooltip = "Casts a ray against the physics world returning the first hit")]
		public static bool Raycast(Vector3 start, Vector3 direction, float length, out SRaycastResult hitResult)
		{
			hitResult = new SRaycastResult();
			direction.Normalize();

			Ray ray = new Ray(start, direction);

			return CEngine.Instance.CurrentWorld.PhysicsWorld.Raycast(ref ray, length, ref hitResult);
		}
	}
}
