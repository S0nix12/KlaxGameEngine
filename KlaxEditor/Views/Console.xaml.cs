using KlaxConfig;
using KlaxEditor.ViewModels.EditorWindows.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KlaxEditor.Views
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class Console : UserControl, IConsoleView
    {
        public Console()
        {
            InitializeComponent();
        }

        public void SelectEndOfConsoleInput()
        {
            ConsoleInput.SelectionStart = ConsoleInput.Text.Length;
        }

        private void KeyBinding_Changed(object sender, EventArgs e)
        {
            ConsoleInput.Text = "";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Scrollviewer.ScrollToBottom();
        }
    }
}
