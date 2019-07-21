using KlaxCore.Core;
using KlaxCore.EditorHelper;
using KlaxEditor.Annotations;
using KlaxEditor.Utility;
using KlaxEditor.ViewModels;
using KlaxShared.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KlaxEditor.UserControls.InspectorControls.Collections
{
	/// <summary>
	/// Interaction logic for ListInspector.xaml
	/// </summary>
	public partial class ListInspector : BaseInspectorControl, INotifyPropertyChanged
	{
		private class CListEntryViewModel : CViewModelBase
		{
			public CListEntryViewModel(int index)
			{
				m_index = index;
			}

			public CListEntryViewModel(int index, object value, Type type)
			{
				m_index = index;
				m_value = value;
				m_type = type;
			}

			private int m_index;
			public int Index
			{
				get { return m_index; }
				set { m_index = value; RaisePropertyChanged(); }
			}

			private object m_value;
			public object Value
			{
				get { return m_value; }
				set
				{
					m_value = value;
					RaisePropertyChanged();
				}
			}

			private Type m_type;
			public Type Type
			{
				get { return m_type; }
				set { m_type = value; RaisePropertyChanged(); }
			}
		}

		public ListInspector()
		{
			InitializeComponent();

			AddElementCommand = new CRelayCommand((arg) =>
			{
				object defaultObject = EditorKlaxScriptUtility.GetTypeDefault(m_collectionElementType);
				m_displayedList.Add(new CListEntryViewModel(m_displayedList.Count, defaultObject, m_collectionElementType));

				if (PropertyInfo.Value == null)
				{
					IList newList = (IList)Activator.CreateInstance(PropertyInfo.ValueType);

					foreach (var value in m_displayedList)
					{
						newList.Add(value.Value);
					}
					SetInspectorValue(PropertyInfo, null, newList, true);
				}
				else
				{
					if (m_inspector.DispatchSetter)
					{
						CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
						{
							IList list = PropertyInfo.GetOriginalValue<IList>();
							list.Add(defaultObject);
						});
					}
					else
					{
						IList list = PropertyInfo.GetOriginalValue<IList>();
						list.Add(defaultObject);
					}
				}

				CollectionList.ItemsSource = null;
				CollectionList.ItemsSource = m_displayedList;
				ListExpander.Header = m_displayedList.Count + " Elements";
			});

			ClearCommand = new CRelayCommand(arg =>
			{
				ClearCollection();
			});
		}

		public void OnElementValueChanged(StandaloneInspectorControl control, object newValue, ref bool outChangeValid)
		{
			CListEntryViewModel vm = control.DataContext as CListEntryViewModel;

			if (vm != null)
			{
				SetInspectorElementValue(vm.Index, newValue);
			}
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			m_collectionElementType = PropertyInfo.ValueType.GenericTypeArguments[0];

			if (info.Value != null)
			{
				dynamic list = info.Value;

				if (EqualsContentwise(info.Value as IList))
				{
					ListExpander.Header = m_displayedList.Count + " Elements";
					return;
				}

				int count = list.Count;
				if (m_displayedList.Count < count)
				{
					int oldSize = m_displayedList.Count;

					for (int i = oldSize; i < count; i++)
					{
						m_displayedList.Add(new CListEntryViewModel(i, null, m_collectionElementType));
					}
				}
				else if (m_displayedList.Count > count)
				{
					m_displayedList.RemoveRange(count, m_displayedList.Count - count);
				}

				for (int i = 0; i < count; i++)
				{
					m_displayedList[i].Value = list[i];
					m_displayedList[i].Type = m_collectionElementType;
				}

				ListExpander.Header = count + " Elements";
				CollectionList.ItemsSource = null;
				CollectionList.ItemsSource = m_displayedList;
			}
			else
			{
				//Collection is null. Show empty collection instead
				m_displayedList.Clear();
				ListExpander.Header = "0 Elements";
				CollectionList.ItemsSource = null;
				CollectionList.ItemsSource = m_displayedList;
			}
		}

		private bool EqualsContentwise(IList otherCollection)
		{
			if (otherCollection.Count != m_displayedList.Count)
				return false;

			for (int i = 0, count = otherCollection.Count; i < count; i++)
			{
				if (otherCollection[i] == null && m_displayedList[i].Value == null)
					continue;

				if ((otherCollection[i] == null && m_displayedList[i].Value != null) || (m_displayedList[i].Value == null && otherCollection[i] != null) || (!otherCollection[i].Equals(m_displayedList[i].Value)))
				{
					return false;
				}
			}

			return true;
		}

		private void SetInspectorElementValue(int index, object value)
		{
			if (m_inspector.DispatchSetter)
			{
				IList sourceList = PropertyInfo.GetOriginalValue<IList>();
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					sourceList[index] = value;
				});
			}
			else
			{
				IList sourceList = PropertyInfo.GetOriginalValue<IList>();
				sourceList[index] = value;
			}
		}

		private void RemoveInspectorElementValue(int index)
		{
			m_displayedList.RemoveAt(index);

			for (int i = index, count = m_displayedList.Count; i < count; i++)
			{
				m_displayedList[i].Index--;
			}

			if (m_inspector.DispatchSetter)
			{
				IList sourceList = PropertyInfo.GetOriginalValue<IList>();
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					sourceList.RemoveAt(index);
				});
			}
			else
			{
				IList sourceList = PropertyInfo.GetOriginalValue<IList>();
				sourceList.RemoveAt(index);
			}
		}

		private void ClearCollection()
		{
			m_displayedList.Clear();
			ListExpander.Header = "0 Elements";
			CollectionList.ItemsSource = null;
			CollectionList.ItemsSource = m_displayedList;


			if (m_inspector.DispatchSetter)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					IList sourceList = PropertyInfo.GetOriginalValue<IList>();
					if (sourceList == null)
						return;
					sourceList.Clear();
				});
			}
			else
			{
				IList sourceList = PropertyInfo.GetOriginalValue<IList>();
				if (sourceList == null)
					return;

				sourceList.Clear();
			}
		}

		private void StandaloneInspectorControl_OnLockedChanged(StandaloneInspectorControl control, bool bIsLocked)
		{
			m_inspector.Lock(bIsLocked);
		}

		private void RemoveElement_Click(object sender, RoutedEventArgs e)
		{
			int index = ((CListEntryViewModel)((MenuItem)sender).DataContext).Index;

			RemoveInspectorElementValue(index);

			ListExpander.Header = m_displayedList.Count + " Elements";
			CollectionList.ItemsSource = null;
			CollectionList.ItemsSource = m_displayedList;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ICommand AddElementCommand { get; }
		public ICommand ClearCommand { get; }

		private List<CListEntryViewModel> m_displayedList = new List<CListEntryViewModel>();

		private Type m_collectionElementType;
	}
}
