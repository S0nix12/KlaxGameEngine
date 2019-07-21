using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxIO.Input;
using KlaxShared.Attributes;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class InputScriptFunctionLibrary
	{
		[KlaxFunction(Category = "Input", IsImplicit = true)]
		public static bool IsButtonPressed(EInputButton button)
		{
			if (Input.IsInputClassActive(EInputClass.Default))
			{
				return Input.IsButtonPressed(button);
			}

			return false;
		}

		[KlaxFunction(Category = "Input", IsImplicit = true)]
		public static float GetAxisValue(EInputAxis axis)
		{
			if (Input.IsInputClassActive(EInputClass.Default))
			{
				return Input.GetNativeAxisValue(axis);
			}

			return 0.0f;
		}

		[KlaxFunction(Category = "Input", IsImplicit = true)]
		public static bool IsButtonEqual(EInputButton a, EInputButton b)
		{
			return a == b;
		}

		[KlaxFunction(Category = "Input", ParameterName1 = "Visible")]
		public static void SetCursorVisibility(bool bIsVisible)
		{
			Input.SetCursorVisibility(bIsVisible);
		}
	}
}
