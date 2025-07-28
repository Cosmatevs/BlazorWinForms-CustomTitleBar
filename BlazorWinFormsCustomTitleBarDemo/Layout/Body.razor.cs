using BlazorWinFormsCustomTitleBar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace BlazorWinFormsCustomTitleBarDemo.Layout;

public partial class Body
{
    [Inject] public required TitleBarLessBlazorWindow MainWindow { get; set; }
    [Inject] public required IJSRuntime JS { get; set; }

    private const string JS_FUNCTIONS_HOLDER_NAME = "blazorWinFormsCustomTitleBar";

    private DotNetObjectReference<Body> _dotNetObjectReference = null!;
    private bool _canUseKeyboardShortcuts = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _dotNetObjectReference = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync($"{JS_FUNCTIONS_HOLDER_NAME}.initializeKeyboardShortcuts", _dotNetObjectReference);

            MainWindow.SystemPreferredThemeChanged += SynchronizeWebViewBackgroundColor;
            await SynchronizeWebViewBackgroundColor();
        }
    }

    public void Dispose()
    {
        _dotNetObjectReference?.Dispose();
        MainWindow.SystemPreferredThemeChanged -= SynchronizeWebViewBackgroundColor;
    }

    [JSInvokable]
    public bool HandleKeyboardShortcut(KeyboardEventArgs e)
    {
        if (!_canUseKeyboardShortcuts)
            return false;

#if DEBUG
        if (e.Key == "F12")
        {
            MainWindow.BlazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
            return true;
        }
#endif

        return false;
    }

    [GeneratedRegex(@"rgba?\((?<red>[0-9]{1,3}),\s*(?<green>[0-9]{1,3}),\s*(?<blue>[0-9]{1,3})\s*(?:,\s*(?:[01]|0?\.\d+))?\)")]
    private static partial Regex GetJSColorRegex();

    private async Task SynchronizeWebViewBackgroundColor()
    {
        var backgroundColorString = await JS.InvokeAsync<string>($"{JS_FUNCTIONS_HOLDER_NAME}.getBodyBackgroundColor");

        var jsColorMatch = GetJSColorRegex().Match(backgroundColorString);
        if (jsColorMatch.Success is false)
            return;

        var red = int.Parse(jsColorMatch.Groups["red"].Value);
        var green = int.Parse(jsColorMatch.Groups["green"].Value);
        var blue = int.Parse(jsColorMatch.Groups["blue"].Value);
        MainWindow.BlazorWebView.BackColor = Color.FromArgb(255, red, green, blue);
        // BlazorWebView.BackColor is visible while resizing the window
    }

    private async void SynchronizeWebViewBackgroundColor(object? sender, TitleBarLessBlazorWindow.AppThemeEnum theme)
        => await SynchronizeWebViewBackgroundColor();
}