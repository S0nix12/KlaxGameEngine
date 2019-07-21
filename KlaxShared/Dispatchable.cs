using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxShared
{
    public interface IDispatchable<T> where T : System.Enum
    {
        void Dispatch(T priority, Action action);
        bool IsInAuthoritativeThread();
    }
}
