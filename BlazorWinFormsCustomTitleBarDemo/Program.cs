using BlazorWinFormsCustomTitleBar;
using BlazorWinFormsCustomTitleBarDemo.Layout;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorWinFormsCustomTitleBarDemo;
internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        var mainWindow = new TitleBarLessBlazorWindow(
                @"wwwroot\index.html",
                rootComponents => rootComponents.Add<Body>("body")
            );
#if DEBUG
        mainWindow.AddBlazorServices = services => services.AddBlazorWebViewDeveloperTools();
#endif
        Application.Run(mainWindow);
    }
}