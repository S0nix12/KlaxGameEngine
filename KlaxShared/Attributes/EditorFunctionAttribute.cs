using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class EditorFunctionAttribute : Attribute
	{
		public string DisplayName { get; set; }
		public string Tooltip { get; set; }
		public string Category { get; set; }
		public int CategoryPriority { get; set; }
	}
}
