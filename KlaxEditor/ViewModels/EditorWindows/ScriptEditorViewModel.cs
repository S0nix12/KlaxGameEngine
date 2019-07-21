using KlaxEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KlaxEditor.ViewModels.EditorWindows
{
	class CScriptEditorViewModel : CEditorWindowViewModel
	{
		public CScriptEditorViewModel()
			: base("Scripting")
		{
			BitmapImage bi = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Tabs/outliner.png"));
			IconSource = bi;

			Content = new ScriptEditor();
		}
	}
}
