using System.Runtime.InteropServices;

namespace BlazorWinFormsCustomTitleBar;

public partial class WindowsUXAPI
{
    public enum DwmWindowAttribute : uint
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19,
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20
    }

    public enum PreferredAppMode
    {
        Default = 0,
        AllowDark = 1,
        ForceDark = 2,
        ForceLight = 3,
        Max = 4
    };

    [LibraryImport("uxtheme.dll", EntryPoint = "#104", SetLastError = true)]
    public static partial void RefreshImmersiveColorPolicyState();

    [LibraryImport("uxtheme.dll", EntryPoint = "#132", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShouldAppsUseDarkMode();

    [LibraryImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true)]
    public static partial int SetPreferredAppMode(PreferredAppMode mode);

    [LibraryImport("uxtheme.dll", EntryPoint = "#136", SetLastError = true)]
    public static partial void FlushMenuThemes();

    [LibraryImport("DwmApi")]
    public static partial int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, int[] attrValue, int attrSize);
}
