using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class KlaxPropertyAttribute : Attribute
    {
        public string DisplayName { get; set; }
		public string Tooltip { get; set; }
		public string Category { get; set; } = "Default";
        public int CategoryPriority { get; set; }
		public bool IsReadOnly { get; set; }
    }
}
