using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxShared.Attributes;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class BasicScriptFunctionLibrary
	{
		[KlaxFunction(Category = "Basic", Tooltip = "Checks if a given object is not null")]
		public static bool IsValid(object objectToCheck)
		{
			return objectToCheck != null;
		}

		[KlaxFunction(Category = "Basic", Tooltip = "Returns the type of the given object")]
		public static Type GetType(object target)
		{
			return target.GetType();
		}

		[KlaxFunction(Category = "Basic", Tooltip = "Calls the ToString() method on given object", IsImplicit = true)]
		public static string ToString(object target)
		{
			return target != null ? target.ToString() : string.Empty;
		}
	}
}
