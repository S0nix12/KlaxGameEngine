using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxCore.KlaxScript.Interfaces;
using KlaxEditor.Utility;
using KlaxEditor.ViewModels.KlaxScript;
using KlaxEditor.Views;
using KlaxIO.AssetManager.Assets;
using KlaxIO.AssetManager.Loaders;
using KlaxShared.Definitions;
using Microsoft.Win32;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

namespace KlaxEditor.ViewModels.EditorWindows
{
	class CDirectoryEntry : CViewModelBase
	{
		public CDirectoryEntry(string directoryPath, CAssetBrowserViewModel viewModel, CDirectoryEntry parent = null)
		{
			ParentDirectory = parent;
			m_viewModel = viewModel;
			Path = directoryPath;
			string absolutePath = ProjectDefinitions.GetAbsolutePath(directoryPath);
			DirectoryInfo dirInfo = new DirectoryInfo(absolutePath);
			Name = string.IsNullOrWhiteSpace(directoryPath) ? "Project" : dirInfo.Name;
			EditName = Name;

			UpdateSubDirectories();

			SelectCommand = new CRelayCommand(OnDirectoryClicked);
			DragEnterCommand = new CRelayCommand(OnDragEnter);
			DragOverCommand = new CRelayCommand(OnDragOver);
			DropCommand = new CRelayCommand(OnDrop);
			AddFolderCommand = new CRelayCommand(OnAddFolder);
			DeleteFolderCommand = new CRelayCommand(OnDeleteFolder);
		}

		public void UpdateSubDirectories()
		{
			SubDirectories.Clear();
			string absolutePath = ProjectDefinitions.GetAbsolutePath(Path);

			var subDirectories = Directory.GetDirectories(absolutePath);
			foreach (string subDirectory in subDirectories)
			{
				SubDirectories.Add(new CDirectoryEntry(ProjectDefinitions.GetRelativePath(subDirectory), m_viewModel, this));
			}
		}

		private void OnAddFolder(object e)
		{
			string absoluteFolderPath = ProjectDefinitions.GetAbsolutePath(Path);
			string newFolderName = "NewFolder";
			string newFolderPath = System.IO.Path.Combine(absoluteFolderPath, newFolderName);
			if (Directory.Exists(newFolderPath))
			{
				bool bFoundName = false;
				for (int i = 0; i < 1000; i++)
				{
					string folderName = newFolderName + i;
					newFolderPath = System.IO.Path.Combine(absoluteFolderPath, folderName);
					if (!Directory.Exists(newFolderPath))
					{
						bFoundName = true;
						break;
					}
				}

				if (!bFoundName)
				{
					return;
				}
			}

			Directory.CreateDirectory(newFolderPath);
			SubDirectories.Add(new CDirectoryEntry(ProjectDefinitions.GetRelativePath(newFolderPath), m_viewModel, this));
		}

		private void OnDeleteFolder(object e)
		{
			if (ParentDirectory == null)
			{
				// Can't delete root
				return;
			}

			CAssetRegistry.Instance.RemoveFolder(Path);
			ParentDirectory.SubDirectories.Remove(this);
		}

		private void OnDirectoryClicked(object sender)
		{
			IsSelected = true;
		}
		private void OnDragEnter(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent("assetEntry") || args.Data.GetDataPresent("assetEntries") || args.Data.GetDataPresent(DataFormats.FileDrop))
			{
				args.Effects = DragDropEffects.Move;
			}
			else if (args.Data.GetDataPresent("folderEntry"))
			{
				if (args.Data.GetData("folderEntry") is CDirectoryEntry folder)
				{
					// Cant move root folder
					if (folder.ParentDirectory != null)
					{
						args.Effects = DragDropEffects.Move;
					}
				}
			}

			args.Handled = true;
		}

		private void OnDragOver(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent("assetEntry") || args.Data.GetDataPresent("assetEntries") || args.Data.GetDataPresent(DataFormats.FileDrop))
			{
				args.Effects = DragDropEffects.Move;
			}
			else if (args.Data.GetDataPresent("folderEntry"))
			{
				if (args.Data.GetData("folderEntry") is CDirectoryEntry folder)
				{
					// Cant move root folder
					if (folder.ParentDirectory != null)
					{
						args.Effects = DragDropEffects.Move;
					}
				}
			}

			args.Handled = true;
		}

		private void OnDrop(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			if (args.Data.GetDataPresent("assetEntry"))
			{
				// Move single asset
				CAssetEntryViewModel assetEntry = (CAssetEntryViewModel)args.Data.GetData("assetEntry");
				if (CAssetRegistry.Instance.MoveAssetFile(assetEntry.Asset, Path))
				{
					m_viewModel.RemoveShownAsset(assetEntry);
				}
			}
			else if (args.Data.GetDataPresent("assetEntries"))
			{
				// Move multiple assets
				object[] assetEntries = (object[])args.Data.GetData("assetEntries");
				foreach (object entry in assetEntries)
				{
					CAssetEntryViewModel assetEntry = (CAssetEntryViewModel)entry;
					if (CAssetRegistry.Instance.MoveAssetFile(assetEntry.Asset, Path))
					{
						m_viewModel.RemoveShownAsset(assetEntry);
					}
				}
			}
			else if (args.Data.GetDataPresent("folderEntry"))
			{
				CDirectoryEntry folderEntry = (CDirectoryEntry)args.Data.GetData("folderEntry");
				if (folderEntry.ParentDirectory != null)
				{
					CAssetRegistry.Instance.MoveAssetFolder(folderEntry.Path, Path);
					folderEntry.ParentDirectory.UpdateSubDirectories();
					UpdateSubDirectories();
				}
			}
			else if (args.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);
				foreach (string file in files)
				{
					m_viewModel.ImportFile(file, Path);
				}
			}
		}

		private void OnEndEditName()
		{
			if (EditName != Name)
			{
				if (CAssetRegistry.Instance.RenameFolder(Path, EditName))
				{
					Name = EditName;
					ParentDirectory?.UpdateSubDirectories();
				}
				else
				{
					EditName = Name;
				}
			}
		}

		public string Path { get; set; }

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set { m_name = value; RaisePropertyChanged(); }
		}

		private string m_editName;
		public string EditName
		{
			get { return m_editName; }
			set
			{
				m_editName = value;
				OnEndEditName();
				RaisePropertyChanged();
			}
		}

		public CDirectoryEntry ParentDirectory { get; set; }
		public ObservableCollection<CDirectoryEntry> SubDirectories { get; set; } = new ObservableCollection<CDirectoryEntry>();

		private bool m_bIsSelected;
		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				m_bIsSelected = value;
				if (m_bIsSelected)
				{
					m_viewModel.SetSelectedDirectory(this);
				}
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
			}
		}

		public ICommand SelectCommand { get; set; }
		public ICommand DragEnterCommand { get; set; }
		public ICommand DragOverCommand { get; set; }
		public ICommand DropCommand { get; set; }
		public ICommand AddFolderCommand { get; set; }
		public ICommand DeleteFolderCommand { get; set; }
		private CAssetBrowserViewModel m_viewModel;
	}

	class CAssetEntryViewModel : CViewModelBase
	{
		public CAssetEntryViewModel(CAsset asset, CAssetBrowserViewModel viewModel)
		{
			Asset = asset;
			m_viewModel = viewModel;
			TypeName = asset.GetTypeName();
			Color = EditorConversionUtility.ConvertEngineColorToSystem(asset.GetTypeColor());
			BorderColor = new SolidColorBrush(Color);

			Name = Asset.Name;
			EditName = Name;

			DeleteAssetCommand = new CRelayCommand(OnDeleteAsset);
			MouseLeftDownCommand = new CRelayCommand(OnMouseLeftDown);
		}

		private void OnEndEditName()
		{
			if (EditName != Name)
			{
				if (CAssetRegistry.Instance.RenameAsset(Asset, EditName))
				{
					Name = EditName;
				}
				else
				{
					EditName = Name;
				}
			}
		}

		private void OnDeleteAsset(object e)
		{
			m_viewModel.DeleteSelectedAssets();
		}

		private void OnMouseLeftDown(object e)
		{
			MouseButtonEventArgs args = (MouseButtonEventArgs)e;
			if (args.ClickCount == 2)
			{
				//Double click
				if (Asset is CEntityAsset<CEntity> entityAsset)
				{
					var tool = CWorkspace.Instance.GetTool<CEntityBuilderViewModel>();
					tool.OpenAsset(entityAsset);
				}
				else if(Asset is CKlaxScriptInterfaceAsset interfaceAsset)
				{
					var interfaceEditor = CWorkspace.Instance.GetTool<CInterfaceEditorViewmodel>();
					interfaceEditor.OpenAsset(interfaceAsset);
				}
				else if (Asset is CLevelAsset levelAsset)
				{
					CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
					{
						Stopwatch timer = new Stopwatch();
						timer.Start();
						CLevel newLevel = levelAsset.GetLevel();
						timer.Stop();
						LogUtility.Log("Level deserialize took {0} ms", timer.Elapsed.TotalMilliseconds);
						CEngine.Instance.CurrentWorld.ChangeLevel(levelAsset, newLevel);
					});
				}
			}
		}

		private string m_name;
		public string Name
		{
			get { return m_name; }
			set { m_name = value; RaisePropertyChanged(); }
		}
		private string m_editName;
		public string EditName
		{
			get { return m_editName; }
			set
			{
				m_editName = value;
				OnEndEditName();
				RaisePropertyChanged();
			}
		}

		private bool m_bIsSelected;

		public bool IsSelected
		{
			get { return m_bIsSelected; }
			set
			{
				m_bIsSelected = value;
				RaisePropertyChanged();
				if (m_bIsSelected)
				{
					m_viewModel.m_selectedAssets.Add(this);
					if (Asset is CMaterialAsset materialAsset)
					{
						CWorkspace.Instance.GetTool<MaterialEditorViewModel>().TargetMaterialAsset = materialAsset;
					}
					else if (Asset is CMeshAsset meshAsset)
					{
						PreviewScene previewScene = CWorkspace.Instance.GetTool<CAssetPreviewerViewModel>().PreviewScene;
						previewScene.ShowMeshSelection = false;
						previewScene.EditorThread_SetPreviewMesh(meshAsset, false);
					}
				}
				else
				{
					m_viewModel.m_selectedAssets.Remove(this);
				}
			}
		}

		public string TypeName { get; set; }
		public Color Color { get; set; }
		public SolidColorBrush BorderColor { get; set; }
		public CAsset Asset { get; private set; }
		public ICommand EndEditNameCommand { get; private set; }
		public ICommand DeleteAssetCommand { get; set; }
		public ICommand MouseLeftDownCommand { get; set; }

		private readonly CAssetBrowserViewModel m_viewModel;
	}

	class CAssetBrowserViewModel : CEditorWindowViewModel
	{
		public CAssetBrowserViewModel() : base("AssetBrowser")
		{
			SetIconSourcePath("Resources/Images/Tabs/assetbrowser.png");

			Content = new AssetBrowserView();
			m_rootDirectory = new CDirectoryEntry("", this); // Start at the project root
			RootFolders.Add(m_rootDirectory);
			SelectedFolderPath.Add(m_rootDirectory);
			ImportCommand = new CRelayCommand(OnImport);
			DeleteAssetCommand = new CRelayCommand(OnDeleteAsset);
			CreateMaterialCommand = new CRelayCommand(OnCreateMaterial);
			CreateEntityCommand = new CRelayCommand(OnCreateEntity);
			CreateInterfaceCommand = new CRelayCommand(OnCreateInterface);
		}

		public void SetSelectedDirectory(CDirectoryEntry directory)
		{
			ActiveDirectory = directory.Path;
			SelectedFolderPath.Clear();
			CDirectoryEntry dir = directory;
			SelectedFolderPath.Add(RootDirectory);
			while (dir.ParentDirectory != null)
			{
				SelectedFolderPath.Insert(1, dir);
				dir = dir.ParentDirectory;
			}
			UpdateShownAssets();
		}

		public void RemoveShownAsset(CAssetEntryViewModel assetEntry)
		{
			m_shownAssets.Remove(assetEntry);
		}

		public void UpdateShownAssets()
		{
			m_shownAssets.Clear();
			m_selectedAssets.Clear();
			List<CAsset> assets = new List<CAsset>();
			CAssetRegistry.Instance.GetAssetInDirectoryLoaded(ActiveDirectory, assets);
			foreach (CAsset asset in assets)
			{
				m_shownAssets.Add(new CAssetEntryViewModel(asset, this));
			}
		}

		public void ImportFile(string filename, string targetPath)
		{
			CImportManager.Instance.Import(filename, targetPath, true);
			if (targetPath == ActiveDirectory)
			{
				UpdateShownAssets();
			}
		}

		private void OnDeleteAsset(object e)
		{
			DeleteSelectedAssets();
		}

		private void OnImport(object e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Multiselect = true;

			if (fileDialog.ShowDialog() == true)
			{
				foreach (var fileName in fileDialog.FileNames)
				{
					ImportFile(fileName, ActiveDirectory);
				}
			}
		}

		private void OnCreateMaterial(object e)
		{
			CMaterialAsset newMaterial = new CMaterialAsset();
			newMaterial.Name = "Material";
			newMaterial.LoadFinished();
			CAssetRegistry.Instance.RegisterAsset(newMaterial, ActiveDirectory, false);
			UpdateShownAssets();
		}

		private void OnCreateEntity(object e)
		{
			CEntity emptyEntity = new CEntity();
			emptyEntity.Name = "Entity";
			CEntityAsset<CEntity>.CreateFromEntity(emptyEntity, ActiveDirectory);
			UpdateShownAssets();
		}

		private void OnCreateInterface(object e)
		{
			CKlaxScriptInterfaceAsset interfaceAsset = new CKlaxScriptInterfaceAsset();
			interfaceAsset.Name = "ScriptInterface";
			interfaceAsset.LoadFinished();
			CAssetRegistry.Instance.RegisterAsset(interfaceAsset, ActiveDirectory, false);
			UpdateShownAssets();
		}

		public void DeleteSelectedAssets()
		{
			foreach (var assetEntry in m_selectedAssets)
			{
				CAssetRegistry.Instance.RemoveAssetFile(assetEntry.Asset);
				RemoveShownAsset(assetEntry);
			}

			m_selectedAssets.Clear();
		}

		public void DeselectSelectedAssets()
		{
			List<CAssetEntryViewModel> copy = new List<CAssetEntryViewModel>(m_selectedAssets);
			foreach (var assetEntry in copy)
			{
				assetEntry.IsSelected = false;
			}
		}

		private ObservableCollection<CAssetEntryViewModel> m_shownAssets = new ObservableCollection<CAssetEntryViewModel>();
		public ObservableCollection<CAssetEntryViewModel> ShownAssets
		{
			get { return m_shownAssets; }
			set { m_shownAssets = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<CDirectoryEntry> m_selectedFolderPath = new ObservableCollection<CDirectoryEntry>();
		public ObservableCollection<CDirectoryEntry> SelectedFolderPath
		{
			get { return m_selectedFolderPath; }
			set { m_selectedFolderPath = value; RaisePropertyChanged(); }
		}

		private ObservableCollection<CDirectoryEntry> m_rootFolders = new ObservableCollection<CDirectoryEntry>();
		public ObservableCollection<CDirectoryEntry> RootFolders
		{
			get { return m_rootFolders; }
			set { m_rootFolders = value; RaisePropertyChanged(); }
		}

		private CDirectoryEntry m_rootDirectory;
		public CDirectoryEntry RootDirectory
		{
			get { return m_rootDirectory; }
			set { m_rootDirectory = value; RaisePropertyChanged(); }
		}

		private string m_activeDirectory;
		public string ActiveDirectory
		{
			get { return m_activeDirectory; }
			set { m_activeDirectory = value; RaisePropertyChanged(); }
		}

		public readonly List<CAssetEntryViewModel> m_selectedAssets = new List<CAssetEntryViewModel>();

		public ICommand ImportCommand { get; set; }
		public ICommand DeleteAssetCommand { get; set; }
		public ICommand CreateMaterialCommand { get; set; }
		public ICommand CreateEntityCommand { get; set; }
		public ICommand CreateInterfaceCommand { get; set; }
	}
}
