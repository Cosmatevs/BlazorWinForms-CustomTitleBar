using System.Runtime.InteropServices;

namespace BlazorWinFormsCustomTitleBar;
public static partial class WindowsAPI
{
    public enum WindowMessage
    {
        WM_NCCALCSIZE = 0x0083,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCLBUTTONDBLCLICK = 0x00A3,
        WM_SETTINGSCHANGE = 0x001A,
        WM_SYSCOMMAND = 0x0112
    }

    [Flags]
    public enum EnableMenuItemFlag : uint
    {
        MF_ENABLED = 0x00000000,
        MF_GRAYED = 0x00000001
    }

    [Flags]
    public enum TrackPopupMenuFlag : uint
    {
        // The function returns the menu item identifier of the user's selection in the return value.
        TPM_RETURNCMD = 0x0100,
        // User can select menu items with both the left and right mouse buttons.
        TPM_RIGHTBUTTON = 0x0002
    }

    [Flags]
    public enum WindowAreaEnum : uint
    {
        HTCAPTION = 2,
        HTTOP = 12
    }

    // WM_SYSCOMMAND wParams and EnableMenuItem item IDs to some extent
    [Flags]
    public enum SystemCommandEnum : uint
    {
        SC_MOVE = 0xF010,
        SC_KEYMENU = 0xF100
    }

    [LibraryImport("user32.dll")]
    public static partial IntPtr EnableMenuItem(IntPtr hMenu, SystemCommandEnum uIDEnableItem, EnableMenuItemFlag uEnable);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

    [LibraryImport("user32.dll")]
    public static partial IntPtr PostMessageW(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

    [LibraryImport("user32.dll")]
    public static partial int TrackPopupMenu(IntPtr hMenu, TrackPopupMenuFlag uFlags,
        int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    /// <summary>
    /// NCCALCSIZE_PARAMS
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NcCalcSizeParams
    {
        public NcCalcSizeRectangles Rectangles;
        public WindowPosition Position;
    }

    /// <summary>
    /// rgrc
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NcCalcSizeRectangles
    {
        public Rect R1, R2, R3;
    }

    /// <summary>
    /// RECT
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    /// <summary>
    /// WINDOWPOS
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPosition
    {
        public IntPtr Hwnd;
        public IntPtr HwndZOrderInsertAfter;
        public int X, Y, Width, Height;
        public uint Flags;
    }
}
