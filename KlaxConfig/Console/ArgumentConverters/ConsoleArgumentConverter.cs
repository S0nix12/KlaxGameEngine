using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxConfig.Console
{
    public abstract class CConsoleArgumentConverter
    {
        public CConsoleArgumentConverter(Type type)
        {
            Type = type;
        }

        public abstract bool ConvertFromString(string arg, out object outConvertedArg);
        public abstract string ConvertToString(object arg);

        public Type Type { get; }
    }
}
