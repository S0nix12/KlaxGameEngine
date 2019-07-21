using KlaxCore.EditorHelper;
using KlaxCore.KlaxScript;
using KlaxEditor.Annotations;
using KlaxEditor.ViewModels;
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
	public partial class TypeInspector : BaseInspectorControl, INotifyPropertyChanged
	{
		public TypeInspector()
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

		public override void SetValueOnly(object value)
		{
			m_isRawType = false;
			if (value == null)
			{
				AssetSelector.SelectedIndex = -1;
			}
			else
			{
				CKlaxScriptTypeInfo typeInfo = value as CKlaxScriptTypeInfo;
				if (typeInfo == null && value is Type rawType)
				{
					m_isRawType = true;
					CKlaxScriptRegistry.Instance.TryGetTypeInfo(rawType, out typeInfo);
				}

				int selectedIndex = -1;
				if (value != null)
				{
					selectedIndex = m_availableAssets.FindIndex(availableTypeInfo => availableTypeInfo.Type == typeInfo.Type);
				}
				AssetSelector.SelectedIndex = selectedIndex;
			}
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			m_isRawType = false;

			CKlaxScriptTypeInfo typeInfo = info.Value as CKlaxScriptTypeInfo;
			if (typeInfo == null && info.ValueType == typeof(Type))
			{
				m_isRawType = true;
				CKlaxScriptRegistry.Instance.TryGetTypeInfo(info.Value as Type, out typeInfo);
			}

			m_bIsLocked = true;
			if (!m_bItemSourcesSet)
			{
				m_availableAssets.Clear();
				foreach (CKlaxScriptTypeInfo klaxType in CKlaxScriptRegistry.Instance.Types)
				{
					m_availableAssets.Add(klaxType);
				}

				AssetSelector.ItemsSource = m_availableAssets;
				m_bItemSourcesSet = true;
			}

			if (typeInfo != m_oldValue)
			{
				int selectedIndex = -1;
				if (info.Value != null)
				{
					selectedIndex = m_availableAssets.FindIndex(availableTypeInfo => availableTypeInfo.Type == typeInfo.Type);
				}
				AssetSelector.SelectedIndex = selectedIndex;
			}

			m_oldValue = typeInfo;
			m_bIsLocked = false;
		}

		private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!m_bIsLocked)
			{
				if (AssetSelector.SelectedIndex >= 0)
				{
					CKlaxScriptTypeInfo selectedAsset = (CKlaxScriptTypeInfo)AssetSelector.SelectedItem;
					object newValue = selectedAsset;
					if (m_isRawType)
					{
						newValue = selectedAsset.Type;
					}

					SetInspectorValue(PropertyInfo, PropertyInfo.Value, newValue);
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
		private readonly List<CKlaxScriptTypeInfo> m_availableAssets = new List<CKlaxScriptTypeInfo>();
		private object m_oldValue;
		private bool m_isRawType;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
