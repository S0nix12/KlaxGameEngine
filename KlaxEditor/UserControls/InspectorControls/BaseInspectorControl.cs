using KlaxCore.Core;
using KlaxEditor.Utility.UndoRedo;
using KlaxEditor.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using KlaxCore.EditorHelper;
using KlaxShared.Attributes;

namespace KlaxEditor.UserControls.InspectorControls
{
	public class BaseInspectorControl : UserControl
	{
		[CVar]
		public static float FloatInspectorDragMultiplier { get; set; } = 0.08f;
		[CVar]
		public static float IntegerInspectorDragMultiplier { get; set; } = 0.08f;

		public void Init(IPropertyInspector inspector)
		{
			m_inspector = inspector;
		}

		public virtual void SetValueOnly(object value) { }

		public virtual void PropertyInfoChanged(CObjectProperty info)
		{
			PropertyInfo = info;
		}

		public void SetInspectorValue<T>(CObjectProperty property, T oldValue, T newValue, bool bRecordUndoAction = true)
		{
			m_inspector.PropertySetter(property, oldValue, newValue, bRecordUndoAction);
		}


		protected IPropertyInspector m_inspector;
		protected CObjectProperty PropertyInfo { get; private set; }
	}
}
