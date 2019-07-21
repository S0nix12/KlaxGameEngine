using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

internal static class MouseHook
{
	private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
	private static int _mouseHookHandle;
	private static HookProc _mouseDelegate;

	private static event MouseUpEventHandler MouseUp;
	public static event MouseUpEventHandler OnMouseUp
	{
		add
		{
			Subscribe();
			MouseUp += value;
		}
		remove
		{
			MouseUp -= value;
			Unsubscribe();
		}
	}

	private static event MouseMoveEventHandler MouseMove;
	public static event MouseMoveEventHandler OnMouseMove
	{
		add
		{
			Subscribe();
			MouseMove += value;
		}
		remove
		{
			MouseMove -= value;
			Unsubscribe();
		}
	}

	private static void Unsubscribe()
	{
		if (_mouseHookHandle != 0)
		{
			int result = UnhookWindowsHookEx(_mouseHookHandle);
			_mouseHookHandle = 0;
			_mouseDelegate = null;
			if (result == 0)
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new Win32Exception(errorCode);
			}
		}
	}

	private static void Subscribe()
	{
		if (_mouseHookHandle == 0)
		{
			_mouseDelegate = MouseHookProc;
			_mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL,
				_mouseDelegate,
				GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
				0);
			if (_mouseHookHandle == 0)
			{
				int errorCode = Marshal.GetLastWin32Error();
				throw new Win32Exception(errorCode);
			}
		}
	}

	private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			MSLLHOOKSTRUCT mouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
			if (wParam == WM_LBUTTONUP)
			{
				if (MouseUp != null)
				{
					MouseUp.Invoke(null, new Point(mouseHookStruct.pt.x, mouseHookStruct.pt.y));
				}
			}
			else if (wParam == WM_MOUSEMOVE)
			{
				var point = System.Windows.Forms.Cursor.Position;
				MouseMove?.Invoke(null, new Point(point.X, point.Y));
			}
		}
		return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
	}

	private const int WH_MOUSE_LL = 14;
	private const int WM_LBUTTONUP = 0x0202;
	private const int WM_MOUSEMOVE = 0x0200;

	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int x;
		public int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MSLLHOOKSTRUCT
	{
		public POINT pt;
		public uint mouseData;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto,
	   CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	private static extern int UnhookWindowsHookEx(int idHook);

	[DllImport("user32.dll", CharSet = CharSet.Auto,
		 CallingConvention = CallingConvention.StdCall)]
	private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetModuleHandle(string name);
}

public delegate void MouseUpEventHandler(object sender, Point p);
public delegate void MouseMoveEventHandler(object sender, Point delta);