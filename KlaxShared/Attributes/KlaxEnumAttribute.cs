using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Attributes
{
	public class KlaxEnumAttribute : KlaxScriptTypeAttribute
	{
		public KlaxEnumAttribute()
		{
			Color = new SharpDX.Color(0, 112, 28, 255);
		}
	}
}
