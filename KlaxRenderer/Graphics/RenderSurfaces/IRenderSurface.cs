using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics
{
    public interface IRenderSurface
    {
        int GetHeight();
        int GetWidth();
	    int GetLeft();
	    int GetTop();
		IntPtr GetHWND();
    }
}
