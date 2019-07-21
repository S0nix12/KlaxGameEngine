using KlaxCore.Core;
using KlaxCore.EditorHelper;
using KlaxCore.KlaxScript;
using KlaxEditor.UserControls.InspectorControls.Collections;
using KlaxEditor.UserControls.InspectorControls.PropertyInspectors.Collections;
using KlaxEditor.Utility.UndoRedo;
using KlaxIO.AssetManager.Assets;
using KlaxShared;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using KlaxEditor.UserControls.InspectorControls.PropertyInspectors;

namespace KlaxEditor.UserControls.InspectorControls
{
	public class CInspectorControlInfo
	{
		public CInspectorControlInfo(Type type, CObjectProperty property, CCategoryInfo category, BaseInspectorControl control, InspectorPropertyName nameControl)
		{
			categoryInfo = category;
			inspectorControl = control;
			valueType = type;
			objectProperty = property;
			propertyNameControl = nameControl;
		}

		public Type valueType;
		public CCategoryInfo categoryInfo;
		public BaseInspectorControl inspectorControl;
		public InspectorPropertyName propertyNameControl;
		public CObjectProperty objectProperty;
	}

	public class InspectorType
	{
		public InspectorType(Type controlType, object defaultValue)
		{
			this.controlType = controlType;
			this.defaultValue = defaultValue;
		}

		public Type controlType;
		public object defaultValue;
	}

	public partial class PropertyInspector : UserControl, IPropertyInspector
	{
		public PropertyInspector()
		{
			InitializeComponent();

			PropertySetter = DefaultSetter;
		}

		static PropertyInspector()
		{
			void RegisterInspectorControl(Type userControlType, Type targetType, object defaultValue)
			{
				if (userControlType.IsSubclassOf(typeof(BaseInspectorControl)))
				{
					InspectorTypeMap.Add(targetType, new InspectorType(userControlType, defaultValue));
				}
			}

			RegisterInspectorControl(typeof(FloatInspector), typeof(float), 0.0f);
			RegisterInspectorControl(typeof(BoolInspector), typeof(bool), false);
			RegisterInspectorControl(typeof(ColorInspector), typeof(Color4), Color4.White);
			RegisterInspectorControl(typeof(IntegerInspector), typeof(int), 0);
			RegisterInspectorControl(typeof(StringInspector), typeof(string), "");
			RegisterInspectorControl(typeof(Vector2Inspector), typeof(Vector2), Vector2.Zero);
			RegisterInspectorControl(typeof(Vector3Inspector), typeof(Vector3), Vector3.Zero);
			RegisterInspectorControl(typeof(QuaternionInspector), typeof(Quaternion), Quaternion.Identity);
			RegisterInspectorControl(typeof(TypeInspector), typeof(CKlaxScriptTypeInfo), null);
			RegisterInspectorControl(typeof(TypeInspector), typeof(Type), null);
			RegisterInspectorControl(typeof(SubtypeOfInspector), typeof(SSubtypeOf<>), null);
			RegisterInspectorControl(typeof(HashedNameInspector), typeof(SHashedName), SHashedName.Empty);

			RegisterInspectorControl(typeof(AssetReferenceInspector), typeof(CAssetReference<>), null);
			RegisterInspectorControl(typeof(ListInspector), typeof(List<>), null);
			RegisterInspectorControl(typeof(DictionaryInspector), typeof(Dictionary<,>), null);
		}

		private void DefaultSetter(CObjectProperty property, object oldValue, object newValue, bool bRecordUndoAction)
		{
			if (!ReferenceEquals(oldValue, null) && oldValue.Equals(newValue))
				return;

			void SetValueInternal(CObjectProperty prop, object value)
			{
				if (DispatchSetter)
				{
					CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
					{
						if (property.FieldInfo != null)
						{
							property.FieldInfo.SetValue(property.Target, value);
						}
						else
						{
							property.PropertyInfo.SetValue(property.Target, value);
						}
					});
				}
				else
				{
					if (property.FieldInfo != null)
					{
						property.FieldInfo.SetValue(property.Target, value);
					}
					else
					{
						property.PropertyInfo.SetValue(property.Target, value);
					}
				}
			}

			void Undo()
			{
				SetValueInternal(property, oldValue);
			}
			void Redo()
			{
				SetValueInternal(property, newValue);
			}

			if (bRecordUndoAction)
			{
				CUndoItem item = new CRelayUndoItem(Undo, Redo);
				UndoRedoUtility.Record(item);
			}

			Redo();
		}

		public void Unfocus()
		{
			MainBorder.Focus();
		}

		public static bool GetInspectorType(Type objectType, out InspectorType outInspectorType)
		{
			if (InspectorTypeMap.TryGetValue(objectType, out outInspectorType))
			{
				return true;
			}
			else if (objectType.IsGenericType)
			{
				if (InspectorTypeMap.TryGetValue(objectType.GetGenericTypeDefinition(), out outInspectorType))
				{
					return true;
				}
			}
			else if (objectType.IsEnum)
			{
				outInspectorType = EnumInspectorType;
				return true;
			}

			return false;
		}

		public void ShowInspectors(IEnumerable<CObjectBase> baseInfos)
		{
			if (m_bLocked)
				return;

			inspectorTypes.Clear();
			supportedProperties.Clear();

			foreach (var baseObj in baseInfos)
			{
				if (baseObj is CObjectProperty prop)
				{
					if (GetInspectorType(prop.ValueType, out InspectorType outResult))
					{
						inspectorTypes.Add(outResult);
						supportedProperties.Add(prop);
					}
				}
				else if (baseObj is CObjectFunction func)
				{

				}
			}

			bool bValidLayout = !m_bLayoutInvalidated;
			bValidLayout &= m_controls.Count == inspectorTypes.Count;

			if (bValidLayout)
			{
				for (int i = 0, count = m_controls.Count; i < count; i++)
				{
					Type existingType = m_controls[i].objectProperty.ValueType;

					if (!supportedProperties[i].Category.Equals(m_controls[i].categoryInfo) || supportedProperties[i].ValueType != existingType)
					{
						bValidLayout = false;
						break;
					}
				}
			}
			
			if (bValidLayout)
			{
				//Reuse layout
				for (int i = 0, count = m_controls.Count; i < count; i++)
				{
					CInspectorControlInfo info = m_controls[i];
					info.objectProperty = supportedProperties[i];
					info.inspectorControl.PropertyInfoChanged(info.objectProperty);
					info.propertyNameControl.PropertyInfoChanged(info.objectProperty);
				}
			}
			else
			{
				ClearInspector();

				for (int i = 0, count = supportedProperties.Count; i < count; i++)
				{
					CObjectProperty property = supportedProperties[i];

					CBaseInspectorCategory categoryControl = null;
					if (!m_categoryMap.TryGetValue(property.Category, out categoryControl))
					{
						if (UseSimpleCategoryDisplay)
						{
							categoryControl = new SimpleInspectorCategory(property.Category, this);
						}
						else
						{
							categoryControl = new ExpandableInspectorCategory(property.Category, this);
						}

						m_categoryMap.Add(property.Category, categoryControl);
						m_categories.Add(categoryControl);

						if (m_categoryLeftColumnSize.HasValue)
						{
							categoryControl.ResizeColumns(m_categoryLeftColumnSize.Value);
						}
					}

					Type inspectorControlType = inspectorTypes[i].controlType;
					BaseInspectorControl newPropertyInspector = (BaseInspectorControl)Activator.CreateInstance(inspectorControlType);
					newPropertyInspector.Init(this);
					newPropertyInspector.PropertyInfoChanged(property);

					InspectorPropertyName newNameControl = new InspectorPropertyName(property, inspectorTypes[i].defaultValue, property.Name, newPropertyInspector);
					CInspectorControlInfo info = new CInspectorControlInfo(inspectorControlType, property, property.Category, newPropertyInspector, newNameControl);
					categoryControl.AddPropertyInspector(newPropertyInspector, newNameControl);

					m_controls.Add(info);
				}

				List<CBaseInspectorCategory> categories = new List<CBaseInspectorCategory>(m_categories);
				int Comp(CBaseInspectorCategory x, CBaseInspectorCategory y)
				{
					return x.Priority - y.Priority;
				}
				categories.Sort(Comp);
				CategoryPanel.Children.Clear();

				foreach (var element in categories)
				{
					CategoryPanel.Children.Add(element);
				}
			}

			m_currentlyDisplayedList = baseInfos;
			m_bLayoutInvalidated = false;
		}

		public void ResizeColumns(GridLength leftColumn)
		{
			foreach (var category in m_categories)
			{
				category.ResizeColumns(leftColumn);
			}

			m_categoryLeftColumnSize = leftColumn;
		}

		public void ClearInspector()
		{
			CategoryPanel.Children.Clear();
			m_controls.Clear();
			m_categoryMap.Clear();
			m_categories.Clear();
			m_currentlyDisplayedList = null;
		}

		public void Lock(bool bLocked)
		{
			m_bLocked = bLocked;
		}

		private static void PropertiesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is PropertyInspector inspector)
			{
				if (e.NewValue == null)
				{
					inspector.ClearInspector();
				}
				else
				{
					inspector.ShowInspectors(e.NewValue as IEnumerable<CObjectBase>);
				}
			}
		}

		private static void UseSimpleCategoriesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is PropertyInspector inspector)
			{
				bool newValue = (bool)e.NewValue;
				if (inspector.m_currentlyDisplayedList != null)
				{
					inspector.m_bLayoutInvalidated = true;
					inspector.ShowInspectors(inspector.m_currentlyDisplayedList);
				}
			}
		}

		private static void NameColumnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is PropertyInspector inspector)
			{
				double newValue = (double)e.NewValue;
				
				inspector.m_categoryLeftColumnSize = new GridLength(newValue);
				inspector.m_bLayoutInvalidated = true;
				inspector.ShowInspectors(inspector.m_currentlyDisplayedList);
			}
		}

		private IEnumerable<CObjectBase> m_currentlyDisplayedList;

		private List<InspectorType> inspectorTypes = new List<InspectorType>(8);
		private List<CObjectProperty> supportedProperties = new List<CObjectProperty>(16);

		private List<CInspectorControlInfo> m_controls = new List<CInspectorControlInfo>(32);
		private Dictionary<CCategoryInfo, CBaseInspectorCategory> m_categoryMap = new Dictionary<CCategoryInfo, CBaseInspectorCategory>(12);
		private List<CBaseInspectorCategory> m_categories = new List<CBaseInspectorCategory>(12);
		private GridLength? m_categoryLeftColumnSize;

		private bool m_bLocked;
		private bool m_bLayoutInvalidated;

		public static Dictionary<Type, InspectorType> InspectorTypeMap { get; } = new Dictionary<Type, InspectorType>();
		public PropertySetterDelegate PropertySetter { get; set; }

		public static InspectorType EnumInspectorType { get; } = new InspectorType(typeof(EnumInspector), 0);

		public static readonly DependencyProperty DispatchSetterProperty = DependencyProperty.Register("DispatchSetter", typeof(bool), typeof(PropertyInspector), new PropertyMetadata(true));
		public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register(nameof(Properties), typeof(IEnumerable<CObjectBase>), typeof(PropertyInspector), new PropertyMetadata(new PropertyChangedCallback(PropertiesPropertyChanged)));
		public static readonly DependencyProperty UseSimpleCategoriesProperty = DependencyProperty.Register(nameof(UseSimpleCategoryDisplay), typeof(bool), typeof(PropertyInspector), new UIPropertyMetadata(new PropertyChangedCallback(UseSimpleCategoriesPropertyChanged)));
		public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(nameof(NameColumnWidth), typeof(double), typeof(PropertyInspector), new UIPropertyMetadata(new PropertyChangedCallback(NameColumnWidthPropertyChanged)));

		public double NameColumnWidth
		{
			get { return (double)GetValue(NameColumnWidthProperty); }
			set { SetValue(NameColumnWidthProperty, value); }
		}

		public bool UseSimpleCategoryDisplay
		{
			get { return (bool)GetValue(UseSimpleCategoriesProperty); }
			set { SetValue(UseSimpleCategoriesProperty, value); }
		}

		public IEnumerable<CObjectBase> Properties
		{
			get { return (IEnumerable<CObjectBase>)GetValue(PropertiesProperty); }
			set { SetValue(PropertiesProperty, value); }
		}

		public bool DispatchSetter
		{
			get { return (bool)GetValue(DispatchSetterProperty); }
			set { SetValue(DispatchSetterProperty, value); }
		}
	}
}
