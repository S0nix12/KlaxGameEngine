using KlaxCore.EditorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KlaxEditor.UserControls.InspectorControls
{
	public delegate void PropertySetterDelegate(CObjectProperty property, object oldValue, object newValue, bool bRecordUndoRedo);

	public interface IPropertyInspector
	{
		void Lock(bool bIsLocked);
		void Unfocus();
		void ResizeColumns(GridLength leftColumn);

		bool DispatchSetter { get; set; }
		PropertySetterDelegate PropertySetter { get; set; }
	}
}
