using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxConfig.Console
{
    public class CStringArgumentConverter : CConsoleArgumentConverter
    {
        public CStringArgumentConverter()
            : base(typeof(string))
        {
        }

        public override bool ConvertFromString(string arg, out object outConvertedArg)
        {
            outConvertedArg = arg;
            return true;
        }

        public override string ConvertToString(object arg)
        {
            return (string)arg;
        }
    }
}
