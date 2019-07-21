using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxEditor.UserControls.InspectorControls;
using KlaxEditor.Utility;
using KlaxEditor.Views;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KlaxCore.EditorHelper;
using KlaxCore.GameFramework.Editor;
using KlaxShared.Utilities;
using KlaxEditor.Utility.UndoRedo;
using KlaxCore.GameFramework.Assets;
using System.Windows.Threading;
using KlaxCore.KlaxScript;
using KlaxShared.Attributes;
using System.Reflection;

namespace KlaxEditor.ViewModels.EditorWindows
{
	interface IInspectorViewModel
	{
		void QueueEntityInformationUpdate(SEntityId entityId, bool bReselectTarget);
		void AttachComponent(SEntityComponentId childComponent, SEntityComponentId parentComponent);
	}

	class CInspectorBaseViewModel<T> : CViewModelBase where T : CInspectorBaseViewModel<T>
	{
		public static bool NOTIFY_SELECTION_EVENTS = true;

		public CInspectorBaseViewModel(IInspectorViewModel viewModel, string name)
		{
			m_viewModel = viewModel;
			m_name = name;
		}

		public void SetViewModelMarked(bool bMarked)
		{
			if (m_bIsSelected != bMarked)
			{
				m_bIsSelected = bMarked;
				RaisePropertyChanged(nameof(IsSelected));
			}
		}

		protected virtual void OnSelectedChanged(bool bNewValue)
		{

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

		private ObservableCollection<T> m_children = new ObservableCollection<T>();
		public ObservableCollection<T> Children
		{
			get { return m_children; }
			set
			{
				m_children = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bIsExpanded = true;
		public bool IsExpanded
		{
			get { return m_bIsExpanded; }
			set
			{
				m_bIsExpanded = value;
				RaisePropertyChanged();

				if (IsSelected && Keyboard.IsKeyDown(Key.LeftCtrl))
				{
					void Expand(T vm)
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
					OnSelectedChanged(value);

					RaisePropertyChanged();
				}
			}
		}

		protected readonly IInspectorViewModel m_viewModel;
	}

	class CInspectorSceneComponentViewModel : CInspectorBaseViewModel<CInspectorSceneComponentViewModel>
	{
		public CInspectorSceneComponentViewModel(IInspectorViewModel viewModel, string name, SEntityComponentId componentId, string dragIdentifier)
			: base(viewModel, name)
		{
			ComponentId = componentId;

			MakeRootCommand = new CRelayCommand(OnMakeRoot);
			DeleteComponentCommand = new CRelayCommand(OnDeleteComponent);
			DragEnterCommand = new CRelayCommand(OnDragEnter);
			DragOverCommand = new CRelayCommand(OnDragOver);
			DropCommand = new CRelayCommand(OnDrop);

			m_dragIdentifier = dragIdentifier;
		}

		protected virtual void OnDeleteComponent(object argument)
		{
			EditorEntityUtility.DestroyComponent(ComponentId);

			m_viewModel.QueueEntityInformationUpdate(ComponentId.EntityId, true);
		}

		protected virtual void OnMakeRoot(object argument)
		{
			EditorEntityUtility.MakeComponentRoot(ComponentId);

			m_viewModel.QueueEntityInformationUpdate(ComponentId.EntityId, true);
		}

		protected override void OnSelectedChanged(bool bNewValue)
		{
			CWorkspace workspace = CWorkspace.Instance;
			if (bNewValue)
			{
				workspace.SetSelectedObject(new CEditableObject(ComponentId));
			}
			else
			{
				if (workspace.SelectedEditableObject == ComponentId)
				{
					workspace.SetSelectedObject(null);
				}
			}
		}

		private void OnDragEnter(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent(m_dragIdentifier))
			{
				CInspectorSceneComponentViewModel otherComponent = (CInspectorSceneComponentViewModel)args.Data.GetData(m_dragIdentifier);
				if (otherComponent.ComponentId != ComponentId)
				{
					args.Effects = DragDropEffects.Move;
				}
			}

			args.Handled = true;
		}

		private void OnDragOver(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent(m_dragIdentifier))
			{
				CInspectorSceneComponentViewModel otherComponent = (CInspectorSceneComponentViewModel)args.Data.GetData(m_dragIdentifier);
				if (otherComponent.ComponentId != ComponentId)
				{
					args.Effects = DragDropEffects.Move;
				}
			}

			args.Handled = true;
		}

		private void OnDrop(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			if (args.Data.GetDataPresent(m_dragIdentifier) && args.Data.GetData(m_dragIdentifier) != this)
			{
				CInspectorSceneComponentViewModel otherComponent = args.Data.GetData(m_dragIdentifier) as CInspectorSceneComponentViewModel;
				if (otherComponent != null && otherComponent.ComponentId != ComponentId)
				{
					m_viewModel.AttachComponent(otherComponent.ComponentId, ComponentId);
				}
			}
		}

		public SEntityComponentId ComponentId { get; }

		public ICommand MakeRootCommand { get; private set; }
		public ICommand DeleteComponentCommand { get; private set; }
		public ICommand DragEnterCommand { get; set; }
		public ICommand DragOverCommand { get; set; }
		public ICommand DropCommand { get; set; }

		private string m_dragIdentifier;
	}

	class CInspectorEntityComponentViewModel : CInspectorBaseViewModel<CInspectorEntityComponentViewModel>
	{
		public CInspectorEntityComponentViewModel(IInspectorViewModel viewModel, string name, SEntityComponentId componentId)
			: base(viewModel, name)
		{
			ComponentId = componentId;

			DeleteComponentCommand = new CRelayCommand(arg =>
			{
				EditorEntityUtility.DestroyComponent(ComponentId);

				m_viewModel.QueueEntityInformationUpdate(ComponentId.EntityId, true);
			});
		}

		protected override void OnSelectedChanged(bool bNewValue)
		{
			CWorkspace workspace = CWorkspace.Instance;
			if (bNewValue)
			{
				workspace.SetSelectedObject(new CEditableObject(ComponentId));
			}
			else
			{
				if (workspace.SelectedEditableObject == ComponentId)
				{
					workspace.SetSelectedObject(null);
				}
			}
		}

		public ICommand DeleteComponentCommand { get; }

		public SEntityComponentId ComponentId { get; }
	}

	class CInspectorEntityViewModel : CInspectorBaseViewModel<CInspectorEntityViewModel>
	{
		public CInspectorEntityViewModel(IInspectorViewModel vm, string name, SEntityId entityId)
			: base(vm, name)
		{
			EntityId = entityId;
		}

		protected override void OnSelectedChanged(bool bNewValue)
		{
			CWorkspace workspace = CWorkspace.Instance;
			if (bNewValue)
			{
				workspace.SetSelectedObject(new CEditableObject(EntityId));
			}
			else
			{
				if (workspace.SelectedEditableObject == EntityId)
				{
					workspace.SetSelectedObject(null);
				}
			}
		}

		public SEntityId EntityId { get; }
	}

	class CAddComponentCategoryViewModel : CViewModelBase
	{
		public CAddComponentCategoryViewModel(string name, List<CAddComponentEntryViewModel> componentTypes)
		{
			m_name = name;
			m_componentTypes = new ObservableCollection<CAddComponentEntryViewModel>(componentTypes);
		}

		private ObservableCollection<CAddComponentEntryViewModel> m_componentTypes;
		public ObservableCollection<CAddComponentEntryViewModel> ComponentTypes
		{
			get { return m_componentTypes; }
			set
			{
				m_componentTypes = value;
				RaisePropertyChanged();
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
	}

	class CAddComponentEntryViewModel : CViewModelBase
	{
		public CAddComponentEntryViewModel(string name, Type type)
		{
			m_name = name;
			m_type = type;
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

		private Type m_type;
		public Type Type
		{
			get { return m_type; }
			set
			{
				m_type = value;
				RaisePropertyChanged();
			}
		}
	}

	class CEntityCommandViewModel : CViewModelBase
	{
		public CEntityCommandViewModel(string name, ICommand command)
		{
			m_name = name;
			Command = command;
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

		public ICommand Command { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	class CInspectorViewModel : CEditorWindowViewModel, IInspectorViewModel
	{
		public CInspectorViewModel()
			: base("Inspector")
		{
			SetIconSourcePath("Resources/Images/Tabs/inspector.png");

			Content = new Inspector();
			m_view = Content as IInspectorView;

			InitializeEntityMenues();

			RenameEntityCommand = new CRelayCommand(InternalRenameEntity);
			AddComponentCommand = new CRelayCommand(InternalAddComponent);
			ToggleAddComponentMenuCommand = new CRelayCommand((obj) => AddComponentMenuOpen = !AddComponentMenuOpen);
			ToggleEntityCommandsMenuCommand = new CRelayCommand((obj) => EntityCommandsMenuOpen = !EntityCommandsMenuOpen);

			CWorkspace.Instance.OnSelectedEditableObjectChanged += (oldObj, newObj) =>
			{
				m_view?.ClearInspector();
				m_view?.LockInspector(true);

				if (newObj != null)
				{
					m_view.SetInspectorVisible(true);

					switch (newObj.Type)
					{
						case CEditableObject.EObjectType.Entity:
							InspectEntity(newObj.EntityId);
							if (oldObj?.GetTargetEntityId() != newObj?.GetTargetEntityId())
							{
								QueueEntityInformationUpdate(newObj.GetTargetEntityId(), true);
							}

							EditorEntityUtility.PickRootComponent(newObj.EntityId);
							break;
						case CEditableObject.EObjectType.Component:
							InspectComponent(newObj.ComponentId);
							break;
					}
				}
				else
				{
					m_view?.SetInspectorVisible(false);
					Reset();
					EditorEntityUtility.PickComponent(SEntityComponentId.Invalid);
				}
			};
		}

		private void InitializeEntityMenues()
		{
			List<CAddComponentEntryViewModel> tempList = new List<CAddComponentEntryViewModel>(128);
			Dictionary<string, List<CAddComponentEntryViewModel>> components = new Dictionary<string, List<CAddComponentEntryViewModel>>(12);

			foreach (var type in CKlaxScriptRegistry.Instance.Types)
			{
				if (type.Type.IsSubclassOf(typeof(CEntityComponent)))
				{
					KlaxComponentAttribute attribute = type.Type.GetCustomAttribute<KlaxComponentAttribute>();

					if (attribute.HideInEditor)
						continue;

					CAddComponentEntryViewModel vm = new CAddComponentEntryViewModel(type.Name, type.Type);

					if (components.TryGetValue(attribute.Category, out List<CAddComponentEntryViewModel> list))
					{
						list.Add(vm);
					}
					else
					{
						List<CAddComponentEntryViewModel> newList = new List<CAddComponentEntryViewModel>(8);
						components.Add(attribute.Category, newList);

						newList.Add(vm);
					}
				}
			}

			List<CAddComponentCategoryViewModel> orderedList = new List<CAddComponentCategoryViewModel>(components.Count);

			foreach (var entry in components)
			{
				CAddComponentCategoryViewModel vm = new CAddComponentCategoryViewModel(entry.Key, entry.Value);
				orderedList.Add(vm);
			}

			orderedList = orderedList.OrderBy(p => p.Name).ToList();
			//Always show Common on top
			for (int i = 0, count = orderedList.Count; i < count; i++)
			{
				if (orderedList[i].Name == "Common")
				{
					ContainerUtilities.Swap(orderedList, i, 0);
					break;
				}
			}

			m_addComponentMenuCategories = new ObservableCollection<CAddComponentCategoryViewModel>(orderedList);

			m_entityCommands = new ObservableCollection<CEntityCommandViewModel>();
			m_entityCommands.Add(new CEntityCommandViewModel("Save as Asset", new CRelayCommand((obj) =>
			{
				EntityCommandsMenuOpen = false;

				CAssetBrowserViewModel assetBrowser = CWorkspace.Instance.GetTool<CAssetBrowserViewModel>();
				string assetPath = assetBrowser.ActiveDirectory;
				SEntityId id = m_selectedObject.GetTargetEntityId();

				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					CEntity entity = id.GetEntity();
					CEntityAsset<CEntity>.CreateFromEntity(entity, assetPath);
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
					{
						assetBrowser.UpdateShownAssets();
					}));
				});
			})));
		}

		public void AttachComponent(SEntityComponentId child, SEntityComponentId parent)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CSceneComponent parentObj = parent.GetComponent<CSceneComponent>();
				CSceneComponent childObj = child.GetComponent<CSceneComponent>();

				if (parentObj != null && childObj != null)
				{
					CSceneComponent oldParent = childObj.ParentComponent;
					SEntityComponentId oldParentid = new SEntityComponentId(oldParent);

					if (childObj.AttachToComponent(parentObj))
					{
						UpdateEntityInformation_EngineThread(m_selectedObject.GetTargetEntityId(), true);

						void Undo()
						{
							CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
							{
								CSceneComponent oldParentInst = oldParentid.GetComponent<CSceneComponent>();
								CSceneComponent childInst = child.GetComponent<CSceneComponent>();

								if (oldParentInst != null && childInst != null)
								{
									childInst.AttachToComponent(oldParentInst);
									UpdateEntityInformation_EngineThread(child.EntityId, true);
								}
							});
						}

						void Redo()
						{
							CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
							{
								CSceneComponent newParentInst = parent.GetComponent<CSceneComponent>();
								CSceneComponent childInst = child.GetComponent<CSceneComponent>();

								if (newParentInst != null && childInst != null)
								{
									childInst.AttachToComponent(newParentInst);
									UpdateEntityInformation_EngineThread(child.EntityId, true);
								}
							});
						}

						CRelayUndoItem item = new CRelayUndoItem(Undo, Redo);
						UndoRedoUtility.Record(item);
					}
				}
			});
		}

		private void InternalAddComponent(object argument)
		{
			AddComponentMenuOpen = false;

			if ((argument is Type type) && m_selectedObject != null)
			{
				SEntityId selectedEntityId = m_selectedObject.GetTargetEntityId();

				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					CEntity entity = selectedEntityId.GetEntity();
					if (entity != null)
					{
						entity.AddComponent(type, true, true);

						UpdateEntityInformation_EngineThread(selectedEntityId);
						UndoRedoUtility.Purge(null);
					}
				});
			}
		}

		private void InternalRenameEntity(object argument)
		{
			string name = argument as string;

			EntityInfo[0].Name = name;

			CWorldOutlinerViewModel vm = CWorkspace.Instance.GetTool<CWorldOutlinerViewModel>();
			vm.SelectedEntityViewModel.Name = name;

			EntityName = name;
		}

		private void InspectEntity(SEntityId entityId)
		{
			CEngine engine = CEngine.Instance;
			engine.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CUpdateScheduler scheduler = engine.CurrentWorld.UpdateScheduler;
				if (m_updateScope != null && m_updateScope.IsConnected())
				{
					scheduler.Disconnect(m_updateScope);
				}

				CEntity entity = entityId.GetEntity();
				if (entity != null)
				{
					m_selectedObject = new CEditableObject(entityId);
					m_updateScope = scheduler.Connect(UpdateCallback, EUpdatePriority.ResourceLoading);

					Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
					{
						m_desiredTarget = new CEditableObject(entityId);
						UnmarkEverything();
						MarkEntityInInspector(true);
					}));
				}
			});
		}

		private void InspectComponent(SEntityComponentId componentId)
		{
			bool bUpdateInspectorObjectList = ShouldUpdateInspectorList(componentId);

			CEngine engine = CEngine.Instance;
			CWorldOutlinerViewModel worldOutliner = CWorkspace.Instance.GetTool<CWorldOutlinerViewModel>();
			engine.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CUpdateScheduler scheduler = engine.CurrentWorld.UpdateScheduler;
				if (m_updateScope != null && m_updateScope.IsConnected())
				{
					scheduler.Disconnect(m_updateScope);
				}

				CEntityComponent component = componentId.GetComponent();
				if (component != null)
				{
					m_selectedObject = new CEditableObject(componentId);
					m_updateScope = scheduler.Connect(UpdateCallback, EUpdatePriority.ResourceLoading);

					if (bUpdateInspectorObjectList)
					{
						UpdateEntityInformation_EngineThread(new SEntityId(component.Owner.Id));
					}

					worldOutliner.PickingComponentId.GetComponent<CScenePickingComponent>().Pick(component as CSceneComponent);

					Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
					{
						m_desiredTarget = new CEditableObject(componentId);
						UnmarkEverything();
						MarkComponentInInspector(componentId, true);
					}));
				}
				else
				{
					Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
					{
						m_desiredTarget = null;
						UnmarkEverything();
					}));
				}
			});
		}

		private void UnmarkEverything()
		{
			MarkComponentInInspector(SEntityComponentId.Invalid, false);
			MarkEntityInInspector(false);
		}

		private void MarkComponentInInspector(SEntityComponentId componentId, bool bMark)
		{
			bool bMarkEverything = componentId == SEntityComponentId.Invalid;

			void Mark(CInspectorSceneComponentViewModel sceneVm)
			{
				if (sceneVm.ComponentId == componentId || bMarkEverything)
					sceneVm.SetViewModelMarked(bMark);

				foreach (var child in sceneVm.Children)
					Mark(child);
			}

			if (SceneComponents.Count > 0)
			{
				Mark(SceneComponents[0]);
			}

			foreach (var entComp in EntityComponents)
			{
				if (entComp.ComponentId == componentId || bMarkEverything)
					entComp.SetViewModelMarked(bMark);
			}
		}

		private void MarkEntityInInspector(bool bMark)
		{
			if (EntityInfo.Count > 0)
			{
				EntityInfo[0].SetViewModelMarked(bMark);
			}
		}

		private bool ShouldUpdateInspectorList(SEntityComponentId inspectedComponent)
		{
			if (m_desiredTarget == null)
			{
				return true;
			}

			if (inspectedComponent == m_desiredTarget.ComponentId)
			{
				return true;
			}

			if (m_desiredTarget.EntityId == inspectedComponent.EntityId)
			{
				return false;
			}

			if (m_desiredTarget.ComponentId.EntityId == inspectedComponent.EntityId)
			{
				return false;
			}

			return true;
		}

		private void Reset()
		{
			void DeselectSceneComponents(CInspectorSceneComponentViewModel vm)
			{
				vm.IsSelected = false;
				foreach (var child in vm.Children)
					DeselectSceneComponents(child);
			}

			if (SceneComponents.Count > 0)
			{
				DeselectSceneComponents(SceneComponents[0]);
			}

			if (EntityInfo.Count > 0)
			{
				EntityInfo[0].IsSelected = false;
			}

			foreach (var entityComp in EntityComponents)
			{
				entityComp.IsSelected = false;
			}

			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				if (m_updateScope != null && m_updateScope.IsConnected())
				{
					CEngine.Instance.CurrentWorld.UpdateScheduler.Disconnect(m_updateScope);
					UpdateCallback(0.0f);

					m_selectedObject = null;
				}
			});

			m_desiredTarget = null;
		}

		private void UpdateCallback(float deltaTime)
		{
			if (m_selectedObject == null)
				return;

			object target = m_selectedObject.GetEngineObject_EngineThread();
			CObjectProperties properties = EditorHelpers.GetObjectProperties(target);

			if (properties != null)
			{
				switch (m_selectedObject.Type)
				{
					case CEditableObject.EObjectType.Entity:
						properties.target = m_selectedObject;
						break;
					case CEditableObject.EObjectType.Component:
						properties.target = m_selectedObject;
						break;
				}

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
					if (m_desiredTarget == null || !m_desiredTarget.Equals(properties.target))
					{
						return;
					}

					if (properties.target is CEditableObject editObj)
					{
						if (m_lastShownTarget != null && !m_lastShownTarget.Equals(editObj))
						{
							m_view.ClearInspector();
							m_view.LockInspector(false);
						}
					}

					if (m_view != null && properties.properties.Count > 0)
					{
						m_view.ShowInspectors(properties.properties);
					}

					m_lastShownTarget = properties.target as CEditableObject;
				}));
			}
		}

		public void QueueEntityInformationUpdate(SEntityId entityId, bool bReselectTarget = false)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				UpdateEntityInformation_EngineThread(entityId, bReselectTarget);
			});
		}

		private void UpdateEntityInformation_EngineThread(SEntityId entityId, bool bReselectTarget = false)
		{
			void CreateViewModel(CSceneComponent component, CInspectorSceneComponentViewModel parent, CEntity owner)
			{
				if (component.Owner != owner || !component.ShowInInspector || component.MarkedForDestruction)
				{
					return;
				}

				SEntityComponentId id = new SEntityComponentId(component, false);

				CInspectorSceneComponentViewModel vm = new CInspectorSceneComponentViewModel(this, component.Name, id, "sceneComponent");
				parent.Children.Add(vm);

				foreach (var child in component.Children)
				{
					CreateViewModel(child, vm, owner);
				}
			}

			CEntity entity = entityId.GetEntity();
			if (entity != null)
			{
				CInspectorEntityViewModel entityInfo = null;
				CInspectorSceneComponentViewModel sceneComponentsInfo = null;
				List<CInspectorEntityComponentViewModel> entityComponentsInfo = null;

				sceneComponentsInfo = null;
				if (entity.RootComponent != null)
				{
					sceneComponentsInfo = new CInspectorSceneComponentViewModel(this, entity.RootComponent.Name, new SEntityComponentId(entity.RootComponent, false), "sceneComponent");
					foreach (var child in entity.RootComponent.Children)
					{
						CreateViewModel(child, sceneComponentsInfo, entity);
					}
				}

				List<CEntityComponent> components = new List<CEntityComponent>(8);
				entityComponentsInfo = new List<CInspectorEntityComponentViewModel>(4);
				entity.GetComponents(components);

				for (int i = components.Count - 1; i >= 0; i--)
				{
					if (!(components[i] is CSceneComponent) && components[i].ShowInInspector && !components[i].MarkedForDestruction)
					{
						entityComponentsInfo.Add(new CInspectorEntityComponentViewModel(this, components[i].Name, new SEntityComponentId(components[i], false)));
					}
				}

				entityInfo = new CInspectorEntityViewModel(this, entity.Name, new SEntityId(entity.Id));

				string entityName = entity.Name;

				Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
			   {
				   m_view?.LockInspector(false);
				   UpdateEntityInformation_EditorThread(sceneComponentsInfo, entityComponentsInfo, entityInfo);

				   m_entityName = entityName;
				   RaisePropertyChanged(nameof(EntityName));
				   m_view.SetEntityName(entityName);

				   if (bReselectTarget)
				   {
					   CWorkspace.Instance.SetSelectedObject(m_selectedObject, true);
				   }
			   }));
			}
			else
			{
				//Clear component list when selecting invalid entity
				Application.Current.Dispatcher.BeginInvoke((Action)(() =>
				{
					UpdateEntityInformation_EditorThread(null, null, null);
				}));
			}
		}

		private void UpdateEntityInformation_EditorThread(CInspectorSceneComponentViewModel sceneComponentRoot, List<CInspectorEntityComponentViewModel> entityComponents, CInspectorEntityViewModel entity)
		{
			SceneComponents.Clear();
			EntityComponents.Clear();
			EntityInfo.Clear();

			if (sceneComponentRoot != null)
			{
				SceneComponents.Add(sceneComponentRoot);
			}

			if (entityComponents != null)
			{
				EntityComponents = new ObservableCollection<CInspectorEntityComponentViewModel>(entityComponents);
			}

			if (entity != null)
			{
				EntityInfo.Add(entity);
			}
		}

		private ObservableCollection<CInspectorEntityViewModel> m_entityInfo = new ObservableCollection<CInspectorEntityViewModel>();
		public ObservableCollection<CInspectorEntityViewModel> EntityInfo
		{
			get { return m_entityInfo; }
			set
			{
				m_entityInfo = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CInspectorSceneComponentViewModel> m_sceneComponents = new ObservableCollection<CInspectorSceneComponentViewModel>();
		public ObservableCollection<CInspectorSceneComponentViewModel> SceneComponents
		{
			get { return m_sceneComponents; }
			set
			{
				m_sceneComponents = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CInspectorEntityComponentViewModel> m_entityComponents = new ObservableCollection<CInspectorEntityComponentViewModel>();
		public ObservableCollection<CInspectorEntityComponentViewModel> EntityComponents
		{
			get { return m_entityComponents; }
			set
			{
				m_entityComponents = value;
				RaisePropertyChanged();
			}
		}

		private string m_entityName;
		public string EntityName
		{
			get { return m_entityName; }
			set
			{
				if (m_entityName != value)
				{
					m_entityName = value;
					RaisePropertyChanged();

					if (EntityInfo.Count > 0)
					{
						SEntityId entityId = EntityInfo[0].EntityId;
						CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
						{
							CEntity entity = entityId.GetEntity();
							if (entity != null)
							{
								entity.Name = value;
							}
						});

						EntityInfo[0].Name = value;
					}
				}
			}
		}

		private bool m_bAddComponentMenuOpen;
		public bool AddComponentMenuOpen
		{
			get { return m_bAddComponentMenuOpen; }
			set
			{
				if (m_bAddComponentMenuOpen != value)
				{
					m_bAddComponentMenuOpen = value;
					RaisePropertyChanged();
				}
			}
		}

		private ObservableCollection<CAddComponentCategoryViewModel> m_addComponentMenuCategories;
		public ObservableCollection<CAddComponentCategoryViewModel> AddComponentMenuCategories
		{
			get { return m_addComponentMenuCategories; }
			set
			{
				m_addComponentMenuCategories = value;
				RaisePropertyChanged();
			}
		}

		private bool m_bEntityCommandMenuOpen;
		public bool EntityCommandsMenuOpen
		{
			get { return m_bEntityCommandMenuOpen; }
			set
			{
				m_bEntityCommandMenuOpen = value;
				RaisePropertyChanged();
			}
		}

		private ObservableCollection<CEntityCommandViewModel> m_entityCommands;
		public ObservableCollection<CEntityCommandViewModel> EntityCommands
		{
			get { return m_entityCommands; }
			set
			{
				m_entityCommands = value;
				RaisePropertyChanged();
			}
		}

		private CEditableObject m_desiredTarget;
		private CEditableObject m_lastShownTarget;

		public ICommand RenameEntityCommand { get; private set; }
		public ICommand AddComponentCommand { get; private set; }
		public ICommand ToggleAddComponentMenuCommand { get; private set; }
		public ICommand ToggleEntityCommandsMenuCommand { get; private set; }
		public ICommand KeyDownCommand { get; private set; }

		private IInspectorView m_view;
		private CUpdateScope m_updateScope;

		//Engine Thread
		private readonly object m_lockObject = new object();
		private CEditableObject m_selectedObject;
	}
}
