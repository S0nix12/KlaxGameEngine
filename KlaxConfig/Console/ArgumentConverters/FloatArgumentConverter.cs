using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxConfig.Console
{
    public class CFloatArgumentConverter : CConsoleArgumentConverter
    {
        public CFloatArgumentConverter()
            : base(typeof(float))
        {
        }

        public override bool ConvertFromString(string arg, out object outConvertedArg)
        {
            if (float.TryParse(arg, out float outFloat))
            {
                outConvertedArg = outFloat;
                return true;
            }

            outConvertedArg = null;
            return false;
        }

        public override string ConvertToString(object arg)
        {
            return ((float)arg).ToString();
        }
    }
}
