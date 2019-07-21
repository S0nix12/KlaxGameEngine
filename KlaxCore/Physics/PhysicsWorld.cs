using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUutilities.Threading;
using KlaxCore.Core;
using KlaxCore.Core.View;
using KlaxCore.GameFramework;
using KlaxIO.Input;
using KlaxMath;
using KlaxRenderer;
using KlaxRenderer.Debug;
using KlaxRenderer.Scene;
using KlaxShared.Attributes;
using SharpDX;
using BoundingSphere = BEPUutilities.BoundingSphere;
using Matrix = BEPUutilities.Matrix;
using Quaternion = SharpDX.Quaternion;
using Ray = SharpDX.Ray;
using Vector3 = BEPUutilities.Vector3;

namespace KlaxCore.Physics
{
	public class CPhysicsWorld
	{
		[CVar]
		public static int EnablePhysicsUpdate { get; set; } = 1;

		[CVar]
		private static float PhysicsTimeStepDuration { get; set; } = 1f / 60f;

		public void Init(CWorld gameWorld)
		{
			m_gameWorld = gameWorld;

			for (int i = 0; i < System.Environment.ProcessorCount; i++)
			{
				m_looper.AddThread();
			}
			m_physicSpace = new Space(m_looper);
			m_physicSpace.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
			m_physicSpace.TimeStepSettings.TimeStepDuration = PhysicsTimeStepDuration;
			Input.RegisterListener(OnInputEvent);
		}

		private void OnInputEvent(ReadOnlyCollection<SInputButtonEvent> buttonevents, string textinput)
		{
			foreach (var buttonEvent in buttonevents)
			{
				if (buttonEvent.button == EInputButton.MouseMiddleButton && buttonEvent.buttonEvent == EButtonEvent.Pressed)
				{
					CViewManager viewManager = m_gameWorld.ViewManager;
					int mouseAbsX = System.Windows.Forms.Cursor.Position.X - (int)viewManager.ScreenLeft;
					int mouseAbsY = System.Windows.Forms.Cursor.Position.Y - (int)viewManager.ScreenTop;

					if (mouseAbsX < 0 || mouseAbsY < 0)
					{
						return;
					}

					viewManager.GetViewInfo(out SSceneViewInfo viewInfo);
					Ray pickRay = Ray.GetPickRay(mouseAbsX, mouseAbsY, new ViewportF(0, 0, viewManager.ScreenWidth, viewManager.ScreenHeight), viewInfo.ViewMatrix * viewInfo.ProjectionMatrix);
					if (m_physicSpace.RayCast(pickRay.ToBepu(), 9999.0f, out RayCastResult result))
					{
						float fieldRadius = 5.0f;
						float impulseStrength = 20.0f;
						List<BroadPhaseEntry> queryResults = new List<BroadPhaseEntry>();
						BoundingSphere bs = new BoundingSphere(result.HitData.Location, fieldRadius);
						m_physicSpace.BroadPhase.QueryAccelerator.GetEntries(bs, queryResults);
						foreach (var entry in queryResults)
						{
							var entityCollision = entry as EntityCollidable;
							if (entityCollision != null)
							{
								var e = entityCollision.Entity;
								if (e.IsDynamic)
								{
									Vector3 toEntity = e.Position - result.HitData.Location;
									float length = toEntity.Length();
									float strength = impulseStrength / length;
									toEntity.Y = 1.0f;
									toEntity.Normalize();
									e.ApplyImpulse(e.Position, toEntity * strength);
								}
							}
						}
					}
				}
			}
		}

		public void Update(float deltaTime)
		{
			if (EnablePhysicsUpdate > 0)
			{
				if (deltaTime > m_physicSpace.TimeStepSettings.TimeStepDuration)
				{
					m_physicSpace.Update();
				}
				else
				{
					m_physicSpace.Update(deltaTime);
				}
			}
		}

		public void AddPhysicsObject(ISpaceObject physicsObject)
		{
			m_physicSpace.Add(physicsObject);
		}

		public void RemovePhysicsObject(ISpaceObject physicsObject)
		{
			m_physicSpace.Remove(physicsObject);
		}

		#region Casts&Sweeps
		/// <summary>
		/// Returns the first hit of a given ray tested against the physics world, does not filter anything even objects which are not collidable will be hit if they exist in the physics world
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="outHitResult">The hit that was found if any</param>
		/// <returns>True if any object was hit</returns>
		public bool Raycast(ref Ray ray, float length, ref SRaycastResult outHitResult)
		{
			if (m_physicSpace.RayCast(ray.ToBepu(), length, out RayCastResult result))
			{
				if (result.HitObject.Tag is CEntity gameEntity)
				{
					outHitResult.HitEntity = gameEntity;
					outHitResult.Location = result.HitData.Location.ToSharp();
					outHitResult.Normal = result.HitData.Normal.ToSharp();
					outHitResult.Distance = result.HitData.T;
					return true;
				}

				return false;
			}

			return false;
		}

		/// <summary>
		/// Returns the first hit of a given ray tested against the physics world, does filtering based on the given CollisionRules only solid collisions will be reported
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="raycastRules">Collision Rules to use for the ray, anything above "Normal" is ignored</param>
		/// <param name="outHitResult">The hit that was found if any</param>
		/// <returns>True if any object was hit</returns>
		public bool Raycast(ref Ray ray, float length, CollisionRules raycastRules, ref SRaycastResult outHitResult)
		{
			SimpleCollisionRulesOwner rulesOwner = new SimpleCollisionRulesOwner(raycastRules);
			return Raycast(ref ray, length, GetSolidFilter(rulesOwner), ref outHitResult);
		}

		/// <summary>
		/// Returns the first hit of a given ray tested against the physics world, does filtering based on the given CollisionGroup only solid collisions will be reported
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="group">Collision Group to use for the ray, anything above "Normal" is ignored</param>
		/// <param name="outHitResult">The hit that was found if any</param>
		/// <returns>True if any object was hit</returns>
		public bool Raycast(ref Ray ray, float length, CollisionGroup group, ref SRaycastResult outHitResult)
		{
			return Raycast(ref ray, length, GetSolidFilter(group), ref outHitResult);
		}

		/// <summary>
		/// Returns the first hit of a given ray tested against the physics world, does filtering based on the given function
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="filter">Filter to check if an object is valid to hit</param>
		/// <param name="outHitResult">The hit that was found if any</param>
		/// <returns>True if any object was hit</returns>
		public bool Raycast(ref Ray ray, float length, Func<BroadPhaseEntry, bool> filter, ref SRaycastResult outHitResult)
		{
			if (m_physicSpace.RayCast(ray.ToBepu(), length, filter, out RayCastResult result))
			{
				if (result.HitObject.Tag is CEntity gameEntity)
				{
					outHitResult.HitEntity = gameEntity;
					outHitResult.Location = result.HitData.Location.ToSharp();
					outHitResult.Normal = result.HitData.Normal.ToSharp();
					outHitResult.Distance = result.HitData.T;
					return true;
				}

				return false;
			}

			return false;
		}

		/// <summary>
		/// Returns all overlapping objects of a given ray with the physics world, the ray does NOT stop at solid hit but will report all overlaps and solid hits on the ray
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="rayRules">Collision Rules to use for the ray anything above "NoSolver" is ignored</param>
		/// <param name="outResults">All overlaps on the ray</param>
		/// <returns>True if any object was hit</returns>
		public bool RayOverlap(ref Ray ray, float length, CollisionRules rayRules, List<SRaycastResult> outResults)
		{
			SimpleCollisionRulesOwner rulesOwner = new SimpleCollisionRulesOwner(rayRules);
			List<RayCastResult> physicsResults = new List<RayCastResult>();
			if (m_physicSpace.RayCast(ray.ToBepu(), length, GetOverlapFilter(rulesOwner), physicsResults))
			{
				foreach (RayCastResult physicsResult in physicsResults)
				{
					if (physicsResult.HitObject.Tag is CEntity gameEntity)
					{
						CollisionRule rule = CollisionRules.GetCollisionRule(physicsResult.HitObject, rulesOwner);
						SRaycastResult gameResult = new SRaycastResult()
						{
							HitEntity = gameEntity,
							Location = physicsResult.HitData.Location.ToSharp(),
							Normal = physicsResult.HitData.Normal.ToSharp(),
							Distance = physicsResult.HitData.T,
							bIsSolidHit = rule <= CollisionRule.Normal
						};
						outResults.Add(gameResult);
					}
				}
				return outResults.Count > 0;
			}

			return false;
		}

		/// <summary>
		/// Returns all overlapping objects of a given ray with the physics world, the ray does NOT stop at solid hit but will report all overlaps and solid hits on the ray
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="group">Collision Group to use for the ray anything above "NoSolver" is ignored</param>
		/// <param name="outResults">All overlaps on the ray</param>
		/// <returns>True if any object was hit</returns>
		public bool RayOverlap(ref Ray ray, float length, CollisionGroup group, List<SRaycastResult> outResults)
		{
			List<RayCastResult> physicsResults = new List<RayCastResult>();
			if (m_physicSpace.RayCast(ray.ToBepu(), length, GetOverlapFilter(group), physicsResults))
			{
				foreach (RayCastResult physicsResult in physicsResults)
				{
					if (physicsResult.HitObject.Tag is CEntity gameEntity)
					{
						CollisionRule rule = GetCollisionRuleWithGroup(physicsResult.HitObject, group);
						SRaycastResult gameResult = new SRaycastResult()
						{
							HitEntity = gameEntity,
							Location = physicsResult.HitData.Location.ToSharp(),
							Normal = physicsResult.HitData.Normal.ToSharp(),
							Distance = physicsResult.HitData.T,
							bIsSolidHit = rule <= CollisionRule.Normal
						};
						outResults.Add(gameResult);
					}
				}
				return outResults.Count > 0;
			}

			return false;
		}

		/// <summary>
		/// Returns all overlapping objects of a given ray with the physics world, as no rules are given to this function all hits will be reported as non solid
		/// </summary>
		/// <param name="ray">Ray to test against</param>
		/// <param name="length">Maximum distance to test</param>
		/// <param name="outResults">All overlaps on the ray</param>
		/// <returns>True if any object was hit</returns>
		public bool RayOverlap(ref Ray ray, float length, List<SRaycastResult> outResults)
		{
			List<RayCastResult> physicsResults = new List<RayCastResult>();
			if (m_physicSpace.RayCast(ray.ToBepu(), length, physicsResults))
			{
				foreach (RayCastResult physicsResult in physicsResults)
				{
					if (physicsResult.HitObject.Tag is CEntity gameEntity)
					{
						SRaycastResult gameResult = new SRaycastResult()
						{
							HitEntity = gameEntity,
							Location = physicsResult.HitData.Location.ToSharp(),
							Normal = physicsResult.HitData.Normal.ToSharp(),
							Distance = physicsResult.HitData.T,
							bIsSolidHit = false
						};
						outResults.Add(gameResult);
					}
				}
				return outResults.Count > 0;
			}

			return false;
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPosition">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweepDir">Direction in which to sweep</param>
		/// <param name="length">Length of the sweep</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool ConvexSweep(ConvexShape shape, SharpDX.Vector3 startPosition, SharpDX.Quaternion rotation, SharpDX.Vector3 sweepDir, float length, ref SRaycastResult outSweepResult)
		{
			SharpDX.Vector3 sweep = sweepDir * length;
			return ConvexSweep(shape, ref startPosition, ref rotation, ref sweep, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool ConvexSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, ref SRaycastResult outSweepResult)
		{
			RigidTransform startTransform = new RigidTransform(startPos.ToBepu(), rotation.ToBepu());
			Vector3 sweepVector = sweep.ToBepu();
			if (m_physicSpace.ConvexCast(shape, ref startTransform, ref sweepVector, out RayCastResult result))
			{
				if (result.HitObject.Tag is CEntity gameEntity)
				{
					outSweepResult.HitEntity = gameEntity;
					outSweepResult.Location = result.HitData.Location.ToSharp();
					outSweepResult.Normal = -result.HitData.Normal.ToSharp();
					outSweepResult.Distance = result.HitData.T;
					outSweepResult.bIsSolidHit = true;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="sweepRules">Collision rules to filter the sweep hits, anything above "Normal" is ignored</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool ConvexSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionRules sweepRules, ref SRaycastResult outSweepResult)
		{
			SimpleCollisionRulesOwner rulesOwner = new SimpleCollisionRulesOwner(sweepRules);
			return ConvexSweep(shape, ref startPos, ref rotation, ref sweep, GetSolidFilter(rulesOwner), ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="group">Collision group to filter the sweep hits, anything above "Normal" is ignored</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool ConvexSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionGroup group, ref SRaycastResult outSweepResult)
		{
			return ConvexSweep(shape, ref startPos, ref rotation, ref sweep, GetSolidFilter(group), ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="filter">Function to filter hits</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool ConvexSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, Func<BroadPhaseEntry, bool> filter, ref SRaycastResult outSweepResult)
		{
			RigidTransform startTransform = new RigidTransform(startPos.ToBepu(), rotation.ToBepu());
			Vector3 sweepVector = sweep.ToBepu();
			if (m_physicSpace.ConvexCast(shape, ref startTransform, ref sweepVector, filter, out RayCastResult result))
			{
				if (result.HitObject.Tag is CEntity gameEntity)
				{
					outSweepResult.HitEntity = gameEntity;
					outSweepResult.Location = result.HitData.Location.ToSharp();
					outSweepResult.Normal = -result.HitData.Normal.ToSharp();
					outSweepResult.Distance = result.HitData.T;
					outSweepResult.bIsSolidHit = true;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting all hits on the path that are not ignored by the filter, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="sweepRules">Collision rules to filter the sweep hits, anything above "NoSolver" is ignored</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool MultiSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionRules sweepRules, List<SRaycastResult> outSweepResults)
		{
			SimpleCollisionRulesOwner rulesOwner = new SimpleCollisionRulesOwner(sweepRules);
			RigidTransform startTransform = new RigidTransform(startPos.ToBepu(), rotation.ToBepu());
			Vector3 sweepVector = sweep.ToBepu();
			List<RayCastResult> physicsResults = new List<RayCastResult>();
			if (m_physicSpace.ConvexCast(shape, ref startTransform, ref sweepVector, GetOverlapFilter(rulesOwner), physicsResults))
			{
				foreach (RayCastResult physicsResult in physicsResults)
				{
					if (physicsResult.HitObject.Tag is CEntity gameEntity)
					{
						CollisionRule rule = CollisionRules.GetCollisionRule(physicsResult.HitObject, rulesOwner);
						SRaycastResult gameResult = new SRaycastResult()
						{
							HitEntity = gameEntity,
							Location = physicsResult.HitData.Location.ToSharp(),
							Normal = -physicsResult.HitData.Normal.ToSharp(),
							Distance = physicsResult.HitData.T,
							bIsSolidHit = rule <= CollisionRule.Normal
						};
						outSweepResults.Add(gameResult);
					}
				}
				return outSweepResults.Count > 0;
			}

			return false;
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting all hits on the path that are not ignored by the filter, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="group">Collision group to filter the sweep hits, anything above "NoSolver" is ignored</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool MultiSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionGroup group, List<SRaycastResult> outSweepResults)
		{
			RigidTransform startTransform = new RigidTransform(startPos.ToBepu(), rotation.ToBepu());
			Vector3 sweepVector = sweep.ToBepu();
			List<RayCastResult> physicsResults = new List<RayCastResult>();
			if (m_physicSpace.ConvexCast(shape, ref startTransform, ref sweepVector, GetOverlapFilter(group), physicsResults))
			{
				foreach (RayCastResult physicsResult in physicsResults)
				{
					if (physicsResult.HitObject.Tag is CEntity gameEntity)
					{
						CollisionRule rule = GetCollisionRuleWithGroup(physicsResult.HitObject, group);
						SRaycastResult gameResult = new SRaycastResult()
						{
							HitEntity = gameEntity,
							Location = physicsResult.HitData.Location.ToSharp(),
							Normal = -physicsResult.HitData.Normal.ToSharp(),
							Distance = physicsResult.HitData.T,
							bIsSolidHit = rule <= CollisionRule.Normal
						};
						outSweepResults.Add(gameResult);
					}
				}
				return outSweepResults.Count > 0;
			}

			return false;
		}

		/// <summary>
		/// Sweeps a convex shape against the physics world reporting all hits on the path even if they ignore collisions, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="shape">Shape to sweep</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool MultiSweep(ConvexShape shape, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, List<SRaycastResult> outSweepResults)
		{
			RigidTransform startTransform = new RigidTransform(startPos.ToBepu(), rotation.ToBepu());
			Vector3 sweepVector = sweep.ToBepu();
			List<RayCastResult> physicsResults = new List<RayCastResult>();
			if (m_physicSpace.ConvexCast(shape, ref startTransform, ref sweepVector, physicsResults))
			{
				foreach (RayCastResult physicsResult in physicsResults)
				{
					if (physicsResult.HitObject.Tag is CEntity gameEntity)
					{
						SRaycastResult gameResult = new SRaycastResult()
						{
							HitEntity = gameEntity,
							Location = physicsResult.HitData.Location.ToSharp(),
							Normal = -physicsResult.HitData.Normal.ToSharp(),
							Distance = physicsResult.HitData.T,
							bIsSolidHit = false
						};
						outSweepResults.Add(gameResult);
					}
				}
				return outSweepResults.Count > 0;
			}

			return false;
		}

		/// <summary>
		/// Sweeps a sphere against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool SphereSweep(float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Vector3 sweep, ref SRaycastResult outSweepResult)
		{
			SphereShape sphereShape = new SphereShape(radius);
			Quaternion rotation = Quaternion.Identity;
			return ConvexSweep(sphereShape, ref startPos, ref rotation, ref sweep, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a sphere against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="sweepRules">Collision rules to filter the sweep hits, anything above "Normal" is ignored</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool SphereSweep(float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Vector3 sweep, CollisionRules sweepRules, ref SRaycastResult outSweepResult)
		{
			SphereShape sphereShape = new SphereShape(radius);
			Quaternion rotation = Quaternion.Identity;
			return ConvexSweep(sphereShape, ref startPos, ref rotation, ref sweep, sweepRules, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a sphere against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="group">Collision group to filter the sweep hits, anything above "Normal" is ignored</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool SphereSweep(float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Vector3 sweep, CollisionGroup group, ref SRaycastResult outSweepResult)
		{
			SphereShape sphereShape = new SphereShape(radius);
			Quaternion rotation = Quaternion.Identity;
			return ConvexSweep(sphereShape, ref startPos, ref rotation, ref sweep, group, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a sphere against the physics world reporting all hits on the path that are not ignored by the filter, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="sweepRules">Collision rules to filter the sweep hits, anything above "NoSolver" is ignored</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool SphereMultiSweep(float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Vector3 sweep, CollisionRules sweepRules, List<SRaycastResult> outSweepResults)
		{
			SphereShape sphereShape = new SphereShape(radius);
			Quaternion rotation = Quaternion.Identity;
			return MultiSweep(sphereShape, ref startPos, ref rotation, ref sweep, sweepRules, outSweepResults);
		}

		/// <summary>
		/// Sweeps a sphere against the physics world reporting all hits on the path that are not ignored by the filter, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="group">Collision group to filter the sweep hits, anything above "NoSolver" is ignored</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool SphereMultiSweep(float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Vector3 sweep, CollisionGroup group, List<SRaycastResult> outSweepResults)
		{
			SphereShape sphereShape = new SphereShape(radius);
			Quaternion rotation = Quaternion.Identity;
			return MultiSweep(sphereShape, ref startPos, ref rotation, ref sweep, group, outSweepResults);
		}

		/// <summary>
		/// Sweeps a sphere against the physics world reporting all hits on the path even if they ignore collisions, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool SphereMultiSweep(float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Vector3 sweep, List<SRaycastResult> outSweepResults)
		{
			SphereShape sphereShape = new SphereShape(radius);
			Quaternion rotation = Quaternion.Identity;
			return MultiSweep(sphereShape, ref startPos, ref rotation, ref sweep, outSweepResults);
		}

		/// <summary>
		/// Sweeps a capsule against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="length">Length of the capsule</param>
		/// <param name="radius">Radius of the capsule</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool CapsuleSweep(float length, float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, ref SRaycastResult outSweepResult)
		{
			CapsuleShape capsuleShape = new CapsuleShape(length, radius);
			return ConvexSweep(capsuleShape, ref startPos, ref rotation, ref sweep, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a capsule against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="length">Length of the capsule</param>
		/// <param name="radius">Radius of the capsule</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="sweepRules">Collision rules to filter the sweep hits, anything above "Normal" is ignored</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool CapsuleSweep(float length, float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionRules sweepRules, ref SRaycastResult outSweepResult)
		{
			CapsuleShape capsuleShape = new CapsuleShape(length, radius);
			return ConvexSweep(capsuleShape, ref startPos, ref rotation, ref sweep, sweepRules, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a capsule against the physics world reporting the closest hit to the start location, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="length">Length of the capsule</param>
		/// <param name="radius">Radius of the capsule</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="group">Collision group to filter the sweep hits, anything above "Normal" is ignored</param>
		/// <param name="outSweepResult">Closest hit to the start location</param>
		/// <returns>True if any object was hit</returns>
		public bool CapsuleSweep(float length, float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionGroup group, ref SRaycastResult outSweepResult)
		{
			CapsuleShape capsuleShape = new CapsuleShape(length, radius);
			return ConvexSweep(capsuleShape, ref startPos, ref rotation, ref sweep, group, ref outSweepResult);
		}

		/// <summary>
		/// Sweeps a capsule against the physics world reporting all hits on the path that are not ignored by the filter, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="length">Length of the capsule</param>
		/// <param name="radius">Radius of the capsule</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="sweepRules">Collision rules to filter the sweep hits, anything above "NoSolver" is ignored</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool CapsuleMultiSweep(float length, float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionRules sweepRules, List<SRaycastResult> outSweepResults)
		{
			CapsuleShape capsuleShape = new CapsuleShape(length, radius);
			return MultiSweep(capsuleShape, ref startPos, ref rotation, ref sweep, sweepRules, outSweepResults);
		}

		/// <summary>
		/// Sweeps a capsule against the physics world reporting all hits on the path that are not ignored by the filter, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="length">Length of the capsule</param>
		/// <param name="radius">Radius of the capsule</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="group">Collision group to filter the sweep hits, anything above "NoSolver" is ignored</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool CapsuleMultiSweep(float length, float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, CollisionGroup group, List<SRaycastResult> outSweepResults)
		{
			CapsuleShape capsuleShape = new CapsuleShape(length, radius);
			return MultiSweep(capsuleShape, ref startPos, ref rotation, ref sweep, group, outSweepResults);
		}

		/// <summary>
		/// Sweeps a capsule against the physics world reporting all hits on the path even if they ignore collisions, beware convex sweeps can be very costly especially if they are long
		/// </summary>
		/// <param name="length">Length of the capsule</param>
		/// <param name="radius">Radius of the capsule</param>
		/// <param name="startPos">Location to start the sweep from</param>
		/// <param name="rotation">Rotation of the shape during the sweep</param>
		/// <param name="sweep">Direction in which to sweep, the length of the vector is the length of the sweep</param>
		/// <param name="outSweepResults">All overlaps on the sweep path</param>
		/// <returns>True if any object was hit</returns>
		public bool CapsuleMultiSweep(float length, float radius, ref SharpDX.Vector3 startPos, ref SharpDX.Quaternion rotation, ref SharpDX.Vector3 sweep, List<SRaycastResult> outSweepResults)
		{
			CapsuleShape capsuleShape = new CapsuleShape(length, radius);
			return MultiSweep(capsuleShape, ref startPos, ref rotation, ref sweep, outSweepResults);
		}

		private Func<BroadPhaseEntry, bool> GetSolidFilter(ICollisionRulesOwner rulesOwner)
		{
			bool Filter(BroadPhaseEntry e)
			{
				CollisionRule rule = CollisionRules.GetCollisionRule(e, rulesOwner);
				return rule <= CollisionRule.Normal;
			}

			return Filter;
		}
		private Func<BroadPhaseEntry, bool> GetSolidFilter(CollisionGroup group)
		{
			bool Filter(BroadPhaseEntry e)
			{
				return GetCollisionRuleWithGroup(e, group) <= CollisionRule.Normal;
			}

			return Filter;
		}
		private Func<BroadPhaseEntry, bool> GetOverlapFilter(ICollisionRulesOwner rulesOwner)
		{
			bool Filter(BroadPhaseEntry e)
			{
				CollisionRule rule = CollisionRules.GetCollisionRule(e, rulesOwner);
				return rule <= CollisionRule.NoSolver;
			}

			return Filter;
		}
		private Func<BroadPhaseEntry, bool> GetOverlapFilter(CollisionGroup group)
		{
			bool Filter(BroadPhaseEntry e)
			{
				return GetCollisionRuleWithGroup(e, group) <= CollisionRule.NoSolver;
			}

			return Filter;
		}
		#endregion

		private CollisionRule GetCollisionRuleWithGroup(ICollisionRulesOwner rulesOwner, CollisionGroup group)
		{
			if (rulesOwner.CollisionRules.Personal != CollisionRule.Defer)
			{
				return rulesOwner.CollisionRules.Personal;
			}

			CollisionGroupPair pair = new CollisionGroupPair(group, rulesOwner.CollisionRules.Group);
			CollisionRules.CollisionGroupRules.TryGetValue(pair, out CollisionRule rule);
			return rule;
		}

		private readonly ParallelLooper m_looper = new ParallelLooper();
		private Space m_physicSpace;
		private CWorld m_gameWorld;
	}
}
