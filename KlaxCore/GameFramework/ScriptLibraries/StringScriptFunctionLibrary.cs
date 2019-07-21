using KlaxShared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.GameFramework.ScriptLibraries
{
	[KlaxLibrary]
	public static class CStringScriptFunctionLibrary
	{
		[KlaxFunction(Category = "String", ParameterName1 = "A", IsImplicit = true)]
		public static int Length(string a)
		{
			return a.Length;
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "B", IsImplicit = true)]
		public static string Append(string a, string b)
		{
			return a + b;
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "B", ParameterName3 = "Index", IsImplicit = true)]
		public static string Insert(string a, string b, int index)
		{
			return a.Insert(index, b);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Index", ParameterName3 = "Length", IsImplicit = true)]
		public static string Substring(string a, int index, int length)
		{
			return a.Substring(index, length);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Index", ParameterName3 = "Length", IsImplicit = true)]
		public static string Remove(string a, int index, int length)
		{
			return a.Remove(index, length);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "B", ParameterName3 = "Replacement", IsImplicit = true)]
		public static string Replace(string a, string b, string c)
		{
			return a.Replace(b, c);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Index", IsImplicit = true)]
		public static string SubstringAll(string a, int index)
		{
			return a.Substring(index);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Substring", IsImplicit = true)]
		public static int Find(string a, string substring)
		{
			return a.IndexOf(substring);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Substring", IsImplicit = true)]
		public static bool Contains(string a, string substring)
		{
			return a.Contains(substring);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Substring", IsImplicit = true)]
		public static bool StartsWith(string a, string substring)
		{
			return a.StartsWith(substring);
		}

		[KlaxFunction(Category = "String", ParameterName1 = "A", ParameterName2 = "Substring", IsImplicit = true)]
		public static bool EndsWith(string a, string substring)
		{
			return a.EndsWith(substring);
		}
	}
}
