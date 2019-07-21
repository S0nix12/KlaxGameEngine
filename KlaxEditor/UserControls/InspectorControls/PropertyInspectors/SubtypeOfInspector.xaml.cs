using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using KlaxCore.KlaxScript;
using KlaxEditor.Annotations;
using KlaxEditor.ViewModels;

namespace KlaxEditor.UserControls.InspectorControls.PropertyInspectors
{
	/// <summary>
	/// Interaction logic for SubtypeOfInspector.xaml
	/// </summary>
	public partial class SubtypeOfInspector : BaseInspectorControl, INotifyPropertyChanged
	{
		public SubtypeOfInspector()
		{
			InitializeComponent();
		}

		private void UpdateFilter()
		{
			ICollectionView view = CollectionViewSource.GetDefaultView(AssetSelector.ItemsSource);
			if (string.IsNullOrWhiteSpace(m_filterText))
			{
				view.Filter = null;
			}
			else
			{
				view.Filter = (o) => ((CKlaxScriptTypeInfo)o).Name.IndexOf(m_filterText, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);
			Type newParentType = null;
			if (info.ValueType.IsGenericType && info.ValueType.GenericTypeArguments.Length > 0)
			{
				newParentType = info.ValueType.GenericTypeArguments[0];
			}
			else
			{
				newParentType = typeof(object);
			}

			m_bIsLocked = true;
			if (!m_bItemSourcesSet || newParentType != m_parentType)
			{
				m_parentType = newParentType;
				m_possibleTypes.Clear();
				foreach (CKlaxScriptTypeInfo klaxType in CKlaxScriptRegistry.Instance.Types)
				{
					if (m_parentType.IsAssignableFrom(klaxType.Type))
					{
						m_possibleTypes.Add(klaxType);
					}
				}

				AssetSelector.ItemsSource = m_possibleTypes;
				m_bItemSourcesSet = true;
			}

			if (info.Value != m_oldValue)
			{
				int selectedIndex = -1;
				if (info.Value != null)
				{
					dynamic subtypeValue = info.Value;
					selectedIndex = m_possibleTypes.FindIndex(typeInfo => typeInfo.Type == subtypeValue.Type);
				}
				AssetSelector.SelectedIndex = selectedIndex;
			}

			m_oldValue = info.Value;
			m_bIsLocked = false;
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!m_bIsLocked)
			{
				if (AssetSelector.SelectedIndex >= 0)
				{
					CKlaxScriptTypeInfo selectedType = (CKlaxScriptTypeInfo)AssetSelector.SelectedItem;
					object subtypeValue = Activator.CreateInstance(PropertyInfo.ValueType, selectedType.Type);
					SetInspectorValue(PropertyInfo, PropertyInfo.Value, subtypeValue);
				}
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string m_filterText;
		public string FilterText
		{
			get { return m_filterText; }
			set
			{
				m_filterText = value;
				UpdateFilter();
				RaisePropertyChanged();
			}
		}

		private ICommand m_onFilterChangedCommand;
		public ICommand OnFilterChangedCommand
		{
			get { return m_onFilterChangedCommand; }
			set { m_onFilterChangedCommand = value; RaisePropertyChanged(); }
		}

		private bool m_bIsLocked;

		private bool m_bItemSourcesSet;
		private readonly List<CKlaxScriptTypeInfo> m_possibleTypes = new List<CKlaxScriptTypeInfo>();
		private Type m_parentType;
		private object m_oldValue;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
