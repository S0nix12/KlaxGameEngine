using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using KlaxCore.Core;
using KlaxCore.EditorHelper;
using KlaxEditor.Annotations;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxIO.AssetManager.Assets;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for AssetReferenceInspector.xaml
	/// </summary>
	public partial class AssetReferenceInspector : BaseInspectorControl, INotifyPropertyChanged
	{
		public AssetReferenceInspector()
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
				view.Filter = (o) => ((CAsset)o).Name.IndexOf(m_filterText, StringComparison.OrdinalIgnoreCase) >= 0;
			}

		}

		public override void SetValueOnly(object value)
		{
			int selectedIndex = -1;
			if (value != null)
			{
				dynamic dynValue = value;
				selectedIndex = m_availableAssets.FindIndex((asset => ReferenceEquals(asset, dynValue.GetAsset())));
			}

			AssetSelector.SelectedIndex = selectedIndex;
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);
			m_bIsLocked = true;
			Type newAssetType = null;
			if (info.ValueType.IsGenericType && info.ValueType.GenericTypeArguments.Length > 0)
			{
				newAssetType = info.ValueType.GenericTypeArguments[0];
			}
			else
			{
				newAssetType = typeof(CAsset);
			}

			if (!m_bItemSourcesSet || m_assetType != newAssetType)
			{
				m_assetType = newAssetType;
				m_availableAssets.Clear();
				Type assetReferenceType = typeof(CAssetReference<>).MakeGenericType(m_assetType);
				Type listType = typeof(List<>).MakeGenericType(assetReferenceType);
				object referenceList = Activator.CreateInstance(listType);
				MethodInfo getAssets = m_getAssetsMethod.MakeGenericMethod(m_assetType);
				getAssets.Invoke(CAssetRegistry.Instance, new object[] { referenceList });
				dynamic dynList = referenceList;
				foreach (dynamic reference in dynList)
				{
					m_availableAssets.Add(reference.GetAsset());
				}

				AssetSelector.ItemsSource = m_availableAssets;

				m_bItemSourcesSet = true;

			}

			if (info.Value != m_oldValue)
			{
				int selectedIndex = -1;
				if (info.Value != null)
				{
					dynamic dynValue = info.Value;
					selectedIndex = m_availableAssets.FindIndex((asset => ReferenceEquals(asset, dynValue.GetAsset())));
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
					CAsset selectedAsset = (CAsset)AssetSelector.SelectedItem;
					object newValue = Activator.CreateInstance(PropertyInfo.ValueType, selectedAsset);
					SetInspectorValue(PropertyInfo, PropertyInfo.Value, newValue);
				}
			}
		}

		private void OnDragEnter(object sender, DragEventArgs e)
		{
			e.Handled = true;
			e.Effects = DragDropEffects.None;
			if (e.Data.GetDataPresent("assetEntry"))
			{
				if (e.Data.GetData("assetEntry") is CAssetEntryViewModel assetEntry)
				{
					if (assetEntry.Asset.GetType() == m_assetType)
					{
						e.Effects = DragDropEffects.Link;
					}
				}
			}
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			e.Handled = true;
			e.Effects = DragDropEffects.None;
			if (e.Data.GetDataPresent("assetEntry"))
			{
				if (e.Data.GetData("assetEntry") is CAssetEntryViewModel assetEntry)
				{
					if (assetEntry.Asset.GetType() == m_assetType)
					{
						e.Effects = DragDropEffects.Link;
					}
				}
			}
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent("assetEntry"))
			{
				return;
			}

			if (e.Data.GetData("assetEntry") is CAssetEntryViewModel assetEntry)
			{
				if (assetEntry.Asset.GetType() == m_assetType)
				{
					AssetSelector.SelectedItem = assetEntry.Asset;
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

		private Type m_assetType;
		private bool m_bItemSourcesSet;
		private readonly List<CAsset> m_availableAssets = new List<CAsset>();
		private readonly MethodInfo m_getAssetsMethod = typeof(CAssetRegistry).GetMethod("GetAssetsLoaded");
		private object m_oldValue;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
