using KlaxEditor.Views;
using System.Windows;
using System.Windows.Controls;
using KlaxCore.EditorHelper;
using System.Collections.Generic;

namespace KlaxEditor.UserControls.InspectorControls
{
    /// <summary>
    /// Interaction logic for InspectorCategory.xaml
    /// </summary>
    public partial class ExpandableInspectorCategory : CBaseInspectorCategory
    {
		public ExpandableInspectorCategory(CCategoryInfo info, IPropertyInspector inspector)
			: base(info.Priority)
        {
            InitializeComponent();

            Expander.Header = info.Name;

			m_inspector = inspector;
			m_propertyGrid = PropertyGrid;
			m_leftColumn = LeftColumn;
			m_rightColumn = RightColumn;
		}
	}
}
