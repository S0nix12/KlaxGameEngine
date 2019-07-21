using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxShared.Attributes;
using Newtonsoft.Json;
using SharpDX;

namespace KlaxCore.GameFramework
{
	[KlaxComponent(Category = "Common")]
	public class CSceneComponent : CEntityComponent
    {
		public override void Destroy()
		{
			MarkedForDestruction = true;
			for (int i = 0, count = m_children.Count; i < count; i++)
			{
				m_children[i].Destroy();
			}

			if (World != null)
			{
				World.UpdateScheduler.ConnectOneTimeUpdate((delta) =>
				{
					Shutdown();
					Owner.UnregisterComponent(this);
					Detach();

				}, EUpdatePriority.Latest);
			}
			else
			{
				Shutdown();
				Owner.UnregisterComponent(this);
				Detach();
			}
		}

		[KlaxFunction(Category = "SceneComponent", Tooltip = "Attach this component to the given scene component, optionally transferring component ownership if the target does not belong to the current owning entity")]
		public bool AttachToComponent(CSceneComponent parentComponent, bool bTransferOwnership = false)
        {
            if (parentComponent == this)
            {
                LogUtility.Log("[Components] Tried to attach to self, aborting");
                return false;
			}

			if (parentComponent == null)
			{
				Detach();
				return false;
			}

			if (parentComponent == m_parentComponent)
			{
				LogUtility.Log("[Components] Tried attaching component to the same parent again, aborting");
				return false;
			}

			if (parentComponent.IsChildOf(this))
            {
                LogUtility.Log("[Components] Tried to attach to a component which is already a child, this would create a loop, aborting");
            	return false;
			}

			Detach();

            CSceneComponent oldParent = m_parentComponent;
            m_parentComponent = parentComponent;
            m_transform.Parent = m_parentComponent.m_transform;
            m_parentComponent.m_children.Add(this);


            // todo henning handle different transform modes (Keep WorldTransform, Snap To etc.)

            if (m_parentComponent.Owner != Owner)
            {
                if (bTransferOwnership)
                {
                    ChangeOwner(m_parentComponent.Owner);
                }
                else
                {
					CEntity currentParent = Owner.Parent;
					if (currentParent != null)
					{
						currentParent.UnregisterChildEntity(Owner);
					}

                    parentComponent.Owner.RegisterChildEntity(Owner);
                    World.TriggerHierarchyChanged(this, oldParent, parentComponent);
                }
            }

			return true;
		}

		[KlaxFunction(Category = "SceneComponent", Tooltip = "Detach this component from it's parent")]
		public void Detach()
		{
            if (m_parentComponent != null)
            {
                m_parentComponent.m_children.Remove(this);
				m_parentComponent.Owner.UnregisterChildEntity(Owner);
				World?.TriggerHierarchyChanged(this, m_parentComponent, null);
            }

            m_parentComponent = null;
            m_transform.Parent = null;
        }

		/// <summary>
		/// Traverses the parent-child hierarchy to see whether this component ultimately is a child of given component
		/// </summary>
		[KlaxFunction(Category = "SceneComponent", Tooltip = "Returns if this component is a child of the given component")]
		public bool IsChildOf(CSceneComponent parentComponent)
		{
			CSceneComponent parent = m_parentComponent;
			while (parent != null)
			{
				if (parent == parentComponent)
				{
					return true;
				}

				parent = parent.m_parentComponent;
			}

			return false;
		}

        public override void ChangeOwner(CEntity newOwner)
		{
            base.ChangeOwner(newOwner);
            foreach (CSceneComponent child in m_children)
            {
                child.ChangeOwner(newOwner);
            }
        }

		[KlaxFunction(Category = "Transformation", Tooltip = "Set the location of this component in world space")]
        public void SetWorldPosition(in Vector3 worldPosition)
		{
            m_transform.SetWorldPosition(in worldPosition);
        }

		[KlaxFunction(Category = "Transformation", Tooltip = "Set the rotation of this component in world space")]
        public void SetWorldRotation(in Quaternion worldRotation)
		{
            m_transform.SetWorldRotation(in worldRotation);
        }

		[KlaxFunction(Category = "Transformation", Tooltip = "Set the scale of this component in world space")]
        public void SetWorldScale(in Vector3 worldScale)
		{
            m_transform.SetWorldScale(in worldScale);
        }

		[JsonIgnore]
        public Vector3 WorldPosition
		{
            get { return m_transform.WorldPosition; }
        }
		[JsonIgnore]
        public Quaternion WorldRotation
		{
            get { return m_transform.WorldRotation; }
        }
		[JsonIgnore]
        public Vector3 WorldScale
		{
            get { return m_transform.WorldScale; }
        }
       [KlaxProperty(Category = "Transform", CategoryPriority = -1, DisplayName = "Position")]
		[JsonIgnore]
        public Vector3 LocalPosition
		{
            get { return m_transform.Position; }
            set { m_transform.Position = value; }
        }
       [KlaxProperty(Category = "Transform", CategoryPriority = -1, DisplayName = "Rotation")]
		[JsonIgnore]
        public Quaternion LocalRotation
		{
            get { return m_transform.Rotation; }
            set { m_transform.Rotation = value; }
        }
       [KlaxProperty(Category = "Transform", CategoryPriority = -1, DisplayName = "Scale")]
		[JsonIgnore]
        public Vector3 LocalScale
		{
            get { return m_transform.Scale; }
            set { m_transform.Scale = value; }
        }
		[JsonIgnore]
        public Vector3 Forward
		{
            get { return m_transform.Forward; }
        }
		[JsonIgnore]
        public Vector3 Right
		{
            get { return m_transform.Right; }
        }
		[JsonIgnore]
        public Vector3 Up
		{
            get { return m_transform.Up; }
        }

		[JsonProperty]
        protected Transform m_transform = new Transform();
		[JsonIgnore]
		internal Transform Transform
		{
			get { return m_transform; }
		}


		[JsonProperty]
        private CSceneComponent m_parentComponent = null;
		[JsonIgnore]
		public CSceneComponent ParentComponent
		{
            get { return m_parentComponent; }
        }
        
		[JsonProperty]
        private readonly List<CSceneComponent> m_children = new List<CSceneComponent>();
		[JsonIgnore]
        public ReadOnlyCollection<CSceneComponent> Children
		{
            get { return new ReadOnlyCollection<CSceneComponent>(m_children); }
        }
    }
}
