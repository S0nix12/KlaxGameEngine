using System;
using System.Diagnostics;
using KlaxCore.Core;
using KlaxShared.Attributes;
using Newtonsoft.Json;

namespace KlaxCore.GameFramework
{
	[KlaxComponent(HideInEditor = true)]
	public class CEntityComponent
	{
		private static string GetComponentName(CEntityComponent component)
		{
			return component.GetType().Name.Substring(1) + ++component.m_owner.ComponentCounter;
		}

		[JsonConstructor]
		internal CEntityComponent()
		{
			ComponentGuid = Guid.NewGuid();
		}

		internal void RegisterComponent(CEntity owner)
		{
			m_owner = owner;
			m_owner.RegisterComponent(this);

			Name = GetComponentName(this);
			Id = m_owner.ComponentCounter;

			if (ComponentGuid == Guid.Empty)
			{
				ComponentGuid = Guid.NewGuid();
			}
		}

		public virtual void Init()
		{
		}

		public virtual void Start()
		{
		}

		public virtual void Shutdown()
		{
		}

		[KlaxFunction(Category = "Component", Tooltip = "Destroy this component")]
		public virtual void Destroy()
		{
			MarkedForDestruction = true;

			if (World != null)
			{
				World.UpdateScheduler.ConnectOneTimeUpdate((delta) =>
				{
					Shutdown();
					Owner.UnregisterComponent(this);

				}, EUpdatePriority.Latest);
			}
			else
			{
				Shutdown();
				Owner.UnregisterComponent(this);
			}
		}

		[KlaxFunction(Category = "Component", Tooltip = "Change the owner of this component")]
		public virtual void ChangeOwner(CEntity newOwner)
		{
			Debug.Assert(m_owner != null);
			m_owner.UnregisterComponent(this);
			newOwner.RegisterComponent(this);
			m_owner = newOwner;
		}

		public bool ShowInInspector { get; protected set; } = true;
		public bool MarkedForDestruction { get; protected set; }
		public string Name { get; set; }

		[JsonProperty]
		protected CEntity m_owner;
		[JsonIgnore]
		public CEntity Owner
		{
			get { return m_owner; }
		}

		[JsonProperty]
		public Guid ComponentGuid { get; private set; }

		[JsonIgnore]
		public CWorld World
		{
			get { return m_owner.World; }
		}

		[JsonProperty]
		public int Id { get; private set; }
	}

	public struct SEntityId
	{
		public readonly static SEntityId Invalid = new SEntityId(0);

		public SEntityId(int entityId)
		{
			EntityId = entityId;
			OverrideEntity = null;
		}

		public SEntityId(CEntity entity, bool bOverrideEntity = false)
		{
			if (bOverrideEntity)
			{
				OverrideEntity = entity;
				EntityId = 0;
			}
			else
			{
				EntityId = entity.Id;
				OverrideEntity = null;
			}
		}

		public CEntity GetEntity()
		{
			return OverrideEntity ?? CEngine.Instance?.CurrentWorld?.GetEntityById(EntityId);
		}

		public static bool operator ==(SEntityId lhs, SEntityId rhs)
		{
			return lhs.OverrideEntity == rhs.OverrideEntity && lhs.EntityId == rhs.EntityId;
		}

		public static bool operator !=(SEntityId lhs, SEntityId rhs)
		{
			return !(lhs == rhs);
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + EntityId.GetHashCode();
			hash = hash * 31 + (OverrideEntity != null ? OverrideEntity.GetHashCode() : 0);
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is SEntityId id)
			{
				return this == id;
			}

			return false;
		}

		public CEntity OverrideEntity { get; }
		public int EntityId { get; }
	}

	public struct SEntityComponentId
	{
		public readonly static SEntityComponentId Invalid = new SEntityComponentId(0, 0);

		public SEntityComponentId(int entityId, int componentId)
		{
			EntityId = new SEntityId(entityId);
			ComponentId = componentId;
			OverrideComponent = null;
		}

		public SEntityComponentId(CEntityComponent component, bool bOverrideComponent = false)
		{
			if (bOverrideComponent)
			{
				OverrideComponent = component;
				EntityId = SEntityId.Invalid;
				ComponentId = 0;
			}
			else
			{
				EntityId = new SEntityId(component.Owner.Id);
				ComponentId = component.Id;
				OverrideComponent = null;
			}
		}

		public CEntityComponent GetComponent()
		{
			return OverrideComponent ?? GetEntity()?.GetComponentById<CEntityComponent>(ComponentId);
		}

		public T GetComponent<T>() where T : CEntityComponent
		{
			return GetComponent() as T;
		}

		public CEntity GetEntity()
		{
			return EntityId.GetEntity();
		}

		public static bool operator ==(SEntityComponentId lhs, SEntityComponentId rhs)
		{
			return lhs.ComponentId == rhs.ComponentId && lhs.EntityId == rhs.EntityId && lhs.OverrideComponent == rhs.OverrideComponent;
		}

		public static bool operator !=(SEntityComponentId lhs, SEntityComponentId rhs)
		{
			return !(lhs == rhs);
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + EntityId.GetHashCode();
			hash = hash * 31 + ComponentId.GetHashCode();
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is SEntityComponentId id)
			{
				return this == id;
			}

			return false;
		}

		public SEntityId EntityId { get; }
		public int ComponentId { get; }

		public CEntityComponent OverrideComponent { get; }

	}
}
