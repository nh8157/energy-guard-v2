using System.Diagnostics;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Models;
using Microsoft.UI.Xaml.Input;

namespace EnergyPerformance.Views;

/// <summary>
/// Code behind for the <see cref="TrayIconView"/> view.
/// </summary>
public sealed partial class TrayIconView
{

    private readonly EnergyUsageModel Model;

    public TrayIconView()
    {
        Model = App.GetService<EnergyUsageModel>();
        InitializeComponent();
    }


    /// <summary>
    /// Shows/hides the window when the tray icon is double clicked or Show/Hide button in the context flyout is pressed.
    /// </summary>
    public async void ShowHideWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
    {
        var window = App.MainWindow;
        if (window == null)
        {
            return;
        }

        if (window.Visible)
        {
            window.Hide();
        }
        else
        {
            await App.GetService<IActivationService>().ActivateAsync(args);
            //window.Show();
        }
    }

    /// <summary>
    /// Exits the application when the Exit button in the context flyout is pressed.
    /// Saves the model to local app data storage before exiting.
    /// </summary>
    public async void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
    {
        
        App.HandleClosedEvents = false;
        await Model.Save();
        TrayIcon.Dispose();
        Debug.WriteLine("Closing application from TrayIcon");
        App.MainWindow?.Close();
    }
}
