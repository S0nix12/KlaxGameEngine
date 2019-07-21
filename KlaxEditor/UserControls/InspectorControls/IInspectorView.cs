using KlaxEditor.ViewModels.EditorWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.EditorHelper;

namespace KlaxEditor.UserControls.InspectorControls
{
    public interface IInspectorView
    {
        void ShowInspectors(List<CObjectBase> propertyInfo);
        void LockInspector(bool bLocked);
        void ClearInspector();

		void SetInspectorVisible(bool bActive);
		void SetEntityName(string name);

		
	}
}
