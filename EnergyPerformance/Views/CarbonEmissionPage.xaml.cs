using System.Diagnostics;
using EnergyPerformance.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace EnergyPerformance.Views;

/// <summary>
/// Code behind for the home page.
/// </summary>
public sealed partial class CarbonEmissionPage : Page
{
    public CarbonEmissionViewModel ViewModel
    {
        get;
    }

    /// <summary>
    /// Sets the ViewModel property to the <see cref="MainViewModel"/> instance from the App class.
    /// Sets the default colour and brush for the buttons.
    /// Calls the SetSelectedButton function.
    /// </summary>
    public CarbonEmissionPage()
    {
        ViewModel = App.GetService<CarbonEmissionViewModel>();
        InitializeComponent();
    }

    private void SwitchToPowerUsage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MainPage));
    }

}
