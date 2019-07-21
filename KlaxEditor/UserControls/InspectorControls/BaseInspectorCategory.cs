using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KlaxEditor.UserControls.InspectorControls
{
	public class CBaseInspectorCategory : UserControl
	{
		public CBaseInspectorCategory(int priority)
		{
			Priority = priority;
		}

		public void AddPropertyInspector(UIElement inspector, UIElement name)
		{
			RowDefinition definition = new RowDefinition();
			definition.Height = new GridLength(0.0, GridUnitType.Auto);
			m_propertyGrid.RowDefinitions.Add(definition);

			m_propertyGrid.Children.Add(inspector);
			m_propertyGrid.Children.Add(name);

			Grid.SetColumn(inspector, 3);
			Grid.SetColumn(name, 0);
			Grid.SetRow(inspector, m_propertyGrid.RowDefinitions.Count - 1);
			Grid.SetRow(name, m_propertyGrid.RowDefinitions.Count - 1);
		}

		public void ResizeColumns(GridLength left)
		{
			m_leftColumn.Width = left;
		}

		protected void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{
			m_inspector?.ResizeColumns(m_leftColumn.Width);
		}

		public int Priority { get; }

		protected Grid m_propertyGrid;
		protected ColumnDefinition m_leftColumn;
		protected ColumnDefinition m_rightColumn;
		protected IPropertyInspector m_inspector;
	}
}
