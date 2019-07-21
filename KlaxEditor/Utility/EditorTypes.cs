using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxShared.Utilities;

namespace KlaxEditor.Utility
{
	public class CEditableObject
	{
		public enum EObjectType
		{
			Entity,
			Component
		}

		public CEditableObject(SEntityId entityId)
		{
			m_entityId = entityId;
			Type = EObjectType.Entity;
		}

		public CEditableObject(SEntityComponentId componentId)
		{
			m_componentId = componentId;
			Type = EObjectType.Component;
		}

		public SEntityId GetTargetEntityId()
		{
			switch (Type)
			{
				case EObjectType.Entity:
					return EntityId;
				case EObjectType.Component:
					return ComponentId.EntityId;
			}

			return SEntityId.Invalid;
		}

		public object GetEngineObject_EngineThread()
		{
			switch (Type)
			{
				case EObjectType.Entity:
					return m_entityId.GetEntity();
				case EObjectType.Component:
					return m_componentId.GetComponent();
			}

			return null;
		}

		public override bool Equals(object obj)
		{
			if (obj is CEditableObject editableObj)
			{
				return m_entityId == editableObj.m_entityId && m_componentId == editableObj.m_componentId;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return ContainerUtilities.CombineHashes(m_entityId, m_componentId);
		}

		public static bool operator ==(CEditableObject b, CEditableObject c)
		{
			return ((ReferenceEquals(b, null) && ReferenceEquals(c, null)) || (!ReferenceEquals(b, null) && b.Equals(c)));
		}

		public static bool operator !=(CEditableObject b, CEditableObject c)
		{
			return !(b == c);
		}

		public static bool operator ==(CEditableObject b, SEntityId c)
		{
			return b?.Type == EObjectType.Entity && b?.m_entityId == c;
		}

		public static bool operator !=(CEditableObject b, SEntityId c)
		{
			return !(b == c);
		}

		public static bool operator ==(CEditableObject b, SEntityComponentId c)
		{
			return !ReferenceEquals(b, null) && b.Type == EObjectType.Component && b.m_componentId == c;
		}

		public static bool operator !=(CEditableObject b, SEntityComponentId c)
		{
			return !(b == c);
		}
		
		public EObjectType Type { get; }
		public SEntityId EntityId { get { return m_entityId; } }
		public SEntityComponentId ComponentId { get { return m_componentId; } }

		private readonly SEntityId m_entityId;
		private readonly SEntityComponentId m_componentId;
	}
}
