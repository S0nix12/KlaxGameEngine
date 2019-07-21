using System;
using System.Windows;
using KlaxConfig;
using System.ComponentModel;
using KlaxIO.Log;
using KlaxEditor.Views;
using KlaxEditor.ViewModels;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Shapes;
using KlaxCore.Core;
using KlaxShared;
using WpfSharpDxControl;
using KlaxRenderer.Graphics;
using System.Windows.Data;
using KlaxIO.Input;

namespace KlaxEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			LocationChanged += (obj, args) =>
			{
				var screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
				MaxHeight = screen.WorkingArea.Height + 12;
			};

			Closing += OnClosing;
			Closed += OnClose;

			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
			{
				InitializeEngine();
				InitializeWorkspace();
			}));
		}

		private void InitializeWorkspace()
		{
			DataContext = new CWorkspace();
			CWorkspace.Instance.DockingManager = dockManager;

			foreach (var tool in CWorkspace.Instance.Tools)
			{
				if (!tool.CanBeInvisible || tool.IsAlwaysHidden)
					continue;

				System.Windows.Controls.MenuItem toolMenu = new System.Windows.Controls.MenuItem();
				toolMenu.Header = tool.Name;
				toolMenu.IsCheckable = true;
				toolMenu.DataContext = tool;

				Binding isCheckedBinding = new Binding();
				isCheckedBinding.Path = new PropertyPath("IsVisible");
				isCheckedBinding.Mode = BindingMode.TwoWay;
				toolMenu.SetBinding(System.Windows.Controls.MenuItem.IsCheckedProperty, isCheckedBinding);

				ToolsMenu.Items.Add(toolMenu);
			}
		}

		private void InitializeEngine()
		{
			CInitializer initializer = new CInitializer();
			CLogger editorLogger = new CLogger("editor.log", true, true, true);
			initializer.Add(editorLogger);

			CEngine.Create(initializer, true);
			Input.CursorVisibilitySetter = (arg) =>
			{
				Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() =>
				{
					if (arg)
						System.Windows.Forms.Cursor.Show();
					else
						System.Windows.Forms.Cursor.Hide();
				}));
			};
		}

		private void OnClosing(object sender, CancelEventArgs args)
		{
			bool? result = KlaxDialog.ShowDialog("Do you really want to close the editor?", EDialogIcon.Warning, MessageBoxButton.YesNo);
			if (result.HasValue)
			{
				args.Cancel = !result.Value;
			}
			else
			{
				args.Cancel = true;
			}
		}

		private void OnClose(object sender, EventArgs args)
		{
			CEngine.Instance.Dispatch(EEngineUpdatePriority.EndFrame, CEngine.Instance.Shutdown);
		}
	}
}
