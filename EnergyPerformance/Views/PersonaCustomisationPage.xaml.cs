using System.Data;
using CommunityToolkit.WinUI.UI;
using EnergyPerformance.Models;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Media.AppRecording;

namespace EnergyPerformance.Views;

public sealed partial class PersonaCustomisationPage : Page
{
    const float DEFAULT = 2.0f;

    public PersonaCustomisationViewModel ViewModel
    {
        get;
    }
    
    public PersonaCustomisationPage()
    {
        ViewModel = App.GetService<PersonaCustomisationViewModel>();
        InitializeComponent();
        //PersonaSlider.LayoutUpdated += PersonaSlider_LayoutUpdated;
    }

    // Function to Activate Popup
    private void ShowPopup(object sender, RoutedEventArgs e)
    {
        if (!PersonaPopup.IsOpen) { PersonaPopup.IsOpen = true; }
    }

    // Function to Hide Popup
    // Deselects any selections from the persona list
    private void HidePopup(object sender, RoutedEventArgs e)
    {
        PersonaList.DeselectAll();
        if (PersonaPopup.IsOpen) { PersonaPopup.IsOpen = false; }
    }

    // Function called when the selection from persona list has changed
    // Calls ShowPopup and alters the values depending on the selection from the list
    // Safety check for whether the selection is a valid selection (This function is also called when the list is deselected)
    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowPopup(sender, e);
        var selectedIndex = PersonaList.SelectedIndex;

        AppSelection.SelectedIndex = selectedIndex;
        if (selectedIndex != -1)
        {
            PersonaSlider.Value = ViewModel.ApplicationList[selectedIndex].EnergyValue;
        }        
    }

    // Function to set the persona slider value to the default setting (2, in this case)
    private void RestoreDefault(object sender, RoutedEventArgs e)
    {
        PersonaSlider.Value = DEFAULT;
    }

    // Function to Apply Persona
    // Grabs the selected index, and updates the energy value and energy rating to the new values
    // Safety check for whether the selection is a valid selection
    private void ApplyPersona(object sender, RoutedEventArgs e)
    {
        var selectedIndex = AppSelection.SelectedIndex;
        if (selectedIndex != -1)
        {
            var item = ViewModel.ApplicationList[selectedIndex];
            item.EnergyValue = (float)PersonaSlider.Value;
            item.EnergyRating = item.UpdateEnergyRating((int)PersonaSlider.Value);
        }
    }

    //private void PersonaSlider_LayoutUpdated(object? sender, object e)
    //{
    //    if (VisualTreeHelper.GetOpenPopupsForXamlRoot(PersonaSlider.XamlRoot).LastOrDefault() is Popup popup)
    //    {
    //        popup.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    //    }
    //}
}