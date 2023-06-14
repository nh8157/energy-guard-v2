using System.Diagnostics;
using System.Runtime.InteropServices;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace EnergyPerformance;

// <summary>
// Code behind for the main window of the application.
// Contains code for performing required operations at app startup and shutdown, including setting the Title bar
// name and icon, and saving the model back to disk.
public sealed partial class MainWindow : WindowEx
{
    private readonly AppWindow appWindow;

    public MainWindow()
    {
        InitializeComponent();

        appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this)));
        appWindow.Closing += AppWindow_Closing;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }

    // <summary>
    // Performs operations required at app shutdown, including saving the application.
    // </summary>
    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        Debug.WriteLine("MainWindow.appWindow.Closing");
        if (App.MainWindow.Visible && App.HandleClosedEvents)
        {
            Debug.WriteLine("Hiding window.");
            args.Cancel = true;
            this.Hide();
        }
        await App.GetService<EnergyUsageModel>().Save();
        await Task.CompletedTask;
    }

}
