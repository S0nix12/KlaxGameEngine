using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.Events;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities;
using KlaxCore.Core;
using KlaxCore.Physics;
using KlaxCore.Physics.Components;
using KlaxMath;
using KlaxShared.Utilities;
using Quaternion = SharpDX.Quaternion;
using Vector3 = SharpDX.Vector3;
using Matrix = SharpDX.Matrix;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using KlaxCore.KlaxScript;
using KlaxIO.Input;

namespace KlaxCore.GameFramework
{
	[KlaxScriptType(Name = "Entity")]
	public class CEntity : CWorldObject
	{
		public override void Init(CWorld world, object userData)
		{
			base.Init(world, userData);

			m_bIsAlive = true;
			m_bIsInitialized = true;

			int count = m_components.Count;
			for (int i = 0; i < count; i++)
			{
				m_components[i].Init();
				m_guidToComponent.Add(m_components[i].ComponentGuid, m_components[i]);
			}

			KlaxScriptObject.Init(this);
		}

		public virtual void Start()
		{
			if (IsPhysicsEnabled)
			{
				RecreatePhysics();
			}

			Input.RegisterListener(OnInputEvent);

			int count = m_components.Count;
			for (int i = 0; i < count; i++)
			{
				m_components[i].Start();
			}

			if (UpdateScriptEvent.HasHandlers())
			{
				m_updateScope = World.UpdateScheduler.Connect(Update, EUpdatePriority.Default);
			}

			StartScriptEvent.Invoke();
		}

		public void Update(float deltaTime)
		{
			UpdateScriptEvent.Invoke(deltaTime, this);
		}

		public virtual void Shutdown()
		{
			foreach (CEntityComponent component in m_components)
			{
				component.Shutdown();
			}
			m_bIsAlive = false;
			IsPhysicsEnabled = false;

			CEntity parent = Parent;
			if (parent != null)
			{
				parent.UnregisterChildEntity(this);
			}

			Input.UnregisterListener(OnInputEvent);
			m_updateScope?.Disconnect();
		}

		[KlaxFunction(Category = "Component", Tooltip = "Adds a component of the given type to this entity")]
		public CEntityComponent AddComponentToEntity(SSubtypeOf<CEntityComponent> componentType, bool bAutoAttach, bool bInitComponent)
		{
			return AddComponent(componentType.Type, bAutoAttach, bInitComponent);
		}

		public CEntityComponent AddComponent(Type type, bool bAutoAttach, bool bInitComponent)
		{
			if (!type.IsSubclassOf(typeof(CEntityComponent)))
				return null;

			CEntityComponent component = Activator.CreateInstance(type) as CEntityComponent;
			if (component != null)
			{
				component.RegisterComponent(this);
				m_guidToComponent.Add(component.ComponentGuid, component);

				if (bAutoAttach && component is CSceneComponent sceneComponent)
				{
					AttachComponent(sceneComponent);
				}

				if (bInitComponent)
				{
					component.Init();
				}

				return component;
			}

			return null;
		}

		public T AddComponent<T>(bool bAutoAttach, bool bInitComponent) where T : CEntityComponent, new()
		{
			T component = new T();
			component.RegisterComponent(this);
			m_guidToComponent.Add(component.ComponentGuid, component);

			if (bAutoAttach && component is CSceneComponent sceneComponent)
			{
				AttachComponent(sceneComponent);
			}

			if (bInitComponent)
			{
				component.Init();
			}

			return component;
		}

		public void RegisterComponent(CEntityComponent component)
		{
			if (!m_components.Contains(component))
			{
				m_components.Add(component);
			}
		}

		public void UnregisterComponent(CEntityComponent component)
		{
			m_components.Remove(component);

			if (m_rootComponent == component)
			{
				Detach();
				m_rootComponent = null;
			}
		}

		private void PrePhysicsUpdate(float deltaTime)
		{
			if (!IsPhysicsEnabled)
			{
				return;
			}

			if (m_bIsPhysicsDirty)
			{
				RecreatePhysics();
			}

			if (m_bIsStaticColliderDirty && IsPhysicsStatic)
			{
				List<CColliderComponent> colliders = new List<CColliderComponent>();
				GetComponents(colliders);
				foreach (CColliderComponent collider in colliders)
				{
					collider.UpdateStaticCollider();
				}

				PhysicalStatic.Shape.CollidableTree.Refit();
				PhysicalStatic.UpdateBoundingBox();

				m_bIsStaticColliderDirty = false;
				m_prePhysicsUpdateScope.Disconnect();
				m_prePhysicsUpdateScope = null;
			}
			SetPhysicsLocation();
		}

		private void PostPhysicsUpdate(float deltaTime)
		{
			SyncToPhysicsLocation();
		}

		#region Transform Helper Functions	   
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetWorldPosition()
		{
			if (m_rootComponent != null)
			{
				return m_rootComponent.WorldPosition;
			}

			return Vector3.Zero;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetWorldScale()
		{
			if (m_rootComponent != null)
			{
				return m_rootComponent.WorldScale;
			}

			return Vector3.Zero;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Quaternion GetWorldRotation()
		{
			if (m_rootComponent != null)
			{
				return m_rootComponent.WorldRotation;
			}

			return Quaternion.Identity;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetLocalPosition()
		{
			if (m_rootComponent != null)
			{
				return m_rootComponent.LocalPosition;
			}

			return Vector3.Zero;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetLocalScale()
		{
			if (m_rootComponent != null)
			{
				return m_rootComponent.LocalScale;
			}

			return Vector3.Zero;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Quaternion GetLocalRotation()
		{
			if (m_rootComponent != null)
			{
				return m_rootComponent.LocalRotation;
			}

			return Quaternion.Identity;
		}

		[KlaxFunction(Category = "Entity")]
		public void SetWorldPosition(in Vector3 worldPosition)
		{
			m_rootComponent?.SetWorldPosition(in worldPosition);
		}
		[KlaxFunction(Category = "Entity")]
		public void SetWorldRotation(in Quaternion worldRotation)
		{
			m_rootComponent?.SetWorldRotation(in worldRotation);
		}
		[KlaxFunction(Category = "Entity")]
		public void SetWorldScale(in Vector3 worldScale)
		{
			m_rootComponent?.SetWorldScale(in worldScale);
		}
		[KlaxFunction(Category = "Entity")]
		public void SetLocalPosition(in Vector3 position)
		{
			if (m_rootComponent != null)
			{
				m_rootComponent.LocalPosition = position;
			}
		}
		[KlaxFunction(Category = "Entity")]
		public void SetLocalRotation(in Quaternion rotation)
		{
			if (m_rootComponent != null)
			{
				m_rootComponent.LocalRotation = rotation;
			}
		}
		[KlaxFunction(Category = "Entity")]
		public void SetLocalScale(in Vector3 scale)
		{
			if (m_rootComponent != null)
			{
				m_rootComponent.LocalScale = scale;
			}
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetForward()
		{
			return m_rootComponent?.Forward ?? Axis.Forward;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetRight()
		{
			return m_rootComponent?.Right ?? Axis.Right;
		}
		[KlaxFunction(Category = "Entity", IsImplicit = true)]
		public Vector3 GetUp()
		{
			return m_rootComponent?.Up ?? Axis.Up;
		}
		#endregion

		/// <summary>
		/// Attaches a component to this entity. If this entity has a root component the given component is attached to the root component otherwise it will become the root component
		/// </summary>
		/// <param name="componentToAttach"></param>
		[KlaxFunction(Category = "SceneComponent", Tooltip = "Attaches the given scene component to this entities root or making it the root if their is no current root")]
		public void AttachComponent(CSceneComponent componentToAttach)
		{
			if (m_rootComponent == null)
			{
				m_rootComponent = componentToAttach;
				if (componentToAttach.Owner != this)
				{
					componentToAttach.ChangeOwner(this);
				}
			}
			else
			{
				componentToAttach.AttachToComponent(m_rootComponent);
			}
		}

		/// <summary>
		/// Promotes a child scene component to be the new root component of this entity. Will only succeed if no loops are created
		/// </summary>
		/// <param name="newRoot"></param>
		[KlaxFunction(Category = "SceneComponent", Tooltip = "Makes the given child component the new root component, the given component has to be a child of this entity")]
		public bool SetRootComponent(CSceneComponent newRoot)
		{
			if (newRoot.Owner != this)
				return false;

			CSceneComponent oldRoot = RootComponent;
			CSceneComponent oldRootParent = RootComponent?.ParentComponent;
			newRoot.Detach();

			oldRoot?.AttachToComponent(newRoot);
			newRoot.AttachToComponent(oldRootParent);

			var children = oldRoot.Children;
			for (int i = children.Count - 1; i >= 0; i--)
			{
				children[i].AttachToComponent(newRoot);
			}

			m_rootComponent = newRoot;

			return true;
		}

		/// <summary>
		/// Attaches this entity to another entities root component
		/// </summary>
		/// <param name="parentEntity"></param>
		[KlaxFunction(Category = "Entity", Tooltip = "Attaches this entity to another entity")]
		public void AttachToEntity(CEntity parentEntity)
		{
			AttachToComponent(parentEntity.RootComponent);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentComponent"></param>
		[KlaxFunction(Category = "Entity", Tooltip = "Attaches this entity to scene component")]
		public void AttachToComponent(CSceneComponent parentComponent)
		{
			if (parentComponent != null && m_rootComponent != null && parentComponent.Owner != this)
			{
				m_rootComponent.AttachToComponent(parentComponent);
			}
		}

		[KlaxFunction(Category = "Entity", Tooltip = "Detach this entity if it was attached to another entity")]
		public void Detach()
		{
			if (m_rootComponent != null && m_rootComponent.ParentComponent != null)
			{
				m_rootComponent.Detach();
			}
		}

		/// <summary>
		/// Internal function used to register child entities when a scene component gets attached to something else
		/// </summary>
		/// <param name="childEntity"></param>
		public void RegisterChildEntity(CEntity childEntity)
		{
			ContainerUtilities.AddUnique(m_children, childEntity);
		}

		/// <summary>
		/// Internal function used to unregister child entities when they get destroyed or detached
		/// </summary>
		/// <param name="childEntity"></param>
		public void UnregisterChildEntity(CEntity childEntity)
		{
			m_children.Remove(childEntity);
		}

		/// <summary>
		/// Returns the component with the given guid if part of this entity
		/// </summary>
		/// <param name="componentGuid"></param>
		/// <returns></returns>
		public CEntityComponent GetComponentByGuid(Guid componentGuid)
		{
			m_guidToComponent.TryGetValue(componentGuid, out CEntityComponent outComponent);
			return outComponent;
		}

		/// <summary>
		/// Gets the first component of the given type owned by this entity
		/// </summary>
		/// <typeparam name="T">Type of the component to get</typeparam>
		/// <returns></returns>
		public T GetComponent<T>() where T : CEntityComponent
		{
			for (int i = 0; i < m_components.Count; i++)
			{
				if (m_components[i] is T component)
				{
					return component;
				}
			}

			return null;
		}

		/// <summary>
		/// Get all components of the given type owned by this entity
		/// </summary>
		/// <typeparam name="T">Type of components to get</typeparam>
		/// <param name="outComponents"></param>
		public void GetComponents<T>(List<T> outComponents) where T : CEntityComponent
		{
			outComponents.Clear();
			for (int i = 0; i < m_components.Count; i++)
			{
				if (m_components[i] is T component)
				{
					outComponents.Add(component);
				}
			}
		}

		/// <summary>
		/// Gets the component that has the given id owned by this entity
		/// </summary>
		/// <typeparam name="T">Type of the component to get</typeparam>
		/// <param name="componentId">Unique component id</param>
		/// <returns></returns>
		public T GetComponentById<T>(int componentId) where T : CEntityComponent
		{
			for (int i = 0; i < m_components.Count; i++)
			{
				if (m_components[i].Id == componentId)
				{
					return m_components[i] as T;
				}
			}

			return null;
		}

		public override void Destroy()
		{
			base.Destroy();

			RemovePhysics();
			m_bMarkedForDestruction = true;
			World?.MarkEntityForDestruction(this);
		}

		#region ScriptUtility

		private void OnInputEvent(ReadOnlyCollection<SInputButtonEvent> buttonevents, string textinput)
		{
			foreach (SInputButtonEvent buttonEvent in buttonevents)
			{
				if (buttonEvent.buttonEvent == EButtonEvent.Pressed)
				{
					ButtonPressed.Invoke(buttonEvent.button);
				}
				else
				{
					ButtonReleased.Invoke(buttonEvent.button);
				}
			}
		}

		#endregion

		#region Physics

		/// <summary>
		/// Invoked whenever this entities physics gets enabled/disabled
		/// </summary>
		public event Action<bool> OnPhysicsStateChanged;
		/// <summary>
		/// Invoked whenever this entities physics become static/dynamic (true if static false otherwise)
		/// </summary>
		public event Action<bool> OnPhysicsStaticStateChanged;

		public delegate void BeginCollision(SCollisionEventData collision);
		/// <summary>
		/// Invoked when this Entity starts colliding
		/// </summary>
		public event Action<SCollisionEventData> OnBeginCollision;

		[KlaxEvent(DisplayName = "BeginCollision", ParameterName1 = "Collision")]
		public readonly CKlaxScriptEvent<SCollisionEventData> BeginCollisionScript = new CKlaxScriptEvent<SCollisionEventData>();

		public delegate void EndCollision(CEntity entityA, CEntity entityB);
		/// <summary>
		/// Invoked when this Entity stops colliding with another entity. It can still be colliding with other entities
		/// </summary>
		public event Action<CEntity, CEntity> OnEndCollision;

		/// <summary>
		/// Removes this entities physics from the physics world and reconstructs the colliders from its components
		/// Afterwards the new physics representation is added to the physics world again, no motion state is saved during the process
		/// </summary>
		public void RecreatePhysics()
		{
			CollisionRules oldCollisionRules = null;
			if (PhysicalEntity != null)
			{
				oldCollisionRules = PhysicalEntity.CollisionInformation.CollisionRules;
			}
			else if (PhysicalStatic != null)
			{
				oldCollisionRules = PhysicalStatic.CollisionRules;
			}
			RemovePhysics();

			PhysicalEntity = null;
			PhysicalStatic = null;

			List<CColliderComponent> colliders = new List<CColliderComponent>();
			GetComponents(colliders);

			if (colliders.Count > 0)
			{
				if (IsPhysicsStatic)
				{
					// Static construction			
					List<Collidable> staticCollidables = new List<Collidable>(colliders.Count);
					for (int i = 0; i < colliders.Count; i++)
					{
						colliders[i].GetStaticCollidablesWithScale(staticCollidables);
					}

					if (staticCollidables.Count == 0)
					{
						LogUtility.Log("Tried to activate static physics for entity " + Name + "but it does not contain any static colliders", ELogVerbosity.Debug);
						IsPhysicsEnabled = false;
						m_bWantsPhysicsEnabled = true;
						return;
					}

					StaticGroup staticGroup = new StaticGroup(staticCollidables);
					staticGroup.Tag = this;
					World.PhysicsWorld.AddPhysicsObject(staticGroup);
					PhysicalStatic = staticGroup;
					PhysicalStatic.Events.InitialCollisionDetected += OnStartStaticCollision;
					PhysicalStatic.Events.CollisionEnded += OnStopStaticCollision;

					if (oldCollisionRules != null)
					{
						PhysicalStatic.CollisionRules = oldCollisionRules;
					}
				}
				else
				{
					List<CompoundShapeEntry> shapeEntries = new List<CompoundShapeEntry>(colliders.Count);
					for (int i = 0; i < colliders.Count; i++)
					{
						CColliderComponent collider = colliders[i];

						if (collider.GetShapeWithScale() is EntityShape entityShape)
						{
							Quaternion localRotation = Quaternion.Conjugate(GetWorldRotation()) * collider.WorldRotation;
							Vector3 localPosition = Vector3.TransformCoordinate(collider.WorldPosition, Matrix.Invert(RootComponent.Transform.WorldMatrix));

							CompoundShapeEntry shapeEntry = new CompoundShapeEntry(entityShape, new RigidTransform(localPosition.ToBepu(), localRotation.ToBepu()));
							shapeEntries.Add(shapeEntry);
						}
					}

					if (shapeEntries.Count == 0)
					{
						LogUtility.Log("Tried to activate dynamic physics for entity " + Name + "but it does not contain any dynamic colliders", ELogVerbosity.Debug);
						IsPhysicsEnabled = false;
						m_bWantsPhysicsEnabled = true;
						return;
					}

					PhysicalEntity = new CompoundBody(shapeEntries, PhysicsMass);
					PhysicalEntity.Tag = this;
					PhysicalEntity.CollisionInformation.Tag = this;
					PhysicalEntity.CollisionInformation.Events.InitialCollisionDetected += OnStartCollision;
					PhysicalEntity.CollisionInformation.Events.CollisionEnded += OnStopCollision;

					if (oldCollisionRules != null)
					{
						PhysicalEntity.CollisionInformation.CollisionRules = oldCollisionRules;
					}

					if (m_bUseEntityPivotAsMassCenter)
					{
						// We want the center of the physical entity to match our Entity Location so we need to offset the collision shape
						PhysicalEntity.CollisionInformation.LocalPosition = PhysicalEntity.Position;
					}
					else
					{
						m_physicsOffset = PhysicalEntity.Position.ToSharp();
					}

					PhysicalEntity.Position = GetWorldPosition().ToBepu();
					PhysicalEntity.Orientation = GetWorldRotation().ToBepu();
					World.PhysicsWorld.AddPhysicsObject(PhysicalEntity);

					if (m_prePhysicsUpdateScope == null)
					{
						m_prePhysicsUpdateScope = World.UpdateScheduler.Connect(PrePhysicsUpdate, EUpdatePriority.PrePhysics);
						m_postPhysicsUpdateScope = World.UpdateScheduler.Connect(PostPhysicsUpdate, EUpdatePriority.PostPhysics);
					}
				}
			}

			m_bIsPhysicsDirty = false;
			m_bIsStaticColliderDirty = false;
		}

		private void RemovePhysics()
		{
			if (PhysicalEntity != null)
			{
				World.PhysicsWorld.RemovePhysicsObject(PhysicalEntity);
				PhysicalEntity = null;
			}

			if (PhysicalStatic != null)
			{
				World.PhysicsWorld.RemovePhysicsObject(PhysicalStatic);
				PhysicalStatic = null;
			}
		}

		private void OnStartStaticCollision(StaticGroup sender, Collidable other, CollidablePairHandler pair)
		{
			System.Diagnostics.Debug.Assert(pair.Contacts.Count > 0);
			if (OnBeginCollision != null || BeginCollisionScript.HasHandlers())
			{
				CEntity entityA = (CEntity)sender.Tag;
				CEntity entityB;
				if (other is EntityCollidable otherEntityCollidable)
				{
					entityB = (CEntity)otherEntityCollidable.Entity.Tag;
				}
				else
				{
					entityB = (CEntity)other.Tag;
				}

				ContactInformation contactInfo = pair.Contacts[0];
				SCollisionEventData eventData = new SCollisionEventData(entityA, entityB, pair.EntityA, pair.EntityB, pair.Contacts[0].Contact);
				if (contactInfo.IsMeshContact)
				{
					if (sender == pair.CollidableB)
					{
						eventData.Normal *= -1;
					}
				}
				else
				{
					if (sender == pair.CollidableA)
					{
						eventData.Normal *= -1;
					}
				}

				OnBeginCollision?.Invoke(eventData);
				BeginCollisionScript.Invoke(eventData);
			}
		}
		private void OnStopStaticCollision(StaticGroup sender, Collidable other, CollidablePairHandler pair)
		{
			if (OnEndCollision != null)
			{
				CEntity entityA = (CEntity)sender.Tag;
				CEntity entityB;
				if (other is EntityCollidable otherEntityCollidable)
				{
					entityB = (CEntity)otherEntityCollidable.Entity.Tag;
				}
				else
				{
					entityB = (CEntity)other.Tag;
				}

				OnEndCollision.Invoke(entityA, entityB);
			}
		}

		private void OnStartCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
		{
			System.Diagnostics.Debug.Assert(pair.Contacts.Count > 0);
			if (OnBeginCollision != null || BeginCollisionScript.HasHandlers())
			{
				CEntity entityA = (CEntity)sender.Entity.Tag;
				CEntity entityB;
				if (other is EntityCollidable otherEntityCollidable)
				{
					entityB = (CEntity)otherEntityCollidable.Entity.Tag;
				}
				else
				{
					entityB = (CEntity)other.Tag;
				}

				ContactInformation contactInfo = pair.Contacts[0];
				SCollisionEventData eventData = new SCollisionEventData(entityA, entityB, pair.EntityA, pair.EntityB, pair.Contacts[0].Contact);
				if (contactInfo.IsMeshContact)
				{
					if (sender == pair.CollidableB)
					{
						eventData.Normal *= -1;
					}
				}
				else
				{
					if (sender == pair.CollidableA)
					{
						eventData.Normal *= -1;
					}
				}
				OnBeginCollision?.Invoke(eventData);
				BeginCollisionScript.Invoke(eventData);
			}
		}
		private void OnStopCollision(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
		{
			if (OnEndCollision != null)
			{
				CEntity entityA = (CEntity)sender.Entity.Tag;
				CEntity entityB;
				if (other is EntityCollidable otherEntityCollidable)
				{
					entityB = (CEntity)otherEntityCollidable.Entity.Tag;
				}
				else
				{
					entityB = (CEntity)other.Tag;
				}

				OnEndCollision.Invoke(entityA, entityB);
			}
		}

		private void SyncToPhysicsLocation()
		{
			if (PhysicalEntity != null)
			{
				SetWorldRotation(PhysicalEntity.Orientation.ToSharp());

				if (m_physicsOffset.IsZero)
				{
					SetWorldPosition(PhysicalEntity.Position.ToSharp());
				}
				else
				{
					Vector3 worldOffset = Vector3.Transform(m_physicsOffset, GetWorldRotation());
					SetWorldPosition(PhysicalEntity.Position.ToSharp() - worldOffset);
				}
			}
		}

		private void SetPhysicsLocation()
		{
			if (PhysicalEntity != null)
			{
				if (m_physicsOffset.IsZero)
				{
					PhysicalEntity.Position = (GetWorldPosition()).ToBepu();
				}
				else
				{
					Vector3 worldOffset = Vector3.Transform(m_physicsOffset, GetWorldRotation());
					PhysicalEntity.Position = (GetWorldPosition() + worldOffset).ToBepu();
				}
				PhysicalEntity.Orientation = GetWorldRotation().ToBepu();
			}
		}

		/// <summary>
		/// Used to tell the entity that its physics need to be reconstructed, some complex physical entities need to be reconstructed on being scaled or other changes
		/// Primitive colliders will just update their shape without forcing a reconstruct
		/// </summary>
		public void MarkPhysicsDirty()
		{
			m_bIsPhysicsDirty = true;
			if (IsPhysicsStatic && IsPhysicsEnabled)
			{
				if (m_prePhysicsUpdateScope == null || !m_prePhysicsUpdateScope.IsConnected())
				{
					m_prePhysicsUpdateScope = World.UpdateScheduler.ConnectOneTimeUpdate(PrePhysicsUpdate, EUpdatePriority.PrePhysics);
				}
			}
			else if (m_bWantsPhysicsEnabled)
			{
				IsPhysicsEnabled = true;
			}
		}

		private bool m_bIsStaticColliderDirty;
		/// <summary>
		/// Used to tell the entity that its static colliders changed, if the entity is static its static group will be updated in the next PrePhysicsUpdate
		/// Unlike MarkPhysicsDirty this will not trigger a full reconstruct if a full reconstruct is needed use MarkPhysicsDirty
		/// </summary>
		public void MarkStaticColliderDirty()
		{
			if (IsPhysicsStatic && IsPhysicsEnabled)
			{
				m_bIsStaticColliderDirty = true;
				if (m_prePhysicsUpdateScope == null || !m_prePhysicsUpdateScope.IsConnected())
				{
					m_prePhysicsUpdateScope = World.UpdateScheduler.ConnectOneTimeUpdate(PrePhysicsUpdate, EUpdatePriority.PrePhysics);
				}
			}
			else if (m_bWantsPhysicsEnabled)
			{
				IsPhysicsEnabled = true;
			}
		}

		/// <summary>
		/// Representation of this Entity in the PhysicsWorld, can be null if not registered for Physics or static
		/// </summary>
		[JsonIgnore]
		public Entity PhysicalEntity { get; private set; }

		/// <summary>
		/// Representation of this Entity in the PhysicsWorld if set to static, static physics cannot be moved. If the Entity is moveable this object is Null
		/// </summary>
		[JsonIgnore]
		public StaticGroup PhysicalStatic { get; private set; }

		private float m_physicsMass = 1.0f;
		/// <summary>
		/// Sets the mass of the physical entity, setting this to a positive value will force the entity to be dynamic while an negative value will make the entity kinematic
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public float PhysicsMass
		{
			get { return m_physicsMass; }
			set
			{
				if (Math.Abs(m_physicsMass - value) > float.Epsilon)
				{
					m_physicsMass = value;
					if (PhysicalEntity != null)
					{
						PhysicalEntity.Mass = m_physicsMass;
					}
				}
			}
		}

		[JsonProperty]
		private bool m_bWantsPhysicsEnabled;
		[JsonProperty]
		private bool m_bIsPhysicsEnabled;
		[JsonIgnore]
		[KlaxProperty(Category = "Physics")]
		public bool IsPhysicsEnabled
		{
			get { return m_bIsPhysicsEnabled; }
			set
			{
				if (m_bIsPhysicsEnabled != value)
				{
					m_bWantsPhysicsEnabled = false;
					m_bIsPhysicsEnabled = value;
					if (m_bIsPhysicsEnabled)
					{
						RecreatePhysics();
					}
					else
					{
						if (PhysicalEntity != null)
						{
							World.PhysicsWorld.RemovePhysicsObject(PhysicalEntity);
						}
						else if (PhysicalStatic != null)
						{
							World.PhysicsWorld.RemovePhysicsObject(PhysicalStatic);
						}

						PhysicalEntity = null;
						PhysicalStatic = null;

						m_prePhysicsUpdateScope?.Disconnect();
						m_prePhysicsUpdateScope = null;
						m_postPhysicsUpdateScope?.Disconnect();
						m_postPhysicsUpdateScope = null;
					}
					OnPhysicsStateChanged?.Invoke(m_bIsPhysicsEnabled);
				}
			}
		}

		[JsonProperty]
		private bool m_bIsPhysicsStatic;
		/// <summary>
		/// Gets or Sets if this entities physics are static, static physics are constructed at spawn time or when the entity is set to static, they cannot be moved afterwards
		/// </summary>
		[JsonIgnore]
		[KlaxProperty(Category = "Physics")]
		public bool IsPhysicsStatic
		{
			get { return m_bIsPhysicsStatic; }
			set
			{
				if (m_bIsPhysicsStatic != value)
				{
					m_bIsPhysicsStatic = value;
					if (m_bIsPhysicsEnabled)
					{
						RecreatePhysics();
						OnPhysicsStaticStateChanged?.Invoke(m_bIsPhysicsEnabled);
					}
				}
			}
		}

		private bool m_bUseEntityPivotAsMassCenter;
		/// <summary>
		/// Should the entities pivot be used as the physics center of mass otherwise the computed center of mass from all attached colliders is used resulting in physical correct behavior
		/// </summary>
		[KlaxProperty(Category = "Physics")]
		public bool UseEntityPivotAsMassCenter
		{
			get { return m_bUseEntityPivotAsMassCenter; }
			set
			{
				if (m_bUseEntityPivotAsMassCenter != value)
				{
					m_bUseEntityPivotAsMassCenter = value;
					if (PhysicalEntity != null)
					{
						if (m_bUseEntityPivotAsMassCenter)
						{
							PhysicalEntity.CollisionInformation.LocalPosition = m_physicsOffset.ToBepu();
							m_physicsOffset = Vector3.Zero;
						}
						else
						{
							m_physicsOffset = PhysicalEntity.CollisionInformation.LocalPosition.ToSharp();
							PhysicalEntity.CollisionInformation.LocalPosition = BEPUutilities.Vector3.Zero;
						}
					}
				}
			}
		}

		private bool m_bIsPhysicsDirty;
		private CUpdateScope m_prePhysicsUpdateScope;
		private CUpdateScope m_postPhysicsUpdateScope;
		private Vector3 m_physicsOffset;
		#endregion

		[KlaxProperty(Category = "Basic", IsReadOnly = true)]
		public string Name { get; set; }
		[JsonIgnore]
		public int Id { get; set; }
		public bool ShowInOutliner { get; protected set; } = true;

		private bool m_bIsInitialized;
		[JsonIgnore]
		public bool IsInitialized
		{
			get { return m_bIsInitialized; }
		}

		private bool m_bMarkedForDestruction;
		[JsonIgnore]
		public bool MarkedForDestruction
		{
			get { return m_bMarkedForDestruction; }
		}

		private bool m_bIsAlive;
		[JsonIgnore]
		[KlaxProperty(Category = "Basic", IsReadOnly = true)]
		public bool IsAlive
		{
			get { return m_bIsAlive; }
			set { m_bIsAlive = value; }
		}

		/// <summary>
		/// Root component of this entity, defines the world transform. Can be null
		/// </summary>
		[JsonIgnore]
		[KlaxProperty(Category = "Basic", IsReadOnly = true)]
		public CSceneComponent RootComponent
		{
			get { return m_rootComponent; }
		}

		public ReadOnlyCollection<CEntity> Children
		{
			get { return new ReadOnlyCollection<CEntity>(m_children); }
		}

		[KlaxProperty(Category = "Basic", IsReadOnly = true)]
		public CEntity Parent
		{
			get { return m_rootComponent?.ParentComponent?.Owner; }
		}

		[JsonIgnore]
		public CKlaxScriptObject KlaxScriptObject
		{
			get
			{
				if (m_klaxScriptObject == null)
					m_klaxScriptObject = new CKlaxScriptObject();

				return m_klaxScriptObject;
			}
		}

		[JsonProperty]
		public int ComponentCounter { get; internal set; }

		[JsonProperty]
		private readonly List<CEntityComponent> m_components = new List<CEntityComponent>();
		private readonly Dictionary<Guid, CEntityComponent> m_guidToComponent = new Dictionary<Guid, CEntityComponent>();
		[JsonProperty]
		private readonly List<CEntity> m_children = new List<CEntity>();
		[JsonProperty]
		private CSceneComponent m_rootComponent;
		[JsonProperty]
		private CKlaxScriptObject m_klaxScriptObject = new CKlaxScriptObject();

		#region ScriptEvent

		[KlaxEvent(Category = "Basic", DisplayName = "Start")]
		[JsonIgnore]
		public CKlaxScriptEvent StartScriptEvent = new CKlaxScriptEvent();

		[KlaxEvent(Category = "Basic", DisplayName = "Update", ParameterName1 = "DeltaTime", ParameterName2 = "SourceEntity")]
		[JsonIgnore]
		public CKlaxScriptEvent<float, CEntity> UpdateScriptEvent = new CKlaxScriptEvent<float, CEntity>();

		[KlaxEvent(Category = "Input", ParameterName1 = "Button")]
		[JsonIgnore]
		public CKlaxScriptEvent<EInputButton> ButtonPressed = new CKlaxScriptEvent<EInputButton>();

		[KlaxEvent(Category = "Input", ParameterName1 = "Button")]
		[JsonIgnore]
		public CKlaxScriptEvent<EInputButton> ButtonReleased = new CKlaxScriptEvent<EInputButton>();

		private CUpdateScope m_updateScope;
		#endregion
	}
}
