using System.Diagnostics;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.Views;

/// <summary>
/// Code behind for Settings page.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    private readonly string _currencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPage"/> class.
    /// Also retrieves the correct currency code for the user's region so that the currency symbol can be displayed.
    /// </summary>
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        
        InitializeComponent();
        _currencyCode = "CurrencyCode".GetLocalized();
        SetNumberBoxNumberFormatter(WeeklyBudgetNumberBox);
        SetNumberBoxNumberFormatter(UnitCostNumberBox);
    }

    /// <summary>
    /// Sets the number formatter for the number box.
    /// Ensures that the number is formatted as currency with the correct currency symbol for the user's region.
    /// </summary>
    private void SetNumberBoxNumberFormatter(NumberBox numberBox)
    {

        var rounder = new IncrementNumberRounder
        {
            Increment = 0.01,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };


        var formatter = new CurrencyFormatter(_currencyCode)
        {
            IntegerDigits = 1,
            FractionDigits = 2,
            NumberRounder = rounder
        };

        numberBox.NumberFormatter = formatter;
    }

    /// <summary>
    /// Gets the correct theme based on the selected value of the combo box.
    /// Requests the SettingsViewModel to switch to switch themes.
    /// </summary>
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBoxItem = (ComboBoxItem)e.AddedItems.First();
        ElementTheme theme;
        if (comboBoxItem.Tag.Equals("Light"))
        {
            theme = ElementTheme.Light;
        }
        else if (comboBoxItem.Tag.Equals("Dark"))
        {
            theme = ElementTheme.Dark;
        }
        else
        {
            theme = ElementTheme.Default;
        }
        ViewModel.SwitchThemeCommand.Execute(theme);
    }

    /// <summary>
    /// Requests the SettingsViewModel to toggle on/off the launch on startup setting.
    /// </summary>
    /// <remarks>
    /// If the setting could not be turned on (due to user device settings), the toggle switch is set back to off.
    /// In the ViewModel, the updated setting (if changed) is saved in the local settings file.
    /// </remarks>
    private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        var toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch != null)
        {
            await ViewModel.LaunchOnStartupActions(toggleSwitch.IsOn);
            if (toggleSwitch.IsOn && !ViewModel.EnabledLaunchOnStartup)
            {
                toggleSwitch.IsOn = false;
            }
        }
    }

    /// <summary>
    /// Requests the SettingsViewModel to toggle on/off the auto control setting.
    /// </summary>
    /// <remarks>
    /// In the ViewModel, the updated setting is saved in the local settings file.
    /// </remarks>
    private void AutoControlToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        var toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch == null)
        {
            return;
        }
        if (ViewModel.SupportedCpu)
        {
            ViewModel.SwitchAutoControlSettingCommand.Execute(toggleSwitch.IsOn);
        }
        else
        {
            if (toggleSwitch.IsOn)
            {
                toggleSwitch.IsOn = false;
            }
        }
    }


}
