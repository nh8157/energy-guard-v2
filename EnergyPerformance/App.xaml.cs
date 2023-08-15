using System.Diagnostics;
using EnergyPerformance.Activation;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Notifications;
using EnergyPerformance.Services;
using EnergyPerformance.ViewModels;
using EnergyPerformance.Views;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.Notifications;

namespace EnergyPerformance;

/// <summary>
/// The core Application class, the app is built and launched from here.
/// Registering of services to the host (i.e. Dependency Injection) is done here, as well as creation of the main window
/// and launching of the ActivationService and Hosted services.
/// </summary>
public partial class App : Application
{
    private static AppInstance? _instance;

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    /// <summary>
    /// Method to retrieve a service registered with the Host.
    /// </summary>
    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static bool HandleClosedEvents { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// Registration of classes with the Generic Host is performed here.
    /// Start point of the application
    /// </summary>
    public App()
    {
        InitializeComponent();
        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Debug Model first
            services.AddSingleton<DebugModel>();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            //services.AddSingleton<IHostedService, TaskService>();

            // Why adding interface here?
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Initializing HttpClientFactory
            services.AddHttpClient();

            // --- Registering background services and their dependencies
            services.AddHostedService<PeriodicDataSaverService>();

            services.AddSingleton<CpuInfo>(); // container for live CPU usage data
            services.AddHostedService<CpuTrackerService>();

            services.AddSingleton<PowerInfo>(); // container for live Power usage data
            services.AddHostedService<PowerMonitorService>();

            services.AddSingleton<LocationInfo>();
            services.AddHostedService<LocationService>();

            services.AddSingleton<CarbonIntensityInfo>();
            services.AddHostedService<CarbonIntensityUpdateService>();

            services.AddSingleton<EnergyRateInfo>();
            services.AddHostedService<EnergyRateService>();

            services.AddSingleton<IDatabaseService, DatabaseService>();
            // ---

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<EnergyUsageFileService>();
            services.AddSingleton<PersonaFileService>();

            // Models
            services.AddSingleton<EnergyUsageModel>();
            services.AddSingleton<PersonaModel>();

            // Views and ViewModels
            services.AddTransient<CarbonEmissionPage>();
            services.AddTransient<CarbonEmissionViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<DebugViewModel>();
            services.AddTransient<DebugPage>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<EnergyUsageViewModel>();
            services.AddTransient<EnergyUsagePage>();
            services.AddTransient<PersonaViewModel>();
            services.AddTransient<PersonaListPage>();
            services.AddTransient<CustomisePersonaPage>();
            services.AddTransient<AddPersonaPage>();
            services.AddTransient<SystemMonitorViewModel>();
            services.AddTransient<SystemMonitorPage>();
            services.AddTransient<MonitorDetailViewModel>();
            services.AddTransient<MonitorDetailPage>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<HistoryPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();
     
        Debug.WriteLine("Starting application");
        
        MainWindow.Closed += async (sender, args) =>
        {
            Debug.WriteLine("MainWindow.Closed");
            await Host.StopAsync();
        };
        UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        App.Current.UnhandledException += App_UnhandledException;
    }

    // not needed for application, simply here to show sequence of shutdown events
    private async void CurrentDomain_ProcessExit(object? sender, EventArgs e) {
        Debug.WriteLine("AppDomain.CurrentDomain.ProcessExit");

        // Persona clean up
        App.GetService<PersonaModel>().DisableEnabledPersona();

        // Notification clean up
        ToastNotificationManager.History.Clear();

        await Task.CompletedTask;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        //  Windows Runtime typically terminates an app after the unhandled exception event is fired.
        //  Whilst it is possible to do exception handling here and prevent termination, it is normally not recommended
        //  as continuing after an exception may not be safe, amongst several other reasons.
        //  This is all based on official guidance from Microsoft in:  
        //  https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

        //  Since we only need to save user data for this app, and this is an edge case, the exception is simply logged here,
        //  as the PeriodicDataSaverService will have already saved data recently and any loss of information will be minimal.
        //  Attempting to save the data here may not be safe, as the exception may have been caused by a data corruption.
        Debug.WriteLine(e.Exception);
    }

    /// <summary>
    /// Method to initialize the Application, this is called by the OS when the app is launched.
    /// Starts the activation service and hosted services and ensures the app is single-instanced.
    /// </summary>
    /// <remarks>
    /// The app must be single-instanced to ensure that multiple versions of background services are not running at the same time.
    /// </remarks>
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _instance = AppInstance.FindOrRegisterForKey("main");
        // Make the app single-instanced
        if (!_instance.IsCurrent)
        {
            // Redirect the activation (and args) to the "main" instance, and exit.
            // Reference: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle
            var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            await _instance.RedirectActivationToAsync(activatedEventArgs);
            Process.GetCurrentProcess().Kill();
            return;
        }

        base.OnLaunched(args);

        // start the ActivationService which all perform required actions at startup and call InitializeAsync() methods
        // for any registered services/objects which require asynchronous initialization
        await App.GetService<IActivationService>().ActivateAsync(args);

        // call StartAsync() on IHostedServices registered to the Host
        await Host.StartAsync();

        App.GetService<IAppNotificationService>().Initialize();
    }

}
