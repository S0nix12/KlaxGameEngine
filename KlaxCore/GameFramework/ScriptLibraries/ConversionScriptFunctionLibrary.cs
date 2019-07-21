using KlaxShared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class ConversionScriptFunctionLibrary
	{
		[KlaxFunction(Category = "Int", DisplayName = "ToFloat", IsImplicit = true, ParameterName1 = "A")]
		public static float ToFloat(int a) { return a; }

		[KlaxFunction(Category = "Float", DisplayName = "ToInt", IsImplicit = true, ParameterName1 = "A")]
		public static int ToInt(float a) { return (int)a; }
	}
}
