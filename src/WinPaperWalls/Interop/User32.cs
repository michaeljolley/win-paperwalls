using System.Runtime.InteropServices;

namespace WinPaperWalls.Interop;

internal static partial class PInvoke
{
	internal static partial class User32
	{
		[LibraryImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static partial bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

		internal enum WindowShowStyle
		{
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_RESTORE = 9
		}
	}
}
