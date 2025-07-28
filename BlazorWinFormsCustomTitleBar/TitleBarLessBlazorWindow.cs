using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;
using static BlazorWinFormsCustomTitleBar.WindowsAPI;
using static BlazorWinFormsCustomTitleBar.WindowsUXAPI;

namespace BlazorWinFormsCustomTitleBar;
public partial class TitleBarLessBlazorWindow : Form
{
    public BlazorWebView BlazorWebView => blazorWebView;

    private bool _isActive = true;
    public bool IsActive
    {
        get => _isActive;
        private set
        {
            if (_isActive == value)
                return;
            _isActive = value;
            ActiveChanged?.Invoke(this, _isActive);
        }
    }
    public event EventHandler<bool>? ActiveChanged;

    public enum AppThemeEnum
    {
        Light,
        Dark
    }

    private AppThemeEnum _systemPreferredTheme;
    public AppThemeEnum SystemPreferredTheme
    {
        get => _systemPreferredTheme;
        private set
        {
            if (_systemPreferredTheme == value)
                return;
            _systemPreferredTheme = value;
            SystemPreferredThemeChanged?.Invoke(this, _systemPreferredTheme);
        }
    }
    public event EventHandler<AppThemeEnum>? SystemPreferredThemeChanged;

    public Action<ServiceCollection>? AddBlazorServices { get; set; }
    public Action<CoreWebView2Settings>? InitializeWebViewSettings { get; set; }
    public Action<CoreWebView2ContextMenuRequestedEventArgs> WebViewContextMenuRequested { get; set; }
        = DefaultWebViewContextMenuRequested;

    private FormWindowState _lastWindowState = FormWindowState.Normal;
    private int? _topBarHeight;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hostPage">The path of your page, usually "wwwroot\index.html"</param>
    /// <param name="initializeComponents">Declaration of Blazor components to use, e.g. <c>rootComponents.Add&lt;Body&gt;("body")</c></param>
    public TitleBarLessBlazorWindow(
        string hostPage,
        Action<RootComponentsCollection> initializeComponents)
    {
        InitializeComponent();
        Resize += OnResize;
        Activated += (o, s) => { IsActive = true; };
        Deactivate += (o, s) => { IsActive = false; };

        SetPreferredAppMode(PreferredAppMode.AllowDark);
        SynchronizeSystemThemePreference();

        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        services.AddSingleton(s => this);
        AddBlazorServices?.Invoke(services);

        blazorWebView.Services = services.BuildServiceProvider();
        blazorWebView.HostPage = hostPage;
        initializeComponents(blazorWebView.RootComponents);
        blazorWebView.BlazorWebViewInitialized += SetWebViewSettings;
    }

    public bool IsMaximized()
    => WindowState == FormWindowState.Maximized;
    public void ToggleMaximization()
        => WindowState = IsMaximized() ? FormWindowState.Normal : FormWindowState.Maximized;
    public void Minimize()
        => WindowState = FormWindowState.Minimized;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == (int)WindowMessage.WM_NCCALCSIZE && m.WParam == 1)
        {
            if (_topBarHeight is null && WindowState == FormWindowState.Normal)
            {
                // This operation pushes the title bar out of the visible area.
                //
                // A window in the normal state includes not only the top bar,
                // but also an additional margin.
                // I couldn't find a way to predict whether this command
                // is called before maximizing or restoring a window.
                // That's why this operation must happen when the window
                // is in its normal state.
                //
                // It's better to push out more in the maximized state
                // than to show a part of the original title bar
                // in the normal state.
                //
                // I'm afraid it's not a very reliable solution.

                var clientRectangle = RectangleToScreen(ClientRectangle);
                _topBarHeight = clientRectangle.Y - Bounds.Y;
            }

            var mmi = Marshal.PtrToStructure<NcCalcSizeParams>(m.LParam);
            mmi.Rectangles.R1.Top -= _topBarHeight ?? 0;
            Marshal.StructureToPtr(mmi, m.LParam, true);
        }
        else if (m.Msg == (int)WindowMessage.WM_SETTINGSCHANGE && Marshal.PtrToStringAuto(m.LParam) == "ImmersiveColorSet")
            SynchronizeSystemThemePreference();

        base.WndProc(ref m);
    }

    private void OnResize(object? sender, EventArgs e)
    {
        if (WindowState == _lastWindowState)
            return;

        var systemMenu = GetSystemMenu();
        if (IsMaximized())
        {
            // _topBarHeight, which is used to push out the top bar,
            // contains an additional margin that isn't used
            // in the maximized state.
            // We need to discover this margin and make a correction.
            //
            // I'm afraid it's not a very reliable solution.

            var clientArea = RectangleToScreen(ClientRectangle);
            var screenWorkingArea = Screen.GetWorkingArea(this);
            var marginCorrection = screenWorkingArea.Top - clientArea.Top;
            Padding = new Padding(0, marginCorrection, 0, 0);
            EnableMenuItem(systemMenu, SystemCommandEnum.SC_MOVE, EnableMenuItemFlag.MF_GRAYED);
        }
        else
        {
            Padding = new Padding(0, 0, 0, 0);
            EnableMenuItem(systemMenu, SystemCommandEnum.SC_MOVE, EnableMenuItemFlag.MF_ENABLED);
        }

        _lastWindowState = WindowState;
    }

    private void SynchronizeSystemThemePreference()
    {
        RefreshImmersiveColorPolicyState();
        var preferredTheme = ShouldAppsUseDarkMode() ? AppThemeEnum.Dark : AppThemeEnum.Light;
        if (preferredTheme == SystemPreferredTheme)
            return;

        SetWindowTheme(preferredTheme);

        SystemPreferredTheme = preferredTheme;
    }

    private void SetWindowTheme(AppThemeEnum theme)
    {
        int[] attributeValue = theme == AppThemeEnum.Dark ? [0x01] : [0x00];
        if (DwmSetWindowAttribute(Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, attributeValue, sizeof(int)) != 0)
            DwmSetWindowAttribute(Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, attributeValue, sizeof(int));
    }

    private IntPtr GetSystemMenu()
        => WindowsAPI.GetSystemMenu(Handle, false);

    public void ShowSystemMenu(Point location)
    {
        var selectedItem = TrackPopupMenu(
            GetSystemMenu(),
            TrackPopupMenuFlag.TPM_RETURNCMD | TrackPopupMenuFlag.TPM_RIGHTBUTTON,
            location.X, location.Y,
            0, Handle, IntPtr.Zero);

        if (selectedItem != 0)
            PostMessageW(Handle, WindowMessage.WM_SYSCOMMAND, selectedItem, IntPtr.Zero);
    }

    private void SendMouseMessage(WindowMessage message, WindowAreaEnum area)
    {
        ReleaseCapture();
        PostMessageW(
            Handle,
            message,
            (int)area,
            0);
    }

    public void TitleBarLeftButtonDown(bool doubleClick)
        => SendMouseMessage(
            doubleClick ? WindowMessage.WM_NCLBUTTONDBLCLICK : WindowMessage.WM_NCLBUTTONDOWN,
            WindowAreaEnum.HTCAPTION);

    public void TitleBarRightButtonUp()
        => ShowSystemMenu(MousePosition);

    public void TopBorderLeftButtonDown(bool doubleClick)
        => SendMouseMessage(
            doubleClick ? WindowMessage.WM_NCLBUTTONDBLCLICK : WindowMessage.WM_NCLBUTTONDOWN,
            WindowAreaEnum.HTTOP);

    private void SetWebViewSettings(object? sender, EventArgs e)
    {
        var core = blazorWebView.WebView.CoreWebView2;
        var settings = core.Settings;

        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.IsGeneralAutofillEnabled = false;
        settings.IsPinchZoomEnabled = false;
        settings.IsZoomControlEnabled = false;

        settings.AreDefaultContextMenusEnabled = true;
        InitializeWebViewSettings?.Invoke(settings);
        core.ContextMenuRequested += (o, e) => WebViewContextMenuRequested(e);
    }

    private static readonly string[] _allowedWebViewContextMenuItems = [
            "undo",
            "redo",
            "copy",
            "paste",
            "pasteAndMatchStyle",
            "selectAll"
        ];

    private static void DefaultWebViewContextMenuRequested(CoreWebView2ContextMenuRequestedEventArgs e)
    {
        for (int i = e.MenuItems.Count - 1; i >= 0; i--)
        {
            if (_allowedWebViewContextMenuItems.Contains(e.MenuItems[i].Name) is false)
                e.MenuItems.RemoveAt(i);
        }
    }
}
