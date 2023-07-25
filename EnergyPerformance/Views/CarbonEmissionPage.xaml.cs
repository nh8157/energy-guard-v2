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
    private readonly Color defaultColor;
    private readonly SolidColorBrush defaultBrush;
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
        defaultColor = Colors.Aqua;
        defaultBrush = new SolidColorBrush(defaultColor);
        SetSelectedButton();
    }


    /// <summary>
    /// Sets the foreground colour of the button with the selected mode to the default colour.
    /// Called when the page is loaded.
    /// </summary>
    private void SetSelectedButton()
    {
        var selectedButton = FindName(ViewModel.SelectedMode) as AppBarButton;
        if (selectedButton is not null)
        {
            selectedButton.Foreground = defaultBrush;
        }
    }

    /// <summary>
    /// Sets the foreground colour of the button pressed to the default colour.
    /// Resets the colors of all other buttons.
    /// </summary>
    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as AppBarButton;
        if (button is not null)
        {
            button.Foreground = defaultBrush;
            ResetColours(button);
        }
    }

    /// <summary>
    /// Resets the foreground colour of all buttons except the one pressed.
    /// </summary>
    private void ResetColours(AppBarButton button)
    {
        // ClearValue property of the button is used to reset the foreground colour as setting the button's foreground property
        // to null like in previous .NET frameworks does not work, and setting the foreground to either White or Black causes issues
        // with theming.
        if (!button.Equals(Auto))
        {
            Auto.ClearValue(AppBarButton.ForegroundProperty);

        } if (!button.Equals(Casual))
        {
            Casual.ClearValue(AppBarButton.ForegroundProperty);

        } if (!button.Equals(Work))
        {
            Work.ClearValue(AppBarButton.ForegroundProperty);
        } if (!button.Equals(Performance))
        {
            Performance.ClearValue(AppBarButton.ForegroundProperty);
        }

    }

}
