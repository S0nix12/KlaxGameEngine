using KlaxCore.KlaxScript;
using KlaxEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KlaxEditor.Utility
{
	public static class EditorKlaxScriptUtility
	{
		/// <summary>
		/// Returns the KlaxScript type name if it exists, otherwise just the type name
		/// </summary>
		/// <param name="nativeType">The type whose editor name should be returned</param>
		/// <returns>Display name of given type</returns>
		public static string GetTypeName(Type nativeType)
		{
			if (CKlaxScriptRegistry.Instance.TryGetTypeInfo(nativeType, out CKlaxScriptTypeInfo outInfo))
			{
				return outInfo.Name;
			}

			if (nativeType == null)
			{
				return "Invalid";
			}

			return nativeType.Name;
		}

		internal static object GetTypeDefault(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}
	}
}
