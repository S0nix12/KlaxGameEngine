using KlaxCore.KlaxScript.Nodes;
using KlaxEditor.ViewModels.KlaxScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KlaxEditor.Selectors
{
	class CExecutionPinTemplateSelector : DataTemplateSelector
	{
		public DataTemplate SwitchTemplate { get; set; }
		public DataTemplate DefaultTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item is CSwitchExecutionPinViewModel)
			{
				return SwitchTemplate;
			}
			else
			{
				return DefaultTemplate;
			}
		}
	}
}
