using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using KlaxEditor.ViewModels.KlaxScript;

namespace KlaxEditor.Views.KlaxScript.DataSelector
{
	class AddNodeDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CategoryDataTemplate { get; set; }
		public DataTemplate NodeDataTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is CNodeEntryViewModel)
			{
				return NodeDataTemplate;
			}

			return CategoryDataTemplate;
		}
	}
}
