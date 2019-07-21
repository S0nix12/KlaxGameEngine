using KlaxCore.EditorHelper;
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

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for SimpleInspectorCategory.xaml
	/// </summary>
	public partial class SimpleInspectorCategory : CBaseInspectorCategory
	{
		public SimpleInspectorCategory(CCategoryInfo info, IPropertyInspector inspector)
			: base(info.Priority)
		{
			InitializeComponent();

			m_inspector = inspector;
			m_propertyGrid = PropertyGrid;
			m_leftColumn = LeftColumn;
			m_rightColumn = RightColumn;
		}
	}
}
