using System.Diagnostics;
using EnergyPerformance.Activation;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Models;
using EnergyPerformance.ViewModels;
using EnergyPerformance.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EnergyPerformance.Services;


/// <summary>
/// Activation Service. Performs required actions on application startup, including calling InitializeAsync methods for classes
/// requiring asynchronous initialization.
/// </summary>
public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly EnergyUsageModel _energyUsageModel;
    private readonly PersonaModel _personaModel;
    private readonly SettingsViewModel _settingsViewModel;
    private bool _initialized;
    private UIElement? _shell = null;

    public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, IThemeSelectorService themeSelectorService, EnergyUsageModel energyUsageModel, PersonaModel personaModel, SettingsViewModel settingsViewModel)
    {
        _initialized = false;
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _themeSelectorService = themeSelectorService;
        _energyUsageModel = energyUsageModel;
        _personaModel = personaModel;
        _settingsViewModel = settingsViewModel;
    }

    /// <summary>
    /// Executes required tasks after activation.
    /// </summary>
    /// <returns></returns>
    public async Task StartupAsync()
    {
        await _themeSelectorService.SetRequestedThemeAsync();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Executes tasks required during start-up, or when the window is re-activated after being minimized.
    /// </summary>
    /// <param name="activationArgs">Activation Arguments</param>
    /// <returns></returns>
    public async Task ActivateAsync(object activationArgs)
    {
        // Execute tasks before activation.
        await InitializeAsync();

        // Set the MainWindow Content.
        if (App.MainWindow.Content == null)
        {
            _shell = App.GetService<ShellPage>();
            App.MainWindow.Content = _shell ?? new Frame();
        }

        // Handle activation via ActivationHandlers.
        await HandleActivationAsync(activationArgs);

        // Activate the MainWindow.
        App.MainWindow.Activate();

        // Execute tasks after activation.
        await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }

    /// <summary>
    /// Calls InitializeAsync methods for classes requiring asynchronous initialization.
    /// The model is only initialized once at startup.
    /// </summary>
    private async Task InitializeAsync()
    {
        // model should be initialized only once at startup
        if (!_initialized)
        {
            Debug.WriteLine("Initializing model.");
            await _energyUsageModel.InitializeAsync();
            await _personaModel.InitializeAsync();
            _initialized = true;
        }
        Debug.WriteLine("Initializing services.");
        await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
        await _settingsViewModel.InitializeAsync();
        await Task.CompletedTask;
    }

}
