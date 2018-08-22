using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxCore.Core
{
    public class ValueEvolution<T>
    {
        public ValueEvolution(T _max, T _min, T _start, T _step, T _backStep)
        {
            max = _max;
            min = _min;
            current = _start;
            step = _step;
            backStep = _backStep;
        }

        public void Step()
        {
            dynamic dCurrent = current;
            if(bUp)
            {
                dCurrent += step;
                if(dCurrent >= max)
                {
                    dCurrent = max;
                    bUp = false;
                }
            }
            else
            {
                dCurrent -= backStep;
                if(dCurrent <= min)
                {
                    dCurrent = min;
                    bUp = true;
                }
            }

            current = dCurrent;
        }

        public T GetCurrent()
        {
            return current;
        }

        private T max;
        private T min;
        private T current;
        private T step;
        private T backStep;

        private bool bUp;
    }
}
