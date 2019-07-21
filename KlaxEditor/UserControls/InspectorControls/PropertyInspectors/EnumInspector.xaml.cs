using KlaxCore.EditorHelper;
using KlaxEditor.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for TypeInspector.xaml
	/// </summary>
	public partial class EnumInspector : BaseInspectorControl, INotifyPropertyChanged
	{
		public EnumInspector()
		{
			InitializeComponent();
		}

		private string GetNameByValue(int value)
		{
			if (m_enumType == null)
				return string.Empty;


			string[] names = Enum.GetNames(m_enumType);
			var rawValues = Enum.GetValues(m_enumType);
			int[] values = new int[rawValues.Length];

			for (int i = 0, count = rawValues.Length; i < count; i++)
			{
				values[i] = Convert.ToInt32(rawValues.GetValue(i));
			}

			for (int i = 0, count = names.Length; i < count; i++)
			{
				if (values[i] == value)
				{
					return names[i];
				}
			}

			return string.Empty;
		}

		private int GetValueByName(string name)
		{
			if (m_enumType != null)
			{
				return Convert.ToInt32(Enum.Parse(m_enumType, name, true));
			}

			return 0;
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
				view.Filter = (o) => ((string)o).IndexOf(m_filterText, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		public override void SetValueOnly(object value)
		{
			int selectedIndex = -1;
			if (value != null)
			{
				selectedIndex = m_availableAssets.FindIndex(typeInfo => typeInfo == GetNameByValue(Convert.ToInt32(value)));
			}
			AssetSelector.SelectedIndex = selectedIndex;
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			m_bIsLocked = true;
			if (!m_bItemSourcesSet)
			{
				m_enumType = info.ValueType;

				m_availableAssets.Clear();
				m_availableAssets.AddRange(Enum.GetNames(m_enumType));

				AssetSelector.ItemsSource = m_availableAssets;
				m_bItemSourcesSet = true;
			}

			if (info.Value != m_oldValue)
			{
				int selectedIndex = -1;
				if (info.Value != null)
				{
					selectedIndex = m_availableAssets.FindIndex(typeInfo => typeInfo == GetNameByValue(Convert.ToInt32(info.Value)));
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
					string selectedAsset = (string)AssetSelector.SelectedItem;
					SetInspectorValue(PropertyInfo, PropertyInfo.Value, GetValueByName(selectedAsset));
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

		private Type m_enumType;
		private bool m_bItemSourcesSet;
		private readonly List<string> m_availableAssets = new List<string>();
		private object m_oldValue;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
