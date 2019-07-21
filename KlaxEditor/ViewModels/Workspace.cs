using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KlaxConfig;
using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxCore.GameFramework.Editor;
using KlaxCore.KlaxScript;
using KlaxEditor.Utility;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxEditor.ViewModels.KlaxScript;
using KlaxIO.AssetManager.Assets;
using KlaxIO.AssetManager.Loaders;
using SharpDX;
using Xceed.Wpf.AvalonDock;

namespace KlaxEditor.ViewModels
{
	class CWorkspace : CViewModelBase
	{
		public static CWorkspace Instance { get; private set; }

		public CWorkspace()
		{
			CKlaxScriptRegistry registry = CKlaxScriptRegistry.Instance;

			Instance = this;
			UndoRedoModel = new CUndoRedoModel();

			Tools.Add(new CViewportViewModel());
			Tools.Add(new CConsoleViewModel());
			Tools.Add(new CWorldOutlinerViewModel());
			Tools.Add(new CInspectorViewModel());
			Tools.Add(new MaterialEditorViewModel());
			Tools.Add(new CAssetPreviewerViewModel());
			Tools.Add(new CAssetBrowserViewModel());
			Tools.Add(new CEntityBuilderViewModel());
			Tools.Add(new CEntityBuilderInspectorViewModel());
			Tools.Add(new CNodeGraphViewModel());
			Tools.Add(new CInterfaceEditorViewmodel());

			foreach (var tool in Tools)
			{
				tool.PostToolInit();
			}

			InitCommands();
			InitUndoRedo();
		}
		
		public event Action<CEditableObject, CEditableObject> OnSelectedEditableObjectChanged;

		public void PostWorldLoad()
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CWorld world = CEngine.Instance.CurrentWorld;
				world.OnEntityDestroyed += (entity) =>
				{
					Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
					{
						SetSelectedObject(null);
					}));
				};

				world.OnLevelChanged += (asset, level) =>
				{
					UndoRedoUtility.Purge(null);
				};
			});

			foreach (var tool in Tools)
			{
				tool.PostWorldLoad();
			}
		}

		public T GetTool<T>() where T : CEditorWindowViewModel
		{
			foreach (var tool in Tools)
			{
				if (tool is T castedTool)
				{
					return castedTool;
				}
			}

			return null;
		}

		private void InitCommands()
		{
			NewLevelCommand = new CRelayCommand(OnNewLevel);
			SaveLevelCommand = new CRelayCommand(OnSaveLevel, obj => OpenedLevelAsset != null);
			SaveLevelAsCommand = new CRelayCommand(OnSaveLevelAs);
			CloseCommand = new CRelayCommand(OnClose);

			SpawnEmptyEntity = new CRelayCommand(OnSpawnEmptyEntity);
			SpawnCubeEntity = new CRelayCommand(OnSpawnCubeEntity);

			UndoCommand = new CRelayCommand((arg) => { UndoRedoModel.Undo(); }, (arg) => { return UndoRedoModel.CanUndo; });
			RedoCommand = new CRelayCommand((arg) => { UndoRedoModel.Redo(); }, (arg) => { return UndoRedoModel.CanRedo; });

			PlayGameCommand = new CRelayCommand(OnPlayGame, CanPlayGame);
			StopGameCommand = new CRelayCommand(OnStopGame, CanStopGame);
		}

		private bool CanPlayGame(object argument)
		{
			return !IsInPlayMode && m_prePlayModeLevel == null;
		}

		private void OnPlayGame(object argument)
		{
			IsInPlayMode = true;
			m_prePlayModeLevelReference = OpenedLevelAsset;

			UndoRedoModel.IsRecording = false;
			UndoRedoModel.Purge(null);

			CViewportViewModel viewport = GetTool<CViewportViewModel>();
			viewport.IsVisible = true;
			viewport.IsActive = true;
			viewport.LockMouseCursor();

			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CWorld world = CEngine.Instance.CurrentWorld;

				m_prePlayModeLevel = new CLevelAsset(world.LoadedLevel, "EditorTempLevel");
				world.StartPlayMode();

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
				}));
			});
		}

		private void OnStopGame(object argument)
		{
			IsInPlayMode = false;

			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CWorld world = CEngine.Instance.CurrentWorld;

				world.StopPlayMode();
				world.ChangeLevel(null, m_prePlayModeLevel.GetLevel());

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
					OpenedLevelAsset = m_prePlayModeLevelReference;
					m_prePlayModeLevel = null;
					m_prePlayModeLevelReference = null;
					UndoRedoUtility.Purge(null);
					UndoRedoModel.IsRecording = true;

					GetTool<CViewportViewModel>().FreeMouseCursor();
				}));
			});
		}

		private bool CanStopGame(object argument)
		{
			return IsInPlayMode && m_prePlayModeLevel != null;
		}

		private void InitUndoRedo()
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CTransformGizmo.OnTranslationChanged += (tr, oldPos, newPos) =>
				{
					void undo() { tr.SetWorldPosition(oldPos); }
					void redo() { tr.SetWorldPosition(newPos); }

					CRelayUndoItem item = new CRelayUndoItem(undo, redo);
					UndoRedoUtility.Record(item);
				};
				CTransformGizmo.OnRotationChanged += (tr, oldRot, newRot) =>
				{
					void undo() { tr.SetWorldRotation(oldRot); }
					void redo() { tr.SetWorldRotation(newRot); }

					CRelayUndoItem item = new CRelayUndoItem(undo, redo);
					UndoRedoUtility.Record(item);
				};
				CTransformGizmo.OnScaleChanged += (tr, oldScale, newScale) =>
				{
					void undo() { tr.SetWorldScale(oldScale); }
					void redo() { tr.SetWorldScale(newScale); }

					CRelayUndoItem item = new CRelayUndoItem(undo, redo);
					UndoRedoUtility.Record(item);
				};
			});
		}

		public void SetSelectedObject(CEditableObject obj, bool bForce = false)
		{
			if (obj == SelectedEditableObject && !bForce)
				return;

			CEditableObject oldValue = SelectedEditableObject;
			SelectedEditableObject = obj;
			OnSelectedEditableObjectChanged?.Invoke(oldValue, obj);
		}

		private void OnSpawnEmptyEntity(object arg)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity newEntity = CEngine.Instance.CurrentWorld.SpawnEntity<CEntity>();
				SEntityId id = new SEntityId(newEntity.Id);

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
					Instance.SetSelectedObject(new CEditableObject(id));
				}));

				UndoRedoUtility.Purge(null);
			});
		}

		private void OnSpawnCubeEntity(object arg)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CMeshAsset cubeAsset = CImportManager.Instance.MeshImporter.LoadMeshAsync("TestResources/Cube.fbx");
				CEntity cubeEntity = CEngine.Instance.CurrentWorld.SpawnEntity<CEntity>();
				cubeEntity.AddComponent<CSceneComponent>(true, true);
				CMeshComponent meshComponent = cubeEntity.AddComponent<CMeshComponent>(true, true);
				meshComponent.LocalPosition = new Vector3(0, 0, 0.5f);
				meshComponent.Mesh = cubeAsset;
				SEntityId id = new SEntityId(cubeEntity.Id);

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
					Instance.SetSelectedObject(new CEditableObject(id));
				}));
				UndoRedoUtility.Purge(null);
			});
		}

		private void OnSpawnSphereEntity(object arg)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity newEntity = CEngine.Instance.CurrentWorld.SpawnEntity<CEntity>();
				SEntityId id = new SEntityId(newEntity.Id);

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
					Instance.SetSelectedObject(new CEditableObject(id));
				}));
				UndoRedoUtility.Purge(null);
			});
		}

		public void PostLevelLoad(CAssetReference<CLevelAsset> levelAsset)
		{
			OpenedLevelAsset = levelAsset;
		}

		private void OnNewLevel(object argument)
		{
			OpenedLevelAsset = null;

			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEngine.Instance.CurrentWorld.CreateLevel();
			});

			Instance.SetSelectedObject(null, true);
		}

		private void OnSaveLevel(object arg)
		{
			string assetPath = null;

			CAssetBrowserViewModel assetBrowser = Instance.GetTool<CAssetBrowserViewModel>();
			if (OpenedLevelAsset != null)
			{
				assetPath = OpenedLevelAsset.GetAsset().Path;
				CLevelAsset asset = OpenedLevelAsset.GetAsset();

				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					asset.SetLevel(CEngine.Instance.CurrentWorld.LoadedLevel);
					CAssetRegistry.Instance.SaveAsset(asset);

					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
					{
						assetBrowser.UpdateShownAssets();
					}));
				});
			}
		}

		private void OnSaveLevelAs(object argument)
		{
			CAssetBrowserViewModel assetBrowser = Instance.GetTool<CAssetBrowserViewModel>();
			string assetPath = assetBrowser.ActiveDirectory;
			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CLevelAsset levelAsset = new CLevelAsset(CEngine.Instance.CurrentWorld.LoadedLevel, "Level", assetPath);
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				{
					assetBrowser.UpdateShownAssets();
				}));
			});
		}

		private void OnLoadLayout(object param)
		{
			m_layoutManager.OnLoadLayout(param);
		}

		private void OnSaveLayout(object param)
		{
			m_layoutManager.OnSaveLayout(param);
		}

		private void OnSaveLayoutAs(object param)
		{
			m_layoutManager.OnSaveLayoutAs(param);
		}

		private void OnClose(object argument)
		{
			Application.Current.MainWindow.Close();
		}

		private DockingManager m_dockingManager;
		public DockingManager DockingManager
		{
			get { return m_dockingManager; }
			set
			{
				m_dockingManager = value;
				LayoutManager = new CLayoutManagerViewModel(value, this);
			}
		}

		public CAssetReference<CLevelAsset> OpenedLevelAsset { get; private set; }
		public CEditableObject SelectedEditableObject { get; private set; }

		public ObservableCollection<CEditorWindowViewModel> Tools { get; } = new ObservableCollection<CEditorWindowViewModel>();

		public CUndoRedoModel UndoRedoModel { get; }

		public bool IsInPlayMode { get; private set; }

		public ICommand UndoCommand { get; private set; }
		public ICommand RedoCommand { get; private set; }

		public ICommand SpawnEmptyEntity { get; private set; }
		public ICommand SpawnCubeEntity { get; private set; }
		public ICommand SpawnSphereEntity { get; private set; }

		public ICommand SaveLevelCommand { get; private set; }
		public ICommand SaveLevelAsCommand { get; private set; }
		public ICommand CloseCommand { get; private set; }
		public ICommand NewLevelCommand { get; private set; }
		public ICommand OpenLevelCommand { get; private set; }

		public ICommand PlayGameCommand { get; private set; }
		public ICommand StopGameCommand { get; private set; }

		private CLayoutManagerViewModel m_layoutManager;
		public CLayoutManagerViewModel LayoutManager
		{
			get { return m_layoutManager; }
			set
			{
				m_layoutManager = value;
				RaisePropertyChanged();
			}
		}

		private CLevelAsset m_prePlayModeLevel;
		private CAssetReference<CLevelAsset> m_prePlayModeLevelReference;
	}
}
