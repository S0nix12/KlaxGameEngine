using KlaxCore.GameFramework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace KlaxCore.Core
{
	public class CLevel : CWorldObject
	{
		public override void Init(CWorld world, object userData)
		{
			base.Init(world, userData);
			foreach (CEntity entity in m_entities)
			{
				World.InitWithWorld(entity, false);
				if (entity.Parent == null)
				{
					m_childEntities.Add(entity);
				}
			}

			World.OnHierarchyChanged += HandleChildEntities;
		}

		public void Shutdown()
		{
			foreach (CEntity entity in m_entities)
			{
				World.DestroyEntity(entity, false);
			}

			m_entities.Clear();
			m_childEntities.Clear();

			World.OnHierarchyChanged -= HandleChildEntities;
		}

		public void AddEntity(CEntity entity)
		{
			m_entities.Add(entity);

			if (entity.Parent == null)
			{
				m_childEntities.Add(entity);
			}

			if (IsPlaying)
			{
				entity.Start();
			}
		}

		public void RemoveEntity(CEntity entity)
		{
			m_entities.Remove(entity);
			m_childEntities.Remove(entity);
		}

		public T GetEntity<T>() where T : CEntity
		{
			foreach (CEntity entity in m_entities)
			{
				if (entity is T outEntity)
				{
					return outEntity;
				}
			}

			return null;
		}

		public void LoadFromFile(string path)
		{

		}

		public void SaveToFile(string path)
		{
			for (int i = 0; i < m_entities.Count; i++)
			{
			}
		}

		private void HandleChildEntities(CSceneComponent childComponent, CSceneComponent oldParentComponent, CSceneComponent newParentComponent)
		{
			if (childComponent == childComponent.Owner.RootComponent)
			{
				if (oldParentComponent == null)
				{
					m_childEntities.Remove(childComponent.Owner);
				}
				else if (newParentComponent == null)
				{
					m_childEntities.Add(childComponent.Owner);
				}
			}
		}

		[JsonIgnore]
		public ReadOnlyCollection<CEntity> ChildEntities { get { return new ReadOnlyCollection<CEntity>(m_childEntities); } }

		[JsonProperty]
		private List<CEntity> m_entities = new List<CEntity>(128);
		private List<CEntity> m_childEntities = new List<CEntity>(128);

		private bool m_bIsPlaying;
		[JsonIgnore]
		public bool IsPlaying
		{
			get { return m_bIsPlaying; }
			set
			{
				if (m_bIsPlaying != value)
				{
					m_bIsPlaying = value;
					if (m_bIsPlaying)
					{
						List<CEntity> entitiesCopy = new List<CEntity>(m_entities);
						for (int i = 0, count = entitiesCopy.Count; i < count; i++)
						{
							CEntity entity = entitiesCopy[i];
							if (!entity.MarkedForDestruction)
							{
								entity.Start();
							}
						}
					}
				}
			}
		}
	}
}
