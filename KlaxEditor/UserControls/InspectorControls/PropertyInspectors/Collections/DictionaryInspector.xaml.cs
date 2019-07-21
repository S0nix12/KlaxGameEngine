using KlaxCore.Core;
using KlaxCore.EditorHelper;
using KlaxEditor.Annotations;
using KlaxEditor.Utility;
using KlaxEditor.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace KlaxEditor.UserControls.InspectorControls.PropertyInspectors.Collections
{
	/// <summary>
	/// Interaction logic for DictionaryInspector.xaml
	/// </summary>
	public partial class DictionaryInspector : BaseInspectorControl, INotifyPropertyChanged
	{
		private class CDictionaryEntryViewModel : CViewModelBase
		{
			public CDictionaryEntryViewModel(int index, object key, object value, Type keyType, Type valueType)
			{
				m_index = index;
				m_key = key;
				m_value = value;
				m_keyType = keyType;
				m_valueType = valueType;
			}

			private int m_index;
			public int Index
			{
				get { return m_index; }
				set { m_index = value; RaisePropertyChanged(); }
			}

			private object m_key;
			public object Key
			{
				get { return m_key; }
				set { m_key = value; RaisePropertyChanged(); }
			}

			private object m_value;
			public object Value
			{
				get { return m_value; }
				set { m_value = value; RaisePropertyChanged(); }
			}

			private Type m_keyType;
			public Type KeyType
			{
				get { return m_keyType; }
				set { m_keyType = value; RaisePropertyChanged(); }
			}

			private Type m_valueType;
			public Type ValueType
			{
				get { return m_valueType; }
				set { m_valueType = value; RaisePropertyChanged(); }
			}
		}

		public DictionaryInspector()
		{
			AddElementCommand = new CRelayCommand((arg) =>
			{
				if (m_addKeyValue == null)
				{
					LogUtility.Log("The dictionary {0}", PropertyInfo.Name);
					return;
				}

				if (m_displayedList.Any(vm => SafeEquals(m_addKeyValue, vm.Key)))
				{
					LogUtility.Log("The dictionary {0} already contains an element with a default key! You cannot add a new element as long as the default key exists.", PropertyInfo.Name);
					return;
				}

				object defaultValueObject = EditorKlaxScriptUtility.GetTypeDefault(m_valueType);
				m_displayedList.Add(new CDictionaryEntryViewModel(m_displayedList.Count, m_addKeyValue, defaultValueObject, m_keyType, m_valueType));


				if (PropertyInfo.Value == null)
				{
					IDictionary newDictionary = (IDictionary)Activator.CreateInstance(PropertyInfo.ValueType);

					foreach (var viewModel in m_displayedList)
					{
						newDictionary.Add(viewModel.Key, viewModel.Value);
					}
					SetInspectorValue(PropertyInfo, null, newDictionary, true);
				}
				else
				{
					if (m_inspector.DispatchSetter)
					{
						CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
						{
							IDictionary dictionary = PropertyInfo.GetOriginalValue<IDictionary>();
							dictionary.Add(m_addKeyValue, defaultValueObject);
						});
					}
					else
					{
						IDictionary dictionary = PropertyInfo.GetOriginalValue<IDictionary>();
						dictionary.Add(m_addKeyValue, defaultValueObject);
					}
				}

				CollectionList.ItemsSource = null;
				CollectionList.ItemsSource = m_displayedList;
				HeaderText = m_displayedList.Count + " Elements";
			});

			ClearCommand = new CRelayCommand(arg =>
			{
				ClearCollection();
			});

			InitializeComponent();
		}

		public void OnElementValueChanged(StandaloneInspectorControl control, object newValue, ref bool outChangeValid)
		{
			CDictionaryEntryViewModel vm = control.DataContext as CDictionaryEntryViewModel;

			if (vm != null)
			{
				SetInspectorElementValue(vm.Key, newValue);
			}
		}

		public void OnElementKeyChanged(StandaloneInspectorControl control, object newValue, ref bool outChangeValid)
		{
			CDictionaryEntryViewModel vm = control.DataContext as CDictionaryEntryViewModel;

			if (vm != null)
			{
				object oldKey = vm.Key;
				if (m_displayedList.Any(viewModel => SafeEquals(viewModel.Key, newValue)))
				{
					//New key is already part of dictionary. Revert key change
					outChangeValid = false;
				}
				else
				{
					SetInspectorElementKey(oldKey, newValue, vm.Value);
				}
			}
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			KeyType = PropertyInfo.ValueType.GenericTypeArguments[0];
			ValueType = PropertyInfo.ValueType.GenericTypeArguments[1];

			if (info.Value != null)
			{
				dynamic dictionary = info.Value;
				IDictionary dictionaryInterface = info.Value as IDictionary;

				var keyCollection = dictionaryInterface.Keys;
				var valueCollection = dictionaryInterface.Values;

				object[] keys = new object[keyCollection.Count];
				object[] values = new object[valueCollection.Count];

				keyCollection.CopyTo(keys, 0);
				valueCollection.CopyTo(values, 0);

				if (EqualsContentwise(dictionaryInterface, keys, values))
				{
					HeaderText = m_displayedList.Count + " Elements";
					return;
				}

				int count = dictionary.Count;
				if (m_displayedList.Count < count)
				{
					int oldSize = m_displayedList.Count;

					for (int i = oldSize; i < count; i++)
					{
						m_displayedList.Add(new CDictionaryEntryViewModel(i, keys[i], values[i], m_keyType, m_valueType));
					}
				}
				else if (m_displayedList.Count > count)
				{
					m_displayedList.RemoveRange(count, m_displayedList.Count - count);
				}

				for (int i = 0; i < count; i++)
				{
					CDictionaryEntryViewModel model = m_displayedList[i];
					model.Index = i;
					model.Key = keys[i];
					model.Value = values[i];
					model.KeyType = m_keyType;
					model.ValueType = m_valueType;
				}

				HeaderText = count + " Elements";
				CollectionList.ItemsSource = null;
				CollectionList.ItemsSource = m_displayedList;
			}
			else
			{
				//Collection is null. Show empty collection instead
				m_displayedList.Clear();
				HeaderText = "0 Elements";
				CollectionList.ItemsSource = null;
				CollectionList.ItemsSource = m_displayedList;
			}
		}

		private bool SafeEquals(object a, object b)
		{
			if (a == null && b == null)
				return true;

			if ((a == null && b != null) || (b == null && a != null) || (!a.Equals(b)))
			{
				return false;
			}

			return true;
		}

		private bool EqualsContentwise(IDictionary otherDictionary, object[] keys, object[] values)
		{

			if (otherDictionary.Count != m_displayedList.Count)
				return false;

			for (int i = 0, count = m_displayedList.Count; i < count; i++)
			{
				object key = m_displayedList[i].Key;
				object value = m_displayedList[i].Value;

				if (ContainsElement(keys, key, out int outIndex))
				{
					if (!SafeEquals(values[i], value))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		private bool ContainsElement(object[] array, object element, out int outIndex)
		{
			for (int i = 0, count = array.Length; i < count; i++)
			{
				if (SafeEquals(element, array[i]))
				{
					outIndex = i;
					return true;
				}
			}

			outIndex = -1;
			return false;
		}

		private void SetInspectorElementKey(object oldKey, object key, object value)
		{
			if (m_inspector.DispatchSetter)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					IDictionary sourceDictionary = PropertyInfo.GetOriginalValue<IDictionary>();
					sourceDictionary.Remove(oldKey);
					sourceDictionary[key] = value;
				});
			}
			else
			{
				IDictionary sourceDictionary = PropertyInfo.GetOriginalValue<IDictionary>();
				sourceDictionary.Remove(oldKey);
				sourceDictionary[key] = value;
			}
		}

		private void SetInspectorElementValue(object key, object value)
		{
			if (m_inspector.DispatchSetter)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					IDictionary sourceDictionary = PropertyInfo.GetOriginalValue<IDictionary>();
					sourceDictionary[key] = value;
				});
			}
			else
			{
				IDictionary sourceDictionary = PropertyInfo.GetOriginalValue<IDictionary>();
				sourceDictionary[key] = value;
			}
		}

		private void RemoveInspectorElementValue(int index, object key)
		{
			m_displayedList.RemoveAt(index);

			for (int i = index, count = m_displayedList.Count; i < count; i++)
			{
				m_displayedList[i].Index--;
			}

			if (m_inspector.DispatchSetter)
			{
				IDictionary sourceList = PropertyInfo.GetOriginalValue<IDictionary>();
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					sourceList.Remove(key);
				});
			}
			else
			{
				IDictionary sourceList = PropertyInfo.GetOriginalValue<IDictionary>();
				sourceList.Remove(key);
			}
		}

		private void ClearCollection()
		{
			m_displayedList.Clear();
			HeaderText = "0 Elements";
			CollectionList.ItemsSource = null;
			CollectionList.ItemsSource = m_displayedList;

			if (m_inspector.DispatchSetter)
			{
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					IDictionary sourceDict = PropertyInfo.GetOriginalValue<IDictionary>();
					if (sourceDict == null)
						return;

					sourceDict.Clear();
				});
			}
			else
			{
				IDictionary sourceDict = PropertyInfo.GetOriginalValue<IDictionary>();
				if (sourceDict == null)
					return;

				sourceDict.Clear();
			}
		}

		private void StandaloneInspectorControl_OnLockedChanged(StandaloneInspectorControl control, bool bIsLocked)
		{
			m_inspector.Lock(bIsLocked);
		}

		private void RemoveElement_Click(object sender, RoutedEventArgs e)
		{
			CDictionaryEntryViewModel vm = (CDictionaryEntryViewModel)((MenuItem)sender).DataContext;

			RemoveInspectorElementValue(vm.Index, vm.Key);

			HeaderText = m_displayedList.Count + " Elements";
			CollectionList.ItemsSource = null;
			CollectionList.ItemsSource = m_displayedList;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private ICommand m_addElementCommand;
		public ICommand AddElementCommand
		{
			get { return m_addElementCommand; }
			set { m_addElementCommand = value; RaisePropertyChanged(); }
		}

		private ICommand m_clearCommand;
		public ICommand ClearCommand
		{
			get { return m_clearCommand; }
			set { m_clearCommand = value; RaisePropertyChanged(); }
		}

		private Type m_keyType;
		public Type KeyType
		{
			get { return m_keyType; }
			set
			{
				if (m_keyType != value && value != null)
				{
					AddKeyValue = value.IsValueType ? Activator.CreateInstance(value) : null;
				}
				m_keyType = value;
				RaisePropertyChanged();
			}
		}

		private Type m_valueType;
		public Type ValueType
		{
			get { return m_valueType; }
			set { m_valueType = value; RaisePropertyChanged(); }
		}

		private object m_addKeyValue;
		public object AddKeyValue
		{
			get { return m_addKeyValue; }
			set { m_addKeyValue = value; RaisePropertyChanged(); }
		}

		private string m_headerText;
		public string HeaderText
		{
			get { return m_headerText; }
			set { m_headerText = value; RaisePropertyChanged(); }
		}

		private List<CDictionaryEntryViewModel> m_displayedList = new List<CDictionaryEntryViewModel>();
	}
}
