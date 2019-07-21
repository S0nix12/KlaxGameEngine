using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxRenderer.Graphics
{
    public class RenderFormSurface : IRenderSurface
    {
        public RenderFormSurface(RenderForm renderForm)
        {
	        Point topLeft = renderForm.PointToScreen(new Point(0, 0));
	        m_hwnd = renderForm.Handle;
	        m_width = renderForm.ClientSize.Width;
	        m_height = renderForm.ClientSize.Height;
        }
        public int GetHeight() => m_height;
        public IntPtr GetHWND() => m_hwnd;
        public int GetWidth() => m_width;

	    public int GetLeft()
	    {
		    return m_topLeft.X;
	    }

	    public int GetTop()
		{
			return m_topLeft.Y;
		}
		
	    private IntPtr m_hwnd;
	    private Point m_topLeft;
	    private int m_width;
	    private int m_height;
    }
}
