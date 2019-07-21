using KlaxCore.EditorHelper;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KlaxEditor.UserControls.InspectorControls
{
	public partial class StandaloneInspectorControl : UserControl, IPropertyInspector
	{
		public StandaloneInspectorControl()
		{
			InitializeComponent();

			PropertySetter = (property, oldValue, newValue, bUndo) =>
			{
				SetInspectorValue(newValue);
			};
		}

		private static void InspectorControlTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is StandaloneInspectorControl control)
			{
				control.SetInspectorType((Type)e.NewValue);
			}
		}

		private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is StandaloneInspectorControl control)
			{
				BaseInspectorControl inspectorControl = control.m_inspectorControl;
				if (inspectorControl != null)
				{
					inspectorControl.PropertyInfoChanged(new CObjectProperty("Standalone", null, null, e.NewValue, control.m_inspectorControlType, null, null));
				}
			}
		}

		public void SetInspectorType(Type type)
		{
			if (PropertyInspector.GetInspectorType(type, out InspectorType outResult))
			{
				BaseInspectorControl control = (BaseInspectorControl)Activator.CreateInstance(outResult.controlType);
				control.Init(this);

				object defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
				bool bIsSameType = (Value != null && Value.GetType() == type);
				control.PropertyInfoChanged(new CObjectProperty("Standalone", null, null, bIsSameType ? Value : defaultValue, type, null, null));

				m_inspectorControl = control;
				m_inspectorControlType = type;

				Presenter.Content = control;
			}
		}

		public void SetInspectorValue(object value)
		{
			bool bIsValidChange = true;
			OnValueChanged?.Invoke(this, value, ref bIsValidChange);

			if (bIsValidChange)
			{
				Value = value;
			}
			else
			{
				m_inspectorControl.SetValueOnly(Value);
			}
		}

		public void Lock(bool bIsLocked)
		{
			if (Locked != bIsLocked)
			{
				Locked = bIsLocked;
				OnLockedChanged?.Invoke(this, bIsLocked);
			}
		}

		public void Unfocus()
		{
			Focus();
		}

		public void ResizeColumns(GridLength leftColumn)
		{
			throw new NotImplementedException();
		}

		public bool DispatchSetter
		{
			get { return (bool)GetValue(DispatchSetterProperty); }
			set { SetValue(DispatchSetterProperty, value); }
		}

		public Type InspectorControlType
		{
			get { return (Type)GetValue(InspectorControlTypeProperty); }
			set { SetValue(InspectorControlTypeProperty, value); }
		}

		public object Value
		{
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public bool Locked
		{
			get { return (bool)GetValue(LockedProperty); }
			set { SetValue(LockedProperty, value); }
		}

		public PropertySetterDelegate PropertySetter { get; set; }

		public delegate void OnValueChangedSignature(StandaloneInspectorControl control, object newValue, ref bool outIsValidChange);
		public event OnValueChangedSignature OnValueChanged;
		public event Action<StandaloneInspectorControl, bool> OnLockedChanged;

		public static DependencyProperty InspectorControlTypeProperty = DependencyProperty.Register(nameof(InspectorControlType), typeof(Type), typeof(StandaloneInspectorControl), new PropertyMetadata(new PropertyChangedCallback(InspectorControlTypePropertyChanged)));
		public static DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(StandaloneInspectorControl), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));
		public static readonly DependencyProperty DispatchSetterProperty = DependencyProperty.Register(nameof(DispatchSetter), typeof(bool), typeof(StandaloneInspectorControl), new PropertyMetadata(true));
		public static readonly DependencyProperty LockedProperty = DependencyProperty.Register(nameof(Locked), typeof(bool), typeof(StandaloneInspectorControl), new PropertyMetadata(false));

		private BaseInspectorControl m_inspectorControl;
		private Type m_inspectorControlType;
	}
}
