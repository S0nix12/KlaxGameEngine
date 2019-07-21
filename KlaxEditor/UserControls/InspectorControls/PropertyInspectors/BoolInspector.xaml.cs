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

namespace KlaxEditor.UserControls.InspectorControls
{
    /// <summary>
    /// Interaction logic for BoolInspector.xaml
    /// </summary>
    public partial class BoolInspector : BaseInspectorControl
    {
        public BoolInspector()
        {
            InitializeComponent();
        }

		public override void SetValueOnly(object value)
		{
			bool boolValue = (bool)value;

			m_ignoreChanges = true;
			Checkbox.IsChecked = boolValue;
			m_ignoreChanges = false;
		}

		public override void PropertyInfoChanged(CObjectProperty info)
        {
            base.PropertyInfoChanged(info);

            m_ignoreChanges = true;
            Checkbox.IsChecked = (bool)info.Value;
            m_ignoreChanges = false;
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (!m_ignoreChanges)
            {
                SetInspectorValue(PropertyInfo, (bool)PropertyInfo.Value, Checkbox.IsChecked);
            }
        }

        private bool m_ignoreChanges;
    }
}
