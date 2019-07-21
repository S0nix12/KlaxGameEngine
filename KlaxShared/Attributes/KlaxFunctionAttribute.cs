using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class KlaxFunctionAttribute : Attribute
	{
		public string DisplayName { get; set; }
		public string Tooltip { get; set; }
		public string Category { get; set; } = "Default";
		public bool IsImplicit { get; set; }

		public string ParameterName1 { get; set; }
		public string ParameterName2 { get; set; }
		public string ParameterName3 { get; set; }
		public string ParameterName4 { get; set; }
		public string ParameterName5 { get; set; }
		public string ParameterName6 { get; set; }
		public string ParameterName7 { get; set; }
		public string ParameterName8 { get; set; }
		public string ParameterName9 { get; set; }
		public string ParameterName10 { get; set; }
	}
}
