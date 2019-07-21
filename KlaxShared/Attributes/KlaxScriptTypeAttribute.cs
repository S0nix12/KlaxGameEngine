using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Attributes
{
	public class KlaxScriptTypeAttribute : Attribute
	{
		public string Name { get; set; }
		public Color Color { get; set; }
	}
}
