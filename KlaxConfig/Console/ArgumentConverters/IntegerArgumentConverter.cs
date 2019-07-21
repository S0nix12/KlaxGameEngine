using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxConfig.Console
{
    public class CIntegerArgumentConverter : CConsoleArgumentConverter
    {
        public CIntegerArgumentConverter()
            : base(typeof(int))
        {
        }

        public override bool ConvertFromString(string arg, out object outConvertedArg)
        {
            if (int.TryParse(arg, out int outFloat))
            {
                outConvertedArg = outFloat;
                return true;
            }

            outConvertedArg = null;
            return false;
        }

        public override string ConvertToString(object arg)
        {
            return ((int)arg).ToString();
        }
    }
}
