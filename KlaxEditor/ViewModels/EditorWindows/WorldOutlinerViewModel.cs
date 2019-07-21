using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Editor;
using KlaxEditor;
using KlaxEditor.ViewModels;
using KlaxEditor.Utility;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxEditor.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KlaxCore.EditorHelper;
using KlaxCore.GameFramework.Assets;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.ViewModels.EditorWindows.Interfaces;
using KlaxIO.AssetManager.Assets;

namespace KlaxEditor.ViewModels
{
	class COutlinerEntityViewModel : CViewModelBase
	{
		public COutlinerEntityViewModel(CWorldOutlinerViewModel vm, SEntityId entityId, string name, COutlinerEntityViewModel parent, int index)
		{
			EntityId = entityId;
			m_viewModel = vm;
			m_name = name;
			Parent = parent;
			Index = index;

			InitCommands();
		}

		private void InitCommands()
		{
			DeleteCommand = new CRelayCommand(OnDeleteCommand);
			DetachCommand = new CRelayCommand(OnDetachCommand);
			DragEnterCommand = new CRelayCommand(OnDragEnter);
			DragOverCommand = new CRelayCommand(OnDragOver);
			DropCommand = new CRelayCommand(OnDrop);
			CreateEntityAssetCommand = new CRelayCommand(OnCreateAssetFromEntity);
		}

		private void OnCreateAssetFromEntity(object e)
		{
			// We need the asset browser to get the active path where we want to save the entity asset
			CAssetBrowserViewModel assetBrowser = CWorkspace.Instance.GetTool<CAssetBrowserViewModel>();
			string assetPath = assetBrowser.ActiveDirectory;
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity entity = EntityId.GetEntity();
				CEntityAsset<CEntity>.CreateFromEntity(entity, assetPath);
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
				{
					assetBrowser.UpdateShownAssets();
				}));
			});
		}

		private void OnDetachCommand(object argument)
		{
			EditorEntityUtility.DetachEntityFromAllParents(EntityId);
		}

		private void OnDeleteCommand(object arg)
		{
			EditorEntityUtility.DestroyEntity(EntityId);
		}

		private void OnDragEnter(object e)
		{
			DragEventArgs args = (DragEventArgs) e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent("entity") && args.Data.GetData("entity") != this)
			{
				args.Effects = DragDropEffects.Move;
			}

			args.Handled = true;
		}

		private void OnDragOver(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent("entity") && args.Data.GetData("entity") != this)
			{
				args.Effects = DragDropEffects.Move;
			}

			args.Handled = true;
		}

		private void OnDrop(object e)
		{
			DragEventArgs args = (DragEventArgs) e;
			if (args.Data.GetDataPresent("entity") && args.Data.GetData("entity") != this)
			{
				COutlinerEntityViewModel otherEntity = args.Data.GetData("entity") as COutlinerEntityViewModel;
				if (otherEntity != null && otherEntity != this)
				{
					m_viewModel.AttachEntities(otherEntity.EntityId, EntityId);
				}
			}
		}

		public void MarkInOutliner(bool bMark)
		{
			if (bMark != m_bIsSelected)
			{
				m_bIsSelected = bMark;
				m_viewModel.SelectedEntityViewModel = bMark ? this : null;
				RaisePropertyChanged(nameof(IsSelected));
			}
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<COutlinerEntityViewModel> m_children = new ObservableCollection<COutlinerEntityViewModel>();
		public ObservableCollection<COutlinerEntityViewModel> Children
		{
			get { return m_children; }
			set
			{
				m_children = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bIsExpanded;
		public bool IsExpanded
		{
			get { return m_bIsExpanded; }
			set
			{
				m_bIsExpanded = value;
				RaisePropertyChanged();

				if (IsSelected && Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					void Expand(COutlinerEntityViewModel vm)
					{
						vm.IsExpanded = m_bIsExpanded;

						foreach (var child in Children)
						{
							child.IsExpanded = m_bIsExpanded;
						}
					}

					foreach (var child in Children)
					{
						Expand(child);
					}
				}
			}
		}

		private bool m_bIsSelected;
		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				if (m_bIsSelected != value)
				{
					m_bIsSelected = value;

					if (value)
					{
						m_viewModel.SelectedEntityViewModel = this;
					}
					else if (m_viewModel.SelectedEntityViewModel == this)
					{
						m_viewModel.SelectedEntityViewModel = null;
					}

					CWorkspace workspace = CWorkspace.Instance;
					if (m_bIsSelected)
					{
						CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
						{
							CEntity entity = EntityId.GetEntity();
							CSceneComponent root = entity.RootComponent;

							if (root == null)
							{
								Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
								{
									workspace.SetSelectedObject(new CEditableObject(EntityId));
								}));
							}
							else
							{
								Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
								{
									workspace.SetSelectedObject(new CEditableObject(new SEntityComponentId(root)));
								}));
							}
						});
					}

					RaisePropertyChanged();
				}
			}
		}

		public COutlinerEntityViewModel Parent { get; private set; }
		public int Index { get; private set; }
		public SEntityId EntityId { get; }
		public ICommand DeleteCommand { get; private set; }
		public ICommand DetachCommand { get; private set; }
		public ICommand DragEnterCommand { get; private set; }
		public ICommand DragOverCommand { get; private set; }
		public ICommand DropCommand { get; private set; }
		public ICommand CreateEntityAssetCommand { get; private set; }

		private CWorldOutlinerViewModel m_viewModel;
	}

	class CWorldOutlinerViewModel : CEditorWindowViewModel
	{
		public CWorldOutlinerViewModel()
			: base("World Outliner")
		{
			SetIconSourcePath("Resources/Images/Tabs/outliner.png");

			Content = new WorldOutliner();
			m_view = Content as IOutlinerView;

			CWorkspace.Instance.OnSelectedEditableObjectChanged += (oldObj, newObj) =>
			{
				if (newObj != null)
				{
					switch (newObj.Type)
					{
						case CEditableObject.EObjectType.Entity:
							HighlightEntity(newObj.EntityId);
							break;
						case CEditableObject.EObjectType.Component:
							HighlightEntity(newObj.ComponentId.EntityId);
							break;
					}
				}
				else
				{
					SelectedEntityViewModel?.MarkInOutliner(false);
				}
			};

			KeyDownCommand = new CRelayCommand(param =>
			{
				KeyEventArgs args = (KeyEventArgs)param;
				if (IsActive && args.Key == Key.Delete)
				{
					if (SelectedEntityViewModel != null)
					{
						EditorEntityUtility.DestroyEntity(SelectedEntityViewModel.EntityId);
					}
				}
			});
		}

		public override void PostWorldLoad()
		{
			base.PostWorldLoad();

			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				Action<CEntity> callback = (entity) =>
				{
					CHierarchyEntry root = EditorHelpers.FillLevelHierarchy();
					Application.Current.Dispatcher.Invoke(() =>
					{
						UpdateOutliner(root);
					});
				};

				Action<CAssetReference<CLevelAsset>, CLevel> levelChanged = (levelAsset, level) =>
				{
					CHierarchyEntry root = EditorHelpers.FillLevelHierarchy();
					SEntityComponentId newPickerId = SpawnScenePicker_EngineThread(CEngine.Instance.CurrentWorld);
					CWorkspace.Instance.PostLevelLoad(levelAsset);
					Application.Current.Dispatcher.Invoke(() =>
					{
						PickingComponentId = newPickerId;
						UpdateOutliner(root);
					});
				};

				CWorld currentWorld = CEngine.Instance.CurrentWorld;
				currentWorld.OnEntitySpawned += callback;
				currentWorld.OnEntityDestroyed += callback;
				currentWorld.OnEntityRevived += callback;
				currentWorld.OnLevelChanged += levelChanged;
				currentWorld.OnHierarchyChanged += (child, oldParent, newParent) =>
				{
					if (child == child.Owner?.RootComponent)
					{
						callback(null);
					}
				};
				SEntityComponentId pickerId = SpawnScenePicker_EngineThread(currentWorld);
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() => { PickingComponentId = pickerId; }));

				CScenePickingComponent.OnComponentPicked += (component) =>
				{
					if (component == null)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							CWorkspace.Instance.SetSelectedObject(null);
						});
					}
					else
					{
						CEntity entity = component.Owner;
						CEditableObject editObj = null;

						if (entity.RootComponent == null)
						{
							editObj = new CEditableObject(new SEntityId(entity.Id));
						}
						else
						{
							editObj = new CEditableObject(new SEntityComponentId(entity.RootComponent));
						}

						Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
						{
							CWorkspace.Instance.SetSelectedObject(editObj);
						}));
					}
				};

				callback(null);
			});
		}

		public SEntityComponentId SpawnScenePicker_EngineThread(CWorld world)
		{
			CScenePickerEntity scenePicker = world.LoadedLevel.GetEntity<CScenePickerEntity>();
			if (scenePicker == null)
			{
				scenePicker = world.SpawnEntity<CScenePickerEntity>();
			}
			return new SEntityComponentId(scenePicker.ScenePickingComponent);
		}

		public void HighlightEntity(SEntityId entityId)
		{
			bool CheckEntity(COutlinerEntityViewModel vm)
			{
				if (vm.EntityId == entityId)
				{
					vm.MarkInOutliner(true);
					return true;
				}

				for (int i = 0, count = vm.Children.Count; i < count; i++)
				{
					if (CheckEntity(vm.Children[i]))
					{
						vm.IsExpanded = true;
						return true;
					}
				}

				return false;
			}

			for (int i = 0, count = m_entities.Count; i < count; i++)
			{
				COutlinerEntityViewModel entry = m_entities[i];
				if (CheckEntity(entry))
				{
					if (entry.Children.Count > 0)
					{
						entry.IsExpanded = true;
					}
					return;
				}
			}
		}

		public void SetSelectedEntity(SEntityId entityId)
		{
			bool CheckEntity(COutlinerEntityViewModel vm)
			{
				if (vm.EntityId == entityId)
				{
					vm.IsSelected = true;
					return true;
				}

				for (int i = 0, count = vm.Children.Count; i < count; i++)
				{
					if (CheckEntity(vm.Children[i]))
					{
						vm.IsExpanded = true;
						return true;
					}
				}

				return false;
			}

			for (int i = 0, count = m_entities.Count; i < count; i++)
			{
				COutlinerEntityViewModel entry = m_entities[i];
				if (CheckEntity(entry))
				{
					if (entry.Children.Count > 0)
					{
						entry.IsExpanded = true;
					}
					return;
				}
			}
		}

		public void UpdateOutliner(CHierarchyEntry root)
		{
			void AddOutlinerEntity(CHierarchyEntry entity, COutlinerEntityViewModel parent)
			{
				COutlinerEntityViewModel viewModel = new COutlinerEntityViewModel(this, new SEntityId(entity.EntityId), entity.Label, parent, -1);
				parent.Children.Add(viewModel);

				foreach (var child in entity.Children)
				{
					AddOutlinerEntity(child, viewModel);
				}
			}

			m_entities.Clear();

			for (int i = 0, count = root.Children.Count; i < count; i++)
			{
				CHierarchyEntry child = root.Children[i];

				COutlinerEntityViewModel rootEntity = new COutlinerEntityViewModel(this, new SEntityId(child.EntityId), child.Label, null, i);
				foreach (var hierarchyChild in child.Children)
				{
					AddOutlinerEntity(hierarchyChild, rootEntity);
				}
				m_entities.Add(rootEntity);
			}

			CWorkspace.Instance.SetSelectedObject(CWorkspace.Instance.SelectedEditableObject, true);
		}

		internal void AttachEntities(SEntityId child, SEntityId parent)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity childEntity = child.GetEntity();
				CEntity parentEntity = parent.GetEntity();

				if (childEntity != null && parentEntity != null)
				{
					CEntity oldParent = childEntity.Parent;
					bool bOldParentExists = oldParent != null;
					SEntityId oldParentId = bOldParentExists ? new SEntityId(oldParent.Id) : SEntityId.Invalid;

					if (parentEntity.RootComponent != null && childEntity.RootComponent != null)
					{
						if (!parentEntity.RootComponent.IsChildOf(childEntity.RootComponent))
						{
							childEntity.AttachToEntity(parentEntity);

							void Undo()
							{
								CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
								{
									CEntity childInst = child.GetEntity();

									if (childInst != null)
									{
										if (bOldParentExists)
										{
											CEntity oldParentInst = oldParentId.GetEntity();
											if (oldParentInst != null && childInst != null)
											{
												childInst.AttachToEntity(oldParentInst);
											}
										}
										else
										{
											childInst.Detach();
										}
									}
								});
							}

							void Redo()
							{
								CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
								{
									CEntity newParentInst = parent.GetEntity();
									CEntity childInst = child.GetEntity();

									if (newParentInst != null && childInst != null)
									{
										childInst.AttachToEntity(newParentInst);
									}
								});
							}

							CRelayUndoItem item = new CRelayUndoItem(Undo, Redo);
							UndoRedoUtility.Record(item);
						}
					}
				}
			});
		}

		private ObservableCollection<COutlinerEntityViewModel> m_entities = new ObservableCollection<COutlinerEntityViewModel>();
		public ObservableCollection<COutlinerEntityViewModel> Entities
		{
			get { return m_entities; }
			set
			{
				m_entities = value;
				RaisePropertyChanged();
			}
		}

		private COutlinerEntityViewModel m_selectedEntityViewModel;
		public COutlinerEntityViewModel SelectedEntityViewModel
		{
			get { return m_selectedEntityViewModel; }
			set
			{
				m_selectedEntityViewModel = value;

				if (SelectedEntityViewModel != null)
				{
					COutlinerEntityViewModel vm = SelectedEntityViewModel;
					while (vm.Parent != null)
					{
						vm = vm.Parent;
					}

					m_view.ScrollToEntity(vm.Index);
				}
			}
		}

		public SEntityComponentId PickingComponentId { get; private set; }

		public ICommand KeyDownCommand { get; }

		private IOutlinerView m_view;
	}
}
