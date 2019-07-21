using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KlaxCore.EditorHelper;
using KlaxCore.GameFramework.Editor;

namespace KlaxEditor.Utility
{
	static class EditorEntityUtility
	{
		public static void DetachEntityFromAllParents(SEntityId entityId)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity target = entityId.GetEntity();
				if (target == null || target.RootComponent == null || target.RootComponent.ParentComponent == null)
					return;

				SEntityComponentId oldRootParent = new SEntityComponentId(target.RootComponent.ParentComponent);

				void Do()
				{
					CWorld world = CEngine.Instance.CurrentWorld;
					CEntity entity = entityId.GetEntity();

					if (entity != null)
					{
						entity.Detach();
					}
				}

				void Undo()
				{
					CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
					{
						CWorld world = CEngine.Instance.CurrentWorld;
						CEntity entity = entityId.GetEntity();
						CSceneComponent oldParent = oldRootParent.GetComponent<CSceneComponent>();

						if (oldParent == null)
						{
							LogUtility.Log("[UndoRedo] The old parent is invalid! Undo stack has been corrupted and cleared.");
							UndoRedoUtility.Purge(null);
							return;
						}

						if (entity != null)
						{
							entity.AttachToComponent(oldParent);
						}
					});
				}

				void Redo()
				{
					CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
					{
						Do();
					});
				}

				Do();

				CRelayUndoItem item = new CRelayUndoItem(Undo, Redo);
				UndoRedoUtility.Record(item);
			});
		}

		public static void DestroyEntity(SEntityId entityId)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity entity = entityId.GetEntity();
				if (entity != null)
				{
					entity.Destroy();
				}

				UndoRedoUtility.Purge(null);
			});
		}

		public static void MakeComponentRoot(SEntityComponentId id, bool bDispatch = true)
		{
			void Command()
			{
				CSceneComponent newRoot = id.GetComponent<CSceneComponent>();
				if (newRoot == null)
					return;

				CSceneComponent oldRoot = newRoot.Owner.RootComponent;
				if (oldRoot == null)
					return;

				SEntityComponentId oldId = new SEntityComponentId(oldRoot);

				void Do()
				{
					CSceneComponent component = id.GetComponent<CSceneComponent>();
					if (component != null)
					{
						CEntity owner = component.Owner;
						owner.SetRootComponent(component);
					}
				}

				void Redo()
				{
					CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
					{
						Do();

						Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
						{
							CWorkspace space = CWorkspace.Instance;
							space.SetSelectedObject(space.SelectedEditableObject, true);
						}));
					});
				}

				void Undo()
				{
					CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
					{
						CSceneComponent component = oldId.GetComponent<CSceneComponent>();

						if (component != null)
						{
							CEntity owner = component.Owner;
							owner.SetRootComponent(component);

							Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
							{
								CWorkspace space = CWorkspace.Instance;
								space.SetSelectedObject(space.SelectedEditableObject, true);
							}));
						}
					});
				}

				Do();

				CUndoItem item = new CRelayUndoItem(Undo, Redo);
				UndoRedoUtility.Record(item);
			}

			if (bDispatch)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					Command();
				});
			}
			else
			{
				Command();
			}
		}

		public static void DestroyComponent(SEntityComponentId id)
		{
			if (id.OverrideComponent == null)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					CEntityComponent component = id.GetComponent<CEntityComponent>();
					if (component != null)
					{
						component.Destroy();
					}
				});
			}
			else
			{
				CEntityComponent component = id.GetComponent<CEntityComponent>();
				if (component != null)
				{
					component.Destroy();
				}
			}

			UndoRedoUtility.Purge(null);
		}

		public static void PickComponent(SEntityComponentId id)
		{
			CWorldOutlinerViewModel worldOutliner = CWorkspace.Instance.GetTool<CWorldOutlinerViewModel>();
			if (worldOutliner != null)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					worldOutliner.PickingComponentId.GetComponent<CScenePickingComponent>()?.Pick(id.GetComponent<CSceneComponent>());
				});
			}
		}

		public static void PickRootComponent(SEntityId entityId)
		{
			CWorldOutlinerViewModel worldOutliner = CWorkspace.Instance.GetTool<CWorldOutlinerViewModel>();
			if (worldOutliner != null)
			{
				CEntity entity = entityId.GetEntity();
				if (entity == null)
				{
					return;
				}

				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					worldOutliner.PickingComponentId.GetComponent<CScenePickingComponent>()?.Pick(entity.RootComponent);
				});
			}
		}
	}
}
