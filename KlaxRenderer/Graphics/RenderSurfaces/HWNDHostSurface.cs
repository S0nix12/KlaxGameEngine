using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics.RenderSurfaces
{
    public class HWNDHostSurface : IRenderSurface
    {
        public HWNDHostSurface(IntPtr hwnd, int width, int height, int left, int top)
        {
            m_HWND = hwnd;
            m_width = width;
            m_height = height;
	        m_left = left;
	        m_top = top;
		}

        public int GetHeight()
        {
            return m_height;
        }

        public IntPtr GetHWND()
        {
            return m_HWND;
        }

        public int GetWidth()
        {
            return m_width;
        }

	    public int GetTop()
	    {
		    return m_top;
	    }

	    public int GetLeft()
	    {
		    return m_left;
	    }

        private IntPtr m_HWND;
        private int    m_height;
        private int    m_width;
	    private int    m_left;
		private int    m_top;
    }
}
