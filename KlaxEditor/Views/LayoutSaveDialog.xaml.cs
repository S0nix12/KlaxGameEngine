using System.Windows;

namespace KlaxEditor.Views
{
	/// <summary>
	/// Interaction logic for LayoutSaveDialog.xaml
	/// </summary>
	public partial class LayoutSaveDialog : Window
	{
		public LayoutSaveDialog()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
