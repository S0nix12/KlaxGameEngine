using KlaxEditor.ViewModels;
using KlaxEditor.Views;
using KlaxIO;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace KlaxEditor.ViewModels
{
	class CLayoutManagerViewModel : CViewModelBase
	{
		public class CLayoutPresetViewModel : CViewModelBase
		{
			public CLayoutPresetViewModel(CLayoutManagerViewModel viewModel, string name, string path)
			{
				m_name = name;
				m_path = path;
				m_viewModel = viewModel;

				Load = new CRelayCommand((param) =>
				{
					m_viewModel.LoadLayout(m_path);
				});
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

			private string m_path;
			public string Path
			{
				get { return m_path; }
				set
				{
					m_path = value;
					RaisePropertyChanged();
				}
			}

			private string m_inputGesture;
			public string InputGesture
			{
				get { return m_inputGesture; }
				set { m_inputGesture = value; RaisePropertyChanged(); }
			}

			public ICommand Load { get; }
			private CLayoutManagerViewModel m_viewModel;
		}

		public CLayoutManagerViewModel(DockingManager manager, CWorkspace workspace)
		{
			m_dockingManager = manager;
			m_workspace = workspace;

			m_userLayoutFileFolder = Paths.UserDirectory + "\\Layouts\\";
			m_layoutFilePath = m_userLayoutFileFolder + "layout.cfg";
			m_builtinLayoutFileFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Resources\\Layouts\\";
			Directory.CreateDirectory(m_userLayoutFileFolder);

			LoadLayoutCommand = new CRelayCommand(OnLoadLayout, (arg) => m_dockingManager != null);
			LoadLayoutAtIndexCommand = new CRelayCommand(OnLoadAtIndexLayout, (arg) => m_dockingManager != null);
			SaveLayoutCommand = new CRelayCommand(OnSaveLayout, (arg) => m_dockingManager != null);
			SaveLayoutAsCommand = new CRelayCommand(OnSaveLayoutAs, (arg) => m_dockingManager != null);

			InitLayouts();
		}

		private void InitLayouts()
		{
			//Built-in layouts
			Directory.CreateDirectory(m_builtinLayoutFileFolder);
			foreach (var filePath in Directory.GetFiles(m_builtinLayoutFileFolder))
			{
				string extension = Path.GetExtension(filePath);
				if (extension == ".cfg")
				{
					LayoutPresets.Add(new CLayoutPresetViewModel(this, Path.GetFileNameWithoutExtension(filePath), filePath));
				}
			}

			//User defined layouts
			Directory.CreateDirectory(m_builtinLayoutFileFolder);
			foreach (var filePath in Directory.GetFiles(m_userLayoutFileFolder))
			{
				string extension = Path.GetExtension(filePath);
				if (extension == ".cfg")
				{
					LayoutPresets.Add(new CLayoutPresetViewModel(this, Path.GetFileNameWithoutExtension(filePath), filePath));
				}
			}

			UpdateInputGestures();
		}

		public bool DoesLayoutExist(string layoutName, out string existingPath)
		{
			//Check built-in layouts first
			string builtInPath = m_builtinLayoutFileFolder + layoutName + ".cfg";
			if (File.Exists(builtInPath))
			{
				existingPath = builtInPath;
				return true;
			}

			string userDefinedPath = m_userLayoutFileFolder + layoutName + ".cfg";
			if (File.Exists(userDefinedPath))
			{
				existingPath = userDefinedPath;
				return true;
			}

			existingPath = string.Empty;
			return false;
		}

		public bool LoadLayout(string path)
		{
			if (File.Exists(path))
			{
				XmlLayoutSerializer ser = new XmlLayoutSerializer(m_dockingManager);
				ser.LayoutSerializationCallback += (s, e) =>
				{
					Type type = Type.GetType(e.Model.ContentId, false, true);
					if (type != null)
					{
						foreach (CEditorWindowViewModel model in m_workspace.Tools)
						{
							if (model.GetType() == type)
							{
								e.Content = model;
								model.IsVisible = true;
								break;
							}
						}
					}
				};

				foreach (var model in m_workspace.Tools)
				{
					model.IsVisible = false;
				}

				try
				{
					ser.Deserialize(path);
				}
				catch
				{
					LogUtility.Log("The last used layout could not be loaded! The editor was reset to the default layout.");
					ser.Deserialize(m_layoutPresets[0].Path);
				}
				return true;
			}

			return false;
		}

		private void UpdateInputGestures()
		{
			for (int i = 0; i < m_layoutPresets.Count; i++)
			{
				if (Enum.TryParse($"F{i + 1}", out Key key))
				{
					m_layoutPresets[i].InputGesture = key.ToString();
				}
				else if (i >= 12)
				{
					m_layoutPresets[i].InputGesture = string.Empty;
				}
			}
		}

		public void OnLoadLayout(object param)
		{
			if (!LoadLayout(m_layoutFilePath))
			{
				LoadLayout(m_builtinLayoutFileFolder + "Default.cfg");
			}
		}

		public void OnLoadAtIndexLayout(object param)
		{
			if (param is string indexString)
			{
				int index = int.Parse(indexString);
				if (index >= 0 && index < m_layoutPresets.Count)
				{
					m_layoutPresets[(int)index].Load.Execute(null);
				}
			}
		}

		public void OnSaveLayout(object param)
		{
			var layoutSerializer = new XmlLayoutSerializer(m_dockingManager);

			if (File.Exists(m_layoutFilePath))
			{
				File.SetAttributes(m_layoutFilePath, FileAttributes.Normal);
				File.Delete(m_layoutFilePath);
			}
			layoutSerializer.Serialize(m_layoutFilePath);
		}

		public void OnSaveLayoutAs(object param)
		{
			LayoutSaveDialog dialog = new LayoutSaveDialog();
			bool? result = dialog.ShowDialog();

			string layoutName = dialog.LayoutNameBox.Text;
			string layoutPath = m_userLayoutFileFolder + "\\" + layoutName + ".cfg";
			if (DoesLayoutExist(layoutName, out string existingPath))
			{
				bool? assuranceResult = KlaxDialog.ShowDialog(string.Format("A layout named {0} already exists. Do you want to overwrite it?", layoutName), EDialogIcon.Warning, MessageBoxButton.YesNo);
				if (!assuranceResult.HasValue || !assuranceResult.Value)
				{
					return;
				}
				else
				{
					File.SetAttributes(existingPath, FileAttributes.Normal);
					File.Delete(existingPath);

					for (int i = 0, count = m_layoutPresets.Count; i < count; i++)
					{
						if (m_layoutPresets[i].Name == layoutName)
						{
							m_layoutPresets.RemoveAt(i);
							break;
						}
					}
				}
			}

			var layoutSerializer = new XmlLayoutSerializer(m_dockingManager);
			layoutSerializer.Serialize(layoutPath);

			LayoutPresets.Add(new CLayoutPresetViewModel(this, layoutName, layoutPath));
			UpdateInputGestures();
		}

		public ICommand LoadLayoutCommand { get; private set; }
		public ICommand LoadLayoutAtIndexCommand { get; private set; }
		public ICommand SaveLayoutCommand { get; private set; }
		public ICommand SaveLayoutAsCommand { get; private set; }

		public ICommand CanLoadLayout { get; private set; }
		public ICommand CanSaveLayout { get; private set; }

		private ObservableCollection<CLayoutPresetViewModel> m_layoutPresets = new ObservableCollection<CLayoutPresetViewModel>();
		public ObservableCollection<CLayoutPresetViewModel> LayoutPresets
		{
			get { return m_layoutPresets; }
			set
			{
				m_layoutPresets = value;
				RaisePropertyChanged();
			}
		}

		private readonly DockingManager m_dockingManager;
		private readonly CWorkspace m_workspace;

		private readonly string m_layoutFilePath;
		private readonly string m_userLayoutFileFolder;
		private readonly string m_builtinLayoutFileFolder;
	}
}
