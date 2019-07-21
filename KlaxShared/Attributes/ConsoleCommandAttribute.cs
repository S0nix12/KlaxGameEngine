using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsoleCommandAttribute : Attribute
    {
        public ConsoleCommandAttribute()
        { }

        public ConsoleCommandAttribute(string nameOverride)
        {
            NameOverride = nameOverride;
        }

        public string NameOverride { get; }
    }
}
