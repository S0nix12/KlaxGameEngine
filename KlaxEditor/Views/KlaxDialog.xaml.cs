using KlaxEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KlaxEditor.Views
{
    public enum EDialogIcon
    {
        None,
        Warning
    }

    public partial class KlaxDialog : Window
    {
        public KlaxDialog()
        {
            Owner = System.Windows.Application.Current.MainWindow;

            InitializeComponent();
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs args)
        {
            DialogResult = (DataContext as CDialogViewModel).Result;
        }

        public static bool? ShowDialog(string message, EDialogIcon icon, MessageBoxButton buttons)
        {
            ImageSource source = null;
            switch (icon)
            {
                case EDialogIcon.Warning:
                    source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Windows/fa_warning.png"));
                    break;
            }

            KlaxDialog dialog = new KlaxDialog();
            CDialogViewModel viewModel = new CDialogViewModel(message, buttons, dialog.Close, source);
            dialog.DataContext = viewModel;

            return dialog.ShowDialog();
        }
        
        public static bool? ShowDialog(string message, BitmapSource bitmap, MessageBoxButton buttons)
        {
            KlaxDialog dialog = new KlaxDialog();
            CDialogViewModel viewModel = new CDialogViewModel(message, buttons, dialog.Close, bitmap);
            dialog.DataContext = viewModel;
            return dialog.ShowDialog();
        }

        public static bool? ShowDialog(string message, Uri iconUri, MessageBoxButton buttons)
        {
            KlaxDialog dialog = new KlaxDialog();
            CDialogViewModel viewModel = new CDialogViewModel(message, buttons, dialog.Close, new BitmapImage(iconUri));
            dialog.DataContext = viewModel;

            return dialog.ShowDialog();
        }
    }
}
