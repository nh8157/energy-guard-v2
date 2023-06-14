using EnergyPerformance.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.Views;

/// <summary>
/// Code behind for the EnergyUsagePage page.
/// </summary>
public sealed partial class EnergyUsagePage : Page
{
    public EnergyUsageViewModel ViewModel
    {
        get;
    }

    public EnergyUsagePage()
    {
        ViewModel = App.GetService<EnergyUsageViewModel>();
        InitializeComponent();
        Loaded += OnLoad;
    }

    /// <summary>
    /// Invoked when this page is about to be displayed in a Frame.
    /// </summary>
    private void OnLoad(object sender, RoutedEventArgs e)
    {
        ApplyTheme(ActualTheme);
    }

    /// <summary>
    /// Applies the theme set in user settings to the OxyPlot graph
    /// </summary>
    private void ApplyTheme(ElementTheme theme)
    {
        ViewModel.Model.ApplyTheme(theme);
    }

}