using KlaxEditor.UserControls.InspectorControls;
using KlaxEditor.Views;
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
using KlaxCore.EditorHelper;

namespace KlaxEditor.UserControls
{
	/// <summary>
	/// Interaction logic for InspectorPropertyName.xaml
	/// </summary>
	public partial class InspectorPropertyName : UserControl
	{
		public InspectorPropertyName(CObjectProperty property, object defaultValue, string name, BaseInspectorControl valueInspector)
		{
			InitializeComponent();

			SetName(name);
			m_property = property;
			m_defaultValue = defaultValue;
			m_valueInspector = valueInspector;
		}

		internal void SetName(string name)
		{
			PropertyName.Text = name;
			PropertyName.ToolTip = name;
			m_name = name;
		}

		internal void PropertyInfoChanged(CObjectProperty objectProperty)
		{
			m_property = objectProperty;
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			m_valueInspector.SetInspectorValue(m_property, m_property.Value, m_defaultValue, true);
			m_valueInspector.SetValueOnly(m_defaultValue);
		}

		private string m_name;
		private object m_defaultValue;
		private CObjectProperty m_property;
		private BaseInspectorControl m_valueInspector;
	}
}
